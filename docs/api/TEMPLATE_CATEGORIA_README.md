# 🎯 TEMPLATE: Estructura Estándar para READMEs de Categorías

**Propósito:** Plantilla reutilizable para documentar categorías de APIs en OKLA  
**Uso:** Copiar esta estructura y adaptarla para cada categoría  
**Ejemplos Completados:** marketing/README.md, geolocation/README.md

---

## 📝 ESTRUCTURA REQUERIDA

Cada README de categoría DEBE incluir estas secciones en este orden:

### 1. HEADER (Obligatorio)

```markdown
# 🔗 [EMOJI] [NOMBRE] APIs

**APIs:** [N] (API1, API2, API3...)  
**Estado:** En Implementación (Fase X)  
**Prioridad:** 🔴 CRÍTICA / 🟠 ALTA / 🟡 MEDIA

---

## 📖 Resumen

[Descripción en 2-3 líneas de qué hace esta categoría]

### Casos de Uso en OKLA

✅ [Caso de uso 1]  
✅ [Caso de uso 2]  
✅ [Caso de uso 3]  
✅ [Caso de uso 4]  
✅ [Caso de uso 5]
```

### 2. COMPARATIVA DE APIs (Obligatorio)

```markdown
## 📊 Comparativa de APIs

| Aspecto           | API1       | API2         | API3        |
| ----------------- | ---------- | ------------ | ----------- |
| **Costo**         | $X/mes     | $Y/mes       | $Z/mes      |
| **Velocidad**     | <100ms     | <50ms        | <150ms      |
| **Cobertura RD**  | Excelente  | Bueno        | Bueno       |
| **Documentación** | Completa   | Muy Completa | Completa    |
| **Mejor Para**    | [desc]     | [desc]       | [desc]      |
| **Recomendado**   | ✅ PRIMARY | Fallback     | Alternativa |
```

### 3. ENDPOINTS (Obligatorio)

```markdown
## 🔗 Endpoints por API

### API 1 Nombre

\`\`\`
GET /endpoint1 # Descripción
POST /endpoint2 # Descripción
DELETE /endpoint3/{id} # Descripción
\`\`\`

### API 2 Nombre

\`\`\`
GET /endpoint1 # Descripción
POST /endpoint2 # Descripción
\`\`\`
```

### 4. BACKEND IMPLEMENTATION (Obligatorio)

```markdown
## 💻 Implementación Backend (C#)

### Service Interface

[Codigo C# de interfaz IServicio]

### Domain Models

[Codigo C# de entidades]

### Service Implementation

[Codigo C# completo con HttpClient, parsing JSON, etc]

### CQRS Commands

[Codigo C# de handlers MediatR]
```

### 5. FRONTEND IMPLEMENTATION (Obligatorio)

```markdown
## 🖥️ Implementación Frontend (React/TypeScript)

### Service API

[TypeScript interfaces y clase Service]

### React Components

[Componentes React funcionales con hooks]

### Custom Hooks

[React hooks personalizados si aplica]
```

### 6. TESTING (Obligatorio)

```markdown
## 🧪 Testing

### Unit Tests (xUnit)

[Tests xUnit con Moq]

### Integration Tests

[Tests de integración]

### Jest Tests (Frontend)

[Tests Jest/React Testing Library]
```

### 7. TROUBLESHOOTING (Obligatorio)

```markdown
## 🔧 Troubleshooting

| Problema | Causa   | Solución   |
| -------- | ------- | ---------- |
| Error 1  | Causa 1 | Solución 1 |
| Error 2  | Causa 2 | Solución 2 |
```

### 8. INTEGRACIÓN CON OKLA (Obligatorio)

```markdown
## ✅ Integración con OKLA Backend

### Pasos de Implementación

1. **Crear microservicio**
   \`\`\`bash
   dotnet new webapi -n ServiceName
   \`\`\`

2. **Instalar NuGets**
   \`\`\`xml
   <PackageReference Include="..." Version="X.X.X" />
   \`\`\`

3. **Configurar appsettings.json**
   \`\`\`json
   { "ApiName": { "ApiKey": "{{KEY}}" } }
   \`\`\`

4. **Registrar en Program.cs**
   \`\`\`csharp
   services.AddScoped<IService, ServiceImpl>();
   \`\`\`

5. **Agregar en Gateway (ocelot.json)**
   \`\`\`json
   { "UpstreamPathTemplate": "/api/path/{everything}" }
   \`\`\`

6. **Eventos RabbitMQ (si aplica)**
   \`\`\`csharp
   public record EventName(...);
   \`\`\`
```

