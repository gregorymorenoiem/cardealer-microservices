# POLÍTICA 11: GIT Y CONTROL DE VERSIONES

**Versión**: 1.0  
**Última Actualización**: 2025-11-30  
**Estado**: OBLIGATORIO ✅  
**Responsable**: Equipo de Arquitectura CarDealer

---

## 📋 RESUMEN EJECUTIVO

**POLÍTICA CRÍTICA**: Todos los desarrolladores deben seguir Git Flow con feature branches, commits atómicos con mensajes descriptivos (Conventional Commits), y pull requests con code review obligatorio. Commits directos a `main` o `develop` están PROHIBIDOS.

**Objetivo**: Mantener un historial de cambios limpio, trazable, y revertible; facilitar code reviews; y prevenir conflictos en el código.

**Alcance**: Aplica a TODOS los desarrolladores y repositorios del ecosistema CarDealer.

---

## 🎯 GIT WORKFLOW OBLIGATORIO

### Git Flow Strategy

```
main (production)
  ↑
  └── release/1.x.x (pre-production)
        ↑
        └── develop (integration)
              ↑
              ├── feature/nueva-funcionalidad
              ├── bugfix/corregir-error
              ├── hotfix/parche-critico
              └── refactor/mejora-codigo
```

**Reglas**:
- `main`: Solo código en producción. **PROTEGIDO** - No commits directos
- `develop`: Integración de features. **PROTEGIDO** - Solo via PR
- `feature/*`: Nuevas funcionalidades (merge a `develop`)
- `bugfix/*`: Corrección de bugs (merge a `develop`)
- `hotfix/*`: Parches críticos (merge a `main` y `develop`)
- `release/*`: Preparación de releases (merge a `main` y `develop`)

---

## 📝 CONVENTIONAL COMMITS

### Formato Obligatorio

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types Permitidos

| Type | Descripción | Ejemplo |
|------|-------------|---------|
| `feat` | Nueva funcionalidad | `feat(api): add endpoint to delete errors` |
| `fix` | Corrección de bug | `fix(validation): prevent SQL injection in error message` |
| `refactor` | Refactorización sin cambio de funcionalidad | `refactor(repository): simplify query logic` |
| `docs` | Cambios en documentación | `docs(readme): update installation steps` |
| `test` | Agregar o modificar tests | `test(integration): add tests for error logging` |
| `chore` | Tareas de mantenimiento | `chore(deps): update Polly to 8.4.2` |
| `style` | Formateo de código | `style(controller): fix indentation` |
| `perf` | Mejora de performance | `perf(query): add index to error_logs.timestamp` |
| `ci` | Cambios en CI/CD | `ci(github): add build workflow` |
| `build` | Cambios en sistema de build | `build(docker): optimize Dockerfile` |
| `revert` | Revertir commit anterior | `revert: revert "feat(api): add endpoint"` |

---

### Ejemplos de Commits

#### ✅ CORRECTO

```bash
# Feature
git commit -m "feat(errors): add pagination to GetAllErrors endpoint

- Add pageNumber and pageSize query parameters
- Implement PaginatedResult<T> DTO
- Add unit tests for pagination logic
- Update Swagger documentation

Closes #123"

# Bug Fix
git commit -m "fix(circuit-breaker): prevent infinite retry loop

The circuit breaker was not opening after consecutive failures,
causing infinite retries. Changed FailureRatio from 1.0 to 0.5
and added MinimumThroughput of 10.

Fixes #456"

# Refactor
git commit -m "refactor(repository): extract query logic to specification pattern

- Create ISpecification<T> interface
- Implement ErrorsByServiceSpecification
- Reduce code duplication in repository methods
- No functional changes"

# Documentation
git commit -m "docs(architecture): add C4 component diagram

Added detailed component diagram showing:
- API Layer components
- Application Layer with CQRS
- Infrastructure Layer dependencies"

# Breaking Change
git commit -m "feat(api)!: change error endpoint response format

BREAKING CHANGE: Error response now returns ErrorDto instead of raw entity.
Clients must update to new response format.

Before:
{
  \"Id\": \"123\",
  \"ServiceName\": \"ErrorService\"
}

After:
{
  \"id\": \"123\",
  \"serviceName\": \"ErrorService\",
  \"timestamp\": \"2025-11-30T10:00:00Z\"
}"
```

