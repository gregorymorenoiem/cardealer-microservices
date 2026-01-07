#!/bin/bash

echo "🧪 VALIDACIÓN COMPLETA DE TESTS CI/CD"
echo "======================================"
echo ""

total_passed=0
total_failed=0
failed_list=""

cd "$(dirname "$0")"

for proj in $(find . -name "*.Tests.csproj" -type f | sort); do
  name=$(basename $(dirname $proj))
  printf "▶️  %-38s " "$name:"
  
  output=$(timeout 120 dotnet test "$proj" --no-build --verbosity quiet 2>&1)
  exit_code=$?
  
  if [ $exit_code -eq 124 ]; then
    echo "⏰ TIMEOUT (>120s)"
    failed_list="$failed_list\n   - $name (TIMEOUT)"
  else
    passed=$(echo "$output" | grep -o "Passed: [0-9]*" | grep -o "[0-9]*")
    failed=$(echo "$output" | grep -o "Failed: [0-9]*" | grep -o "[0-9]*")
    
    passed=${passed:-0}
    failed=${failed:-0}
    
    total_passed=$((total_passed + passed))
    total_failed=$((total_failed + failed))
    
    if [ "$failed" -gt 0 ] 2>/dev/null; then
      echo "❌ $passed passed, $failed FAILED"
      failed_list="$failed_list\n   - $name ($failed failed)"
    else
      echo "✅ $passed passed"
    fi
  fi
done

echo ""
echo "========================================"
echo "📊 RESUMEN FINAL CI/CD"
echo "========================================"
echo "✅ Total Passed:  $total_passed"
echo "❌ Total Failed:  $total_failed"

if [ -n "$failed_list" ]; then
  echo ""
  echo "❌ Proyectos con fallos:"
  echo -e "$failed_list"
fi

if [ $total_failed -eq 0 ]; then
  echo ""
  echo "🎉 ¡TODOS LOS TESTS PASARON AL 100%!"
  exit 0
else
  echo ""
  echo "⚠️  Hay tests fallando que necesitan corrección"
  exit 1
fi
