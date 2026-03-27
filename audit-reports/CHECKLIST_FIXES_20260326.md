# ✅ CHECKLIST DE FIXES — OKLA AI AGENTS

**Generado:** 2026-03-26 14:10 AST  
**Auditoría:** AI Agents Production Readiness  

---

## 🔴 CRÍTICO (Hoy)

### [ ] Fix RecoAgent 502
**Responsable:** Backend Team  
**Acción:**
```bash
docker logs reco-agent | tail -50
# O si está crashed:
docker-compose restart reco-agent
# Luego validar:
curl -s https://okla.com.do/api/reco-agent/health
# Debe retornar 200, no 502
```
**Validación:** GET /api/reco-agent/health → 200 OK  
**ETA:** 15 min  

### [ ] Deploy PricingAgent
**Responsable:** DevOps  
**Acción:**
```bash
docker-compose -f compose.docker.yaml up -d pricing-agent
# Validar:
curl -s https://okla.com.do/api/pricing-agent/health
# Debe retornar 200, no 404
```
**Validación:** GET /api/pricing-agent/health → 200 OK  
**ETA:** 20 min  

### [ ] Deploy SupportAgent
**Responsable:** DevOps  
**Acción:**
```bash
docker-compose -f compose.docker.yaml up -d support-agent
# Validar:
curl -s https://okla.com.do/api/support-agent/health
# Debe retornar 200, no 404
```
**Validación:** GET /api/support-agent/health → 200 OK  
**ETA:** 20 min  

---

## 🟡 ALTO (Antes de 4PM)

### [ ] Obtener Admin Token para LLM Gateway
**Responsable:** Security/DevOps  
**Acción:**
```bash
# Get valid JWT token from auth system
export ADMIN_TOKEN="<valid-jwt-here>"
# Validar:
curl -H "Authorization: Bearer $ADMIN_TOKEN" \
  https://okla.com.do/api/admin/llm-gateway/health
# Debe retornar config completa, no 401
```
**Validación:** /api/admin/llm-gateway/health → 200 OK  
**ETA:** 10 min  

### [ ] Mejorar Mensajes de Error WhatsApp
**Responsable:** Backend Team  
**Actual:**
```json
{"errors":{"hub.verify_token":["The token field is required."]}}
```
**Mejorado:**
```json
{
  "error": "Missing required parameter",
  "parameter": "hub.verify_token",
  "example": "GET /api/whatsapp/webhook?hub.mode=subscribe&hub.verify_token=YOUR_TOKEN&hub.challenge=test"
}
```
**Validación:** Error message incluye ejemplo  
**ETA:** 15 min  

---

## ⏳ ESPERAR (No acción requerida)

### [ ] Frontend Maintenance
**Responsable:** DevOps  
**Status:** Countdown 13h 29m (desde 09:59 AST)  
**Validación:** https://okla.com.do/vehiculos carga sin mantenimiento  
**ETA:** ~23:30 AST  

---

## ✅ VERIFICACIONES POST-FIXES

### Checklist de Validación (después de todos los fixes)

```bash
# 1. Verify all health endpoints
echo "1. Checking health endpoints..."
for endpoint in \
  "search-agent" \
  "chat" \
  "pricing-agent" \
  "reco-agent" \
  "support-agent"
do
  status=$(curl -s -o /dev/null -w "%{http_code}" \
    "https://okla.com.do/api/$endpoint/health")
  echo "  $endpoint: $status"
done

# 2. Test SearchAgent
echo "2. Testing SearchAgent..."
curl -s -X POST https://okla.com.do/api/search-agent/search \
  -H "Content-Type: application/json" \
  -d '{"query":"toyota corolla"}' | jq '.success'

# 3. Test DealerChat
echo "3. Testing DealerChat..."
curl -s https://okla.com.do/api/chat/health | jq '.status'

# 4. Test PricingAgent
echo "4. Testing PricingAgent..."
curl -s -X POST https://okla.com.do/api/pricing-agent/estimate \
  -H "Content-Type: application/json" \
  -d '{"brand":"Toyota","model":"Corolla"}' | jq '.resultado // .error' | head -c 50

# 5. Test RecoAgent
echo "5. Testing RecoAgent..."
curl -s https://okla.com.do/api/reco-agent/status | jq '.status'

# 6. Verify Frontend
echo "6. Checking Frontend..."
curl -s -I https://okla.com.do/vehiculos | grep -E "HTTP|200|maintenance"
```

---

## 📋 COMUNICACIÓN

### Stakeholders a Notificar

- [ ] **Engineering Team** — Status de fixes completados
- [ ] **Product Team** — ETA de feature availability  
- [ ] **Customers** — Comunicado sobre downtime (si aplica)
- [ ] **QA Team** — Re-audit request cuando fixes completos

### Mensaje Template

```
OKLA AI Agents Status Update — 2026-03-26

✅ What's Working:
   - SearchAgent: Operational (3.8s latency)
   - DealerChat API: Healthy
   - Security: 9.6/10 verified

🔧 Being Fixed:
   - RecoAgent: Restart in progress
   - PricingAgent: Deployment in progress
   - SupportAgent: Deployment in progress

⏳ Awaiting:
   - Frontend: Maintenance completing (~23:30 AST)

ETA Full Availability: 2026-03-26 ~16:00 AST
```

---

## 📊 TRACKING

### Daily Status (después de fixes)

```
Date: 2026-03-26
Time: 14:10 AST
Status: [ ] RecoAgent [ ] PricingAgent [ ] SupportAgent [ ] Frontend

Targets:
- [ ] All health endpoints return 200 OK
- [ ] All agents respond to test requests
- [ ] Frontend loads without maintenance page
- [ ] Re-audit completed successfully
```

---

## 🎯 SUCCESS CRITERIA

Auditoría considerada **EXITOSA** cuando:

- ✅ RecoAgent /health returns 200 OK
- ✅ PricingAgent /health returns 200 OK  
- ✅ SupportAgent /health returns 200 OK
- ✅ LLM Gateway verified with admin token
- ✅ Frontend maintenance completed
- ✅ All 10 agents operational and tested
- ✅ Security score remains 9.6/10+
- ✅ No new 500 errors introduced

---

## 📞 ESCALATION

If any fix takes > 30 min:
1. Notify engineering lead
2. Check docker logs: `docker logs <service>`
3. Review recent deployments
4. Escalate to DevOps if needed

---

**Documento generado:** 2026-03-26 14:10 AST  
**Próxima auditoría:** 2026-03-26 (después de todos los fixes)
