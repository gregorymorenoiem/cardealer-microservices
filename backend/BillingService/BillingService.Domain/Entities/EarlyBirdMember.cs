namespace BillingService.Domain.Entities;

/// <summary>
/// Representa un usuario que se inscribió en el plan "Early Bird" - 3 meses gratis.
/// Esta inscripción otorga el badge permanente de "Miembro Fundador".
/// </summary>
public class EarlyBirdMember
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime FreeUntil { get; private set; }
    public bool HasUsedBenefit { get; private set; }
    public DateTime? BenefitUsedAt { get; private set; }
    public string? SubscriptionIdWhenUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // For EF Core
    private EarlyBirdMember() { }

    /// <summary>
    /// Constructor para inscribir un nuevo miembro Early Bird
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="freeMonths">Cantidad de meses gratis (default 3)</param>
    public EarlyBirdMember(Guid userId, int freeMonths = 3)
    {
        if (freeMonths <= 0)
            throw new ArgumentException("Free months must be greater than 0", nameof(freeMonths));

        Id = Guid.NewGuid();
        UserId = userId;
        EnrolledAt = DateTime.UtcNow;
        FreeUntil = DateTime.UtcNow.AddMonths(freeMonths);
        HasUsedBenefit = false;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si el usuario aún está en el período gratuito
    /// </summary>
    public bool IsInFreePeriod()
    {
        return DateTime.UtcNow <= FreeUntil && !HasUsedBenefit;
    }

    /// <summary>
    /// Marca el beneficio como usado al crear una suscripción
    /// </summary>
    public void MarkBenefitUsed(Guid subscriptionId)
    {
        if (HasUsedBenefit)
            throw new InvalidOperationException("Early bird benefit has already been used");

        HasUsedBenefit = true;
        BenefitUsedAt = DateTime.UtcNow;
        SubscriptionIdWhenUsed = subscriptionId.ToString();
    }

    /// <summary>
    /// Calcula días restantes del período gratuito
    /// </summary>
    public int GetRemainingFreeDays()
    {
        if (!IsInFreePeriod())
            return 0;

        var remaining = (FreeUntil - DateTime.UtcNow).Days;
        return remaining > 0 ? remaining : 0;
    }

    /// <summary>
    /// Verifica si el usuario tiene el badge de Miembro Fundador
    /// El badge es permanente una vez inscrito
    /// </summary>
    public bool HasFounderBadge()
    {
        return true; // El badge es permanente para todos los Early Bird members
    }
}
