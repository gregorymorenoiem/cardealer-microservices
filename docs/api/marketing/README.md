# 📧 Marketing APIs

**APIs:** 4 (Mailchimp, Google Ads, Facebook Ads, LinkedIn Ads)  
**Estado:** En Implementación (Fase 2)  
**Prioridad:** 🟠 ALTA

---

## 📖 Resumen

Plataformas de marketing y publicidad para alcanzar compradores y expandir ventas a través de múltiples canales (email, search, social).

### Casos de Uso en OKLA

✅ **Email Marketing:** Newsletters a compradores y dealers  
✅ **Google Ads:** Campañas de búsqueda para vehículos específicos  
✅ **Facebook/Instagram Ads:** Retargeting a usuarios que vieron vehículos  
✅ **Lookalike Audiences:** Encontrar compradores similares a los mejores clientes  
✅ **Segmentación:** Campaigns por tipo de vehículo, región, presupuesto  
✅ **Lead Nurturing:** Seguimiento automático de prospectos  
✅ **A/B Testing:** Optimizar mensajes y creativos

---

## 📊 Comparativa de APIs

| API              | Costo Inicial            | Tipo             | ROI Típico | Mejor Para        |
| ---------------- | ------------------------ | ---------------- | ---------- | ----------------- |
| **Mailchimp**    | $0 (free tier)           | Email Marketing  | 300-400%   | Segmentos de 10K+ |
| **Google Ads**   | Variable (CPC: $0.50-$2) | Search + Display | 200-300%   | Búsquedas hot     |
| **Facebook Ads** | Variable (CPM: $5-15)    | Social + Display | 150-250%   | Retargeting       |
| **LinkedIn Ads** | Variable (CPC: $2-5)     | Professional     | 100-200%   | B2B Dealers       |

---

## 🔗 Endpoints por API

### 1. Mailchimp

```
POST   /lists/{list_id}/members              # Subscribir email
GET    /lists/{list_id}/members              # Listar suscriptores
DELETE /lists/{list_id}/members/{email_hash} # Desuscribir
POST   /campaigns                             # Crear campaña
POST   /campaigns/{campaign_id}/actions/send # Enviar campaña
GET    /reports/all-campaigns                # Ver reports
```

### 2. Google Ads

```
GET    /v14/customers/{customer_id}/campaigns         # Listar campañas
POST   /v14/customers/{customer_id}/campaigns         # Crear campaña
GET    /v14/customers/{customer_id}/ad_groups         # Ad groups
POST   /v14/customers/{customer_id}/ads               # Crear ads
GET    /v14/customers/{customer_id}/google_ads_fields # Reporting
```

### 3. Facebook Ads

```
GET    /v18.0/{user_id}/adaccounts             # Cuentas de ads
POST   /v18.0/{ad_account_id}/campaigns        # Crear campaña
POST   /v18.0/{ad_account_id}/adsets           # Ad sets
POST   /v18.0/{ad_account_id}/ads              # Crear ads
GET    /v18.0/{campaign_id}/insights           # Analytics
```

### 4. LinkedIn Ads

```
POST   /v2/adAccounts/{account_id}/campaigns             # Crear campaña
GET    /v2/adAccounts/{account_id}/campaigns             # Listar
POST   /v2/adAccounts/{account_id}/adCreatives          # Creative
POST   /v2/adAccounts/{account_id}/adCampaignGroups     # Campaign groups
GET    /v2/adAccounts/{account_id}/adAnalytics          # Reporting
```

---

## 💻 Implementación Backend (C#)

