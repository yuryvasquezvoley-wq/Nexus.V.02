from pathlib import Path
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import A4, landscape
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfgen import canvas
from reportlab.platypus import Paragraph

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "output" / "pdf"
OUT.mkdir(parents=True, exist_ok=True)

pdfmetrics.registerFont(TTFont("Arial", r"C:\Windows\Fonts\arial.ttf"))
pdfmetrics.registerFont(TTFont("ArialBold", r"C:\Windows\Fonts\arialbd.ttf"))
pdfmetrics.registerFont(TTFont("Consolas", r"C:\Windows\Fonts\consola.ttf"))
pdfmetrics.registerFont(TTFont("ConsolasBold", r"C:\Windows\Fonts\consolab.ttf"))

BG = colors.HexColor("#080D17")
PANEL = colors.HexColor("#111B2A")
PANEL2 = colors.HexColor("#17263A")
CYAN = colors.HexColor("#71E5F6")
RED = colors.HexColor("#FF3153")
GREEN = colors.HexColor("#57E38B")
GOLD = colors.HexColor("#FFC66D")
WHITE = colors.HexColor("#F1F3F7")
MUTED = colors.HexColor("#BAC5D2")

def paragraph(c, text, x, top, width, font="Arial", size=11, color=WHITE, leading=None, align=TA_LEFT):
    style = ParagraphStyle(
        "p", fontName=font, fontSize=size, leading=leading or size * 1.35,
        textColor=color, alignment=align, spaceAfter=0, splitLongWords=True,
    )
    obj = Paragraph(text, style)
    _, height = obj.wrap(width, 1000)
    obj.drawOn(c, x, top - height)
    return top - height

def background(c, w, h, page, section):
    c.setFillColor(BG)
    c.rect(0, 0, w, h, fill=1, stroke=0)
    c.setFillColor(RED)
    c.rect(0, h - 8, w, 8, fill=1, stroke=0)
    c.setFillColor(CYAN)
    c.setFont("ConsolasBold", 9)
    c.drawString(34, 22, "NEXUS  //  " + section.upper())
    c.setFillColor(MUTED)
    c.drawRightString(w - 34, 22, f"{page:02d}")

def card(c, x, y, w, h, title, text, accent=CYAN, text_size=10):
    c.setFillColor(PANEL)
    c.roundRect(x, y, w, h, 10, fill=1, stroke=0)
    c.setFillColor(accent)
    c.roundRect(x, y + h - 5, w, 5, 3, fill=1, stroke=0)
    c.setFillColor(accent)
    c.setFont("ConsolasBold", 12)
    c.drawString(x + 16, y + h - 26, title)
    paragraph(c, text, x + 16, y + h - 39, w - 32, size=text_size, color=WHITE)

