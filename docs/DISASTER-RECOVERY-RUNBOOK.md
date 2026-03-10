# 🚨 Disaster Recovery Runbook — OKLA (CarDealer Microservices)

> **RTO Target: < 2 hours** from total failure detection to full service restoration.
> **RPO Target: 24 hours** (daily backups at 3:00 AM RD / 7:00 AM UTC).
>
> Last tested: _(update after each DR drill)_
> Last updated: 2026-03-09

---

## 📋 Quick Reference

| Item                 | Value                                                                                               |
| -------------------- | --------------------------------------------------------------------------------------------------- |
| **K8s Cluster**      | DOKS (DigitalOcean Kubernetes) — `okla-k8s-cluster`                                                 |
| **Region**           | NYC1 (production)                                                                                   |
| **DB Provider**      | DigitalOcean Managed PostgreSQL                                                                     |
| **Backup Bucket**    | `okla-dr-backups` (DO Spaces, SFO3 — cross-region)                                                  |
| **Backup Schedule**  | Daily at 3:00 AM RD (7:00 AM UTC)                                                                   |
| **Local Retention**  | 30 days                                                                                             |
| **Remote Retention** | 90 days                                                                                             |
| **Databases**        | 12 (auth, user, role, vehicles, media, billing, notification, error, review, admin, audit, contact) |

---

## 🔴 Scenario 1: Complete Database Failure

**Symptoms:** All services return 503, readiness probes failing, DB connection errors in logs.

### Step 1 — Confirm the failure (ETA: 5 min)

```bash
# Check pod status
kubectl get pods -n okla | grep -v Running

# Check DB-dependent service logs
kubectl logs deployment/authservice -n okla --tail=50 | grep -i "connection\|postgres\|database"

# Verify DO Managed DB status
doctl databases list
doctl databases connection <db-id> --format Host,Port,Database
```

### Step 2 — Assess damage scope (ETA: 5 min)

```bash
# If DO Managed DB is responsive but data is corrupted:
#   → Proceed to Step 3a (Point-in-Time Recovery from DO)
#
# If DO Managed DB is completely gone:
#   → Proceed to Step 3b (Full restore from backup)
```

### Step 3a — Point-in-Time Recovery via DO Managed DB (ETA: 15-30 min)

DigitalOcean Managed PostgreSQL has automatic daily backups with 7-day retention.

```bash
# List available backups
doctl databases backups list <db-id>

# Fork from a backup (creates new DB cluster from backup)
doctl databases fork <db-id> --restore-from <backup-id> --name okla-db-restored

# Get new connection details
doctl databases connection okla-db-restored --format Host,Port,User,Password

# Update K8s secret with new DB host
kubectl create secret generic database-secrets -n okla \
  --from-literal=POSTGRES_HOST=<new-host> \
  --from-literal=POSTGRES_PORT=25060 \
  --from-literal=POSTGRES_USER=doadmin \
  --from-literal=POSTGRES_PASSWORD=<new-password> \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart all deployments to pick up new connection
kubectl rollout restart deployment -n okla
```

### Step 3b — Full Restore from Backup (ETA: 30-60 min)

```bash
# 1. Create new DO Managed PostgreSQL
doctl databases create okla-db-restored \
  --engine pg \
  --version 16 \
  --region nyc1 \
  --size db-s-2vcpu-4gb \
  --num-nodes 1

# 2. Get new connection details
NEW_HOST=$(doctl databases connection okla-db-restored --format Host --no-header)
NEW_PORT=$(doctl databases connection okla-db-restored --format Port --no-header)
NEW_USER=$(doctl databases connection okla-db-restored --format User --no-header)
NEW_PASS=$(doctl databases connection okla-db-restored --format Password --no-header)

# 3. Download latest backups from DO Spaces
LATEST_DATE=$(s3cmd ls s3://okla-dr-backups/postgres/authservice/ \
  --host=sfo3.digitaloceanspaces.com \
  --host-bucket="%(bucket)s.sfo3.digitaloceanspaces.com" | tail -1 | awk '{print $4}' | grep -oP '\d{8}')

DATABASES="authservice userservice roleservice vehiclessaleservice mediaservice billingservice notificationservice errorservice reviewservice adminservice auditservice contactservice"

mkdir -p /tmp/okla-restore
for DB in $DATABASES; do
  echo "📥 Downloading $DB backup..."
  s3cmd get "s3://okla-dr-backups/postgres/${DB}/" /tmp/okla-restore/ \
    --host=sfo3.digitaloceanspaces.com \
    --host-bucket="%(bucket)s.sfo3.digitaloceanspaces.com" \
    --recursive --skip-existing
done

# 4. Create databases and restore
for DB in $DATABASES; do
  echo "🔧 Creating database $DB..."
  PGPASSWORD=$NEW_PASS psql -h $NEW_HOST -p $NEW_PORT -U $NEW_USER -d defaultdb \
    -c "CREATE DATABASE $DB;"

  LATEST_DUMP=$(ls -t /tmp/okla-restore/${DB}_*.dump 2>/dev/null | head -1)
  if [ -n "$LATEST_DUMP" ]; then
    echo "📦 Restoring $DB from $LATEST_DUMP..."
    PGPASSWORD=$NEW_PASS pg_restore -h $NEW_HOST -p $NEW_PORT -U $NEW_USER \
      -d $DB --no-owner --no-privileges --clean --if-exists "$LATEST_DUMP"
    echo "✅ $DB restored"
  else
    echo "⚠️ No backup found for $DB"
  fi
done

# 5. Update K8s secret
kubectl create secret generic database-secrets -n okla \
  --from-literal=POSTGRES_HOST=$NEW_HOST \
  --from-literal=POSTGRES_PORT=$NEW_PORT \
  --from-literal=POSTGRES_USER=$NEW_USER \
  --from-literal=POSTGRES_PASSWORD=$NEW_PASS \
  --dry-run=client -o yaml | kubectl apply -f -

# 6. Restart all deployments
kubectl rollout restart deployment -n okla

# 7. Verify health
sleep 60
kubectl get pods -n okla | grep -v Running
```

