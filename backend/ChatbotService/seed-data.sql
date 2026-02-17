-- ============================================================
-- OKLA ChatbotService — Seed Data para Pruebas
-- ============================================================
-- Crea 2 dealers con personalidades diferentes, sus vehículos,
-- y configuraciones del chatbot.
--
-- Dealer 1: "Auto Dominicana Premium" — Formal, premium
-- Dealer 2: "MotorMax RD" — Informal, accesible, popular
-- ============================================================

-- ── Extensión UUID ────────────────────────────────────────
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================
-- 1. CONFIGURACIONES DE CHATBOT (una por dealer)
-- ============================================================

INSERT INTO chatbot_configurations (
    "Id", "DealerId", "Name", "IsActive",
    "Plan", "FreeInteractionsPerMonth", "CostPerInteraction",
    "MaxInteractionsPerSession", "MaxInteractionsPerUserPerDay",
    "MaxInteractionsPerUserPerMonth", "MaxGlobalInteractionsPerDay",
    "MaxGlobalInteractionsPerMonth",
    "BotName", "BotAvatarUrl", "WelcomeMessage", "OfflineMessage",
    "LimitReachedMessage",
    "EnableWebChat", "EnableWhatsApp", "EnableFacebook", "EnableInstagram",
    "EnableAutoInventorySync", "EnableAutoReports", "EnableAutoLearning",
    "EnableHealthMonitoring",
    "TimeZone",
    "CreatedAt", "UpdatedAt",
    "LlmProjectId", "LlmModelId", "LanguageCode"
) VALUES
-- ── Dealer 1: Auto Dominicana Premium ─────────────────────
(
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    '11111111-1111-1111-1111-111111111111',
    'Auto Dominicana Premium Chatbot',
    true,
    0, -- Plan: Free/Basic
    500, 0.05,
    25, 100, 1000, 10000, 200000,
    'Ana', NULL,
    '¡Bienvenido a Auto Dominicana Premium! 🚗✨ Soy Ana, tu asistente virtual. Estoy aquí para ayudarte a encontrar el vehículo perfecto. ¿En qué puedo servirte hoy?',
    'Estamos fuera de horario. Nuestro equipo te atenderá en horario laboral: Lun-Vie 8AM-6PM, Sáb 9AM-2PM.',
    'Has alcanzado el límite de interacciones para esta sesión. ¿Te gustaría que un asesor te contacte directamente?',
    true, false, false, false,
    true, true, true, true,
    'America/Santo_Domingo',
    NOW(), NOW(),
    'okla-llm', 'okla-llama3-8b', 'es'
),
-- ── Dealer 2: MotorMax RD ─────────────────────────────────
(
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    '22222222-2222-2222-2222-222222222222',
    'MotorMax RD Chatbot',
    true,
    0,
    300, 0.05,
    20, 80, 800, 8000, 150000,
    'Carlos', NULL,
    '¡Klk mi pana! 🔥 Soy Carlos de MotorMax RD. Tenemos los mejores precios en carros usados y nuevos. ¿Qué andas buscando?',
    'Tamo cerrado ahorita. Vuelve Lun-Sáb 9AM-7PM. ¡Te esperamos! 🤙',
    'Ya llegaste al límite de preguntas por ahora. Déjame tu número y te llamamos pa'' ayudarte mejor.',
    true, true, false, false,
    true, true, true, true,
    'America/Santo_Domingo',
    NOW(), NOW(),
    'okla-llm', 'okla-llama3-8b', 'es'
)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================================
-- 2. VEHÍCULOS — Dealer 1: Auto Dominicana Premium
-- ============================================================

