namespace EventTrackingService.Domain.Entities;

/// <summary>
/// Specialized entity for page view events
/// </summary>
public class PageViewEvent : TrackedEvent
{
    public string PageUrl { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string? PreviousUrl { get; set; }
    public int? ScrollDepth { get; set; } // Percentage scrolled (0-100)
    public int? TimeOnPage { get; set; } // Seconds spent on page
    public bool IsExit { get; set; } // Was this the last page of the session
    public bool IsBounce { get; set; } // Did user leave within 10 seconds

    public PageViewEvent()
    {
        EventType = "page_view";
    }

    /// <summary>
    /// Marks this as a bounce (left within 10 seconds)
    /// </summary>
    public void MarkAsBounce()
    {
        IsBounce = true;
        IsExit = true;
    }

    /// <summary>
    /// Marks this as an exit (last page of session)
    /// </summary>
    public void MarkAsExit()
    {
        IsExit = true;
    }

    /// <summary>
    /// Sets time spent on page
    /// </summary>
    public void SetTimeOnPage(int seconds)
    {
        TimeOnPage = seconds;
        if (seconds < 10)
        {
            MarkAsBounce();
        }
    }

    /// <summary>
    /// Checks if user engaged with content (scrolled > 50%)
    /// </summary>
    public bool IsEngaged() => ScrollDepth.HasValue && ScrollDepth.Value > 50;
}
