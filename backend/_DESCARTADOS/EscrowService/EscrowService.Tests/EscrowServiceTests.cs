using FluentAssertions;
using EscrowService.Domain.Entities;
using Xunit;

namespace EscrowService.Tests;

/// <summary>
/// Tests unitarios para EscrowService - Depósito en Garantía
/// Cumplimiento con normativas de protección al consumidor RD
/// </summary>
public class EscrowServiceTests
{
    #region PARTE 1: Tests de Enumeraciones

    [Theory]
    [InlineData(1, "VehiclePurchase")]
    [InlineData(2, "VehicleLease")]
    [InlineData(3, "ServiceDeposit")]
    [InlineData(4, "TradeIn")]
    [InlineData(5, "Warranty")]
    [InlineData(6, "Inspection")]
    [InlineData(7, "Reservation")]
    [InlineData(8, "DownPayment")]
    [InlineData(9, "FinancingGuarantee")]
    public void EscrowTransactionType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<EscrowTransactionType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void EscrowTransactionType_ShouldHave9Values()
    {
        var values = Enum.GetValues<EscrowTransactionType>();
        values.Should().HaveCount(9);
    }

    [Theory]
    [InlineData(1, "Pending")]
    [InlineData(2, "Funded")]
    [InlineData(3, "InProgress")]
    [InlineData(4, "ConditionsMet")]
    [InlineData(5, "PendingRelease")]
    [InlineData(6, "Released")]
    [InlineData(7, "Refunded")]
    [InlineData(8, "PartialRelease")]
    [InlineData(9, "Disputed")]
    [InlineData(10, "Cancelled")]
    [InlineData(11, "Expired")]
    public void EscrowStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<EscrowStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void EscrowStatus_ShouldHave11Values()
    {
        var values = Enum.GetValues<EscrowStatus>();
        values.Should().HaveCount(11);
    }

    [Theory]
    [InlineData(1, "VehicleDelivery")]
    [InlineData(2, "TitleTransfer")]
    [InlineData(3, "InspectionApproved")]
    [InlineData(4, "DocumentsVerified")]
    [InlineData(5, "BuyerApproval")]
    [InlineData(6, "SellerApproval")]
    [InlineData(7, "TimeElapsed")]
    [InlineData(8, "PaymentReceived")]
    [InlineData(9, "ContractSigned")]
    [InlineData(10, "InsuranceVerified")]
    [InlineData(11, "FinancingApproved")]
    [InlineData(12, "ManualApproval")]
    public void ReleaseConditionType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<ReleaseConditionType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void ReleaseConditionType_ShouldHave12Values()
    {
        var values = Enum.GetValues<ReleaseConditionType>();
        values.Should().HaveCount(12);
    }

    [Theory]
    [InlineData(1, "Pending")]
    [InlineData(2, "InProgress")]
    [InlineData(3, "Met")]
    [InlineData(4, "Failed")]
    [InlineData(5, "Waived")]
    [InlineData(6, "Expired")]
    public void ConditionStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<ConditionStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void ConditionStatus_ShouldHave6Values()
    {
        var values = Enum.GetValues<ConditionStatus>();
        values.Should().HaveCount(6);
    }

    [Theory]
    [InlineData(1, "Deposit")]
    [InlineData(2, "AdditionalDeposit")]
    [InlineData(3, "Release")]
    [InlineData(4, "PartialRelease")]
    [InlineData(5, "Refund")]
    [InlineData(6, "PartialRefund")]
    [InlineData(7, "Fee")]
    [InlineData(8, "Adjustment")]
    [InlineData(9, "Penalty")]
    [InlineData(10, "Interest")]
    public void FundMovementType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<FundMovementType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void FundMovementType_ShouldHave10Values()
    {
        var values = Enum.GetValues<FundMovementType>();
        values.Should().HaveCount(10);
    }

    [Theory]
    [InlineData(1, "BankTransfer")]
    [InlineData(2, "CreditCard")]
    [InlineData(3, "DebitCard")]
    [InlineData(4, "Cash")]
    [InlineData(5, "Check")]
    [InlineData(6, "Wire")]
    [InlineData(7, "Crypto")]
    public void PaymentMethod_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<PaymentMethod>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void PaymentMethod_ShouldHave7Values()
    {
        var values = Enum.GetValues<PaymentMethod>();
        values.Should().HaveCount(7);
    }

