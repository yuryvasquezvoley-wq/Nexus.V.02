IF DB_ID(N'NexusGame') IS NULL
    CREATE DATABASE NexusGame;
GO

USE NexusGame;
GO

IF OBJECT_ID(N'dbo.Partida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partida
    (
        SlotId             int             NOT NULL CONSTRAINT PK_Partida PRIMARY KEY,
        NombreCadete       nvarchar(50)    NOT NULL,
        Edad               int             NOT NULL,
        Realidad           nvarchar(20)    NOT NULL,
        PosicionX          float           NOT NULL,
        PosicionY          float           NOT NULL,
        Vida               int             NOT NULL,
        Energia            int             NOT NULL,
        Conexion           int             NOT NULL,
        CelulasMedicas     int             NOT NULL,
        Resistencia        float           NOT NULL,
        NaveReparada       bit             NOT NULL,
        RecuperacionUsada  bit             NOT NULL,
        FechaGuardado      datetime2(0)    NOT NULL,
        CONSTRAINT CK_Partida_Slot CHECK (SlotId = 1),
        CONSTRAINT CK_Partida_Realidad CHECK (Realidad IN (N'REALIDAD 102', N'REALIDAD 999')),
        CONSTRAINT CK_Partida_Recursos CHECK
            (Vida BETWEEN 0 AND 100 AND Energia BETWEEN 0 AND 100 AND Conexion BETWEEN 0 AND 100
             AND Resistencia BETWEEN 0 AND 100 AND CelulasMedicas >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.NucleoPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NucleoPartida
    (
        SlotId   int           NOT NULL,
        NucleoId nvarchar(20)  NOT NULL,
        CONSTRAINT PK_NucleoPartida PRIMARY KEY (SlotId, NucleoId),
        CONSTRAINT FK_NucleoPartida_Partida FOREIGN KEY (SlotId) REFERENCES dbo.Partida(SlotId) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.EnemigoPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EnemigoPartida
    (
        SlotId       int            NOT NULL,
        EnemigoId    nvarchar(80)   NOT NULL,
        Realidad     nvarchar(20)   NOT NULL,
        Nombre       nvarchar(80)   NOT NULL,
        Tipo         nvarchar(60)   NOT NULL,
        PosicionX    float          NOT NULL,
        PosicionY    float          NOT NULL,
        DestinoX     float          NOT NULL,
        DestinoY     float          NOT NULL,
        Vida         int            NOT NULL,
        VidaMaxima   int            NOT NULL,
        Velocidad    real           NOT NULL,
        EsJefe       bit            NOT NULL,
        EsZombie     bit            NOT NULL,
        Derrotada    bit            NOT NULL,
        CONSTRAINT PK_EnemigoPartida PRIMARY KEY (SlotId, EnemigoId),
        CONSTRAINT FK_EnemigoPartida_Partida FOREIGN KEY (SlotId) REFERENCES dbo.Partida(SlotId) ON DELETE CASCADE,
        CONSTRAINT CK_EnemigoPartida_Realidad CHECK (Realidad IN (N'REALIDAD 102', N'REALIDAD 999')),
        CONSTRAINT CK_EnemigoPartida_Vida CHECK (Vida BETWEEN 0 AND VidaMaxima AND VidaMaxima > 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.ObjetoPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ObjetoPartida
    (
        SlotId     int           NOT NULL,
        Realidad   nvarchar(20)  NOT NULL,
        Tipo       nvarchar(16)  NOT NULL,
        Indice     int           NOT NULL,
        PosicionX  float         NOT NULL,
        PosicionY  float         NOT NULL,
        CONSTRAINT PK_ObjetoPartida PRIMARY KEY (SlotId, Realidad, Tipo, Indice),
        CONSTRAINT FK_ObjetoPartida_Partida FOREIGN KEY (SlotId) REFERENCES dbo.Partida(SlotId) ON DELETE CASCADE,
        CONSTRAINT CK_ObjetoPartida_Realidad CHECK (Realidad IN (N'REALIDAD 102', N'REALIDAD 999')),
        CONSTRAINT CK_ObjetoPartida_Tipo CHECK (Tipo IN (N'ENERGIA', N'CELULA'))
    );
END;
GO

IF OBJECT_ID(N'dbo.PortalPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PortalPartida
    (
        SlotId     int           NOT NULL,
        Realidad   nvarchar(20)  NOT NULL,
        PosicionX  float         NOT NULL,
        PosicionY  float         NOT NULL,
        CONSTRAINT PK_PortalPartida PRIMARY KEY (SlotId, Realidad),
        CONSTRAINT FK_PortalPartida_Partida FOREIGN KEY (SlotId) REFERENCES dbo.Partida(SlotId) ON DELETE CASCADE,
        CONSTRAINT CK_PortalPartida_Realidad CHECK (Realidad IN (N'REALIDAD 102', N'REALIDAD 999'))
    );
END;
GO

PRINT N'Esquema NEXUS creado o ya existente.';