#### ❌ PROHIBIDO

```bash
# ❌ Mensaje genérico
git commit -m "fix bug"

# ❌ Sin type
git commit -m "updated error service"

# ❌ Sin descripción
git commit -m "feat(api): changes"

# ❌ Múltiples cambios no relacionados
git commit -m "add new endpoint, fix validation, update docs, refactor repository"

# ❌ Minúsculas en subject
git commit -m "Feat(api): Add endpoint"  # ❌ 'Feat' debe ser 'feat'

# ❌ Punto final en subject
git commit -m "feat(api): add endpoint."  # ❌ Sin punto final

# ❌ Subject muy largo (>50 chars)
git commit -m "feat(api): add a new endpoint to retrieve all error logs with pagination and filtering capabilities"
```

---

## 🌿 BRANCH NAMING CONVENTIONS

### Formato

```
<type>/<issue-number>-<short-description>
```

### Ejemplos

```bash
# Feature
feature/123-add-error-pagination
feature/456-implement-circuit-breaker

# Bug Fix
bugfix/789-fix-sql-injection
bugfix/101-correct-validation-logic

# Hotfix
hotfix/202-patch-security-vulnerability
hotfix/303-fix-production-crash

# Refactor
refactor/404-simplify-repository-pattern
refactor/505-extract-common-interfaces

# Documentation
docs/606-update-architecture-diagram
docs/707-add-troubleshooting-guide

# Chore
chore/808-update-dependencies
chore/909-configure-sonarqube
```

---

## 🔀 PULL REQUEST WORKFLOW

### 1. Crear Feature Branch

```bash
# Actualizar develop
git checkout develop
git pull origin develop

# Crear feature branch
git checkout -b feature/123-add-error-pagination

# Trabajar en la feature
# ...hacer cambios...

# Commits atómicos
git add .
git commit -m "feat(api): add pagination parameters to GetAllErrors"
git commit -m "test(api): add pagination tests"
git commit -m "docs(swagger): update API documentation"
```

---

### 2. Push y Crear PR

```bash
# Push al remote
git push origin feature/123-add-error-pagination

# Crear PR en GitHub/Azure DevOps
# Title: feat(api): add pagination to GetAllErrors endpoint
# Description: (ver template abajo)
```

---

### 3. Template de Pull Request

