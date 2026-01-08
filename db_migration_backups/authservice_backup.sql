--
-- PostgreSQL database dump
--

\restrict 3YMM0kL6ElDks4RmkSrRCfEopz36pedfLlkkoBCnkNHpRc4SNbDG3jPCWVb6AcJ

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
-- Name: RefreshTokens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."RefreshTokens" (
    "Id" text NOT NULL,
    "Token" text NOT NULL,
    "UserId" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "CreatedByIp" text DEFAULT ''::text NOT NULL,
    "RevokedAt" timestamp with time zone,
    "RevokedByIp" text,
    "RevokedReason" text,
    "ReplacedByToken" text
);


ALTER TABLE public."RefreshTokens" OWNER TO postgres;

--
-- Name: Users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Users" (
    "Id" text NOT NULL,
    "UserName" text,
    "NormalizedUserName" text,
    "Email" text,
    "NormalizedEmail" text,
    "EmailConfirmed" boolean DEFAULT false NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean DEFAULT false NOT NULL,
    "TwoFactorEnabled" boolean DEFAULT false NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean DEFAULT true NOT NULL,
    "AccessFailedCount" integer DEFAULT 0 NOT NULL,
    "FullName" text,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "DealerId" text,
    "AccountType" integer DEFAULT 0 NOT NULL,
    "ExternalAuthProvider" integer,
    "ExternalUserId" text
);


ALTER TABLE public."Users" OWNER TO postgres;

--
-- Name: VerificationTokens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."VerificationTokens" (
    "Id" uuid NOT NULL,
    "Token" text NOT NULL,
    "Email" text DEFAULT ''::text NOT NULL,
    "UserId" text NOT NULL,
    "Type" integer NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "IsUsed" boolean DEFAULT false NOT NULL,
    "UsedAt" timestamp with time zone
);


