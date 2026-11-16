# NEXUS V2.0 - Windows Forms

La solución contiene dos proyectos:

- `Nexus.V.02`: versión original de consola, conservada.
- `NexusWindowsForm`: nueva interfaz de escritorio en Windows Forms.

## Ejecutar la versión gráfica

1. Abre `Nexus.V.02.slnx` en Visual Studio 2022/2026 con el SDK de .NET 10 instalado.
2. En el Explorador de soluciones, haz clic derecho en `NexusWindowsForm`.
3. Selecciona **Establecer como proyecto de inicio**.
4. Ejecuta con **F5**.

El proyecto gráfico utiliza `.NET 10` y Windows Forms. Debe ejecutarse en Windows.

## Editar la interfaz

La estructura visual y sus estilos están centralizados en `NexusWindowsForm/Form1.cs`:

- `BuildInterface()`: ventana principal, barra superior y navegación.
- `ShowRegistration()`: formulario de registro.
- `ShowDashboard()`: panel principal.
- `ShowExploration()`: mapa y controles.
- `ShowInventory()` y `ShowProfile()`: paneles de información.
- `MakeButton()`, `MakeLabel()` y `CreateCard()`: estilos reutilizables.

Los controles se crean por código para que el diseño sea fácil de ajustar desde esos métodos. El mapa se genera dinámicamente según el estado del juego.

## Mecánicas incluidas

- Registro de explorador.
- Realidad 102 y Realidad 999.
- Mapa dimensional de 10 x 10.
- Movimiento con W/A/S/D y flechas.
- Vida, energía y conexión.
- Recolección de células de energía.
- Combate contra bichos dimensionales.
- Análisis de anomalías.
- Recuperación de energía.
- Inventario y perfil.
- Registro del núcleo y finalización de misión.

La versión original de consola permanece en su carpeta y no se elimina.
