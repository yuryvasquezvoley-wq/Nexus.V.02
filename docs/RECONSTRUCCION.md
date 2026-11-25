# Clonación limpia y reconstrucción

## Preparar una copia nueva

1. En Visual Studio, usa **Clonar un repositorio** y clona el repositorio actualizado en una carpeta distinta a la copia usada para desarrollar.
2. Abre `Nexus.V.02.slnx` desde la carpeta nueva y restaura los paquetes NuGet.
3. Comprueba que `NexusWindowsForm` sea el proyecto de inicio y compila en `Debug`.
4. Ejecuta los scripts desde la raíz del repositorio en este orden:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i "database\sql\01_crear_base.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i "database\sql\02_prueba_esquema.sql"
```

5. Si usas otra instancia, crea `NexusGame` allí y configura `NEXUS_SQL_CONNECTION` con una cadena válida para esa instancia. No guardes credenciales personales dentro del repositorio.
6. Inicia NEXUS, registra el cadete y prueba guardado, cierre, nueva apertura y carga.
7. Ejecuta `database/sql/03_consultas_verificacion.sql` en esa misma base y guarda capturas de resultados para el informe.

## Tablas

- `Partida`: slot, cadete, posición, realidad, vida, energía, conexión y progreso.
- `NucleoPartida`: núcleos que el cadete ya recogió.
- `EnemigoPartida`: estado, salud y posición de enemigos de cada realidad.
- `ObjetoPartida`: objetos de energía/células que quedan en el mapa.
- `PortalPartida`: posición de portal en cada realidad.

## Requisito de ejecución

La compilación en este proyecto apunta a `net11.0-windows`; el equipo limpio necesita una versión compatible del SDK y de Windows Desktop Runtime. Para una reconstrucción no se deben copiar `bin`, `obj`, perfiles del IDE ni bases de datos locales de la copia original.

## Resultado actual

La compilación fuente se ejecutó en el entorno de desarrollo con 0 errores. SSMS mostró una sesión conectada a LocalDB, pero no se ejecutaron scripts desde esa ventana para no interrumpir la interacción del usuario. Para completar esta actividad, ejecuta los dos scripts desde SSMS en la copia limpia y adjunta las capturas de SSMS y NEXUS. No presentes la persistencia como validada mientras no realices ese paso.
