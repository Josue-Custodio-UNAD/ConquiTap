-- ============================================================
-- ConquiTap - Script de Base de Datos
-- Asociación Dominicana del Nordeste (ADONE)
-- Sistema de Gestión de Clubes Adventistas
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ConquiTap')
BEGIN
    CREATE DATABASE ConquiTap;
END
GO

USE ConquiTap;
GO

-- ============================================================
-- TABLAS DE CATÁLOGOS
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Asociaciones')
CREATE TABLE Asociaciones (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255),
    Activo      BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Zonas')
CREATE TABLE Zonas (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       NVARCHAR(100) NOT NULL,
    AsociacionId INT NOT NULL,
    Activo       BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Zonas_Asociacion FOREIGN KEY (AsociacionId) REFERENCES Asociaciones(Id)
);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Distritos')
CREATE TABLE Distritos (
    Id     INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    ZonaId INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Distritos_Zona FOREIGN KEY (ZonaId) REFERENCES Zonas(Id)
);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Iglesias')
CREATE TABLE Iglesias (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    Nombre     NVARCHAR(100) NOT NULL,
    DistritoId INT NOT NULL,
    Activo     BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Iglesias_Distrito FOREIGN KEY (DistritoId) REFERENCES Distritos(Id)
);

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Especialidades')
CREATE TABLE Especialidades (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL,
    Categoria   NVARCHAR(50),
    Descripcion NVARCHAR(255),
    Activo      BIT NOT NULL DEFAULT 1
);

-- ============================================================
-- TABLA USUARIOS
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuarios')
CREATE TABLE Usuarios (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario  NVARCHAR(50) NOT NULL,
    ContrasenaHash NVARCHAR(256) NOT NULL,
    Correo         NVARCHAR(100),
    -- Categoria: Miembro, Directivo, Administrador
    Categoria      NVARCHAR(20) NOT NULL DEFAULT 'Miembro',
    Activo         BIT NOT NULL DEFAULT 1,
    FechaCreacion  DATETIME NOT NULL DEFAULT GETDATE(),
    UltimoAcceso   DATETIME,
    CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario)
);

-- ============================================================
-- TABLA CLUBES
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Clubes')
CREATE TABLE Clubes (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    IglesiaId     INT,
    -- TipoClub: Aventureros, Conquistadores, Guias
    TipoClub      NVARCHAR(20) NOT NULL,
    AnoFundacion  INT,
    Activo        BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Clubes_Iglesia FOREIGN KEY (IglesiaId) REFERENCES Iglesias(Id)
);

-- ============================================================
-- TABLA MIEMBROS
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Miembros')
CREATE TABLE Miembros (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    Nombres          NVARCHAR(100) NOT NULL,
    Apellidos        NVARCHAR(100) NOT NULL,
    Cedula           NVARCHAR(20),
    Telefono         NVARCHAR(20),
    Correo           NVARCHAR(100),
    UsuarioId        INT,
    AsociacionId     INT,
    ZonaId           INT,
    DistritoId       INT,
    IglesiaId        INT,
    -- Categoria: Miembro, Directivo
    Categoria        NVARCHAR(20) NOT NULL DEFAULT 'Miembro',
    Puesto           NVARCHAR(50),
    Clase            NVARCHAR(50),
    FechaInvestidura DATE,
    ClubId           INT,
    -- Sexo: M, F
    Sexo             CHAR(1),
    FechaNacimiento  DATE,
    FechaBautismo    DATE,
    Activo           BIT NOT NULL DEFAULT 1,
    FechaRegistro    DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Miembros_Usuario     FOREIGN KEY (UsuarioId)    REFERENCES Usuarios(Id),
    CONSTRAINT FK_Miembros_Asociacion  FOREIGN KEY (AsociacionId) REFERENCES Asociaciones(Id),
    CONSTRAINT FK_Miembros_Zona        FOREIGN KEY (ZonaId)       REFERENCES Zonas(Id),
    CONSTRAINT FK_Miembros_Distrito    FOREIGN KEY (DistritoId)   REFERENCES Distritos(Id),
    CONSTRAINT FK_Miembros_Iglesia     FOREIGN KEY (IglesiaId)    REFERENCES Iglesias(Id),
    CONSTRAINT FK_Miembros_Club        FOREIGN KEY (ClubId)       REFERENCES Clubes(Id)
);