INSERT INTO chatbot_vehicles (
    "Id", "ChatbotConfigurationId", "VehicleId",
    "Make", "Model", "Year", "Trim", "Color",
    "Price", "OriginalPrice",
    "BodyType", "FuelType", "Transmission",
    "Mileage", "EngineSize",
    "Description", "SearchableText",
    "MainImageUrl",
    "IsAvailable", "IsFeatured",
    "ViewCount", "InquiryCount",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Toyota RAV4 2024
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Toyota', 'RAV4', 2024, 'XLE', 'Blanco Perla',
    2850000, NULL,
    'SUV', 'Gasolina', 'Automática',
    5000, '2.5L',
    'Toyota RAV4 2024 XLE en excelente estado. Safety Sense 3.0, pantalla táctil 10.5", Android Auto/Apple CarPlay, cámara 360°. Garantía de fábrica vigente.',
    'toyota rav4 2024 xle blanco suv gasolina automatica yipeta',
    'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Hyundai Tucson 2024
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Hyundai', 'Tucson', 2024, 'SEL', 'Negro',
    2450000, NULL,
    'SUV', 'Gasolina', 'Automática',
    3000, '2.5L',
    'Hyundai Tucson 2024 SEL. Diseño moderno, BlueLink connected services, asientos calefactados, techo panorámico.',
    'hyundai tucson 2024 sel negro suv gasolina automatica yipeta',
    'https://images.unsplash.com/photo-1633695446032-2d3f58c31bdb?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Honda CR-V 2023
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Honda', 'CR-V', 2023, 'EX-L', 'Gris Plateado',
    3100000, NULL,
    'SUV', 'Gasolina', 'CVT',
    8000, '1.5L Turbo',
    'Honda CR-V 2023 EX-L con motor turbo. Interior en cuero, Honda Sensing suite completo, wireless CarPlay.',
    'honda cr-v crv 2023 exl gris suv gasolina cvt turbo yipeta',
    'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=800',
    true, false, 0, 0, NOW(), NOW()
),
-- Kia Sportage 2024
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Kia', 'Sportage', 2024, 'LX', 'Rojo',
    1950000, NULL,
    'SUV', 'Gasolina', 'Automática',
    4000, '2.5L',
    'Kia Sportage 2024 LX. Excelente relación precio-calidad. Cámara trasera, sensores de estacionamiento, pantalla 8".',
    'kia sportage 2024 lx rojo suv gasolina automatica yipeta barato economico',
    'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Toyota Corolla 2024
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Toyota', 'Corolla', 2024, 'SE', 'Azul Celestial',
    1650000, NULL,
    'Sedán', 'Gasolina', 'CVT',
    2000, '2.0L',
    'Toyota Corolla 2024 SE. El sedán más confiable del mercado. TSS 3.0, eficiente en combustible, bajo mantenimiento.',
    'toyota corolla 2024 se azul sedan gasolina cvt carro economico',
    'https://images.unsplash.com/photo-1623869675781-80aa31012a5a?w=800',
    true, false, 0, 0, NOW(), NOW()
),
-- Honda Civic 2024
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Honda', 'Civic', 2024, 'Sport', 'Negro',
    1800000, NULL,
    'Sedán', 'Gasolina', 'CVT',
    1500, '2.0L',
    'Honda Civic 2024 Sport. Diseño deportivo, Bose premium audio, Honda Sensing, aro 18".',
    'honda civic 2024 sport negro sedan gasolina cvt carro deportivo',
    'https://images.unsplash.com/photo-1606016159991-dfe4f2746ad5?w=800',
    true, false, 0, 0, NOW(), NOW()
),
-- Mitsubishi L200 2024
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Mitsubishi', 'L200', 2024, 'GLS', 'Plateado',
    2600000, NULL,
    'Pickup', 'Diesel', 'Manual',
    3500, '2.4L Diesel',
    'Mitsubishi L200 2024 GLS 4x4. Ideal para trabajo y aventura. Super Select 4WD, capacidad de carga 1 tonelada.',
    'mitsubishi l200 2024 gls plateado pickup diesel manual camioneta 4x4',
    'https://images.unsplash.com/photo-1559416523-140ddc3d238c?w=800',
    true, false, 0, 0, NOW(), NOW()
),
-- Hyundai Sonata 2023
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    uuid_generate_v4(),
    'Hyundai', 'Sonata', 2023, 'Limited', 'Blanco Perla',
    2200000, 2400000,
    'Sedán', 'Gasolina', 'Automática',
    12000, '2.5L',
    'Hyundai Sonata 2023 Limited ¡EN OFERTA! Interior en cuero Nappa, techo panorámico, Bose 12 altavoces, HTRAC AWD.',
    'hyundai sonata 2023 limited blanco sedan gasolina automatica lujo oferta',
    'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800',
    true, true, 0, 0, NOW(), NOW()
);

