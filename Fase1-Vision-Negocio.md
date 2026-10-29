# FASE 1: VISIÓN DE NEGOCIO Y JUSTIFICACIÓN
## MSApiMantenimientosEvolutivos - API de Gestión de Mantenimientos Evolutivos y Métricas de Calidad

---

**Documento de Visión del Producto**  
**Versión:** 1.0  
**Fecha:** Octubre 2026  
**Autor:** Arquitecto de Soluciones - Célula de Mantenimientos Evolutivos  
**Institución:** Banco Ecuador - Área de Tecnología  
**Nombre de la API:** MSApiMantenimientosEvolutivos

---

## TABLA DE CONTENIDOS

1. [Resumen Ejecutivo](#1-resumen-ejecutivo)
2. [Contexto y Problemática Actual](#2-contexto-y-problemática-actual)
3. [Justificación de Negocio](#3-justificación-de-negocio)
4. [Anatomía y Naturaleza de la API](#4-anatomía-y-naturaleza-de-la-api)
5. [Cadena de Valor](#5-cadena-de-valor)
6. [Modelo de Monetización](#6-modelo-de-monetización)
7. [Objetivos Estratégicos](#7-objetivos-estratégicos)
8. [Métricas de Éxito](#8-métricas-de-éxito)
9. [Stakeholders](#9-stakeholders)
10. [Riesgos y Mitigación](#10-riesgos-y-mitigación)
11. [Conclusiones y Próximos Pasos](#11-conclusiones-y-próximos-pasos)

---

## 1. RESUMEN EJECUTIVO

### 1.1 Visión del Producto

Desarrollar **MSApiMantenimientosEvolutivos**, una API RESTful empresarial que centralice y automatice la gestión del ciclo de vida de mantenimientos evolutivos (proyectos menores a 300 horas) en las aplicaciones de Banco Ecuador. La API integra la captura y análisis de métricas de calidad de código, reduciendo riesgos operacionales y optimizando la eficiencia del equipo de desarrollo.

### 1.2 Problema Central

La célula de mantenimientos evolutivos de Banco Ecuador gestiona **4 proyectos mensuales** que impactan aplicaciones críticas del banco (core banking, canales digitales, CRM). Actualmente, el proceso de revisión de código y consolidación de métricas de calidad es **manual, disperso y propenso a errores**, lo que genera:

- Sobrecarga operacional: 4-6 horas por cada revisión de código
- Métricas almacenadas en hojas de cálculo sin trazabilidad
- Defectos que llegan a producción por falta de controles automatizados
- Costos de retrabajo evitables

### 1.3 Solución Propuesta

MSApiMantenimientosEvolutivos actúa como **sistema de registro único (System of Record)** para:

1. Gestionar el inventario de mantenimientos evolutivos activos y finalizados
2. Registrar y consultar métricas de calidad de código (cobertura, complejidad, vulnerabilidades)
3. Generar alertas automáticas cuando métricas críticas se degradan por debajo de umbrales definidos
4. Proveer datos estructurados para dashboards ejecutivos y reportes de auditoría

### 1.4 Valor de Negocio Cuantificado

| Métrica de Impacto | Línea Base (Actual) | Con API (Proyectado) | Mejora |
|---------------------|---------------------|----------------------|--------|
| Tiempo de revisión por mantenimiento | 4-6 horas | 1-2 horas | **-67%** |
| Consolidación de métricas mensual | 8 horas | 20 minutos | **-96%** |
| Defectos en producción por mes | 2-3 | 0-1 | **-67%** |
| Costo de retrabajo mensual | $3,500 USD | $1,000 USD | **$30,000 USD/año** |
| Tiempo de onboarding de nuevos arquitectos | 2 semanas | 3 días | **-70%** |

**ROI proyectado a 12 meses**: 312%  
**Periodo de recuperación**: 3.8 meses

---

## 2. CONTEXTO Y PROBLEMÁTICA ACTUAL

### 2.1 Situación del Área de Mantenimientos Evolutivos

La célula de mantenimientos evolutivos de Banco Ecuador es responsable de:

- Desarrollos que no superan las **300 horas** de esfuerzo
- Intervenciones en múltiples aplicaciones bancarias de acuerdo con la necesidad del cliente interno
- Cumplimiento de estándares de seguridad y calidad regulatorios del sector financiero ecuatoriano

**Composición del equipo**:

| Rol | Cantidad | Responsabilidad Principal |
|-----|----------|--------------------------|
| Desarrolladores | 8 | Codificación, pruebas unitarias, pull requests |
| Arquitectos revisores | 2 | Revisión de código, validación de métricas, aprobación técnica |

**Volumen operacional**: 4 mantenimientos evolutivos por mes (~48 por año)

### 2.2 Flujo de Trabajo Actual (As-Is)

```
1. Solicitud de mantenimiento ingresa al backlog
2. Arquitecto asigna mantenimiento a uno o más desarrolladores
3. Desarrollador trabaja en branch dedicado
4. Al finalizar: código entregado en pull request
5. ⛔ MANUAL: Arquitecto revisa código línea por línea
6. ⛔ MANUAL: Arquitecto ejecuta herramientas de análisis estático localmente
7. ⛔ MANUAL: Arquitecto consolida métricas en Excel
8. ⛔ MANUAL: Arquitecto aprueba o rechaza el pull request
9. ⛔ MANUAL: Al fin de mes, consolidar métricas de los 4 mantenimientos
10. ⛔ MANUAL: Reportar estado a gerencia en presentación PowerPoint
```

Los pasos 5 a 10 representan tareas manuales, repetitivas y sin estandarización.

### 2.3 Puntos de Dolor (Pain Points)

#### 2.3.1 Falta de Estandarización
- Los 2 arquitectos revisores manejan checklists mentales diferentes
- Criterios de aprobación inconsistentes entre uno y otro
- Un desarrollador puede ser aprobado por un arquitecto y rechazado por el otro con el mismo código

#### 2.3.2 Pérdida de Información
- Métricas históricas almacenadas en laptops personales o carpetas compartidas sin estructura
- Si un arquitecto deja la organización, se pierde el conocimiento acumulado
- No existe forma de comparar la evolución de calidad entre trimestres

#### 2.3.3 Visibilidad Limitada para Gerencia
- El gerente solo ve el estado de métricas al cierre de mes
- No puede identificar mantenimientos en riesgo de forma temprana
- Los reportes son estáticos y no permiten análisis detallado

#### 2.3.4 Carga Operacional Desproporcionada
- Con solo 2 arquitectos para 8 desarrolladores, el cuello de botella es la revisión
- Los arquitectos gastan aproximadamente el 40% de su tiempo en tareas administrativas
- Desarrolladores esperan 1-2 días por feedback de revisión

#### 2.3.5 Riesgos de Cumplimiento
- Auditorías internas del banco requieren evidencia de revisiones técnicas
- La falta de trazabilidad puede generar hallazgos críticos ante la Superintendencia de Bancos del Ecuador
- No hay alertas proactivas de violaciones de seguridad

---

## 3. JUSTIFICACIÓN DE NEGOCIO

### 3.1 Alineación con Objetivos Estratégicos de Banco Ecuador

#### 3.1.1 Transformación Digital
Banco Ecuador impulsa la digitalización de sus procesos internos. MSApiMantenimientosEvolutivos:
- Elimina procesos manuales basados en hojas de cálculo
- Reduce cuellos de botella en el delivery de software
- Permite escalar el modelo operativo sin aumentar personal

#### 3.1.2 Excelencia Operacional
MSApiMantenimientosEvolutivos:
- Estandariza umbrales de calidad (cobertura >80%, 0 vulnerabilidades críticas)
- Detecta degradación de calidad antes del despliegue
- Provee métricas objetivas que reemplazan evaluaciones subjetivas

#### 3.1.3 Gestión de Riesgos y Cumplimiento Regulatorio
La Superintendencia de Bancos del Ecuador exige trazabilidad de cambios en sistemas críticos. MSApiMantenimientosEvolutivos:
- Genera un audit trail completo de métricas por mantenimiento
- Documenta quién registró, quién revisó y cuándo se aprobó
- Facilita la preparación de reportes para entes reguladores

### 3.2 Análisis Costo-Beneficio

#### 3.2.1 Costos (Inversión Inicial)

| Concepto | Detalle | Costo (USD) |
|----------|---------|-------------|
| Desarrollo de la API | 200 horas de desarrollo interno | $10,000 |
| Infraestructura Cloud | 12 meses en ambiente contenedorizado | $1,200 |
| Testing y QA | Pruebas de carga, seguridad, unitarias | $1,500 |
| Documentación y capacitación | OpenAPI, guías de uso, sesiones de entrenamiento | $1,000 |
| **TOTAL INVERSIÓN** | | **$13,700** |

#### 3.2.2 Beneficios (Anualizados)

| Concepto | Cálculo | Ahorro (USD/año) |
|----------|---------|-------------------|
| Reducción tiempo de revisión | 48 mantenimientos/año × 3.5 horas ahorradas × $40/hora | $6,720 |
| Eliminación consolidación manual | 12 meses × 7.5 horas × $40/hora | $3,600 |
| Reducción de defectos en producción | 18 defectos evitados × $1,500/defecto | $27,000 |
| Eficiencia en onboarding | 1 arquitecto nuevo/año × 60 horas × $40/hora | $2,400 |
| Reducción tiempo de auditorías | Preparación más rápida de evidencias | $3,000 |
| **TOTAL BENEFICIOS** | | **$42,720** |

#### 3.2.3 ROI y Periodo de Recuperación

```
ROI = (Beneficios - Costos) / Costos × 100
ROI = ($42,720 - $13,700) / $13,700 × 100 = 312%

Periodo de Recuperación = Inversión / (Beneficios / 12)
Periodo = $13,700 / ($42,720 / 12) = 3.8 meses
```

**La inversión se recupera en menos de 4 meses.**

### 3.3 Análisis de Alternativas

| Alternativa | Ventajas | Desventajas | Decisión |
|-------------|----------|-------------|----------|
| **Status quo** (seguir con Excel) | Sin inversión | Overhead creciente, riesgos de cumplimiento | ❌ Insostenible |
| **Herramienta comercial** (Jira plugin, SonarQube dashboard) | Implementación rápida | Licencias costosas, no personalizable al flujo del banco | ❌ No se ajusta |
| **API custom interna (MSApiMantenimientosEvolutivos)** | Personalizable, propiedad intelectual del banco | Requiere desarrollo | ✅ **SELECCIONADA** |

---

## 4. ANATOMÍA Y NATURALEZA DE LA API

### 4.1 Clasificación de la API

#### 4.1.1 Por Tipo de Acceso

**API Privada (Internal API)**
- Audiencia: Equipos internos de desarrollo y arquitectura de Banco Ecuador
- No expuesta a internet público
- Acceso restringido a red corporativa o VPN

#### 4.1.2 Por Naturaleza de Datos

**API Operacional (Operational API)**
- Maneja datos transaccionales en tiempo real
- Operaciones CRUD sobre mantenimientos y métricas
- Naturaleza transaccional (OLTP), no analítica

#### 4.1.3 Por Función de Negocio

**API de Sistema (System API)**
- Capa de abstracción sobre la base de datos de mantenimientos y métricas
- Expone capacidades de negocio: gestión de mantenimientos, registro de métricas, alertas
- Sirve como base para futuros consumidores (dashboard web, aplicaciones móviles, integraciones)

### 4.2 Características Técnicas

#### 4.2.1 Estilo Arquitectónico

**REST (Representational State Transfer)**

Justificación de la elección:

| Criterio | REST | GraphQL | gRPC |
|----------|------|---------|------|
| Adopción en Banco Ecuador | ✅ Estándar actual | ❌ No adoptado | ❌ No adoptado |
| Documentación OpenAPI | ✅ Nativo | ⚠️ Requiere tooling extra | ❌ Usa Protocol Buffers |
| Curva de aprendizaje del equipo | ✅ Baja | ⚠️ Media | ❌ Alta |
| Interoperabilidad con herramientas | ✅ Universal | ⚠️ Limitada | ⚠️ Limitada |
| Complejidad de consultas requeridas | ✅ CRUD simple, suficiente | ⚠️ Sobredimensionado | ❌ No necesario |

**Conclusión**: REST es la opción adecuada dado que las operaciones son CRUD estándar, el equipo ya tiene experiencia con REST, y se requiere documentación OpenAPI como entregable del proyecto.

#### 4.2.2 Recursos Principales

| Recurso | Descripción | Operaciones |
|---------|-------------|-------------|
| `/maintenances` | Mantenimientos evolutivos del banco | GET, POST, PATCH, DELETE |
| `/metrics` | Métricas de calidad de código | GET, POST |
| `/users` | Gestión de usuarios y autenticación | GET, POST, PATCH |
| `/auth` | Autenticación y tokens JWT | POST |

#### 4.2.3 Modelo de Madurez REST (Richardson Maturity Model)

**Nivel 2 - HTTP Verbs**

La API utiliza correctamente verbos HTTP (GET, POST, PATCH, DELETE), códigos de estado (200, 201, 400, 401, 403, 404, 500) y recursos con URIs semánticas.

Ejemplo de respuesta:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Actualización módulo préstamos",
  "applicationName": "core-banking-loans",
  "estimatedHours": 240,
  "status": "in-review",
  "squad": "Squad Alpha",
  "assignedArchitect": "juan.perez@bancoecuador.com",
  "createdAt": "2026-10-01T10:00:00Z",
  "updatedAt": "2026-10-10T15:30:00Z"
}
```

### 4.3 Capacidades No Funcionales

#### 4.3.1 Seguridad
- **Autenticación**: JWT (JSON Web Tokens) con expiración configurable
- **Autorización**: RBAC (Role-Based Access Control)
  - `developer`: Consultar mantenimientos asignados, registrar métricas
  - `architect`: Gestión completa de mantenimientos, revisión de métricas, aprobaciones
  - `manager`: Lectura de estadísticas y reportes consolidados
- **Encriptación**: TLS 1.2+ en tránsito, contraseñas hasheadas con bcrypt

#### 4.3.2 Performance
- Latencia objetivo P95: < 300ms para endpoints de lectura
- Throughput: Soportar 100 requests/segundo en pico
- Concurrencia: 10-15 usuarios simultáneos (tamaño del equipo)

#### 4.3.3 Disponibilidad
- SLA: 99% uptime (horario laboral L-V 8am-6pm)
- Ventana de mantenimiento: Fines de semana

#### 4.3.4 Escalabilidad
- Diseño stateless que permite escalar horizontalmente
- Base de datos PostgreSQL con capacidad de crecimiento

---

## 5. CADENA DE VALOR

### 5.1 Flujo de Valor Actual vs. Propuesto

**Proceso Actual (As-Is)**:
```
Desarrollo (5 días) → Espera revisión (2 días) → Revisión manual (6h) →
Espera aprobación (1 día) → Consolidación métricas (2h) → Deploy (1 día)

Lead Time Total: 9-10 días
Tiempo de Valor Agregado: 6 días
Desperdicio: 3-4 días (40%)
```

**Proceso con MSApiMantenimientosEvolutivos (To-Be)**:
```
Desarrollo (5 días) → Registro métricas via API (15 min) →
Revisión asistida por datos (1.5h) → Aprobación (inmediata) → Deploy (1 día)

Lead Time Total: 6.5 días
Tiempo de Valor Agregado: 6 días
Desperdicio: 0.5 días (8%)
```

**Reducción de Lead Time**: 32%  
**Eliminación de desperdicio**: 80%

### 5.2 Propuesta de Valor por Stakeholder

#### Para los 8 Desarrolladores
- Feedback de revisión más rápido (de 2 días a horas)
- Criterios de aprobación claros, objetivos y consistentes
- Menos retrabajo por rechazos ambiguos

#### Para los 2 Arquitectos Revisores
- Métricas disponibles antes de iniciar la revisión (no hay que ejecutar herramientas manualmente)
- Vista consolidada de mantenimientos pendientes
- Alertas automáticas cuando métricas caen por debajo de umbrales

#### Para la Gerencia de TI
- Visibilidad en tiempo real del estado de los 4 mantenimientos mensuales
- Identificación temprana de proyectos en riesgo
- Datos listos para reportes ejecutivos en cualquier momento

#### Para Auditoría Interna
- Trazabilidad completa: qué se midió, cuándo y por quién
- Evidencia de cumplimiento de estándares de calidad
- Historial consultable para cualquier periodo

---

## 6. MODELO DE MONETIZACIÓN

### 6.1 Monetización Interna (Valor para Banco Ecuador)

Como API interna, MSApiMantenimientosEvolutivos genera valor medible a través de un modelo de **showback** (demostración de valor del área de arquitectura):

| Servicio | Unidad | Valor Interno |
|----------|--------|---------------|
| Registro y gestión de mantenimiento | Por proyecto | $50 USD |
| Registro y consulta de métricas | Por escaneo registrado | $15 USD |
| Almacenamiento histórico de métricas | Por GB-mes | $10 USD |
| Consultas de estadísticas consolidadas | Por consulta | $5 USD |

**Valor anual generado internamente**: ~$4,800 USD (cargado a los proyectos consumidores)

### 6.2 Monetización Externa (Potencial como Producto SaaS)

Si Banco Ecuador decide productizar la API para ofrecerla a otras instituciones financieras:

#### Modelo Freemium

| Plan | Límites | Precio Mensual |
|------|---------|----------------|
| **Free** | Hasta 3 mantenimientos/mes, 1 usuario | $0 |
| **Professional** | Hasta 20 mantenimientos/mes, 10 usuarios | $299 USD |
| **Enterprise** | Ilimitado + soporte dedicado + SLA garantizado | $1,499 USD |

#### Proyección de Ingresos (Año 1)

Mercado objetivo: Bancos y cooperativas medianas en Ecuador y Latinoamérica

```
Pesimista (3 clientes):   3 × $299 × 12  = $10,764/año
Base (8 clientes):        8 × $600 × 12  = $57,600/año
Optimista (15 clientes): 15 × $1,000 × 12 = $180,000/año
```

### 6.3 Nuevos Flujos de Ingresos

La API abre la puerta a servicios complementarios monetizables:

1. **Consultoría en calidad de código**: Basada en los datos recopilados por la API
2. **Benchmarking sectorial**: Comparar métricas entre instituciones financieras (datos anonimizados)
3. **Certificación de calidad**: Sello "Calidad verificada por MSApiMantenimientosEvolutivos"

---

## 7. OBJETIVOS ESTRATÉGICOS

### 7.1 Objetivos de Negocio

| ID | Objetivo | Métrica | Meta (6 meses) |
|----|----------|---------|----------------|
| BG-01 | Reducir tiempo de revisión de código | Horas por revisión | < 2 horas |
| BG-02 | Disminuir defectos en producción | Defectos por mes | < 1 |
| BG-03 | Acelerar consolidación de métricas | Horas por mes | < 30 minutos |
| BG-04 | Mejorar satisfacción de desarrolladores | Encuesta interna | > 80% satisfacción |
| BG-05 | Reducir costo operacional mensual | USD por mes | -$2,000 USD |

### 7.2 Objetivos Técnicos

| ID | Objetivo | Métrica | Meta |
|----|----------|---------|------|
| TG-01 | Disponibilidad del servicio | Uptime | > 99% |
| TG-02 | Performance de lectura | Latencia P95 | < 300ms |
| TG-03 | Cobertura de pruebas | Test coverage | > 80% |
| TG-04 | Tasa de error de la API | Error rate | < 1% |
| TG-05 | Seguridad | Vulnerabilidades críticas | 0 |

### 7.3 Plan de Adopción

| Fase | Descripción | Timeline | KPI |
|------|-------------|----------|-----|
| **Piloto** | 1 arquitecto + 4 desarrolladores | Semana 1-2 | 2 mantenimientos registrados |
| **Adopción completa** | 2 arquitectos + 8 desarrolladores | Semana 3-4 | 4 mantenimientos registrados |
| **Operación estable** | Uso estándar en toda la célula | Mes 2-3 | 100% mantenimientos gestionados via API |

---

## 8. MÉTRICAS DE ÉXITO

### 8.1 KPIs de Impacto Operacional

| KPI | Fórmula | Línea Base | Meta Q4 2026 |
|-----|---------|------------|--------------|
| Cycle Time | Tiempo desde inicio de desarrollo hasta deploy | 9.5 días | 6.5 días |
| Review Efficiency | Mantenimientos revisados por arquitecto por día | 1 | 2 |
| Automated Metrics % | Métricas registradas via API / total métricas | 0% | 100% |

### 8.2 KPIs de Calidad de Software

| KPI | Fórmula | Línea Base | Meta Q4 2026 |
|-----|---------|------------|--------------|
| Code Coverage promedio | % código con tests | 60% | 85% |
| Defect Density | Defectos por cada 1000 líneas de código | 2.0 | 0.5 |
| Technical Debt Ratio | Deuda técnica / total desarrollo | 12% | 6% |

### 8.3 KPIs de Uso de la API

| KPI | Valor Esperado |
|-----|----------------|
| Total API calls por día | > 50 |
| Usuarios activos mensuales | 10 (toda la célula) |
| Mantenimientos creados por mes | 4 |
| Métricas registradas por mes | > 16 (4 por mantenimiento mínimo) |

---

## 9. STAKEHOLDERS

### 9.1 Mapa de Stakeholders

| Stakeholder | Rol | Interés | Influencia | Estrategia |
|-------------|-----|---------|------------|------------|
| Gerente de TI | Sponsor del proyecto | Alto | Alta | Mantener informado con reportes ejecutivos |
| Arquitectos (2) | Usuarios primarios y revisores | Alto | Alta | Colaborar activamente en el diseño |
| Desarrolladores (8) | Usuarios operativos | Medio | Media | Informar y capacitar regularmente |
| Auditoría Interna | Consumidor de evidencias | Alto | Alta | Garantizar trazabilidad completa |
| Superintendencia de Bancos | Regulador externo | Alto | Muy Alta | Cumplir normativa vigente |

### 9.2 Matriz RACI

| Actividad | Arquitecto Líder | Desarrolladores | Gerente TI | Auditoría |
|-----------|-------------------|-----------------|------------|-----------|
| Definir requisitos de la API | **A** | C | R | I |
| Desarrollo e implementación | C | **R/A** | I | I |
| Revisión de seguridad | **A** | I | I | C |
| Aprobación de despliegue | **A** | I | R | C |
| Monitoreo post-despliegue | **R/A** | C | I | I |

**R**: Responsable | **A**: Accountable | **C**: Consultado | **I**: Informado

---

## 10. RIESGOS Y MITIGACIÓN

### 10.1 Riesgos de Negocio

| ID | Riesgo | Probabilidad | Impacto | Mitigación |
|----|--------|--------------|---------|------------|
| R-01 | Baja adopción por resistencia al cambio | Media | Alto | Involucrar al equipo desde el diseño, capacitación práctica |
| R-02 | Prioridad desplazada por otros proyectos | Media | Medio | Apoyo del sponsor (Gerente TI), demostrar quick wins |

### 10.2 Riesgos Técnicos

| ID | Riesgo | Probabilidad | Impacto | Mitigación |
|----|--------|--------------|---------|------------|
| R-03 | Performance inadecuada bajo carga | Baja | Alto | Pruebas de carga tempranas, diseño escalable |
| R-04 | Vulnerabilidad de seguridad | Baja | Crítico | JWT con expiración, validación de inputs, pruebas de seguridad |
| R-05 | Pérdida de datos | Baja | Crítico | Backups diarios automatizados, base de datos con replicación |

### 10.3 Riesgos Operacionales

| ID | Riesgo | Probabilidad | Impacto | Mitigación |
|----|--------|--------------|---------|------------|
| R-06 | Dependencia de conocimiento en pocas personas | Media | Alto | Documentación completa, pair programming |
| R-07 | Downtime del servicio en horario laboral | Baja | Alto | Monitoreo con alertas, health checks automatizados |

---

## 11. CONCLUSIONES Y PRÓXIMOS PASOS

### 11.1 Resumen de Justificación

MSApiMantenimientosEvolutivos representa una inversión estratégica para Banco Ecuador que:

- Resuelve un problema operacional concreto y cuantificado de la célula de mantenimientos
- Se alinea con los objetivos de transformación digital y excelencia operacional del banco
- Genera un ahorro proyectado de **$30,000 USD/año** con una inversión de $13,700 USD
- Mejora la calidad del software entregado y reduce riesgos regulatorios
- Tiene potencial de convertirse en un producto SaaS comercializable a otras instituciones financieras

**Recomendación**: Aprobar la continuación hacia la Fase 2 (Arquitectura y Patrones de Diseño).

### 11.2 Próximos Entregables

| Fase | Entregable | Fecha de Entrega |
|------|------------|------------------|
| **Fase 2** | Diagrama de arquitectura y justificación de patrones | 17 octubre 2026 |
| **Fase 3** | Contrato OpenAPI (Swagger) completo | 23 octubre 2026 |
| **Fase 4** | Código fuente, pruebas, seguridad y despliegue | 29 octubre 2026 |

### 11.3 Criterios de Aceptación de Fase 1

- [x] Problema de negocio claramente articulado con datos de Banco Ecuador
- [x] Justificación financiera con ROI > 300%
- [x] Anatomía de la API definida (tipo, naturaleza, estilo arquitectónico)
- [x] Cadena de valor mapeada (As-Is vs To-Be)
- [x] Modelo de monetización presentado (interno y externo)
- [x] Stakeholders identificados con matriz RACI
- [x] Riesgos evaluados con planes de mitigación

---

## ANEXOS

### Anexo A: Glosario de Términos

| Término | Definición |
|---------|------------|
| Mantenimiento Evolutivo | Desarrollo de software menor a 300 horas que mejora o extiende funcionalidad existente |
| Code Coverage | Porcentaje de líneas de código cubiertas por pruebas automatizadas |
| Complejidad Ciclomática | Métrica que mide el número de caminos independientes en el código |
| Technical Debt | Costo implícito de refactorización futura causado por decisiones de corto plazo |
| RBAC | Role-Based Access Control, control de acceso basado en roles |
| JWT | JSON Web Token, estándar para transmitir información de autenticación de forma segura |
| SLA | Service Level Agreement, acuerdo de nivel de servicio |

### Anexo B: Referencias

1. Richardson, L. - "RESTful Web APIs" - O'Reilly Media
2. Humble, J., Farley, D. - "Continuous Delivery" - Addison-Wesley
3. Normativa de la Superintendencia de Bancos del Ecuador - Resolución SB-2024
4. OpenAPI Specification 3.0 - https://spec.openapis.org/oas/v3.0.3

### Anexo C: Historial de Versiones

| Versión | Fecha | Autor | Cambios |
|---------|-------|-------|---------|
| 1.0 | 12-Oct-2026 | Arquitecto de Soluciones | Versión inicial del documento |

---

*Fin del Documento de Visión - Fase 1*
*MSApiMantenimientosEvolutivos - Banco Ecuador*
