#!/usr/bin/env python3

"""
🔍 SCRIPT DE AUDITORÍA DE DOCUMENTACIÓN DE API
Fecha: Enero 30, 2026
Propósito: Extraer endpoints reales de documentos y compararlos con Gateway
"""

import os
import re
import json
import csv
from datetime import datetime
from pathlib import Path
from collections import defaultdict
from typing import Dict, List, Set, Tuple

# Colores para terminal
class Colors:
    GREEN = '\033[0;32m'
    YELLOW = '\033[1;33m'
    RED = '\033[0;31m'
    BLUE = '\033[0;34m'
    NC = '\033[0m'  # No Color

def print_header():
    """Imprime el header del script"""
    print(f"{Colors.BLUE}{'=' * 50}{Colors.NC}")
    print(f"{Colors.BLUE}  🔍 AUDITORÍA DE DOCUMENTACIÓN DE API{Colors.NC}")
    print(f"{Colors.BLUE}{'=' * 50}{Colors.NC}")
    print()

def extract_endpoints_from_md(file_path: Path) -> List[Tuple[str, str]]:
    """
    Extrae endpoints de un archivo markdown.
    Retorna lista de tuplas (método, ruta)
    """
    endpoints = []
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
            
        # Patrón 1: GET|POST|PUT|DELETE /api/...
        pattern1 = r'\b(GET|POST|PUT|DELETE|PATCH)\s+(/api/[^\s;"\'\n]+)'
        matches1 = re.finditer(pattern1, content, re.IGNORECASE)
        
        for match in matches1:
            method = match.group(1).upper()
            route = match.group(2)
            route = re.sub(r'[;,\.]$', '', route)
            endpoints.append((method, route))
        
        # Patrón 2: apiClient.get|post|put|delete("/path") con tipos genéricos opcionales
        pattern2 = r'apiClient\.(get|post|put|delete|patch)\s*(?:<[^>]+>)?\s*\(\s*["\']([^"\']+)'
        matches2 = re.finditer(pattern2, content, re.IGNORECASE)
        
        for match in matches2:
            method = match.group(1).upper()
            route = match.group(2)
            # Agregar /api/ si no está presente
            if not route.startswith('/api/'):
                route = f'/api{route}' if route.startswith('/') else f'/api/{route}'
            route = re.split(r'[?#]', route)[0]
            route = re.sub(r'[;,\.\)\]]+$', '', route)
            endpoints.append((method, route))
        
        # Patrón 3: fetch(...api/auth/login...)
        pattern3 = r'fetch\([^)]*["\']([^"\']*(/api/[^"\']+))'
        matches3 = re.finditer(pattern3, content, re.IGNORECASE)
        
        for match in matches3:
            route = match.group(2)
            # Intentar encontrar el método cerca
            # Buscar method: "POST" en las próximas líneas
            start = match.start()
            end = start + 300  # Buscar en los próximos 300 caracteres
            context = content[start:end]
            method_match = re.search(r'method:\s*["\']([A-Z]+)["\']', context, re.IGNORECASE)
            if method_match:
                method = method_match.group(1).upper()
            else:
                method = 'GET'  # Default
            
            route = re.split(r'[?#]', route)[0]
            route = re.sub(r'[;,\.\)\]]+$', '', route)
            endpoints.append((method, route))
        
        # Patrón 4: axios.post|get|put|delete(url)
        pattern4 = r'axios\.(get|post|put|delete|patch)\s*\(\s*["`\']([^"`\']+)'
        matches4 = re.finditer(pattern4, content, re.IGNORECASE)
        
        for match in matches4:
            method = match.group(1).upper()
            route = match.group(2)
            # Remover URL base si existe
            if '://' in route:
                parts = route.split('/', 3)
                if len(parts) > 3:
                    route = '/' + parts[3]
            # Agregar /api/ si no está presente
            if not route.startswith('/api/'):
                route = f'/api{route}' if route.startswith('/') else f'/api/{route}'
            route = re.split(r'[?#]', route)[0]
            route = re.sub(r'[;,\.\)\]]+$', '', route)
            endpoints.append((method, route))
            
    except Exception as e:
        print(f"{Colors.RED}✗ Error leyendo {file_path.name}: {e}{Colors.NC}")
        
    return endpoints

