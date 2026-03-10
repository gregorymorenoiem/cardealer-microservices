using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Text;
using VehiclesSaleService.Api.Controllers;
using VehiclesSaleService.Domain.Entities;
using VehiclesSaleService.Domain.Interfaces;
using VehiclesSaleService.Infrastructure.Messaging;
using VehiclesSaleService.Infrastructure.Persistence;
using CarDealer.Shared.MultiTenancy;

namespace VehiclesSaleService.Tests.Integration;

public class ImportControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IVehicleRepository> _vehicleRepo;
    private readonly Mock<IEventPublisher> _eventPublisher;
    private readonly Mock<ILogger<ImportController>> _logger;
    private readonly ImportController _controller;

    public ImportControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"ImportTests_{Guid.NewGuid()}")
            .Options;

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.CurrentDealerId).Returns(Guid.NewGuid());
        tenantContext.Setup(t => t.HasDealerContext).Returns(true);

        _context = new ApplicationDbContext(options, tenantContext.Object);
        _vehicleRepo = new Mock<IVehicleRepository>();
        _eventPublisher = new Mock<IEventPublisher>();
        _logger = new Mock<ILogger<ImportController>>();

        // Setup vehicle repo to return passed vehicle with an Id
        _vehicleRepo.Setup(r => r.CreateAsync(It.IsAny<Vehicle>()))
            .ReturnsAsync((Vehicle v) =>
            {
                v.Id = Guid.NewGuid();
                return v;
            });

        _controller = new ImportController(_context, _vehicleRepo.Object, _eventPublisher.Object, _logger.Object);

        // Setup authenticated user claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("dealerId", Guid.NewGuid().ToString()),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // ═══════════════════════════════════════════════
    // CSV Upload Tests
    // ═══════════════════════════════════════════════

    [Fact]
    public async Task Import_ValidCsv_ReturnsSuccessWithCreatedVehicles()
    {
        // Arrange
        var csv = "marca,modelo,año,precio,kilometraje,descripcion\n" +
                  "Toyota,Camry,2022,1250000,35000,Excelente estado\n" +
                  "Honda,Civic,2023,980000,15000,Como nuevo\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.SuccessCount.Should().Be(2);
        importResult.ErrorCount.Should().Be(0);
        importResult.TotalRows.Should().Be(2);
    }

    [Fact]
    public async Task Import_NullFile_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Import(null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Import_UnsupportedFormat_ReturnsBadRequest()
    {
        // Arrange
        var file = CreateFormFile("data", "import.pdf");

        // Act
        var result = await _controller.Import(file);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Import_EmptyCsv_ReturnsBadRequest()
    {
        // Arrange — header only, no data rows
        var csv = "marca,modelo,año,precio,kilometraje\n";
        var file = CreateFormFile(csv, "empty.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Import_RowWithMissingMake_ReportsError()
    {
        // Arrange — row missing marca
        var csv = "marca,modelo,año,precio,kilometraje\n" +
                  ",Camry,2022,1250000,35000\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.ErrorCount.Should().Be(1);
        importResult.Results[0].Errors.Should().Contain(e => e.Contains("Marca"));
    }

    [Fact]
    public async Task Import_RowWithNegativePrice_ReportsError()
    {
        // Arrange
        var csv = "marca,modelo,año,precio,kilometraje\n" +
                  "Toyota,Camry,2022,-100,35000\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.ErrorCount.Should().Be(1);
        importResult.Results[0].Errors.Should().Contain(e => e.Contains("Precio"));
    }

    [Fact]
    public async Task Import_RowWithInvalidYear_ReportsError()
    {
        // Arrange
        var csv = "marca,modelo,año,precio,kilometraje\n" +
                  "Toyota,Camry,1800,1250000,35000\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.ErrorCount.Should().Be(1);
        importResult.Results[0].Errors.Should().Contain(e => e.Contains("Año"));
    }

    [Fact]
    public async Task Import_MixedValidAndInvalidRows_ReportsPartialSuccess()
    {
        // Arrange — 1 valid, 1 invalid (missing model)
        var csv = "marca,modelo,año,precio,kilometraje\n" +
                  "Toyota,Camry,2022,1250000,35000\n" +
                  "Honda,,2023,980000,15000\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.SuccessCount.Should().Be(1);
        importResult.ErrorCount.Should().Be(1);
    }

    [Fact]
    public async Task Import_DuplicateVinInBatch_ReportsError()
    {
        // Arrange — same VIN in two rows
        var csv = "marca,modelo,año,precio,kilometraje,vin\n" +
                  "Toyota,Camry,2022,1250000,35000,1HGBH41JXMN109186\n" +
                  "Honda,Civic,2023,980000,15000,1HGBH41JXMN109186\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        // First row succeeds, second row should have VIN duplicate error
        importResult.SuccessCount.Should().Be(1);
        importResult.ErrorCount.Should().Be(1);
        importResult.Results[1].Errors.Should().Contain(e => e.Contains("VIN duplicado"));
    }

    [Fact]
    public async Task Import_BilingualHeaders_WorksCorrectly()
    {
        // Arrange — English column headers
        var csv = "make,model,year,price,mileage,description\n" +
                  "Toyota,Camry,2022,1250000,35000,Great condition\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.SuccessCount.Should().Be(1);
    }

    // ═══════════════════════════════════════════════
    // Template Tests
    // ═══════════════════════════════════════════════

    [Fact]
    public void DownloadTemplate_ReturnsValidCsvFile()
    {
        // Act
        var result = _controller.DownloadTemplate();

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Contain("text/csv");
        fileResult.FileDownloadName.Should().Contain("plantilla");
        var content = Encoding.UTF8.GetString(fileResult.FileContents);
        content.Should().Contain("marca");
        content.Should().Contain("modelo");
    }

    // ═══════════════════════════════════════════════
    // Error Report Tests
    // ═══════════════════════════════════════════════

    [Fact]
    public void DownloadErrorReport_WithErrors_ReturnsCsvFile()
    {
        // Arrange
        var request = new ErrorReportRequest
        {
            Errors = new List<ImportRowResult>
            {
                new() { Row = 1, Success = false, Errors = new List<string> { "Marca es requerida" } },
                new() { Row = 3, Success = false, Errors = new List<string> { "Precio inválido" } },
            }
        };

        // Act
        var result = _controller.DownloadErrorReport(request);

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Contain("text/csv");
        var content = Encoding.UTF8.GetString(fileResult.FileContents);
        content.Should().Contain("fila,error");
        content.Should().Contain("Marca es requerida");
    }

    [Fact]
    public void DownloadErrorReport_EmptyErrors_ReturnsBadRequest()
    {
        // Arrange
        var request = new ErrorReportRequest { Errors = new List<ImportRowResult>() };

        // Act
        var result = _controller.DownloadErrorReport(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ═══════════════════════════════════════════════
    // History Tests
    // ═══════════════════════════════════════════════

    [Fact]
    public async Task GetHistory_ReturnsPersistedRecords()
    {
        // Arrange — insert a history record for the authenticated user
        var userId = Guid.Parse(_controller.ControllerContext.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)!.Value);

        _context.ImportHistories.Add(new ImportHistory
        {
            SellerId = userId,
            Filename = "test.csv",
            TotalRows = 10,
            SuccessCount = 8,
            ErrorCount = 2,
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetHistory();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var history = okResult.Value.Should().BeAssignableTo<List<ImportHistoryDto>>().Subject;
        history.Should().HaveCount(1);
        history[0].Filename.Should().Be("test.csv");
        history[0].Successful.Should().Be(8);
        history[0].Failed.Should().Be(2);
    }

    // ═══════════════════════════════════════════════
    // Validation Tests
    // ═══════════════════════════════════════════════

    [Fact]
    public async Task Import_InvalidCondition_ReportsError()
    {
        // Arrange
        var csv = "marca,modelo,año,precio,kilometraje,condicion\n" +
                  "Toyota,Camry,2022,1250000,35000,invalido\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.ErrorCount.Should().Be(1);
        importResult.Results[0].Errors.Should().Contain(e => e.Contains("Condición inválida"));
    }

    [Fact]
    public async Task Import_NegativeMileage_ReportsError()
    {
        // Arrange
        var csv = "marca,modelo,año,precio,kilometraje\n" +
                  "Toyota,Camry,2022,1250000,-5000\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        var result = await _controller.Import(file);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var importResult = okResult.Value.Should().BeOfType<ImportResult>().Subject;
        importResult.ErrorCount.Should().Be(1);
        importResult.Results[0].Errors.Should().Contain(e => e.Contains("negativo"));
    }

    [Fact]
    public async Task Import_PersistsHistoryRecord()
    {
        // Arrange
        var csv = "marca,modelo,año,precio,kilometraje\n" +
                  "Toyota,Camry,2022,1250000,35000\n";
        var file = CreateFormFile(csv, "import.csv");

        // Act
        await _controller.Import(file);

        // Assert — history should be persisted
        var historyCount = await _context.ImportHistories.CountAsync();
        historyCount.Should().Be(1);

        var history = await _context.ImportHistories.FirstAsync();
        history.Filename.Should().Be("import.csv");
        history.SuccessCount.Should().Be(1);
        history.ErrorCount.Should().Be(0);
    }

    // ═══════════════════════════════════════════════
    // Helper
    // ═══════════════════════════════════════════════

    private static IFormFile CreateFormFile(string content, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = fileName.EndsWith(".csv")
                ? "text/csv"
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }
}
