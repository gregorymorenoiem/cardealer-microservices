--
-- PostgreSQL database dump
--

\restrict KcxE22WeyaAYajCIFUDs1EE5jl3dl3UJ6XjRFatXazjbZpjXPkHwnoyTAIKWVx9

-- Dumped from database version 16.11 (Debian 16.11-1.pgdg13+1)
-- Dumped by pg_dump version 16.11 (Debian 16.11-1.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: DealerEmployeeInvitations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."DealerEmployeeInvitations" (
    "Id" uuid NOT NULL,
    "DealerId" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "DealerRole" integer NOT NULL,
    "Permissions" character varying(2000) DEFAULT '[]'::character varying NOT NULL,
    "InvitedBy" uuid NOT NULL,
    "Status" integer DEFAULT 0 NOT NULL,
    "InvitationDate" timestamp with time zone DEFAULT now() NOT NULL,
    "ExpirationDate" timestamp with time zone NOT NULL,
    "AcceptedDate" timestamp with time zone,
    "Token" character varying(256) NOT NULL
);


ALTER TABLE public."DealerEmployeeInvitations" OWNER TO postgres;

--
-- Name: DealerEmployees; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."DealerEmployees" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "DealerId" uuid NOT NULL,
    "DealerRole" integer NOT NULL,
    "Permissions" character varying(2000) DEFAULT '[]'::character varying NOT NULL,
    "InvitedBy" uuid NOT NULL,
    "Status" integer DEFAULT 0 NOT NULL,
    "InvitationDate" timestamp with time zone DEFAULT now() NOT NULL,
    "ActivationDate" timestamp with time zone,
    "Notes" character varying(500),
    "UserId1" uuid NOT NULL
);


ALTER TABLE public."DealerEmployees" OWNER TO postgres;

--
-- Name: DealerModuleSubscriptions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."DealerModuleSubscriptions" (
    "Id" uuid NOT NULL,
    "DealerId" uuid NOT NULL,
    "ModuleAddonId" uuid NOT NULL,
    "Status" integer NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone,
    "TrialEndDate" timestamp with time zone,
    "MonthlyPrice" numeric NOT NULL,
    "IsYearlyBilling" boolean NOT NULL,
    "StripeSubscriptionItemId" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid
);


ALTER TABLE public."DealerModuleSubscriptions" OWNER TO postgres;

--
-- Name: DealerSubscriptions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."DealerSubscriptions" (
    "Id" uuid NOT NULL,
    "DealerId" uuid NOT NULL,
    "Plan" integer NOT NULL,
    "Status" integer NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone,
    "TrialEndDate" timestamp with time zone,
    "Features" text NOT NULL,
    "CurrentListings" integer NOT NULL,
    "ListingsThisMonth" integer NOT NULL,
    "FeaturedUsed" integer NOT NULL,
    "StripeSubscriptionId" text,
    "StripeCustomerId" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid
);


ALTER TABLE public."DealerSubscriptions" OWNER TO postgres;

--
-- Name: Dealers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Dealers" (
    "Id" uuid NOT NULL,
    "BusinessName" character varying(200) NOT NULL,
    "TradeName" character varying(200),
    "Description" character varying(2000),
    "DealerType" integer NOT NULL,
    "Email" character varying(256) NOT NULL,
    "Phone" character varying(30) NOT NULL,
    "WhatsApp" character varying(30),
    "Website" character varying(500),
    "Address" character varying(500) NOT NULL,
    "City" character varying(100) NOT NULL,
    "State" character varying(100) NOT NULL,
    "ZipCode" character varying(20),
    "Country" character varying(10) DEFAULT 'DO'::character varying NOT NULL,
    "Latitude" double precision,
    "Longitude" double precision,
    "LogoUrl" character varying(1000),
    "BannerUrl" character varying(1000),
    "PrimaryColor" character varying(10),
    "BusinessRegistrationNumber" character varying(50),
    "TaxId" character varying(50),
    "DealerLicenseNumber" character varying(100),
    "LicenseExpiryDate" timestamp with time zone,
    "BusinessLicenseDocumentUrl" character varying(1000),
    "VerificationStatus" integer DEFAULT 0 NOT NULL,
    "VerifiedAt" timestamp with time zone,
    "VerifiedByUserId" uuid,
    "VerificationNotes" character varying(1000),
    "RejectionReason" character varying(1000),
    "TotalListings" integer NOT NULL,
    "ActiveListings" integer NOT NULL,
    "TotalSales" integer NOT NULL,
    "AverageRating" numeric(3,2) NOT NULL,
    "TotalReviews" integer NOT NULL,
    "ResponseTimeMinutes" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "AcceptsFinancing" boolean NOT NULL,
    "AcceptsTradeIn" boolean NOT NULL,
    "OffersWarranty" boolean NOT NULL,
    "HomeDelivery" boolean NOT NULL,
    "BusinessHours" jsonb,
    "SocialMediaLinks" jsonb,
    "SubscriptionId" uuid,
    "MaxListings" integer NOT NULL,
    "IsFeatured" boolean NOT NULL,
    "FeaturedUntil" timestamp with time zone,
    "OwnerUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone
);


ALTER TABLE public."Dealers" OWNER TO postgres;

--
-- Name: IdentityDocuments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."IdentityDocuments" (
    "Id" uuid NOT NULL,
    "SellerProfileId" uuid NOT NULL,
    "DocumentType" integer NOT NULL,
    "DocumentNumber" character varying(50) NOT NULL,
    "IssuingCountry" character varying(10),
    "IssueDate" timestamp with time zone,
    "ExpiryDate" timestamp with time zone,
    "FrontImageUrl" character varying(1000) NOT NULL,
    "BackImageUrl" character varying(1000),
    "SelfieWithDocumentUrl" character varying(1000),
    "ExtractedFullName" character varying(200),
    "ExtractedDateOfBirth" timestamp with time zone,
    "ExtractedAddress" character varying(500),
    "Status" integer DEFAULT 0 NOT NULL,
    "VerifiedAt" timestamp with time zone,
    "VerifiedByUserId" uuid,
    "VerificationNotes" character varying(1000),
    "RejectionReason" character varying(1000),
    "IsEncrypted" boolean NOT NULL,
    "ViewCount" integer NOT NULL,
    "LastViewedAt" timestamp with time zone,
    "LastViewedByUserId" uuid,
    "UploadedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL
);


ALTER TABLE public."IdentityDocuments" OWNER TO postgres;

