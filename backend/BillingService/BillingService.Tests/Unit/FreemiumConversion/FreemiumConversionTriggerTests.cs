using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BillingService.Tests.Unit.FreemiumConversion;

/// <summary>
/// Unit tests for Freemium Conversion Triggers.
/// Tests the business logic that automatically triggers emails and promotions
/// to convert free-tier dealers to paying customers based on specific behaviors.
///
/// ## Conversion Trigger Spec (OKLA Freemium Model v3):
/// 
/// 1. **5-Inquiries-in-30-Days Trigger** → "Upgrade to VISIBLE"
///    - Dealer on LIBRE plan receives exactly 5 inquiries within 30-day window
///    - Email: "Congratulations! You're trending. Upgrade to VISIBLE for more visibility"
///    - Offer: First month 50% off ($14.50 instead of $29)
///    - Should fire only ONCE per dealer per 30-day period
/// 
/// 2. **45-Days-No-Sale Trigger** → "Upgrade to Premium Listing"
///    - Dealer on LIBRE plan has vehicle listed for 45+ consecutive days WITHOUT a sale
///    - Email: "Dealers with VISIBLE plan sell on average 18 days faster"
///    - CTA: "Upgrade this listing to VISIBLE"
///    - Should fire only ONCE per vehicle per 45-day cycle
///
/// 3. **0-Inquiries-in-14-Days Trigger** → "Free Trial of Visibility"
///    - Dealer on LIBRE plan with 0 inquiries in last 14 days
///    - Email: "Get 7 days of FREE VISIBLE visibility to boost your sales"
///    - Trial grants: Increased listing exposure, Featured slot access
///    - Should fire only ONCE per dealer per month
///    - Manual cleanup: Must be disabled/reset if dealer takes action
///
/// 4. **Duplicate Prevention**:
///    - Same trigger should NOT fire twice for same dealer in same period
///    - Triggers must have cooldown or state tracking to prevent spam
///    - Email open rate tracking to measure effectiveness
/// </summary>
public class FreemiumConversionTriggerTests
{
    // ════════════════════════════════════════════════════════════════════════
    // Domain Models for Testing
    // ════════════════════════════════════════════════════════════════════════

    public enum PlanType { LIBRE, VISIBLE, PROFESSIONAL, ENTERPRISE }
    public enum TriggerType { InquiriesMilestone, NoSalePenalty, InactivityWarning }
    public enum TriggeredAction { ConversionEmail, TrialOffer, PremiumUpgrade }

    public class Dealer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PlanType CurrentPlan { get; set; } = PlanType.LIBRE;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpgradeAt { get; set; }
        public List<Inquiry> Inquiries { get; set; } = new();
        public List<Vehicle> Vehicles { get; set; } = new();
        public List<ConversionEvent> ConversionHistory { get; set; } = new();