```markdown
## 📝 Descripción

Breve descripción de los cambios realizados.

## 🎯 Tipo de Cambio

- [ ] Feature (nueva funcionalidad)
- [ ] Bug fix (corrección de error)
- [ ] Refactor (sin cambio de funcionalidad)
- [ ] Documentation (solo documentación)
- [ ] Performance (mejora de performance)
- [ ] Breaking change (cambio que rompe compatibilidad)

## 🔗 Issue Relacionado

Closes #123

## 📋 Checklist

### Código
- [ ] El código sigue las políticas de Clean Architecture
- [ ] Se agregaron tests unitarios (cobertura >80%)
- [ ] Se agregaron tests de integración
- [ ] No hay código comentado o debug logs
- [ ] No hay TODOs sin issue asociado

### Seguridad
- [ ] Input validation implementada (FluentValidation)
- [ ] SQL Injection / XSS prevention verificado
- [ ] Authentication/Authorization verificado
- [ ] Secrets no expuestos en código

### Resiliencia
- [ ] Circuit Breaker configurado (si aplica)
- [ ] Retry policies implementadas (si aplica)
- [ ] Timeout configurado
- [ ] Error handling apropiado

### Documentación
- [ ] README.md actualizado (si aplica)
- [ ] CHANGELOG.md actualizado
- [ ] XML comments agregados
- [ ] Swagger documentation actualizada
- [ ] ARCHITECTURE.md actualizado (si aplica)

### Testing
- [ ] Todos los tests pasan localmente
- [ ] Build exitoso
- [ ] No hay warnings de compilación
- [ ] Cobertura de código >80%

### Base de Datos (si aplica)
- [ ] Migración creada
- [ ] Migración probada (up y down)
- [ ] Índices apropiados agregados
- [ ] No hay queries N+1

## 🧪 Cómo Probar

1. Checkout del branch
   ```bash
   git checkout feature/123-add-error-pagination
   ```

2. Restaurar dependencias
   ```bash
   dotnet restore
   ```

3. Ejecutar tests
   ```bash
   dotnet test
   ```

4. Ejecutar aplicación
   ```bash
   dotnet run --project ErrorService.Api
   ```

5. Probar endpoint
   ```bash
   curl "https://localhost:5001/api/errors?pageNumber=1&pageSize=10"
   ```

## 📸 Screenshots (si aplica)

[Agregar capturas de pantalla de UI o Swagger]

## 🔄 Impacto en Otros Servicios

- [ ] No hay impacto en otros servicios
- [ ] Requiere actualización en [Servicio X]
- [ ] Breaking change - requiere migración

## 📝 Notas Adicionales

Cualquier información adicional que los reviewers deban saber.
```

---

### 4. Code Review Checklist

#### Para el Reviewer

```markdown
## 🔍 Code Review Checklist

### Arquitectura
- [ ] Sigue Clean Architecture (capas correctas)
- [ ] CQRS implementado correctamente
- [ ] Dependency injection apropiado
- [ ] No hay dependencias circulares

### Código
- [ ] Nombres descriptivos (clases, métodos, variables)
- [ ] Métodos pequeños (<20 líneas idealmente)
- [ ] No hay código duplicado
- [ ] No hay magic numbers o strings
- [ ] Manejo de nulls apropiado

### Testing
- [ ] Tests unitarios presentes y pasando
- [ ] Tests de integración presentes (si aplica)
- [ ] Cobertura >80%
- [ ] Tests son legibles y mantenibles
- [ ] Arrange-Act-Assert pattern

### Seguridad
- [ ] Input validation con FluentValidation
- [ ] SQL Injection prevention
- [ ] XSS prevention
- [ ] Authentication/Authorization correcta
- [ ] No hay secrets hardcodeados

### Performance
- [ ] No hay queries N+1
- [ ] Paginación implementada (si aplica)
- [ ] Índices de BD apropiados
- [ ] No hay blocking calls innecesarios

### Documentación
- [ ] XML comments presentes
- [ ] README actualizado
- [ ] CHANGELOG actualizado
- [ ] Swagger actualizado

### General
- [ ] Commit messages siguen Conventional Commits
- [ ] No hay merge conflicts
- [ ] Build exitoso
- [ ] No hay warnings
```

---

### 5. Merge Strategy

```bash
# Opción 1: Squash and Merge (RECOMENDADO para features)
# Combina todos los commits en uno solo
# Mantiene historial limpio en develop/main

# Opción 2: Rebase and Merge
# Mantiene commits individuales pero sin merge commit
# Útil cuando los commits ya están bien organizados

# Opción 3: Merge Commit
# Crea merge commit explícito
# Útil para releases o integraciones grandes

# ❌ PROHIBIDO: Merge sin PR
git checkout develop
git merge feature/123-add-error-pagination  # ❌ NO HACER
```

**REGLA**: SIEMPRE usar Pull Request, nunca merge directo.

---

## 🏷️ TAGGING Y RELEASES

### Semantic Versioning

```
MAJOR.MINOR.PATCH

MAJOR: Breaking changes (incompatible API changes)
MINOR: New features (backward compatible)
PATCH: Bug fixes (backward compatible)
```

**Ejemplos**:
- `1.0.0` - Primera versión estable
- `1.1.0` - Nueva feature (pagination)
- `1.1.1` - Bug fix (validation error)
- `2.0.0` - Breaking change (API restructure)

