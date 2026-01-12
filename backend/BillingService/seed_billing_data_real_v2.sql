-- =====================================================
-- SEED DATA FOR BILLING SERVICE
-- Run this script on the billingservice database
-- Uses REAL dealer ID from the system: 62b4318c-3900-4a8d-a24b-b705f4b30884 (dealer@test.com)
-- =====================================================

-- Real Dealer ID from userservice (dealer@test.com)
-- UserId: 62b4318c-3900-4a8d-a24b-b705f4b30884

-- Create sample subscription for the real dealer
INSERT INTO "Subscriptions" (
    "Id",
    "DealerId", 
    "Plan", 
    "Status", 
    "Cycle",
    "PricePerCycle",
    "Currency",
    "StartDate",
    "TrialEndDate",
    "NextBillingDate",
    "MaxUsers",
    "MaxVehicles",
    "Features",
    "CreatedAt"
) VALUES
-- Professional plan for dealer@test.com
(
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    2, -- Professional = 2
    1, -- Active = 1
    0, -- Monthly = 0
    79.00,
    'USD',
    NOW() - INTERVAL '6 months',
    NULL,
    NOW() + INTERVAL '15 days',
    20,
    500,
    '{"analytics": true, "api": true, "crm": true, "customBranding": true}',
    NOW() - INTERVAL '6 months'
)
ON CONFLICT ("Id") DO UPDATE SET
    "Status" = EXCLUDED."Status",
    "NextBillingDate" = EXCLUDED."NextBillingDate";

-- Create sample invoices for dealer@test.com
INSERT INTO "Invoices" (
    "Id",
    "DealerId",
    "InvoiceNumber",
    "SubscriptionId",
    "Status",
    "Subtotal",
    "TaxAmount",
    "TotalAmount",
    "PaidAmount",
    "Currency",
    "IssueDate",
    "DueDate",
    "PaidDate",
    "CreatedAt"
) VALUES
-- Paid invoices from previous months
(
    'b1c2d3e4-f5a6-4b78-9012-cdef12345671',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00001',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    3, -- Paid = 3
    79.00,
    0.00,
    79.00,
    79.00,
    'USD',
    NOW() - INTERVAL '5 months',
    NOW() - INTERVAL '5 months' + INTERVAL '30 days',
    NOW() - INTERVAL '5 months' + INTERVAL '2 days',
    NOW() - INTERVAL '5 months'
),
(
    'b2c3d4e5-f6a7-4c89-0123-def123456782',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00002',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    3, -- Paid = 3
    79.00,
    0.00,
    79.00,
    79.00,
    'USD',
    NOW() - INTERVAL '4 months',
    NOW() - INTERVAL '4 months' + INTERVAL '30 days',
    NOW() - INTERVAL '4 months' + INTERVAL '3 days',
    NOW() - INTERVAL '4 months'
),
(
    'b3c4d5e6-f7a8-4d90-1234-ef1234567893',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00003',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    3, -- Paid = 3
    79.00,
    0.00,
    79.00,
    79.00,
    'USD',
    NOW() - INTERVAL '3 months',
    NOW() - INTERVAL '3 months' + INTERVAL '30 days',
    NOW() - INTERVAL '3 months' + INTERVAL '1 day',
    NOW() - INTERVAL '3 months'
),
(
    'b4c5d6e7-f8a9-4e01-2345-f01234567804',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00004',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    3, -- Paid = 3
    79.00,
    0.00,
    79.00,
    79.00,
    'USD',
    NOW() - INTERVAL '2 months',
    NOW() - INTERVAL '2 months' + INTERVAL '30 days',
    NOW() - INTERVAL '2 months' + INTERVAL '5 days',
    NOW() - INTERVAL '2 months'
),
(
    'b5c6d7e8-f9a0-4f12-3456-012345678915',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00005',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    3, -- Paid = 3
    79.00,
    0.00,
    79.00,
    79.00,
    'USD',
    NOW() - INTERVAL '1 month',
    NOW() - INTERVAL '1 month' + INTERVAL '30 days',
    NOW() - INTERVAL '1 month' + INTERVAL '2 days',
    NOW() - INTERVAL '1 month'
),
-- Current month - pending invoice
(
    'b6c7d8e9-f0a1-4023-4567-123456789026',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00006',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    2, -- Sent = 2
    79.00,
    0.00,
    79.00,
    0.00,
    'USD',
    NOW(),
    NOW() + INTERVAL '30 days',
    NULL,
    NOW()
)
ON CONFLICT ("Id") DO NOTHING;

