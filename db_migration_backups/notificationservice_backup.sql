--
-- PostgreSQL database dump
--

\restrict XPTkz7eFEPSvgBbUifUf9D7Cdgow753ShQd0ZpuQATaow45KaRi0GBaSZzp9MZO

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
-- Name: NotificationLogs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NotificationLogs" (
    "Id" uuid NOT NULL,
    "NotificationId" uuid NOT NULL,
    "Action" character varying(50) NOT NULL,
    "Details" character varying(2000),
    "ErrorMessage" character varying(2000),
    "Timestamp" timestamp with time zone NOT NULL,
    "ProviderResponse" character varying(1000),
    "ProviderMessageId" character varying(200),
    "Cost" numeric(18,6),
    "IpAddress" character varying(45),
    "UserAgent" character varying(500),
    "Metadata" text
);


ALTER TABLE public."NotificationLogs" OWNER TO postgres;

--
-- Name: NotificationQueues; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."NotificationQueues" (
    "Id" uuid NOT NULL,
    "NotificationId" uuid NOT NULL,
    "QueuedAt" timestamp with time zone NOT NULL,
    "ProcessedAt" timestamp with time zone,
    "RetryCount" integer NOT NULL,
    "ErrorMessage" character varying(1000),
    "Status" text NOT NULL,
    "NextRetryAt" timestamp with time zone
);


ALTER TABLE public."NotificationQueues" OWNER TO postgres;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Name: notification_templates; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notification_templates (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    subject character varying(500) NOT NULL,
    body text NOT NULL,
    type character varying(20) NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    variables jsonb,
    "DealerId" uuid,
    category character varying(100),
    created_by character varying(100) DEFAULT 'System'::character varying NOT NULL,
    description character varying(1000),
    preview_data jsonb,
    previous_version_id uuid,
    tags character varying(500),
    updated_by character varying(100),
    validation_rules jsonb,
    version integer DEFAULT 1 NOT NULL
);


ALTER TABLE public.notification_templates OWNER TO postgres;

--
-- Name: notifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.notifications (
    id uuid NOT NULL,
    type character varying(20) NOT NULL,
    recipient character varying(255) NOT NULL,
    subject character varying(500) NOT NULL,
    content text NOT NULL,
    status character varying(20) NOT NULL,
    provider character varying(20) NOT NULL,
    priority character varying(20) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    sent_at timestamp with time zone,
    retry_count integer NOT NULL,
    error_message character varying(1000),
    template_name character varying(100),
    metadata jsonb,
    "DealerId" uuid
);


ALTER TABLE public.notifications OWNER TO postgres;

--
-- Name: scheduled_notifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.scheduled_notifications (
    id uuid NOT NULL,
    notification_id uuid NOT NULL,
    scheduled_for timestamp with time zone NOT NULL,
    time_zone character varying(50) DEFAULT 'UTC'::character varying,
    status character varying(20) NOT NULL,
    is_recurring boolean DEFAULT false NOT NULL,
    recurrence_type character varying(20),
    cron_expression character varying(100),
    next_execution timestamp with time zone,
    last_execution timestamp with time zone,
    execution_count integer DEFAULT 0 NOT NULL,
    max_executions integer,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone,
    cancelled_at timestamp with time zone,
    created_by character varying(100) DEFAULT 'System'::character varying NOT NULL,
    cancelled_by character varying(100),
    cancellation_reason character varying(500),
    failure_count integer DEFAULT 0 NOT NULL,
    last_error character varying(2000)
);


ALTER TABLE public.scheduled_notifications OWNER TO postgres;

