import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import HomePage from './pages/HomePage';
// Vehicle module pages
import { 
  VehicleBrowsePage, 
  VehicleDetailPage, 
  VehicleComparePage, 
  SellYourCarPage,
  VehiclesHomePage
} from './pages/vehicles';
import VehicleMapViewPage from './pages/vehicles/MapViewPage';
// User pages
import { 
  UserDashboardPage, 
  MessagesPage, 
  WishlistPage, 
  ProfilePage 
} from './pages/user';
// Auth pages
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import ForgotPasswordPage from './pages/auth/ForgotPasswordPage';
import OAuthCallbackPage from './pages/auth/OAuthCallbackPage';
// Admin pages
import AdminDashboardPage from './pages/admin/AdminDashboardPage';
import PendingApprovalsPage from './pages/admin/PendingApprovalsPage';
import UsersManagementPage from './pages/admin/UsersManagementPage';
import AdminListingsPage from './pages/admin/AdminListingsPage';
import AdminReportsPage from './pages/admin/AdminReportsPage';
import AdminSettingsPage from './pages/admin/AdminSettingsPage';
import CategoriesManagementPage from './pages/admin/CategoriesManagementPage';
// Dealer pages
import DealerDashboardPage from './pages/dealer/DealerDashboardPage';
import DealerListingsPage from './pages/dealer/DealerListingsPage';
import CRMPage from './pages/dealer/CRMPage';
import AnalyticsPage from './pages/dealer/AnalyticsPage';
import DealerOnboardingPage from './pages/dealer/DealerOnboardingPage';
import CreateSellerPage from './pages/dealer/CreateSellerPage';
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
  CookiesPage 
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
          <Route path="/login" element={
            <AuthLayout>
              <LoginPage />
            </AuthLayout>
          } />
          <Route path="/register" element={
            <AuthLayout>
              <RegisterPage />
            </AuthLayout>
          } />
          <Route path="/forgot-password" element={
            <AuthLayout>
              <ForgotPasswordPage />
            </AuthLayout>
          } />
          <Route path="/auth/callback/:provider" element={<OAuthCallbackPage />} />
          
          {/* Protected Routes */}
          <Route path="/profile" element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          } />
          <Route path="/dashboard" element={
            <ProtectedRoute>
              <UserDashboardPage />
            </ProtectedRoute>
          } />
          <Route path="/messages" element={
            <ProtectedRoute>
              <MessagesPage />
            </ProtectedRoute>
          } />
          <Route path="/sell" element={
            <ProtectedRoute>
              <SellYourCarPage />
            </ProtectedRoute>
          } />
          
          {/* Dealer Routes */}
          <Route path="/dealer/onboarding" element={<DealerOnboardingPage />} />
          <Route path="/seller/create" element={<CreateSellerPage />} />
          <Route path="/dealer" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <DealerDashboardPage />
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/listings" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <DealerListingsPage />
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/crm" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <CRMPage />
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/appointments" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <div className="p-6">
                  <h1 className="text-2xl font-bold">Citas</h1>
                  <p className="text-gray-500 mt-2">Gestión de citas con clientes. Página en desarrollo...</p>
                </div>
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/analytics" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <AnalyticsPage />
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/billing" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <BillingDashboardPage />
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/plans" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <PlansPage />
              </DealerLayout>
            </ProtectedRoute>
          } />
          <Route path="/dealer/settings" element={
            <ProtectedRoute requireDealer>
              <DealerLayout>
                <div className="p-6">
                  <h1 className="text-2xl font-bold">Configuración</h1>
                  <p className="text-gray-500 mt-2">Configuración de cuenta. Página en desarrollo...</p>
                </div>
              </DealerLayout>
            </ProtectedRoute>
          } />
          
          {/* Admin Routes */}
          <Route path="/admin" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <AdminDashboardPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/pending" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <PendingApprovalsPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/users" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <UsersManagementPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/listings" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <AdminListingsPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/reports" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <AdminReportsPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/settings" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <AdminSettingsPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          <Route path="/admin/categories" element={
            <ProtectedRoute requireAdmin>
              <AdminLayout>
                <CategoriesManagementPage />
              </AdminLayout>
            </ProtectedRoute>
          } />
          
          {/* 404 Page */}
          <Route path="*" element={
            <div className="min-h-screen flex items-center justify-center">
              <div className="text-center">
                <h1 className="text-4xl font-bold text-gray-900 mb-4">404</h1>
                <p className="text-gray-600">Page not found</p>
                <a href="/" className="btn btn-primary mt-4">Go back home</a>
              </div>
            </div>
          } />
        </Routes>
      </Router>
    </QueryClientProvider>
  );
}

export default App;