def build_slides():
    path = OUT / "Presentacion_Arquitectura_NEXUS.pdf"
    w, h = 960, 540
    c = canvas.Canvas(str(path), pagesize=(w, h))

    # 1. Cover
    background(c, w, h, 1, "Presentación de arquitectura")
    c.setFillColor(CYAN)
    c.setFont("ConsolasBold", 17)
    c.drawString(62, 428, "VIAJE ENTRE REALIDADES")
    c.setFillColor(WHITE)
    c.setFont("ConsolasBold", 56)
    c.drawString(58, 350, "NEXUS")
    paragraph(c, "Proyecto 2D en C# y Windows Forms", 62, 322, 510, font="ArialBold", size=19, color=WHITE)
    paragraph(c, "Arquitectura, reglas de juego, persistencia SQL Server y plan de pruebas.", 62, 277, 495, size=13, color=MUTED)
    card(c, 62, 112, 246, 106, "INTERFAZ", "Cuatro formularios: menú, registro/mapa, historia y secuencia espacial.", CYAN, 10)
    card(c, 326, 112, 246, 106, "JUEGO", "Movimiento, recursos, enemigos, portales y combate dentro del mapa.", RED, 10)
    card(c, 590, 112, 300, 106, "ESTADO", "Guardado de una partida y colecciones en tablas relacionadas.", GREEN, 10)
    c.setFillColor(RED)
    c.circle(798, 360, 77, stroke=0, fill=1)
    c.setFillColor(PANEL)
    c.circle(798, 360, 64, stroke=0, fill=1)
    c.setFillColor(CYAN)
    c.circle(798, 360, 39, stroke=0, fill=1)
    c.setFillColor(PANEL)
    c.circle(798, 360, 24, stroke=0, fill=1)
    c.setFillColor(WHITE)
    c.setFont("ConsolasBold", 11)
    c.drawCentredString(798, 352, "NEXUS")
    paragraph(c, "Estado: compilación comprobada; validación SQL y pruebas en equipo de demostración pendientes.", 62, 85, 820, size=10, color=GOLD)
    c.showPage()

    # 2. Architecture
    background(c, w, h, 2, "Arquitectura")
    c.setFillColor(WHITE); c.setFont("ConsolasBold", 27); c.drawString(54, 475, "Flujo de pantallas y código")
    boxes = [
        (58, 344, 180, 72, "Form1", "Menú"),
        (278, 344, 190, 72, "Form2", "Registro / mapa"),
        (512, 344, 190, 72, "Form3", "Historia"),
        (746, 344, 164, 72, "Form4", "Meteoritos"),
    ]
    for x, y, bw, bh, title, subtitle in boxes:
        c.setFillColor(PANEL2); c.roundRect(x, y, bw, bh, 9, fill=1, stroke=0)
        c.setFillColor(CYAN if title != "Form4" else RED); c.setFont("ConsolasBold", 15); c.drawCentredString(x+bw/2, y+43, title)
        c.setFillColor(WHITE); c.setFont("Arial", 10); c.drawCentredString(x+bw/2, y+22, subtitle)
    c.setStrokeColor(CYAN); c.setLineWidth(2)
    for x1, x2 in [(238, 278), (468, 512), (702, 746)]:
        c.line(x1, 380, x2-8, 380); c.line(x2-8, 380, x2-15, 384); c.line(x2-8, 380, x2-15, 376)
    c.setStrokeColor(RED); c.line(607, 335, 607, 286); c.line(607, 286, 842, 286); c.line(842, 286, 842, 335)
    c.setFillColor(MUTED); c.setFont("Arial", 9); c.drawCentredString(725, 270, "Form4 se abre desde la historia y luego regresa a Form3")
    card(c, 58, 88, 254, 130, "REGLAS", "ReglasPartida.cs calcula recuperación limitada sin depender de controles visuales.", GREEN)
    card(c, 333, 88, 278, 130, "JUGABILIDAD", "Form2 concentra temporizadores, dibujo, teclado, colisiones, enemigos y recursos.", RED)
    card(c, 632, 88, 278, 130, "DATOS", "RepositorioPartidasSql valida y guarda el estado por medio de una transacción.", CYAN)
    c.showPage()

    # 3. Method change
    background(c, w, h, 3, "Reto de C#")
    c.setFillColor(WHITE); c.setFont("ConsolasBold", 27); c.drawString(54, 475, "Mejora de RecuperarEnergia")
    card(c, 58, 260, 392, 160, "ANTES", "La acción podía consumir el único uso aunque energía y conexión ya estuvieran completas.", RED, 12)
    card(c, 510, 260, 392, 160, "DESPUÉS", "Primero calcula el resultado. Si nada aumenta, no consume el uso; si aplica, suma +20 energía y +10 conexión con tope 100.", GREEN, 12)
    c.setFillColor(PANEL2); c.roundRect(58, 96, 844, 124, 12, fill=1, stroke=0)
    c.setFillColor(CYAN); c.setFont("ConsolasBold", 13); c.drawString(80, 191, "ENTRADAS  →  RESULTADO")
    metrics = [(135, "ENERGÍA", "+20", "máximo 100"), (386, "CONEXIÓN", "+10", "máximo 100"), (642, "USO", "1 vez", "solo si aplica")]
    for x, label, value, foot in metrics:
        c.setFillColor(WHITE); c.setFont("ArialBold", 10); c.drawCentredString(x, 158, label)
        c.setFillColor(GOLD); c.setFont("ConsolasBold", 23); c.drawCentredString(x, 128, value)
        c.setFillColor(MUTED); c.setFont("Arial", 9); c.drawCentredString(x, 109, foot)
    paragraph(c, "Comprobación de código: la solución compila. La interacción del botón debe capturarse en tu ejecución.", 58, 75, 820, size=10, color=GOLD)
    c.showPage()

    # 4. SQL model
    background(c, w, h, 4, "Persistencia SQL")
    c.setFillColor(WHITE); c.setFont("ConsolasBold", 27); c.drawString(54, 475, "Partida y tablas relacionadas")
    c.setFillColor(PANEL2); c.roundRect(350, 300, 260, 120, 12, fill=1, stroke=0)
    c.setFillColor(CYAN); c.setFont("ConsolasBold", 17); c.drawCentredString(480, 382, "Partida")
    paragraph(c, "Cadete, realidad, posición, vida, energía, conexión y progreso.", 370, 360, 220, size=10, color=WHITE, align=TA_CENTER)
    children = [
        (55, 125, "NucleoPartida", "Núcleos recogidos"),
        (275, 125, "EnemigoPartida", "Vida y posición"),
        (495, 125, "ObjetoPartida", "Objetos en mapa"),
        (715, 125, "PortalPartida", "Portal por realidad"),
    ]
    for x, y, title, detail in children:
        c.setFillColor(PANEL); c.roundRect(x, y, 190, 100, 10, fill=1, stroke=0)
        c.setFillColor(GREEN); c.setFont("ConsolasBold", 11); c.drawCentredString(x+95, y+64, title)
        c.setFillColor(WHITE); c.setFont("Arial", 9); c.drawCentredString(x+95, y+40, detail)
        c.setStrokeColor(CYAN); c.line(x+95, y+100, x+95, 300)
    paragraph(c, "Scripts: 01 crea el esquema; 02 valida tablas con transacción reversible; 03 consulta la partida real, sus colecciones y restricciones.", 58, 94, 842, size=11, color=WHITE)
    paragraph(c, "Las consultas son de lectura. No generan resultados hasta que exista una partida guardada en NexusGame.", 58, 58, 842, size=9, color=GOLD)
    c.showPage()

    # 5. Verification status
    background(c, w, h, 5, "Verificación y entrega")
    c.setFillColor(WHITE); c.setFont("ConsolasBold", 27); c.drawString(54, 475, "Lo comprobado y lo que falta mostrar")
    card(c, 58, 302, 398, 118, "COMPROBADO AQUÍ", "La solución Windows Forms compila con 0 errores. Documentación, esquema SQL y consultas quedaron en el repositorio.", GREEN, 11)
    card(c, 504, 302, 398, 118, "PENDIENTE EN TU PC", "Ejecutar partida, persistencia contra SQL Server, reconstrucción limpia y capturar resultados reales de SSMS y del juego.", GOLD, 11)
    steps = [
        "1. Inicia NEXUS y guarda una partida.",
        "2. Cierra y carga la partida; confirma que el estado persiste.",
        "3. Ejecuta 03_consultas_verificacion.sql y captura resultados.",
        "4. Enseña commit, rama testing-debugging y Release si está publicada.",
    ]
    y = 248
    for line in steps:
        c.setFillColor(CYAN); c.circle(72, y+2, 4, fill=1, stroke=0)
        paragraph(c, line, 88, y+12, 770, size=12, color=WHITE)
        y -= 42
    c.setFillColor(RED); c.setFont("ConsolasBold", 11); c.drawString(58, 71, "No se reporta como aprobada una prueba que aún no se ejecutó.")
    c.save()
    return path