--
-- Data for Name: NotificationLogs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."NotificationLogs" ("Id", "NotificationId", "Action", "Details", "ErrorMessage", "Timestamp", "ProviderResponse", "ProviderMessageId", "Cost", "IpAddress", "UserAgent", "Metadata") FROM stdin;
eaa79d1c-d222-4468-b8ab-9df107279b33	cf667d55-00f9-4b7b-826c-3529d9fa5ada	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 18:56:47.635967+00	\N	\N	\N	\N	\N	\N
36593b99-8c38-43ce-9b7d-226441e7dc26	977958cb-5f5b-4562-a33c-34ccc73ee6cf	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 19:02:48.186989+00	\N	\N	\N	\N	\N	\N
e1907546-d47d-4fc9-9480-ca2f98c0527f	cf5e99a7-eb99-4fa1-91f2-a845110d0e58	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 19:11:04.322529+00	\N	\N	\N	\N	\N	\N
105afacb-de6d-426e-ac1a-34c4483a2d23	8db70279-7562-4d9f-87ac-d523c8962727	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 19:11:10.337461+00	\N	\N	\N	\N	\N	\N
5c82d36a-035b-4473-bef4-b7b6bcbaaaf7	278e921c-3774-4d73-a020-fe03c446f5d2	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 19:11:16.768365+00	\N	\N	\N	\N	\N	\N
d66f88fe-0e5c-4380-9d82-25b57f98de9b	cf92319d-b722-4fd6-b481-f67f716cff48	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 19:54:07.463619+00	\N	\N	\N	\N	\N	\N
c0b2725f-8143-4453-bb5a-a36222204a71	4e129295-82f4-4c7a-8e7c-b1d2c59519cb	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-06 20:01:23.826123+00	\N	\N	\N	\N	\N	\N
612bfe0e-8bf5-4a5a-b645-ac2d7192865e	0770bdb4-7133-4653-ae3c-bff30a9a7345	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-08 10:03:01.018273+00	\N	\N	\N	\N	\N	\N
2238b0f8-d5dc-4979-9312-bb60fb1f0585	86e50fe4-cb9b-428c-bcee-5db1bfea33d5	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-08 10:04:04.117392+00	\N	\N	\N	\N	\N	\N
8450ee08-7431-43c5-b194-d2d0b70fcb83	471d9cc8-36d2-43e4-9d34-c9ef4df646e4	FAILED	Failed to send notification	SendGrid API error: Unauthorized	2026-01-08 10:27:39.411688+00	\N	\N	\N	\N	\N	\N
\.


--
-- Data for Name: NotificationQueues; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."NotificationQueues" ("Id", "NotificationId", "QueuedAt", "ProcessedAt", "RetryCount", "ErrorMessage", "Status", "NextRetryAt") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20251023183351_InitialCreate	8.0.11
20251206020716_AddMultiTenantSupport	8.0.11
\.


--
-- Data for Name: notification_templates; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.notification_templates (id, name, subject, body, type, is_active, created_at, updated_at, variables, "DealerId", category, created_by, description, preview_data, previous_version_id, tags, updated_by, validation_rules, version) FROM stdin;
\.


