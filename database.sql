-- ============================================================
-- Bank Challenge — Database script (PostgreSQL)
-- Deliverable: SQL view of the model (equivalent to EF Core's)
-- ============================================================
-- Topology: one PostgreSQL instance, TWO databases (database-per-service)
--   bank_clients  -> ClientService  (Personas, Clientes)
--   bank_accounts -> AccountService (ClientesLectura, Cuentas, Movimientos)
--
-- Design rules applied:
--   * Soft delete: nothing is ever deleted physically; Cliente/Cuenta are
--     deactivated with Estado = false (logical DELETE).
--   * Movimientos: immutable ledger (F2), no DELETE. Cuentas: CRU (F1).
--   * IDs: Personas.Id / Clientes.Id / Movimientos.Id = identity (int);
--     NumeroCuenta = natural business key (NOT identity).
--     ClienteId is the conceptual/API name of the Cliente identity; in TPT
--     (EF Core mapped column-for-column) it is the shared Id column.
--   * ClientesLectura is the read model OWNED by AccountService, fed by
--     RabbitMQ events from ClientService. Cuentas has an FK to it: you
--     cannot create an account for a client the service does not know.
--   * Identificacion is UNIQUE: business key + enumeration mitigation.
--
-- Execution:
--   a) At Docker runtime, EF Core migrations (Database.Migrate()) create
--      the databases and schema when each service boots.
--   b) This script is for review / manual bootstrap on an empty instance,
--      run as superuser (e.g.: psql -U admin -f database.sql).
--      Do NOT mix (a) and (b) on the same instance.

-- ============================================================
-- DATABASE 1: bank_clients — ClientService (Persona / Cliente)
-- ============================================================
CREATE DATABASE bank_clients;

\connect bank_clients

-- Base entity (TPT: table per type)
CREATE TABLE Personas (
    Id             INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Nombre         VARCHAR(150) NOT NULL,
    Genero         VARCHAR(20)  NOT NULL,
    Edad           INT          NOT NULL,
    Identificacion VARCHAR(50)  NOT NULL UNIQUE,
    Direccion      VARCHAR(255) NOT NULL DEFAULT '',
    Telefono       VARCHAR(20)  NOT NULL,
    CONSTRAINT CK_Personas_Age CHECK (Edad BETWEEN 1 AND 130)
);

-- Derived entity: PK = FK to Personas.Id (TPT). ClienteId is the
-- conceptual name of this identity; EF maps it to the shared Id column.
CREATE TABLE Clientes (
    Id         INT PRIMARY KEY,
    Contrasena VARCHAR(255) NOT NULL,
    Estado     BOOLEAN NOT NULL,
    CONSTRAINT FK_Clientes_Personas FOREIGN KEY (Id) REFERENCES Personas(Id)
);

-- Reference seed (use case 1 of the statement).
-- The Postman demo creates the same data through the API; do not duplicate.
INSERT INTO Personas (Nombre, Genero, Edad, Identificacion, Direccion, Telefono) VALUES
('Jose Lema',          'Masculino', 35, '1723456789', 'Otavalo sn y principal', '098254785'),
('Marianela Montalvo', 'Femenino',  32, '1734567890', 'Amazonas y NNUU',        '097548965'),
('Juan Osorio',        'Masculino', 40, '1745678901', '13 junio y Equinoccial', '098874587');

INSERT INTO Clientes (Id, Contrasena, Estado)
SELECT p.Id, v.Contrasena, v.Estado
FROM (VALUES
    ('1723456789', '1234', TRUE),
    ('1734567890', '5678', TRUE),
    ('1745678901', '1245', TRUE)
) AS v(Identificacion, Contrasena, Estado)
JOIN Personas p ON p.Identificacion = v.Identificacion;

-- ============================================================
-- DATABASE 2: bank_accounts — AccountService (Cuenta / Movimiento)
-- ============================================================
CREATE DATABASE bank_accounts;

\connect bank_accounts

-- Client read model (OWNED by AccountService).
-- Kept in sync by consuming ClientService events via RabbitMQ.
CREATE TABLE ClientesLectura (
    ClienteId INT PRIMARY KEY,
    Nombre    VARCHAR(150) NOT NULL
);

-- Natural PK: the account number is defined by the bank, not a sequence
CREATE TABLE Cuentas (
    NumeroCuenta INT PRIMARY KEY,
    TipoCuenta   VARCHAR(50)  NOT NULL,
    SaldoInicial NUMERIC(18,2) NOT NULL,
    Estado       BOOLEAN NOT NULL,
    ClienteId    INT NOT NULL,
    CONSTRAINT FK_Cuentas_ClientesLectura FOREIGN KEY (ClienteId)
        REFERENCES ClientesLectura(ClienteId)
);

-- Immutable transactions ledger (F2)
CREATE TABLE Movimientos (
    Id             INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Fecha          TIMESTAMP NOT NULL,
    TipoMovimiento VARCHAR(50)   NOT NULL,
    Valor          NUMERIC(18,2) NOT NULL,
    Saldo          NUMERIC(18,2) NOT NULL,
    NumeroCuenta   INT NOT NULL,
    CONSTRAINT FK_Movimientos_Cuentas FOREIGN KEY (NumeroCuenta) REFERENCES Cuentas(NumeroCuenta)
);

-- Reference seed. ClienteId 1..3 = bank_clients seed order.
INSERT INTO ClientesLectura (ClienteId, Nombre) VALUES
(1, 'Jose Lema'),
(2, 'Marianela Montalvo'),
(3, 'Juan Osorio');

-- Use cases 2 and 3 of the statement
INSERT INTO Cuentas (NumeroCuenta, TipoCuenta, SaldoInicial, Estado, ClienteId) VALUES
(478758, 'Ahorros',    2000.00, TRUE, 1),
(225487, 'Corriente',   100.00, TRUE, 2),
(495878, 'Ahorros',       0.00, TRUE, 3),
(496825, 'Ahorros',     540.00, TRUE, 2),
(585545, 'Corriente',  1000.00, TRUE, 1); -- case 3: new account for Jose Lema

-- Use case 4. CURRENT_DATE: the F4 report always has data for today.
-- Deposito/Retiro kept without accents in data for consistency.
INSERT INTO Movimientos (Fecha, TipoMovimiento, Valor, Saldo, NumeroCuenta) VALUES
(CURRENT_DATE, 'Retiro',    -575.00, 1425.00, 478758), -- 2000 - 575
(CURRENT_DATE, 'Deposito',   600.00,  700.00, 225487), --  100 + 600
(CURRENT_DATE, 'Deposito',   150.00,  150.00, 495878), --    0 + 150
(CURRENT_DATE, 'Retiro',    -540.00,    0.00, 496825); --  540 - 540