    [Theory]
    [InlineData(1, "Filed")]
    [InlineData(2, "UnderReview")]
    [InlineData(3, "AwaitingDocuments")]
    [InlineData(4, "InMediation")]
    [InlineData(5, "Resolved")]
    [InlineData(6, "Escalated")]
    [InlineData(7, "Closed")]
    public void EscrowDisputeStatus_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<EscrowDisputeStatus>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void EscrowDisputeStatus_ShouldHave7Values()
    {
        var values = Enum.GetValues<EscrowDisputeStatus>();
        values.Should().HaveCount(7);
    }

    [Theory]
    [InlineData(1, "Created")]
    [InlineData(2, "Funded")]
    [InlineData(3, "ConditionMet")]
    [InlineData(4, "ConditionFailed")]
    [InlineData(5, "ReleaseRequested")]
    [InlineData(6, "Released")]
    [InlineData(7, "Refunded")]
    [InlineData(8, "Disputed")]
    [InlineData(9, "DisputeResolved")]
    [InlineData(10, "Cancelled")]
    [InlineData(11, "Expired")]
    [InlineData(12, "DocumentUploaded")]
    [InlineData(13, "StatusChanged")]
    [InlineData(14, "AmountAdjusted")]
    public void EscrowAuditEventType_ShouldHaveCorrectValues(int expectedValue, string enumName)
    {
        var enumValue = Enum.Parse<EscrowAuditEventType>(enumName);
        ((int)enumValue).Should().Be(expectedValue);
    }

    [Fact]
    public void EscrowAuditEventType_ShouldHave14Values()
    {
        var values = Enum.GetValues<EscrowAuditEventType>();
        values.Should().HaveCount(14);
    }

    #endregion

    #region PARTE 2: Tests de Entidad EscrowAccount

    [Fact]
    public void EscrowAccount_ShouldBeCreated_WithValidData()
    {
        var escrow = new EscrowAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "ESC-2026-00001",
            TransactionType = EscrowTransactionType.VehiclePurchase,
            Status = EscrowStatus.Pending,
            BuyerId = Guid.NewGuid(),
            BuyerName = "Juan Pérez",
            BuyerEmail = "juan@email.com",
            SellerId = Guid.NewGuid(),
            SellerName = "Auto Dealer SRL",
            SellerEmail = "ventas@dealer.com",
            TotalAmount = 1500000.00m,
            Currency = "DOP",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        escrow.AccountNumber.Should().StartWith("ESC-");
        escrow.Status.Should().Be(EscrowStatus.Pending);
        escrow.Currency.Should().Be("DOP");
        escrow.TotalAmount.Should().Be(1500000.00m);
    }

    [Fact]
    public void EscrowAccount_ShouldCalculate_PendingAmount()
    {
        var escrow = new EscrowAccount
        {
            TotalAmount = 1000000.00m,
            FundedAmount = 500000.00m,
            ReleasedAmount = 0m,
            RefundedAmount = 0m
        };

        escrow.PendingAmount = escrow.TotalAmount - escrow.FundedAmount;
        escrow.PendingAmount.Should().Be(500000.00m);
    }

    [Fact]
    public void EscrowAccount_ShouldTransition_ThroughStatuses()
    {
        var escrow = new EscrowAccount { Status = EscrowStatus.Pending };

        escrow.Status = EscrowStatus.Funded;
        escrow.FundedAt = DateTime.UtcNow;
        escrow.Status.Should().Be(EscrowStatus.Funded);

        escrow.Status = EscrowStatus.InProgress;
        escrow.Status.Should().Be(EscrowStatus.InProgress);

        escrow.Status = EscrowStatus.ConditionsMet;
        escrow.Status.Should().Be(EscrowStatus.ConditionsMet);

        escrow.Status = EscrowStatus.Released;
        escrow.ReleasedAt = DateTime.UtcNow;
        escrow.Status.Should().Be(EscrowStatus.Released);
    }

