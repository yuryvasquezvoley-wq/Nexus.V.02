# NEXUS — Windows Forms

Juego 2D en C# y Windows Forms con registro de cadete, mapa, realidades, combate, secuencia espacial y persistencia de una partida en SQL Server.

## Requisitos

- Windows 10/11.
- Visual Studio con la carga **Desarrollo de escritorio de .NET** y el SDK .NET 11.
- SQL Server LocalDB (incluido normalmente con Visual Studio) o una instancia SQL Server accesible.
- SQL Server Management Studio es opcional, útil para ejecutar los scripts y revisar las evidencias.

## Abrir, compilar y ejecutar

Abre `Nexus.V.02.slnx` en Visual Studio, selecciona `NexusWindowsForm` como proyecto de inicio y ejecuta. También puedes compilar desde la raíz:

```powershell
dotnet build Nexus.V.02.slnx
```

## Crear la base de datos

En una consola con `sqlcmd` y acceso a LocalDB, desde la raíz del repositorio:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i "database\sql\01_crear_base.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i "database\sql\02_prueba_esquema.sql"
```

NEXUS usa por defecto:

```text
Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=NexusGame;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5
```

Para otra instancia, define `NEXUS_SQL_CONNECTION` antes de iniciar el juego. Ejemplo para PowerShell:

```powershell
$env:NEXUS_SQL_CONNECTION = "Data Source=localhost;Initial Catalog=NexusGame;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5"
```

Después, guarda y carga desde los botones del mapa. Si no existe una partida guardada, **CARGAR PARTIDA** informa que no hay datos. La partida no se guarda automáticamente.

## Base de código

- `NexusWindowsForm/Form1.cs`: menú y música.
- `NexusWindowsForm/Form2.cs`: registro y, después del inicio, mapa, controles, encuentros, recursos y guardado/carga.
- `NexusWindowsForm/Form3.cs`: historia de misión y continuación hacia el mapa.
- `NexusWindowsForm/Form4.cs`: secuencia arcade de meteoritos.
- `NexusWindowsForm/Persistencia/`: modelo de estado, reglas simples y repositorio SQL Server.
- `database/sql/`: creación del esquema, prueba transaccional y consultas de verificación.
- `NexusWindowsForm/Resources/`: gráficos y audio del juego.
- `docs/`: arquitectura, configuración, pruebas y evidencia.

## Documentos

- [Arquitectura y flujo](docs/ARQUITECTURA.md)
- [Reconstrucción limpia y SQL](docs/RECONSTRUCCION.md)
- [Pruebas y depuración](docs/PRUEBAS_DEBUGGING.md)
- [Evidencias que debes enviar](docs/EVIDENCIAS_PARA_ENVIAR.md)
- [Consultas SQL de verificación](database/sql/03_consultas_verificacion.sql)

## Estado de verificación en este entorno

La solución se compiló en Windows con 0 errores y 60 advertencias del proyecto/SDK. La consola de desarrollo no pudo abrir LocalDB; SSMS sí mostró una conexión activa del usuario, pero no ejecuté consultas en esa ventana porque detectó interacción en curso. Ejecuta los scripts desde SSMS y captura el resultado antes de marcar guardado/recuperación como aprobados. El paquete Windows se generó en `output/release`; aún requiere probarse en otro equipo y adjuntarse a una GitHub Release.
