# Pruebas y depuración — rama `testing-debugging`

## Alcance

Este documento registra qué se comprobó en la rama y qué necesita una instancia real de SQL Server o una demostración manual. No se anotan como aprobadas pruebas que no se ejecutaron.

## Matriz de pruebas

| ID | Escenario | Resultado esperado | Estado actual |
|---|---|---|---|
| T01 | Compilar `Nexus.V.02.slnx` | 0 errores | Aprobado: 0 errores; el SDK informó advertencias |
| T02 | Cadete con nombre vacío o demasiado largo | Validación visible y no iniciar partida inválida | Pendiente de captura manual |
| T03 | Edad fuera del rango permitido | Rechazar entrada inválida | Pendiente de captura manual |
| T04 | `RecuperarEnergia`, energía/conexión bajo el máximo, uso disponible | Aumenta 20 de energía y 10 de conexión, con tope 100, y consume el uso | Lógica incluida; pendiente de interacción/captura en el formulario |
| T05 | `RecuperarEnergia`, ambos valores completos | No modifica recursos ni consume el uso | Lógica corregida; pendiente de interacción/captura en el formulario |
| T06 | Intentar recuperar por segunda vez | No volver a aumentar recursos | Pendiente de captura manual |
| T07 | Guardar una partida con datos | Persistir todos los datos en el slot 1 | Pendiente: ejecutar desde NEXUS y SSMS |
| T08 | Cerrar y cargar una partida guardada | Restaurar jugador y colecciones | Pendiente: ejecutar desde NEXUS y SSMS |
| T09 | Desconexión o nombre de base inválido | Mensaje entendible; no cerrar inesperadamente el juego | Pendiente de prueba con SQL Server |
| T10 | Ejecutar `02_prueba_esquema.sql` | Confirmar tablas/relaciones con rollback | Pendiente de ejecutar en SSMS |
| T11 | Ejecutar `03_consultas_verificacion.sql` | Mostrar resumen e integridad, errores 0 | Pendiente de SSMS con partida real guardada |
| T12 | Guardar desde el menú después de volver del mapa | Persistir el último estado de sesión en el slot SQL | Pendiente de interacción/captura |
| T13 | Cargar desde el menú principal | Abrir el estado SQL guardado sin crear un cadete nuevo | Pendiente de interacción/captura |
| T14 | Eliminar desde el menú y revisar conteos SQL | Borrar el slot; sus tablas dependientes quedan vacías por cascada | Pendiente de interacción/captura |

## Cambio revisado: energía

### Antes

La interfaz podía marcar `recuperacionUsada = true` incluso cuando energía y conexión ya estaban en 100. El cadete perdía una acción sin recibir nada.

### Después

`ReglasPartida.RecuperarEnergia` calcula primero los nuevos valores. Si no cambia ninguno, devuelve `Aplicada = false` sin gastar el uso. Si cambian, suma +20 y +10 con máximo 100 y la interfaz marca la recuperación como usada. También rechaza valores fuera de 0–100.

## Ejecución pendiente en la computadora de demostración

1. Ejecutar los casos T02–T06 y capturar la pantalla de NEXUS con resultado.
2. Configurar LocalDB o SQL Server y ejecutar `01_crear_base.sql`.
3. Ejecutar `02_prueba_esquema.sql`; después iniciar NEXUS, guardar, cerrar y cargar.
4. Ejecutar `03_consultas_verificacion.sql` y adjuntar los resultados.
5. Si una prueba falla, anotar el mensaje exacto, corregirlo, repetir el mismo caso y guardar ambas capturas.

La comprobación de compilación y la comprobación funcional contra SQL son actividades diferentes; la primera no demuestra la segunda. SSMS estaba conectado a LocalDB en el equipo, pero no ejecuté scripts mientras detecté uso activo de esa ventana.