-- ============================================================
-- 3. VEHÍCULOS — Dealer 2: MotorMax RD (más económicos)
-- ============================================================

INSERT INTO chatbot_vehicles (
    "Id", "ChatbotConfigurationId", "VehicleId",
    "Make", "Model", "Year", "Trim", "Color",
    "Price", "OriginalPrice",
    "BodyType", "FuelType", "Transmission",
    "Mileage", "EngineSize",
    "Description", "SearchableText",
    "MainImageUrl",
    "IsAvailable", "IsFeatured",
    "ViewCount", "InquiryCount",
    "CreatedAt", "UpdatedAt"
) VALUES
-- Toyota Hilux 2022
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Toyota', 'Hilux', 2022, 'SR5', 'Blanco',
    2100000, 2300000,
    'Pickup', 'Diesel', 'Automática',
    25000, '2.8L Diesel',
    'Toyota Hilux 2022 SR5 4x4 diesel. Camioneta de trabajo probada. Aire acondicionado, bluetooth, cámara trasera.',
    'toyota hilux 2022 sr5 blanco pickup diesel automatica camioneta 4x4 trabajo oferta',
    'https://images.unsplash.com/photo-1559416523-140ddc3d238c?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Hyundai Accent 2023
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Hyundai', 'Accent', 2023, 'GL', 'Gris',
    1050000, NULL,
    'Sedán', 'Gasolina', 'Automática',
    15000, '1.6L',
    'Hyundai Accent 2023 GL. El carro más pela''o del lote. Aire acondicionado, dirección hidráulica, buena economía.',
    'hyundai accent 2023 gl gris sedan gasolina automatica carro barato pelao economico',
    'https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Kia K5 2023
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Kia', 'K5', 2023, 'GT-Line', 'Rojo',
    1750000, 1900000,
    'Sedán', 'Gasolina', 'Automática',
    18000, '1.6L Turbo',
    'Kia K5 2023 GT-Line turbo. ¡Un chivo de carro! Pantalla 10.25", techo panorámico, motor turbo con power.',
    'kia k5 2023 gt-line rojo sedan gasolina automatica turbo deportivo chivo oferta',
    'https://images.unsplash.com/photo-1623869675781-80aa31012a5a?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Suzuki Vitara 2022
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Suzuki', 'Vitara', 2022, 'GLX', 'Verde',
    1450000, NULL,
    'SUV', 'Gasolina', 'Automática',
    20000, '1.4L Turbo',
    'Suzuki Vitara 2022 GLX. Yipeta compacta y económica. AllGrip 4WD, pantalla táctil, buen consumo.',
    'suzuki vitara 2022 glx verde suv gasolina automatica yipeta compacta barata 4x4',
    'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800',
    true, false, 0, 0, NOW(), NOW()
),
-- Toyota Yaris 2021
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Toyota', 'Yaris', 2021, 'XLE', 'Blanco',
    850000, NULL,
    'Sedán', 'Gasolina', 'CVT',
    35000, '1.5L',
    'Toyota Yaris 2021 XLE. ¡El más económico! Perfecto para el día a día, bajo consumo, Toyota confiable.',
    'toyota yaris 2021 xle blanco sedan gasolina cvt carro barato pelao economico confiable',
    'https://images.unsplash.com/photo-1623869675781-80aa31012a5a?w=800',
    true, true, 0, 0, NOW(), NOW()
),
-- Nissan Kicks 2023
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Nissan', 'Kicks', 2023, 'SR', 'Naranja/Negro',
    1350000, NULL,
    'SUV', 'Gasolina', 'CVT',
    12000, '1.6L',
    'Nissan Kicks 2023 SR bicolor. Yipeta urbana con estilo. Bose personal audio, Around View Monitor, Safety Shield 360.',
    'nissan kicks 2023 sr naranja suv gasolina cvt yipeta urbana compacta',
    'https://images.unsplash.com/photo-1609521263047-f8f205293f24?w=800',
    true, false, 0, 0, NOW(), NOW()
),
-- Honda HR-V 2022
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    uuid_generate_v4(),
    'Honda', 'HR-V', 2022, 'EX', 'Azul',
    1550000, 1650000,
    'SUV', 'Gasolina', 'CVT',
    22000, '2.0L',
    'Honda HR-V 2022 EX. Yipeta versátil con Magic Seat. Honda Sensing, Apple CarPlay, excelente espacio interior.',
    'honda hrv hr-v 2022 ex azul suv gasolina cvt yipeta oferta versatil',
    'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=800',
    true, false, 0, 0, NOW(), NOW()
);

