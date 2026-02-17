using ChatbotService.Domain.Entities;
using ChatbotService.Domain.Enums;
using ChatbotService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatbotService.Api.Services;

/// <summary>
/// Seeds the database with test data for 2 dealers with different chatbot personalities.
/// Only runs when the database is empty (no configurations exist).
/// </summary>
public static class ChatbotDataSeeder
{
    // Fixed IDs for reproducibility
    private static readonly Guid Dealer1ConfigId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid Dealer2ConfigId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid Dealer1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Dealer2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(ChatbotDbContext db)
    {
        if (await db.ChatbotConfigurations.AnyAsync())
        {
            return; // Already seeded
        }

        // 1. Chatbot Configurations
        var configs = CreateConfigurations();
        db.ChatbotConfigurations.AddRange(configs);
        await db.SaveChangesAsync();

        // 2. Vehicles
        var vehicles = CreateVehicles();
        db.ChatbotVehicles.AddRange(vehicles);
        await db.SaveChangesAsync();

        // 3. Quick Responses
        var responses = CreateQuickResponses();
        db.QuickResponses.AddRange(responses);
        await db.SaveChangesAsync();
    }

    private static List<ChatbotConfiguration> CreateConfigurations()
    {
        return new List<ChatbotConfiguration>
        {
            // ── Dealer 1: Auto Dominicana Premium — Formal, premium ──
            new ChatbotConfiguration
            {
                Id = Dealer1ConfigId,
                DealerId = Dealer1Id,
                Name = "Auto Dominicana Premium Chatbot",
                IsActive = true,
                Plan = ChatbotPlan.Standard,
                FreeInteractionsPerMonth = 500,
                CostPerInteraction = 0.05m,
                MaxInteractionsPerSession = 25,
                MaxInteractionsPerUserPerDay = 100,
                MaxInteractionsPerUserPerMonth = 1000,
                MaxGlobalInteractionsPerDay = 10000,
                MaxGlobalInteractionsPerMonth = 200000,
                BotName = "Ana",
                BotAvatarUrl = string.Empty,
                WelcomeMessage = "¡Bienvenido a Auto Dominicana Premium! 🚗✨ Soy Ana, tu asistente virtual. Estoy aquí para ayudarte a encontrar el vehículo perfecto. ¿En qué puedo servirte hoy?",
                OfflineMessage = "Estamos fuera de horario. Nuestro equipo te atenderá en horario laboral: Lun-Vie 8AM-6PM, Sáb 9AM-2PM.",
                LimitReachedMessage = "Has alcanzado el límite de interacciones para esta sesión. ¿Te gustaría que un asesor te contacte directamente?",
                EnableWebChat = true,
                EnableAutoInventorySync = true,
                EnableAutoReports = true,
                EnableAutoLearning = true,
                EnableHealthMonitoring = true,
                TimeZone = "America/Santo_Domingo",
                LlmProjectId = "okla-llm",
                LlmModelId = "okla-llama3-8b",
                LanguageCode = "es",
                // System prompt for LLM
                SystemPromptText = SystemPromptDealer1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            // ── Dealer 2: MotorMax RD — Informal, accesible ──
            new ChatbotConfiguration
            {
                Id = Dealer2ConfigId,
                DealerId = Dealer2Id,
                Name = "MotorMax RD Chatbot",
                IsActive = true,
                Plan = ChatbotPlan.Standard,
                FreeInteractionsPerMonth = 300,
                CostPerInteraction = 0.05m,
                MaxInteractionsPerSession = 20,
                MaxInteractionsPerUserPerDay = 80,
                MaxInteractionsPerUserPerMonth = 800,
                MaxGlobalInteractionsPerDay = 8000,
                MaxGlobalInteractionsPerMonth = 150000,
                BotName = "Carlos",
                BotAvatarUrl = string.Empty,
                WelcomeMessage = "¡Klk mi pana! 🔥 Soy Carlos de MotorMax RD. Tenemos los mejores precios en carros usados y nuevos. ¿Qué andas buscando?",
                OfflineMessage = "Tamo cerrado ahorita. Vuelve Lun-Sáb 9AM-7PM. ¡Te esperamos! 🤙",
                LimitReachedMessage = "Ya llegaste al límite de preguntas por ahora. Déjame tu número y te llamamos pa' ayudarte mejor.",
                EnableWebChat = true,
                EnableWhatsApp = true,
                EnableAutoInventorySync = true,
                EnableAutoReports = true,
                EnableAutoLearning = true,
                EnableHealthMonitoring = true,
                TimeZone = "America/Santo_Domingo",
                LlmProjectId = "okla-llm",
                LlmModelId = "okla-llama3-8b",
                LanguageCode = "es",
                SystemPromptText = SystemPromptDealer2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        };
    }

    private static List<ChatbotVehicle> CreateVehicles()
    {
        var now = DateTime.UtcNow;
        return new List<ChatbotVehicle>
        {
            // ════════════════════════════════════════════
            // Dealer 1: Auto Dominicana Premium (8 vehículos)
            // ════════════════════════════════════════════
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Toyota", Model = "RAV4", Year = 2024, Trim = "XLE", Color = "Blanco Perla",
                Price = 2_850_000, BodyType = "SUV", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 5000, EngineSize = "2.5L",
                Description = "Toyota RAV4 2024 XLE en excelente estado. Safety Sense 3.0, pantalla táctil 10.5\", Android Auto/Apple CarPlay, cámara 360°. Garantía de fábrica vigente.",
                SearchableText = "toyota rav4 2024 xle blanco suv gasolina automatica yipeta",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Hyundai", Model = "Tucson", Year = 2024, Trim = "SEL", Color = "Negro",
                Price = 2_450_000, BodyType = "SUV", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 3000, EngineSize = "2.5L",
                Description = "Hyundai Tucson 2024 SEL. Diseño moderno, BlueLink connected services, asientos calefactados, techo panorámico.",
                SearchableText = "hyundai tucson 2024 sel negro suv gasolina automatica yipeta",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Honda", Model = "CR-V", Year = 2023, Trim = "EX-L", Color = "Gris Plateado",
                Price = 3_100_000, BodyType = "SUV", FuelType = "Gasolina", Transmission = "CVT",
                Mileage = 8000, EngineSize = "1.5L Turbo",
                Description = "Honda CR-V 2023 EX-L con motor turbo. Interior en cuero, Honda Sensing suite completo, wireless CarPlay.",
                SearchableText = "honda cr-v crv 2023 exl gris suv gasolina cvt turbo yipeta",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Kia", Model = "Sportage", Year = 2024, Trim = "LX", Color = "Rojo",
                Price = 1_950_000, BodyType = "SUV", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 4000, EngineSize = "2.5L",
                Description = "Kia Sportage 2024 LX. Excelente relación precio-calidad. Cámara trasera, sensores de estacionamiento, pantalla 8\".",
                SearchableText = "kia sportage 2024 lx rojo suv gasolina automatica yipeta barato economico",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Toyota", Model = "Corolla", Year = 2024, Trim = "SE", Color = "Azul Celestial",
                Price = 1_650_000, BodyType = "Sedán", FuelType = "Gasolina", Transmission = "CVT",
                Mileage = 2000, EngineSize = "2.0L",
                Description = "Toyota Corolla 2024 SE. El sedán más confiable del mercado. TSS 3.0, eficiente en combustible, bajo mantenimiento.",
                SearchableText = "toyota corolla 2024 se azul sedan gasolina cvt carro economico",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Honda", Model = "Civic", Year = 2024, Trim = "Sport", Color = "Negro",
                Price = 1_800_000, BodyType = "Sedán", FuelType = "Gasolina", Transmission = "CVT",
                Mileage = 1500, EngineSize = "2.0L",
                Description = "Honda Civic 2024 Sport. Diseño deportivo, Bose premium audio, Honda Sensing, aro 18\".",
                SearchableText = "honda civic 2024 sport negro sedan gasolina cvt carro deportivo",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Mitsubishi", Model = "L200", Year = 2024, Trim = "GLS", Color = "Plateado",
                Price = 2_600_000, BodyType = "Pickup", FuelType = "Diesel", Transmission = "Manual",
                Mileage = 3500, EngineSize = "2.4L Diesel",
                Description = "Mitsubishi L200 2024 GLS 4x4. Ideal para trabajo y aventura. Super Select 4WD, capacidad de carga 1 tonelada.",
                SearchableText = "mitsubishi l200 2024 gls plateado pickup diesel manual camioneta 4x4",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Hyundai", Model = "Sonata", Year = 2023, Trim = "Limited", Color = "Blanco Perla",
                Price = 2_200_000, OriginalPrice = 2_400_000, IsOnSale = true,
                BodyType = "Sedán", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 12000, EngineSize = "2.5L",
                Description = "Hyundai Sonata 2023 Limited ¡EN OFERTA! Interior en cuero Nappa, techo panorámico, Bose 12 altavoces, HTRAC AWD.",
                SearchableText = "hyundai sonata 2023 limited blanco sedan gasolina automatica lujo oferta",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },

            // ════════════════════════════════════════════
            // Dealer 2: MotorMax RD (7 vehículos, más económicos)
            // ════════════════════════════════════════════
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Toyota", Model = "Hilux", Year = 2022, Trim = "SR5", Color = "Blanco",
                Price = 2_100_000, OriginalPrice = 2_300_000, IsOnSale = true,
                BodyType = "Pickup", FuelType = "Diesel", Transmission = "Automática",
                Mileage = 25000, EngineSize = "2.8L Diesel",
                Description = "Toyota Hilux 2022 SR5 4x4 diesel. Camioneta de trabajo probada. Aire acondicionado, bluetooth, cámara trasera.",
                SearchableText = "toyota hilux 2022 sr5 blanco pickup diesel automatica camioneta 4x4 trabajo oferta",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Hyundai", Model = "Accent", Year = 2023, Trim = "GL", Color = "Gris",
                Price = 1_050_000, BodyType = "Sedán", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 15000, EngineSize = "1.6L",
                Description = "Hyundai Accent 2023 GL. El carro más pela'o del lote. Aire acondicionado, dirección hidráulica, buena economía.",
                SearchableText = "hyundai accent 2023 gl gris sedan gasolina automatica carro barato pelao economico",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Kia", Model = "K5", Year = 2023, Trim = "GT-Line", Color = "Rojo",
                Price = 1_750_000, OriginalPrice = 1_900_000, IsOnSale = true,
                BodyType = "Sedán", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 18000, EngineSize = "1.6L Turbo",
                Description = "Kia K5 2023 GT-Line turbo. ¡Un chivo de carro! Pantalla 10.25\", techo panorámico, motor turbo con power.",
                SearchableText = "kia k5 2023 gt-line rojo sedan gasolina automatica turbo deportivo chivo oferta",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Suzuki", Model = "Vitara", Year = 2022, Trim = "GLX", Color = "Verde",
                Price = 1_450_000, BodyType = "SUV", FuelType = "Gasolina", Transmission = "Automática",
                Mileage = 20000, EngineSize = "1.4L Turbo",
                Description = "Suzuki Vitara 2022 GLX. Yipeta compacta y económica. AllGrip 4WD, pantalla táctil, buen consumo.",
                SearchableText = "suzuki vitara 2022 glx verde suv gasolina automatica yipeta compacta barata 4x4",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Toyota", Model = "Yaris", Year = 2021, Trim = "XLE", Color = "Blanco",
                Price = 850_000, BodyType = "Sedán", FuelType = "Gasolina", Transmission = "CVT",
                Mileage = 35000, EngineSize = "1.5L",
                Description = "Toyota Yaris 2021 XLE. ¡El más económico! Perfecto para el día a día, bajo consumo, Toyota confiable.",
                SearchableText = "toyota yaris 2021 xle blanco sedan gasolina cvt carro barato pelao economico confiable",
                IsAvailable = true, IsFeatured = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Nissan", Model = "Kicks", Year = 2023, Trim = "SR", Color = "Naranja/Negro",
                Price = 1_350_000, BodyType = "SUV", FuelType = "Gasolina", Transmission = "CVT",
                Mileage = 12000, EngineSize = "1.6L",
                Description = "Nissan Kicks 2023 SR bicolor. Yipeta urbana con estilo. Bose personal audio, Around View Monitor, Safety Shield 360.",
                SearchableText = "nissan kicks 2023 sr naranja suv gasolina cvt yipeta urbana compacta",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId, VehicleId = Guid.NewGuid(),
                Make = "Honda", Model = "HR-V", Year = 2022, Trim = "EX", Color = "Azul",
                Price = 1_550_000, OriginalPrice = 1_650_000, IsOnSale = true,
                BodyType = "SUV", FuelType = "Gasolina", Transmission = "CVT",
                Mileage = 22000, EngineSize = "2.0L",
                Description = "Honda HR-V 2022 EX. Yipeta versátil con Magic Seat. Honda Sensing, Apple CarPlay, excelente espacio interior.",
                SearchableText = "honda hrv hr-v 2022 ex azul suv gasolina cvt yipeta oferta versatil",
                IsAvailable = true, CreatedAt = now, UpdatedAt = now, LastSyncedAt = now,
            },
        };
    }

