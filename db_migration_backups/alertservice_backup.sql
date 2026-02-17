--
-- PostgreSQL database dump
--

\restrict tAFFVHuNTaOqogsSmOGaVVxrvYpC3dQuydrjgTZSrMUhXqjKTdSXJARqw6Ie4dF

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
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Name: price_alerts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.price_alerts (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "VehicleId" uuid NOT NULL,
    "TargetPrice" numeric(18,2) NOT NULL,
    "Condition" integer NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "IsTriggered" boolean DEFAULT false NOT NULL,
    "TriggeredAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.price_alerts OWNER TO postgres;

--
-- Name: saved_searches; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.saved_searches (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "SearchCriteria" jsonb NOT NULL,
    "SendEmailNotifications" boolean DEFAULT true NOT NULL,
    "Frequency" integer DEFAULT 1 NOT NULL,
    "LastNotificationSent" timestamp with time zone,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


ALTER TABLE public.saved_searches OWNER TO postgres;

--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260108103503_Initial	8.0.0
\.


--
-- Data for Name: price_alerts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.price_alerts ("Id", "UserId", "VehicleId", "TargetPrice", "Condition", "IsActive", "IsTriggered", "TriggeredAt", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- Data for Name: saved_searches; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.saved_searches ("Id", "UserId", "Name", "SearchCriteria", "SendEmailNotifications", "Frequency", "LastNotificationSent", "IsActive", "CreatedAt", "UpdatedAt") FROM stdin;
\.


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: price_alerts PK_price_alerts; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.price_alerts
    ADD CONSTRAINT "PK_price_alerts" PRIMARY KEY ("Id");


--
-- Name: saved_searches PK_saved_searches; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.saved_searches
    ADD CONSTRAINT "PK_saved_searches" PRIMARY KEY ("Id");


--
-- Name: idx_price_alerts_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_price_alerts_active ON public.price_alerts USING btree ("IsActive");


--
-- Name: idx_price_alerts_user; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_price_alerts_user ON public.price_alerts USING btree ("UserId");


--
-- Name: idx_price_alerts_user_vehicle; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_price_alerts_user_vehicle ON public.price_alerts USING btree ("UserId", "VehicleId");


--
-- Name: idx_price_alerts_vehicle; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_price_alerts_vehicle ON public.price_alerts USING btree ("VehicleId");


--
-- Name: idx_saved_searches_active; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_saved_searches_active ON public.saved_searches USING btree ("IsActive");


--
-- Name: idx_saved_searches_last_notification; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_saved_searches_last_notification ON public.saved_searches USING btree ("LastNotificationSent");


--
-- Name: idx_saved_searches_user; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_saved_searches_user ON public.saved_searches USING btree ("UserId");


--
-- PostgreSQL database dump complete
--

\unrestrict tAFFVHuNTaOqogsSmOGaVVxrvYpC3dQuydrjgTZSrMUhXqjKTdSXJARqw6Ie4dF

