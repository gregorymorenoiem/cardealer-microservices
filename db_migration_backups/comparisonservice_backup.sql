--
-- PostgreSQL database dump
--

\restrict sweRo1O2aa1aTGkQ7Dsrm7Ma3RGweEoHh5EQdg8QI8EeqysqdFgagcnxpaDnFiN

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
-- Name: comparisons; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.comparisons (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "VehicleIds" jsonb NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "IsPublic" boolean NOT NULL,
    "ShareToken" character varying(50)
);


ALTER TABLE public.comparisons OWNER TO postgres;

--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20260108103456_Initial	8.0.0
\.


--
-- Data for Name: comparisons; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.comparisons ("Id", "UserId", "Name", "VehicleIds", "CreatedAt", "UpdatedAt", "IsPublic", "ShareToken") FROM stdin;
\.


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: comparisons PK_comparisons; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.comparisons
    ADD CONSTRAINT "PK_comparisons" PRIMARY KEY ("Id");


--
-- Name: IX_comparisons_CreatedAt; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_comparisons_CreatedAt" ON public.comparisons USING btree ("CreatedAt");


--
-- Name: IX_comparisons_ShareToken; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_comparisons_ShareToken" ON public.comparisons USING btree ("ShareToken");


--
-- Name: IX_comparisons_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_comparisons_UserId" ON public.comparisons USING btree ("UserId");


--
-- PostgreSQL database dump complete
--

\unrestrict sweRo1O2aa1aTGkQ7Dsrm7Ma3RGweEoHh5EQdg8QI8EeqysqdFgagcnxpaDnFiN

