-- =====================================================
-- SEED DATA FOR BILLING SERVICE
-- Run this script on the billingservice database
-- to populate test data for dealer billing dashboard
-- =====================================================

-- First, let's check the existing dealer from userservice
-- We'll use a known dealer ID from the system

-- Sample Dealer IDs (you may need to update these with actual IDs from your userservice)
-- These are placeholder UUIDs that should match your existing dealers

-- Create sample subscriptions
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
-- Professional plan dealer
(
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    '11111111-1111-1111-1111-111111111111', -- Replace with actual dealer ID
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
),
-- Basic plan dealer
(
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    '22222222-2222-2222-2222-222222222222', -- Replace with actual dealer ID
    1, -- Basic = 1
    1, -- Active = 1
    0, -- Monthly = 0
    29.00,
    'USD',
    NOW() - INTERVAL '3 months',
    NULL,
    NOW() + INTERVAL '8 days',
    5,
    50,
    '{"analytics": true, "crm": true}',
    NOW() - INTERVAL '3 months'
),
-- Enterprise plan dealer
(
    'c3d4e5f6-a7b8-9012-cdef-123456789012',
    '33333333-3333-3333-3333-333333333333', -- Replace with actual dealer ID
    3, -- Enterprise = 3
    1, -- Active = 1
    2, -- Yearly = 2
    1990.00,
    'USD',
    NOW() - INTERVAL '1 year',
    NULL,
    NOW() + INTERVAL '180 days',
    -1, -- Unlimited
    -1, -- Unlimited
    '{"analytics": true, "api": true, "crm": true, "customBranding": true, "dedicatedManager": true}',
    NOW() - INTERVAL '1 year'
)
ON CONFLICT ("Id") DO NOTHING;

-- Create sample invoices for the professional plan dealer
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
-- Paid invoices
(
    'inv-0001-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111111',
    'INV-202601-00001',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
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
    'inv-0002-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111111',
    'INV-202601-00002',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
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
    'inv-0003-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111111',
    'INV-202601-00003',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
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
    'inv-0004-0000-0000-000000000004',
    '11111111-1111-1111-1111-111111111111',
    'INV-202601-00004',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
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
    'inv-0005-0000-0000-000000000005',
    '11111111-1111-1111-1111-111111111111',
    'INV-202601-00005',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
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
    'inv-0006-0000-0000-000000000006',
    '11111111-1111-1111-1111-111111111111',
    'INV-202601-00006',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
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
    'pay-0001-0000-0000-000000000001',
    '11111111-1111-1111-1111-111111111111',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'inv-0001-0000-0000-000000000001',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription payment - Professional',
    '/receipts/pay-0001.pdf',
    0.00,
    NOW() - INTERVAL '5 months' + INTERVAL '2 days',
    NOW() - INTERVAL '5 months' + INTERVAL '2 days'
),
(
    'pay-0002-0000-0000-000000000002',
    '11111111-1111-1111-1111-111111111111',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'inv-0002-0000-0000-000000000002',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription payment - Professional',
    '/receipts/pay-0002.pdf',
    0.00,
    NOW() - INTERVAL '4 months' + INTERVAL '3 days',
    NOW() - INTERVAL '4 months' + INTERVAL '3 days'
),
(
    'pay-0003-0000-0000-000000000003',
    '11111111-1111-1111-1111-111111111111',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'inv-0003-0000-0000-000000000003',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription payment - Professional',
    '/receipts/pay-0003.pdf',
    0.00,
    NOW() - INTERVAL '3 months' + INTERVAL '1 day',
    NOW() - INTERVAL '3 months' + INTERVAL '1 day'
),
(
    'pay-0004-0000-0000-000000000004',
    '11111111-1111-1111-1111-111111111111',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'inv-0004-0000-0000-000000000004',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription payment - Professional',
    '/receipts/pay-0004.pdf',
    0.00,
    NOW() - INTERVAL '2 months' + INTERVAL '5 days',
    NOW() - INTERVAL '2 months' + INTERVAL '5 days'
),
(
    'pay-0005-0000-0000-000000000005',
    '11111111-1111-1111-1111-111111111111',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'inv-0005-0000-0000-000000000005',
    79.00,
    'USD',
    2, -- Succeeded = 2
    0, -- CreditCard = 0
    'Monthly subscription payment - Professional',
    '/receipts/pay-0005.pdf',
    0.00,
    NOW() - INTERVAL '1 month' + INTERVAL '2 days',
    NOW() - INTERVAL '1 month' + INTERVAL '2 days'
)
ON CONFLICT ("Id") DO NOTHING;

-- Create sample Stripe customer records
INSERT INTO "StripeCustomers" (
    "Id",
    "DealerId",
    "StripeCustomerId",
    "Email",
    "Name",
    "Phone",
    "DefaultPaymentMethodId",
    "IsActive",
    "CreatedAt"
) VALUES
(
    'd1e2f3a4-b5c6-7890-1234-567890abcdef',
    '11111111-1111-1111-1111-111111111111',
    'cus_sample_professional',
    'dealer.pro@okla.com.do',
    'Auto Premium RD',
    '+1-809-555-0123',
    'pm_sample_card_visa',
    true,
    NOW() - INTERVAL '6 months'
),
(
    'd2e3f4a5-b6c7-8901-2345-67890abcdef1',
    '22222222-2222-2222-2222-222222222222',
    'cus_sample_basic',
    'dealer.basic@okla.com.do',
    'Autos Económicos SRL',
    '+1-809-555-0456',
    'pm_sample_card_mc',
    true,
    NOW() - INTERVAL '3 months'
),
(
    'd3e4f5a6-b7c8-9012-3456-7890abcdef12',
    '33333333-3333-3333-3333-333333333333',
    'cus_sample_enterprise',
    'dealer.enterprise@okla.com.do',
    'Mega Autos Internacional',
    '+1-809-555-0789',
    'pm_sample_card_amex',
    true,
    NOW() - INTERVAL '1 year'
)
ON CONFLICT ("Id") DO NOTHING;

-- Summary query to verify the data
SELECT 'Subscriptions:' as type, COUNT(*) as count FROM "Subscriptions"
UNION ALL
SELECT 'Invoices:', COUNT(*) FROM "Invoices"
UNION ALL
SELECT 'Payments:', COUNT(*) FROM "Payments"
UNION ALL
SELECT 'StripeCustomers:', COUNT(*) FROM "StripeCustomers";
