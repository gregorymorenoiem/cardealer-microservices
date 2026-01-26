import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useOklaAnalytics } from './hooks/useOklaAnalytics';
import HomePage from './pages/HomePage';
// Vehicle module pages
import {
  VehicleBrowsePage,
  VehicleDetailPage,
  VehicleComparePage,
  SellYourCarPage,
  VehiclesHomePage,
} from './pages/vehicles';
import VehicleMapViewPage from './pages/vehicles/MapViewPage';
// Marketplace pages (Sprint 1)
import { SearchPage } from './pages/SearchPage';
import { FavoritesPage } from './pages/FavoritesPage';
import { ComparisonPage } from './pages/ComparisonPage';
import { AlertsPage } from './pages/AlertsPage';
// Contact pages (Sprint 2)
import { MyInquiriesPage } from './pages/MyInquiriesPage';
import { ReceivedInquiriesPage } from './pages/ReceivedInquiriesPage';
// Payment pages (Sprint 4 - AZUL Integration)
import { AzulPaymentPage } from './pages/AzulPaymentPage';
import { AzulApprovedPage } from './pages/AzulApprovedPage';
import { AzulDeclinedPage } from './pages/AzulDeclinedPage';
import { AzulCancelledPage } from './pages/AzulCancelledPage';
// Dealer Management pages (Sprint 5)
import DealerLandingPage from './pages/DealerLandingPage';
import DealerPricingPage from './pages/DealerPricingPage';
import DealerRegistrationPage from './pages/DealerRegistrationPage';
import DealerDashboard from './pages/DealerDashboard';
// Inventory Management pages (Sprint 6)
import InventoryManagementPage from './pages/InventoryManagementPage';
import { CSVImportPage, LocationsPage } from './pages/dealer';
// Public Dealer Profile pages (Sprint 7)
import PublicDealerProfilePage from './pages/PublicDealerProfilePage';
import DealerProfileEditorPage from './pages/DealerProfileEditorPage';
// Analytics Dashboard (Sprint 8)
import DealerAnalyticsDashboard from './pages/DealerAnalyticsDashboard';
// Advanced Analytics Dashboard (Sprint 12)
import AdvancedDealerDashboard from './pages/AdvancedDealerDashboard';
// Advanced Analytics Pages (Sprint 12 - Dealer Analytics Service)
import AdvancedAnalyticsDashboard from './pages/dealer/AdvancedAnalyticsDashboard';
import InventoryAnalyticsPage from './pages/dealer/InventoryAnalyticsPage';
import LeadFunnelPage from './pages/dealer/LeadFunnelPage';
import AlertsManagementPage from './pages/dealer/AlertsManagementPage';
import ReportsPage from './pages/dealer/ReportsPage';
// User Behavior & Features (Sprint 10)
import UserBehaviorDashboard from './pages/UserBehaviorDashboard';
import FeatureStoreDashboard from './pages/FeatureStoreDashboard';
// Lead Scoring (Sprint 11)
import LeadsDashboard from './pages/LeadsDashboard';
import LeadDetail from './pages/LeadDetail';
// Review System (Sprint 14)
import SellerReviewsPage from './pages/SellerReviewsPage';
import WriteReviewPage from './pages/WriteReviewPage';
// Chatbot (Sprint 17 - OpenAI + WhatsApp + SignalR)
import ChatWidget from './components/Chatbot/ChatWidget';
// User pages
import { UserDashboardPage, MessagesPage, WishlistPage, ProfilePage } from './pages/user';
// Auth pages
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import ForgotPasswordPage from './pages/auth/ForgotPasswordPage';
import OAuthCallbackPage from './pages/auth/OAuthCallbackPage';
import ResetPasswordPage from './pages/auth/ResetPasswordPage';
import VerifyEmailPage from './pages/auth/VerifyEmailPage';
import EmailVerificationPendingPage from './pages/auth/EmailVerificationPendingPage';
import TwoFactorVerifyPage from './pages/auth/TwoFactorVerifyPage';
import SetPasswordPage from './pages/auth/SetPasswordPage';
// User Security Settings
import SecuritySettingsPage from './pages/user/SecuritySettingsPage';
// Privacy & Data Management (Sprint - ARCO Compliance Ley 172-13)
import PrivacyCenterPage from './pages/user/PrivacyCenterPage';
import DataDownloadPage from './pages/user/DataDownloadPage';
import DeleteAccountPage from './pages/user/DeleteAccountPage';
import MyDataPage from './pages/user/MyDataPage';
// Recently Viewed (Recommendations)
import RecentlyViewedPage from './pages/vehicles/RecentlyViewedPage';
// Media Viewers (360 & Video Tours)
import Media360ViewerPage from './pages/vehicles/Media360ViewerPage';
import VideoTourPage from './pages/vehicles/VideoTourPage';
// Admin pages
import AdminDashboardPage from './pages/admin/AdminDashboardPage';
import PendingApprovalsPage from './pages/admin/PendingApprovalsPage';
import UsersManagementPage from './pages/admin/UsersManagementPage';
import AdminListingsPage from './pages/admin/AdminListingsPage';
import AdminReportsPage from './pages/admin/AdminReportsPage';
import AdminSettingsPage from './pages/admin/AdminSettingsPage';
import CategoriesManagementPage from './pages/admin/CategoriesManagementPage';
import KYCAdminReviewPage from './pages/admin/KYCAdminReviewPage';
// KYC Admin Queue, Watchlist, STR pages (Sprint - KYC Service Compliance)
import KYCAdminQueuePage from './pages/admin/KYCAdminQueuePage';
import WatchlistAdminPage from './pages/admin/WatchlistAdminPage';
import STRReportsPage from './pages/admin/STRReportsPage';
// RBAC - Roles & Permissions Management
import RolesManagementPage from './pages/admin/RolesManagementPage';
import PermissionsManagementPage from './pages/admin/PermissionsManagementPage';
import RoleDetailPage from './pages/admin/RoleDetailPage';
// ML Admin Dashboard (Sprint - ML Models Management)
import MLAdminDashboard from './pages/admin/MLAdminDashboard';
// KYC pages
import KYCVerificationPage from './pages/kyc/KYCVerificationPage';
import KYCStatusPage from './pages/kyc/KYCStatusPage';
import BiometricVerificationPage from './pages/kyc/BiometricVerificationPage';
// Dealer pages
import DealerDashboardPage from './pages/dealer/DealerDashboardPage';
import {
  DealerHomePage,
  DealerInventoryPage,
  DealerAddVehiclePage,
  DealerAnalyticsPage,
  DealerSettingsPage,
  DealerVehicleEditPage,
  DealerEmployeesPage,
  DealerEmployeePermissionsPage,
} from './pages/dealer';
import DealerListingsPage from './pages/dealer/DealerListingsPage';
import DealerSalesPage from './pages/dealer/DealerSalesPage';
import DealerAlertsPage from './pages/dealer/DealerAlertsPage';
import DealerBenchmarksPage from './pages/dealer/DealerBenchmarksPage';
import CRMPage from './pages/dealer/CRMPage';
import AnalyticsPage from './pages/dealer/AnalyticsPage';
import DealerOnboardingPage from './pages/dealer/DealerOnboardingPage';
// New Dealer Onboarding V2 Pages (Azul Integration)
import DealerOnboardingPageV2 from './pages/dealer/DealerOnboardingPageV2';
import DealerEmailVerificationPage from './pages/dealer/DealerEmailVerificationPage';
import DealerDocumentsPage from './pages/dealer/DealerDocumentsPage';
import DealerPaymentSetupPage from './pages/dealer/DealerPaymentSetupPage';
import DealerOnboardingStatusPage from './pages/dealer/DealerOnboardingStatusPage';
import DealerInquiriesPage from './pages/dealer/DealerInquiriesPage';
import CreateSellerPage from './pages/dealer/CreateSellerPage';
import SellerProfilePage from './pages/dealer/SellerProfilePage';
import SellerDashboardPage from './pages/seller/SellerDashboardPage';
import SellerPublicProfilePage from './pages/seller/SellerPublicProfilePage';
import SellerProfileSettingsPage from './pages/seller/SellerProfileSettingsPage';
import DealerPortalLayout from './layouts/DealerPortalLayout';
// Common pages (legal, info, help)
import {
  AboutPage,
  HowItWorksPage,
  PricingPage,
  FAQPage,
  ContactPage,
  HelpCenterPage,
  TermsPage,
  PrivacyPage,
  CookiesPage,
} from './pages/common';
// Layouts and components
import AuthLayout from './layouts/AuthLayout';
import AdminLayout from './layouts/AdminLayout';
import DealerLayout from './layouts/DealerLayout';
import ProtectedRoute from './components/organisms/ProtectedRoute';
// Billing pages
import {
  BillingDashboardPage,
  PlansPage,
  InvoicesPage,
  PaymentsPage,
  PaymentMethodsPage,
  CheckoutPage,
} from './pages/billing';
import './index.css';

