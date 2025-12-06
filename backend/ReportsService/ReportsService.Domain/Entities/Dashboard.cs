using CarDealer.Shared.MultiTenancy;

namespace ReportsService.Domain.Entities;

public enum DashboardType
{
    Overview,
    Sales,
    Inventory,
    Financial,
    CRM,
    Marketing,
    Custom
}

public class Dashboard : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public DashboardType Type { get; private set; }
    public string? Layout { get; private set; } // JSON layout configuration

    public bool IsDefault { get; private set; }
    public bool IsPublic { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public ICollection<DashboardWidget> Widgets { get; private set; } = new List<DashboardWidget>();

    private Dashboard() { }

    public Dashboard(
        Guid dealerId,
        string name,
        DashboardType type,
        Guid createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Type = type;
        CreatedBy = createdBy;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLayout(string layout)
    {
        Layout = layout;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPublic(bool isPublic)
    {
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddWidget(DashboardWidget widget)
    {
        Widgets.Add(widget);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveWidget(Guid widgetId)
    {
        var widget = Widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget != null)
        {
            Widgets.Remove(widget);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

public class DashboardWidget
{
    public Guid Id { get; private set; }
    public Guid DashboardId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string WidgetType { get; private set; } = string.Empty; // chart, table, kpi, etc.
    public string? DataSource { get; private set; } // JSON
    public string? Configuration { get; private set; } // JSON

    public int PositionX { get; private set; }
    public int PositionY { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private DashboardWidget() { }

    public DashboardWidget(
        Guid dashboardId,
        string title,
        string widgetType,
        int positionX = 0,
        int positionY = 0,
        int width = 4,
        int height = 2)
    {
        Id = Guid.NewGuid();
        DashboardId = dashboardId;
        Title = title;
        WidgetType = widgetType;
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        Height = height;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetDataSource(string dataSource)
    {
        DataSource = dataSource;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetConfiguration(string configuration)
    {
        Configuration = configuration;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPosition(int x, int y)
    {
        PositionX = x;
        PositionY = y;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        UpdatedAt = DateTime.UtcNow;
    }
}
