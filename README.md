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

## ⚙️ Despliegue

La solución se levanta de forma automatizada mediante Docker Compose, el cual compila los microservicios usando Dockerfiles multi-etapa y levanta las dependencias de infraestructura.

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd backend-bp
```

### 2. Copiar los archivos de entorno

Las credenciales y cadenas de conexión **no se versionan**. Copia los archivos de ejemplo (que sí están en el repo) — los valores de ejemplo funcionan tal cual para correr localmente:

```bash
cp .env.example .env
cp src/Services/ClientService/.env.example src/Services/ClientService/.env
cp src/Services/AccountService/.env.example src/Services/AccountService/.env
```

- El `.env` raíz alimenta la infraestructura (Postgres y RabbitMQ) por interpolación en `docker-compose.yaml`.
- Cada API usa su propio `.env` (`env_file:` en compose) con su cadena de conexión y host del broker; cada servicio queda autocontenido.

### 3. Levantar la solución

```bash
docker compose up --build
```

### 4. Verificar que todo quedó arriba

Con los contenedores corriendo, confirma que ambas APIs respondan:

- Swagger de Clientes: http://localhost:5001/swagger
- Swagger de Cuentas y Movimientos: http://localhost:5002/swagger
- RabbitMQ Management: http://localhost:15672 (usuario/contraseña del `.env` raíz, por defecto `guest`/`guest`)

Si las tres cargan, la solución quedó arriba correctamente.

---

## 🧪 Pruebas

Las pruebas de integración (`tests/AccountService.IntegrationTests`) levantan un **PostgreSQL real y desechable con Testcontainers** contra la API en memoria (`WebApplicationFactory`) — ejercitan F2/F3 de punta a punta (HTTP + base real, sin mocks).

**Requieren Docker corriendo en la máquina** (Testcontainers necesita el daemon disponible para crear y destruir el contenedor). Las pruebas unitarias (`tests/ClientService.Domain.Tests`) no tienen dependencias externas.

```bash
dotnet test backend-bp.sln
```

---

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

## 📬 Colección Postman

Importa `postman/BackendChallenge.postman_collection.json` en Postman. Incluye los 3 clientes, las 4 cuentas y los 4 movimientos de los casos de uso del enunciado, más un request que fuerza el caso de "Saldo no disponible" (F3). Los requests de creación guardan el id devuelto en variables de colección, así que se puede correr con "Run Collection" de arriba a abajo sin editar nada a mano.

---

## 🗄️ Base de Datos

La base de datos se **genera automáticamente** con EF Core Migrations al iniciar la aplicación. No es necesario ejecutar scripts SQL adjuntos.

- **Con Docker** (`docker compose up --build`): PostgreSQL se levanta y se crean automáticamente las bases `bank_clients` (desde `POSTGRES_DB` del `.env` raíz) y `bank_accounts` (al arrancar el `AccountService`, mediante `EnsureCreated`/`Migrate`). El schema inicial viene definido por las migraciones incluidas en cada proyecto.
- **Sin Docker** (desarrollo local): establecer la variable `ConnectionStrings__DefaultConnection` en el archivo `.env` del servicio correspondiente o mediante `dotnet user-secrets`:
  ```bash
  cd src/Services/ClientService/ClientService.Api
  dotnet user-secrets init
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=bank_clients;Username=admin;Password=changeme"
  dotnet user-secrets set "RabbitMQ:Host" "localhost"
  ```
  (repite para `AccountService.Api` con `Database=bank_accounts`).

  Las migraciones se aplican con:
  ```bash
  dotnet ef migrations add <Nombre> --project src/Services/ClientService/ClientService.Infrastructure
  dotnet ef database update --project src/Services/ClientService/ClientService.Infrastructure
  ```
  (idéntico para `AccountService` usando su respectivo proyecto).

- Para aplicar nuevas migraciones o ver el script generado:
  ```bash
  dotnet ef migrations add <Nombre de migración>
  dotnet ef migrations script
  ```

> `BaseDatos.sql` se incluye como entregable con el script de schema de referencia (tablas, PKs, FKs) para revisión rápida sin tener que levantar el proyecto. La fuente de verdad real son las migraciones de EF Core, que son las que efectivamente crean la base al arrancar los servicios — no hace falta ejecutar el `.sql` a mano para nada.