ALTER TABLE public."VerificationTokens" OWNER TO postgres;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Data for Name: RefreshTokens; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."RefreshTokens" ("Id", "Token", "UserId", "ExpiresAt", "CreatedAt", "UpdatedAt", "CreatedByIp", "RevokedAt", "RevokedByIp", "RevokedReason", "ReplacedByToken") FROM stdin;
c2511bd8-87c9-42d1-b02d-308f419747d5	305e4afc5e444342af32b02559f8190d5995faa54dd94ae9b787ea1106de9883	49b29d84-d936-48a4-b99b-0056d1e3dd2f	2026-01-13 18:56:46.778771+00	2026-01-06 18:56:46.779285+00	\N	184.31.176.162	\N	\N	\N	\N
f0fb8466-4981-4024-b0c0-ad320d3cc4d1	45173b9758884f0eab59e6b5296dedbb950ff85f28384e729a6770b9e8ea8890	49b29d84-d936-48a4-b99b-0056d1e3dd2f	2026-01-13 18:57:05.50645+00	2026-01-06 18:57:05.506572+00	\N	184.31.176.162	\N	\N	\N	\N
1d0dc20c-1ade-4010-aada-4d08ef396c8f	fd140d515cf74e15b17c6ef74a9637ad457a3e8d5ddc4a6b9e2212cb6271ed8d	4812c81c-5cb0-42ab-9ada-06b7bd0eadf8	2026-01-13 19:02:47.693932+00	2026-01-06 19:02:47.693979+00	\N	184.31.176.162	\N	\N	\N	\N
4aafc940-1c8e-43b9-84f7-0586679f7843	52bde6e71f264b2680cee55fa528c9197792577481444db794125d082f4402a2	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:11:03.537341+00	2026-01-06 19:11:03.538268+00	\N	184.31.176.162	\N	\N	\N	\N
8359f49e-ab7d-4c1e-8007-00e9e555de6f	f662cc9a1e5d41728dc90e87803323c67bfade8b9ebc40a5a12f2a3d3a0734ff	62b4318c-3900-4a8d-a24b-b705f4b30884	2026-01-13 19:11:10.021272+00	2026-01-06 19:11:10.021386+00	\N	184.31.176.162	\N	\N	\N	\N
84ab92ca-b351-45e0-b766-ecffb5fed745	74853a192f2d410885115180420dbc97803f1fd8af674e0597d14cd04e8921bf	7e5d177e-058a-4b30-ab7f-af3f00594917	2026-01-13 19:11:16.456408+00	2026-01-06 19:11:16.456433+00	\N	184.31.176.162	\N	\N	\N	\N
34877c4e-3253-4810-b0b7-a2de309a6008	4862c487d26445c8b9540cb394982d7522d5183ea15c4cc195bbe9af129fa220	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:11:42.64803+00	2026-01-06 19:11:42.648059+00	\N	184.31.176.162	\N	\N	\N	\N
c0005da6-ba54-42a9-b45f-f636a24ee76d	5186e37d64ee40109b0f4b80c5d46266fe15631f74a44c6784a039d60eb1284d	62b4318c-3900-4a8d-a24b-b705f4b30884	2026-01-13 19:11:51.322274+00	2026-01-06 19:11:51.322351+00	\N	184.31.176.162	\N	\N	\N	\N
7cb160f9-99d2-4505-9671-97779b548afd	5efb5638da96457a90036a28404f00f07fe805e16b9c4957a6c6e7bd1588c58d	7e5d177e-058a-4b30-ab7f-af3f00594917	2026-01-13 19:11:51.419375+00	2026-01-06 19:11:51.419418+00	\N	184.31.176.162	\N	\N	\N	\N
4aaa1159-b562-4019-b1be-f06c4be3d2da	ee694d3706eb4e0bad77c914fbf39614558fbcfabb7049bbb111628153c29bb7	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:11:57.719582+00	2026-01-06 19:11:57.719633+00	\N	184.31.176.162	\N	\N	\N	\N
6eb4155e-eb16-48ba-974d-90fdcd7f4162	655452efba0f46a8aa4c2bcaaf6708cfa4591dae4b654c619ed600b18b950b68	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:12:11.23416+00	2026-01-06 19:12:11.234222+00	2026-01-06 19:12:11.42184+00	184.31.176.162	2026-01-06 19:12:11.421794+00	rotated	system	c88799c08aad4639bf9a73a305c1b935ca71f6fda65f4badb437ae158ac88d64
42b79021-c887-4485-8409-4cb1a13d90fa	c88799c08aad4639bf9a73a305c1b935ca71f6fda65f4badb437ae158ac88d64	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:12:11.421848+00	2026-01-06 19:12:11.421871+00	\N	184.31.176.162	\N	\N	\N	\N
266ec225-6dff-4dc4-92ce-bae76936b900	0783da777071416faeb30e52144c72169aff674bd5fd47d2b183ed087ab6a3a7	62b4318c-3900-4a8d-a24b-b705f4b30884	2026-01-13 19:12:57.515438+00	2026-01-06 19:12:57.515592+00	\N	184.31.176.162	\N	\N	\N	\N
0ebb931c-a995-471c-8028-8132b33e39ef	9517c891bb2e41d7be29260f679590bf019e931433954a04a2046bdc71587d6d	62b4318c-3900-4a8d-a24b-b705f4b30884	2026-01-13 19:13:21.528498+00	2026-01-06 19:13:21.528566+00	\N	184.31.176.162	\N	\N	\N	\N
84642601-7c49-47bf-929f-d3e0a1b3df24	2fd4c5004e3c43178e062c1f1182c2eb5237474cdad24849899966f785b0efca	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:13:47.374528+00	2026-01-06 19:13:47.374556+00	\N	184.31.176.162	\N	\N	\N	\N
9067c52d-4b86-41cf-a4eb-7cddf3492a64	00304f4207a740a8ac314391a042d9719e995fba17904d2bbb24a0872a3d8ef8	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:13:57.614811+00	2026-01-06 19:13:57.614845+00	\N	184.31.176.162	\N	\N	\N	\N
b2f96ba1-df11-45d7-a6ae-3ef24d50c0cb	f209e088b69d476e9de4a8069a8eb1aae7254b18c8bd4d9ba681225072f1c45f	62b4318c-3900-4a8d-a24b-b705f4b30884	2026-01-13 19:14:34.921482+00	2026-01-06 19:14:34.921546+00	\N	172.18.0.3	\N	\N	\N	\N
7d42bddb-045b-4cfc-bf35-e15babd2b76f	d522a7c6fd434affb7d5bb07726e96a70c7a4ca855b94f27b2f6f330025e2abe	f9ccc98b-f2cc-4747-b815-5ca95d4b5377	2026-01-13 19:17:58.75548+00	2026-01-06 19:17:58.755528+00	\N	184.31.176.162	\N	\N	\N	\N
1d6db6fb-41dd-474d-a930-0139b4182708	304cdfe026e74c6587da8faf78cd778ac55514511fb24aab87ecd91a94208ae2	62b4318c-3900-4a8d-a24b-b705f4b30884	2026-01-13 19:25:57.639335+00	2026-01-06 19:25:57.640098+00	\N	184.31.176.162	\N	\N	\N	\N
d5a4256f-6d64-4d31-b94a-af3c3cd6ccab	4f82ec9a74f24ad1a340b62932c56c90d8f2cc8c21394e48870bc479ceeb5d67	798a2ebf-4ab0-46c4-8289-ba2da6ccc955	2026-01-13 19:54:06.871937+00	2026-01-06 19:54:06.872006+00	\N	184.31.176.162	\N	\N	\N	\N
592f4cb4-e4cd-4fca-8e46-6d521e14d3e7	3ca0ed7cbd464c78804d01deeedd9a7a16529d2111994c059d1533484e15272f	3c62b315-776b-43e1-a2b3-69ff98d9d8ba	2026-01-13 20:01:23.358356+00	2026-01-06 20:01:23.358538+00	\N	184.31.176.162	\N	\N	\N	\N
ce7cae26-bfbf-4519-87a4-df1078c8eb5e	85365b7f0d3c45b9a99a3c2b83bd39d4508d5af47526407e811419d2882d6845	8be894d1-e151-4a7d-8918-06fed494bc1a	2026-01-15 10:03:00.11704+00	2026-01-08 10:03:00.117447+00	\N	192.168.65.1	\N	\N	\N	\N
e1e7341f-dddb-4652-a70d-30f074d511cb	c478f3a0b1dd4739b3e22d300fda5ada996bcb64ab014588ac072dadb72385b4	932845d1-a6c2-46ba-8f7c-8285ff1b2930	2026-01-15 10:04:03.585879+00	2026-01-08 10:04:03.585927+00	\N	192.168.65.1	\N	\N	\N	\N
bfcb6187-d7d2-4df9-8774-a37f74460306	a636e47b95ee45439943aeb92b21738e7efc1ac930fd405aba4065ff787b6dab	ad73cdbc-9cce-4457-ae8d-7e4bc3522a80	2026-01-15 10:27:38.791498+00	2026-01-08 10:27:38.791531+00	\N	172.66.2.5	\N	\N	\N	\N
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Users" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "FullName", "CreatedAt", "UpdatedAt", "DealerId", "AccountType", "ExternalAuthProvider", "ExternalUserId") FROM stdin;
8be894d1-e151-4a7d-8918-06fed494bc1a	testuser	TESTUSER	test@okla.com.do	TEST@OKLA.COM.DO	f	AQAAAAIAAYagAAAAEL6MZdH5pdbpJQa2M0qwK88L1rgy3jAa/qa8ej1k0bDPdiWVWplFahNLbExyHeoJLw==	FDALYPIMFKCIR6PGHROLEKS7MTGF37KR	8715b46e-23d9-4de5-8ac6-47090ed7de0b	\N	f	f	\N	t	0	\N	2026-01-08 10:02:59.994201+00	2026-01-08 10:03:00.084925+00	\N	1	\N	\N
49b29d84-d936-48a4-b99b-0056d1e3dd2f	testprod	TESTPROD	testprod@example.com	TESTPROD@EXAMPLE.COM	t	AQAAAAIAAYagAAAAEO57CSXqTTJFpJ71Sv5Q/M928wHfVEetbJJQqFo/44ODBVBqKBiyT2dZd/EeBlbvng==	PV4L2BLE2F5SX73TFWCQ6K7GSG5UA3IK	e968be44-e0ea-4394-8066-e6700d0418cf	\N	f	f	\N	t	0	\N	2026-01-06 18:56:46.59329+00	2026-01-06 18:57:05.566766+00	\N	1	\N	\N
4812c81c-5cb0-42ab-9ada-06b7bd0eadf8	newtest999	NEWTEST999	newtest999@test.com	NEWTEST999@TEST.COM	f	AQAAAAIAAYagAAAAEMRoQQgFTi2N/UUi9TgduDY66r8wI9IDkTXX7moHmLfnR2VtDOcCgQLm25jUOMXaLg==	WMU6USANOVXU4WTJHKKF6VWAX6JEBM7E	356cb6d6-e9f1-406e-bad5-10e194a33d96	\N	f	f	\N	t	0	\N	2026-01-06 19:02:47.666212+00	2026-01-06 19:02:47.672347+00	\N	1	\N	\N
932845d1-a6c2-46ba-8f7c-8285ff1b2930	testuser1767866643	TESTUSER1767866643	testuser1767866643@okla.com.do	TESTUSER1767866643@OKLA.COM.DO	f	AQAAAAIAAYagAAAAEIhoV+YpuW6aeivUXAWwCZLFN8LgtUvaU3ey2a2GQtzYr5+t1hZkXgKl1BrqTA0AiQ==	QGFK6MWWAJLVQCB4VESMIOZI6TYBGDAI	9def25d3-0677-44f9-b9b3-5383cb23310b	\N	f	f	\N	t	0	\N	2026-01-08 10:04:03.582702+00	2026-01-08 10:04:03.584409+00	\N	1	\N	\N
ad73cdbc-9cce-4457-ae8d-7e4bc3522a80	testuser1767868058	TESTUSER1767868058	testuser1767868058@okla.com.do	TESTUSER1767868058@OKLA.COM.DO	f	AQAAAAIAAYagAAAAEJu+bURrBRndqpKScFnibeGtHE1BDz/dnTQ/OCOzl1zGEtdTL+lUhurqTdNynIsKew==	MMIAH22YLBVQIHC5R5U6WSWMA4Z766DG	ca3cfdca-cd31-4aeb-8a1d-a53d043c23e6	\N	f	f	\N	t	0	\N	2026-01-08 10:27:38.786654+00	2026-01-08 10:27:38.788333+00	\N	1	\N	\N
7e5d177e-058a-4b30-ab7f-af3f00594917	empleado1	EMPLEADO1	empleado@test.com	EMPLEADO@TEST.COM	t	AQAAAAIAAYagAAAAEEUHvSpaENyztFwXiBBgdKTesnsmtj8go05zeplhauomo6Z2McndbAngGJtuir5Gbg==	YRXJ75SARTEJN6YYDXPHE72GVGXLX4QQ	5376c0ec-6d92-4168-b436-31d4bd12a8a6	\N	f	f	\N	t	0	\N	2026-01-06 19:11:16.45198+00	2026-01-06 19:11:51.422178+00	\N	1	\N	\N
f9ccc98b-f2cc-4747-b815-5ca95d4b5377	comprador1	COMPRADOR1	comprador@test.com	COMPRADOR@TEST.COM	t	AQAAAAIAAYagAAAAEFsyw7GkLLDC0sKWPtvETeBTilGx26zlMPiiDpgFJpG0Jh3T2gJnezpR6Z5/LPH9Gg==	7M3FM34LQK3DXGR62GLTNQ7VNIZSDFNO	45cd433d-426d-47f5-9cc2-9f451016948f	\N	f	f	\N	t	0	\N	2026-01-06 19:11:03.343345+00	2026-01-06 19:17:58.76174+00	\N	1	\N	\N
62b4318c-3900-4a8d-a24b-b705f4b30884	dealer1	DEALER1	dealer@test.com	DEALER@TEST.COM	t	AQAAAAIAAYagAAAAECxl2sYe4gdGlaeubyavN/iaQ6PzNd8zhAQC/PANi0m8e6vk6reH1tIqH8MDn0f9Gg==	TG22OYMQBU3GPQ5HH42AUDOCJDP5ZT35	2f9f8741-7ff5-42d4-ae11-58ab42a7578d	\N	f	f	\N	t	0	\N	2026-01-06 19:11:10.013764+00	2026-01-06 19:25:57.654846+00	\N	1	\N	\N
798a2ebf-4ab0-46c4-8289-ba2da6ccc955	synctest	SYNCTEST	synctest@example.com	SYNCTEST@EXAMPLE.COM	f	AQAAAAIAAYagAAAAEHvs/LoRzKtv4J3ljtF5sjxk8Cl5RJcoUMKP6Z3a+mYiharhaGOTtE2ryRW9WrAqcw==	HC7NPHQSPE2P4Y3ZYYZD74WVGWNNHO3S	0cd71134-cfb4-4e8d-a8b8-f66ce8c26951	\N	f	f	\N	t	0	\N	2026-01-06 19:54:06.862654+00	2026-01-06 19:54:06.867307+00	\N	1	\N	\N
3c62b315-776b-43e1-a2b3-69ff98d9d8ba	rmqtest1767729683	RMQTEST1767729683	rabbitmq_test_1767729683@test.com	RABBITMQ_TEST_1767729683@TEST.COM	f	AQAAAAIAAYagAAAAEJYHo01lyJre5Md5YByykzLxFaOaANp9pWEq63emw3IRiYr1LiDVSM/DFZL00oHvkQ==	YDHPECNUFMV3UQZBSP4KNI5FCRT3YS3O	2ce7cf14-83cd-412e-b3ab-bc91f646a358	\N	f	f	\N	t	0	\N	2026-01-06 20:01:23.343631+00	2026-01-06 20:01:23.348911+00	\N	1	\N	\N
\.