---

### Crear Release

```bash
# 1. Crear release branch desde develop
git checkout develop
git pull origin develop
git checkout -b release/1.1.0

# 2. Actualizar versión en archivos
# - *.csproj <Version>1.1.0</Version>
# - CHANGELOG.md

# 3. Commit de versión
git commit -am "chore(release): bump version to 1.1.0"

# 4. Push release branch
git push origin release/1.1.0

# 5. Crear PR a main
# Title: "chore(release): version 1.1.0"

# 6. Después de merge a main, crear tag
git checkout main
git pull origin main
git tag -a v1.1.0 -m "Release version 1.1.0

Features:
- Add pagination to GetAllErrors endpoint
- Implement Circuit Breaker for external calls

Bug Fixes:
- Fix SQL injection vulnerability in validation

Breaking Changes:
- None"

# 7. Push tag
git push origin v1.1.0

# 8. Merge release branch a develop
git checkout develop
git merge release/1.1.0
git push origin develop

# 9. Eliminar release branch
git branch -d release/1.1.0
git push origin --delete release/1.1.0
```

---

### CHANGELOG.md Format

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Feature in progress

## [1.1.0] - 2025-11-30

### Added
- Pagination to GetAllErrors endpoint with pageNumber and pageSize parameters
- Circuit Breaker configuration for external HTTP calls
- PaginatedResult<T> DTO for consistent pagination responses

### Changed
- Updated Polly to version 8.4.2
- Improved error logging with structured logging

### Fixed
- SQL injection vulnerability in error message validation
- Memory leak in RabbitMQ connection factory

### Security
- Added XSS prevention in FluentValidation rules

## [1.0.1] - 2025-11-15

### Fixed
- Null reference exception in GetErrorById when error not found

## [1.0.0] - 2025-11-01

### Added
- Initial release
- CRUD operations for error logs
- JWT authentication
- PostgreSQL persistence
- RabbitMQ messaging
- OpenTelemetry tracing
- Prometheus metrics

[Unreleased]: https://github.com/cardealer/errorservice/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/cardealer/errorservice/compare/v1.0.1...v1.1.0
[1.0.1]: https://github.com/cardealer/errorservice/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/cardealer/errorservice/releases/tag/v1.0.0
```

---

## 🔥 HOTFIX WORKFLOW

### Cuando Usar Hotfix

- Bug crítico en producción
- Vulnerabilidad de seguridad
- Data corruption
- Service downtime

**NO usar hotfix para**: Features, refactors, bugs no críticos

---

### Proceso de Hotfix

```bash
# 1. Crear hotfix branch desde main
git checkout main
git pull origin main
git checkout -b hotfix/critical-security-patch

# 2. Implementar fix
# ...hacer cambios...

# 3. Commit
git commit -m "fix(security)!: patch SQL injection vulnerability

CRITICAL: SQL injection vulnerability in error message field.
Added parameterized queries and input sanitization.

Affects: ErrorService API versions 1.0.0 - 1.1.0
CVE: CVE-2025-XXXX

BREAKING CHANGE: Error message field now limited to 5000 characters."

# 4. Ejecutar tests
dotnet test

# 5. Push hotfix
git push origin hotfix/critical-security-patch

# 6. Crear PR a main (URGENTE)
# Title: "fix(security)!: patch SQL injection vulnerability"
# Reviewers: Tech Lead + Security Team

# 7. Después de merge a main, crear tag
git checkout main
git pull origin main
git tag -a v1.1.2 -m "Hotfix: Security patch"
git push origin v1.1.2

# 8. Merge hotfix a develop también
git checkout develop
git merge hotfix/critical-security-patch
git push origin develop

# 9. Eliminar hotfix branch
git branch -d hotfix/critical-security-patch
git push origin --delete hotfix/critical-security-patch

