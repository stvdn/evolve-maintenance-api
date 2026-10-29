# FASE 3: DISEÑO DEL MODELO DE DATOS Y ESPECIFICACIÓN OPENAPI
## MSApiMantenimientosEvolutivos - Banco Ecuador

---

**Documento de Modelo y Contrato de API**  
**Versión:** 1.0  
**Fecha:** Octubre 2026  
**Autor:** Arquitecto de Soluciones - Célula de Mantenimientos Evolutivos  
**Institución:** Banco Ecuador - Área de Tecnología  
**Contrato:** `openapi.yaml` (OpenAPI 3.0.3)

---

## TABLA DE CONTENIDOS

1. [Modelo de Datos](#1-modelo-de-datos)
2. [Inventario de Endpoints](#2-inventario-de-endpoints)
3. [Diseño de Respuestas](#3-diseño-de-respuestas)
4. [Seguridad del Contrato](#4-seguridad-del-contrato)
5. [Convenciones de Diseño](#5-convenciones-de-diseño)

---

## 1. MODELO DE DATOS

### 1.1 Diagrama Entidad-Relación

```
┌──────────────────────────┐       ┌──────────────────────────┐
│         users             │       │     maintenances          │
├──────────────────────────┤       ├──────────────────────────┤
│ PK  id          UUID     │       │ PK  id          UUID     │
│     full_name   VARCHAR  │       │     name        VARCHAR  │
│     email       VARCHAR  │◄──┐   │     description TEXT     │
│     password    VARCHAR  │   │   │     application VARCHAR  │
│     role        ENUM     │   │   │     est_hours   INTEGER  │
│     created_at  TIMESTAMP│   │   │     status      ENUM     │
│     updated_at  TIMESTAMP│   │   │     squad       VARCHAR  │
└──────────────────────────┘   │   │ FK  architect   VARCHAR  │──┐
                               │   │     created_at  TIMESTAMP│  │
                               │   │     updated_at  TIMESTAMP│  │
                               │   └──────────┬───────────────┘  │
                               │              │ 1:N              │
                               │              ▼                  │
                               │   ┌──────────────────────────┐  │
                               │   │     code_metrics          │  │
                               │   ├──────────────────────────┤  │
                               │   │ PK  id          UUID     │  │
                               │   │ FK  maint_id    UUID     │  │
                               │   │     coverage    DECIMAL  │  │
                               │   │     complexity  INTEGER  │  │
                               │   │     code_smells INTEGER  │  │
                               │   │     vulns       INTEGER  │  │
                               │   │     duplicated  DECIMAL  │  │
                               │   │     loc         INTEGER  │  │
                               │   │     tech_debt   INTEGER  │  │
                               │   │     measured_at TIMESTAMP│  │
                               │   └──────────────────────────┘  │
                               │                                  │
                               └──────────────────────────────────┘
                                  assigned_architect (email FK lógico)
```

### 1.2 Definición de Tablas

#### Tabla: `users`

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `id` | UUID | PK, NOT NULL, DEFAULT uuid_generate_v4() | Identificador único |
| `full_name` | VARCHAR(100) | NOT NULL | Nombre completo |
| `email` | VARCHAR(255) | NOT NULL, UNIQUE | Correo corporativo |
| `password_hash` | VARCHAR(255) | NOT NULL | Contraseña hasheada (bcrypt) |
| `role` | ENUM('developer', 'architect', 'manager') | NOT NULL | Rol del usuario |
| `created_at` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Fecha de creación |
| `updated_at` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Última actualización |

**Índices:**
- `idx_users_email` UNIQUE en `email` (búsqueda rápida en login)
- `idx_users_role` en `role` (filtrado por rol)

#### Tabla: `maintenances`

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `id` | UUID | PK, NOT NULL, DEFAULT uuid_generate_v4() | Identificador único |
| `name` | VARCHAR(200) | NOT NULL | Nombre del mantenimiento |
| `description` | TEXT | NULLABLE | Descripción detallada |
| `application_name` | VARCHAR(100) | NOT NULL | Aplicación bancaria afectada |
| `estimated_hours` | INTEGER | NOT NULL, CHECK (1-300) | Horas estimadas |
| `status` | ENUM('draft','in_progress','in_review','completed','cancelled') | NOT NULL, DEFAULT 'draft' | Estado actual |
| `squad` | VARCHAR(100) | NOT NULL | Squad asignado |
| `assigned_architect` | VARCHAR(255) | NOT NULL | Email del arquitecto revisor |
| `created_at` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Fecha de creación |
| `updated_at` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Última actualización |

**Índices:**
- `idx_maintenances_status` en `status` (filtrado frecuente)
- `idx_maintenances_application` en `application_name` (filtrado por app)
- `idx_maintenances_created` en `created_at DESC` (ordenamiento por defecto)

#### Tabla: `code_metrics`

| Columna | Tipo | Restricciones | Descripción |
|---------|------|---------------|-------------|
| `id` | UUID | PK, NOT NULL, DEFAULT uuid_generate_v4() | Identificador único |
| `maintenance_id` | UUID | FK → maintenances(id), NOT NULL | Mantenimiento asociado |
| `coverage_percentage` | DECIMAL(5,2) | NOT NULL, CHECK (0-100) | Cobertura de pruebas (%) |
| `cyclomatic_complexity` | INTEGER | NOT NULL, CHECK (>=0) | Complejidad ciclomática |
| `code_smells` | INTEGER | NOT NULL, CHECK (>=0) | Cantidad de code smells |
| `vulnerabilities` | INTEGER | NOT NULL, CHECK (>=0) | Vulnerabilidades detectadas |
| `duplicated_lines_percentage` | DECIMAL(5,2) | NOT NULL, CHECK (0-100) | Líneas duplicadas (%) |
| `lines_of_code` | INTEGER | NULLABLE, CHECK (>=0) | Líneas de código totales |
| `technical_debt_minutes` | INTEGER | NULLABLE, CHECK (>=0) | Deuda técnica en minutos |
| `measured_at` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Fecha de medición |

**Índices:**
- `idx_metrics_maintenance` en `maintenance_id` (JOIN frecuente)
- `idx_metrics_measured` en `maintenance_id, measured_at DESC` (historial ordenado)

**Foreign Key:**
- `maintenance_id` → `maintenances(id)` ON DELETE CASCADE

### 1.3 Relaciones

| Relación | Tipo | Descripción |
|----------|------|-------------|
| users → maintenances | 1:N (lógica) | Un arquitecto puede tener asignados múltiples mantenimientos (via `assigned_architect`) |
| maintenances → code_metrics | 1:N | Un mantenimiento puede tener múltiples mediciones históricas de métricas |

### 1.4 Script SQL de Creación

```sql
-- Extensión para generar UUIDs
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Tipos ENUM
CREATE TYPE user_role AS ENUM ('developer', 'architect', 'manager');
CREATE TYPE maintenance_status AS ENUM ('draft', 'in_progress', 'in_review', 'completed', 'cancelled');

-- Tabla de usuarios
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role user_role NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);

-- Tabla de mantenimientos
CREATE TABLE maintenances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    application_name VARCHAR(100) NOT NULL,
    estimated_hours INTEGER NOT NULL CHECK (estimated_hours BETWEEN 1 AND 300),
    status maintenance_status NOT NULL DEFAULT 'draft',
    squad VARCHAR(100) NOT NULL,
    assigned_architect VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_maintenances_status ON maintenances(status);
CREATE INDEX idx_maintenances_application ON maintenances(application_name);
CREATE INDEX idx_maintenances_created ON maintenances(created_at DESC);

-- Tabla de métricas de código
CREATE TABLE code_metrics (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    maintenance_id UUID NOT NULL REFERENCES maintenances(id) ON DELETE CASCADE,
    coverage_percentage DECIMAL(5,2) NOT NULL CHECK (coverage_percentage BETWEEN 0 AND 100),
    cyclomatic_complexity INTEGER NOT NULL CHECK (cyclomatic_complexity >= 0),
    code_smells INTEGER NOT NULL CHECK (code_smells >= 0),
    vulnerabilities INTEGER NOT NULL CHECK (vulnerabilities >= 0),
    duplicated_lines_percentage DECIMAL(5,2) NOT NULL CHECK (duplicated_lines_percentage BETWEEN 0 AND 100),
    lines_of_code INTEGER CHECK (lines_of_code >= 0),
    technical_debt_minutes INTEGER CHECK (technical_debt_minutes >= 0),
    measured_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_metrics_maintenance ON code_metrics(maintenance_id);
CREATE INDEX idx_metrics_measured ON code_metrics(maintenance_id, measured_at DESC);
```

---

## 2. INVENTARIO DE ENDPOINTS

### 2.1 Resumen de Endpoints

El contrato OpenAPI define **13 endpoints** distribuidos en 4 módulos:

| # | Método | Ruta | Descripción | Roles Permitidos |
|---|--------|------|-------------|------------------|
| 1 | POST | `/auth/register` | Registrar usuario | Público / Architect |
| 2 | POST | `/auth/login` | Iniciar sesión | Público |
| 3 | GET | `/maintenances` | Listar mantenimientos (paginado, filtros) | Todos |
| 4 | POST | `/maintenances` | Crear mantenimiento | Architect |
| 5 | GET | `/maintenances/stats` | Estadísticas consolidadas | Architect, Manager |
| 6 | GET | `/maintenances/{id}` | Detalle de un mantenimiento | Todos |
| 7 | PATCH | `/maintenances/{id}` | Actualizar mantenimiento | Architect, Developer (parcial) |
| 8 | DELETE | `/maintenances/{id}` | Eliminar mantenimiento | Architect |
| 9 | POST | `/metrics` | Registrar métricas de calidad | Todos |
| 10 | GET | `/metrics/maintenances/{id}` | Métricas actuales de un mantenimiento | Todos |
| 11 | GET | `/metrics/maintenances/{id}/history` | Historial de métricas (paginado) | Todos |
| 12 | GET | `/metrics/alerts` | Alertas de métricas bajo umbral | Architect, Manager |
| 13 | GET | `/users/me` | Perfil del usuario actual | Todos |
| 14 | PATCH | `/users/me` | Actualizar perfil propio | Todos |

### 2.2 Códigos HTTP Utilizados

| Código | Significado | Uso en la API |
|--------|-------------|---------------|
| 200 | OK | Lectura exitosa, actualización exitosa, eliminación exitosa |
| 201 | Created | Creación exitosa de mantenimiento, métrica o usuario |
| 400 | Bad Request | Datos de entrada inválidos (validación Zod) |
| 401 | Unauthorized | Token JWT ausente, expirado o malformado |
| 403 | Forbidden | Rol sin permisos para el recurso solicitado |
| 404 | Not Found | Mantenimiento o recurso no existe |
| 409 | Conflict | Email ya registrado (duplicado) |
| 429 | Too Many Requests | Rate limit excedido |
| 500 | Internal Server Error | Error no controlado del servidor |

### 2.3 Capacidades de Filtrado y Paginación

**Endpoint GET /maintenances** soporta:

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `page` | integer | Número de página (default: 1) | `?page=2` |
| `limit` | integer | Elementos por página (default: 10, max: 100) | `?limit=20` |
| `status` | enum | Filtrar por estado | `?status=in_progress` |
| `applicationName` | string | Filtrar por aplicación | `?applicationName=core-banking` |
| `squad` | string | Filtrar por squad | `?squad=Squad%20Alpha` |
| `sortBy` | enum | Campo de ordenamiento | `?sortBy=estimatedHours` |
| `order` | enum | Dirección (asc/desc) | `?order=asc` |

**Ejemplo de paginación combinada:**
```
GET /api/v1/maintenances?status=in_progress&page=1&limit=5&sortBy=createdAt&order=desc
```

---

## 3. DISEÑO DE RESPUESTAS

### 3.1 Formato Estándar de Respuesta

Todas las respuestas siguen una estructura consistente:

**Respuesta exitosa (sin paginación):**
```json
{
  "status": "success",
  "data": { ... }
}
```

**Respuesta exitosa (con paginación):**
```json
{
  "status": "success",
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 48,
    "totalPages": 5
  }
}
```

**Respuesta de error:**
```json
{
  "status": "error",
  "message": "Descripción del error",
  "statusCode": 400
}
```

**Respuesta de error de validación:**
```json
{
  "status": "error",
  "message": "Error de validación",
  "statusCode": 400,
  "errors": [
    { "field": "estimatedHours", "message": "Debe ser entre 1 y 300" },
    { "field": "email", "message": "Formato de email inválido" }
  ]
}
```

### 3.2 Headers de Rate Limiting

Cuando se excede el límite, la respuesta incluye:

| Header | Descripción | Ejemplo |
|--------|-------------|---------|
| `Retry-After` | Segundos hasta poder hacer otra petición | `60` |
| `X-RateLimit-Limit` | Límite total por minuto | `100` |
| `X-RateLimit-Remaining` | Peticiones restantes | `0` |

---

## 4. SEGURIDAD DEL CONTRATO

### 4.1 Autenticación JWT

**Flujo de autenticación:**
```
1. POST /auth/login { email, password }
2. Servidor valida credenciales contra BD (bcrypt compare)
3. Servidor genera JWT firmado con HS256
4. Cliente recibe token (válido por 8 horas)
5. Cliente incluye en cada petición: Authorization: Bearer <token>
```

**Payload del JWT:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "carlos.lopez@bancoecuador.com",
  "role": "developer",
  "iat": 1729785600,
  "exp": 1729814400
}
```

### 4.2 Matriz de Permisos RBAC

| Recurso | developer | architect | manager |
|---------|-----------|-----------|---------|
| `POST /auth/register` | ✅ (solo developer) | ✅ (cualquier rol) | ❌ |
| `POST /auth/login` | ✅ | ✅ | ✅ |
| `GET /maintenances` | ✅ (solo asignados) | ✅ (todos) | ✅ (todos) |
| `POST /maintenances` | ❌ | ✅ | ❌ |
| `GET /maintenances/stats` | ❌ | ✅ | ✅ |
| `GET /maintenances/{id}` | ✅ | ✅ | ✅ |
| `PATCH /maintenances/{id}` | ⚠️ (solo status→in_review) | ✅ | ❌ |
| `DELETE /maintenances/{id}` | ❌ | ✅ | ❌ |
| `POST /metrics` | ✅ | ✅ | ❌ |
| `GET /metrics/...` | ✅ | ✅ | ✅ |
| `GET /metrics/alerts` | ❌ | ✅ | ✅ |
| `GET /users/me` | ✅ | ✅ | ✅ |
| `PATCH /users/me` | ✅ | ✅ | ✅ |

### 4.3 Validación de Datos de Entrada

Cada endpoint valida datos con Zod antes de procesar:

| Campo | Reglas de Validación |
|-------|---------------------|
| `email` | Formato email válido, dominio @bancoecuador.com |
| `password` | Mínimo 8 caracteres, mayúscula, minúscula, número, carácter especial |
| `fullName` | 3-100 caracteres |
| `name` (mantenimiento) | 5-200 caracteres |
| `estimatedHours` | Entero entre 1 y 300 |
| `coveragePercentage` | Decimal entre 0.00 y 100.00 |
| `vulnerabilities` | Entero >= 0 |
| UUID params | Formato UUID v4 válido |

---

## 5. CONVENCIONES DE DISEÑO

### 5.1 Nomenclatura

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| URLs | kebab-case, plural para colecciones | `/maintenances`, `/code-metrics` |
| Query params | camelCase | `?applicationName=core` |
| JSON fields | camelCase | `estimatedHours`, `createdAt` |
| Base de datos | snake_case | `estimated_hours`, `created_at` |
| Enums | snake_case | `in_progress`, `in_review` |

### 5.2 Versionado

- URL path versioning: `/api/v1/...`
- Cambios breaking requieren nueva versión (`/api/v2/...`)
- Versiones anteriores se mantienen por 6 meses mínimo

### 5.3 Cómo Visualizar el Contrato

El archivo `openapi.yaml` se puede visualizar de las siguientes formas:

1. **Swagger Editor online**: Copiar contenido en [editor.swagger.io](https://editor.swagger.io)
2. **Swagger UI (integrado)**: Al ejecutar la API, disponible en `http://localhost:3000/api-docs`
3. **VS Code**: Extensión "OpenAPI (Swagger) Editor"
4. **Postman**: Importar como colección desde archivo OpenAPI

---

*Fin del Documento de Modelo y Contrato - Fase 3*  
*MSApiMantenimientosEvolutivos - Banco Ecuador*