### 9. COSTOS (Obligatorio)

```markdown
## 💰 Costos Estimados (Mensual)

| Servicio  | Descripción         | Costo |
| --------- | ------------------- | ----- |
| **API 1** | Uso típico          | $XXX  |
| **API 2** | Uso típico          | $XXX  |
| **API 3** | Uso típico          | $XXX  |
| **TOTAL** | Todos los servicios | $XXX  |
```

### 10. FOOTER (Obligatorio)

```markdown
---

**Versión:** 2.0 | **Actualizado:** Enero 15, 2026 | **Completitud:** 100%
```

---

## 📊 ESTRUCTURA DE CÓDIGO POR SECCIÓN

### Backend: Service Interface Pattern

```csharp
namespace OKLA.{ServiceName}Service.Domain.Interfaces
{
    /// <summary>
    /// Interface para integración con [API Name]
    /// </summary>
    public interface I{ServiceName}Service
    {
        /// <summary>
        /// Method description
        /// </summary>
        /// <param name="parameter">Parameter description</param>
        /// <returns>Return type description</returns>
        Task<ResponseDto> MethodAsync(string parameter);
    }

    // DTOs
    public class ResponseDto
    {
        public string Property { get; set; }
    }
}
```

### Backend: Domain Models Pattern

```csharp
namespace OKLA.{ServiceName}Service.Domain.Entities
{
    public class Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public List<RelatedEntity> RelatedEntities { get; set; }
    }
}
```

### Backend: Service Implementation Pattern

```csharp
namespace OKLA.{ServiceName}Service.Infrastructure.Services
{
    public class {ServiceName}Service : I{ServiceName}Service
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public {ServiceName}Service(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["{ServiceName}:ApiKey"];
            _baseUrl = config["{ServiceName}:BaseUrl"];
        }

        public async Task<ResponseDto> MethodAsync(string parameter)
        {
            try
            {
                var url = $"{_baseUrl}/endpoint?param={Uri.EscapeDataString(parameter)}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    return new ResponseDto
                    {
                        Property = root.GetProperty("field").GetString()
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Method failed: {ex.Message}", ex);
            }
        }
    }
}
```

### Backend: CQRS Command Pattern

```csharp
using MediatR;

namespace OKLA.{ServiceName}Service.Application.Features.{Feature}.Commands
{
    public class Create{Entity}Command : IRequest<{EntityDto}>
    {
        public string Property { get; set; }
    }

    public class Create{Entity}Handler : IRequestHandler<Create{Entity}Command, {EntityDto}>
    {
        private readonly I{ServiceName}Service _service;

        public async Task<{EntityDto}> Handle(Create{Entity}Command request, CancellationToken ct)
        {
            // Implementation
            throw new NotImplementedException();
        }
    }
}
```

### Frontend: Service Pattern

```typescript
// src/services/{serviceName}Service.ts

import axios from 'axios';

export interface Entity {
  id: string;
  name: string;
  // ... properties
}

export class {ServiceName}Service {
  private apiUrl = '/api/{serviceName}';

  async method(param: string): Promise<Entity> {
    return axios.get(`${this.apiUrl}/endpoint`, {
      params: { param }
    }).then(r => r.data);
  }
}

export const {serviceName}Service = new {ServiceName}Service();
```

### Frontend: React Component Pattern

```typescript
// src/components/{feature}/{ComponentName}.tsx

import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { {serviceName}Service } from '@/services/{serviceName}Service';

interface Props {
  // Props definition
}

export const {ComponentName}: React.FC<Props> = ({ }) => {
  const { data, isLoading, error } = useQuery({
    queryKey: ['key'],
    queryFn: () => {serviceName}Service.method()
  });

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return (
    <div className="space-y-4">
      {/* Component JSX */}
    </div>
  );
};
```