--
-- Name: ModuleAddons; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ModuleAddons" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    "LongDescription" text NOT NULL,
    "Category" integer NOT NULL,
    "MonthlyPrice" numeric NOT NULL,
    "YearlyPrice" numeric NOT NULL,
    "HasFreeTrial" boolean NOT NULL,
    "TrialDays" integer NOT NULL,
    "Features" text NOT NULL,
    "RequiredModules" text NOT NULL,
    "IncludedInPlans" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "IsVisible" boolean NOT NULL,
    "SortOrder" integer NOT NULL,
    "IconUrl" text,
    "ThumbnailUrl" text,
    "DocumentationUrl" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" uuid
);


ALTER TABLE public."ModuleAddons" OWNER TO postgres;

--
-- Name: PlatformEmployeeInvitations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PlatformEmployeeInvitations" (
    "Id" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "PlatformRole" integer NOT NULL,
    "Permissions" character varying(2000) DEFAULT '[]'::character varying NOT NULL,
    "InvitedBy" uuid NOT NULL,
    "Status" integer DEFAULT 0 NOT NULL,
    "InvitationDate" timestamp with time zone DEFAULT now() NOT NULL,
    "ExpirationDate" timestamp with time zone NOT NULL,
    "AcceptedDate" timestamp with time zone,
    "Token" character varying(256) NOT NULL
);


ALTER TABLE public."PlatformEmployeeInvitations" OWNER TO postgres;

--
-- Name: PlatformEmployees; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."PlatformEmployees" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "PlatformRole" integer NOT NULL,
    "Permissions" character varying(2000) DEFAULT '[]'::character varying NOT NULL,
    "AssignedBy" uuid NOT NULL,
    "Status" integer DEFAULT 1 NOT NULL,
    "HireDate" timestamp with time zone DEFAULT now() NOT NULL,
    "Department" character varying(100),
    "Notes" character varying(500),
    "UserId1" uuid NOT NULL
);


ALTER TABLE public."PlatformEmployees" OWNER TO postgres;

--
-- Name: SellerProfiles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SellerProfiles" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "DateOfBirth" timestamp with time zone,
    "Nationality" character varying(50),
    "Phone" character varying(30) NOT NULL,
    "AlternatePhone" character varying(30),
    "WhatsApp" character varying(30),
    "Email" character varying(256) NOT NULL,
    "Address" character varying(500) NOT NULL,
    "City" character varying(100) NOT NULL,
    "State" character varying(100) NOT NULL,
    "ZipCode" character varying(20),
    "Country" character varying(10) DEFAULT 'DO'::character varying NOT NULL,
    "Latitude" double precision,
    "Longitude" double precision,
    "VerificationStatus" integer DEFAULT 0 NOT NULL,
    "VerifiedAt" timestamp with time zone,
    "VerifiedByUserId" uuid,
    "VerificationNotes" character varying(1000),
    "RejectionReason" character varying(1000),
    "VerificationExpiresAt" timestamp with time zone,
    "TotalListings" integer NOT NULL,
    "ActiveListings" integer NOT NULL,
    "TotalSales" integer NOT NULL,
    "AverageRating" numeric(3,2) NOT NULL,
    "TotalReviews" integer NOT NULL,
    "ResponseTimeMinutes" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "AcceptsOffers" boolean NOT NULL,
    "ShowPhone" boolean NOT NULL,
    "ShowLocation" boolean NOT NULL,
    "PreferredContactMethod" character varying(20),
    "MaxActiveListings" integer NOT NULL,
    "CanSellHighValue" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL
);


ALTER TABLE public."SellerProfiles" OWNER TO postgres;

--
-- Name: SubscriptionHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."SubscriptionHistory" (
    "Id" uuid NOT NULL,
    "DealerSubscriptionId" uuid NOT NULL,
    "FromPlan" integer NOT NULL,
    "ToPlan" integer NOT NULL,
    "Reason" text NOT NULL,
    "ChangedAt" timestamp with time zone NOT NULL,
    "ChangedBy" uuid,
    "Notes" text
);


ALTER TABLE public."SubscriptionHistory" OWNER TO postgres;

--
-- Name: UserRoles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."UserRoles" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    "AssignedBy" character varying(100) DEFAULT 'system'::character varying NOT NULL,
    "RevokedAt" timestamp with time zone,
    "RevokedBy" character varying(100),
    "IsActive" boolean DEFAULT true NOT NULL
);


ALTER TABLE public."UserRoles" OWNER TO postgres;

--
-- Name: Users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PasswordHash" character varying(500) NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "PhoneNumber" character varying(20) NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "EmailConfirmed" boolean DEFAULT false NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "LastLoginAt" timestamp with time zone,
    "AccountType" character varying(50) DEFAULT 'Individual'::character varying NOT NULL,
    "CreatedBy" uuid,
    "DealerId" uuid,
    "DealerPermissions" character varying(2000) DEFAULT '[]'::character varying,
    "DealerRole" integer DEFAULT 0,
    "EmployerUserId" uuid,
    "PlatformPermissions" character varying(2000) DEFAULT '[]'::character varying,
    "PlatformRole" integer DEFAULT 0
);


ALTER TABLE public."Users" OWNER TO postgres;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Name: error_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.error_logs (
    id uuid NOT NULL,
    service_name character varying(100) NOT NULL,
    exception_type character varying(200) NOT NULL,
    message text NOT NULL,
    stack_trace text,
    occurred_at timestamp with time zone NOT NULL,
    endpoint character varying(500),
    http_method character varying(10),
    status_code integer,
    user_id character varying(100),
    metadata jsonb NOT NULL
);


ALTER TABLE public.error_logs OWNER TO postgres;

--
-- Data for Name: DealerEmployeeInvitations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DealerEmployeeInvitations" ("Id", "DealerId", "Email", "DealerRole", "Permissions", "InvitedBy", "Status", "InvitationDate", "ExpirationDate", "AcceptedDate", "Token") FROM stdin;
\.


--
-- Data for Name: DealerEmployees; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DealerEmployees" ("Id", "UserId", "DealerId", "DealerRole", "Permissions", "InvitedBy", "Status", "InvitationDate", "ActivationDate", "Notes", "UserId1") FROM stdin;
\.


--
-- Data for Name: DealerModuleSubscriptions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DealerModuleSubscriptions" ("Id", "DealerId", "ModuleAddonId", "Status", "StartDate", "EndDate", "TrialEndDate", "MonthlyPrice", "IsYearlyBilling", "StripeSubscriptionItemId", "CreatedAt", "UpdatedAt", "CreatedBy") FROM stdin;
\.


--
-- Data for Name: DealerSubscriptions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."DealerSubscriptions" ("Id", "DealerId", "Plan", "Status", "StartDate", "EndDate", "TrialEndDate", "Features", "CurrentListings", "ListingsThisMonth", "FeaturedUsed", "StripeSubscriptionId", "StripeCustomerId", "CreatedAt", "UpdatedAt", "CreatedBy") FROM stdin;
c789ce2d-b850-4372-af7a-f4cdd1e3b2d6	00000000-0000-0000-0000-000000000000	0	0	2026-01-06 19:26:10.733994+00	\N	\N	{}	0	0	0	\N	\N	2026-01-06 19:26:10.734036+00	\N	\N
\.