--
-- Data for Name: VerificationTokens; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."VerificationTokens" ("Id", "Token", "Email", "UserId", "Type", "ExpiresAt", "CreatedAt", "IsUsed", "UsedAt") FROM stdin;
6dee9cf2-6589-4ebb-a60d-a50192179e72	0w_nsis58qAsFDa_dmOqIDm7Nz9oMI01oKFPb8ShABw		49b29d84-d936-48a4-b99b-0056d1e3dd2f	1	2026-01-07 18:56:46.795334+00	2026-01-06 18:56:46.795319+00	f	\N
1e3cd4ea-90d4-4f13-af6c-64f99deeca28	UcM1jgSTyIz3aNStF-5q5dmDg_nmaJtFA7NOFiJSYjM		4812c81c-5cb0-42ab-9ada-06b7bd0eadf8	1	2026-01-07 19:02:47.69777+00	2026-01-06 19:02:47.697769+00	f	\N
1c6c6cad-40ad-4c0f-84cc-cac7c712328e	iDaDNHDf2sCOdl_3rV-yJasMSf8pQxi46ADlPgZ_pd4		f9ccc98b-f2cc-4747-b815-5ca95d4b5377	1	2026-01-07 19:11:03.618676+00	2026-01-06 19:11:03.618655+00	f	\N
1d0b5914-3177-4e01-8a3e-7a326642d5b3	UqPWUZQ8Wf5PQT3NBQGtZ_7iuQbA-A7X8BAHQr6Zqhc		62b4318c-3900-4a8d-a24b-b705f4b30884	1	2026-01-07 19:11:10.023721+00	2026-01-06 19:11:10.023721+00	f	\N
c1907dd4-716d-494d-b339-a8f29375ed75	m4w-5uPp6G2QEOtujOJOy09Xdloa_A74qbDeF4_L4bM		7e5d177e-058a-4b30-ab7f-af3f00594917	1	2026-01-07 19:11:16.458548+00	2026-01-06 19:11:16.458548+00	f	\N
962d681e-5b97-4333-b7a3-e1113b998ba9	CZTmrqQ4wX9hrLlGKmy9GQLAw0oPEiws1pVhkxzVxdQ		798a2ebf-4ab0-46c4-8289-ba2da6ccc955	1	2026-01-07 19:54:06.87419+00	2026-01-06 19:54:06.87419+00	f	\N
a15484f9-9e90-4b45-878e-23537985d754	MXl9EU-8o2CunGGd_kkTw0IX4IP4HfQDoS8R6UQ8vyc		3c62b315-776b-43e1-a2b3-69ff98d9d8ba	1	2026-01-07 20:01:23.361234+00	2026-01-06 20:01:23.361234+00	f	\N
91f31374-61b8-4a3c-a391-7ceca39c081e	KhjFSQquDKMM0f-7iMjb8iWNcyQFnuPZubJC0lRcXGY		8be894d1-e151-4a7d-8918-06fed494bc1a	1	2026-01-09 10:03:00.187672+00	2026-01-08 10:03:00.187657+00	f	\N
6cf29595-fbb4-4f90-a044-8068f1008f95	U-oEAyKmcfxroLo_TxdG2qdX9b1abYQggHCLI0wGxY4		932845d1-a6c2-46ba-8f7c-8285ff1b2930	1	2026-01-09 10:04:03.586737+00	2026-01-08 10:04:03.586736+00	f	\N
7544e1d5-1a01-402f-ae1e-4f390143e243	qTGl2BOaIlTDIyYhEolE17F6tp3Uc_BzydWgjTeKgwI		ad73cdbc-9cce-4457-ae8d-7e4bc3522a80	1	2026-01-09 10:27:38.793024+00	2026-01-08 10:27:38.793024+00	f	\N
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250618160312_InitAuthService	8.0.11
\.


--
-- Name: RefreshTokens PK_RefreshTokens; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RefreshTokens"
    ADD CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: VerificationTokens PK_VerificationTokens; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VerificationTokens"
    ADD CONSTRAINT "PK_VerificationTokens" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_RefreshTokens_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_RefreshTokens_UserId" ON public."RefreshTokens" USING btree ("UserId");


--
-- Name: IX_Users_Email; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Users_Email" ON public."Users" USING btree ("Email");


--
-- Name: IX_VerificationTokens_Token; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_VerificationTokens_Token" ON public."VerificationTokens" USING btree ("Token");


--
-- Name: IX_VerificationTokens_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_VerificationTokens_UserId" ON public."VerificationTokens" USING btree ("UserId");


--
-- Name: RefreshTokens FK_RefreshTokens_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."RefreshTokens"
    ADD CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: VerificationTokens FK_VerificationTokens_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."VerificationTokens"
    ADD CONSTRAINT "FK_VerificationTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict 3YMM0kL6ElDks4RmkSrRCfEopz36pedfLlkkoBCnkNHpRc4SNbDG3jPCWVb6AcJ