### Service Interface

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKLA.MarketingService.Domain.Interfaces
{
    /// <summary>
    /// Interface para integración con Mailchimp
    /// </summary>
    public interface IMailchimpService
    {
        /// <summary>
        /// Suscribir email a lista de Mailchimp
        /// </summary>
        Task<SubscriberResponse> SubscribeAsync(string email, string firstName, string lastName);

        /// <summary>
        /// Obtener lista de suscriptores
        /// </summary>
        Task<List<SubscriberDto>> GetSubscribersAsync(int pageSize = 100, int page = 0);

        /// <summary>
        /// Crear y enviar campaña de email
        /// </summary>
        Task<CampaignResponse> CreateAndSendCampaignAsync(
            string campaignName,
            string subject,
            string htmlContent,
            List<string> tags);

        /// <summary>
        /// Obtener analytics de campaña
        /// </summary>
        Task<CampaignAnalytics> GetCampaignAnalyticsAsync(string campaignId);

        /// <summary>
        /// Desuscribir email
        /// </summary>
        Task<bool> UnsubscribeAsync(string email);
    }

    /// <summary>
    /// Interface para Google Ads
    /// </summary>
    public interface IGoogleAdsService
    {
        /// <summary>
        /// Crear campaña de búsqueda
        /// </summary>
        Task<CampaignResponse> CreateSearchCampaignAsync(
            string campaignName,
            decimal dailyBudget,
            string[] keywords);

        /// <summary>
        /// Crear anuncio
        /// </summary>
        Task<AdResponse> CreateAdAsync(
            string campaignId,
            string headline,
            string description,
            string finalUrl);

        /// <summary>
        /// Obtener métricas de campaña
        /// </summary>
        Task<GoogleAdsMetrics> GetMetricsAsync(string campaignId, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Interface para Facebook Ads
    /// </summary>
    public interface IFacebookAdsService
    {
        /// <summary>
        /// Crear campaña de retargeting
        /// </summary>
        Task<CampaignResponse> CreateRetargetingCampaignAsync(
            string campaignName,
            decimal budget,
            List<string> audienceSegments);

        /// <summary>
        /// Subir audience personalizado
        /// </summary>
        Task<AudienceResponse> UploadCustomAudienceAsync(List<string> emails);

        /// <summary>
        /// Obtener insights de campaña
        /// </summary>
        Task<FacebookAdsMetrics> GetInsightsAsync(string campaignId);
    }
}
```

### Domain Models

```csharp
namespace OKLA.MarketingService.Domain.Entities
{
    public class MarketingCampaign
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; } // "mailchimp", "google_ads", "facebook"
        public string Status { get; set; } // "draft", "running", "completed"
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal Budget { get; set; }
        public decimal? SpentAmount { get; set; }
        public int? Impressions { get; set; }
        public int? Clicks { get; set; }
        public int? Conversions { get; set; }
        public string TargetAudience { get; set; }
        public List<string> Tags { get; set; }
    }

    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
        public string TextContent { get; set; }
        public List<string> Variables { get; set; } // {{first_name}}, {{vehicle_name}}
        public string Category { get; set; } // "new_vehicle", "price_drop", "newsletter"
    }

    public class AudienceSegment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Criteria { get; set; } // JSON con filtros
        public int UserCount { get; set; }
        public List<string> Tags { get; set; }
    }
}
```

### Implementación del Servicio (Mailchimp)

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OKLA.MarketingService.Domain.Interfaces;

namespace OKLA.MarketingService.Infrastructure.Services
{
    public class MailchimpService : IMailchimpService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _dataCenter;
        private readonly string _listId;
        private readonly string _baseUrl;

        public MailchimpService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Mailchimp:ApiKey"];
            _dataCenter = configuration["Mailchimp:DataCenter"]; // us1, us2, etc
            _listId = configuration["Mailchimp:ListId"];
            _baseUrl = $"https://{_dataCenter}.api.mailchimp.com/3.0";
        }

        public async Task<SubscriberResponse> SubscribeAsync(string email, string firstName, string lastName)
        {
            try
            {
                var request = new
                {
                    email_address = email,
                    status = "subscribed",
                    merge_fields = new
                    {
                        FNAME = firstName,
                        LNAME = lastName
                    }
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var httpRequest = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_baseUrl}/lists/{_listId}/members"
                )
                {
                    Content = jsonContent
                };

                // Agregar Authorization header
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"anystring:{_apiKey}"));
                httpRequest.Headers.Add("Authorization", $"Basic {auth}");

                var response = await _httpClient.SendAsync(httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<SubscriberResponse>(content);
                    return result;
                }

                throw new Exception($"Error subscribing: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Mailchimp subscription failed: {ex.Message}", ex);
            }
        }

        public async Task<List<SubscriberDto>> GetSubscribersAsync(int pageSize = 100, int page = 0)
        {
            var subscribers = new List<SubscriberDto>();
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"anystring:{_apiKey}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);

            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/lists/{_listId}/members?count={pageSize}&offset={page * pageSize}"
            );

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(content))
                {
                    var members = doc.RootElement.GetProperty("members");
                    foreach (var member in members.EnumerateArray())
                    {
                        subscribers.Add(new SubscriberDto
                        {
                            Email = member.GetProperty("email_address").GetString(),
                            Status = member.GetProperty("status").GetString(),
                            SubscribedDate = member.GetProperty("timestamp_signup").GetDateTime()
                        });
                    }
                }
            }

            return subscribers;
        }

        public async Task<CampaignResponse> CreateAndSendCampaignAsync(
            string campaignName,
            string subject,
            string htmlContent,
            List<string> tags)
        {
            // Implementación similar...
            throw new NotImplementedException();
        }

        public async Task<CampaignAnalytics> GetCampaignAnalyticsAsync(string campaignId)
        {
            // Implementación similar...
            throw new NotImplementedException();
        }

        public async Task<bool> UnsubscribeAsync(string email)
        {
            // Implementación similar...
            throw new NotImplementedException();
        }
    }

    // Response DTOs
    public class SubscriberResponse
    {
        public string Id { get; set; }
        public string EmailAddress { get; set; }
        public string Status { get; set; }
        public DateTime TimestampSignup { get; set; }
    }

    public class SubscriberDto
    {
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime SubscribedDate { get; set; }
    }

    public class CampaignResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class CampaignAnalytics
    {
        public int Sent { get; set; }
        public int Opens { get; set; }
        public int Clicks { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
    }

    public class GoogleAdsMetrics
    {
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public decimal Cost { get; set; }
        public double Ctr { get; set; }
        public double Cpc { get; set; }
    }

    public class FacebookAdsMetrics
    {
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Actions { get; set; }
        public decimal Spend { get; set; }
    }

    public class AudienceResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int UserCount { get; set; }
    }
}
```