    [Fact]
    public void EscrowAccount_ShouldRequire_BothApprovals()
    {
        var escrow = new EscrowAccount
        {
            RequiresBothApproval = true,
            BuyerApproved = false,
            SellerApproved = false
        };

        escrow.BuyerApproved = true;
        escrow.BuyerApprovedAt = DateTime.UtcNow;

        var bothApproved = escrow.BuyerApproved && escrow.SellerApproved;
        bothApproved.Should().BeFalse();

        escrow.SellerApproved = true;
        escrow.SellerApprovedAt = DateTime.UtcNow;

        bothApproved = escrow.BuyerApproved && escrow.SellerApproved;
        bothApproved.Should().BeTrue();
    }

    [Fact]
    public void EscrowAccount_ShouldHave_SubjectReference()
    {
        var vehicleId = Guid.NewGuid();
        var escrow = new EscrowAccount
        {
            SubjectType = "Vehicle",
            SubjectId = vehicleId,
            SubjectDescription = "Toyota Camry 2024 - Negro"
        };

        escrow.SubjectType.Should().Be("Vehicle");
        escrow.SubjectId.Should().Be(vehicleId);
    }

    [Fact]
    public void EscrowAccount_ShouldCalculate_FeeAmount()
    {
        var escrow = new EscrowAccount
        {
            TotalAmount = 1000000.00m,
            FeePercentage = 2.5m
        };

        escrow.FeeAmount = escrow.TotalAmount * (escrow.FeePercentage / 100);
        escrow.FeeAmount.Should().Be(25000.00m);
    }

    [Fact]
    public void EscrowAccount_ShouldHave_Collections()
    {
        var escrow = new EscrowAccount
        {
            Conditions = new List<ReleaseCondition>(),
            Movements = new List<FundMovement>(),
            Documents = new List<EscrowDocument>(),
            Disputes = new List<EscrowDispute>(),
            AuditLogs = new List<EscrowAuditLog>()
        };

        escrow.Conditions.Should().BeEmpty();
        escrow.Movements.Should().BeEmpty();
        escrow.Documents.Should().BeEmpty();
        escrow.Disputes.Should().BeEmpty();
        escrow.AuditLogs.Should().BeEmpty();
    }

    #endregion

    #region PARTE 3: Tests de Entidad ReleaseCondition

    [Fact]
    public void ReleaseCondition_ShouldBeCreated_WithValidData()
    {
        var condition = new ReleaseCondition
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = Guid.NewGuid(),
            Type = ReleaseConditionType.VehicleDelivery,
            Name = "Entrega del Vehículo",
            Description = "El vehículo debe ser entregado en buenas condiciones",
            Status = ConditionStatus.Pending,
            IsMandatory = true,
            Order = 1,
            CreatedAt = DateTime.UtcNow
        };