--
-- Data for Name: Dealers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Dealers" ("Id", "BusinessName", "TradeName", "Description", "DealerType", "Email", "Phone", "WhatsApp", "Website", "Address", "City", "State", "ZipCode", "Country", "Latitude", "Longitude", "LogoUrl", "BannerUrl", "PrimaryColor", "BusinessRegistrationNumber", "TaxId", "DealerLicenseNumber", "LicenseExpiryDate", "BusinessLicenseDocumentUrl", "VerificationStatus", "VerifiedAt", "VerifiedByUserId", "VerificationNotes", "RejectionReason", "TotalListings", "ActiveListings", "TotalSales", "AverageRating", "TotalReviews", "ResponseTimeMinutes", "IsActive", "AcceptsFinancing", "AcceptsTradeIn", "OffersWarranty", "HomeDelivery", "BusinessHours", "SocialMediaLinks", "SubscriptionId", "MaxListings", "IsFeatured", "FeaturedUntil", "OwnerUserId", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt") FROM stdin;
d0000001-0001-0001-0001-000000000001	Autos Max RD	Autos Max	Concesionario líder en vehículos nuevos y usados de alta calidad	1	ventas@autosmax.com.do	809-555-0101	809-555-0101	https://autosmax.com.do	Av. Abraham Lincoln 1001, Piantini	Santo Domingo	Distrito Nacional	10127	DO	18.4719	-69.9371	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	25	20	150	4.80	85	15	t	t	t	t	t	\N	\N	\N	100	t	\N	d0000001-0001-0001-0001-000000000001	2026-01-05 03:34:55.283664+00	\N	f	\N
d0000002-0002-0002-0002-000000000002	Caribe Motor	Caribe Motor	Especialistas en vehículos importados desde USA y Europa	1	info@caribemotor.com.do	809-555-0102	809-555-0102	https://caribemotor.com.do	Av. Winston Churchill 1050, Naco	Santo Domingo	Distrito Nacional	10122	DO	18.465	-69.9411	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	35	28	200	4.70	120	20	t	t	t	t	f	\N	\N	\N	150	t	\N	d0000002-0002-0002-0002-000000000002	2026-01-05 03:34:55.283664+00	\N	f	\N
d0000003-0003-0003-0003-000000000003	Toyota República Dominicana	Toyota RD	Distribuidor oficial Toyota con servicio premium	0	ventas@toyota.com.do	809-555-0103	809-555-0103	https://toyota.com.do	Av. 27 de Febrero 500, La Julia	Santo Domingo	Distrito Nacional	10135	DO	18.4633	-69.925	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	50	45	320	4.90	200	10	t	t	f	t	t	\N	\N	\N	200	t	\N	d0000003-0003-0003-0003-000000000003	2026-01-05 03:34:55.283664+00	\N	f	\N
d0000004-0004-0004-0004-000000000004	Honda Central	Honda Central	Tu distribuidor Honda de confianza en RD	0	ventas@hondacentral.com.do	809-555-0104	809-555-0104	https://hondacentral.com.do	Av. John F. Kennedy 89, Los Prados	Santo Domingo	Distrito Nacional	10119	DO	18.4822	-69.9267	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	40	35	280	4.80	175	12	t	t	t	t	t	\N	\N	\N	150	t	\N	d0000004-0004-0004-0004-000000000004	2026-01-05 03:34:55.283664+00	\N	f	\N
d0000005-0005-0005-0005-000000000005	BMW Exclusive RD	BMW Exclusive	Experiencia premium en vehículos BMW nuevos y certificados	0	sales@bmwexclusive.com.do	809-555-0105	809-555-0105	https://bmwexclusive.com.do	Av. Sarasota 25, Bella Vista	Santo Domingo	Distrito Nacional	10114	DO	18.4555	-69.9356	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	20	18	80	4.90	45	8	t	t	f	t	t	\N	\N	\N	75	t	\N	d0000005-0005-0005-0005-000000000005	2026-01-05 03:34:55.283664+00	\N	f	\N
d0000006-0006-0006-0006-000000000006	Mercedes-Benz Dominicana	Mercedes-Benz	Distribuidor autorizado Mercedes-Benz	0	info@mercedes.com.do	809-555-0106	809-555-0106	https://mercedes.com.do	Av. Lope de Vega 55, Serrallés	Santo Domingo	Distrito Nacional	10132	DO	18.4666	-69.9289	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	30	25	95	4.90	60	10	t	t	f	t	t	\N	\N	\N	100	t	\N	d0000006-0006-0006-0006-000000000006	2026-01-05 03:35:26.470407+00	\N	f	\N
d0000007-0007-0007-0007-000000000007	Ford Premium RD	Ford Premium	Concesionario Ford con inventario completo de trucks y SUVs	1	ventas@fordpremium.com.do	809-555-0107	809-555-0107	https://fordpremium.com.do	Autopista Duarte Km 9.5, Los Alcarrizos	Santo Domingo Oeste	Santo Domingo	10902	DO	18.5167	-70.0167	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	45	38	250	4.60	130	25	t	t	t	t	f	\N	\N	\N	150	f	\N	d0000007-0007-0007-0007-000000000007	2026-01-05 03:35:26.470407+00	\N	f	\N
d0000008-0008-0008-0008-000000000008	Chevrolet Zone RD	Chevrolet Zone	Todo Chevrolet en un solo lugar	1	info@chevroletzone.com.do	809-555-0108	809-555-0108	https://chevroletzone.com.do	Av. Charles de Gaulle 300, Los Mina	Santo Domingo Este	Santo Domingo	11506	DO	18.4897	-69.8611	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	38	30	180	4.50	95	30	t	t	t	t	t	\N	\N	\N	120	f	\N	d0000008-0008-0008-0008-000000000008	2026-01-05 03:35:26.470407+00	\N	f	\N
d0000009-0009-0009-0009-000000000009	Hyundai Santo Domingo	Hyundai SD	Distribuidor oficial Hyundai	0	ventas@hyundaisd.com.do	809-555-0109	809-555-0109	https://hyundaisd.com.do	Av. Independencia 1500, Gazcue	Santo Domingo	Distrito Nacional	10104	DO	18.4686	-69.9139	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	42	35	220	4.70	140	15	t	t	t	t	t	\N	\N	\N	150	t	\N	d0000009-0009-0009-0009-000000000009	2026-01-05 03:35:26.470407+00	\N	f	\N
d0000010-0010-0010-0010-000000000010	Kia Motors RD	Kia Motors	Vehículos Kia con garantía extendida	0	info@kiamotors.com.do	809-555-0110	809-555-0110	https://kiamotors.com.do	Av. Máximo Gómez 150, Villa Consuelo	Santo Domingo	Distrito Nacional	10206	DO	18.4778	-69.9	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	35	28	190	4.60	110	18	t	t	t	t	f	\N	\N	\N	120	f	\N	d0000010-0010-0010-0010-000000000010	2026-01-05 03:35:26.470407+00	\N	f	\N
d0000011-0011-0011-0011-000000000011	Nissan Plaza	Nissan Plaza	Todos los modelos Nissan disponibles	0	ventas@nissanplaza.com.do	809-555-0111	809-555-0111	https://nissanplaza.com.do	Av. Bolívar 201, Ens. La Fe	Santo Domingo	Distrito Nacional	10108	DO	18.4594	-69.915	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	40	32	210	4.70	125	20	t	t	t	t	t	\N	\N	\N	130	t	\N	d0000011-0011-0011-0011-000000000011	2026-01-05 03:35:52.658044+00	\N	f	\N
d0000012-0012-0012-0012-000000000012	Audi Select RD	Audi Select	Vehículos Audi nuevos y certificados	0	info@audiselect.com.do	809-555-0112	809-555-0112	https://audiselect.com.do	Av. Gustavo Mejía Ricart 120, Evaristo Morales	Santo Domingo	Distrito Nacional	10144	DO	18.475	-69.9389	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	18	15	55	4.80	35	12	t	t	f	t	t	\N	\N	\N	60	t	\N	d0000012-0012-0012-0012-000000000012	2026-01-05 03:35:52.658044+00	\N	f	\N
d0000013-0013-0013-0013-000000000013	Tesla Caribe	Tesla Caribe	Vehículos eléctricos Tesla en el Caribe	0	sales@teslacaribe.com.do	809-555-0113	809-555-0113	https://teslacaribe.com.do	Blue Mall, Av. Winston Churchill, Piantini	Santo Domingo	Distrito Nacional	10127	DO	18.4675	-69.9419	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	12	10	40	5.00	25	5	t	f	f	t	t	\N	\N	\N	50	t	\N	d0000013-0013-0013-0013-000000000013	2026-01-05 03:35:52.658044+00	\N	f	\N
d0000014-0014-0014-0014-000000000014	Jeep Trails RD	Jeep Trails	Especialistas en Jeep y vehículos 4x4	1	ventas@jeeptrails.com.do	809-555-0114	809-555-0114	https://jeeptrails.com.do	Av. San Martín 400, Ens. Paraíso	Santo Domingo	Distrito Nacional	10202	DO	18.4528	-69.9139	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	28	22	120	4.60	75	25	t	t	t	t	f	\N	\N	\N	100	f	\N	d0000014-0014-0014-0014-000000000014	2026-01-05 03:35:52.658044+00	\N	f	\N
d0000015-0015-0015-0015-000000000015	Lexus Elite RD	Lexus Elite	Lujo y confort japonés en RD	0	info@lexuselite.com.do	809-555-0115	809-555-0115	https://lexuselite.com.do	Av. Tiradentes 50, Naco	Santo Domingo	Distrito Nacional	10122	DO	18.4694	-69.9333	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	15	12	45	4.90	30	10	t	t	f	t	t	\N	\N	\N	50	t	\N	d0000015-0015-0015-0015-000000000015	2026-01-05 03:35:52.658044+00	\N	f	\N
d0000016-0016-0016-0016-000000000016	Porsche Caribe	Porsche Caribe	Deportivos Porsche para verdaderos conocedores	0	sales@porschecaribe.com.do	809-555-0116	809-555-0116	https://porschecaribe.com.do	Av. Abraham Lincoln 900, Piantini	Santo Domingo	Distrito Nacional	10127	DO	18.462	-69.945	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	10	8	30	4.90	20	8	t	t	f	t	t	\N	\N	\N	40	t	\N	d0000016-0016-0016-0016-000000000016	2026-01-05 03:37:35.521145+00	\N	f	\N
d0000017-0017-0017-0017-000000000017	Autos Lujosos RD	Autos Lujosos	Colección exclusiva de autos de lujo usados	1	ventas@autoslujosos.com.do	809-555-0117	809-555-0117	https://autoslujosos.com.do	Av. Independencia 1200, Villa Juana	Santo Domingo	Distrito Nacional	10105	DO	18.4778	-69.9211	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	35	28	180	4.40	95	30	t	t	t	t	t	\N	\N	\N	120	f	\N	d0000017-0017-0017-0017-000000000017	2026-01-05 03:37:35.521145+00	\N	f	\N
d0000018-0018-0018-0018-000000000018	AutoMarket SD	AutoMarket	El mercado de autos más grande de Santo Domingo	1	info@automarket.com.do	809-555-0118	809-555-0118	https://automarket.com.do	Autopista Duarte Km 15, Manoguayabo	Santo Domingo Oeste	Santo Domingo	10904	DO	18.52	-70.01	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	75	60	350	4.30	200	35	t	t	t	t	f	\N	\N	\N	200	t	\N	d0000018-0018-0018-0018-000000000018	2026-01-05 03:37:35.521145+00	\N	f	\N
d0000019-0019-0019-0019-000000000019	Usados RD Premium	Usados RD	Los mejores usados con garantía y financiamiento	1	ventas@usadosrd.com.do	809-555-0119	809-555-0119	https://usadosrd.com.do	Av. Charles de Gaulle 500, Los Mina	Santo Domingo Este	Santo Domingo	11506	DO	18.4889	-69.8561	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	55	45	280	4.20	150	25	t	t	t	t	t	\N	\N	\N	150	f	\N	d0000019-0019-0019-0019-000000000019	2026-01-05 03:37:35.521145+00	\N	f	\N
d0000020-0020-0020-0020-000000000020	Premium Autos SD	Premium Autos	Concesionario multimarca premium en SD	1	info@premiumautos.com.do	809-555-0120	809-555-0120	https://premiumautos.com.do	Av. 27 de Febrero 350, Bella Vista	Santo Domingo	Distrito Nacional	10114	DO	18.4567	-69.9278	\N	\N	\N	\N	\N	\N	\N	\N	2	\N	\N	\N	\N	42	35	195	4.50	110	20	t	t	t	t	t	\N	\N	\N	130	t	\N	d0000020-0020-0020-0020-000000000020	2026-01-05 03:37:35.521145+00	\N	f	\N
\.