    private static List<QuickResponse> CreateQuickResponses()
    {
        var now = DateTime.UtcNow;
        return new List<QuickResponse>
        {
            // ── Dealer 1: Auto Dominicana Premium ──
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId,
                Category = "horario", Name = "Horario de atención",
                TriggersJson = """["horario","hora","abierto","cerrado","cuando abren","a que hora"]""",
                Response = "Nuestro horario de atención es: **Lunes a Viernes** de 8:00 AM a 6:00 PM y **Sábados** de 9:00 AM a 2:00 PM. Los domingos estamos cerrados. 📍 Estamos en Av. 27 de Febrero #456, Santo Domingo.",
                Priority = 10, IsActive = true, CreatedAt = now, UpdatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId,
                Category = "financiamiento", Name = "Info financiamiento",
                TriggersJson = """["financiamiento","financiar","credito","prestamo","cuotas","inicial"]""",
                Response = "Trabajamos con **BHD León** y **Banreservas** para ofrecerte las mejores opciones de financiamiento. Un asesor puede darte los detalles específicos según tu perfil. ¿Te gustaría que te contactemos?",
                Priority = 20, IsActive = true, CreatedAt = now, UpdatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer1ConfigId,
                Category = "ubicacion", Name = "Dirección del dealer",
                TriggersJson = """["donde estan","direccion","ubicacion","como llego","mapa"]""",
                Response = "📍 Estamos ubicados en **Av. 27 de Febrero #456, Santo Domingo**. Puedes llamarnos al **809-555-0101** para confirmar tu visita. ¡Te esperamos!",
                Priority = 15, IsActive = true, CreatedAt = now, UpdatedAt = now,
            },

            // ── Dealer 2: MotorMax RD ──
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId,
                Category = "horario", Name = "Horario",
                TriggersJson = """["horario","hora","abierto","cerrado","cuando abren"]""",
                Response = "¡Tamo abierto **Lunes a Sábado de 9AM a 7PM**! 🤙 Domingos descansamos. Pásate por la **Av. Máximo Gómez #789**, Santiago. Llámanos al **809-555-0202**.",
                Priority = 10, IsActive = true, CreatedAt = now, UpdatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId,
                Category = "financiamiento", Name = "Financiamiento",
                TriggersJson = """["financiamiento","financiar","credito","cuotas","inicial"]""",
                Response = "¡Claro que sí! 💰 Financiamos con **Banco Popular**, **BHD** y **Asociación Popular**. Con un inicial desde el 20% te montamos. ¿Cuánto tienes de inicial?",
                Priority = 20, IsActive = true, CreatedAt = now, UpdatedAt = now,
            },
            new()
            {
                Id = Guid.NewGuid(), ChatbotConfigurationId = Dealer2ConfigId,
                Category = "ubicacion", Name = "Dirección",
                TriggersJson = """["donde estan","direccion","ubicacion","como llego"]""",
                Response = "📍 ¡Estamo en la **Av. Máximo Gómez #789, Santiago**! Al lao del Centro Cibao. Llámanos al **809-555-0202** o pásate directo. ¡Te esperamos! 🚗",
                Priority = 15, IsActive = true, CreatedAt = now, UpdatedAt = now,
            },
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // System Prompts per Dealer
    // ═══════════════════════════════════════════════════════════════

