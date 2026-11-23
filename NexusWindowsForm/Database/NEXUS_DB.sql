-- NEXUS_DB: ejecutar en SQL Server Management Studio si se desea crear la estructura manualmente.
IF DB_ID(N'NEXUS_DB') IS NULL
    CREATE DATABASE [NEXUS_DB];
GO

USE [NEXUS_DB];
GO

IF OBJECT_ID(N'dbo.Jugadores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Jugadores
    (
        IdJugador INT IDENTITY(1,1) PRIMARY KEY,
        NombreJugador NVARCHAR(120) NOT NULL,
        NombreNormalizado NVARCHAR(120) NOT NULL UNIQUE,
        Edad INT NOT NULL,
        FechaRegistro DATETIME2 NOT NULL,
        UltimoIngreso DATETIME2 NOT NULL,
        CantidadIngresos INT NOT NULL CONSTRAINT DF_Jugadores_Cantidad DEFAULT 0,
        Nivel INT NOT NULL CONSTRAINT DF_Jugadores_Nivel DEFAULT 1,
        Energia INT NOT NULL,
        Vida INT NOT NULL,
        Conexion INT NOT NULL,
        Realidad NVARCHAR(30) NOT NULL
    );
END;
GO

IF OBJECT_ID(N'dbo.Ingresos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Ingresos
    (
        IdIngreso BIGINT IDENTITY(1,1) PRIMARY KEY,
        IdJugador INT NOT NULL,
        FechaIngreso DATETIME2 NOT NULL,
        FechaSalida DATETIME2 NULL,
        EnergiaInicial INT NOT NULL,
        VidaFinal INT NOT NULL,
        EnergiaFinal INT NOT NULL,
        ConexionFinal INT NOT NULL,
        Realidad NVARCHAR(30) NOT NULL,
        CONSTRAINT FK_Ingresos_Jugadores
            FOREIGN KEY (IdJugador) REFERENCES dbo.Jugadores(IdJugador)
    );
    CREATE INDEX IX_Ingresos_IdJugador_Fecha
        ON dbo.Ingresos(IdJugador, FechaIngreso DESC);
END;
GO