-- Director del Club (referencia a Miembros, agregada después)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Clubes' AND COLUMN_NAME = 'DirectorId')
BEGIN
    ALTER TABLE Clubes ADD DirectorId INT;
    ALTER TABLE Clubes ADD CONSTRAINT FK_Clubes_Director FOREIGN KEY (DirectorId) REFERENCES Miembros(Id);
END

-- ============================================================
-- TABLA MIEMBRO-ESPECIALIDADES (relación muchos a muchos)
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MiembroEspecialidades')
CREATE TABLE MiembroEspecialidades (
    MiembroId      INT NOT NULL,
    EspecialidadId INT NOT NULL,
    FechaObtencion DATE,
    PRIMARY KEY (MiembroId, EspecialidadId),
    CONSTRAINT FK_ME_Miembro      FOREIGN KEY (MiembroId)      REFERENCES Miembros(Id),
    CONSTRAINT FK_ME_Especialidad FOREIGN KEY (EspecialidadId) REFERENCES Especialidades(Id)
);

-- ============================================================
-- TABLA LOG DE ACTIVIDAD
-- ============================================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LogActividad')
CREATE TABLE LogActividad (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId   INT,
    Accion      NVARCHAR(255) NOT NULL,
    Tabla       NVARCHAR(50),
    RegistroId  INT,
    Fecha       DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Log_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

-- ============================================================
-- DATOS INICIALES (SEED)
-- ============================================================

-- Asociación ADONE
IF NOT EXISTS (SELECT * FROM Asociaciones WHERE Nombre = 'ADONE')
    INSERT INTO Asociaciones (Nombre, Descripcion)
    VALUES ('ADONE', 'Asociación Dominicana del Nordeste');

-- Especialidades base
IF NOT EXISTS (SELECT * FROM Especialidades)
BEGIN
    INSERT INTO Especialidades (Nombre, Categoria) VALUES
    ('Cocina',                  'Artes del Hogar'),
    ('Repostería',              'Artes del Hogar'),
    ('Costura',                 'Artes del Hogar'),
    ('Fotografía',              'Artes Creativas'),
    ('Dibujo y Pintura',        'Artes Creativas'),
    ('Música',                  'Artes Creativas'),
    ('Primeros Auxilios',       'Salud'),
    ('Nutrición',               'Salud'),
    ('Plantas Medicinales',     'Salud'),
    ('Astronomía',              'Ciencias'),
    ('Naturaleza',              'Ciencias'),
    ('Biología Marina',         'Ciencias'),
    ('Acampamento',             'Vida al Aire Libre'),
    ('Orientación',             'Vida al Aire Libre'),
    ('Pioneirismo',             'Vida al Aire Libre'),
    ('Primeros Pasos con Dios', 'Honor'),
    ('Compañero',               'Honor'),
    ('Explorador',              'Honor');
END

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

GO
CREATE OR ALTER PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Miembros WHERE Activo = 1) AS TotalMiembros,
        (SELECT COUNT(*) FROM Clubes   WHERE Activo = 1) AS TotalClubes,
        (SELECT COUNT(*) FROM Usuarios WHERE Activo = 1) AS TotalUsuarios,
        (SELECT COUNT(*) FROM Miembros WHERE Activo = 1 AND Categoria = 'Directivo') AS TotalDirectivos,
        (SELECT COUNT(*) FROM Clubes WHERE Activo = 1 AND TipoClub = 'Conquistadores') AS ClubesConquistadores,
        (SELECT COUNT(*) FROM Clubes WHERE Activo = 1 AND TipoClub = 'Aventureros')   AS ClubesAventureros,
        (SELECT COUNT(*) FROM Clubes WHERE Activo = 1 AND TipoClub = 'Guias')         AS ClubesGuias;
END
GO

CREATE OR ALTER PROCEDURE sp_GetMiembrosRecientes
    @Top INT = 5
AS
BEGIN
    SELECT TOP (@Top)
        m.Id,
        m.Nombres + ' ' + m.Apellidos AS NombreCompleto,
        m.Categoria,
        m.Clase,
        c.Nombre AS Club,
        m.FechaRegistro
    FROM Miembros m
    LEFT JOIN Clubes c ON m.ClubId = c.Id
    WHERE m.Activo = 1
    ORDER BY m.FechaRegistro DESC;
END
GO
