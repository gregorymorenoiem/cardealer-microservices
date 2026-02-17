namespace FinanceService.Application.DTOs;

public record AccountDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Type,
    string Currency,
    decimal Balance,
    decimal InitialBalance,
    Guid? ParentAccountId,
    bool IsActive,
    bool IsSystem,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAccountRequest(
    string Code,
    string Name,
    string? Description,
    string Type,
    string Currency,
    decimal InitialBalance = 0,
    Guid? ParentAccountId = null
);

public record UpdateAccountRequest(
    string Name,
    string? Description
);