### CQRS Commands

```csharp
using MediatR;
using OKLA.MarketingService.Application.DTOs;

namespace OKLA.MarketingService.Application.Features.Campaigns.Commands
{
    public class CreateEmailCampaignCommand : IRequest<CampaignDto>
    {
        public string Name { get; set; }
        public string TemplateId { get; set; }
        public List<string> SegmentTags { get; set; }
    }

    public class CreateEmailCampaignHandler : IRequestHandler<CreateEmailCampaignCommand, CampaignDto>
    {
        private readonly IMailchimpService _mailchimpService;
        private readonly IRepository<MarketingCampaign> _campaignRepository;

        public async Task<CampaignDto> Handle(CreateEmailCampaignCommand request, CancellationToken cancellationToken)
        {
            // Crear campaña localmente
            var campaign = new MarketingCampaign
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Platform = "mailchimp",
                Status = "draft",
                CreatedAt = DateTime.UtcNow,
                Tags = request.SegmentTags
            };

            await _campaignRepository.AddAsync(campaign, cancellationToken);
            return new CampaignDto { Id = campaign.Id, Name = campaign.Name, Status = campaign.Status };
        }
    }
}
```

---

## 🖥️ Implementación Frontend (TypeScript/React)

### Service API

```typescript
// src/services/marketingService.ts

import axios from "axios";

export interface Campaign {
  id: string;
  name: string;
  platform: "mailchimp" | "google_ads" | "facebook_ads" | "linkedin";
  status: "draft" | "running" | "completed";
  budget: number;
  spent?: number;
  impressions?: number;
  clicks?: number;
  conversions?: number;
  createdAt: Date;
  startedAt?: Date;
}

export interface EmailTemplate {
  id: string;
  name: string;
  subject: string;
  htmlContent: string;
  category: string;
}

export interface AudienceSegment {
  id: string;
  name: string;
  userCount: number;
  tags: string[];
}

export class MarketingService {
  private apiUrl = "/api/marketing";

  // Mailchimp
  async subscribeMail(email: string, firstName: string, lastName: string) {
    return axios.post(`${this.apiUrl}/mailchimp/subscribe`, {
      email,
      firstName,
      lastName,
    });
  }

  async getSubscribers(page: number = 0) {
    return axios.get(`${this.apiUrl}/mailchimp/subscribers?page=${page}`);
  }

  async sendCampaign(
    campaignName: string,
    templateId: string,
    segmentTags: string[]
  ) {
    return axios.post(`${this.apiUrl}/mailchimp/campaigns/send`, {
      campaignName,
      templateId,
      segmentTags,
    });
  }

  // Google Ads
  async createGoogleAdsCampaign(
    name: string,
    keywords: string[],
    budget: number
  ) {
    return axios.post(`${this.apiUrl}/google-ads/campaigns`, {
      name,
      keywords,
      dailyBudget: budget,
    });
  }

  // Facebook Ads
  async createFacebookCampaign(
    name: string,
    budget: number,
    audienceSegments: string[]
  ) {
    return axios.post(`${this.apiUrl}/facebook-ads/campaigns`, {
      name,
      budget,
      audienceSegments,
    });
  }

  // Analytics
  async getCampaignAnalytics(campaignId: string) {
    return axios.get(`${this.apiUrl}/campaigns/${campaignId}/analytics`);
  }
}

export const marketingService = new MarketingService();
```

