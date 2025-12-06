using CarDealer.Shared.MultiTenancy;

namespace MarketingService.Domain.Entities;

public enum AudienceType
{
    Static,
    Dynamic,
    Imported
}

public class Audience : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public AudienceType Type { get; private set; }

    public string? FilterCriteria { get; private set; } // JSON filter for dynamic audiences
    public int MemberCount { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? LastSyncedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation
    public ICollection<AudienceMember> Members { get; private set; } = new List<AudienceMember>();

    private Audience() { }

    public Audience(
        Guid dealerId,
        string name,
        AudienceType type,
        Guid createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Id = Guid.NewGuid();
        DealerId = dealerId;
        Name = name;
        Type = type;
        CreatedBy = createdBy;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFilterCriteria(string? filterCriteria)
    {
        FilterCriteria = filterCriteria;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMemberCount(int count)
    {
        MemberCount = count;
        LastSyncedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMember(AudienceMember member)
    {
        Members.Add(member);
        MemberCount = Members.Count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveMember(Guid memberId)
    {
        var member = Members.FirstOrDefault(m => m.Id == memberId);
        if (member != null)
        {
            Members.Remove(member);
            MemberCount = Members.Count;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

public class AudienceMember : ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid DealerId { get; set; }
    public Guid AudienceId { get; private set; }

    public string Email { get; private set; } = string.Empty;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Phone { get; private set; }

    public Guid? CustomerId { get; private set; }
    public Guid? LeadId { get; private set; }

    public bool IsSubscribed { get; private set; }
    public DateTime? UnsubscribedAt { get; private set; }

    public DateTime AddedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private AudienceMember() { }

    public AudienceMember(
        Guid dealerId,
        Guid audienceId,
        string email,
        string? firstName = null,
        string? lastName = null,
        string? phone = null)
    {
        Id = Guid.NewGuid();
        DealerId = dealerId;
        AudienceId = audienceId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        IsSubscribed = true;
        AddedAt = DateTime.UtcNow;
    }

    public void Update(string? firstName, string? lastName, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToCustomer(Guid customerId)
    {
        CustomerId = customerId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LinkToLead(Guid leadId)
    {
        LeadId = leadId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unsubscribe()
    {
        IsSubscribed = false;
        UnsubscribedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Resubscribe()
    {
        IsSubscribed = true;
        UnsubscribedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
