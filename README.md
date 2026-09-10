# 🏦 Backend Challenge - Microservicios Bancarios (.NET 8)

Solución backend desarrollada bajo **Arquitectura Hexagonal (Puertos y Adaptadores)** y principios de diseño limpio, estructurada en microservicios independientes sobre **.NET 8**, containerizada con **Docker** y orquestada con **Docker Compose**.

---

## 🚀 Requisitos Previos

Antes de ejecutar la solución, asegúrate de tener instalado en tu máquina:
* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (o Docker Engine + Docker Compose V2).
* Puertos libres en tu equipo:
  * **5001** (API de Clientes)
  * **5002** (API de Cuentas y Movimientos)
  * **5433** (PostgreSQL - *mapeado externamente para evitar conflictos locales*)
  * **5672 / 15672** (RabbitMQ Broker y Dashboard)

---

## ⚙️ Instrucciones de Despliegue

La solución se levanta de forma automatizada mediante Docker Compose, el cual compila los microservicios usando Dockerfiles multi-etapa y levanta las dependencias de infraestructura.

1. Clona el repositorio y sitúate en la raíz del proyecto:
   ```bash
   git clone <url-del-repositorio>
   cd backend-bp

## 🧪 Pruebas

Las pruebas de integración (`tests/AccountService.IntegrationTests`) levantan un **PostgreSQL real y desechable con Testcontainers** contra la API en memoria (`WebApplicationFactory`) — ejercitan F2/F3 de punta a punta (HTTP + base real, sin mocks).

**Requieren Docker corriendo en la máquina** (Testcontainers necesita el daemon disponible para crear y destruir el contenedor). Las pruebas unitarias (`tests/ClientService.Domain.Tests`) no tienen dependencias externas.

```bash
dotnet test backend-bp.sln
```


## 📂 Endpoints Principales

### Clientes (`/clientes`)
- `GET /api/clientes` - Listar clientes
- `POST /api/clientes` - Crear cliente
- `PUT /api/clientes/{id}` - Actualizar cliente
- `DELETE /api/clientes/{id}` - Eliminar cliente

### Cuentas (`/cuentas`)
- `GET /api/cuentas` - Listar cuentas
- `POST /api/cuentas` - Crear cuenta
- `PUT /api/cuentas/{id}` - Actualizar cuenta

### Movimientos (`/movimientos`)
- `GET /api/movimientos` - Listar movimientos
- `POST /api/movimientos` - Registrar movimiento (Valida saldo disponible y arroja error si no hay fondos).

### Reportes (`/reportes`)
- `GET /api/reportes?clienteId=2&desde=2022-02-01&hasta=2022-02-28` - Estado de cuenta consolidado en formato JSON (una fila por movimiento, con saldo posterior). El enunciado expresa `/reportes?fecha=rango fechas&cliente=...`; aquí el rango se interpreta como dos parámetros explícitos `desde`/`hasta` (yyyy-MM-dd) por claridad y robustez de validación.

---