# 10. Notificar a equipo
# Slack, email, incident report
```

---

## 🔒 BRANCH PROTECTION RULES

### Configuración en GitHub/Azure DevOps

#### Branch: `main`

```yaml
Protection Rules:
  - Require pull request before merging: ✅
  - Require approvals: 2 (Tech Lead + Senior Dev)
  - Dismiss stale approvals: ✅
  - Require review from Code Owners: ✅
  - Require status checks to pass: ✅
    - Build
    - Unit Tests
    - Integration Tests
    - Code Coverage >80%
    - SonarQube Quality Gate
  - Require branches to be up to date: ✅
  - Require signed commits: ✅
  - Include administrators: ✅
  - Restrict who can push: Admins only
  - Allow force pushes: ❌
  - Allow deletions: ❌
```

#### Branch: `develop`

```yaml
Protection Rules:
  - Require pull request before merging: ✅
  - Require approvals: 1 (Any Senior Dev)
  - Dismiss stale approvals: ✅
  - Require status checks to pass: ✅
    - Build
    - Unit Tests
    - Integration Tests
  - Require branches to be up to date: ✅
  - Allow force pushes: ❌
  - Allow deletions: ❌
```

---

## 🛠️ GIT HOOKS

### Pre-Commit Hook

```bash
# .git/hooks/pre-commit
#!/bin/sh

echo "Running pre-commit checks..."

# 1. Verificar que no hay secrets
echo "Checking for secrets..."
if git diff --cached | grep -i "password\|secret\|api.key\|token" > /dev/null; then
    echo "❌ ERROR: Possible secret found in staged files"
    exit 1
fi

# 2. Verificar formato de código
echo "Checking code format..."
dotnet format --verify-no-changes
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Code formatting issues found. Run 'dotnet format'"
    exit 1
fi

# 3. Ejecutar tests unitarios
echo "Running unit tests..."
dotnet test --filter "Category=Unit" --no-build
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Unit tests failed"
    exit 1
fi

echo "✅ Pre-commit checks passed"
exit 0
```

---

### Commit-Msg Hook (Conventional Commits)

```bash
# .git/hooks/commit-msg
#!/bin/sh

commit_msg_file=$1
commit_msg=$(cat "$commit_msg_file")

# Regex para Conventional Commits
pattern="^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\([a-z0-9-]+\))?!?: .{1,50}"

if ! echo "$commit_msg" | grep -qE "$pattern"; then
    echo "❌ ERROR: Commit message does not follow Conventional Commits format"
    echo ""
    echo "Format: <type>(<scope>): <subject>"
    echo ""
    echo "Examples:"
    echo "  feat(api): add pagination to errors endpoint"
    echo "  fix(validation): prevent SQL injection"
    echo "  docs(readme): update installation steps"
    echo ""
    echo "Types: feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert"
    exit 1
fi