### Step 4 — Verify restoration (ETA: 10-15 min)

```bash
# Check all pods are running
kubectl get pods -n okla -o wide

# Verify health endpoints
for SVC in authservice userservice roleservice vehiclessaleservice mediaservice billingservice notificationservice errorservice; do
  STATUS=$(kubectl exec deployment/$SVC -n okla -- wget -qO- http://localhost:8080/health/ready 2>/dev/null | head -1)
  echo "$SVC: $STATUS"
done

# Check recent data integrity (spot check)
kubectl exec deployment/authservice -n okla -- wget -qO- http://localhost:8080/health
```

---

## 🟡 Scenario 2: Single Service Database Corruption

```bash
# 1. Identify affected service
kubectl logs deployment/<service-name> -n okla --tail=100

# 2. Download specific backup
DB_NAME="<service-name>"
s3cmd get "s3://okla-dr-backups/postgres/${DB_NAME}/" /tmp/ \
  --host=sfo3.digitaloceanspaces.com \
  --host-bucket="%(bucket)s.sfo3.digitaloceanspaces.com" \
  --recursive

# 3. Restore single database
LATEST_DUMP=$(ls -t /tmp/${DB_NAME}_*.dump | head -1)
PGPASSWORD=$DB_PASS pg_restore -h $DB_HOST -p $DB_PORT -U $DB_USER \
  -d $DB_NAME --no-owner --no-privileges --clean --if-exists "$LATEST_DUMP"

# 4. Restart affected service
kubectl rollout restart deployment/$DB_NAME -n okla
```

---

## 🟢 Scenario 3: Kubernetes Cluster Failure

```bash
# 1. Create new DOKS cluster
doctl kubernetes cluster create okla-k8s-restored \
  --region nyc1 \
  --size s-4vcpu-8gb \
  --count 2

# 2. Apply all manifests
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmaps.yaml
kubectl apply -f k8s/infrastructure.yaml
kubectl apply -f k8s/deployments.yaml
kubectl apply -f k8s/chatbotservice.yaml
kubectl apply -f k8s/services.yaml
kubectl apply -f k8s/ingress.yaml
kubectl apply -f k8s/backup.yaml

# 3. Restore secrets (from backup or recreate)
# Secrets are NOT backed up by Velero — recreate from source
kubectl apply -f k8s/secrets.template.yaml
# Populate with actual values from password manager / vault

# 4. Update DNS to point to new cluster's load balancer
doctl compute load-balancer list
# Update A record for okla.com.do to new LB IP
```

---

## 📅 DR Testing Schedule

| Frequency     | Test              | Description                                                        |
| ------------- | ----------------- | ------------------------------------------------------------------ |
| **Monthly**   | Restore single DB | Download latest backup from DO Spaces, restore to test environment |
| **Quarterly** | Full DR drill     | Simulate complete DB failure, restore all 12 databases             |
| **Annually**  | Full cluster DR   | Simulate cluster loss, rebuild from scratch                        |

### Monthly Test Script

```bash
# Run: scripts/dr-restore-test.sh
# See: scripts/dr-restore-test.sh for automated test
```

---

## 📞 Escalation Contacts

| Role                 | Contact                  | When              |
| -------------------- | ------------------------ | ----------------- |
| Platform Lead        | Slack: #platform-alerts  | First responder   |
| CTO                  | Slack: #cto-alerts       | If RTO > 1 hour   |
| DigitalOcean Support | support.digitalocean.com | Managed DB issues |

---

## ✅ Post-Incident Checklist

- [ ] All pods running and healthy
- [ ] All health endpoints returning 200
- [ ] Spot-check data integrity (recent records exist)
- [ ] Monitoring/alerting restored
- [ ] Backup CronJob verified running
- [ ] Incident report written (Notion/Confluence)
- [ ] RTO measured and logged
- [ ] Update this runbook if procedures changed