// Create a query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      gcTime: 10 * 60 * 1000, // 10 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  // Initialize OKLA Analytics SDK (Sprint 9 - Event Tracking)
  useOklaAnalytics({
    apiUrl: import.meta.env.VITE_API_URL || 'http://localhost:8080',
    autoTrack: true,
    debug: import.meta.env.DEV,
  });

  return (
    <QueryClientProvider client={queryClient}>
      <Router>
        <Routes>
          {/* Public Routes - Vehicle-focused Home */}
          <Route path="/" element={<HomePage />} />

          {/* Vehicle Module Routes */}
          <Route path="/vehicles" element={<VehicleBrowsePage />} />
          <Route path="/vehicles/home" element={<VehiclesHomePage />} />
          <Route path="/vehicles/map" element={<VehicleMapViewPage />} />
          <Route path="/vehicles/:slug" element={<VehicleDetailPage />} />
          <Route path="/vehicles/compare" element={<VehicleComparePage />} />
          <Route path="/browse" element={<VehicleBrowsePage />} />
          <Route path="/compare" element={<VehicleComparePage />} />
          <Route path="/sell-your-car" element={<SellYourCarPage />} />

          {/* Marketplace Routes (Sprint 1) */}
          <Route path="/search" element={<SearchPage />} />
          <Route
            path="/favorites"
            element={
              <ProtectedRoute>
                <FavoritesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/comparison"
            element={
              <ProtectedRoute>
                <ComparisonPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/alerts"
            element={
              <ProtectedRoute>
                <AlertsPage />
              </ProtectedRoute>
            }
          />

          {/* Contact Routes (Sprint 2) */}
          <Route
            path="/my-inquiries"
            element={
              <ProtectedRoute>
                <MyInquiriesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/received-inquiries"
            element={
              <ProtectedRoute>
                <ReceivedInquiriesPage />
              </ProtectedRoute>
            }
          />

          {/* User Routes */}
          <Route path="/wishlist" element={<WishlistPage />} />
          <Route path="/messages" element={<MessagesPage />} />
          <Route path="/dashboard" element={<UserDashboardPage />} />
          <Route path="/profile" element={<ProfilePage />} />

          {/* Billing Routes */}
          <Route path="/billing" element={<BillingDashboardPage />} />
          <Route path="/billing/plans" element={<PlansPage />} />
          <Route path="/billing/invoices" element={<InvoicesPage />} />
          <Route path="/billing/payments" element={<PaymentsPage />} />
          <Route path="/billing/payment-methods" element={<PaymentMethodsPage />} />
          <Route path="/billing/checkout" element={<CheckoutPage />} />

          {/* Information Pages */}
          <Route path="/about" element={<AboutPage />} />
          <Route path="/how-it-works" element={<HowItWorksPage />} />
          <Route path="/pricing" element={<PricingPage />} />
          <Route path="/faq" element={<FAQPage />} />
          <Route path="/contact" element={<ContactPage />} />
          <Route path="/help" element={<HelpCenterPage />} />

          {/* Legal Pages */}
          <Route path="/terms" element={<TermsPage />} />
          <Route path="/privacy" element={<PrivacyPage />} />
          <Route path="/cookies" element={<CookiesPage />} />

          {/* Auth Routes */}
          <Route
            path="/login"
            element={
              <AuthLayout>
                <LoginPage />
              </AuthLayout>
            }
          />
          <Route
            path="/register"
            element={
              <AuthLayout>
                <RegisterPage />
              </AuthLayout>
            }
          />
          <Route
            path="/forgot-password"
            element={
              <AuthLayout>
                <ForgotPasswordPage />
              </AuthLayout>
            }
          />
          <Route path="/auth/callback/:provider" element={<OAuthCallbackPage />} />
          <Route
            path="/reset-password"
            element={
              <AuthLayout>
                <ResetPasswordPage />
              </AuthLayout>
            }
          />
          <Route
            path="/verify-email"
            element={
              <AuthLayout>
                <VerifyEmailPage />
              </AuthLayout>
            }
          />
          <Route
            path="/verify-email-pending"
            element={
              <AuthLayout>
                <EmailVerificationPendingPage />
              </AuthLayout>
            }
          />
          <Route
            path="/verify-2fa"
            element={
              <AuthLayout>
                <TwoFactorVerifyPage />
              </AuthLayout>
            }
          />
          <Route
            path="/auth/set-password"
            element={
              <AuthLayout>
                <SetPasswordPage />
              </AuthLayout>
            }
          />
          <Route
            path="/settings/security"
            element={
              <ProtectedRoute>
                <SecuritySettingsPage />
              </ProtectedRoute>
            }
          />

          {/* Privacy & Data Management Routes (ARCO - Ley 172-13) */}
          <Route
            path="/privacy-center"
            element={
              <ProtectedRoute>
                <PrivacyCenterPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/settings/privacy/my-data"
            element={
              <ProtectedRoute>
                <MyDataPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/settings/privacy/download-my-data"
            element={
              <ProtectedRoute>
                <DataDownloadPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/settings/privacy/delete-account"
            element={
              <ProtectedRoute>
                <DeleteAccountPage />
              </ProtectedRoute>
            }
          />

          {/* Recently Viewed & Recommendations */}
          <Route
            path="/recently-viewed"
            element={
              <ProtectedRoute>
                <RecentlyViewedPage />
              </ProtectedRoute>
            }
          />

          {/* Vehicle Media Viewers (360 & Video Tours) */}
          <Route path="/vehicles/:slug/360" element={<Media360ViewerPage />} />
          <Route path="/vehicles/:slug/video" element={<VideoTourPage />} />

          {/* Payment Routes (Sprint 4 - AZUL) */}
          <Route path="/payment/azul" element={<AzulPaymentPage />} />
          <Route path="/payment/azul/approved" element={<AzulApprovedPage />} />
          <Route path="/payment/azul/declined" element={<AzulDeclinedPage />} />
          <Route path="/payment/azul/cancelled" element={<AzulCancelledPage />} />

          {/* Dealer Management Routes (Sprint 5) */}
          <Route path="/dealer/landing" element={<DealerLandingPage />} />
          <Route path="/dealer/pricing" element={<DealerPricingPage />} />
          <Route path="/dealer/register" element={<DealerRegistrationPage />} />
          <Route
            path="/dealer/dashboard"
            element={
              <ProtectedRoute requireDealer>
                <DealerDashboardPage />
              </ProtectedRoute>
            }
          />

          {/* Analytics Routes (Sprint 8) */}
          <Route
            path="/dealer/analytics"
            element={
              <ProtectedRoute requireDealer>
                <DealerAnalyticsPage />
              </ProtectedRoute>
            }
          />

          {/* Advanced Analytics Routes (Sprint 12) */}
          <Route
            path="/dealer/analytics/advanced"
            element={
              <ProtectedRoute requireDealer>
                <AdvancedDealerDashboard />
              </ProtectedRoute>
            }
          />

          {/* Advanced Analytics - Full Dashboard (DealerAnalyticsService) */}
          <Route
            path="/dealer/analytics/dashboard"
            element={
              <ProtectedRoute requireDealer>
                <AdvancedAnalyticsDashboard />
              </ProtectedRoute>
            }
          />

          {/* Advanced Analytics - Inventory Analysis */}
          <Route
            path="/dealer/analytics/inventory"
            element={
              <ProtectedRoute requireDealer>
                <InventoryAnalyticsPage />
              </ProtectedRoute>
            }
          />

          {/* Advanced Analytics - Lead Funnel */}
          <Route
            path="/dealer/analytics/funnel"
            element={
              <ProtectedRoute requireDealer>
                <LeadFunnelPage />
              </ProtectedRoute>
            }
          />

          {/* Advanced Analytics - Alerts Management */}
          <Route
            path="/dealer/analytics/alerts"
            element={
              <ProtectedRoute requireDealer>
                <AlertsManagementPage />
              </ProtectedRoute>
            }
          />

          {/* Advanced Analytics - Reports */}
          <Route
            path="/dealer/analytics/reports"
            element={
              <ProtectedRoute requireDealer>
                <ReportsPage />
              </ProtectedRoute>
            }
          />

          {/* Dealer Benchmarks - Market Comparison */}
          <Route
            path="/dealer/benchmarks"
            element={
              <ProtectedRoute requireDealer>
                <DealerBenchmarksPage />
              </ProtectedRoute>
            }
          />

          {/* Sales History Route */}
          <Route
            path="/dealer/sales"
            element={
              <ProtectedRoute requireDealer>
                <DealerSalesPage />
              </ProtectedRoute>
            }
          />

          {/* Alerts Route */}
          <Route
            path="/dealer/alerts"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <DealerAlertsPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />

          {/* Inventory Management Routes (Sprint 6) */}
          <Route
            path="/dealer/inventory"
            element={
              <ProtectedRoute requireDealer>
                <DealerInventoryPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/inventory/new"
            element={
              <ProtectedRoute requireDealer>
                <DealerAddVehiclePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/inventory/:id/edit"
            element={
              <ProtectedRoute requireDealer>
                <DealerVehicleEditPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/import"
            element={
              <ProtectedRoute requireDealer>
                <CSVImportPage dealerId="" userId="" />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/locations"
            element={
              <ProtectedRoute requireDealer>
                <LocationsPage dealerId="" />
              </ProtectedRoute>
            }
          />

          {/* Public Dealer Profile Routes (Sprint 7) */}
          <Route path="/dealers/:slug" element={<PublicDealerProfilePage />} />
          <Route
            path="/dealer/profile/edit"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <DealerProfileEditorPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />

          {/* Review System Routes (Sprint 14) */}
          <Route path="/sellers/:sellerId/reviews" element={<SellerReviewsPage />} />
          <Route
            path="/reviews/write/:sellerId"
            element={
              <ProtectedRoute>
                <WriteReviewPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/reviews/write/:sellerId/:vehicleId"
            element={
              <ProtectedRoute>
                <WriteReviewPage />
              </ProtectedRoute>
            }
          />

          {/* Protected Routes */}
          <Route
            path="/profile"
            element={
              <ProtectedRoute>
                <ProfilePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <UserDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/messages"
            element={
              <ProtectedRoute>
                <MessagesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/sell"
            element={
              <ProtectedRoute>
                <SellYourCarPage />
              </ProtectedRoute>
            }
          />

          {/* Dealer Routes - Using DealerPortalLayout (built-in) */}
          <Route path="/dealer/onboarding" element={<DealerOnboardingPage />} />

          {/* New Dealer Onboarding V2 Flow (Azul Integration) */}
          {/* Dealers PAY OKLA for advertising - vehicle sales are external */}
          <Route path="/dealer/onboarding/v2" element={<DealerOnboardingPageV2 />} />
          <Route path="/dealer/onboarding/verify-email" element={<DealerEmailVerificationPage />} />
          <Route path="/dealer/onboarding/documents" element={<DealerDocumentsPage />} />
          <Route path="/dealer/onboarding/payment-setup" element={<DealerPaymentSetupPage />} />
          <Route path="/dealer/onboarding/status" element={<DealerOnboardingStatusPage />} />

          <Route path="/seller/create" element={<CreateSellerPage />} />
          <Route
            path="/seller/profile"
            element={
              <ProtectedRoute>
                <SellerProfilePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/seller/dashboard"
            element={
              <ProtectedRoute>
                <SellerDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/seller/profile/settings"
            element={
              <ProtectedRoute>
                <SellerProfileSettingsPage />
              </ProtectedRoute>
            }
          />
          <Route path="/sellers/:sellerId" element={<SellerPublicProfilePage />} />
          <Route
            path="/dealer"
            element={
              <ProtectedRoute requireDealer>
                <DealerHomePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/listings"
            element={
              <ProtectedRoute requireDealer>
                <DealerListingsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/crm"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <CRMPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/appointments"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <div className="p-6 lg:p-8">
                    <h1 className="text-2xl font-bold text-gray-900 mb-2">Citas</h1>
                    <p className="text-gray-500">
                      Gestión de citas con clientes. Página en desarrollo...
                    </p>
                  </div>
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/billing"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <BillingDashboardPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/plans"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <PlansPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/invoices"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <InvoicesPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/payments"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <PaymentsPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/payment-methods"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <PaymentMethodsPage />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/settings"
            element={
              <ProtectedRoute requireDealer>
                <DealerSettingsPage />
              </ProtectedRoute>
            }
          />

          {/* Dealer Employees Routes (UserService UI) */}
          <Route
            path="/dealer/employees"
            element={
              <ProtectedRoute requireDealer>
                <DealerEmployeesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dealer/employees/:employeeId/permissions"
            element={
              <ProtectedRoute requireDealer>
                <DealerEmployeePermissionsPage />
              </ProtectedRoute>
            }
          />

          {/* Admin Routes */}
          <Route
            path="/admin"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <AdminDashboardPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/pending"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <PendingApprovalsPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/users"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <UsersManagementPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/listings"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <AdminListingsPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/reports"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <AdminReportsPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/settings"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <AdminSettingsPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/categories"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <CategoriesManagementPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />

          {/* RBAC - Roles & Permissions Management */}
          <Route
            path="/admin/roles"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <RolesManagementPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/roles/:id"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <RoleDetailPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/permissions"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <PermissionsManagementPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />

          {/* KYC Routes */}
          <Route
            path="/kyc/verify"
            element={
              <ProtectedRoute>
                <KYCVerificationPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/kyc/biometric-verify"
            element={
              <ProtectedRoute>
                <BiometricVerificationPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/kyc/status"
            element={
              <ProtectedRoute>
                <KYCStatusPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/kyc"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <KYCAdminReviewPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/kyc/:profileId"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <KYCAdminReviewPage />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          {/* KYC Admin Queue - Cola de verificación */}
          <Route
            path="/admin/kyc/queue"
            element={
              <ProtectedRoute requireAdmin>
                <KYCAdminQueuePage />
              </ProtectedRoute>
            }
          />
          {/* Watchlist Admin - PEPs, Sanciones (Ley 155-17) */}
          <Route
            path="/admin/compliance/watchlist"
            element={
              <ProtectedRoute requireAdmin>
                <WatchlistAdminPage />
              </ProtectedRoute>
            }
          />
          {/* STR Reports - Reportes Transacciones Sospechosas (UAF) */}
          <Route
            path="/admin/compliance/str"
            element={
              <ProtectedRoute requireAdmin>
                <STRReportsPage />
              </ProtectedRoute>
            }
          />

          {/* Sprint 10 - User Behavior & Features */}
          <Route
            path="/admin/user-behavior"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <UserBehaviorDashboard />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/user-behavior/:userId"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <UserBehaviorDashboard />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/feature-store"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <FeatureStoreDashboard />
                </AdminLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/feature-store/:entityType/:entityId"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <FeatureStoreDashboard />
                </AdminLayout>
              </ProtectedRoute>
            }
          />

          {/* ML Models Administration */}
          <Route
            path="/admin/ml/models"
            element={
              <ProtectedRoute requireAdmin>
                <AdminLayout>
                  <MLAdminDashboard />
                </AdminLayout>
              </ProtectedRoute>
            }
          />

          {/* Sprint 11 - Lead Scoring */}
          <Route
            path="/dealer/leads/:leadId"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <LeadDetail />
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />

          {/* Sprint 17 - Chatbot Conversations (Dealers only) */}
          <Route
            path="/dealer/conversations"
            element={
              <ProtectedRoute requireDealer>
                <DealerPortalLayout>
                  <div className="p-6 lg:p-8">
                    <h1 className="text-2xl font-bold text-gray-900 mb-2">Conversaciones</h1>
                    <p className="text-gray-500">
                      Gestión de conversaciones del chatbot. En desarrollo...
                    </p>
                  </div>
                </DealerPortalLayout>
              </ProtectedRoute>
            }
          />

          {/* Dealer Inquiries */}
          <Route
            path="/dealer/inquiries"
            element={
              <ProtectedRoute requireDealer>
                <DealerInquiriesPage />
              </ProtectedRoute>
            }
          />

          {/* 404 Page */}
          <Route
            path="*"
            element={
              <div className="min-h-screen flex items-center justify-center">
                <div className="text-center">
                  <h1 className="text-4xl font-bold text-gray-900 mb-4">404</h1>
                  <p className="text-gray-600">Page not found</p>
                  <a href="/" className="btn btn-primary mt-4">
                    Go back home
                  </a>
                </div>
              </div>
            }
          />
        </Routes>

        {/* Global Chat Widget (Sprint 17 - OpenAI + WhatsApp) */}
        <ChatWidget />
      </Router>
    </QueryClientProvider>
  );
}

export default App;
