USE NexusGame;
GO

-- CONSULTA 1: datos generales de la última partida guardada.
SELECT SlotId, NombreCadete, Edad, Realidad, PosicionX, PosicionY,
       Vida, Energia, Conexion, CelulasMedicas, Resistencia,
       NaveReparada, RecuperacionUsada, FechaGuardado
FROM dbo.Partida
WHERE SlotId = 1;
GO

-- CONSULTA 2: resumen de registros guardados por tipo.
SELECT N'Partidas' AS Tipo, COUNT_BIG(*) AS Cantidad
FROM dbo.Partida WHERE SlotId = 1
UNION ALL
SELECT N'Núcleos', COUNT_BIG(*)
FROM dbo.NucleoPartida WHERE SlotId = 1
UNION ALL
SELECT N'Enemigos', COUNT_BIG(*)
FROM dbo.EnemigoPartida WHERE SlotId = 1
UNION ALL
SELECT N'Objetos', COUNT_BIG(*)
FROM dbo.ObjetoPartida WHERE SlotId = 1
UNION ALL
SELECT N'Portales', COUNT_BIG(*)
FROM dbo.PortalPartida WHERE SlotId = 1;
GO

-- CONSULTA 3: núcleos recuperados en la partida.
SELECT NucleoId
FROM dbo.NucleoPartida
WHERE SlotId = 1
ORDER BY NucleoId;
GO

-- CONSULTA 4: enemigos por realidad, vida restante y estado.
SELECT Realidad, EnemigoId, Nombre, Tipo, Vida, VidaMaxima,
       PosicionX, PosicionY, Derrotada
FROM dbo.EnemigoPartida
WHERE SlotId = 1
ORDER BY Realidad, EnemigoId;
GO

-- CONSULTA 5: objetos que siguen en el mapa por realidad y categoría.
SELECT Realidad, Tipo, COUNT_BIG(*) AS Cantidad,
       MIN(Indice) AS PrimerIndice, MAX(Indice) AS UltimoIndice
FROM dbo.ObjetoPartida
WHERE SlotId = 1
GROUP BY Realidad, Tipo
ORDER BY Realidad, Tipo;
GO

-- CONSULTA 6: portales guardados y sus coordenadas.
SELECT Realidad, PosicionX, PosicionY
FROM dbo.PortalPartida
WHERE SlotId = 1
ORDER BY Realidad;
GO

-- CONSULTA 7: controles de integridad. Cada fila debe mostrar 0.
SELECT N'Enemigos fuera del rango de vida' AS Comprobacion, COUNT_BIG(*) AS Errores
FROM dbo.EnemigoPartida
WHERE SlotId = 1 AND (Vida < 0 OR Vida > VidaMaxima OR VidaMaxima <= 0)
UNION ALL
SELECT N'Enemigos con realidad inválida', COUNT_BIG(*)
FROM dbo.EnemigoPartida
WHERE SlotId = 1 AND Realidad NOT IN (N'REALIDAD 102', N'REALIDAD 999')
UNION ALL
SELECT N'Enemigos fuera del mapa o con velocidad inválida', COUNT_BIG(*)
FROM dbo.EnemigoPartida
WHERE SlotId = 1
  AND (PosicionX NOT BETWEEN 0 AND 5000 OR PosicionY NOT BETWEEN 0 AND 5000
       OR DestinoX NOT BETWEEN 0 AND 5000 OR DestinoY NOT BETWEEN 0 AND 5000
       OR Velocidad NOT BETWEEN 0 AND 10)
UNION ALL
SELECT N'Objetos con tipo o realidad inválidos', COUNT_BIG(*)
FROM dbo.ObjetoPartida
WHERE SlotId = 1
  AND (Tipo NOT IN (N'ENERGIA', N'CELULA')
       OR Realidad NOT IN (N'REALIDAD 102', N'REALIDAD 999'))
UNION ALL
SELECT N'Objetos fuera del mapa o con índice negativo', COUNT_BIG(*)
FROM dbo.ObjetoPartida
WHERE SlotId = 1
  AND (Indice < 0 OR PosicionX NOT BETWEEN 0 AND 5000 OR PosicionY NOT BETWEEN 0 AND 5000)
UNION ALL
SELECT N'Portales con realidad inválida', COUNT_BIG(*)
FROM dbo.PortalPartida
WHERE SlotId = 1 AND Realidad NOT IN (N'REALIDAD 102', N'REALIDAD 999')
UNION ALL
SELECT N'Portales fuera del mapa', COUNT_BIG(*)
FROM dbo.PortalPartida
WHERE SlotId = 1
  AND (PosicionX NOT BETWEEN 0 AND 5000 OR PosicionY NOT BETWEEN 0 AND 5000);
GO

-- CONSULTA 8: integridad de claves foráneas declaradas en el esquema.
DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS;
GO