def build_report():
    path = OUT / "Informe_Verificacion_NEXUS.pdf"
    w, h = A4
    c = canvas.Canvas(str(path), pagesize=A4)
    page = 0

    def start(title, kicker):
        nonlocal page
        page += 1
        background(c, w, h, page, "Informe de verificación")
        c.setFillColor(CYAN); c.setFont("ConsolasBold", 10); c.drawString(38, h-56, kicker.upper())
        c.setFillColor(WHITE); c.setFont("ConsolasBold", 23); c.drawString(38, h-91, title)
        return h - 122

    def block_title(y, title):
        c.setFillColor(RED); c.setFont("ConsolasBold", 12); c.drawString(42, y, title)
        return y - 17

    def body(y, text, width=510, size=10, color=WHITE, leading=None):
        return paragraph(c, text, 42, y, width, size=size, color=color, leading=leading)

    # Page 1: summary
    y = start("NEXUS | Informe de implementación", "Resumen de seis actividades")
    y = body(y, "Este informe registra el trabajo preparado en la rama <b>testing-debugging</b> y separa compilación comprobada de pruebas que requieren la computadora de demostración y SQL Server. La evaluación oral y las capturas personales deben completarse por el estudiante.", size=11)
    y -= 24
    y = block_title(y, "Estado de las actividades")
    rows = [
        ("1. Arquitectura", "Documentación y presentación de 5 diapositivas preparadas."),
        ("2. C# y control de versiones", "Regla RecuperarEnergia corregida; compilación 0 errores; prueba de botón pendiente."),
        ("3. Clonación y reconstrucción", "Guía y scripts listos; clonación limpia y ejecución real pendientes."),
        ("4. Integración SQL", "Repositorio y esquema implementados; conexión/guardado/carga deben probarse en SQL Server."),
        ("5. Release ejecutable", "Paquete Windows generado; prueba en otro equipo y publicación de GitHub Release pendientes."),
        ("6. Testing-debugging", "Rama local y matriz preparada; ejecuciones manuales y capturas pendientes."),
    ]
    for heading, desc in rows:
        c.setFillColor(PANEL); c.roundRect(40, y-43, w-80, 38, 6, fill=1, stroke=0)
        c.setFillColor(CYAN); c.setFont("ArialBold", 9); c.drawString(50, y-19, heading)
        paragraph(c, desc, 171, y-10, w-224, size=8.2, color=WHITE, leading=10)
        y -= 51
    y -= 8
    body(y, "La compilación local terminó con <b>0 errores y 60 advertencias</b> del SDK/proyecto. Se generó un paquete Windows que requiere .NET Desktop Runtime 11. No equivale a una prueba de funcionamiento de SQL Server.", size=9, color=GOLD)
    c.showPage()

    # Page 2: architecture
    y = start("1. Presentación de arquitectura", "Formularios y capas")
    y = body(y, "<b>Flujo:</b> Program.Main abre Form1 (menú). Form1 abre Form2, que comienza con el registro. El cadete inicia la cinemática Form3; desde la primera escena se abre Form4 para la secuencia de meteoritos. Al terminar la historia, Form2 muestra el mapa.", size=10.5)
    y -= 24
    boxes = [(50, "Form1", "Menú"), (180, "Form2", "Registro + mapa"), (340, "Form3", "Historia"), (480, "Form4", "Meteoritos")]
    for x, title, sub in boxes:
        c.setFillColor(PANEL2); c.roundRect(x, y-76, 120, 55, 8, fill=1, stroke=0)
        c.setFillColor(CYAN); c.setFont("ConsolasBold", 12); c.drawCentredString(x+60, y-42, title)
        c.setFillColor(WHITE); c.setFont("Arial", 8); c.drawCentredString(x+60, y-59, sub)
    y -= 112
    y = block_title(y, "Clases y lógica")
    y = body(y, "<b>Form2.cs</b> mantiene el ciclo del mapa: teclado, temporizadores, dibujo, colisiones, monstruos, energía, portales y estado de la misión. <b>EstadoPartida.cs</b> representa los datos que se guardan. <b>RepositorioPartidasSql.cs</b> abre una transacción para actualizar jugador y listas relacionadas. <b>ReglasPartida.cs</b> contiene la decisión de recuperación sin acoplarla a botones.", size=9.7)
    y -= 20
    y = block_title(y, "Acceso a datos")
    body(y, "Las tablas Partida, NucleoPartida, EnemigoPartida, ObjetoPartida y PortalPartida relacionan el estado del jugador con lo que existe en ambas realidades. Los scripts están en database/sql y la cadena se configura mediante NEXUS_SQL_CONNECTION.", size=9.7)
    c.showPage()

    # Page 3: method before-after
    y = start("2. Reto C# | RecuperarEnergia", "Propósito y mejora")
    y = body(y, "<b>Propósito:</b> recuperar una sola vez hasta 20 puntos de energía y 10 de conexión, sin sobrepasar 100. <b>Entradas:</b> energía, conexión y si el uso ya ocurrió. <b>Resultado:</b> si la operación se aplicó, si ya se usó y los valores finales.", size=10.5)
    y -= 22
    y = block_title(y, "Antes")
    c.setFillColor(PANEL); c.roundRect(40, y-104, w-80, 88, 7, fill=1, stroke=0)
    before = "if (!recuperacionUsada) {\n    recuperacionUsada = true;\n    energia = Math.Min(100, energia + 20);\n    conexion = Math.Min(100, conexion + 10);\n}"
    code_y = y-35
    for line in before.splitlines():
        c.setFillColor(RED); c.setFont("Consolas", 9); c.drawString(55, code_y, line); code_y -= 14
    y -= 130
    y = block_title(y, "Después")
    c.setFillColor(PANEL); c.roundRect(40, y-126, w-80, 110, 7, fill=1, stroke=0)
    after = "int e = Math.Min(100, energia + 20);\nint c = Math.Min(100, conexion + 10);\nif (e == energia && c == conexion)\n    return new(false, false, energia, conexion);\nreturn new(true, false, e, c);"
    code_y = y-34
    for line in after.splitlines():
        c.setFillColor(GREEN); c.setFont("Consolas", 8.4); c.drawString(55, code_y, line); code_y -= 14
    y -= 151
    body(y, "La interfaz consume el uso solo cuando Aplicada es true. La solución compiló, pero las capturas funcionales de los tres casos de recuperación deben tomarse en la ejecución personal.", size=9.5, color=GOLD)
    c.showPage()

    # Page 4: rebuild and SQL
    y = start("3 y 4. Reconstrucción e integración SQL", "Persistencia")
    y = body(y, "La base se crea desde scripts versionados, no desde un archivo local que dependa de la computadora original.", size=10)
    y -= 22
    y = block_title(y, "Comandos para SQL LocalDB")
    c.setFillColor(PANEL); c.roundRect(40, y-84, w-80, 68, 7, fill=1, stroke=0)
    commands = ['sqlcmd -S "(localdb)\\MSSQLLocalDB" -E -i "database\\sql\\01_crear_base.sql"',
                'sqlcmd -S "(localdb)\\MSSQLLocalDB" -E -i "database\\sql\\02_prueba_esquema.sql"']
    for i, line in enumerate(commands):
        c.setFillColor(CYAN); c.setFont("Consolas", 7.3); c.drawString(50, y-37-i*19, line)
    y -= 111
    y = block_title(y, "Estado persistido")
    y = body(y, "Al guardar desde el mapa, el repositorio captura el cadete, ubicación, vida/energía/conexión, núcleos, enemigos, objetos restantes y portales. Actualiza el slot 1 y sus tablas hijas dentro de una transacción. No permite guardar durante un encuentro.", size=9.5)
    y -= 20
    y = block_title(y, "Limitación de prueba")
    y = body(y, "SSMS mostró conexión activa a LocalDB, pero los scripts no se ejecutaron porque la ventana detectó interacción en curso. Por eso no se verificó una escritura/lectura real. En el equipo de demostración, crea la BD, guarda, cierra, carga y ejecuta database/sql/03_consultas_verificacion.sql. Adjunta capturas reales de las salidas.", size=9.5, color=GOLD)
    y -= 24
    y = block_title(y, "Clonación limpia")
    body(y, "Clonar en una carpeta nueva desde Visual Studio, abrir Nexus.V.02.slnx, restaurar NuGet, compilar, reconstruir el esquema con los scripts y ejecutar la partida sin copiar bin/obj de la carpeta anterior. El resultado de esa máquina debe añadirse a la evidencia.", size=9.5)
    c.showPage()

    # Page 5: tests and release
    y = start("5 y 6. Publicación y pruebas", "Próximas comprobaciones")
    y = block_title(y, "Estado de pruebas")
    checks = [
        ("Compilación", "0 errores", GREEN),
        ("Lógica de recuperación", "corregida; interfaz por probar", GOLD),
        ("Entradas inválidas", "captura manual pendiente", GOLD),
        ("Guardar/cargar SQL", "instancia de demostración pendiente", GOLD),
        ("Fallo de conexión", "prueba manual pendiente", GOLD),
    ]
    for label, result, col in checks:
        c.setFillColor(PANEL); c.roundRect(40, y-29, w-80, 24, 5, fill=1, stroke=0)
        c.setFillColor(WHITE); c.setFont("ArialBold", 9); c.drawString(50, y-21, label)
        c.setFillColor(col); c.setFont("Arial", 8.5); c.drawRightString(w-50, y-21, result)
        y -= 34
    y -= 8
    y = block_title(y, "Rama, release y evidencias")
    y = body(y, "La documentación de pruebas pertenece a testing-debugging. La rama debe enviarse a GitHub y las correcciones terminadas integrarse en feature/nexus-windows-form. El paquete ejecutable está generado; la Release v1.0.0 y la prueba de descarga todavía requieren publicación. El reporte de ActivityWatch se entrega por separado y cubre solamente el periodo en que el monitor estuvo activo.", size=9.5)
    y -= 24
    body(y, "Para completar: añade las capturas reales de la prueba de menú/mapa, movimiento, combate, recuperación, guardado/carga, SSMS, commit/push, clonación limpia y descarga de Release. La guía docs/EVIDENCIAS_PARA_ENVIAR.md da el orden y nombres recomendados.", size=9.5, color=GOLD)
    c.save()
    return path

if __name__ == "__main__":
    print(build_slides())
    print(build_report())