    private const string SystemPromptDealer1 = """
Eres Ana, el asistente virtual de Auto Dominicana Premium, un concesionario de vehículos premium en República Dominicana que opera en la plataforma OKLA (okla.com.do).

IDENTIDAD Y PERSONALIDAD:
- Tu nombre es Ana.
- Representas a Auto Dominicana Premium.
- Tu tono es profesional pero cercano, cálido y servicial.
- Hablas en español dominicano neutro — profesional con calidez caribeña.
- Entiendes modismos dominicanos: "yipeta" (SUV), "guagua" (vehículo/bus), "carro" (auto), "motor"/"moto" (motocicleta), "pela'o" (barato), "chivo" (buena oferta), "vaina" (cosa), "tato" (ok/de acuerdo).
- NUNCA inventes información. Si no sabes algo, ofrece conectar con un agente.
- Sé conciso: respuestas de 2-4 oraciones máximo.
- Usa emojis moderadamente (1-2 por mensaje).

REGLAS:
1. Responde SIEMPRE en español dominicano amigable y profesional.
2. Responde SIEMPRE en formato JSON con los campos: response, intent, confidence, isFallback, parameters, leadSignals, suggestedAction, quickReplies.
3. NUNCA inventes precios, especificaciones o datos que no estén en el inventario proporcionado.
4. NUNCA des asesoría legal, financiera vinculante o diagnósticos mecánicos.
5. Respeta la Ley 358-05 (Protección al Consumidor), Ley 172-13 (Protección de Datos) y normativas DGII.
6. Si no sabes algo, admítelo y ofrece conectar con un asesor humano.
7. Detecta señales de compra (presupuesto, test drive, financiamiento, datos de contacto).
8. Máximo 3-4 vehículos por respuesta de búsqueda.
9. Si el usuario dice que NO quiere financiamiento o que paga al contado, NO ofrezcas financiamiento.
10. Para especificaciones técnicas, usa SOLO los datos del INVENTARIO DISPONIBLE. Si un dato no aparece, redirige al asesor.

REGLAS DE INVENTARIO (CRÍTICAS — anti-alucinación):
11. SOLO puedes recomendar, mencionar o detallar vehículos que aparezcan en INVENTARIO DISPONIBLE.
12. Si el usuario pregunta por una marca, modelo o tipo de vehículo que NO está en INVENTARIO DISPONIBLE, di claramente "Actualmente no tenemos [marca/modelo] en nuestro inventario" y sugiere alternativas del inventario.
13. NUNCA inventes vehículos, precios, especificaciones, colores ni características que no estén explícitamente listados en INVENTARIO DISPONIBLE.
14. Si el usuario pregunta por una especificación (HP, torque, puertas, asientos, tracción, etc.) que NO aparece en los datos del inventario, di "Esa información no está disponible en mi sistema, puedo conectarte con un asesor que te la confirme."
15. Cuando presentes vehículos, usa EXACTAMENTE los precios y datos del INVENTARIO DISPONIBLE — nunca redondees ni aproximes.
16. Si no hay vehículos en el inventario que coincidan con lo que busca el usuario, dilo honestamente y ofrece mostrar lo que SÍ tienes disponible.

PROHIBICIONES LEGALES (República Dominicana):
- NUNCA facilites evasión fiscal (Ley 11-92 / DGII). Toda venta DEBE facturarse con ITBIS y NCF.
- NUNCA aceptes transacciones anónimas ni sin identificación (Ley 155-17 contra Lavado de Activos).
- NUNCA compartas datos personales de clientes (Ley 172-13 de Protección de Datos).
- NUNCA ocultes defectos ni hagas publicidad engañosa (Ley 358-05 Pro-Consumidor).
- NUNCA falsifiques documentos, kilometraje ni historial vehicular.
- NUNCA facilites la falsificación de documentos de ningún tipo.
- NUNCA discrimines por nacionalidad, género, edad o condición (Constitución Art. 39).
- NUNCA vendas vehículos sin documentación legal completa (matrícula, marbete, traspaso, seguro).
- Toda información de clientes es confidencial y no se comparte.
- Si el usuario solicita algo ilegal, rechaza la solicitud con cortesía, cita la ley aplicable y redirige a alternativas legales.

INFORMACIÓN DEL DEALER:
- Nombre: Auto Dominicana Premium
- Ubicación: Av. 27 de Febrero #456, Santo Domingo
- Teléfono: 809-555-0101
- Horario: Lunes-Viernes 8:00-18:00, Sábados 9:00-14:00
- Financiamiento con: BHD León, Banreservas
- Trade-in: Sí
""";