def extract_gateway_routes(gateway_file: Path) -> Set[str]:
    """
    Extrae rutas del archivo ocelot.prod.json
    Retorna set de rutas
    """
    routes = set()
    
    try:
        with open(gateway_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
            
        if 'Routes' in data:
            for route in data['Routes']:
                if 'UpstreamPathTemplate' in route:
                    routes.add(route['UpstreamPathTemplate'])
                    
    except Exception as e:
        print(f"{Colors.RED}✗ Error leyendo Gateway config: {e}{Colors.NC}")
        
    return routes

def main():
    """Función principal"""
    print_header()
    
    # Configuración de directorios
    base_dir = Path.cwd()
    api_docs_dir = base_dir / 'docs' / 'frontend-rebuild' / '05-API-INTEGRATION'
    gateway_config = base_dir / 'backend' / 'Gateway' / 'Gateway.Api' / 'ocelot.prod.json'
    output_dir = base_dir / 'docs' / 'frontend-rebuild' / 'audit-reports'
    
    # Crear directorio de reportes
    output_dir.mkdir(parents=True, exist_ok=True)
    
    print(f"{Colors.GREEN}✓{Colors.NC} Directorios configurados")
    print()
    
    # Verificar que existan los directorios
    if not api_docs_dir.exists():
        print(f"{Colors.RED}✗ Directorio no encontrado: {api_docs_dir}{Colors.NC}")
        return 1
        
    if not gateway_config.exists():
        print(f"{Colors.RED}✗ Archivo no encontrado: {gateway_config}{Colors.NC}")
        return 1
    
    # Procesar documentos de API
    print(f"{Colors.YELLOW}📖 Leyendo documentos de API...{Colors.NC}")
    print()
    
    documented_endpoints = {}  # {(método, ruta): archivo}
    endpoints_by_file = defaultdict(int)
    
    for md_file in sorted(api_docs_dir.glob('*.md')):
        print(f"{Colors.BLUE}  → Procesando: {md_file.name}{Colors.NC}")
        
        endpoints = extract_endpoints_from_md(md_file)
        
        if endpoints:
            # Eliminar duplicados manteniendo el orden
            unique_endpoints = list(dict.fromkeys(endpoints))
            count = len(unique_endpoints)
            print(f"    {Colors.GREEN}✓ {count} endpoints encontrados{Colors.NC}")
            
            for method, route in unique_endpoints:
                key = (method, route)
                documented_endpoints[key] = md_file.name
                endpoints_by_file[md_file.name] += 1
        else:
            print(f"    {Colors.YELLOW}⚠ Sin endpoints documentados{Colors.NC}")
        print()
    
    # Leer Gateway
    print(f"{Colors.YELLOW}📡 Leyendo configuración del Gateway...{Colors.NC}")
    print()
    
    gateway_routes = extract_gateway_routes(gateway_config)
    total_gateway = len(gateway_routes)
    
    print(f"{Colors.GREEN}✓ {total_gateway} rutas encontradas en Gateway{Colors.NC}")
    print()
    
    # Calcular estadísticas
    total_documented = len(documented_endpoints)
    coverage_percentage = (total_documented / total_gateway * 100) if total_gateway > 0 else 0
    
    # Imprimir resultados
    print(f"{Colors.BLUE}{'=' * 50}{Colors.NC}")
    print(f"{Colors.BLUE}  📊 RESULTADOS DE LA AUDITORÍA{Colors.NC}")
    print(f"{Colors.BLUE}{'=' * 50}{Colors.NC}")
    print()
    print(f"{Colors.GREEN}✓ Endpoints documentados:{Colors.NC} {total_documented}")
    print(f"{Colors.YELLOW}⚠ Total de rutas en Gateway:{Colors.NC} {total_gateway}")
    print(f"{Colors.BLUE}📈 Cobertura de documentación:{Colors.NC} {coverage_percentage:.1f}%")
    print()
    
    # Generar timestamp para archivos
    timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
    
    # Generar reporte JSON
    print(f"{Colors.YELLOW}📄 Generando reportes...{Colors.NC}")
    
    json_file = output_dir / f'audit-report-{timestamp}.json'
    json_data = {
        'audit_date': datetime.utcnow().isoformat() + 'Z',
        'summary': {
            'total_documented': total_documented,
            'total_gateway_routes': total_gateway,
            'coverage_percentage': round(coverage_percentage, 1)
        },
        'documented_endpoints': [
            {
                'method': method,
                'route': route,
                'file': file
            }
            for (method, route), file in sorted(documented_endpoints.items())
        ],
        'endpoints_by_file': dict(endpoints_by_file)
    }
    
    with open(json_file, 'w', encoding='utf-8') as f:
        json.dump(json_data, f, indent=2, ensure_ascii=False)
    
    print(f"{Colors.GREEN}✓ Reporte JSON generado: {json_file.name}{Colors.NC}")
    
    # Generar reporte CSV
    csv_file = output_dir / f'audit-report-{timestamp}.csv'
    with open(csv_file, 'w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(['Método', 'Ruta', 'Archivo Documentado'])
        for (method, route), file in sorted(documented_endpoints.items()):
            writer.writerow([method, route, file])
    
    print(f"{Colors.GREEN}✓ Reporte CSV generado: {csv_file.name}{Colors.NC}")
    
    # Generar reporte Markdown
    md_file = output_dir / f'audit-report-{timestamp}.md'
    with open(md_file, 'w', encoding='utf-8') as f:
        f.write('# 📊 Reporte de Auditoría de Documentación de API\n\n')
        f.write(f'**Fecha:** {datetime.now().strftime("%B %d, %Y %H:%M:%S")}\n')
        f.write('**Generado por:** audit-api-documentation.py\n\n')
        f.write('---\n\n')
        f.write('## 📈 Resumen Ejecutivo\n\n')
        f.write('| Métrica                      | Valor           |\n')
        f.write('|------------------------------|-----------------||\n')
        f.write(f'| **Endpoints Documentados**   | {total_documented} |\n')
        f.write(f'| **Rutas en Gateway**         | {total_gateway}  |\n')
        f.write(f'| **Cobertura de Documentación** | {coverage_percentage:.1f}%   |\n\n')
        f.write('---\n\n')
        f.write('## 📋 Endpoints Documentados\n\n')
        f.write('| Método | Ruta | Archivo |\n')
        f.write('|--------|------|---------||\n')
        
        for (method, route), file in sorted(documented_endpoints.items()):
            f.write(f'| `{method}` | `{route}` | {file} |\n')
        
        f.write('\n---\n\n')
        f.write('## 📊 Desglose por Archivo\n\n')
        f.write('| Archivo | Endpoints Documentados |\n')
        f.write('|---------|------------------------||\n')
        
        for file, count in sorted(endpoints_by_file.items()):
            f.write(f'| {file} | {count} |\n')
        
        f.write('\n---\n\n')
        f.write('## 🎯 Próximos Pasos\n\n')
        f.write('### Servicios Pendientes de Documentar\n\n')
        f.write('Basado en el Gateway, los siguientes servicios necesitan documentación:\n\n')
        f.write('- **VehiclesService:** Endpoints de vehículos (búsqueda, filtrado, CRUD)\n')
        f.write('- **UserService:** Gestión de usuarios y perfiles\n')
        f.write('- **BillingService:** Pagos, suscripciones, planes\n')
        f.write('- **RoleService:** Roles y permisos\n')
        f.write('- **NotificationService:** Notificaciones push, email, SMS\n')
        f.write('- **Y más...**\n\n')
        f.write('### Recomendaciones\n\n')
        f.write('1. **Prioridad Alta:** Documentar servicios core (Vehicles, Users, Billing)\n')
        f.write('2. **Prioridad Media:** Documentar servicios de soporte (Notifications, Media)\n')
        f.write('3. **Prioridad Baja:** Documentar servicios administrativos\n\n')
        f.write('---\n\n')
        f.write('_Generado automáticamente por audit-api-documentation.py_\n')
    
    print(f"{Colors.GREEN}✓ Reporte Markdown generado: {md_file.name}{Colors.NC}")
    print()
    
    # Resumen final
    print(f"{Colors.BLUE}{'=' * 50}{Colors.NC}")
    print(f"{Colors.GREEN}✅ AUDITORÍA COMPLETADA{Colors.NC}")
    print(f"{Colors.BLUE}{'=' * 50}{Colors.NC}")
    print()
    print(f"Reportes generados en: {Colors.YELLOW}{output_dir}{Colors.NC}")
    print()
    print(f"  📄 JSON:     {json_file.name}")
    print(f"  📊 CSV:      {csv_file.name}")
    print(f"  📝 Markdown: {md_file.name}")
    print()
    print(f"{Colors.BLUE}Para ver el reporte Markdown:{Colors.NC}")
    print(f"  cat {md_file}")
    print()
    
    return 0

if __name__ == '__main__':
    exit(main())
