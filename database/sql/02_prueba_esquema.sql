USE NexusGame;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Partida', N'U') IS NULL OR
   OBJECT_ID(N'dbo.EnemigoPartida', N'U') IS NULL OR
   OBJECT_ID(N'dbo.NucleoPartida', N'U') IS NULL OR
   OBJECT_ID(N'dbo.ObjetoPartida', N'U') IS NULL OR
   OBJECT_ID(N'dbo.PortalPartida', N'U') IS NULL
    THROW 51000, N'Falta una tabla del esquema NEXUS.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Partida WHERE SlotId=1)
    INSERT dbo.Partida
        (SlotId, NombreCadete, Edad, Realidad, PosicionX, PosicionY, Vida, Energia,
         Conexion, CelulasMedicas, Resistencia, NaveReparada, RecuperacionUsada, FechaGuardado)
    VALUES
        (1, N'PRUEBA SQL', 25, N'REALIDAD 102', 2500, 2500, 100, 80,
         100, 0, 75, 0, 0, SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM dbo.EnemigoPartida WHERE SlotId=1 AND EnemigoId=N'PRUEBA-ENTIDAD')
    INSERT dbo.EnemigoPartida
        (SlotId, EnemigoId, Realidad, Nombre, Tipo, PosicionX, PosicionY, DestinoX, DestinoY,
         Vida, VidaMaxima, Velocidad, EsJefe, EsZombie, Derrotada)
    VALUES
        (1, N'PRUEBA-ENTIDAD', N'REALIDAD 102', N'ACECHADOR', N'PRUEBA', 2600, 2500, 2700, 2500,
         150, 200, 1.5, 0, 1, 0);

IF NOT EXISTS (SELECT 1 FROM dbo.NucleoPartida WHERE SlotId=1 AND NucleoId=N'PRUEBA-NUCLEO')
    INSERT dbo.NucleoPartida (SlotId, NucleoId) VALUES (1, N'PRUEBA-NUCLEO');

IF NOT EXISTS (SELECT 1 FROM dbo.ObjetoPartida WHERE SlotId=1 AND Realidad=N'REALIDAD 102' AND Tipo=N'ENERGIA' AND Indice=999)
    INSERT dbo.ObjetoPartida (SlotId, Realidad, Tipo, Indice, PosicionX, PosicionY)
    VALUES (1, N'REALIDAD 102', N'ENERGIA', 999, 2800, 2500);

IF NOT EXISTS (SELECT 1 FROM dbo.PortalPartida WHERE SlotId=1 AND Realidad=N'REALIDAD 102')
    INSERT dbo.PortalPartida (SlotId, Realidad, PosicionX, PosicionY)
    VALUES (1, N'REALIDAD 102', 4700, 4500);

IF NOT EXISTS (SELECT 1 FROM dbo.EnemigoPartida WHERE SlotId=1 AND EnemigoId=N'PRUEBA-ENTIDAD' AND Vida=150)
    THROW 51001, N'No se pudo recuperar el estado de prueba del enemigo.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.NucleoPartida WHERE SlotId=1 AND NucleoId=N'PRUEBA-NUCLEO')
    THROW 51002, N'No se pudo recuperar el núcleo de prueba.', 1;

PRINT N'Prueba de tablas y lectura de estado correcta; se revierte la transacción.';
ROLLBACK TRANSACTION;
GO
