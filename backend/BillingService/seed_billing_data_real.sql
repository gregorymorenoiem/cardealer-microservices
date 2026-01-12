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
    'sub-real-0001-0000-000000000001',
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
    'inv-real-0001-0000-000000000001',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00001',
    'sub-real-0001-0000-000000000001',
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
    'inv-real-0002-0000-000000000002',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00002',
    'sub-real-0001-0000-000000000001',
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
    'inv-real-0003-0000-000000000003',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00003',
    'sub-real-0001-0000-000000000001',
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
    'inv-real-0004-0000-000000000004',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00004',
    'sub-real-0001-0000-000000000001',
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
    'inv-real-0005-0000-000000000005',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00005',
    'sub-real-0001-0000-000000000001',
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
    'inv-real-0006-0000-000000000006',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'INV-2026-00006',
    'sub-real-0001-0000-000000000001',
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
    'pay-real-0001-0000-000000000001',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'sub-real-0001-0000-000000000001',
    'inv-real-0001-0000-000000000001',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-real-0001.pdf',
    0.00,
    NOW() - INTERVAL '5 months' + INTERVAL '2 days',
    NOW() - INTERVAL '5 months' + INTERVAL '2 days'
),
(
    'pay-real-0002-0000-000000000002',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'sub-real-0001-0000-000000000001',
    'inv-real-0002-0000-000000000002',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-real-0002.pdf',
    0.00,
    NOW() - INTERVAL '4 months' + INTERVAL '3 days',
    NOW() - INTERVAL '4 months' + INTERVAL '3 days'
),
(
    'pay-real-0003-0000-000000000003',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'sub-real-0001-0000-000000000001',
    'inv-real-0003-0000-000000000003',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-real-0003.pdf',
    0.00,
    NOW() - INTERVAL '3 months' + INTERVAL '1 day',
    NOW() - INTERVAL '3 months' + INTERVAL '1 day'
),
(
    'pay-real-0004-0000-000000000004',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'sub-real-0001-0000-000000000001',
    'inv-real-0004-0000-000000000004',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-real-0004.pdf',
    0.00,
    NOW() - INTERVAL '2 months' + INTERVAL '5 days',
    NOW() - INTERVAL '2 months' + INTERVAL '5 days'
),
(
    'pay-real-0005-0000-000000000005',
    '62b4318c-3900-4a8d-a24b-b705f4b30884',
    'sub-real-0001-0000-000000000001',
    'inv-real-0005-0000-000000000005',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription - Professional',
    '/receipts/pay-real-0005.pdf',
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
    'cust-real-0001-0000-000000000001',
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
