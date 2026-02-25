#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOG="/tmp/fix-gateway-sellers-$(date +%s).log"

echo "================================================"  | tee "$LOG"
echo " Fix: Add /api/sellers/* routes to gateway-config" | tee -a "$LOG"
echo " Log: $LOG"                                         | tee -a "$LOG"
echo " $(date)"                                           | tee -a "$LOG"
echo "================================================"  | tee -a "$LOG"

# ── 1. Dump current gateway-config ──────────────────────────────────────────
echo "" | tee -a "$LOG"
echo "[1/5] Dumping current gateway-config..." | tee -a "$LOG"
kubectl get configmap gateway-config -n okla -o json > /tmp/gateway-config-current.json
python3 -c "
import json
with open('/tmp/gateway-config-current.json') as f:
    cm = json.load(f)
config = json.loads(cm['data']['ocelot.json'])
has_sellers = any('sellers' in r.get('UpstreamPathTemplate','') for r in config['Routes'])
print('      Total routes:', len(config['Routes']))
print('      Has sellers routes:', has_sellers)
" 2>&1 | tee -a "$LOG"

# ── 2. Patch: inject sellers routes ─────────────────────────────────────────
echo "" | tee -a "$LOG"
echo "[2/5] Injecting sellers routes..." | tee -a "$LOG"
python3 "$SCRIPT_DIR/patch_gateway_sellers.py" 2>&1 | tee -a "$LOG"

# ── 3. Apply patched configmap ───────────────────────────────────────────────
echo "" | tee -a "$LOG"
echo "[3/5] Applying patched configmap to cluster..." | tee -a "$LOG"
if [ -f /tmp/gateway-config-patched.json ]; then
    kubectl apply -f /tmp/gateway-config-patched.json 2>&1 | tee -a "$LOG"
    echo "      ConfigMap applied." | tee -a "$LOG"
else
    echo "      No patch file (sellers may already exist). Skipping." | tee -a "$LOG"
fi

# ── 4. Rolling restart gateway ───────────────────────────────────────────────
echo "" | tee -a "$LOG"
echo "[4/5] Rolling restart of gateway deployment..." | tee -a "$LOG"
kubectl rollout restart deployment/gateway -n okla 2>&1 | tee -a "$LOG"
kubectl rollout status deployment/gateway -n okla --timeout=120s 2>&1 | tee -a "$LOG"
echo "      Gateway ready." | tee -a "$LOG"

# ── 5. Verify sellers routes are live ────────────────────────────────────────
echo "" | tee -a "$LOG"
echo "[5/5] Verifying sellers routes in live configmap..." | tee -a "$LOG"
python3 -c "
import json, subprocess, sys
result = subprocess.run(['kubectl','get','configmap','gateway-config','-n','okla','-o','json'], capture_output=True, text=True)
cm = json.loads(result.stdout)
config = json.loads(cm['data']['ocelot.json'])
sellers = [r['UpstreamPathTemplate'] for r in config['Routes'] if 'sellers' in r.get('UpstreamPathTemplate','')]
print('      Total routes:', len(config['Routes']))
if sellers:
    print('      Sellers routes found (' + str(len(sellers)) + '):')
    for r in sellers:
        print('        ', r)
else:
    print('      ERROR: sellers routes still missing!')
    sys.exit(1)
" 2>&1 | tee -a "$LOG"

echo "" | tee -a "$LOG"
echo "================================================"  | tee -a "$LOG"
echo " Done! Log saved to: $LOG"                         | tee -a "$LOG"
echo "================================================"  | tee -a "$LOG"
