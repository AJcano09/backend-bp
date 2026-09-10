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
