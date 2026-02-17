# 🔐 KYC Verification APIs

**APIs:** 2 (Onfido, Stripe Identity)  
**Estado:** En Implementación (Fase 1)  
**Prioridad:** 🔴 CRÍTICA

---

## 📖 Resumen

Verificación de identidad (KYC - Know Your Customer) para validar dealers y usuarios.

### Casos de Uso

✅ Verificación de cédula  
✅ Selfie validation  
✅ Prueba de dirección  
✅ Análisis de fraude  
✅ Cumplimiento normativo

---

## 🔗 APIs

| API                 | Costo              | Caso Uso      |
| ------------------- | ------------------ | ------------- |
| **Onfido**          | $2-10/verificación | Full KYC      |
| **Stripe Identity** | $1-5/verificación  | Verif. rápida |

---

## 💻 Onfido Implementation

```csharp
public interface IKycService
{
    Task<KycResult> VerifyIdentityAsync(string documentId, string photoId);
    Task<KycStatus> GetVerificationStatusAsync(string applicantId);
}

public class OnfidoService : IKycService
{
    private readonly OnfidoClient _onfidoClient;

    public async Task<KycResult> VerifyIdentityAsync(string documentId, string photoId)
    {
        var applicant = await _onfidoClient.CreateApplicantAsync(
            new { email = "user@okla.com" });

        var documentCheck = await _onfidoClient.CreateDocumentCheckAsync(
            applicant.Id, documentId);

        var faceCheck = await _onfidoClient.CreateFaceCheckAsync(
            applicant.Id, photoId);

        return new KycResult
        {
            ApplicantId = applicant.Id,
            DocumentStatus = documentCheck.Status,
            FaceStatus = faceCheck.Status,
            IsVerified = documentCheck.Status == "clear" && faceCheck.Status == "clear"
        };
    }
}
```

---

**Versión:** 1.0 | **Actualizado:** Enero 15, 2026