-- ============================================================
-- 4. RESPUESTAS RÁPIDAS — Dealer 1: Auto Dominicana Premium
-- ============================================================

INSERT INTO quick_responses (
    "Id", "ChatbotConfigurationId",
    "Category", "Name", "TriggersJson", "Response",
    "Priority", "IsActive", "BypassLlm",
    "UsageCount", "CreatedAt", "UpdatedAt"
) VALUES
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'horario', 'Horario de atención',
    '["horario","hora","abierto","cerrado","cuando abren","a que hora"]',
    'Nuestro horario de atención es: **Lunes a Viernes** de 8:00 AM a 6:00 PM y **Sábados** de 9:00 AM a 2:00 PM. Los domingos estamos cerrados. 📍 Estamos en Av. 27 de Febrero #456, Santo Domingo.',
    10, true, false,
    0, NOW(), NOW()
),
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'financiamiento', 'Info financiamiento',
    '["financiamiento","financiar","credito","prestamo","cuotas","inicial"]',
    'Trabajamos con **BHD León** y **Banreservas** para ofrecerte las mejores opciones de financiamiento. Un asesor puede darte los detalles específicos según tu perfil. ¿Te gustaría que te contactemos?',
    20, true, false,
    0, NOW(), NOW()
),
(
    uuid_generate_v4(),
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
    'ubicacion', 'Dirección del dealer',
    '["donde estan","direccion","ubicacion","como llego","mapa"]',
    '📍 Estamos ubicados en **Av. 27 de Febrero #456, Santo Domingo**. Puedes llamarnos al **809-555-0101** para confirmar tu visita. ¡Te esperamos!',
    15, true, false,
    0, NOW(), NOW()
);

-- ============================================================
-- 5. RESPUESTAS RÁPIDAS — Dealer 2: MotorMax RD
-- ============================================================

INSERT INTO quick_responses (
    "Id", "ChatbotConfigurationId",
    "Category", "Name", "TriggersJson", "Response",
    "Priority", "IsActive", "BypassLlm",
    "UsageCount", "CreatedAt", "UpdatedAt"
) VALUES
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    'horario', 'Horario',
    '["horario","hora","abierto","cerrado","cuando abren"]',
    '¡Tamo abierto **Lunes a Sábado de 9AM a 7PM**! 🤙 Domingos descansamos. Pásate por la **Av. Máximo Gómez #789**, Santiago. Llámanos al **809-555-0202**.',
    10, true, false,
    0, NOW(), NOW()
),
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    'financiamiento', 'Financiamiento',
    '["financiamiento","financiar","credito","cuotas","inicial"]',
    '¡Claro que sí! 💰 Financiamos con **Banco Popular**, **BHD** y **Asociación Popular**. Con un inicial desde el 20% te montamos. ¿Cuánto tienes de inicial?',
    20, true, false,
    0, NOW(), NOW()
),
(
    uuid_generate_v4(),
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',
    'ubicacion', 'Dirección',
    '["donde estan","direccion","ubicacion","como llego"]',
    '📍 ¡Estamo en la **Av. Máximo Gómez #789, Santiago**! Al lao del Centro Cibao. Llámanos al **809-555-0202** o pásate directo. ¡Te esperamos! 🚗',
    15, true, false,
    0, NOW(), NOW()
);

-- ============================================================
-- 6. SYSTEM PROMPTS POR DEALER (almacenados en SystemPromptText)
-- ============================================================

UPDATE chatbot_configurations
SET "SystemPromptText" = 'Eres Ana, el asistente virtual de Auto Dominicana Premium, un concesionario de vehículos premium en República Dominicana que opera dentro de la plataforma OKLA (okla.com.do).