--
-- Data for Name: IdentityDocuments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."IdentityDocuments" ("Id", "SellerProfileId", "DocumentType", "DocumentNumber", "IssuingCountry", "IssueDate", "ExpiryDate", "FrontImageUrl", "BackImageUrl", "SelfieWithDocumentUrl", "ExtractedFullName", "ExtractedDateOfBirth", "ExtractedAddress", "Status", "VerifiedAt", "VerifiedByUserId", "VerificationNotes", "RejectionReason", "IsEncrypted", "ViewCount", "LastViewedAt", "LastViewedByUserId", "UploadedAt", "UpdatedAt", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: ModuleAddons; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ModuleAddons" ("Id", "Name", "Code", "Description", "LongDescription", "Category", "MonthlyPrice", "YearlyPrice", "HasFreeTrial", "TrialDays", "Features", "RequiredModules", "IncludedInPlans", "IsActive", "IsVisible", "SortOrder", "IconUrl", "ThumbnailUrl", "DocumentationUrl", "CreatedAt", "UpdatedAt", "CreatedBy") FROM stdin;
\.


--
-- Data for Name: PlatformEmployeeInvitations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PlatformEmployeeInvitations" ("Id", "Email", "PlatformRole", "Permissions", "InvitedBy", "Status", "InvitationDate", "ExpirationDate", "AcceptedDate", "Token") FROM stdin;
\.


--
-- Data for Name: PlatformEmployees; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."PlatformEmployees" ("Id", "UserId", "PlatformRole", "Permissions", "AssignedBy", "Status", "HireDate", "Department", "Notes", "UserId1") FROM stdin;
\.


--
-- Data for Name: SellerProfiles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."SellerProfiles" ("Id", "UserId", "FullName", "DateOfBirth", "Nationality", "Phone", "AlternatePhone", "WhatsApp", "Email", "Address", "City", "State", "ZipCode", "Country", "Latitude", "Longitude", "VerificationStatus", "VerifiedAt", "VerifiedByUserId", "VerificationNotes", "RejectionReason", "VerificationExpiresAt", "TotalListings", "ActiveListings", "TotalSales", "AverageRating", "TotalReviews", "ResponseTimeMinutes", "IsActive", "AcceptsOffers", "ShowPhone", "ShowLocation", "PreferredContactMethod", "MaxActiveListings", "CanSellHighValue", "CreatedAt", "UpdatedAt", "IsDeleted") FROM stdin;
\.


--
-- Data for Name: SubscriptionHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."SubscriptionHistory" ("Id", "DealerSubscriptionId", "FromPlan", "ToPlan", "Reason", "ChangedAt", "ChangedBy", "Notes") FROM stdin;
\.


--
-- Data for Name: UserRoles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."UserRoles" ("Id", "UserId", "RoleId", "AssignedAt", "AssignedBy", "RevokedAt", "RevokedBy", "IsActive") FROM stdin;
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "PhoneNumber", "IsActive", "EmailConfirmed", "CreatedAt", "UpdatedAt", "LastLoginAt", "AccountType", "CreatedBy", "DealerId", "DealerPermissions", "DealerRole", "EmployerUserId", "PlatformPermissions", "PlatformRole") FROM stdin;
798a2ebf-4ab0-46c4-8289-ba2da6ccc955	synctest@example.com	SYNCED_FROM_AUTH	synctest			t	f	2026-01-06 19:54:06.862654+00	\N	\N	Individual	\N	\N	\N	\N	\N	\N	\N
3c62b315-776b-43e1-a2b3-69ff98d9d8ba	rabbitmq_test_1767729683@test.com	SYNCED_FROM_AUTH	rmqtest1767729683			t	f	2026-01-06 20:01:23.343631+00	\N	\N	Individual	\N	\N	\N	\N	\N	\N	\N
8be894d1-e151-4a7d-8918-06fed494bc1a	test@okla.com.do	SYNCED_FROM_AUTH	testuser			t	f	2026-01-08 10:02:59.994201+00	\N	\N	Individual	\N	\N	\N	\N	\N	\N	\N
932845d1-a6c2-46ba-8f7c-8285ff1b2930	testuser1767866643@okla.com.do	SYNCED_FROM_AUTH	testuser1767866643			t	f	2026-01-08 10:04:03.582702+00	\N	\N	Individual	\N	\N	\N	\N	\N	\N	\N
ad73cdbc-9cce-4457-ae8d-7e4bc3522a80	testuser1767868058@okla.com.do	SYNCED_FROM_AUTH	testuser1767868058			t	f	2026-01-08 10:27:38.786654+00	\N	\N	Individual	\N	\N	\N	\N	\N	\N	\N
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20251023014417_InitialCreate	8.0.11
20251201102941_InitialUserSchema	8.0.11
20260103211122_AddDealerAndSellerProfile	8.0.11
20260103222548_AddAccountTypeAndSellerFields	8.0.11
\.


