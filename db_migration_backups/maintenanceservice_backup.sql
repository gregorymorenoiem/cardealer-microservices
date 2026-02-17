--
-- PostgreSQL database dump
--

\restrict XSvbBFi6XE23gi5RB81yBEoaTtHLcUqk0ZnuuVxMDPIcJ0zoaYssfjZs56wsZXE

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
-- Name: maintenance_windows; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.maintenance_windows (
    "Id" uuid NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(1000) NOT NULL,
    "Type" text NOT NULL,
    "Status" text NOT NULL,
    "ScheduledStart" timestamp with time zone NOT NULL,
    "ScheduledEnd" timestamp with time zone NOT NULL,
    "ActualStart" timestamp with time zone,
    "ActualEnd" timestamp with time zone,
    "CreatedBy" character varying(100) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "Notes" character varying(2000),
    "NotifyUsers" boolean NOT NULL,
    "NotifyMinutesBefore" integer NOT NULL,
    "AffectedServices" text NOT NULL
);


ALTER TABLE public.maintenance_windows OWNER TO postgres;

--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260108103442_Initial	8.0.0
\.


--
-- Data for Name: maintenance_windows; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.maintenance_windows ("Id", "Title", "Description", "Type", "Status", "ScheduledStart", "ScheduledEnd", "ActualStart", "ActualEnd", "CreatedBy", "CreatedAt", "UpdatedAt", "Notes", "NotifyUsers", "NotifyMinutesBefore", "AffectedServices") FROM stdin;
\.


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: maintenance_windows PK_maintenance_windows; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.maintenance_windows
    ADD CONSTRAINT "PK_maintenance_windows" PRIMARY KEY ("Id");


--
-- Name: IX_maintenance_windows_ScheduledEnd; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_maintenance_windows_ScheduledEnd" ON public.maintenance_windows USING btree ("ScheduledEnd");


--
-- Name: IX_maintenance_windows_ScheduledStart; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_maintenance_windows_ScheduledStart" ON public.maintenance_windows USING btree ("ScheduledStart");


--
-- Name: IX_maintenance_windows_Status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_maintenance_windows_Status" ON public.maintenance_windows USING btree ("Status");


--
-- PostgreSQL database dump complete
--

\unrestrict XSvbBFi6XE23gi5RB81yBEoaTtHLcUqk0ZnuuVxMDPIcJ0zoaYssfjZs56wsZXE

