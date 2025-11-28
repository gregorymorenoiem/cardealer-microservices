using ErrorService.Domain.Entities;
using ErrorService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ErrorService.Tests.Infrastructure.Persistence
{
    public class EfErrorLogRepositoryTests
    {
        [Fact]
        public async Task AddAsync_AddsErrorLogToDbContext()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
                .Options;

            using var context = new ApplicationDbContext(options);
            var repository = new EfErrorLogRepository(context);
            var errorLog = new ErrorLog
            {
                Id = Guid.NewGuid(),
                ServiceName = "TestService",
                ExceptionType = "TestException",
                Message = "Test message",
                StackTrace = "Test stack trace",
                OccurredAt = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(errorLog);

            // Assert
            var savedError = await context.ErrorLogs.FirstOrDefaultAsync();
            Assert.NotNull(savedError);
            Assert.Equal("TestService", savedError.ServiceName);
            Assert.Equal("TestException", savedError.ExceptionType);
            Assert.Equal("Test message", savedError.Message);
        }
    }
}
