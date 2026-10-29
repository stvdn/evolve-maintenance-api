# FASE 2: ARQUITECTURA DE SOFTWARE Y PATRONES DE DISEÑO
## MSApiMantenimientosEvolutivos - Banco Ecuador

---

**Documento de Arquitectura Técnica**  
**Versión:** 1.0  
**Fecha:** Octubre 2026  
**Autor:** Arquitecto de Soluciones - Célula de Mantenimientos Evolutivos  
**Institución:** Banco Ecuador - Área de Tecnología  
**API:** MSApiMantenimientosEvolutivos

---

## TABLA DE CONTENIDOS

1. [Evaluación de Estilos Arquitectónicos](#1-evaluación-de-estilos-arquitectónicos)
2. [Estilo Arquitectónico Seleccionado](#2-estilo-arquitectónico-seleccionado)
3. [Patrones de Diseño Seleccionados](#3-patrones-de-diseño-seleccionados)
4. [Arquitectura de Software](#4-arquitectura-de-software)
5. [Stack Tecnológico](#5-stack-tecnológico)
6. [Diagramas de Referencia](#6-diagramas-de-referencia)

---

## 1. EVALUACIÓN DE ESTILOS ARQUITECTÓNICOS

Antes de seleccionar el estilo arquitectónico, se evaluaron las opciones disponibles contra los requerimientos operacionales de Banco Ecuador: un equipo de 10 personas (8 desarrolladores, 2 arquitectos), 4 mantenimientos evolutivos por mes, y la necesidad de una solución mantenible a largo plazo.

### 1.1 Matriz de Evaluación

| Criterio | RESTful Monolítico Modular | Microservicios | gRPC | Event-Driven | Serverless/FaaS |
|----------|---------------------------|----------------|------|-------------|-----------------|
| Complejidad de desarrollo | ✅ Baja | ❌ Alta | ⚠️ Media | ❌ Alta | ⚠️ Media |
| Curva de aprendizaje del equipo | ✅ Baja (equipo ya usa REST) | ❌ Alta | ❌ Alta (Protocol Buffers) | ⚠️ Media | ⚠️ Media |
| Costo de infraestructura | ✅ Bajo (1 servidor) | ❌ Alto (N servicios) | ⚠️ Medio | ❌ Alto | ⚠️ Variable |
| Documentación OpenAPI | ✅ Nativo | ✅ Por servicio | ❌ Usa Proto files | ⚠️ AsyncAPI | ⚠️ Parcial |
| Escalabilidad | ⚠️ Vertical + Horizontal | ✅ Independiente | ✅ Alta | ✅ Alta | ✅ Automática |
| Mantenibilidad | ✅ Alta (si es modular) | ⚠️ Compleja | ⚠️ Media | ❌ Compleja | ⚠️ Vendor lock-in |
| Time-to-market | ✅ Rápido | ❌ Lento | ⚠️ Medio | ❌ Lento | ⚠️ Medio |
| Adecuado para equipo de 10 | ✅ Ideal | ❌ Requiere +20 | ⚠️ Posible | ❌ Requiere +15 | ⚠️ Posible |

### 1.2 Justificación de Descarte

**Microservicios**: Requieren infraestructura de orquestación (Kubernetes), service discovery, gestión de comunicación inter-servicios y equipos independientes por servicio. Con 10 personas y 4 mantenimientos mensuales, la sobrecarga operacional supera ampliamente los beneficios. La ley de Conway indica que la arquitectura debe reflejar la estructura organizacional: un equipo pequeño y cohesionado se beneficia de un monolito modular.

**gRPC**: Diseñado para comunicación de ultra-baja latencia entre servicios internos. MSApiMantenimientosEvolutivos no tiene ese requerimiento. Además, el equipo no tiene experiencia con Protocol Buffers, y el taller exige contrato OpenAPI, que es nativo de REST.

**Event-Driven**: Justificable cuando hay procesamiento asíncrono complejo, como flujos de eventos en tiempo real. El volumen de 4 mantenimientos/mes no requiere colas de mensajes ni event brokers. Añadiría complejidad sin beneficio proporcional.

**Serverless/FaaS**: Genera dependencia del proveedor cloud (vendor lock-in), dificulta pruebas locales, y el modelo de cobro por invocación no se justifica para un sistema interno de uso continuo durante horario laboral.

---

## 2. ESTILO ARQUITECTÓNICO SELECCIONADO

### 2.1 RESTful - Monolítico Modular

Se selecciona una arquitectura **RESTful** implementada como un **monolito modular por capas**, donde cada módulo funcional (mantenimientos, métricas, autenticación) está claramente separado en su propia estructura de controlador, servicio y repositorio.

### 2.2 Justificación Técnica

| Factor | Detalle |
|--------|---------|
| **Tamaño del equipo** | 10 personas → Un monolito modular permite que todos trabajen en el mismo repositorio con convenciones claras |
| **Volumen operacional** | 4 mantenimientos/mes → No hay necesidad de escalamiento independiente por módulo |
| **Experiencia del equipo** | El equipo ya trabaja con APIs REST → Cero curva de aprendizaje |
| **Requisito del taller** | Se exige contrato OpenAPI → REST genera OpenAPI de forma nativa |
| **Despliegue** | Un solo artefacto desplegable (contenedor Docker) → Simplicidad operacional |
| **Testing** | Pruebas unitarias y de integración en un solo proyecto → Mayor cobertura con menos esfuerzo |

### 2.3 Principios Arquitectónicos

La implementación sigue estos principios:

1. **Separación por capas**: Controller → Service → Repository → Database
2. **Responsabilidad única**: Cada módulo gestiona un solo dominio de negocio
3. **Inversión de dependencias**: Los servicios dependen de abstracciones (interfaces), no de implementaciones concretas
4. **Stateless**: Cada petición HTTP contiene toda la información necesaria (JWT en header), permitiendo escalar horizontalmente
5. **Contract-First**: El contrato OpenAPI se define antes de la implementación

---

## 3. PATRONES DE DISEÑO SELECCIONADOS

Se seleccionaron 2 patrones de diseño orientados a APIs que cubren las necesidades operacionales del sistema: un punto de entrada centralizado y resiliencia ante fallos de servicios externos.

---

### 3.1 Patrón API Gateway

#### 3.1.1 Descripción

El patrón API Gateway establece un punto de entrada único para todas las peticiones que llegan a la API. Actúa como intermediario entre los clientes y los servicios internos, encapsulando responsabilidades transversales (cross-cutting concerns) como autenticación, rate limiting, logging y versionado.

#### 3.1.2 Problema que Resuelve

Sin un API Gateway, cada controlador tendría que implementar su propia validación de JWT, su propio rate limiting y su propio logging. Esto genera:
- Código duplicado en cada módulo
- Inconsistencias en la aplicación de políticas de seguridad
- Dificultad para cambiar políticas globales (ej: cambiar el algoritmo JWT requeriría modificar todos los controladores)

#### 3.1.3 Implementación en MSApiMantenimientosEvolutivos

El API Gateway se implementa como una cadena de middlewares en Express.js que procesan cada petición antes de llegar al controlador:

```
Petición HTTP
    │
    ▼
┌─────────────────────┐
│   Rate Limiter       │  → Máximo 100 peticiones/minuto por usuario
│   (express-rate-limit)│
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│   CORS Handler       │  → Permite solo orígenes autorizados del banco
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│   Request Logger     │  → Registra método, ruta, IP, timestamp
│   (morgan + winston) │
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│   JWT Validator      │  → Valida token, extrae rol del usuario
│   (jsonwebtoken)     │
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│   RBAC Middleware    │  → Verifica permisos según rol (developer/architect/manager)
└─────────┬───────────┘
          ▼
┌─────────────────────┐
│   Route Handler      │  → Dirige al controlador correspondiente
│   (/api/v1/...)      │
└─────────┬───────────┘
          ▼
      Controller
```

#### 3.1.4 Configuración de Rate Limiting

| Rol | Límite por Minuto | Justificación |
|-----|-------------------|---------------|
| `developer` | 60 req/min | Uso operativo normal |
| `architect` | 100 req/min | Revisiones y consultas frecuentes |
| `manager` | 30 req/min | Consultas esporádicas de reportes |

#### 3.1.5 Beneficios para Banco Ecuador

- **Seguridad centralizada**: Un solo punto donde se valida JWT y permisos RBAC
- **Auditoría**: Cada petición queda registrada con usuario, IP, timestamp y resultado
- **Control de tráfico**: Rate limiting previene abuso o errores de integración que generen tráfico excesivo
- **Versionado**: Permite evolucionar la API (/api/v1, /api/v2) sin romper clientes existentes

---

### 3.2 Patrón Circuit Breaker

#### 3.2.1 Descripción

El patrón Circuit Breaker protege al sistema de fallos en cascada cuando un servicio externo deja de responder. Funciona como un interruptor eléctrico: cuando detecta que un servicio falla repetidamente, "abre el circuito" y deja de enviar peticiones, evitando que el sistema principal se degrade.

#### 3.2.2 Problema que Resuelve

MSApiMantenimientosEvolutivos registra métricas de calidad que en el futuro pueden provenir de integraciones con servicios externos (SonarQube, GitHub/GitLab). Si uno de estos servicios se cae o responde lentamente:
- Las peticiones al servicio externo se acumulan (timeouts)
- Los hilos del servidor se bloquean esperando respuesta
- El rendimiento general de la API se degrada
- Otros módulos que no dependen del servicio externo también se ven afectados

#### 3.2.3 Estados del Circuit Breaker

```
                    ┌──────────────────┐
      Éxito         │                  │    N fallos
    ┌───────────── │     CERRADO      │ ──────────────┐
    │               │   (Operación     │               │
    │               │    normal)       │               │
    │               └──────────────────┘               │
    │                                                   ▼
┌───┴──────────────┐                    ┌──────────────────┐
│                  │                    │                  │
│   SEMI-ABIERTO   │◄───────────────── │     ABIERTO      │
│  (Prueba con 1   │   Timeout expira  │  (Rechaza todas  │
│   petición)      │                    │   las peticiones) │
│                  │                    │                  │
└──────┬───────────┘                    └──────────────────┘
       │                                        ▲
       │              Fallo                     │
       └────────────────────────────────────────┘
```

**CERRADO (Closed)**: Estado normal. Todas las peticiones pasan al servicio externo. Si se acumulan N fallos consecutivos, el circuito se abre.

**ABIERTO (Open)**: Las peticiones al servicio externo se rechazan inmediatamente sin intentar la conexión. Se devuelve un valor por defecto o un error controlado. Después de un timeout configurable, pasa a semi-abierto.

**SEMI-ABIERTO (Half-Open)**: Se permite una sola petición de prueba al servicio externo. Si tiene éxito, el circuito se cierra. Si falla, el circuito se abre nuevamente.

#### 3.2.4 Configuración en MSApiMantenimientosEvolutivos

| Parámetro | Valor | Justificación |
|-----------|-------|---------------|
| Umbral de fallos (failureThreshold) | 5 fallos consecutivos | Tolerancia razonable antes de abrir |
| Timeout de reset (resetTimeout) | 30 segundos | Tiempo antes de intentar reconexión |
| Timeout de petición (requestTimeout) | 10 segundos | Máximo tiempo de espera por respuesta |
| Respuesta por defecto | `{ "status": "service_unavailable", "fallback": true }` | Respuesta degradada pero controlada |

#### 3.2.5 Aplicación en MSApiMantenimientosEvolutivos

El Circuit Breaker se aplica en el módulo de métricas, específicamente cuando se consultan o registran métricas que podrían depender de servicios externos:

```
MetricsService
    │
    ▼
┌─────────────────────┐
│   Circuit Breaker    │
│   Wrapper            │
└─────────┬───────────┘
          │
    ┌─────┴─────┐
    │           │
 Circuito    Circuito
 CERRADO     ABIERTO
    │           │
    ▼           ▼
 Llamar      Retornar
 servicio    respuesta
 externo     por defecto
    │
    ▼
 SonarQube / GitHub API
```

#### 3.2.6 Beneficios para Banco Ecuador

- **Resiliencia**: Si SonarQube o GitHub están caídos, la API sigue funcionando para registrar y consultar mantenimientos
- **Recuperación automática**: No requiere intervención manual para reconectar cuando el servicio externo se recupera
- **Visibilidad**: El estado del circuit breaker se puede monitorizar vía endpoint de health check
- **Protección de recursos**: Evita saturar el pool de conexiones con peticiones que van a fallar

---

## 4. ARQUITECTURA DE SOFTWARE

### 4.1 Vista de Capas

La arquitectura se organiza en 5 capas horizontales con flujo de datos de arriba hacia abajo:

```
┌─────────────────────────────────────────────────────────────────┐
│                     CAPA DE CLIENTES                            │
│         Dashboard Web  │  API Consumer (Postman/cURL)           │
└─────────────────────────────┬───────────────────────────────────┘
                              │ HTTPS
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  PATRÓN API GATEWAY                              │
│   Rate Limiter → CORS → Logger → JWT Validator → RBAC → Router │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  CAPA DE CONTROLADORES                           │
│   MaintenancesController │ MetricsController │ AuthController   │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  CAPA DE SERVICIOS                               │
│   MaintenancesService  │  MetricsService  │  AuthService        │
│                        │  (Circuit Breaker)│                     │
└─────────────────┬───────────┬───────────────────────────────────┘
                  │           │
                  ▼           ▼
┌─────────────────────┐  ┌──────────────────┐
│ CAPA DE DATOS       │  │ SERVICIOS        │
│ MaintenanceRepo     │  │ EXTERNOS         │
│ MetricsRepo         │  │ (SonarQube,      │
│ UserRepo            │  │  GitHub)          │
│       │             │  └──────────────────┘
│       ▼             │
│   PostgreSQL        │
└─────────────────────┘
```

### 4.2 Estructura de Módulos

```
src/
├── config/                    # Configuración de la aplicación
│   ├── database.ts            # Conexión a PostgreSQL
│   ├── environment.ts         # Variables de entorno
│   └── swagger.ts             # Configuración OpenAPI
│
├── middlewares/                # PATRÓN API GATEWAY (middlewares)
│   ├── rateLimiter.ts         # Rate limiting por rol
│   ├── cors.ts                # Configuración CORS
│   ├── logger.ts              # Request logging
│   ├── auth.ts                # Validación JWT
│   ├── rbac.ts                # Control de acceso por rol
│   └── errorHandler.ts        # Manejo global de errores
│
├── modules/
│   ├── maintenances/          # Módulo de Mantenimientos
│   │   ├── maintenance.controller.ts
│   │   ├── maintenance.service.ts
│   │   ├── maintenance.repository.ts
│   │   ├── maintenance.model.ts
│   │   ├── maintenance.routes.ts
│   │   └── maintenance.validator.ts
│   │
│   ├── metrics/               # Módulo de Métricas
│   │   ├── metrics.controller.ts
│   │   ├── metrics.service.ts     # Usa Circuit Breaker
│   │   ├── metrics.repository.ts
│   │   ├── metrics.model.ts
│   │   ├── metrics.routes.ts
│   │   └── metrics.validator.ts
│   │
│   └── auth/                  # Módulo de Autenticación
│       ├── auth.controller.ts
│       ├── auth.service.ts
│       ├── auth.repository.ts
│       ├── auth.model.ts
│       ├── auth.routes.ts
│       └── auth.validator.ts
│
├── shared/                    # Componentes compartidos
│   ├── circuitBreaker.ts      # PATRÓN CIRCUIT BREAKER
│   ├── pagination.ts          # Utilidades de paginación
│   └── responses.ts           # Formato estándar de respuestas
│
├── app.ts                     # Configuración de Express + middlewares
└── server.ts                  # Punto de entrada
```

### 4.3 Flujo de una Petición (Ejemplo)

**Escenario**: Un arquitecto consulta las métricas del mantenimiento "MANT-001"

```
1. GET /api/v1/metrics/maintenances/MANT-001
   Header: Authorization: Bearer <jwt_token>

2. Rate Limiter: ✅ Arquitecto tiene 98/100 peticiones disponibles

3. CORS: ✅ Origen permitido

4. Logger: 📝 Registra: GET /api/v1/metrics/maintenances/MANT-001 | IP: 10.0.1.50 | 2026-10-15T10:30:00Z

5. JWT Validator: ✅ Token válido, usuario: juan.perez@bancoecuador.com, rol: architect

6. RBAC: ✅ Rol "architect" tiene permiso para GET /metrics/*

7. MetricsController.getByMaintenance("MANT-001")

8. MetricsService.findByMaintenanceId("MANT-001")

9. MetricsRepository.findByMaintenanceId("MANT-001")
   → SELECT * FROM code_metrics WHERE maintenance_id = 'MANT-001' ORDER BY measured_at DESC

10. Respuesta 200 OK:
    {
      "status": "success",
      "data": {
        "maintenanceId": "MANT-001",
        "metrics": [
          {
            "id": "met-001",
            "coveragePercentage": 85.5,
            "cyclomaticComplexity": 12,
            "codeSmells": 3,
            "vulnerabilities": 0,
            "duplicatedLinesPercentage": 2.1,
            "measuredAt": "2026-10-14T16:00:00Z"
          }
        ]
      },
      "pagination": {
        "page": 1,
        "limit": 10,
        "total": 1
      }
    }
```

---

## 5. STACK TECNOLÓGICO

### 5.1 Tecnologías Seleccionadas

| Capa | Tecnología | Versión | Justificación |
|------|-----------|---------|---------------|
| **Runtime** | Node.js | 20 LTS | Estable, soporte a largo plazo, non-blocking I/O ideal para APIs |
| **Framework** | Express.js | 4.x | Ligero, maduro, enorme ecosistema de middlewares |
| **Lenguaje** | TypeScript | 5.x | Tipado estático reduce errores, mejora mantenibilidad |
| **ORM** | Prisma | 5.x | Type-safe, migraciones automáticas, excelente DX |
| **Base de datos** | PostgreSQL | 16 | Robusta, ACID, estándar en banca, soporte JSON nativo |
| **Autenticación** | jsonwebtoken | 9.x | Estándar JWT, ampliamente usado |
| **Validación** | Zod | 3.x | Validación type-safe, integra con TypeScript |
| **Documentación** | swagger-jsdoc + swagger-ui-express | - | Genera OpenAPI desde código |
| **Logging** | Winston | 3.x | Logging estructurado, múltiples transports |
| **Testing** | Jest + Supertest | - | Framework de testing estándar + testing de endpoints HTTP |
| **Contenedor** | Docker + docker-compose | - | Despliegue consistente, aislamiento de ambiente |
| **Circuit Breaker** | opossum | 8.x | Librería de circuit breaker para Node.js, ligera y probada |

### 5.2 Justificación de Node.js + Express sobre otras opciones

| Criterio | Node.js + Express | Java + Spring Boot | Python + FastAPI |
|----------|-------------------|-------------------|------------------|
| Velocidad de desarrollo | ✅ Rápido | ❌ Verboso | ✅ Rápido |
| Rendimiento para I/O | ✅ Excelente (event loop) | ✅ Bueno (threads) | ⚠️ Bueno (async) |
| Ecosistema de middlewares | ✅ Enorme | ✅ Grande | ⚠️ Medio |
| Tipado estático | ✅ Con TypeScript | ✅ Nativo | ⚠️ Con type hints |
| Curva de aprendizaje | ✅ Baja | ❌ Alta | ✅ Baja |
| Containerización | ✅ Imagen ligera (~150MB) | ❌ Pesado (~400MB) | ✅ Ligero (~200MB) |

---

## 6. DIAGRAMAS DE REFERENCIA

Los diagramas de arquitectura se encuentran en formato draw.io para su edición y revisión:

| Diagrama | Archivo | Descripción |
|----------|---------|-------------|
| Arquitectura General por Capas | `Fase2-Arquitectura-General.drawio` | Vista completa de la arquitectura con capas, componentes y patrones aplicados |
| Patrones de Diseño | `Fase2-Patrones-Diseno.drawio` | Detalle del flujo del API Gateway y estados del Circuit Breaker |

Para abrir los diagramas: Importar los archivos `.drawio` en [app.diagrams.net](https://app.diagrams.net) o en la extensión Draw.io de VS Code.

---

## ANEXO: Resumen de Decisiones Arquitectónicas

| Decisión | Opción Elegida | Alternativas Evaluadas | Razón |
|----------|---------------|----------------------|-------|
| Estilo arquitectónico | RESTful Monolítico Modular | Microservicios, gRPC, Event-Driven, Serverless | Equipo pequeño, volumen bajo, experiencia existente |
| Patrón 1 | API Gateway | - | Obligatorio: centraliza seguridad, logging, rate limiting |
| Patrón 2 | Circuit Breaker | CQRS, BFF, Service Registry | Resiliencia ante fallos externos, crítico en banca |
| Lenguaje | TypeScript | Java, Python | Tipado + velocidad de desarrollo + equipo familiarizado |
| Base de datos | PostgreSQL | MongoDB, MySQL | ACID compliance, estándar bancario, soporte JSON |
| Contenedor | Docker | VM, Bare metal | Portabilidad, consistencia, despliegue rápido |

---

*Fin del Documento de Arquitectura - Fase 2*  
*MSApiMantenimientosEvolutivos - Banco Ecuador*
