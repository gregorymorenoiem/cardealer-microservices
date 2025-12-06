using FluentAssertions;
using Xunit;
using ReportsService.Domain.Entities;

namespace ReportsService.Tests;

public class DashboardTests
{
    private readonly Guid _dealerId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateDashboard()
    {
        // Arrange & Act
        var dashboard = new Dashboard(
            _dealerId,
            "Sales Overview",
            DashboardType.Sales,
            _userId,
            "Main sales dashboard");

        // Assert
        dashboard.Id.Should().NotBeEmpty();
        dashboard.DealerId.Should().Be(_dealerId);
        dashboard.Name.Should().Be("Sales Overview");
        dashboard.Type.Should().Be(DashboardType.Sales);
        dashboard.Description.Should().Be("Main sales dashboard");
        dashboard.IsDefault.Should().BeFalse();
        dashboard.IsPublic.Should().BeFalse();
        dashboard.CreatedBy.Should().Be(_userId);
        dashboard.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowException()
    {
        // Arrange & Act
        var act = () => new Dashboard(_dealerId, "", DashboardType.Sales, _userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name is required*");
    }

    [Fact]
    public void Update_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Old Name", DashboardType.Sales, _userId);

        // Act
        dashboard.Update("New Name", "New Description");

        // Assert
        dashboard.Name.Should().Be("New Name");
        dashboard.Description.Should().Be("New Description");
        dashboard.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetLayout_ShouldSetLayoutConfiguration()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Test Dashboard", DashboardType.Sales, _userId);
        var layout = "{\"columns\": 3}";

        // Act
        dashboard.SetLayout(layout);

        // Assert
        dashboard.Layout.Should().Be(layout);
    }

    [Fact]
    public void SetAsDefault_ShouldSetIsDefaultToTrue()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Test Dashboard", DashboardType.Sales, _userId);

        // Act
        dashboard.SetAsDefault();

        // Assert
        dashboard.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void UnsetAsDefault_ShouldSetIsDefaultToFalse()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Test Dashboard", DashboardType.Sales, _userId);
        dashboard.SetAsDefault();

        // Act
        dashboard.UnsetAsDefault();

        // Assert
        dashboard.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void SetPublic_ShouldUpdateIsPublic()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Test Dashboard", DashboardType.Sales, _userId);

        // Act
        dashboard.SetPublic(true);

        // Assert
        dashboard.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void AddWidget_ShouldAddWidgetToCollection()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Test Dashboard", DashboardType.Sales, _userId);
        var widget = new DashboardWidget(dashboard.Id, "Sales Chart", "chart");

        // Act
        dashboard.AddWidget(widget);

        // Assert
        dashboard.Widgets.Should().HaveCount(1);
        dashboard.Widgets.First().Title.Should().Be("Sales Chart");
    }

    [Fact]
    public void RemoveWidget_ShouldRemoveWidgetFromCollection()
    {
        // Arrange
        var dashboard = new Dashboard(_dealerId, "Test Dashboard", DashboardType.Sales, _userId);
        var widget = new DashboardWidget(dashboard.Id, "Sales Chart", "chart");
        dashboard.AddWidget(widget);

        // Act
        dashboard.RemoveWidget(widget.Id);

        // Assert
        dashboard.Widgets.Should().BeEmpty();
    }
}

public class DashboardWidgetTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateWidget()
    {
        // Arrange
        var dashboardId = Guid.NewGuid();

        // Act
        var widget = new DashboardWidget(dashboardId, "Sales Chart", "chart", 0, 0, 4, 2);

        // Assert
        widget.Id.Should().NotBeEmpty();
        widget.DashboardId.Should().Be(dashboardId);
        widget.Title.Should().Be("Sales Chart");
        widget.WidgetType.Should().Be("chart");
        widget.PositionX.Should().Be(0);
        widget.PositionY.Should().Be(0);
        widget.Width.Should().Be(4);
        widget.Height.Should().Be(2);
    }

    [Fact]
    public void SetDataSource_ShouldSetDataSource()
    {
        // Arrange
        var widget = new DashboardWidget(Guid.NewGuid(), "Test Widget", "chart");
        var dataSource = "{\"endpoint\": \"/api/sales\"}";

        // Act
        widget.SetDataSource(dataSource);

        // Assert
        widget.DataSource.Should().Be(dataSource);
    }

    [Fact]
    public void SetConfiguration_ShouldSetConfiguration()
    {
        // Arrange
        var widget = new DashboardWidget(Guid.NewGuid(), "Test Widget", "chart");
        var config = "{\"chartType\": \"bar\"}";

        // Act
        widget.SetConfiguration(config);

        // Assert
        widget.Configuration.Should().Be(config);
    }

    [Fact]
    public void SetPosition_ShouldUpdatePosition()
    {
        // Arrange
        var widget = new DashboardWidget(Guid.NewGuid(), "Test Widget", "chart");

        // Act
        widget.SetPosition(2, 3);

        // Assert
        widget.PositionX.Should().Be(2);
        widget.PositionY.Should().Be(3);
    }

    [Fact]
    public void SetSize_ShouldUpdateSize()
    {
        // Arrange
        var widget = new DashboardWidget(Guid.NewGuid(), "Test Widget", "chart");

        // Act
        widget.SetSize(6, 4);

        // Assert
        widget.Width.Should().Be(6);
        widget.Height.Should().Be(4);
    }
}

