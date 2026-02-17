-- =====================================================
-- OKLA QA Environment - Crear Bases de Datos
-- =====================================================
-- IMPORTANTE: Solo crea las bases de datos vacías.
-- Las TABLAS son creadas automáticamente por Entity Framework
-- cuando cada microservicio inicia y ejecuta sus migraciones.
-- =====================================================
-- TODOS LOS MICROSERVICIOS (28 bases de datos)
-- =====================================================

SELECT 'Creating databases for OKLA QA environment...' AS status;

-- ============================
-- SERVICIOS CRÍTICOS
-- ============================

-- ErrorService (debe existir primero para logging)
CREATE DATABASE errorservice;

-- AuthService
CREATE DATABASE authservice;

-- VehiclesSaleService
CREATE DATABASE vehiclessaleservice;

-- MediaService
CREATE DATABASE mediaservice;

-- ============================
-- SERVICIOS IMPORTANTES
-- ============================

-- RoleService
CREATE DATABASE roleservice;

-- UserService  
CREATE DATABASE userservice;

-- ContactService
CREATE DATABASE contactservice;

-- NotificationService
CREATE DATABASE notificationservice;

-- AdminService
CREATE DATABASE adminservice;

-- BillingService
CREATE DATABASE billingservice;

-- EventTrackingService
CREATE DATABASE eventtrackingservice;

-- ============================
-- SERVICIOS DE NEGOCIO
-- ============================

-- DealerManagementService
CREATE DATABASE dealermanagementservice;

-- DealerAnalyticsService
CREATE DATABASE dealeranalyticsservice;

-- CRMService
CREATE DATABASE crmservice;

-- SearchService
CREATE DATABASE searchservice;

-- AlertService
CREATE DATABASE alertservice;

-- MaintenanceService
CREATE DATABASE maintenanceservice;

-- ComparisonService
CREATE DATABASE comparisonservice;

-- ReviewService
CREATE DATABASE reviewservice;

-- FinanceService
CREATE DATABASE financeservice;

-- ============================
-- SERVICIOS DE SOPORTE
-- ============================

-- FeatureToggleService
CREATE DATABASE featuretoggleservice;

-- ReportsService
CREATE DATABASE reportsservice;

-- SchedulerService
CREATE DATABASE schedulerservice;

-- AuditService
CREATE DATABASE auditservice;

-- KYCService
CREATE DATABASE kycservice;

-- InventoryManagementService
CREATE DATABASE inventorymanagementservice;

-- AppointmentService
CREATE DATABASE appointmentservice;

-- ChatbotService
CREATE DATABASE chatbotservice;

-- StaffService
CREATE DATABASE staffservice;

SELECT 'All 29 databases created successfully!' AS status;
