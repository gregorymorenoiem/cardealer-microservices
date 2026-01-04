namespace AuthService.Domain.Enums;

/// <summary>
/// Represents the type of user account
/// </summary>
public enum AccountType
{
    /// <summary>Guest user (not registered)</summary>
    Guest = 0,

    /// <summary>Individual user</summary>
    Individual = 1,

    /// <summary>Dealer/Business owner</summary>
    Dealer = 2,

    /// <summary>Employee of a dealer</summary>
    DealerEmployee = 3,

    /// <summary>Platform administrator</summary>
    Admin = 4,

    /// <summary>Platform employee</summary>
    PlatformEmployee = 5
}
