#!/usr/bin/env python3
"""
OKLA (CarDealer) Database Seeding Script
=========================================

Este script llena todas las bases de datos de los microservicios con datos 
comprehensivos que cubren todas las opciones y variantes disponibles.

Autor: Gregory Moreno
Fecha: Enero 2026
Versión: 1.0.0

Uso:
    python seed_database.py [--env dev|docker|prod] [--dry-run] [--only <service>]

Ejemplos:
    python seed_database.py                     # Seed desarrollo local
    python seed_database.py --env docker        # Seed ambiente Docker
    python seed_database.py --dry-run           # Ver qué haría sin ejecutar
    python seed_database.py --only vehicles     # Solo seed de vehículos
"""

import os
import sys
import json
import time
import argparse
import requests
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
from concurrent.futures import ThreadPoolExecutor, as_completed

# ============================================================================
# CONFIGURACIÓN DE AMBIENTES
# ============================================================================

ENVIRONMENTS = {
    "dev": {
        "gateway": "http://localhost:18443",
        "services": {
            "auth": "http://localhost:5001",
            "users": "http://localhost:5003",
            "vehicles": "http://localhost:5009",
            "billing": "http://localhost:5015",
            "contact": "http://localhost:5019",
            "notifications": "http://localhost:5017",
            "media": "http://localhost:5007",
            "dealers": "http://localhost:5039",
            "roles": "http://localhost:5005",
            "reviews": "http://localhost:5059",
        }
    },
    "docker": {
        "gateway": "http://gateway:8080",
        "services": {
            "auth": "http://authservice:8080",
            "users": "http://userservice:8080",
            "vehicles": "http://vehiclessaleservice:8080",
            "billing": "http://billingservice:8080",
            "contact": "http://contactservice:8080",
            "notifications": "http://notificationservice:8080",
            "media": "http://mediaservice:8080",
            "dealers": "http://dealermanagementservice:8080",
            "roles": "http://roleservice:8080",
            "reviews": "http://reviewservice:8080",
        }
    },
    "prod": {
        "gateway": "https://api.okla.com.do",
        "services": {
            # En producción usamos el gateway para todo
            "auth": "https://api.okla.com.do",
            "users": "https://api.okla.com.do",
            "vehicles": "https://api.okla.com.do",
            "billing": "https://api.okla.com.do",
            "contact": "https://api.okla.com.do",
            "notifications": "https://api.okla.com.do",
            "media": "https://api.okla.com.do",
            "dealers": "https://api.okla.com.do",
            "roles": "https://api.okla.com.do",
            "reviews": "https://api.okla.com.do",
        }
    }
}

# ============================================================================
# COLORES PARA CONSOLA
# ============================================================================

class Colors:
    HEADER = '\033[95m'
    BLUE = '\033[94m'
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    RED = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'

def log_info(msg: str):
    print(f"{Colors.CYAN}ℹ️  {msg}{Colors.ENDC}")

def log_success(msg: str):
    print(f"{Colors.GREEN}✅ {msg}{Colors.ENDC}")

def log_warning(msg: str):
    print(f"{Colors.YELLOW}⚠️  {msg}{Colors.ENDC}")

def log_error(msg: str):
    print(f"{Colors.RED}❌ {msg}{Colors.ENDC}")

def log_header(msg: str):
    print(f"\n{Colors.BOLD}{Colors.HEADER}{'='*60}{Colors.ENDC}")
    print(f"{Colors.BOLD}{Colors.HEADER}{msg:^60}{Colors.ENDC}")
    print(f"{Colors.BOLD}{Colors.HEADER}{'='*60}{Colors.ENDC}\n")

# ============================================================================
# CLASES DE RESULTADO
# ============================================================================

@dataclass
class SeedResult:
    """Resultado de una operación de seeding"""
    service: str
    entity: str
    total: int
    success: int
    failed: int
    errors: List[str]
    
    @property
    def success_rate(self) -> float:
        if self.total == 0:
            return 0.0
        return (self.success / self.total) * 100

# ============================================================================
# CLASE PRINCIPAL DE SEEDING
# ============================================================================