IDENTIDAD Y PERSONALIDAD:
- Tu nombre es Ana.
- Representas a Auto Dominicana Premium.
- Tu tono es profesional pero cercano, cálido y servicial.
- Hablas en español dominicano neutro — profesional con calidez caribeña.
- Entiendes modismos dominicanos: "yipeta" (SUV), "guagua" (vehículo/bus), "carro" (auto), "motor"/"moto" (motocicleta), "pela''o" (barato), "chivo" (buena oferta), "vaina" (cosa), "tato" (ok/de acuerdo).
- NUNCA inventes información. Si no sabes algo, ofrece conectar con un agente.
- Sé conciso: respuestas de 2-4 oraciones máximo.
- Usa emojis moderadamente (1-2 por mensaje).

INFORMACIÓN DEL DEALER:
- Nombre: Auto Dominicana Premium
- Teléfono: 809-555-0101
- Dirección: Av. 27 de Febrero #456, Santo Domingo
- Horarios: Lun-Vie 8AM-6PM, Sáb 9AM-2PM
- Financiamiento: BHD León, Banreservas
- Trade-in: Sí
- Servicio de taller: Sí

CUMPLIMIENTO LEGAL (República Dominicana):
1. Ley 358-05: SIEMPRE mostrar precios en RD$ (DOP). NUNCA decir "precio final". Agregar: "*Precio de referencia sujeto a confirmación."
2. Ley 172-13: NUNCA solicitar cédula, tarjeta de crédito por chat. Si el usuario envía datos sensibles, advertir.
3. DGII: Precios NO incluyen traspaso, ITBIS, primera placa.

FORMATO DE RESPUESTA: Responde en texto natural conversacional. NO uses formato JSON. Usa markdown ligero: **negrita** para nombres de vehículos, listas numeradas (1. 2. 3.) para opciones, y emojis moderados.'
WHERE "Id" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890';

UPDATE chatbot_configurations
SET "SystemPromptText" = 'Eres Carlos, el asistente virtual de MotorMax RD, un dealer de carros nuevos y usados en Santiago, República Dominicana. Operas en la plataforma OKLA (okla.com.do).

IDENTIDAD Y PERSONALIDAD:
- Tu nombre es Carlos.
- Representas a MotorMax RD.
- Tu tono es informal, amigable y entusiasta — como un pana que sabe de carros.
- Hablas en español dominicano coloquial — usas expresiones como "klk", "tato", "dimelo", "tranqui".
- Conoces bien el argot: "yipeta" (SUV), "guagua" (vehículo), "pela''o" (barato), "chivo" (buena oferta), "un palo" (un millón de pesos), "vaina" (cosa).
- Eres directo y vas al grano. No andas con rodeos.
- NUNCA inventes precios o datos. Si no sabes, dilo y ofrece conectar con un vendedor.
- Usa emojis con frecuencia (2-3 por mensaje) 🔥🚗💰.

INFORMACIÓN DEL DEALER:
- Nombre: MotorMax RD
- Teléfono: 809-555-0202
- Dirección: Av. Máximo Gómez #789, Santiago
- Horarios: Lun-Sáb 9AM-7PM
- Financiamiento: Banco Popular, BHD, Asociación Popular
- Trade-in: Sí (recibimos tu carro viejo)
- Especialidad: Carros accesibles y buenas ofertas

CUMPLIMIENTO LEGAL (República Dominicana):
1. Ley 358-05: Precios en RD$ (DOP). No decir "precio final". Agregar disclaimer de referencia.
2. Ley 172-13: No pedir cédula ni tarjetas por chat. Advertir si envían datos sensibles.
3. DGII: Precios no incluyen traspaso ni impuestos.

FORMATO DE RESPUESTA: Responde en texto natural conversacional. NO uses formato JSON. Usa markdown ligero: **negrita** para nombres de vehículos, listas numeradas (1. 2. 3.) para opciones, y emojis con frecuencia 🔥🚗💰.'
WHERE "Id" = 'b2c3d4e5-f6a7-8901-bcde-f12345678901';

-- ============================================================
-- VERIFICACIÓN
-- ============================================================
-- Ejecutar después del seed para verificar:
-- SELECT "Id", "Name", "BotName", "DealerId" FROM chatbot_configurations;
-- SELECT "ChatbotConfigurationId", "Make", "Model", "Year", "Price" FROM chatbot_vehicles ORDER BY "ChatbotConfigurationId", "Price";
-- SELECT "ChatbotConfigurationId", "Category", "Name" FROM quick_responses;
