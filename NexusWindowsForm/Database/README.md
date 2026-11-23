# Base de datos de NEXUS

La aplicación Windows Forms guarda el registro de cada cadete en SQL Server.

## Requisitos

- Windows.
- SQL Server LocalDB (normalmente se instala con Visual Studio al seleccionar los componentes de SQL Server Express/LocalDB).
- Restaurar los paquetes NuGet del proyecto al abrirlo.

## Funcionamiento

Al pulsar **INICIALIZAR NEXUS**, después de elegir la realidad:

1. Se crea automáticamente la base de datos `NEXUS_DB` si todavía no existe.
2. Se registra el cadete en `dbo.Jugadores`. El nombre se usa como identificador único sin distinguir mayúsculas/minúsculas.
3. Cada entrada al juego crea una fila nueva en `dbo.Ingresos`, por lo que se conserva el historial de sesiones.
4. Al cerrar la partida, se actualizan la energía, vida, conexión y realidad del cadete, y se guarda la fecha de salida.

Las tablas también se crean automáticamente. El archivo `NEXUS_DB.sql` contiene el mismo esquema para crearlo manualmente desde SQL Server Management Studio.

## Conexión

La conexión predeterminada es:

```
Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;
```

Si utilizas otra instancia de SQL Server, define la variable de entorno `NEXUS_SQL_CONNECTION` antes de abrir el juego. Por ejemplo:

```
Server=localhost\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True;
```

La cuenta de Windows debe tener permiso para crear la base de datos la primera vez y escribir en ella después.
