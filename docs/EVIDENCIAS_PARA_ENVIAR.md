# Evidencias que debes enviar

Usa capturas de tu propia ejecución de NEXUS y SQL Server. No edites los resultados ni incluyas contraseñas o cadenas de conexión privadas.

## Consultas SQL

1. Abre `database/sql/03_consultas_verificacion.sql` en SQL Server Management Studio (SSMS).
2. Conéctate a la misma instancia que usa NEXUS. Si usas la configuración predeterminada, normalmente es `(localdb)\MSSQLLocalDB`.
3. Ejecuta el script completo después de guardar una partida desde el juego.
4. Envía capturas de los resultados de las consultas 1, 2, 3, 4, 5, 6 y 7. La consulta 8 puede tardar un poco; su resultado debe indicar que no se detectaron infracciones.
5. En la consulta 1 debe aparecer el cadete y su estado. En la 2 aparecen los recuentos. Las consultas 3–6 muestran los datos dependientes. En la 7 todas las columnas `Errores` deben valer `0`.

Si la consulta 1 no devuelve filas, inicia NEXUS, crea una partida, guarda con **GUARDAR PARTIDA** y vuelve a ejecutar el script. Si aparece un error de conexión, envía una captura del mensaje sin datos secretos y confirma que SSMS está conectado a la instancia correcta.

## Persistencia de una partida

1. Inicia NEXUS y registra un cadete.
2. Muévete a otra posición, recoge un núcleo u objeto si está disponible y guarda la partida.
3. Toma una captura del mapa y del mensaje de guardado.
4. Cierra el juego por completo. Abre NEXUS otra vez, carga la partida y toma otra captura donde se vea el mismo cadete, realidad, estado y progreso.
5. Ejecuta la consulta SQL 1 y las consultas de recuentos para acompañar las capturas.

## Prueba de `RecuperarEnergia()`

1. Con energía y conexión por debajo del máximo, pulsa **RECUPERAR ENERGÍA** y captura los valores antes y después. La acción correcta restaura energía y conexión hasta sus límites y consume la recuperación única.
2. Guarda y vuelve a cargar; captura que la recuperación usada sigue marcada y que no vuelve a estar disponible.
3. Inicia una partida nueva con energía y conexión ya al máximo; pulsa **RECUPERAR ENERGÍA**. Debe indicar que no hace falta recuperar y no debe gastar el uso único.

## Pruebas visuales y de controles

Envía capturas o una grabación breve de:

- Formularios 1, 3 y 4 abiertos; formulario 2 con el mapa.
- Movimiento en diagonal, correr con Shift, salto/agacharse, según corresponda al estado actual del juego.
- Encuentro y combate: vida de ambos, disparos, ataque del monstruo, derrota o regreso al mapa.
- Portal visible, cambio de realidad y contador de núcleos.
- Publicación de la rama `testing-debugging`, commit visible en GitHub y, si ya existe, la Release.

## Nombres sugeridos

Guarda las imágenes con nombres fáciles de revisar, por ejemplo `01-menu.png`, `02-mapa-y-controles.png`, `03-recuperacion.png`, `04-combate.png`, `05-guardado.png`, `06-carga.png`, `07-sql-partida.png` y `08-sql-integridad.png`.

No marques una prueba como aprobada hasta ejecutarla. Para las pruebas SQL, adjunta la captura del resultado real de SSMS; para las pruebas del juego, adjunta la pantalla real de NEXUS.