        // Metadata for trigger cooldowns
        public Dictionary<string, DateTime> LastTriggerDates { get; set; } = new();
    }

    public class Inquiry
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BuyerName { get; set; } = string.Empty;
    }

    public class Vehicle
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ListedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public bool IsActive { get; set; } = true;

        public int DaysListed => (int)(DateTime.UtcNow - ListedAt).TotalDays;
        public bool HasBeenSold => SoldAt.HasValue;
    }

    public class ConversionEvent
    {
        public Guid Id { get; set; }
        public Guid DealerId { get; set; }
        public TriggerType TriggerType { get; set; }
        public TriggeredAction Action { get; set; }
        public DateTime FiredAt { get; set; }
        public bool EmailSent { get; set; }
        public string EmailTemplateKey { get; set; } = string.Empty;
        public Dictionary<string, object> TemplateParams { get; set; } = new();
    }

    // ════════════════════════════════════════════════════════════════════════
    // Trigger Engine Implementation
    // ════════════════════════════════════════════════════════════════════════

    public class ConversionTriggerEngine
    {
        private const int InquiryThreshold = 5;
        private const int InquiryWindowDays = 30;
        private const int NoSaleDays = 45;
        private const int InactivityDays = 14;
        private const int TriggerCooldownDays = 30;

        public List<ConversionEvent> CalculatePendingTriggersForDealer(Dealer dealer)
        {
            var triggers = new List<ConversionEvent>();
            var now = DateTime.UtcNow;

            // Only process LIBRE dealers
            if (dealer.CurrentPlan != PlanType.LIBRE)
                return triggers;

            // ── Trigger 1: 5 Inquiries in 30 Days → Conversion Email ──────
            var recentInquiries = dealer.Inquiries
                .Where(i => (now - i.CreatedAt).TotalDays <= InquiryWindowDays)
                .ToList();

            if (recentInquiries.Count >= InquiryThreshold &&
                !HasRecentTrigger(dealer, TriggerType.InquiriesMilestone, TriggerCooldownDays))
            {
                triggers.Add(new ConversionEvent
                {
                    Id = Guid.NewGuid(),
                    DealerId = dealer.Id,
                    TriggerType = TriggerType.InquiriesMilestone,
                    Action = TriggeredAction.ConversionEmail,
                    FiredAt = now,
                    EmailTemplateKey = "upgrade_to_visible_milestone",
                    TemplateParams = new Dictionary<string, object>
                    {
                        { "InquiryCount", recentInquiries.Count },
                        { "DaysWindow", InquiryWindowDays },
                        { "DiscountPercent", 50 },
                        { "DiscountedPrice", 14.50m },
                        { "RegularPrice", 29.00m }
                    }
                });
            }

            // ── Trigger 2: 45+ Days Unsold Vehicle → Premium Listing Email ──
            var unsoldOldListings = dealer.Vehicles
                .Where(v => v.IsActive && !v.HasBeenSold && v.DaysListed >= NoSaleDays)
                .ToList();

            foreach (var vehicle in unsoldOldListings)
            {
                // Check if trigger already fired for THIS vehicle in THIS period
                var existingTrigger = dealer.ConversionHistory
                    .FirstOrDefault(e =>
                        e.TriggerType == TriggerType.NoSalePenalty &&
                        e.TemplateParams.TryGetValue("VehicleId", out var vid) &&
                        vid.Equals(vehicle.Id) &&
                        (now - e.FiredAt).TotalDays < NoSaleDays);

                if (existingTrigger == null)
                {
                    triggers.Add(new ConversionEvent
                    {
                        Id = Guid.NewGuid(),
                        DealerId = dealer.Id,
                        TriggerType = TriggerType.NoSalePenalty,
                        Action = TriggeredAction.PremiumUpgrade,
                        FiredAt = now,
                        EmailTemplateKey = "premium_listing_45days_nosale",
                        TemplateParams = new Dictionary<string, object>
                        {
                            { "VehicleId", vehicle.Id },
                            { "VehicleTitle", vehicle.Title },
                            { "DaysListed", vehicle.DaysListed },
                            { "AvgSaleDaysDifference", 18 },
                            { "VisiblePlanMonthlyPrice", 29.00m }
                        }
                    });
                }
            }

            // ── Trigger 3: 0 Inquiries in 14 Days → Free Trial Email ────────
            var recentInquiries14Days = dealer.Inquiries
                .Where(i => (now - i.CreatedAt).TotalDays <= InactivityDays)
                .ToList();

            if (recentInquiries14Days.Count == 0 &&
                !HasRecentTrigger(dealer, TriggerType.InactivityWarning, TriggerCooldownDays))
            {
                triggers.Add(new ConversionEvent
                {
                    Id = Guid.NewGuid(),
                    DealerId = dealer.Id,
                    TriggerType = TriggerType.InactivityWarning,
                    Action = TriggeredAction.TrialOffer,
                    FiredAt = now,
                    EmailTemplateKey = "free_7day_visibility_trial",
                    TemplateParams = new Dictionary<string, object>
                    {
                        { "TrialDays", 7 },
                        { "InactivityDays", InactivityDays },
                        { "TrialFeatures", new[] { "Increased listing exposure", "Featured slot access", "Priority to inquiries" } }
                    }
                });
            }

            return triggers;
        }

        private bool HasRecentTrigger(Dealer dealer, TriggerType triggerType, int windowDays)
        {
            return dealer.ConversionHistory
                .Any(e => e.TriggerType == triggerType &&
                          (DateTime.UtcNow - e.FiredAt).TotalDays < windowDays);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // TEST CASES
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Trigger_5IngquiriesIn30Days_FiresConversionEmail()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var dealer = new Dealer
        {
            Id = dealerId,
            Name = "Auto Venta RD",
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var now = DateTime.UtcNow;
        // Add exactly 5 inquiries in last 30 days (spread across 6-day intervals = 24 days total)
        for (int i = 0; i < 5; i++)
        {
            dealer.Inquiries.Add(new Inquiry
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                VehicleId = Guid.NewGuid(),
                CreatedAt = now.AddDays(-6 * i), // 0, -6, -12, -18, -24 days (all within 30)
                BuyerName = $"Buyer {i + 1}"
            });
        }

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Should().HaveCount(1, "exactly one trigger should fire for 5 inquiries");
        var trigger = triggers.First();
        trigger.TriggerType.Should().Be(TriggerType.InquiriesMilestone);
        trigger.Action.Should().Be(TriggeredAction.ConversionEmail);
        trigger.EmailTemplateKey.Should().Be("upgrade_to_visible_milestone");
        trigger.TemplateParams["InquiryCount"].Should().Be(5);
        trigger.TemplateParams["DiscountPercent"].Should().Be(50);
    }

    [Fact]
    public void Trigger_4IngquiriesIn30Days_DoesNotFire()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            Name = "Low Activity Dealer",
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var now = DateTime.UtcNow;
        for (int i = 0; i < 4; i++) // Only 4 inquiries - should NOT trigger
        {
            dealer.Inquiries.Add(new Inquiry
            {
                Id = Guid.NewGuid(),
                DealerId = dealer.Id,
                VehicleId = Guid.NewGuid(),
                CreatedAt = now.AddDays(-6 * i), // All within 30 days
                BuyerName = $"Buyer {i + 1}"
            });
        }

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Where(t => t.TriggerType == TriggerType.InquiriesMilestone)
            .Should().BeEmpty("trigger should not fire for less than 5 inquiries");
    }

    [Fact]
    public void Trigger_45DaysUnsoldVehicle_FiresPremiumListingEmail()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var dealer = new Dealer
        {
            Id = dealerId,
            Name = "Car Dealer",
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        var vehicleId = Guid.NewGuid();
        dealer.Vehicles.Add(new Vehicle
        {
            Id = vehicleId,
            DealerId = dealerId,
            Title = "2020 Toyota Corolla",
            ListedAt = DateTime.UtcNow.AddDays(-45), // Exactly 45 days old
            SoldAt = null,
            IsActive = true
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        var noSaleTrigger = triggers.FirstOrDefault(t => t.TriggerType == TriggerType.NoSalePenalty);
        noSaleTrigger.Should().NotBeNull("45-day no-sale trigger should fire");
        noSaleTrigger!.Action.Should().Be(TriggeredAction.PremiumUpgrade);
        noSaleTrigger.EmailTemplateKey.Should().Be("premium_listing_45days_nosale");
        noSaleTrigger.TemplateParams["VehicleId"].Should().Be(vehicleId);
        noSaleTrigger.TemplateParams["DaysListed"].Should().Be(45);
    }

    [Fact]
    public void Trigger_44DaysUnsoldVehicle_DoesNotFire()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        dealer.Vehicles.Add(new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            Title = "2020 Honda Civic",
            ListedAt = DateTime.UtcNow.AddDays(-44), // Only 44 days old
            SoldAt = null,
            IsActive = true
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Where(t => t.TriggerType == TriggerType.NoSalePenalty)
            .Should().BeEmpty("trigger should not fire for vehicles under 45 days");
    }

    [Fact]
    public void Trigger_SoldVehicleAfter45Days_DoesNotFire()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        dealer.Vehicles.Add(new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            Title = "2020 Chevrolet Spark",
            ListedAt = DateTime.UtcNow.AddDays(-50),
            SoldAt = DateTime.UtcNow.AddDays(-40), // Sold before 45 days
            IsActive = false
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Where(t => t.TriggerType == TriggerType.NoSalePenalty)
            .Should().BeEmpty("trigger should not fire for sold vehicles");
    }

    [Fact]
    public void Trigger_ZeroInquiriesIn14Days_FiresTrialOffer()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Dealer",
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        // Add old inquiries (outside the 14-day window)
        dealer.Inquiries.Add(new Inquiry
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            VehicleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            BuyerName = "Old Buyer"
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        var inactivityTrigger = triggers.FirstOrDefault(t => t.TriggerType == TriggerType.InactivityWarning);
        inactivityTrigger.Should().NotBeNull("inactivity trigger should fire for 0 inquiries in 14 days");
        inactivityTrigger!.Action.Should().Be(TriggeredAction.TrialOffer);
        inactivityTrigger.EmailTemplateKey.Should().Be("free_7day_visibility_trial");
        inactivityTrigger.TemplateParams["TrialDays"].Should().Be(7);
    }

    [Fact]
    public void Trigger_OneInquiryIn14Days_DoesNotFire()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        // Add 1 inquiry in last 14 days
        dealer.Inquiries.Add(new Inquiry
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            VehicleId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            BuyerName = "Recent Buyer"
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Where(t => t.TriggerType == TriggerType.InactivityWarning)
            .Should().BeEmpty("trigger should not fire if dealer has any recent inquiry");
    }

    [Fact]
    public void Trigger_PaidDealerWithHighActivity_DoesNotFire()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            CurrentPlan = PlanType.VISIBLE, // Not LIBRE
            CreatedAt = DateTime.UtcNow.AddDays(-60),
            LastUpgradeAt = DateTime.UtcNow.AddDays(-30)
        };

        // Add new inquiries
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            dealer.Inquiries.Add(new Inquiry
            {
                Id = Guid.NewGuid(),
                DealerId = dealer.Id,
                VehicleId = Guid.NewGuid(),
                CreatedAt = now.AddDays(-6 * i), // All within 30 days
                BuyerName = $"Buyer {i + 1}"
            });
        }

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Should().BeEmpty("paid dealers should not receive conversion triggers");
    }

    [Fact]
    public void Trigger_DuplicatePrevention_SameTriggerDoesNotFireTwiceIn30Days()
    {
        // Arrange
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        // Add 5 inquiries to trigger the conversion
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            dealer.Inquiries.Add(new Inquiry
            {
                Id = Guid.NewGuid(),
                DealerId = dealer.Id,
                VehicleId = Guid.NewGuid(),
                CreatedAt = now.AddDays(-6 * i), // All within 30 days
                BuyerName = $"Buyer {i + 1}"
            });
        }

        // Record that this trigger already fired recently
        dealer.ConversionHistory.Add(new ConversionEvent
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            TriggerType = TriggerType.InquiriesMilestone,
            Action = TriggeredAction.ConversionEmail,
            FiredAt = now.AddDays(-10), // Fired 10 days ago
            EmailSent = true,
            EmailTemplateKey = "upgrade_to_visible_milestone"
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Where(t => t.TriggerType == TriggerType.InquiriesMilestone)
            .Should().BeEmpty("same trigger should not fire twice within 30 days");
    }

    [Fact]
    public void Trigger_MultipleTriggersSameDealer_AllCanFireIfQualified()
    {
        // Arrange
        var dealerId = Guid.NewGuid();
        var dealer = new Dealer
        {
            Id = dealerId,
            Name = "Very Active Dealer",
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        var now = DateTime.UtcNow;

        // Trigger 1: 5 inquiries in 30 days
        for (int i = 0; i < 5; i++)
        {
            dealer.Inquiries.Add(new Inquiry
            {
                Id = Guid.NewGuid(),
                DealerId = dealerId,
                VehicleId = Guid.NewGuid(),
                CreatedAt = now.AddDays(-6 * i), // All within 30 days
                BuyerName = $"Buyer {i + 1}"
            });
        }

        // Trigger 2: 45-day unsold vehicle
        dealer.Vehicles.Add(new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = dealerId,
            Title = "2019 Nissan Sentra",
            ListedAt = now.AddDays(-50),
            SoldAt = null,
            IsActive = true
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Should().HaveCount(2, "two different triggers should fire if conditions are met");
        triggers.Should()
            .Contain(t => t.TriggerType == TriggerType.InquiriesMilestone,
                     "inquiries milestone trigger should fire")
            .And
            .Contain(t => t.TriggerType == TriggerType.NoSalePenalty,
                     "no-sale penalty trigger should fire");
    }

    [Fact]
    public void Trigger_InactivityAndNoSaleSimultaneously_BothCanFire()
    {
        // Arrange: Dealer with vehicle and no recent inquiries
        var dealer = new Dealer
        {
            Id = Guid.NewGuid(),
            CurrentPlan = PlanType.LIBRE,
            CreatedAt = DateTime.UtcNow.AddDays(-100)
        };

        var now = DateTime.UtcNow;

        // Old inquiries (outside 14-day window)
        dealer.Inquiries.Add(new Inquiry
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            VehicleId = Guid.NewGuid(),
            CreatedAt = now.AddDays(-20),
            BuyerName = "Old Buyer"
        });

        // 45+ day unsold vehicle
        dealer.Vehicles.Add(new Vehicle
        {
            Id = Guid.NewGuid(),
            DealerId = dealer.Id,
            Title = "2018 Hyundai Elantra",
            ListedAt = now.AddDays(-50),
            SoldAt = null,
            IsActive = true
        });

        var engine = new ConversionTriggerEngine();

        // Act
        var triggers = engine.CalculatePendingTriggersForDealer(dealer);

        // Assert
        triggers.Should().HaveCount(2);
        triggers.Should()
            .Contain(t => t.TriggerType == TriggerType.InactivityWarning)
            .And
            .Contain(t => t.TriggerType == TriggerType.NoSalePenalty);
    }
}