class DatabaseSeeder:
    """Clase principal para ejecutar el seeding de la base de datos"""
    
    def __init__(self, env: str = "dev", dry_run: bool = False):
        self.env = env
        self.dry_run = dry_run
        self.config = ENVIRONMENTS[env]
        self.base_path = Path(__file__).parent.parent
        self.results: List[SeedResult] = []
        self.admin_token: Optional[str] = None
        
    def get_service_url(self, service: str) -> str:
        """Obtiene la URL base de un servicio"""
        return self.config["services"].get(service, self.config["gateway"])
    
    def load_json(self, relative_path: str) -> Dict:
        """Carga un archivo JSON desde la carpeta de seeding"""
        file_path = self.base_path / relative_path
        if not file_path.exists():
            log_error(f"Archivo no encontrado: {file_path}")
            return {}
        
        with open(file_path, 'r', encoding='utf-8') as f:
            return json.load(f)
    
    def api_post(self, url: str, data: Dict, headers: Dict = None) -> tuple[bool, Any]:
        """Realiza un POST a la API"""
        if self.dry_run:
            log_info(f"[DRY-RUN] POST {url}")
            return True, {"id": "dry-run-id"}
        
        try:
            default_headers = {
                "Content-Type": "application/json",
            }
            if self.admin_token:
                default_headers["Authorization"] = f"Bearer {self.admin_token}"
            
            if headers:
                default_headers.update(headers)
            
            response = requests.post(url, json=data, headers=default_headers, timeout=30)
            
            if response.status_code in [200, 201, 204]:
                try:
                    return True, response.json()
                except:
                    return True, {}
            else:
                return False, f"HTTP {response.status_code}: {response.text[:200]}"
                
        except requests.exceptions.RequestException as e:
            return False, str(e)
    
    def authenticate_admin(self) -> bool:
        """Autentica como admin para obtener token"""
        log_info("Autenticando como admin...")
        
        # Intentar login con admin
        url = f"{self.get_service_url('auth')}/api/auth/login"
        data = {
            "email": "superadmin@okla.com.do",
            "password": "Test123!"
        }
        
        success, result = self.api_post(url, data)
        if success and isinstance(result, dict) and "token" in result:
            self.admin_token = result["token"]
            log_success("Autenticación exitosa")
            return True
        else:
            log_warning("No se pudo autenticar - continuando sin token")
            return False
    
    # ========================================================================
    # SEEDERS POR SERVICIO
    # ========================================================================
    
    def seed_users(self) -> SeedResult:
        """Seed de usuarios (AuthService)"""
        log_header("SEEDING: USUARIOS (AuthService)")
        
        data = self.load_json("01_auth/users.json")
        users = data.get("users", [])
        
        service_url = self.get_service_url("auth")
        errors = []
        success = 0
        
        for user in users:
            # Preparar datos para registro
            register_data = {
                "email": user["email"],
                "password": "Test123!",  # Contraseña por defecto
                "firstName": user["firstName"],
                "lastName": user["lastName"],
                "accountType": user["accountType"],
                "phoneNumber": user.get("phoneNumber"),
            }
            
            url = f"{service_url}/api/auth/register"
            ok, result = self.api_post(url, register_data)
            
            if ok:
                success += 1
                log_success(f"Usuario creado: {user['email']}")
            else:
                errors.append(f"{user['email']}: {result}")
                log_error(f"Error creando usuario {user['email']}: {result}")
        
        return SeedResult(
            service="AuthService",
            entity="Users",
            total=len(users),
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_dealers(self) -> SeedResult:
        """Seed de dealers (DealerManagementService)"""
        log_header("SEEDING: DEALERS (DealerManagementService)")
        
        data = self.load_json("02_users/dealers.json")
        dealers = data.get("dealers", [])
        
        service_url = self.get_service_url("dealers")
        errors = []
        success = 0
        
        for dealer in dealers:
            url = f"{service_url}/api/dealers"
            ok, result = self.api_post(url, dealer)
            
            if ok:
                success += 1
                log_success(f"Dealer creado: {dealer['businessName']}")
            else:
                errors.append(f"{dealer['businessName']}: {result}")
                log_error(f"Error creando dealer: {result}")
        
        return SeedResult(
            service="DealerManagementService",
            entity="Dealers",
            total=len(dealers),
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_vehicles(self) -> SeedResult:
        """Seed de vehículos (VehiclesSaleService)"""
        log_header("SEEDING: VEHÍCULOS (VehiclesSaleService)")
        
        # Cargar ambos archivos de vehículos
        data1 = self.load_json("03_vehicles/vehicles_part1.json")
        data2 = self.load_json("03_vehicles/vehicles_part2.json")
        
        vehicles = data1.get("vehicles", []) + data2.get("vehicles", [])
        
        service_url = self.get_service_url("vehicles")
        errors = []
        success = 0
        
        for vehicle in vehicles:
            url = f"{service_url}/api/vehicles"
            ok, result = self.api_post(url, vehicle)
            
            if ok:
                success += 1
                log_success(f"Vehículo creado: {vehicle['make']} {vehicle['model']} ({vehicle['year']})")
            else:
                errors.append(f"{vehicle['make']} {vehicle['model']}: {result}")
                log_error(f"Error creando vehículo: {result}")
        
        return SeedResult(
            service="VehiclesSaleService",
            entity="Vehicles",
            total=len(vehicles),
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_billing(self) -> SeedResult:
        """Seed de facturación (BillingService)"""
        log_header("SEEDING: FACTURACIÓN (BillingService)")
        
        data = self.load_json("04_billing/billing_data.json")
        service_url = self.get_service_url("billing")
        errors = []
        success = 0
        total = 0
        
        # Subscriptions
        subscriptions = data.get("subscriptions", [])
        for sub in subscriptions:
            total += 1
            url = f"{service_url}/api/billing/subscriptions"
            ok, result = self.api_post(url, sub)
            if ok:
                success += 1
            else:
                errors.append(f"Subscription {sub.get('id')}: {result}")
        
        # Payments
        payments = data.get("payments", [])
        for payment in payments:
            total += 1
            url = f"{service_url}/api/billing/payments"
            ok, result = self.api_post(url, payment)
            if ok:
                success += 1
            else:
                errors.append(f"Payment {payment.get('id')}: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="BillingService",
            entity="Billing",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_contacts(self) -> SeedResult:
        """Seed de contactos (ContactService)"""
        log_header("SEEDING: CONTACTOS (ContactService)")
        
        data = self.load_json("05_contact/contact_data.json")
        service_url = self.get_service_url("contact")
        errors = []
        success = 0
        total = 0
        
        # Contact Requests
        for request in data.get("contactRequests", []):
            total += 1
            url = f"{service_url}/api/contact/requests"
            ok, result = self.api_post(url, request)
            if ok:
                success += 1
            else:
                errors.append(f"ContactRequest {request.get('id')}: {result}")
        
        # Messages
        for message in data.get("messages", []):
            total += 1
            url = f"{service_url}/api/contact/messages"
            ok, result = self.api_post(url, message)
            if ok:
                success += 1
            else:
                errors.append(f"Message {message.get('id')}: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="ContactService",
            entity="Contacts",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_notifications(self) -> SeedResult:
        """Seed de notificaciones (NotificationService)"""
        log_header("SEEDING: NOTIFICACIONES (NotificationService)")
        
        data = self.load_json("06_notifications/notifications_data.json")
        service_url = self.get_service_url("notifications")
        errors = []
        success = 0
        total = 0
        
        # Templates
        for template in data.get("templates", []):
            total += 1
            url = f"{service_url}/api/notifications/templates"
            ok, result = self.api_post(url, template)
            if ok:
                success += 1
            else:
                errors.append(f"Template {template.get('id')}: {result}")
        
        # Notifications
        for notification in data.get("notifications", []):
            total += 1
            url = f"{service_url}/api/notifications"
            ok, result = self.api_post(url, notification)
            if ok:
                success += 1
            else:
                errors.append(f"Notification {notification.get('id')}: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="NotificationService",
            entity="Notifications",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_media(self) -> SeedResult:
        """Seed de media (MediaService)"""
        log_header("SEEDING: MEDIA (MediaService)")
        
        data = self.load_json("07_media/media_data.json")
        service_url = self.get_service_url("media")
        errors = []
        success = 0
        total = 0
        
        # Solo registrar metadatos (no subir archivos reales en seeding)
        for image in data.get("vehicleImages", []):
            total += 1
            url = f"{service_url}/api/media/register"
            ok, result = self.api_post(url, image)
            if ok:
                success += 1
            else:
                errors.append(f"Image {image.get('id')}: {result}")
        
        for avatar in data.get("userAvatars", []):
            total += 1
            url = f"{service_url}/api/media/register"
            ok, result = self.api_post(url, avatar)
            if ok:
                success += 1
            else:
                errors.append(f"Avatar {avatar.get('id')}: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="MediaService",
            entity="Media",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_dealer_management(self) -> SeedResult:
        """Seed de gestión de dealers (DealerManagementService)"""
        log_header("SEEDING: GESTIÓN DE DEALERS")
        
        data = self.load_json("08_dealer_management/dealer_management_data.json")
        service_url = self.get_service_url("dealers")
        errors = []
        success = 0
        total = 0
        
        # Locations
        for location in data.get("dealerLocations", []):
            total += 1
            url = f"{service_url}/api/dealers/{location['dealerId']}/locations"
            ok, result = self.api_post(url, location)
            if ok:
                success += 1
            else:
                errors.append(f"Location {location.get('id')}: {result}")
        
        # Staff
        for staff in data.get("dealerStaff", []):
            total += 1
            url = f"{service_url}/api/dealers/{staff['dealerId']}/staff"
            ok, result = self.api_post(url, staff)
            if ok:
                success += 1
            else:
                errors.append(f"Staff {staff.get('id')}: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="DealerManagementService",
            entity="DealerManagement",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_roles(self) -> SeedResult:
        """Seed de roles y permisos (RoleService)"""
        log_header("SEEDING: ROLES Y PERMISOS (RoleService)")
        
        data = self.load_json("09_roles/roles_data.json")
        service_url = self.get_service_url("roles")
        errors = []
        success = 0
        total = 0
        
        # Permissions primero
        for permission in data.get("permissions", []):
            total += 1
            url = f"{service_url}/api/permissions"
            ok, result = self.api_post(url, permission)
            if ok:
                success += 1
            else:
                errors.append(f"Permission {permission.get('id')}: {result}")
        
        # Roles
        for role in data.get("roles", []):
            total += 1
            url = f"{service_url}/api/roles"
            ok, result = self.api_post(url, role)
            if ok:
                success += 1
            else:
                errors.append(f"Role {role.get('id')}: {result}")
        
        # Role-Permission mappings
        for mapping in data.get("rolePermissions", []):
            total += 1
            url = f"{service_url}/api/roles/{mapping['roleId']}/permissions"
            ok, result = self.api_post(url, {"permissionIds": mapping["permissionIds"]})
            if ok:
                success += 1
            else:
                errors.append(f"RolePermission mapping: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="RoleService",
            entity="Roles",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    def seed_reviews(self) -> SeedResult:
        """Seed de reviews (ReviewService)"""
        log_header("SEEDING: REVIEWS (ReviewService)")
        
        data = self.load_json("10_reviews/reviews_data.json")
        service_url = self.get_service_url("reviews")
        errors = []
        success = 0
        total = 0
        
        # Reviews
        for review in data.get("reviews", []):
            total += 1
            url = f"{service_url}/api/reviews"
            ok, result = self.api_post(url, review)
            if ok:
                success += 1
            else:
                errors.append(f"Review {review.get('id')}: {result}")
        
        # Review Responses
        for response in data.get("reviewResponses", []):
            total += 1
            url = f"{service_url}/api/reviews/{response['reviewId']}/responses"
            ok, result = self.api_post(url, response)
            if ok:
                success += 1
            else:
                errors.append(f"ReviewResponse {response.get('id')}: {result}")
        
        # Dealer Badges
        for badge in data.get("dealerBadges", []):
            total += 1
            url = f"{service_url}/api/dealers/{badge['dealerId']}/badges"
            ok, result = self.api_post(url, badge)
            if ok:
                success += 1
            else:
                errors.append(f"Badge {badge.get('id')}: {result}")
        
        log_info(f"Procesados: {success}/{total}")
        
        return SeedResult(
            service="ReviewService",
            entity="Reviews",
            total=total,
            success=success,
            failed=len(errors),
            errors=errors
        )
    
    # ========================================================================
    # EJECUCIÓN PRINCIPAL
    # ========================================================================
    
    def run(self, only: Optional[str] = None):
        """Ejecuta el seeding completo o parcial"""
        start_time = datetime.now()
        
        log_header("OKLA DATABASE SEEDING")
        log_info(f"Ambiente: {self.env}")
        log_info(f"Gateway: {self.config['gateway']}")
        log_info(f"Dry Run: {self.dry_run}")
        log_info(f"Fecha: {start_time.strftime('%Y-%m-%d %H:%M:%S')}")
        
        # Verificar conectividad
        if not self.dry_run:
            try:
                response = requests.get(f"{self.config['gateway']}/health", timeout=5)
                if response.status_code == 200:
                    log_success("Gateway conectado")
                else:
                    log_warning(f"Gateway respondió con código {response.status_code}")
            except Exception as e:
                log_error(f"No se puede conectar al gateway: {e}")
                if input("¿Continuar de todos modos? (y/n): ").lower() != 'y':
                    return
        
        # Autenticar
        if not self.dry_run:
            self.authenticate_admin()
        
        # Orden de seeding (respetando dependencias)
        seed_order = [
            ("users", self.seed_users),
            ("roles", self.seed_roles),
            ("dealers", self.seed_dealers),
            ("dealer_management", self.seed_dealer_management),
            ("vehicles", self.seed_vehicles),
            ("billing", self.seed_billing),
            ("contacts", self.seed_contacts),
            ("reviews", self.seed_reviews),
            ("notifications", self.seed_notifications),
            ("media", self.seed_media),
        ]
        
        # Filtrar si se especificó --only
        if only:
            seed_order = [(name, func) for name, func in seed_order if name == only]
            if not seed_order:
                log_error(f"Servicio '{only}' no encontrado")
                return
        
        # Ejecutar seeders
        for name, seeder in seed_order:
            try:
                result = seeder()
                self.results.append(result)
            except Exception as e:
                log_error(f"Error en seeder {name}: {e}")
                self.results.append(SeedResult(
                    service=name,
                    entity=name,
                    total=0,
                    success=0,
                    failed=1,
                    errors=[str(e)]
                ))
        
        # Resumen final
        self.print_summary(start_time)
    
    def print_summary(self, start_time: datetime):
        """Imprime el resumen de la ejecución"""
        end_time = datetime.now()
        duration = (end_time - start_time).total_seconds()
        
        log_header("RESUMEN DE SEEDING")
        
        total_records = sum(r.total for r in self.results)
        total_success = sum(r.success for r in self.results)
        total_failed = sum(r.failed for r in self.results)
        
        print(f"\n{'Servicio':<30} {'Total':<10} {'Éxito':<10} {'Error':<10} {'%':<10}")
        print("-" * 70)
        
        for result in self.results:
            status = f"{result.success_rate:.1f}%"
            color = Colors.GREEN if result.success_rate == 100 else (Colors.YELLOW if result.success_rate > 50 else Colors.RED)
            print(f"{color}{result.service:<30} {result.total:<10} {result.success:<10} {result.failed:<10} {status:<10}{Colors.ENDC}")
        
        print("-" * 70)
        overall_rate = (total_success / total_records * 100) if total_records > 0 else 0
        print(f"{Colors.BOLD}{'TOTAL':<30} {total_records:<10} {total_success:<10} {total_failed:<10} {overall_rate:.1f}%{Colors.ENDC}")
        
        print(f"\n⏱️  Tiempo total: {duration:.2f} segundos")
        print(f"📊 Registros procesados: {total_records}")
        print(f"✅ Exitosos: {total_success}")
        print(f"❌ Fallidos: {total_failed}")
        
        if total_failed > 0:
            log_warning("\nErrores encontrados:")
            for result in self.results:
                if result.errors:
                    print(f"\n  {result.service}:")
                    for error in result.errors[:5]:  # Máximo 5 errores por servicio
                        print(f"    - {error[:100]}")
                    if len(result.errors) > 5:
                        print(f"    ... y {len(result.errors) - 5} errores más")

# ============================================================================
# MAIN
# ============================================================================

def main():
    parser = argparse.ArgumentParser(
        description="OKLA Database Seeding Script",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Ejemplos:
  python seed_database.py                     # Seed completo en desarrollo
  python seed_database.py --env docker        # Seed en Docker
  python seed_database.py --dry-run           # Ver qué haría sin ejecutar
  python seed_database.py --only vehicles     # Solo seed de vehículos
  python seed_database.py --only dealers      # Solo seed de dealers
        """
    )
    
    parser.add_argument(
        "--env",
        choices=["dev", "docker", "prod"],
        default="dev",
        help="Ambiente a usar (default: dev)"
    )
    
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Solo mostrar qué haría, sin ejecutar"
    )
    
    parser.add_argument(
        "--only",
        choices=["users", "roles", "dealers", "dealer_management", "vehicles", 
                 "billing", "contacts", "reviews", "notifications", "media"],
        help="Solo ejecutar un seeder específico"
    )
    
    args = parser.parse_args()
    
    # Confirmación para producción
    if args.env == "prod" and not args.dry_run:
        print(f"{Colors.RED}{Colors.BOLD}")
        print("=" * 60)
        print("⚠️  ADVERTENCIA: VAS A EJECUTAR EN PRODUCCIÓN")
        print("=" * 60)
        print(f"{Colors.ENDC}")
        
        confirm = input("Escribe 'CONFIRMAR PRODUCCION' para continuar: ")
        if confirm != "CONFIRMAR PRODUCCION":
            print("Cancelado.")
            sys.exit(0)
    
    seeder = DatabaseSeeder(env=args.env, dry_run=args.dry_run)
    seeder.run(only=args.only)

if __name__ == "__main__":
    main()