### React Components

```typescript
// src/components/marketing/CampaignDashboard.tsx

import React, { useState, useEffect } from "react";
import { useQuery, useMutation } from "@tanstack/react-query";
import { marketingService } from "@/services/marketingService";

export const CampaignDashboard = () => {
  const [selectedPlatform, setSelectedPlatform] = useState<
    "mailchimp" | "google_ads"
  >("mailchimp");

  const { data: campaigns, isLoading } = useQuery({
    queryKey: ["campaigns", selectedPlatform],
    queryFn: () => marketingService.getCampaigns(selectedPlatform),
  });

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Marketing Dashboard</h1>

      <div className="flex gap-2">
        <button
          onClick={() => setSelectedPlatform("mailchimp")}
          className={`px-4 py-2 rounded ${
            selectedPlatform === "mailchimp"
              ? "bg-blue-600 text-white"
              : "bg-gray-200"
          }`}
        >
          Mailchimp
        </button>
        <button
          onClick={() => setSelectedPlatform("google_ads")}
          className={`px-4 py-2 rounded ${
            selectedPlatform === "google_ads"
              ? "bg-blue-600 text-white"
              : "bg-gray-200"
          }`}
        >
          Google Ads
        </button>
      </div>

      {isLoading ? (
        <div>Cargando...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {campaigns?.map((campaign) => (
            <div key={campaign.id} className="border rounded-lg p-4">
              <h3 className="font-semibold">{campaign.name}</h3>
              <p className="text-sm text-gray-600">{campaign.status}</p>
              <div className="mt-2 space-y-1">
                {campaign.impressions && (
                  <p>Impresiones: {campaign.impressions}</p>
                )}
                {campaign.clicks && <p>Clicks: {campaign.clicks}</p>}
                {campaign.spent && <p>Gastado: ${campaign.spent}</p>}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
```

---

## 🧪 Testing

### Unit Tests (xUnit)

```csharp
using Xunit;
using Moq;
using OKLA.MarketingService.Infrastructure.Services;

namespace OKLA.MarketingService.Tests
{
    public class MailchimpServiceTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly MailchimpService _service;

        public MailchimpServiceTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _service = new MailchimpService(_mockHttpClient.Object, MockConfiguration());
        }

        [Fact]
        public async Task SubscribeAsync_WithValidEmail_ReturnsSuccessResponse()
        {
            // Arrange
            var email = "user@example.com";
            var firstName = "John";
            var lastName = "Doe";

            // Act
            var result = await _service.SubscribeAsync(email, firstName, lastName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.EmailAddress);
        }

        [Fact]
        public async Task GetSubscribersAsync_ReturnsListOfSubscribers()
        {
            // Act
            var result = await _service.GetSubscribersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<SubscriberDto>>(result);
        }
    }
}
```