# Verificar longitud del subject (max 50 chars)
subject=$(echo "$commit_msg" | head -n 1 | sed 's/^[^:]*: //')
if [ ${#subject} -gt 50 ]; then
    echo "❌ ERROR: Commit subject too long (max 50 characters)"
    exit 1
fi

echo "✅ Commit message format valid"
exit 0
```

---

### Pre-Push Hook

```bash
# .git/hooks/pre-push
#!/bin/sh

echo "Running pre-push checks..."

# 1. Ejecutar todos los tests
echo "Running all tests..."
dotnet test
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Tests failed"
    exit 1
fi

# 2. Verificar cobertura de código
echo "Checking code coverage..."
dotnet test /p:CollectCoverage=true /p:Threshold=80
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Code coverage below 80%"
    exit 1
fi

# 3. Verificar build
echo "Running build..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build failed"
    exit 1
fi

echo "✅ Pre-push checks passed"
exit 0
```

---

## 📊 GIT BEST PRACTICES

### Commits Atómicos

```bash
# ✅ CORRECTO - Commits pequeños y enfocados
git add ErrorService.Api/Controllers/ErrorsController.cs
git commit -m "feat(api): add GetById endpoint"

git add ErrorService.Application/Queries/GetErrorByIdQuery.cs
git commit -m "feat(query): implement GetErrorById query"

git add ErrorService.Tests/Queries/GetErrorByIdQueryTests.cs
git commit -m "test(query): add tests for GetErrorById"

# ❌ PROHIBIDO - Un commit gigante con todo
git add .
git commit -m "add new feature"
```

---

### Rebase vs Merge

```bash
# ✅ RECOMENDADO - Rebase antes de PR para historial limpio
git checkout feature/123-add-pagination
git fetch origin
git rebase origin/develop

# Si hay conflictos, resolverlos
git add .
git rebase --continue

git push --force-with-lease origin feature/123-add-pagination

# ❌ EVITAR - Merge commits frecuentes en feature branch
git merge develop  # Crea merge commits innecesarios
```

---

### Limpiar Historial

```bash
# ✅ Combinar commits antes de PR (interactive rebase)
git rebase -i HEAD~5

# Editor se abre:
pick a1b2c3d feat(api): add endpoint
pick d4e5f6g test(api): add tests
pick h7i8j9k docs(api): update swagger
pick k0l1m2n fix(api): typo in response
pick n3o4p5q refactor(api): simplify logic

# Cambiar a:
pick a1b2c3d feat(api): add endpoint
squash d4e5f6g test(api): add tests
squash h7i8j9k docs(api): update swagger
squash k0l1m2n fix(api): typo in response
squash n3o4p5q refactor(api): simplify logic

# Resultado: 1 commit limpio
# "feat(api): add endpoint with tests and documentation"
```

---

### Cherry-Pick

```bash
# Aplicar commit específico a otro branch
git checkout hotfix/security-patch
git cherry-pick a1b2c3d  # Commit con el fix

# Si hay conflictos
git cherry-pick --continue

# O abortar
git cherry-pick --abort
```

---

## 🔍 GIT COMMANDS ÚTILES

### Ver Historial Limpio

```bash
# Historial compacto
git log --oneline --graph --decorate --all

# Últimos 10 commits
git log --oneline -10

# Commits por autor
git log --author="John Doe" --oneline

# Commits en rango de fechas
git log --since="2025-11-01" --until="2025-11-30" --oneline
```

---

### Buscar en Historial

```bash
# Buscar por mensaje de commit
git log --grep="pagination" --oneline

# Buscar cambios en archivo específico
git log --oneline -- ErrorService.Api/Controllers/ErrorsController.cs

# Buscar quién modificó línea específica
git blame ErrorService.Api/Controllers/ErrorsController.cs

# Buscar cuándo se introdujo un string
git log -S "PageSize" --oneline
```

---

### Deshacer Cambios

```bash
# Deshacer cambios en working directory
git checkout -- ErrorService.Api/Controllers/ErrorsController.cs

# Unstage archivo
git reset HEAD ErrorService.Api/Controllers/ErrorsController.cs

# Deshacer último commit (mantener cambios)
git reset --soft HEAD~1

# Deshacer último commit (descartar cambios)
git reset --hard HEAD~1

# Revertir commit específico (crea nuevo commit)
git revert a1b2c3d
```

---

### Stash

```bash
# Guardar cambios temporalmente
git stash save "WIP: implementing pagination"

# Listar stashes
git stash list

# Aplicar último stash
git stash pop

# Aplicar stash específico
git stash apply stash@{1}

# Ver contenido de stash
git stash show -p stash@{0}

# Eliminar stash
git stash drop stash@{0}

# Limpiar todos los stashes
git stash clear
```

---

## ✅ CHECKLIST DE GIT

### Antes de Commit
- [ ] Código compila sin errores
- [ ] Todos los tests pasan
- [ ] No hay código comentado o debug logs
- [ ] No hay TODOs sin issue
- [ ] No hay secrets o contraseñas
- [ ] Formato de código correcto (`dotnet format`)

### Commit Message
- [ ] Sigue Conventional Commits format
- [ ] Type correcto (feat, fix, docs, etc.)
- [ ] Scope apropiado
- [ ] Subject descriptivo (<50 chars)
- [ ] Body explica el "por qué" (si necesario)
- [ ] Referencias a issues (Closes #123)

### Antes de Push
- [ ] Branch actualizado con develop/main
- [ ] Conflictos resueltos
- [ ] Tests pasan localmente
- [ ] Cobertura >80%
- [ ] Build exitoso

### Pull Request
- [ ] PR title sigue Conventional Commits
- [ ] Description completa con template
- [ ] Checklist completo
- [ ] Reviewers asignados
- [ ] Labels apropiados (feature, bug, etc.)
- [ ] Milestone asignado (si aplica)

### Code Review
- [ ] Al menos 1 approval (develop)
- [ ] Al menos 2 approvals (main)
- [ ] Todos los comments resueltos
- [ ] CI/CD checks pasando
- [ ] No hay merge conflicts

### Merge
- [ ] Squash commits (si tiene muchos commits)
- [ ] Delete branch después de merge
- [ ] Update CHANGELOG.md
- [ ] Tag version (si es release)

---

## 🚫 ANTI-PATRONES DE GIT

### ❌ PROHIBIDO

```bash
# ❌ Commits gigantes
git add .
git commit -m "many changes"

# ❌ Commit directa a main/develop
git checkout main
git commit -am "quick fix"  # ❌ NO!

# ❌ Force push a branches compartidos
git push --force origin develop  # ❌ NUNCA!

# ❌ Merge sin PR
git checkout develop
git merge feature/123  # ❌ Usar PR

# ❌ Secrets en commits
git commit -m "add config" 
# appsettings.json contains password  # ❌ NO!

# ❌ Mensajes de commit inútiles
git commit -m "fix"
git commit -m "update"
git commit -m "changes"
git commit -m "asdf"
git commit -m "test"

# ❌ Commits con código roto
git commit -m "WIP - not working yet"  # ❌ NO PUSHEAR

# ❌ Resolver conflictos aceptando todo
git checkout --theirs .  # ❌ Revisar conflicto por conflicto
```

### ✅ CORRECTO

```bash
# ✅ Commits atómicos y descriptivos
git add ErrorService.Api/Controllers/ErrorsController.cs
git commit -m "feat(api): add pagination to GetAllErrors endpoint"

# ✅ PR para todo
git push origin feature/123-add-pagination
# Crear PR en GitHub

# ✅ Squash and merge
# En GitHub: "Squash and merge" button

# ✅ Secrets en User Secrets o Key Vault
dotnet user-secrets set "Jwt:SecretKey" "value"

# ✅ Mensajes descriptivos
git commit -m "fix(validation): prevent SQL injection in error message

Added regex validation to detect SQL injection patterns.
Updated FluentValidation rules with SqlInjectionPattern.

Fixes #456"

# ✅ Solo commitear código que compila y pasa tests
dotnet test
git commit -m "feat(api): add pagination"

# ✅ Resolver conflictos manualmente
git mergetool
# Revisar archivo por archivo
git add .
git rebase --continue
```

---

## 📚 RECURSOS Y REFERENCIAS

- **Conventional Commits**: [https://www.conventionalcommits.org/](https://www.conventionalcommits.org/)
- **Git Flow**: [https://nvie.com/posts/a-successful-git-branching-model/](https://nvie.com/posts/a-successful-git-branching-model/)
- **Keep a Changelog**: [https://keepachangelog.com/](https://keepachangelog.com/)
- **Semantic Versioning**: [https://semver.org/](https://semver.org/)
- **Git Best Practices**: [https://git-scm.com/book/en/v2](https://git-scm.com/book/en/v2)

---

**Fecha de Vigencia**: 2025-11-30  
**Aprobado por**: Equipo de Arquitectura CarDealer  
**Revisión**: Trimestral

**NOTA**: Git workflow es OBLIGATORIO. Commits directos a `main` o `develop` resultan en reversión inmediata y notificación al equipo de arquitectura.