--
-- Data for Name: notifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.notifications (id, type, recipient, subject, content, status, provider, priority, created_at, sent_at, retry_count, error_message, template_name, metadata, "DealerId") FROM stdin;
cf667d55-00f9-4b7b-826c-3529d9fa5ada	Email	testprod@example.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, testprod!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 18:56:46.933939+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "testprod", "supportEmail": "support@example.com"}	\N
977958cb-5f5b-4562-a33c-34ccc73ee6cf	Email	newtest999@test.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, newtest999!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 19:02:47.754264+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "newtest999", "supportEmail": "support@example.com"}	\N
cf5e99a7-eb99-4fa1-91f2-a845110d0e58	Email	comprador@test.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, comprador1!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 19:11:03.722215+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "comprador1", "supportEmail": "support@example.com"}	\N
8db70279-7562-4d9f-87ac-d523c8962727	Email	dealer@test.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, dealer1!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 19:11:10.028926+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "dealer1", "supportEmail": "support@example.com"}	\N
278e921c-3774-4d73-a020-fe03c446f5d2	Email	empleado@test.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, empleado1!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 19:11:16.461371+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "empleado1", "supportEmail": "support@example.com"}	\N
cf92319d-b722-4fd6-b481-f67f716cff48	Email	synctest@example.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, synctest!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 19:54:06.884488+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "synctest", "supportEmail": "support@example.com"}	\N
4e129295-82f4-4c7a-8e7c-b1d2c59519cb	Email	rabbitmq_test_1767729683@test.com	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, rmqtest1767729683!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-06 20:01:23.371294+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "rmqtest1767729683", "supportEmail": "support@example.com"}	\N
0770bdb4-7133-4653-ae3c-bff30a9a7345	Email	test@okla.com.do	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, testuser!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-08 10:03:00.219355+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "testuser", "supportEmail": "support@example.com"}	\N
86e50fe4-cb9b-428c-bcee-5db1bfea33d5	Email	testuser1767866643@okla.com.do	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, testuser1767866643!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-08 10:04:03.61237+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "testuser1767866643", "supportEmail": "support@example.com"}	\N
471d9cc8-36d2-43e4-9d34-c9ef4df646e4	Email	testuser1767868058@okla.com.do	Welcome to Our Platform!	\n            <!DOCTYPE html>\n            <html>\n            <head>\n                <style>\n                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }\n                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }\n                    .welcome { color: #28a745; }\n                </style>\n            </head>\n            <body>\n                <div class='container'>\n                    <h2 class='welcome'>Welcome, testuser1767868058!</h2>\n                    <p>Thank you for joining our platform. We're excited to have you on board!</p>\n                    <p>Your account has been successfully created and you can now start using all our features.</p>\n                    <p>If you have any questions, feel free to contact our support team.</p>\n                    <p>Best regards,<br/>The Team</p>\n                </div>\n            </body>\n            </html>	Failed	SendGrid	Medium	2026-01-08 10:27:38.813794+00	\N	1	SendGrid API error: Unauthorized	\N	{"loginUrl": "http://localhost:3000/login", "username": "testuser1767868058", "supportEmail": "support@example.com"}	\N
\.


--
-- Data for Name: scheduled_notifications; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.scheduled_notifications (id, notification_id, scheduled_for, time_zone, status, is_recurring, recurrence_type, cron_expression, next_execution, last_execution, execution_count, max_executions, created_at, updated_at, cancelled_at, created_by, cancelled_by, cancellation_reason, failure_count, last_error) FROM stdin;
\.


--
-- Name: NotificationLogs PK_NotificationLogs; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NotificationLogs"
    ADD CONSTRAINT "PK_NotificationLogs" PRIMARY KEY ("Id");


--
-- Name: NotificationQueues PK_NotificationQueues; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NotificationQueues"
    ADD CONSTRAINT "PK_NotificationQueues" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: notification_templates PK_notification_templates; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notification_templates
    ADD CONSTRAINT "PK_notification_templates" PRIMARY KEY (id);


--
-- Name: notifications PK_notifications; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.notifications
    ADD CONSTRAINT "PK_notifications" PRIMARY KEY (id);


--
-- Name: scheduled_notifications PK_scheduled_notifications; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.scheduled_notifications
    ADD CONSTRAINT "PK_scheduled_notifications" PRIMARY KEY (id);


--
-- Name: IX_NotificationLogs_Action; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_NotificationLogs_Action" ON public."NotificationLogs" USING btree ("Action");


--
-- Name: IX_NotificationLogs_NotificationId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_NotificationLogs_NotificationId" ON public."NotificationLogs" USING btree ("NotificationId");


--
-- Name: IX_NotificationLogs_NotificationId_Timestamp; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_NotificationLogs_NotificationId_Timestamp" ON public."NotificationLogs" USING btree ("NotificationId", "Timestamp");


--
-- Name: IX_NotificationLogs_Timestamp; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_NotificationLogs_Timestamp" ON public."NotificationLogs" USING btree ("Timestamp");


--
-- Name: IX_NotificationQueues_NotificationId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_NotificationQueues_NotificationId" ON public."NotificationQueues" USING btree ("NotificationId");


--
-- Name: IX_NotificationTemplate_DealerId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_NotificationTemplate_DealerId" ON public.notification_templates USING btree ("DealerId");


--
-- Name: IX_Notification_DealerId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Notification_DealerId" ON public.notifications USING btree ("DealerId");


--
-- Name: IX_notification_templates_category; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notification_templates_category" ON public.notification_templates USING btree (category);


--
-- Name: IX_notification_templates_is_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notification_templates_is_active" ON public.notification_templates USING btree (is_active);


--
-- Name: IX_notification_templates_name; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_notification_templates_name" ON public.notification_templates USING btree (name);


--
-- Name: IX_notification_templates_previous_version_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notification_templates_previous_version_id" ON public.notification_templates USING btree (previous_version_id);


--
-- Name: IX_notification_templates_type; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notification_templates_type" ON public.notification_templates USING btree (type);


--
-- Name: IX_notification_templates_version; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notification_templates_version" ON public.notification_templates USING btree (version);


--
-- Name: IX_notifications_created_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_created_at" ON public.notifications USING btree (created_at);


--
-- Name: IX_notifications_provider; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_provider" ON public.notifications USING btree (provider);


--
-- Name: IX_notifications_recipient; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_recipient" ON public.notifications USING btree (recipient);


--
-- Name: IX_notifications_recipient_created_at; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_recipient_created_at" ON public.notifications USING btree (recipient, created_at);


--
-- Name: IX_notifications_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_status" ON public.notifications USING btree (status);


--
-- Name: IX_notifications_type; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_type" ON public.notifications USING btree (type);


--
-- Name: IX_notifications_type_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_notifications_type_status" ON public.notifications USING btree (type, status);


--
-- Name: IX_scheduled_notifications_is_recurring; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_is_recurring" ON public.scheduled_notifications USING btree (is_recurring);


--
-- Name: IX_scheduled_notifications_next_execution; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_next_execution" ON public.scheduled_notifications USING btree (next_execution);


--
-- Name: IX_scheduled_notifications_notification_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_notification_id" ON public.scheduled_notifications USING btree (notification_id);


--
-- Name: IX_scheduled_notifications_scheduled_for; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_scheduled_for" ON public.scheduled_notifications USING btree (scheduled_for);


--
-- Name: IX_scheduled_notifications_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_status" ON public.scheduled_notifications USING btree (status);


--
-- Name: IX_scheduled_notifications_status_next_execution; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_status_next_execution" ON public.scheduled_notifications USING btree (status, next_execution);


--
-- Name: IX_scheduled_notifications_status_scheduled_for; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_scheduled_notifications_status_scheduled_for" ON public.scheduled_notifications USING btree (status, scheduled_for);


--
-- Name: NotificationLogs FK_NotificationLogs_notifications_NotificationId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NotificationLogs"
    ADD CONSTRAINT "FK_NotificationLogs_notifications_NotificationId" FOREIGN KEY ("NotificationId") REFERENCES public.notifications(id) ON DELETE CASCADE;


--
-- Name: NotificationQueues FK_NotificationQueues_notifications_NotificationId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."NotificationQueues"
    ADD CONSTRAINT "FK_NotificationQueues_notifications_NotificationId" FOREIGN KEY ("NotificationId") REFERENCES public.notifications(id) ON DELETE CASCADE;


--
-- Name: scheduled_notifications FK_scheduled_notifications_notifications_notification_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.scheduled_notifications
    ADD CONSTRAINT "FK_scheduled_notifications_notifications_notification_id" FOREIGN KEY (notification_id) REFERENCES public.notifications(id) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict XPTkz7eFEPSvgBbUifUf9D7Cdgow753ShQd0ZpuQATaow45KaRi0GBaSZzp9MZO