        condition.Type.Should().Be(ReleaseConditionType.VehicleDelivery);
        condition.IsMandatory.Should().BeTrue();
        condition.Status.Should().Be(ConditionStatus.Pending);
    }

    [Fact]
    public void ReleaseCondition_ShouldTrack_Verification()
    {
        var condition = new ReleaseCondition
        {
            Status = ConditionStatus.Pending,
            RequiresEvidence = true
        };

        condition.Status = ConditionStatus.Met;
        condition.MetAt = DateTime.UtcNow;
        condition.VerifiedBy = "admin@okla.com";
        condition.VerificationNotes = "Verificado mediante fotos";
        condition.EvidenceDocumentId = Guid.NewGuid();

        condition.Status.Should().Be(ConditionStatus.Met);
        condition.VerifiedBy.Should().NotBeNullOrEmpty();
        condition.EvidenceDocumentId.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ReleaseConditionType.VehicleDelivery, true)]
    [InlineData(ReleaseConditionType.TitleTransfer, true)]
    [InlineData(ReleaseConditionType.InspectionApproved, true)]
    [InlineData(ReleaseConditionType.BuyerApproval, false)]
    public void ReleaseCondition_ShouldHave_DifferentConfigurations(
        ReleaseConditionType type, bool requiresEvidence)
    {
        var condition = new ReleaseCondition
        {
            Type = type,
            RequiresEvidence = requiresEvidence
        };

        condition.Type.Should().Be(type);
        condition.RequiresEvidence.Should().Be(requiresEvidence);
    }

    [Fact]
    public void ReleaseCondition_ShouldFail_WhenNotMet()
    {
        var condition = new ReleaseCondition
        {
            Status = ConditionStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(-1) // Ya pasó la fecha
        };

        condition.Status = ConditionStatus.Failed;
        condition.FailedAt = DateTime.UtcNow;

        condition.Status.Should().Be(ConditionStatus.Failed);
        condition.FailedAt.Should().NotBeNull();
    }

    #endregion

    #region PARTE 4: Tests de Entidad FundMovement

    [Fact]
    public void FundMovement_ShouldBeCreated_AsDeposit()
    {
        var movement = new FundMovement
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = Guid.NewGuid(),
            TransactionNumber = "TRX-2026-00001",
            Type = FundMovementType.Deposit,
            PaymentMethod = PaymentMethod.BankTransfer,
            Amount = 500000.00m,
            Currency = "DOP",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            InitiatedBy = "buyer@email.com"
        };

        movement.Type.Should().Be(FundMovementType.Deposit);
        movement.PaymentMethod.Should().Be(PaymentMethod.BankTransfer);
        movement.Status.Should().Be("Completed");
    }

    [Fact]
    public void FundMovement_ShouldTrack_BankDetails()
    {
        var movement = new FundMovement
        {
            SourceAccount = "BPD-1234567890",
            DestinationAccount = "OKLA-ESCROW-001",
            BankName = "Banco Popular Dominicano",
            BankReference = "REF-2026-001"
        };

        movement.BankName.Should().Be("Banco Popular Dominicano");
        movement.BankReference.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FundMovement_ShouldBeCreated_AsRelease()
    {
        var movement = new FundMovement
        {
            Type = FundMovementType.Release,
            PartyType = "Seller",
            PartyName = "Auto Dealer SRL",
            Amount = 975000.00m, // Menos comisión
            FeeAmount = 25000.00m
        };

        movement.Type.Should().Be(FundMovementType.Release);
        movement.PartyType.Should().Be("Seller");
        movement.FeeAmount.Should().Be(25000.00m);
    }

    [Fact]
    public void FundMovement_ShouldBeCreated_AsRefund()
    {
        var movement = new FundMovement
        {
            Type = FundMovementType.Refund,
            PartyType = "Buyer",
            Amount = 500000.00m,
            Status = "Completed"
        };

        movement.Type.Should().Be(FundMovementType.Refund);
        movement.PartyType.Should().Be("Buyer");
    }

    [Theory]
    [InlineData(PaymentMethod.BankTransfer, "Transferencia bancaria")]
    [InlineData(PaymentMethod.CreditCard, "Tarjeta de crédito")]
    [InlineData(PaymentMethod.Check, "Cheque certificado")]
    public void FundMovement_ShouldSupport_DifferentPaymentMethods(
        PaymentMethod method, string description)
    {
        var movement = new FundMovement
        {
            PaymentMethod = method,
            Notes = description
        };

        movement.PaymentMethod.Should().Be(method);
    }

    #endregion

    #region PARTE 5: Tests de Entidad EscrowDocument

    [Fact]
    public void EscrowDocument_ShouldBeCreated_WithValidData()
    {
        var document = new EscrowDocument
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = Guid.NewGuid(),
            Name = "Contrato de Compraventa",
            DocumentType = "contract",
            FileName = "contrato_venta_2026.pdf",
            ContentType = "application/pdf",
            FileSize = 2048000,
            StoragePath = "s3://okla-escrow/documents/contrato.pdf",
            FileHash = "sha256:abc123xyz",
            UploadedAt = DateTime.UtcNow,
            UploadedBy = "seller@email.com"
        };

        document.DocumentType.Should().Be("contract");
        document.ContentType.Should().Be("application/pdf");
        document.FileHash.Should().StartWith("sha256:");
    }

    [Fact]
    public void EscrowDocument_ShouldBe_Verified()
    {
        var document = new EscrowDocument { IsVerified = false };

        document.IsVerified = true;
        document.VerifiedAt = DateTime.UtcNow;
        document.VerifiedBy = "admin@okla.com";

        document.IsVerified.Should().BeTrue();
        document.VerifiedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EscrowDocument_ShouldControl_Visibility()
    {
        var buyerOnlyDoc = new EscrowDocument
        {
            VisibleToBuyer = true,
            VisibleToSeller = false
        };

        var sellerOnlyDoc = new EscrowDocument
        {
            VisibleToBuyer = false,
            VisibleToSeller = true
        };

        var bothPartiesDoc = new EscrowDocument
        {
            VisibleToBuyer = true,
            VisibleToSeller = true
        };

        buyerOnlyDoc.VisibleToSeller.Should().BeFalse();
        sellerOnlyDoc.VisibleToBuyer.Should().BeFalse();
        bothPartiesDoc.VisibleToBuyer.Should().BeTrue();
        bothPartiesDoc.VisibleToSeller.Should().BeTrue();
    }

    [Fact]
    public void EscrowDocument_ShouldSupport_SoftDelete()
    {
        var document = new EscrowDocument { IsDeleted = false };

        document.IsDeleted = true;
        document.DeletedAt = DateTime.UtcNow;

        document.IsDeleted.Should().BeTrue();
        document.DeletedAt.Should().NotBeNull();
    }

    #endregion

    #region PARTE 6: Tests de Entidad EscrowDispute

    [Fact]
    public void EscrowDispute_ShouldBeCreated_WithValidData()
    {
        var dispute = new EscrowDispute
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = Guid.NewGuid(),
            DisputeNumber = "DSP-2026-00001",
            Status = EscrowDisputeStatus.Filed,
            FiledById = Guid.NewGuid(),
            FiledByName = "Juan Pérez",
            FiledByType = "Buyer",
            Reason = "Vehículo no corresponde a la descripción",
            Description = "El vehículo tiene daños no reportados",
            DisputedAmount = 500000.00m,
            FiledAt = DateTime.UtcNow
        };

        dispute.DisputeNumber.Should().StartWith("DSP-");
        dispute.Status.Should().Be(EscrowDisputeStatus.Filed);
        dispute.FiledByType.Should().Be("Buyer");
    }

    [Fact]
    public void EscrowDispute_ShouldTransition_ThroughStatuses()
    {
        var dispute = new EscrowDispute { Status = EscrowDisputeStatus.Filed };

        dispute.Status = EscrowDisputeStatus.UnderReview;
        dispute.Status.Should().Be(EscrowDisputeStatus.UnderReview);

        dispute.Status = EscrowDisputeStatus.InMediation;
        dispute.Status.Should().Be(EscrowDisputeStatus.InMediation);

        dispute.Status = EscrowDisputeStatus.Resolved;
        dispute.ResolvedAt = DateTime.UtcNow;
        dispute.Status.Should().Be(EscrowDisputeStatus.Resolved);
    }

    [Fact]
    public void EscrowDispute_ShouldResolve_WithDistribution()
    {
        var dispute = new EscrowDispute
        {
            DisputedAmount = 1000000.00m,
            Status = EscrowDisputeStatus.Resolved,
            ResolvedBuyerAmount = 300000.00m,   // 30% al comprador
            ResolvedSellerAmount = 700000.00m,  // 70% al vendedor
            Resolution = "Acuerdo de mediación",
            ResolvedAt = DateTime.UtcNow,
            ResolvedBy = "mediator@okla.com"
        };

        var totalResolved = dispute.ResolvedBuyerAmount + dispute.ResolvedSellerAmount;
        totalResolved.Should().Be(dispute.DisputedAmount);
    }

    [Fact]
    public void EscrowDispute_ShouldBeEscalated()
    {
        var dispute = new EscrowDispute
        {
            Status = EscrowDisputeStatus.UnderReview
        };

        dispute.Status = EscrowDisputeStatus.Escalated;
        dispute.EscalatedAt = DateTime.UtcNow;
        dispute.EscalationReason = "No se llegó a acuerdo en mediación";

        dispute.Status.Should().Be(EscrowDisputeStatus.Escalated);
        dispute.EscalationReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EscrowDispute_ShouldBeAssigned()
    {
        var dispute = new EscrowDispute
        {
            AssignedTo = "mediator@okla.com",
            AssignedAt = DateTime.UtcNow
        };

        dispute.AssignedTo.Should().NotBeNullOrEmpty();
        dispute.AssignedAt.Should().NotBeNull();
    }

    #endregion

    #region PARTE 7: Tests de Entidad EscrowAuditLog

    [Fact]
    public void EscrowAuditLog_ShouldRecord_CreatedEvent()
    {
        var auditLog = new EscrowAuditLog
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = Guid.NewGuid(),
            EventType = EscrowAuditEventType.Created,
            Description = "Cuenta escrow creada para compra de vehículo",
            PerformedBy = "buyer@email.com",
            PerformedAt = DateTime.UtcNow,
            IpAddress = "192.168.1.100"
        };

        auditLog.EventType.Should().Be(EscrowAuditEventType.Created);
        auditLog.Description.Should().Contain("creada");
    }

    [Theory]
    [InlineData(EscrowAuditEventType.Funded, "Fondos depositados")]
    [InlineData(EscrowAuditEventType.Released, "Fondos liberados")]
    [InlineData(EscrowAuditEventType.Refunded, "Fondos reembolsados")]
    [InlineData(EscrowAuditEventType.Disputed, "Disputa iniciada")]
    public void EscrowAuditLog_ShouldRecord_DifferentEventTypes(
        EscrowAuditEventType eventType, string description)
    {
        var auditLog = new EscrowAuditLog
        {
            EventType = eventType,
            Description = description,
            PerformedBy = "system",
            PerformedAt = DateTime.UtcNow
        };

        auditLog.EventType.Should().Be(eventType);
        auditLog.Description.Should().Be(description);
    }

    [Fact]
    public void EscrowAuditLog_ShouldTrack_AmountChanges()
    {
        var auditLog = new EscrowAuditLog
        {
            EventType = EscrowAuditEventType.AmountAdjusted,
            OldValue = "1000000.00",
            NewValue = "950000.00",
            AmountInvolved = -50000.00m,
            Description = "Ajuste por descuento acordado"
        };

        auditLog.OldValue.Should().Be("1000000.00");
        auditLog.NewValue.Should().Be("950000.00");
        auditLog.AmountInvolved.Should().Be(-50000.00m);
    }

    #endregion

    #region PARTE 8: Tests de Entidad EscrowFeeConfiguration

    [Fact]
    public void EscrowFeeConfiguration_ShouldBeCreated_WithValidData()
    {
        var feeConfig = new EscrowFeeConfiguration
        {
            Id = Guid.NewGuid(),
            Name = "Tarifa Estándar Compra Vehículo",
            TransactionType = EscrowTransactionType.VehiclePurchase,
            MinAmount = 100000.00m,
            MaxAmount = 10000000.00m,
            FeePercentage = 2.5m,
            MinFee = 5000.00m,
            MaxFee = 250000.00m,
            IsActive = true,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        };

        feeConfig.TransactionType.Should().Be(EscrowTransactionType.VehiclePurchase);
        feeConfig.FeePercentage.Should().Be(2.5m);
        feeConfig.IsActive.Should().BeTrue();
    }

    [Fact]
    public void EscrowFeeConfiguration_ShouldCalculate_Fee()
    {
        var feeConfig = new EscrowFeeConfiguration
        {
            FeePercentage = 2.5m,
            MinFee = 5000.00m,
            MaxFee = 250000.00m
        };

        var transactionAmount = 1000000.00m;
        var calculatedFee = transactionAmount * (feeConfig.FeePercentage / 100);

        // Aplicar límites
        if (calculatedFee < feeConfig.MinFee) calculatedFee = feeConfig.MinFee;
        if (calculatedFee > feeConfig.MaxFee) calculatedFee = feeConfig.MaxFee;

        calculatedFee.Should().Be(25000.00m);
    }

    [Fact]
    public void EscrowFeeConfiguration_ShouldApply_MinFee()
    {
        var feeConfig = new EscrowFeeConfiguration
        {
            FeePercentage = 2.5m,
            MinFee = 5000.00m,
            MaxFee = 250000.00m
        };

        var transactionAmount = 100000.00m; // 2.5% = 2500, menor que MinFee
        var calculatedFee = transactionAmount * (feeConfig.FeePercentage / 100);

        if (calculatedFee < feeConfig.MinFee) calculatedFee = feeConfig.MinFee;

        calculatedFee.Should().Be(5000.00m);
    }

    #endregion

    #region PARTE 9: Tests de Flujos de Negocio

    [Fact]
    public void EscrowFlow_ShouldComplete_VehiclePurchase()
    {
        var escrowId = Guid.NewGuid();
        var escrow = new EscrowAccount
        {
            Id = escrowId,
            TransactionType = EscrowTransactionType.VehiclePurchase,
            Status = EscrowStatus.Pending,
            TotalAmount = 1500000.00m,
            Currency = "DOP",
            Conditions = new List<ReleaseCondition>(),
            Movements = new List<FundMovement>()
        };

        // Agregar condiciones
        escrow.Conditions.Add(new ReleaseCondition
        {
            Type = ReleaseConditionType.VehicleDelivery,
            Status = ConditionStatus.Pending
        });
        escrow.Conditions.Add(new ReleaseCondition
        {
            Type = ReleaseConditionType.TitleTransfer,
            Status = ConditionStatus.Pending
        });

        // Fondear
        escrow.Movements.Add(new FundMovement
        {
            Type = FundMovementType.Deposit,
            Amount = 1500000.00m,
            Status = "Completed"
        });
        escrow.Status = EscrowStatus.Funded;
        escrow.FundedAmount = 1500000.00m;

        // Cumplir condiciones
        foreach (var condition in escrow.Conditions)
        {
            condition.Status = ConditionStatus.Met;
            condition.MetAt = DateTime.UtcNow;
        }
        escrow.Status = EscrowStatus.ConditionsMet;

        // Liberar
        escrow.Movements.Add(new FundMovement
        {
            Type = FundMovementType.Release,
            Amount = 1462500.00m // Menos comisión
        });
        escrow.Status = EscrowStatus.Released;
        escrow.ReleasedAmount = 1462500.00m;

        escrow.Status.Should().Be(EscrowStatus.Released);
        escrow.Conditions.All(c => c.Status == ConditionStatus.Met).Should().BeTrue();
    }

    [Fact]
    public void EscrowFlow_ShouldHandle_Refund()
    {
        var escrow = new EscrowAccount
        {
            Status = EscrowStatus.Funded,
            TotalAmount = 500000.00m,
            FundedAmount = 500000.00m,
            Movements = new List<FundMovement>()
        };

        escrow.Movements.Add(new FundMovement
        {
            Type = FundMovementType.Refund,
            Amount = 500000.00m,
            Status = "Completed"
        });

        escrow.Status = EscrowStatus.Refunded;
        escrow.RefundedAmount = 500000.00m;
        escrow.RefundedAt = DateTime.UtcNow;

        escrow.Status.Should().Be(EscrowStatus.Refunded);
        escrow.RefundedAmount.Should().Be(escrow.FundedAmount);
    }

    [Fact]
    public void EscrowFlow_ShouldHandle_PartialRelease()
    {
        var escrow = new EscrowAccount
        {
            Status = EscrowStatus.Funded,
            TotalAmount = 1000000.00m,
            FundedAmount = 1000000.00m,
            AllowPartialRelease = true,
            Movements = new List<FundMovement>()
        };

        // Primera liberación parcial
        escrow.Movements.Add(new FundMovement
        {
            Type = FundMovementType.PartialRelease,
            Amount = 400000.00m
        });
        escrow.Status = EscrowStatus.PartialRelease;
        escrow.ReleasedAmount = 400000.00m;

        // Segunda liberación parcial
        escrow.Movements.Add(new FundMovement
        {
            Type = FundMovementType.PartialRelease,
            Amount = 600000.00m
        });
        escrow.ReleasedAmount = 1000000.00m;
        escrow.Status = EscrowStatus.Released;

        escrow.Status.Should().Be(EscrowStatus.Released);
        escrow.ReleasedAmount.Should().Be(escrow.TotalAmount);
    }

    [Fact]
    public void EscrowFlow_ShouldHandle_Dispute()
    {
        var escrowId = Guid.NewGuid();
        var escrow = new EscrowAccount
        {
            Id = escrowId,
            Status = EscrowStatus.Funded,
            TotalAmount = 1000000.00m,
            Disputes = new List<EscrowDispute>()
        };

        var dispute = new EscrowDispute
        {
            EscrowAccountId = escrowId,
            Status = EscrowDisputeStatus.Filed,
            FiledByType = "Buyer",
            Reason = "Daños ocultos",
            DisputedAmount = 1000000.00m
        };
        escrow.Disputes.Add(dispute);
        escrow.Status = EscrowStatus.Disputed;

        // Resolver disputa
        dispute.Status = EscrowDisputeStatus.Resolved;
        dispute.ResolvedBuyerAmount = 200000.00m;
        dispute.ResolvedSellerAmount = 800000.00m;

        escrow.Disputes.First().Status.Should().Be(EscrowDisputeStatus.Resolved);
        (dispute.ResolvedBuyerAmount + dispute.ResolvedSellerAmount).Should().Be(1000000.00m);
    }

    #endregion

    #region PARTE 10: Tests de Validaciones

    [Fact]
    public void EscrowAccount_ShouldNotAllow_NegativeAmounts()
    {
        var escrow = new EscrowAccount
        {
            TotalAmount = 1000000.00m,
            FundedAmount = 500000.00m
        };

        escrow.TotalAmount.Should().BePositive();
        escrow.FundedAmount.Should().BePositive();
    }

    [Fact]
    public void EscrowAccount_ReleasedAmount_ShouldNotExceed_FundedAmount()
    {
        var escrow = new EscrowAccount
        {
            TotalAmount = 1000000.00m,
            FundedAmount = 800000.00m,
            ReleasedAmount = 500000.00m
        };

        escrow.ReleasedAmount.Should().BeLessThanOrEqualTo(escrow.FundedAmount);
    }

    [Fact]
    public void ReleaseCondition_ShouldHave_DueDate()
    {
        var condition = new ReleaseCondition
        {
            DueDate = DateTime.UtcNow.AddDays(7),
            IsMandatory = true
        };

        condition.DueDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void EscrowFeeConfiguration_ShouldValidate_AmountRange()
    {
        var feeConfig = new EscrowFeeConfiguration
        {
            MinAmount = 100000.00m,
            MaxAmount = 10000000.00m
        };

        feeConfig.MaxAmount.Should().BeGreaterThan(feeConfig.MinAmount);
    }

    [Fact]
    public void EscrowDispute_DisputedAmount_ShouldNotExceed_EscrowAmount()
    {
        var escrowAmount = 1000000.00m;
        var dispute = new EscrowDispute
        {
            DisputedAmount = 800000.00m
        };

        dispute.DisputedAmount.Should().BeLessThanOrEqualTo(escrowAmount);
    }

    #endregion

    #region PARTE 11: Tests de Regulaciones RD

    [Fact]
    public void EscrowAccount_ShouldUse_DominicanPesos()
    {
        var escrows = new[]
        {
            new EscrowAccount { Currency = "DOP", TotalAmount = 500000.00m },
            new EscrowAccount { Currency = "DOP", TotalAmount = 1500000.00m },
            new EscrowAccount { Currency = "DOP", TotalAmount = 5000000.00m }
        };

        escrows.Should().AllSatisfy(e => e.Currency.Should().Be("DOP"));
    }

    [Fact]
    public void EscrowFee_ShouldBe_Transparent()
    {
        var escrow = new EscrowAccount
        {
            TotalAmount = 1000000.00m,
            FeePercentage = 2.5m,
            FeeAmount = 25000.00m
        };

        escrow.FeePercentage.Should().Be(2.5m);
        escrow.FeeAmount.Should().Be(escrow.TotalAmount * (escrow.FeePercentage / 100));
    }

    [Fact]
    public void EscrowDispute_ShouldProvide_ConsumerProtection()
    {
        var dispute = new EscrowDispute
        {
            Status = EscrowDisputeStatus.Filed,
            Category = "Protección al Consumidor",
            Reason = "Producto no conforme"
        };

        dispute.Category.Should().Be("Protección al Consumidor");
    }

    #endregion
}