--
-- Data for Name: error_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.error_logs (id, service_name, exception_type, message, stack_trace, occurred_at, endpoint, http_method, status_code, user_id, metadata) FROM stdin;
bf287a04-0d5a-4cf1-9f53-d7468adcfe7d	UserService	PostgresException	42703: column u.CreatedBy does not exist\n\nPOSITION: 493	   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass30_0`2.<<ExecuteAsync>b__0>d.MoveNext()\n--- End of stack trace from previous location ---\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at UserService.Infrastructure.Persistence.UserRepository.GetAllAsync(Int32 page, Int32 pageSize) in /app/UserService/UserService.Infrastructure/Persistence/UserRepository.cs:line 36\n   at UserService.Application.UseCases.Users.GetUsers.GetUsersQueryHandler.Handle(GetUsersQuery request, CancellationToken cancellationToken) in /app/UserService/UserService.Application/UseCases/Users/GetUsers/GetUsersQuery.cs:line 23\n   at UserService.Application.Behaviors.ValidationBehavior`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /app/UserService/UserService.Application/Behaviors/ValidationBehavior.cs:line 26\n   at UserService.Api.Controllers.UsersController.GetUsers(Int32 page, Int32 pageSize) in /app/UserService/UserService.Api/Controllers/UsersController.cs:line 34\n   at lambda_method8(Closure, Object)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at UserService.Shared.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context, IErrorReporter errorReporter, IEventPublisher eventPublisher) in /app/UserService/UserService.Shared/Middleware/RoleHandlingMiddleware.cs:line 27	2026-01-06 18:57:37.98891+00	/api/users	GET	500	\N	{"ClientIp": "184.31.176.162", "RequestId": "0HNIC91SLIGEB:00000001", "UserAgent": "curl/8.7.1", "ServiceName": "UserService", "RequestQuery": "?page=1"}
b3fd0d9e-a898-4a3e-bd5d-324e422b1c3e	UserService	PostgresException	42703: column u.CreatedBy does not exist\n\nPOSITION: 493	   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass30_0`2.<<ExecuteAsync>b__0>d.MoveNext()\n--- End of stack trace from previous location ---\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at UserService.Infrastructure.Persistence.UserRepository.GetAllAsync(Int32 page, Int32 pageSize) in /app/UserService/UserService.Infrastructure/Persistence/UserRepository.cs:line 36\n   at UserService.Application.UseCases.Users.GetUsers.GetUsersQueryHandler.Handle(GetUsersQuery request, CancellationToken cancellationToken) in /app/UserService/UserService.Application/UseCases/Users/GetUsers/GetUsersQuery.cs:line 23\n   at UserService.Application.Behaviors.ValidationBehavior`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /app/UserService/UserService.Application/Behaviors/ValidationBehavior.cs:line 26\n   at UserService.Api.Controllers.UsersController.GetUsers(Int32 page, Int32 pageSize) in /app/UserService/UserService.Api/Controllers/UsersController.cs:line 34\n   at lambda_method8(Closure, Object)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at UserService.Shared.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context, IErrorReporter errorReporter, IEventPublisher eventPublisher) in /app/UserService/UserService.Shared/Middleware/RoleHandlingMiddleware.cs:line 27	2026-01-06 19:13:48.025232+00	/api/users	GET	500	\N	{"ClientIp": "184.31.176.162", "RequestId": "0HNIC91SLIGFE:00000001", "UserAgent": "curl/8.7.1", "ServiceName": "UserService", "RequestQuery": ""}
950d881c-5c50-46b9-9a73-c2fc7365a206	UserService	PostgresException	42703: column u.CreatedBy does not exist\n\nPOSITION: 493	   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass30_0`2.<<ExecuteAsync>b__0>d.MoveNext()\n--- End of stack trace from previous location ---\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at UserService.Infrastructure.Persistence.UserRepository.GetAllAsync(Int32 page, Int32 pageSize) in /app/UserService/UserService.Infrastructure/Persistence/UserRepository.cs:line 36\n   at UserService.Application.UseCases.Users.GetUsers.GetUsersQueryHandler.Handle(GetUsersQuery request, CancellationToken cancellationToken) in /app/UserService/UserService.Application/UseCases/Users/GetUsers/GetUsersQuery.cs:line 23\n   at UserService.Application.Behaviors.ValidationBehavior`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /app/UserService/UserService.Application/Behaviors/ValidationBehavior.cs:line 26\n   at UserService.Api.Controllers.UsersController.GetUsers(Int32 page, Int32 pageSize) in /app/UserService/UserService.Api/Controllers/UsersController.cs:line 34\n   at lambda_method8(Closure, Object)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at UserService.Shared.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context, IErrorReporter errorReporter, IEventPublisher eventPublisher) in /app/UserService/UserService.Shared/Middleware/RoleHandlingMiddleware.cs:line 27	2026-01-06 19:13:57.639958+00	/api/users	GET	500	\N	{"ClientIp": "184.31.176.162", "RequestId": "0HNIC91SLIGFG:00000001", "UserAgent": "curl/8.7.1", "ServiceName": "UserService", "RequestQuery": ""}
bac5ca7d-073e-4a71-9dfd-fe17e8bc7580	UserService	PostgresException	42703: column u.CreatedBy does not exist\n\nPOSITION: 493	   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass30_0`2.<<ExecuteAsync>b__0>d.MoveNext()\n--- End of stack trace from previous location ---\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at UserService.Infrastructure.Persistence.UserRepository.GetAllAsync(Int32 page, Int32 pageSize) in /app/UserService/UserService.Infrastructure/Persistence/UserRepository.cs:line 36\n   at UserService.Application.UseCases.Users.GetUsers.GetUsersQueryHandler.Handle(GetUsersQuery request, CancellationToken cancellationToken) in /app/UserService/UserService.Application/UseCases/Users/GetUsers/GetUsersQuery.cs:line 23\n   at UserService.Application.Behaviors.ValidationBehavior`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /app/UserService/UserService.Application/Behaviors/ValidationBehavior.cs:line 26\n   at UserService.Api.Controllers.UsersController.GetUsers(Int32 page, Int32 pageSize) in /app/UserService/UserService.Api/Controllers/UsersController.cs:line 34\n   at lambda_method8(Closure, Object)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at UserService.Shared.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context, IErrorReporter errorReporter, IEventPublisher eventPublisher) in /app/UserService/UserService.Shared/Middleware/RoleHandlingMiddleware.cs:line 27	2026-01-06 19:17:59.118227+00	/api/users	GET	500	\N	{"ClientIp": "184.31.176.162", "RequestId": "0HNIDB14Q0CEN:00000001", "UserAgent": "curl/8.7.1", "ServiceName": "UserService", "RequestQuery": ""}
89e71a0a-3e3d-4fe8-bb72-bcb6c14f3def	UserService	PostgresException	42703: column u.CreatedBy does not exist\n\nPOSITION: 493	   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)\n   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)\n   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.InitializeReaderAsync(AsyncEnumerator enumerator, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass30_0`2.<<ExecuteAsync>b__0>d.MoveNext()\n--- End of stack trace from previous location ---\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementationAsync[TState,TResult](Func`4 operation, Func`4 verifySucceeded, TState state, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)\n   at UserService.Infrastructure.Persistence.UserRepository.GetAllAsync(Int32 page, Int32 pageSize) in /app/UserService/UserService.Infrastructure/Persistence/UserRepository.cs:line 36\n   at UserService.Application.UseCases.Users.GetUsers.GetUsersQueryHandler.Handle(GetUsersQuery request, CancellationToken cancellationToken) in /app/UserService/UserService.Application/UseCases/Users/GetUsers/GetUsersQuery.cs:line 23\n   at UserService.Application.Behaviors.ValidationBehavior`2.Handle(TRequest request, RequestHandlerDelegate`1 next, CancellationToken cancellationToken) in /app/UserService/UserService.Application/Behaviors/ValidationBehavior.cs:line 26\n   at UserService.Api.Controllers.UsersController.GetUsers(Int32 page, Int32 pageSize) in /app/UserService/UserService.Api/Controllers/UsersController.cs:line 34\n   at lambda_method8(Closure, Object)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.AwaitableObjectResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)\n   at Microsoft.AspNetCore.Routing.EndpointMiddleware.<Invoke>g__AwaitRequestTask|7_0(Endpoint endpoint, Task requestTask, ILogger logger)\n   at UserService.Shared.Middleware.ErrorHandlingMiddleware.InvokeAsync(HttpContext context, IErrorReporter errorReporter, IEventPublisher eventPublisher) in /app/UserService/UserService.Shared/Middleware/RoleHandlingMiddleware.cs:line 27	2026-01-06 19:22:09.263547+00	/api/users	GET	500	\N	{"ClientIp": "184.31.176.162", "RequestId": "0HNIDB34O7L4K:00000001", "UserAgent": "curl/8.7.1", "ServiceName": "UserService", "RequestQuery": ""}
\.


--
-- Name: DealerEmployeeInvitations PK_DealerEmployeeInvitations; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployeeInvitations"
    ADD CONSTRAINT "PK_DealerEmployeeInvitations" PRIMARY KEY ("Id");


--
-- Name: DealerEmployees PK_DealerEmployees; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployees"
    ADD CONSTRAINT "PK_DealerEmployees" PRIMARY KEY ("Id");


--
-- Name: DealerModuleSubscriptions PK_DealerModuleSubscriptions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerModuleSubscriptions"
    ADD CONSTRAINT "PK_DealerModuleSubscriptions" PRIMARY KEY ("Id");


--
-- Name: DealerSubscriptions PK_DealerSubscriptions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerSubscriptions"
    ADD CONSTRAINT "PK_DealerSubscriptions" PRIMARY KEY ("Id");


--
-- Name: Dealers PK_Dealers; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Dealers"
    ADD CONSTRAINT "PK_Dealers" PRIMARY KEY ("Id");


--
-- Name: IdentityDocuments PK_IdentityDocuments; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."IdentityDocuments"
    ADD CONSTRAINT "PK_IdentityDocuments" PRIMARY KEY ("Id");


--
-- Name: ModuleAddons PK_ModuleAddons; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ModuleAddons"
    ADD CONSTRAINT "PK_ModuleAddons" PRIMARY KEY ("Id");


--
-- Name: PlatformEmployeeInvitations PK_PlatformEmployeeInvitations; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PlatformEmployeeInvitations"
    ADD CONSTRAINT "PK_PlatformEmployeeInvitations" PRIMARY KEY ("Id");


--
-- Name: PlatformEmployees PK_PlatformEmployees; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PlatformEmployees"
    ADD CONSTRAINT "PK_PlatformEmployees" PRIMARY KEY ("Id");


--
-- Name: SellerProfiles PK_SellerProfiles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."SellerProfiles"
    ADD CONSTRAINT "PK_SellerProfiles" PRIMARY KEY ("Id");


--
-- Name: SubscriptionHistory PK_SubscriptionHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."SubscriptionHistory"
    ADD CONSTRAINT "PK_SubscriptionHistory" PRIMARY KEY ("Id");


--
-- Name: UserRoles PK_UserRoles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "PK_UserRoles" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: error_logs PK_error_logs; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.error_logs
    ADD CONSTRAINT "PK_error_logs" PRIMARY KEY (id);


--
-- Name: IX_DealerEmployeeInvitations_Email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployeeInvitations_Email" ON public."DealerEmployeeInvitations" USING btree ("Email");


--
-- Name: IX_DealerEmployeeInvitations_InvitedBy; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployeeInvitations_InvitedBy" ON public."DealerEmployeeInvitations" USING btree ("InvitedBy");


--
-- Name: IX_DealerEmployeeInvitations_Status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployeeInvitations_Status" ON public."DealerEmployeeInvitations" USING btree ("Status");


--
-- Name: IX_DealerEmployeeInvitations_Token; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_DealerEmployeeInvitations_Token" ON public."DealerEmployeeInvitations" USING btree ("Token");


--
-- Name: IX_DealerEmployees_DealerId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployees_DealerId" ON public."DealerEmployees" USING btree ("DealerId");


--
-- Name: IX_DealerEmployees_InvitedBy; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployees_InvitedBy" ON public."DealerEmployees" USING btree ("InvitedBy");


--
-- Name: IX_DealerEmployees_Status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployees_Status" ON public."DealerEmployees" USING btree ("Status");


--
-- Name: IX_DealerEmployees_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployees_UserId" ON public."DealerEmployees" USING btree ("UserId");


--
-- Name: IX_DealerEmployees_UserId1; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerEmployees_UserId1" ON public."DealerEmployees" USING btree ("UserId1");


--
-- Name: IX_DealerModuleSubscriptions_DealerId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerModuleSubscriptions_DealerId" ON public."DealerModuleSubscriptions" USING btree ("DealerId");


--
-- Name: IX_DealerModuleSubscriptions_ModuleAddonId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_DealerModuleSubscriptions_ModuleAddonId" ON public."DealerModuleSubscriptions" USING btree ("ModuleAddonId");


--
-- Name: IX_Dealers_BusinessRegistrationNumber; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Dealers_BusinessRegistrationNumber" ON public."Dealers" USING btree ("BusinessRegistrationNumber");


--
-- Name: IX_Dealers_City; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Dealers_City" ON public."Dealers" USING btree ("City");


--
-- Name: IX_Dealers_Email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Dealers_Email" ON public."Dealers" USING btree ("Email");


--
-- Name: IX_Dealers_Latitude_Longitude; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Dealers_Latitude_Longitude" ON public."Dealers" USING btree ("Latitude", "Longitude");


--
-- Name: IX_Dealers_OwnerUserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Dealers_OwnerUserId" ON public."Dealers" USING btree ("OwnerUserId");


--
-- Name: IX_Dealers_VerificationStatus; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Dealers_VerificationStatus" ON public."Dealers" USING btree ("VerificationStatus");


--
-- Name: IX_IdentityDocuments_DocumentType_DocumentNumber; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_IdentityDocuments_DocumentType_DocumentNumber" ON public."IdentityDocuments" USING btree ("DocumentType", "DocumentNumber");


--
-- Name: IX_IdentityDocuments_SellerProfileId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_IdentityDocuments_SellerProfileId" ON public."IdentityDocuments" USING btree ("SellerProfileId");


--
-- Name: IX_IdentityDocuments_Status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_IdentityDocuments_Status" ON public."IdentityDocuments" USING btree ("Status");


--
-- Name: IX_PlatformEmployeeInvitations_Email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployeeInvitations_Email" ON public."PlatformEmployeeInvitations" USING btree ("Email");


--
-- Name: IX_PlatformEmployeeInvitations_InvitedBy; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployeeInvitations_InvitedBy" ON public."PlatformEmployeeInvitations" USING btree ("InvitedBy");


--
-- Name: IX_PlatformEmployeeInvitations_Status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployeeInvitations_Status" ON public."PlatformEmployeeInvitations" USING btree ("Status");


--
-- Name: IX_PlatformEmployeeInvitations_Token; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_PlatformEmployeeInvitations_Token" ON public."PlatformEmployeeInvitations" USING btree ("Token");


--
-- Name: IX_PlatformEmployees_AssignedBy; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployees_AssignedBy" ON public."PlatformEmployees" USING btree ("AssignedBy");


--
-- Name: IX_PlatformEmployees_Status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployees_Status" ON public."PlatformEmployees" USING btree ("Status");


--
-- Name: IX_PlatformEmployees_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployees_UserId" ON public."PlatformEmployees" USING btree ("UserId");


--
-- Name: IX_PlatformEmployees_UserId1; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_PlatformEmployees_UserId1" ON public."PlatformEmployees" USING btree ("UserId1");


--
-- Name: IX_SellerProfiles_City; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_SellerProfiles_City" ON public."SellerProfiles" USING btree ("City");


--
-- Name: IX_SellerProfiles_Latitude_Longitude; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_SellerProfiles_Latitude_Longitude" ON public."SellerProfiles" USING btree ("Latitude", "Longitude");


--
-- Name: IX_SellerProfiles_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_SellerProfiles_UserId" ON public."SellerProfiles" USING btree ("UserId");


--
-- Name: IX_SellerProfiles_VerificationStatus; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_SellerProfiles_VerificationStatus" ON public."SellerProfiles" USING btree ("VerificationStatus");


--
-- Name: IX_SubscriptionHistory_DealerSubscriptionId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_SubscriptionHistory_DealerSubscriptionId" ON public."SubscriptionHistory" USING btree ("DealerSubscriptionId");


--
-- Name: IX_UserRoles_IsActive; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_UserRoles_IsActive" ON public."UserRoles" USING btree ("IsActive");


--
-- Name: IX_UserRoles_RoleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_UserRoles_RoleId" ON public."UserRoles" USING btree ("RoleId");


--
-- Name: IX_UserRoles_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_UserRoles_UserId" ON public."UserRoles" USING btree ("UserId");


--
-- Name: IX_Users_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Users_CreatedAt" ON public."Users" USING btree ("CreatedAt");


--
-- Name: IX_Users_Email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Users_Email" ON public."Users" USING btree ("Email");


--
-- Name: IX_Users_IsActive; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Users_IsActive" ON public."Users" USING btree ("IsActive");


--
-- Name: IX_error_logs_occurred_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_error_logs_occurred_at" ON public.error_logs USING btree (occurred_at);


--
-- Name: IX_error_logs_service_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_error_logs_service_name" ON public.error_logs USING btree (service_name);


--
-- Name: IX_error_logs_service_name_occurred_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_error_logs_service_name_occurred_at" ON public.error_logs USING btree (service_name, occurred_at);


--
-- Name: IX_error_logs_status_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_error_logs_status_code" ON public.error_logs USING btree (status_code);


--
-- Name: IX_error_logs_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_error_logs_user_id" ON public.error_logs USING btree (user_id);


--
-- Name: UQ_UserRoles_UserId_RoleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "UQ_UserRoles_UserId_RoleId" ON public."UserRoles" USING btree ("UserId", "RoleId");


--
-- Name: DealerEmployeeInvitations FK_DealerEmployeeInvitations_Users_InvitedBy; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployeeInvitations"
    ADD CONSTRAINT "FK_DealerEmployeeInvitations_Users_InvitedBy" FOREIGN KEY ("InvitedBy") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: DealerEmployees FK_DealerEmployees_Dealers_DealerId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployees"
    ADD CONSTRAINT "FK_DealerEmployees_Dealers_DealerId" FOREIGN KEY ("DealerId") REFERENCES public."Dealers"("Id") ON DELETE CASCADE;


--
-- Name: DealerEmployees FK_DealerEmployees_Users_InvitedBy; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployees"
    ADD CONSTRAINT "FK_DealerEmployees_Users_InvitedBy" FOREIGN KEY ("InvitedBy") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: DealerEmployees FK_DealerEmployees_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployees"
    ADD CONSTRAINT "FK_DealerEmployees_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: DealerEmployees FK_DealerEmployees_Users_UserId1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerEmployees"
    ADD CONSTRAINT "FK_DealerEmployees_Users_UserId1" FOREIGN KEY ("UserId1") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: DealerModuleSubscriptions FK_DealerModuleSubscriptions_ModuleAddons_ModuleAddonId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerModuleSubscriptions"
    ADD CONSTRAINT "FK_DealerModuleSubscriptions_ModuleAddons_ModuleAddonId" FOREIGN KEY ("ModuleAddonId") REFERENCES public."ModuleAddons"("Id") ON DELETE CASCADE;


--
-- Name: DealerModuleSubscriptions FK_DealerModuleSubscriptions_Users_DealerId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."DealerModuleSubscriptions"
    ADD CONSTRAINT "FK_DealerModuleSubscriptions_Users_DealerId" FOREIGN KEY ("DealerId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: IdentityDocuments FK_IdentityDocuments_SellerProfiles_SellerProfileId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."IdentityDocuments"
    ADD CONSTRAINT "FK_IdentityDocuments_SellerProfiles_SellerProfileId" FOREIGN KEY ("SellerProfileId") REFERENCES public."SellerProfiles"("Id") ON DELETE CASCADE;


--
-- Name: PlatformEmployeeInvitations FK_PlatformEmployeeInvitations_Users_InvitedBy; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PlatformEmployeeInvitations"
    ADD CONSTRAINT "FK_PlatformEmployeeInvitations_Users_InvitedBy" FOREIGN KEY ("InvitedBy") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: PlatformEmployees FK_PlatformEmployees_Users_AssignedBy; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PlatformEmployees"
    ADD CONSTRAINT "FK_PlatformEmployees_Users_AssignedBy" FOREIGN KEY ("AssignedBy") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: PlatformEmployees FK_PlatformEmployees_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PlatformEmployees"
    ADD CONSTRAINT "FK_PlatformEmployees_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: PlatformEmployees FK_PlatformEmployees_Users_UserId1; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."PlatformEmployees"
    ADD CONSTRAINT "FK_PlatformEmployees_Users_UserId1" FOREIGN KEY ("UserId1") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: SubscriptionHistory FK_SubscriptionHistory_DealerSubscriptions_DealerSubscriptionId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."SubscriptionHistory"
    ADD CONSTRAINT "FK_SubscriptionHistory_DealerSubscriptions_DealerSubscriptionId" FOREIGN KEY ("DealerSubscriptionId") REFERENCES public."DealerSubscriptions"("Id") ON DELETE CASCADE;


--
-- Name: UserRoles FK_UserRoles_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict KcxE22WeyaAYajCIFUDs1EE5jl3dl3UJ6XjRFatXazjbZpjXPkHwnoyTAIKWVx9