### Frontend: React Hook Pattern

```typescript
// src/hooks/use{ServiceName}.ts

import { useState, useEffect } from 'react';
import { {serviceName}Service } from '@/services/{serviceName}Service';

export const use{ServiceName} = (param: string) => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    setLoading(true);
    {serviceName}Service.method(param)
      .then(setData)
      .catch(setError)
      .finally(() => setLoading(false));
  }, [param]);

  return { data, loading, error };
};
```

### Testing: xUnit Pattern

```csharp
using Xunit;
using Moq;
using OKLA.{ServiceName}Service.Infrastructure.Services;

namespace OKLA.{ServiceName}Service.Tests
{
    public class {ServiceName}ServiceTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly {ServiceName}Service _service;

        public {ServiceName}ServiceTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _service = new {ServiceName}Service(_mockHttpClient.Object, GetMockConfig());
        }

        [Fact]
        public async Task MethodAsync_WithValidInput_ReturnsExpectedResult()
        {
            // Arrange
            var input = "test";

            // Act
            var result = await _service.MethodAsync(input);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("expected", result.Property);
        }

        [Theory]
        [InlineData("input1")]
        [InlineData("input2")]
        public async Task MethodAsync_WithVariousInputs_ReturnsResults(string input)
        {
            // Act
            var result = await _service.MethodAsync(input);

            // Assert
            Assert.NotNull(result);
        }
    }
}
```

---

## ✅ CHECKLIST: Antes de Marcar Como "COMPLETO"

- [ ] Header con emoji, nombre, # de APIs, estado, prioridad
- [ ] Resumen en 2-3 líneas
- [ ] Mínimo 5 casos de uso en OKLA
- [ ] Tabla comparativa de todas las APIs de la categoría
- [ ] Endpoints listados para cada API
- [ ] Service Interface en C#
- [ ] Domain Models/Entities
- [ ] Service Implementation completa (mínimo 1 API)
- [ ] CQRS Commands (al menos 1)
- [ ] React Components (mínimo 1 componente)
- [ ] Custom Hook (si aplica)
- [ ] Unit Tests (mínimo 3 tests)
- [ ] Integration Tests (mínimo 1)
- [ ] Jest Tests para React (si aplica)
- [ ] Tabla de Troubleshooting (mínimo 5 problemas)
- [ ] Pasos de integración con OKLA
- [ ] Tabla de costos estimados
- [ ] Footer con versión y fecha

---

## 🎓 EJEMPLOS COMPLETADOS

Estos archivos cumplen 100% esta estructura y pueden servir como referencia:

1. **marketing/README.md** - 4 APIs, comercial
2. **geolocation/README.md** - 2 APIs, mapas y ubicación
3. **communications/README.md** - 3 APIs, comunicaciones

---

## 🚀 CÓMO USAR ESTE TEMPLATE

### Opción 1: Manual Rápido (1-2 horas)

1. Copiar esta estructura
2. Cambiar nombre, emoji, APIs
3. Completar cada sección con ejemplos reales

### Opción 2: Usar Ejemplos Existentes

1. Abrir `marketing/README.md`
2. Copiar estructura
3. Reemplazar APIs específicas

### Opción 3: Solicitar a GitHub Copilot

Decir: "Necesito expandir {categoria}/README.md siguiendo el TEMPLATE_CATEGORIA_README.md con estructura completa"

---

## 📌 NOTAS IMPORTANTES

1. **Longitud esperada:** 800-1,200 líneas por README
2. **Código real:** Usar código C# y TypeScript que compile
3. **Testing:** Mínimo 3 tests unitarios + 1 integración
4. **OKLA-specific:** Mencionar casos de uso reales de OKLA
5. **Costos:** Investigar precios reales de cada API
6. **Versión:** Actualizar a 2.0, fecha actual
7. **Completitud:** Marcar como 100% cuando este checklist esté completo

---

**Versión:** 2.0 | **Actualizado:** Enero 15, 2026 | **Tipo:** Template/Guía