### Integration Tests (xUnit)

```csharp
[Collection("Mailchimp Integration")]
public class MailchimpIntegrationTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly IMailchimpService _service;

    public async Task InitializeAsync()
    {
        // Setup
        _httpClient = new HttpClient();
        _service = new MailchimpService(_httpClient, GetConfiguration());
    }

    [Fact]
    public async Task FullCampaignFlow_CreateAndSend_Success()
    {
        // Arrange
        var campaignName = "Test Campaign";
        var templateId = "template-1";
        var tags = new List<string> { "test" };

        // Act
        var result = await _service.CreateAndSendCampaignAsync(
            campaignName,
            "Test Subject",
            "<h1>Test</h1>",
            tags
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("running", result.Status);
    }
}
```

---

## 🔧 Troubleshooting

| Problema                   | Causa                     | Solución                            |
| -------------------------- | ------------------------- | ----------------------------------- |
| **401 Unauthorized**       | API key inválida          | Verificar API key en configuración  |
| **List ID not found**      | List ID incorrecto        | Verificar en dashboard de Mailchimp |
| **Rate limiting**          | Demasiadas requests       | Implementar exponential backoff     |
| **Campaign not sending**   | Subscribers sin confirmar | Verificar status de confirmación    |
| **Google Ads auth failed** | Token expirado            | Refrescar OAuth token               |
| **Facebook timeout**       | API lenta                 | Aumentar timeout a 30s              |

---

## ✅ Integración con OKLA Backend

### Pasos de Implementación

1. **Crear MarketingService microservicio**

   ```bash
   dotnet new webapi -n MarketingService
   ```

2. **Instalar NuGets**

   ```xml
   <PackageReference Include="Mailchimp.Net" Version="1.3.15" />
   <PackageReference Include="Google.Ads.GoogleAds" Version="20.0.0" />
   <PackageReference Include="Facebook" Version="7.0.6" />
   ```

3. **Configurar appsettings.json**

   ```json
   {
     "Mailchimp": {
       "ApiKey": "{{MAILCHIMP_API_KEY}}",
       "DataCenter": "us1",
       "ListId": "{{LIST_ID}}"
     },
     "GoogleAds": {
       "DeveloperToken": "{{GOOGLE_ADS_TOKEN}}",
       "ClientId": "{{CLIENT_ID}}"
     }
   }
   ```

4. **Registrar en Program.cs**

   ```csharp
   services.AddScoped<IMailchimpService, MailchimpService>();
   services.AddScoped<IGoogleAdsService, GoogleAdsService>();
   ```

5. **Agregar ruta en Gateway (ocelot.json)**

   ```json
   {
     "DownstreamPathTemplate": "/api/{everything}",
     "DownstreamScheme": "http",
     "DownstreamHostAndPorts": [{ "Host": "marketingservice", "Port": 8080 }],
     "UpstreamPathTemplate": "/api/marketing/{everything}",
     "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"]
   }
   ```

6. **Crear eventos RabbitMQ**
   ```csharp
   public record CampaignCreatedEvent(Guid CampaignId, string Platform, string Name);
   public record CampaignCompletedEvent(Guid CampaignId, int Conversions);
   ```

---

## 📊 Costos Estimados (Mensual)

| API              | Descripción             | Costo   |
| ---------------- | ----------------------- | ------- |
| **Mailchimp**    | 10K+ suscriptores       | $150    |
| **Google Ads**   | $1K presupuesto diario  | $30,000 |
| **Facebook Ads** | $500 presupuesto diario | $15,000 |
| **LinkedIn Ads** | $500 presupuesto diario | $15,000 |
| **TOTAL**        | Todos los servicios     | $60,150 |

---

**Versión:** 2.0 | **Actualizado:** Enero 15, 2026 | **Completitud:** 100%
