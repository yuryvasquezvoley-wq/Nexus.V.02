# Arquitectura de NEXUS

## Flujo de formularios

```text
Program.Main
  └─ Form1 (menú)
      └─ Form2 (registro, mapa, guardar/cargar)
          └─ Form3 (cinemática de historia)
              └─ Form4 (secuencia de meteoritos durante la historia)
          └─ Form2 cambia del registro al mapa
      ├─ Cargar partida → Form2 restaura directamente el último slot
      └─ Guardar/eliminar → RepositorioPartidasSql opera sobre el slot SQL
```

`Program.Main` prepara WinForms y abre `Form1`. El menú abre `Form2`, que contiene el registro y el mapa como paneles de la misma ventana. Al iniciar, `Form2` valida que se haya escrito el nombre y abre `Form3` con la historia. Desde la primera escena, la barra espaciadora abre `Form4` para la secuencia arcade; al cerrarla, la historia continúa. Al terminar, `Form2` cambia del registro al mapa.

## Componentes

| Área | Archivo o carpeta | Responsabilidad |
|---|---|---|
| Inicio y formularios | `Program.cs`, `Form1.cs`–`Form4.cs` | Menú, registro/mapa, historia y secuencia espacial |
| Renderizado | `BufferedPanel.cs`, `Form2.cs` | Dibujo del mapa y entidades con panel de doble búfer |
| Reglas de partida | `Persistencia/ReglasPartida.cs` | Regla pura de recuperación de energía/conexión |
| Modelo persistido | `Persistencia/EstadoPartida.cs` | Jugador, enemigos, objetos, portales y núcleos |
| Repositorio | `Persistencia/RepositorioPartidasSql.cs` | Guardado transaccional y recuperación de un slot SQL |
| Datos | `database/sql/*.sql` | Esquema, comprobación transaccional y consultas de inspección |
| Recursos | `Resources/` | Imágenes, música y efectos de sonido |

## Persistencia

El repositorio `RepositorioPartidasSql` guarda un slot (`SlotId = 1`) dentro de una transacción. Actualiza la partida y reemplaza sus colecciones dependientes en un solo commit SQL para evitar dejar un guardado a medias. Las tablas hijas tienen claves foráneas y se eliminan en cascada. El mapa impide guardar/cargar durante el combate. El estado guardado incluye al cadete, recursos, realidad, núcleos recuperados, enemigos por realidad, objetos aún presentes y portales.

El acceso se configura con `NEXUS_SQL_CONNECTION`; si no está definida, usa LocalDB `MSSQLLocalDB`. El script de creación es la fuente del esquema; no se depende de archivos de base de datos locales incluidos en el proyecto. El menú guarda el último estado de sesión devuelto por el mapa, abre la partida persistida directamente y permite eliminar el slot y sus filas dependientes dentro de una transacción.

## Método mejorado: `RecuperarEnergia`

En `Form2`, el evento del botón delega la decisión a `ReglasPartida.RecuperarEnergia(energia, conexion, recuperacionUsada)`. La función limita ambos recursos a 100, permite un uso y no consume ese uso si energía y conexión ya están completas. Devuelve valores nuevos y un resultado que la interfaz convierte en mensaje.

Antes, la acción marcaba el uso como gastado aun cuando ambos medidores estaban llenos. Después, marca la recuperación como usada solamente cuando realmente aumenta energía o conexión.

## Diagrama de guardado

```text
Botón Guardar → Form2 captura entidades → EstadoPartida
   → RepositorioPartidasSql valida → abre transacción
   → upsert Partida + reemplaza tablas hijas → COMMIT

Botón Cargar → SELECT Partida y tablas hijas
   → Form2 restaura jugador, mapa, recursos y colecciones
Botón Eliminar → DELETE tablas dependientes → DELETE Partida → COMMIT
```