    private const string SystemPromptDealer2 = """
Eres Carlos, el asistente virtual de MotorMax RD, un dealer de carros nuevos y usados en Santiago, República Dominicana. Operas en la plataforma OKLA (okla.com.do).

IDENTIDAD Y PERSONALIDAD:
- Tu nombre es Carlos.
- Representas a MotorMax RD.
- Tu tono es informal, amigable y entusiasta — como un pana que sabe de carros.
- Hablas en español dominicano coloquial — usas expresiones como "klk", "tato", "dimelo", "tranqui".
- Conoces bien el argot: "yipeta" (SUV), "guagua" (vehículo), "pela'o" (barato), "chivo" (buena oferta), "un palo" (un millón de pesos), "vaina" (cosa).
- NUNCA inventes precios o datos. Si no sabes, dilo y ofrece conectar con un vendedor.
- Sé conciso: respuestas directas de 2-4 oraciones.
- Usa emojis con frecuencia (2-3 por mensaje) 🔥🚗💰.

REGLAS:
1. Responde SIEMPRE en español dominicano amigable y profesional.
2. Responde SIEMPRE en formato JSON con los campos: response, intent, confidence, isFallback, parameters, leadSignals, suggestedAction, quickReplies.
3. NUNCA inventes precios, especificaciones o datos que no estén en el inventario proporcionado.
4. NUNCA des asesoría legal, financiera vinculante o diagnósticos mecánicos.
5. Respeta la Ley 358-05 (Protección al Consumidor), Ley 172-13 (Protección de Datos) y normativas DGII.
6. Si no sabes algo, admítelo y ofrece conectar con un asesor humano.
7. Detecta señales de compra (presupuesto, test drive, financiamiento, datos de contacto).
8. Máximo 3-4 vehículos por respuesta de búsqueda.
9. Si el usuario dice que NO quiere financiamiento o que paga al contado, NO ofrezcas financiamiento.
10. Para especificaciones técnicas, usa SOLO los datos del INVENTARIO DISPONIBLE. Si un dato no aparece, redirige al asesor.

REGLAS DE INVENTARIO (CRÍTICAS — anti-alucinación):
11. SOLO puedes recomendar, mencionar o detallar vehículos que aparezcan en INVENTARIO DISPONIBLE.
12. Si el usuario pregunta por una marca, modelo o tipo de vehículo que NO está en INVENTARIO DISPONIBLE, di claramente "Actualmente no tenemos [marca/modelo] en nuestro inventario" y sugiere alternativas del inventario.
13. NUNCA inventes vehículos, precios, especificaciones, colores ni características que no estén explícitamente listados en INVENTARIO DISPONIBLE.
14. Si el usuario pregunta por una especificación (HP, torque, puertas, asientos, tracción, etc.) que NO aparece en los datos del inventario, di "Esa información no está disponible en mi sistema, puedo conectarte con un asesor que te la confirme."
15. Cuando presentes vehículos, usa EXACTAMENTE los precios y datos del INVENTARIO DISPONIBLE — nunca redondees ni aproximes.
16. Si no hay vehículos en el inventario que coincidan con lo que busca el usuario, dilo honestamente y ofrece mostrar lo que SÍ tienes disponible.

PROHIBICIONES LEGALES (República Dominicana):
- NUNCA facilites evasión fiscal (Ley 11-92 / DGII). Toda venta DEBE facturarse con ITBIS y NCF.
- NUNCA aceptes transacciones anónimas ni sin identificación (Ley 155-17 contra Lavado de Activos).
- NUNCA compartas datos personales de clientes (Ley 172-13 de Protección de Datos).
- NUNCA ocultes defectos ni hagas publicidad engañosa (Ley 358-05 Pro-Consumidor).
- NUNCA falsifiques documentos, kilometraje ni historial vehicular.
- NUNCA facilites la falsificación de documentos de ningún tipo.
- NUNCA discrimines por nacionalidad, género, edad o condición (Constitución Art. 39).
- NUNCA vendas vehículos sin documentación legal completa (matrícula, marbete, traspaso, seguro).
- Toda información de clientes es confidencial y no se comparte.
- Si el usuario solicita algo ilegal, rechaza la solicitud con cortesía, cita la ley aplicable y redirige a alternativas legales.

INFORMACIÓN DEL DEALER:
- Nombre: MotorMax RD
- Ubicación: Av. Máximo Gómez #789, Santiago
- Teléfono: 809-555-0202
- Horario: Lunes-Sábado 9:00-19:00
- Financiamiento con: Banco Popular, BHD, Asociación Popular
- Trade-in: Sí
""";
}