-- Create sample payments
INSERT INTO "Payments" (
    "Id",
    "DealerId",
    "SubscriptionId",
    "InvoiceId",
    "Amount",
    "Currency",
    "Status",
    "Method",
    "Description",
    "ReceiptUrl",
    "RefundedAmount",
    "CreatedAt",
    "ProcessedAt"
) VALUES
(
    'c1d2e3f4-a5b6-4c78-9012-def123456701',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    'b1c2d3e4-f5a6-4b78-9012-cdef12345671',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-0001.pdf',
    0.00,
    NOW() - INTERVAL '5 months' + INTERVAL '2 days',
    NOW() - INTERVAL '5 months' + INTERVAL '2 days'
),
(
    'c2d3e4f5-a6b7-4d89-0123-ef1234567812',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    'b2c3d4e5-f6a7-4c89-0123-def123456782',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-0002.pdf',
    0.00,
    NOW() - INTERVAL '4 months' + INTERVAL '3 days',
    NOW() - INTERVAL '4 months' + INTERVAL '3 days'
),
(
    'c3d4e5f6-a7b8-4e90-1234-f01234567923',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    'b3c4d5e6-f7a8-4d90-1234-ef1234567893',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-0003.pdf',
    0.00,
    NOW() - INTERVAL '3 months' + INTERVAL '1 day',
    NOW() - INTERVAL '3 months' + INTERVAL '1 day'
),
(
    'c4d5e6f7-a8b9-4f01-2345-012345678034',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    'b4c5d6e7-f8a9-4e01-2345-f01234567804',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-0004.pdf',
    0.00,
    NOW() - INTERVAL '2 months' + INTERVAL '5 days',
    NOW() - INTERVAL '2 months' + INTERVAL '5 days'
),
(
    'c5d6e7f8-a9b0-4012-3456-123456789145',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'a1b2c3d4-e5f6-4890-abcd-ef1234567890',
    'b5c6d7e8-f9a0-4f12-3456-012345678915',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-0005.pdf',
    0.00,
    NOW() - INTERVAL '1 month' + INTERVAL '2 days',
    NOW() - INTERVAL '1 month' + INTERVAL '2 days'
)
ON CONFLICT ("Id") DO NOTHING;

-- Create Stripe customer record
INSERT INTO "StripeCustomers" (
    "Id",
    "DealerId",
    "StripeCustomerId",
    "Email",
    "Name",
    "DefaultPaymentMethodId",
    "CreatedAt"
) VALUES
(
    'd1e2f3a4-b5c6-4d78-9012-ef1234567801',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'cus_test_dealer01',
    'dealer@test.com',
    'Test Dealer',
    'pm_card_visa',
    NOW() - INTERVAL '6 months'
)
ON CONFLICT ("Id") DO NOTHING;

-- Verify data was inserted
SELECT 'Subscriptions inserted:' as info, COUNT(*) FROM "Subscriptions" WHERE "DealerId" = '62b4318c-3900-4a8d-a24b-b705f4b30884';
SELECT 'Invoices inserted:' as info, COUNT(*) FROM "Invoices" WHERE "DealerId" = '62b4318c-3900-4a8d-a24b-b705f4b30884';
SELECT 'Payments inserted:' as info, COUNT(*) FROM "Payments" WHERE "DealerId" = '62b4318c-3900-4a8d-a24b-b705f4b30884';
SELECT 'Stripe Customers inserted:' as info, COUNT(*) FROM "StripeCustomers" WHERE "DealerId" = '62b4318c-3900-4a8d-a24b-b705f4b30884';

SELECT 'Billing seed data for dealer@test.com completed successfully!' as result;
