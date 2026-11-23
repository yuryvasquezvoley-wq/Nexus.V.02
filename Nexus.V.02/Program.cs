
using System;
using System.Collections.Generic;
using System.Threading;
///como
namespace NEXUS
{
    class Explorador
    {
        private string nombre;
        private int edad;
        private int energia;
        private int conexion;
        private int vida;

        public string Nombre
        {
            get { return nombre; }
            private set { nombre = value; }
        }

        public int Edad
        {
            get { return edad; }
            private set { edad = value; }
        }

        public int Energia
        {
            get { return energia; }
            private set
            {
                if (value < 0)
                    energia = 0;
                else if (value > 100)
                    energia = 100;
                else
                    energia = value;
            }
        }

        public int Conexion
        {
            get { return conexion; }
            private set
            {
                if (value < 0)
                    conexion = 0;
                else if (value > 100)
                    conexion = 100;
                else
                    conexion = value;
            }
        }

        public int Vida
        {
            get { return vida; }
            private set
            {
                if (value < 0)
                    vida = 0;
                else if (value > 100)
                    vida = 100;
                else
                    vida = value;
            }
        }

        public Explorador(string nombre, int edad, int energia, int conexion)
        {
            Nombre = nombre;
            Edad = edad;
            Energia = energia;
            Conexion = conexion;
            Vida = 100;
        }

        public void RecibirDanio(int cantidad)
        {
            Vida -= cantidad;
        }

        public void RecuperarVida(int cantidad)
        {
            Vida += cantidad;
        }

        public bool GastarEnergia(int cantidad)
        {
            if (Energia >= cantidad)
            {
                Energia -= cantidad;
                Conexion -= 5;
                return true;
            }

            return false;
        }

        public void RecuperarEnergia(int cantidad)
        {
            Energia += cantidad;
            Conexion += 10;
        }

        public void MostrarDatos()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║          PERFIL DEL EXPLORADOR       ║");
            Console.WriteLine("╠══════════════════════════════════════╣");
            Console.WriteLine($"║ Nombre       : {Nombre,-20} ║");
            Console.WriteLine($"║ Edad         : {Edad,-20} ║");
            Console.WriteLine($"║ Vida         : {Vida,-20} ║");
            Console.WriteLine($"║ Energía      : {Energia,-20} ║");
            Console.WriteLine($"║ Conexión     : {Conexion,-20} ║");
            Console.WriteLine("╚══════════════════════════════════════╝");

            Console.ResetColor();
        }

        public void MostrarEnergia()
        {
            int bloques = Energia / 10;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ENERGÍA [");

            for (int i = 0; i < 10; i++)
            {
                if (i < bloques)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("█");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("░");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"] {Energia}/100");

            Console.ResetColor();
        }

        public void MostrarVida()
        {
            int bloques = Vida / 10;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("VIDA    [");

            for (int i = 0; i < 10; i++)
            {
                if (i < bloques)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("█");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("░");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"] {Vida}/100");

            Console.ResetColor();
        }

        public void MostrarConexion()
        {
            int bloques = Conexion / 10;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("CONEXIÓN[");

            for (int i = 0; i < 10; i++)
            {
                if (i < bloques)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("█");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("░");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"] {Conexion}/100");

            Console.ResetColor();
        }
    }



    class Objeto
    {
        private string nombre;
        private int cantidad;

        public string Nombre
        {
            get { return nombre; }
        }

        public int Cantidad
        {
            get { return cantidad; }
        }

        public Objeto(string nombre)
        {
            this.nombre = nombre;
            cantidad = 0;
        }

        public void Agregar()
        {
            cantidad++;
        }

        public bool Usar()
        {
            if (cantidad > 0)
            {
                cantidad--;
                return true;
            }

            return false;
        }
    }


    class Inventario
    {
        private List<Objeto> objetos;

        public Inventario()
        {
            objetos = new List<Objeto>();

            AgregarTipoObjeto("CELULA DE ENERGIA");
            AgregarTipoObjeto("RECURSO");
        }

        private void AgregarTipoObjeto(string nombre)
        {
            objetos.Add(new Objeto(nombre));
        }

        public void AgregarObjeto(string nombre)
        {
            foreach (Objeto objeto in objetos)
            {
                if (objeto.Nombre == nombre)
                {
                    objeto.Agregar();
                    return;
                }
            }

            Objeto nuevo = new Objeto(nombre);
            nuevo.Agregar();
            objetos.Add(nuevo);
        }

        public bool TieneObjeto(string nombre)
        {
            foreach (Objeto objeto in objetos)
            {
                if (objeto.Nombre == nombre &&
                    objeto.Cantidad > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool UsarObjeto(string nombre)
        {
            foreach (Objeto objeto in objetos)
            {
                if (objeto.Nombre == nombre)
                {
                    return objeto.Usar();
                }
            }

            return false;
        }

        public void MostrarInventario()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║             INVENTARIO               ║");
            Console.WriteLine("╠══════════════════════════════════════╣");

            foreach (Objeto objeto in objetos)
            {
                Console.WriteLine(
                    $"║ {objeto.Nombre,-24} x{objeto.Cantidad,-4}║"
                );
            }

            Console.WriteLine("╚══════════════════════════════════════╝");

            Console.ResetColor();
        }
    }



    class Bicho
    {
        private string nombre;
        private int vida;
        private int ataque;
        public bool EsZombie { get; private set; }

        public string Nombre
        {
            get { return nombre; }
        }

        public int Vida
        {
            get { return vida; }
        }

        public int Ataque
        {
            get { return ataque; }
        }

        public Bicho(bool esZombie = false)
        {
            EsZombie = esZombie;
            nombre = esZombie ? "ZOMBIE - REALIDAD 102" : "BICHO DIMENSIONAL";
            vida = 30;
            ataque = 10;
        }

        public void RecibirDanio(int cantidad)
        {
            vida -= cantidad;

            if (vida < 0)
                vida = 0;
        }

        public void Atacar(Explorador explorador)
        {
            explorador.RecibirDanio(ataque);
        }
    }


    class Realidad
    {
        private string nombre;
        private string[,] mapa;
        private int zombieX = 5;
        private int zombieY = 0;

        public string Nombre
        {
            get { return nombre; }
        }

        public Realidad(string nombre)
        {
            this.nombre = nombre;

            // MAPA 10 X 10
            mapa = new string[,]
            {
                { ".", ".", ".", ".", ".", ".", ".", ".", ".", "." },
                { ".", ".", "*", ".", ".", ".", ".", "+", ".", "." },
                { ".", "#", ".", ".", "*", ".", ".", ".", ".", "." },
                { ".", ".", ".", "+", ".", ".", "#", ".", ".", "." },
                { ".", "*", ".", ".", ".", ".", ".", ".", "+", "." },
                { ".", ".", "#", ".", ".", "R", ".", ".", ".", "." },
                { ".", "+", ".", ".", "#", ".", ".", "*", ".", "." },
                { ".", ".", ".", "*", ".", ".", ".", ".", "#", "." },
                { ".", "#", ".", ".", "+", ".", "*", ".", ".", "." },
                { ".", ".", ".", ".", ".", ".", ".", ".", "O", "." }
            };

            // Decoración adicional exclusiva de la Realidad 102.
            // La Realidad 999 conserva exactamente su mapa anterior.
            if (nombre == "REALIDAD 102")
            {
                int[,] arboles = { { 1, 1 }, { 4, 0 }, { 8, 1 }, { 2, 4 }, { 7, 5 }, { 1, 8 } };
                int[,] casas = { { 3, 1 }, { 7, 2 }, { 0, 6 }, { 8, 7 } };

                for (int i = 0; i < arboles.GetLength(0); i++)
                    if (mapa[arboles[i, 0], arboles[i, 1]] == ".")
                        mapa[arboles[i, 0], arboles[i, 1]] = "T";

                for (int i = 0; i < casas.GetLength(0); i++)
                    if (mapa[casas[i, 0], casas[i, 1]] == ".")
                        mapa[casas[i, 0], casas[i, 1]] = "H";

                for (int x = 0; x < 10; x++)
                {
                    if (mapa[3, x] == ".") mapa[3, x] = "=";
                    if (mapa[7, x] == ".") mapa[7, x] = "=";
                }
            }

            // Coloca el portal de extracción en una casilla libre aleatoria.
            // Nunca aparece debajo del cadete ni sobre otro elemento del mapa.
            int portalX;
            int portalY;

            do
            {
                portalX = Random.Shared.Next(0, 10);
                portalY = Random.Shared.Next(0, 10);
            }
            while ((portalX == 0 && portalY == 0) ||
                   mapa[portalY, portalX] != "." ||
                   (nombre == "REALIDAD 102" &&
                    portalX == zombieX && portalY == zombieY));

            mapa[portalY, portalX] = "E";
        }

        public string ObtenerElemento(int x, int y)
        {
            return mapa[y, x];
        }

        public void CambiarElemento(int x, int y, string elemento)
        {
            mapa[y, x] = elemento;
        }

        public void MoverZombie(int jugadorX, int jugadorY)
        {
            if (Nombre != "REALIDAD 102")
                return;

            int[,] direcciones = { { 0, -1 }, { 0, 1 }, { -1, 0 }, { 1, 0 } };
            int inicio = Random.Shared.Next(0, 4);

            for (int i = 0; i < 4; i++)
            {
                int direccion = (inicio + i) % 4;
                int nuevoX = zombieX + direcciones[direccion, 0];
                int nuevoY = zombieY + direcciones[direccion, 1];

                if (nuevoX < 0 || nuevoX >= 10 || nuevoY < 0 || nuevoY >= 10)
                    continue;

                if (nuevoX == jugadorX && nuevoY == jugadorY)
                    continue;

                string terreno = mapa[nuevoY, nuevoX];
                if (terreno == "." || terreno == "T" || terreno == "H" || terreno == "=")
                {
                    zombieX = nuevoX;
                    zombieY = nuevoY;
                    return;
                }
            }
        }

        public void MostrarMapa(int jugadorX, int jugadorY)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine($"║              {Nombre,-30}║");
            Console.WriteLine("╠════════════════════════════════════════════════╣");

            for (int fila = 0; fila < 10; fila++)
            {
                Console.Write("║ ");

                for (int columna = 0; columna < 10; columna++)
                {
                    if (columna == jugadorX &&
                        fila == jugadorY)
                    {
                        Console.ForegroundColor =
                            ConsoleColor.Green;

                        Console.Write("@   ");

                        continue;
                    }

                    if (Nombre == "REALIDAD 102" &&
                        fila == zombieY && columna == zombieX)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("[Z] ");
                        continue;
                    }

                    string elemento =
                        mapa[fila, columna];

                    if (elemento == "T")
                        Console.ForegroundColor = ConsoleColor.Green;

                    else if (elemento == "H")
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    else if (elemento == "=")
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    else if (elemento == "*")
                        Console.ForegroundColor =
                            ConsoleColor.White;

                    else if (elemento == "+")
                        Console.ForegroundColor =
                            ConsoleColor.Red;

                    else if (elemento == "#")
                        Console.ForegroundColor =
                            ConsoleColor.DarkRed;

                    else if (elemento == "O")
                        Console.ForegroundColor =
                            ConsoleColor.Yellow;

                    else if (elemento == "E")
                        Console.ForegroundColor =
                            ConsoleColor.Green;

                    else if (elemento == "R")
                        Console.ForegroundColor =
                            ConsoleColor.Cyan;

                    else
                        Console.ForegroundColor =
                            ConsoleColor.DarkGray;

                    string baldosa = elemento switch
                    {
                        "." => ((fila * 7 + columna * 3) % 5) switch
                        {
                            0 => "::  ",
                            1 => "''  ",
                            2 => "..  ",
                            _ => "··  "
                        },
                        "E" => "[E] ",
                        "*" => "[+] ",
                        "+" => "[!] ",
                        "#" => "[X] ",
                        "O" => "[O] ",
                        "R" => "[R] ",
                        "T" => "♣♣  ",
                        "H" => "⌂   ",
                        "=" => "════",
                        _ => "??  "
                    };
                    Console.Write(baldosa);
                }

                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine("║");
            }

            Console.WriteLine("╚════════════════════════════════════════════════╝");

            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("@ Cadete   [E] Extracción   [+] Recurso   [!] Anomalía");
            Console.WriteLine("[X] Peligro   [R] Anomalía   [O] Núcleo   ♣♣ Árbol   ⌂ Casa   ════ Calle");
            if (Nombre == "REALIDAD 102")
                Console.WriteLine("[Z] Zombie en movimiento");
        }
    }



    class NexusGame
    {
        private Explorador explorador;
        private Realidad[] realidades;
        private Inventario inventario;

        private bool juegoActivo;
        private bool misionActiva;
        private bool nucleoEncontrado;

        private int posicionX;
        private int posicionY;

        private Realidad realidadActual;

        public NexusGame(Explorador explorador)
        {
            this.explorador = explorador;

            realidades = new Realidad[]
            {
                new Realidad("REALIDAD 102"),
                new Realidad("REALIDAD 999")
            };

            inventario = new Inventario();

            juegoActivo = true;
            misionActiva = false;
            nucleoEncontrado = false;

            posicionX = 0;
            posicionY = 0;
        }



        public void Iniciar()
        {
            CargarNexus();

            while (juegoActivo)
            {
                RestaurarPantalla();

                MostrarHUD();
                MostrarMenu();

                string opcion =
                    Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        ExplorarRealidad();
                        break;

                    case "2":
                        AnalizarAnomalia();
                        break;

                    case "3":
                        ConsultarEstado();
                        break;

                    case "4":
                        RecuperarEnergia();
                        break;

                    case "5":
                        MostrarPerfil();
                        break;

                    case "6":
                        inventario.MostrarInventario();
                        Pausa();
                        break;

                    case "0":
                        Desconectar();
                        break;

                    default:
                        MensajeError(
                            "OPCIÓN NO VÁLIDA."
                        );
                        break;
                }
            }
        }


        private void MostrarHUD()
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║              N E X U S                 ║"
            );

            Console.WriteLine(
                "║          SYSTEM V2.0 ONLINE            ║"
            );

            Console.WriteLine(
                "╠════════════════════════════════════════╣"
            );

            Console.WriteLine(
                $"║ EXPLORADOR: {explorador.Nombre,-25}║"
            );

            explorador.MostrarVida();
            explorador.MostrarEnergia();
            explorador.MostrarConexion();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();
        }



        private void MostrarMenu()
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║             NEXUS SYSTEM              ║"
            );

            Console.WriteLine(
                "╠════════════════════════════════════════╣"
            );

            Console.WriteLine(
                "║ [1] > EXPLORAR REALIDAD                ║"
            );

            Console.WriteLine(
                "║ [2] > ANALIZAR ANOMALÍA                ║"
            );

            Console.WriteLine(
                "║ [3] > CONSULTAR ESTADO                 ║"
            );

            Console.WriteLine(
                "║ [4] > RECUPERAR ENERGÍA                ║"
            );

            Console.WriteLine(
                "║ [5] > VER PERFIL                       ║"
            );

            Console.WriteLine(
                "║ [6] > INVENTARIO                       ║"
            );

            Console.WriteLine(
                "║ [0] > DESCONECTAR                      ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.Write(
                ">> SELECCIONE UNA OPCIÓN: "
            );
        }



        private void ExplorarRealidad()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║           SELECCIÓN DE REALIDAD        ║"
            );

            Console.WriteLine(
                "╠════════════════════════════════════════╣"
            );

            Console.WriteLine(
                "║ [1] REALIDAD 102                       ║"
            );

            Console.WriteLine(
                "║ [2] REALIDAD 999                       ║"
            );

            Console.WriteLine(
                "║ [0] REGRESAR                           ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.Write(
                ">> OPCIÓN: "
            );

            string opcion =
                Console.ReadLine();

            if (opcion == "0")
                return;

            int indice;

            if (opcion == "1")
                indice = 0;

            else if (opcion == "2")
                indice = 1;

            else
            {
                MensajeError(
                    "REALIDAD NO VÁLIDA."
                );

                return;
            }

            if (!explorador.GastarEnergia(30))
            {
                MensajeError(
                    "ENERGÍA INSUFICIENTE PARA VIAJAR."
                );

                return;
            }

            realidadActual =
                realidades[indice];

            posicionX = 0;
            posicionY = 0;

            nucleoEncontrado = false;
            misionActiva = true;

            MostrarNuevaMision();

            Pausa();

            IniciarExploracion();
        }


        private void MostrarNuevaMision()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║             NUEVA MISIÓN               ║"
            );

            Console.WriteLine(
                "╠════════════════════════════════════════╣"
            );

            Console.WriteLine(
                $"║ REALIDAD: {realidadActual.Nombre,-25}║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine();

            Console.WriteLine(
                ">> AL CADETE SE LE HA ASIGNADO UNA NUEVA MISIÓN."
            );

            Console.WriteLine();

            Console.WriteLine(
                "OBJETIVOS:"
            );

            Console.WriteLine(
                "1. Explorar la realidad."
            );

            Console.WriteLine(
                "2. Recolectar recursos."
            );

            Console.WriteLine(
                "3. Administrar el inventario."
            );

            Console.WriteLine(
                "4. Enfrentarse a las entidades."
            );

            Console.WriteLine(
                "5. Llegar a la zona de la anomalía."
            );

            Console.WriteLine(
                "6. Encontrar el núcleo energético."
            );

            Console.WriteLine(
                "7. Registrar el núcleo energético."
            );

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine(
                ">> MISIÓN: LA ANOMALÍA PERDIDA"
            );

            Console.ResetColor();
        }



        private void IniciarExploracion()
        {
            bool explorando = true;
            Console.Clear();

            while (explorando &&
                   misionActiva &&
                   explorador.Vida > 0)
            {
                Console.SetCursorPosition(0, 0);

                MostrarPanelExploracion();

                Console.WriteLine();

                Console.ForegroundColor =
                    ConsoleColor.White;

                Console.WriteLine(
                    "W = ARRIBA"
                );

                Console.WriteLine(
                    "S = ABAJO"
                );

                Console.WriteLine(
                    "A = IZQUIERDA"
                );

                Console.WriteLine(
                    "D = DERECHA"
                );

                Console.WriteLine(
                    "ESC = SALIR DE LA EXPLORACIÓN"
                );

                Console.WriteLine();

                Console.Write(
                    ">> MUEVE AL CADETE: "
                );

                ConsoleKeyInfo tecla =
                    Console.ReadKey(true);

                if (tecla.Key ==
                    ConsoleKey.Escape)
                {
                    explorando = false;
                    continue;
                }

                // Con energía en cero, el cadete queda sin fuerzas para moverse.
                // Puede salir con ESC y recargar desde el menú principal.
                if (explorador.Energia <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine(">> ENERGÍA AGOTADA. NO PUEDES CORRER NI DESPLAZARTE.");
                    Console.WriteLine(">> PRESIONA ESC PARA SALIR Y RECARGAR EN EL MENÚ.");
                    Thread.Sleep(900);
                    continue;
                }

                int nuevoX =
                    posicionX;

                int nuevoY =
                    posicionY;

                if (tecla.Key ==
                    ConsoleKey.W)
                {
                    nuevoY--;
                }

                else if (tecla.Key ==
                         ConsoleKey.S)
                {
                    nuevoY++;
                }

                else if (tecla.Key ==
                         ConsoleKey.A)
                {
                    nuevoX--;
                }

                else if (tecla.Key ==
                         ConsoleKey.D)
                {
                    nuevoX++;
                }

                else
                {
                    continue;
                }

                if (nuevoX < 0 ||
                    nuevoX > 9 ||
                    nuevoY < 0 ||
                    nuevoY > 9)
                {
                    Console.ForegroundColor =
                        ConsoleColor.Red;

                    Console.WriteLine();

                    Console.WriteLine(
                        ">> NO PUEDES SALIR DEL MAPA."
                    );

                    Thread.Sleep(700);

                    continue;
                }

                if (!explorador.GastarEnergia(2))
                {
                    Console.ForegroundColor =
                        ConsoleColor.Red;

                    Console.WriteLine();

                    Console.WriteLine(
                        ">> ENERGÍA INSUFICIENTE."
                    );

                    Thread.Sleep(1000);

                    continue;
                }

                posicionX = nuevoX;
                posicionY = nuevoY;

                ProcesarCasilla();

                // El zombie se desplaza únicamente en la Realidad 102.
                realidadActual.MoverZombie(posicionX, posicionY);

                if (explorador.Vida <= 0)
                {
                    Derrota();
                    return;
                }

                if (!misionActiva)
                {
                    return;
                }
            }
        }



        private void MostrarPanelExploracion()
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════════════════════╗"
            );

            Console.WriteLine(
                $"║ CADETE: {explorador.Nombre,-20} | VIDA: {explorador.Vida,3}/100 ║"
            );

            Console.WriteLine(
                $"║ ENERGÍA: {explorador.Energia,3}/100 | POSICIÓN: ({posicionX},{posicionY})      ║"
            );

            Console.WriteLine(
                "╠════════════════════════════════════════════════════════╣"
            );

            Console.WriteLine(
                "║                    MAPA NEXUS 10 X 10                 ║"
            );

            Console.WriteLine(
                "╠════════════════════════════════════════════════════════╣"
            );

            realidadActual.MostrarMapa(
                posicionX,
                posicionY
            );

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine();

            inventario.MostrarInventario();

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine();

            Console.WriteLine(
                "MISIÓN ACTUAL:"
            );

            if (!nucleoEncontrado)
            {
                Console.WriteLine(
                    ">> ENCUENTRA EL NÚCLEO ENERGÉTICO."
                );
            }
            else
            {
                Console.WriteLine(
                    ">> NÚCLEO ENCONTRADO. REGÍSTRALO."
                );
            }

            Console.ResetColor();
        }


        private void ProcesarCasilla()
        {
            string elemento =
                realidadActual.ObtenerElemento(
                    posicionX,
                    posicionY
                );

            if (elemento == ".")
            {
                return;
            }

            if (elemento == "*")
            {
                RecolectarRecurso();
                return;
            }

            if (elemento == "+")
            {
                EncontrarBicho();
                return;
            }

            if (elemento == "#")
            {
                EncontrarBicho();
                return;
            }

            if (elemento == "R")
            {
                LlegarZonaAnomalia();
                return;
            }

            if (elemento == "O")
            {
                EncontrarNucleo();
                return;
            }
        }


        private void RecolectarRecurso()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║           RECURSO ENCONTRADO            ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> HAS ENCONTRADO UNA CÉLULA DE ENERGÍA."
            );

            inventario.AgregarObjeto(
                "CELULA DE ENERGIA"
            );

            realidadActual.CambiarElemento(
                posicionX,
                posicionY,
                "."
            );

            Console.WriteLine();

            inventario.MostrarInventario();

            Pausa();
        }


        private void EncontrarBicho()
        {
            bool esZombie = realidadActual.Nombre == "REALIDAD 102";
            Bicho bicho = new Bicho(esZombie);
            CombateBicho(bicho);
        }


        private void CombateBicho(Bicho bicho)
        {
            while (bicho.Vida > 0 &&
                   explorador.Vida > 0)
            {
                var decision = NexusWindowsForm.BattleDialog.Show(
                    explorador.Vida,
                    bicho.Vida,
                    bicho.EsZombie
                );
                if (decision == System.Windows.Forms.DialogResult.Abort)
                {
                    Console.WriteLine(">> HAS ESCAPADO DEL BICHO.");
                    return;
                }

                // Cadete ataca
                bicho.RecibirDanio(20);

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine();

                Console.WriteLine(
                    ">> EL CADETE ATACA."
                );

                Console.WriteLine(
                    ">> DAÑO REALIZADO: 20"
                );

                if (bicho.Vida <= 0)
                {
                    Console.WriteLine();

                    Console.WriteLine(
                        "✓ BICHO ELIMINADO."
                    );

                    Console.WriteLine(
                        ">> EL CAMINO HA QUEDADO LIBRE."
                    );

                    Console.ResetColor();

                    Pausa();

                    realidadActual.CambiarElemento(
                        posicionX,
                        posicionY,
                        "."
                    );

                    return;
                }

                // Bicho ataca
                bicho.Atacar(explorador);

                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine();

                Console.WriteLine(
                    ">> EL BICHO ATACA."
                );

                Console.WriteLine(
                    ">> DAÑO RECIBIDO: 10"
                );

                Console.WriteLine(
                    $">> VIDA ACTUAL: {explorador.Vida}/100"
                );

                Console.ResetColor();

                Thread.Sleep(800);
            }

            if (explorador.Vida <= 0)
            {
                Derrota();
            }
        }



        private void LlegarZonaAnomalia()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║          ZONA DE ANOMALÍA              ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.WriteLine(
                "⚡ HAS LLEGADO A LA ZONA DE LA ANOMALÍA."
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> LOS SENSORES DETECTAN UNA GRAN"
            );

            Console.WriteLine(
                ">> FUENTE DE ENERGÍA MÁS ADELANTE."
            );

            Console.ResetColor();

            Pausa();
        }



        private void EncontrarNucleo()
        {
            if (nucleoEncontrado)
                return;

            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║        NÚCLEO ENERGÉTICO ENCONTRADO    ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> EL CADETE HA ENCONTRADO EL NÚCLEO."
            );

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine(
                "¿QUIERES REGISTRAR EL NÚCLEO?"
            );

            Console.WriteLine();

            Console.WriteLine(
                "[1] SÍ"
            );

            Console.WriteLine(
                "[2] NO"
            );

            Console.WriteLine();

            Console.Write(
                ">> OPCIÓN: "
            );

            ConsoleKeyInfo tecla =
                Console.ReadKey(true);

            if (tecla.Key ==
                ConsoleKey.D1)
            {
                RegistrarNucleo();
            }
            else if (tecla.Key ==
                     ConsoleKey.D2)
            {
                Console.WriteLine();

                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine(
                    ">> EL NÚCLEO NO HA SIDO REGISTRADO."
                );

                Console.WriteLine(
                    ">> CONTINÚA LA EXPLORACIÓN."
                );

                Console.ResetColor();

                Pausa();
            }
        }



        private void RegistrarNucleo()
        {
            nucleoEncontrado = true;

            // El portal conserva su ubicación aleatoria; no se mueve
            // a la casilla del núcleo ni queda oculto bajo el cadete.
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║          NÚCLEO REGISTRADO             ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.WriteLine(
                "✓ EL NÚCLEO ENERGÉTICO HA SIDO REGISTRADO."
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> INFORMACIÓN ENVIADA A LA NAVE."
            );

            Console.WriteLine(
                ">> ANOMALÍA IDENTIFICADA."
            );

            Console.WriteLine();

            Thread.Sleep(1500);

            MisionCompletada();
        }



        private void MisionCompletada()
        {
            misionActiva = false;

            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║           MISIÓN COMPLETADA            ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine(
                ">> EL CADETE HA CUMPLIDO TODOS LOS OBJETIVOS."
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> NÚCLEO ENERGÉTICO REGISTRADO."
            );

            Console.WriteLine(
                ">> ANOMALÍA IDENTIFICADA."
            );

            Console.WriteLine(
                ">> DATOS ENVIADOS A LA NAVE."
            );

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine(
                "=========================================="
            );

            Console.WriteLine(
                "Y ASÍ ES COMO EL CADETE LOGRÓ"
            );

            Console.WriteLine(
                "DETENER LA ANOMALÍA."
            );

            Console.WriteLine();

            Console.WriteLine(
                "EL CADETE REGRESÓ A LA NAVE"
            );

            Console.WriteLine(
                "CON EL NÚCLEO ENERGÉTICO REGISTRADO."
            );

            Console.WriteLine(
                "=========================================="
            );

            Console.ResetColor();

            Pausa();
        }


        private void AnalizarAnomalia()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║          ANÁLISIS DE ANOMALÍA          ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            if (!explorador.GastarEnergia(20))
            {
                MensajeError(
                    "ENERGÍA INSUFICIENTE."
                );

                return;
            }

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine(
                ">> ESCANEANDO ESPACIO DIMENSIONAL..."
            );

            Console.ForegroundColor =
                ConsoleColor.Red;

            for (int i = 0; i < 20; i++)
            {
                Console.Write("█");
                Thread.Sleep(50);
            }

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Yellow;

            Console.WriteLine(
                "⚡ ANOMALÍA DIMENSIONAL DETECTADA."
            );

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine(
                ">> COSTO DEL ANÁLISIS: 20 ENERGÍA"
            );

            Pausa();
        }


        private void ConsultarEstado()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║              ESTADO NEXUS              ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            explorador.MostrarVida();
            explorador.MostrarEnergia();
            explorador.MostrarConexion();

            Console.WriteLine();

            if (explorador.Energia <= 20)
            {
                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.WriteLine(
                    "⚠ NIVEL DE ENERGÍA CRÍTICO."
                );
            }
            else if (explorador.Energia <= 50)
            {
                Console.ForegroundColor =
                    ConsoleColor.Yellow;

                Console.WriteLine(
                    "⚠ NIVEL DE ENERGÍA MEDIO."
                );
            }
            else
            {
                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine(
                    "✓ NIVEL DE ENERGÍA ESTABLE."
                );
            }

            Pausa();
        }


        private void RecuperarEnergia()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║         RECUPERACIÓN DE ENERGÍA        ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine(
                ">> CONECTANDO AL NÚCLEO DE RECUPERACIÓN..."
            );

            Console.ForegroundColor =
                ConsoleColor.Red;

            for (int i = 0; i < 20; i++)
            {
                Console.Write("█");
                Thread.Sleep(50);
            }

            Console.WriteLine();

            explorador.RecuperarEnergia(20);

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "✓ SE RECUPERARON 20 PUNTOS DE ENERGÍA."
            );

            explorador.MostrarEnergia();

            Pausa();
        }


        private void MostrarPerfil()
        {
            Console.Clear();

            explorador.MostrarDatos();

            Pausa();
        }



        private void Derrota()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║             MISIÓN FALLIDA             ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> EL CADETE HA PERDIDO TODA SU VIDA."
            );

            Console.WriteLine(
                ">> LA MISIÓN HA FALLADO."
            );

            Console.ResetColor();

            misionActiva = false;

            Pausa();
        }


        private void Desconectar()
        {
            Console.Clear();

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║          DESCONEXIÓN DE NEXUS          ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine();

            Console.WriteLine(
                ">> CERRANDO CONEXIÓN..."
            );

            for (int i = 0; i < 20; i++)
            {
                Console.Write("█");
                Thread.Sleep(50);
            }

            juegoActivo = false;

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "✓ CONEXIÓN FINALIZADA CORRECTAMENTE."
            );

            Thread.Sleep(1000);

            Console.ResetColor();
        }


        private void CargarNexus()
        {
            Console.Clear();

            Console.BackgroundColor =
                ConsoleColor.Black;

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                "███╗   ██╗███████╗██╗  ██╗"
            );

            Console.WriteLine(
                "████╗  ██║██╔════╝╚██╗██╔╝"
            );

            Console.WriteLine(
                "██╔██╗ ██║█████╗   ╚███╔╝ "
            );

            Console.WriteLine(
                "██║╚██╗██║██╔══╝   ██╔██╗ "
            );

            Console.WriteLine(
                "██║ ╚████║███████╗██╔╝ ██╗"
            );

            Console.WriteLine(
                "╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝"
            );

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine(
                "INITIALIZING NEXUS SYSTEM..."
            );

            Console.ForegroundColor =
                ConsoleColor.Red;

            for (int i = 0; i < 25; i++)
            {
                Console.Write("█");
                Thread.Sleep(40);
            }

            Console.WriteLine();

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "✓ NEXUS V2.0 ONLINE."
            );

            Thread.Sleep(1000);
        }

        private void MensajeError(string mensaje)
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine();

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║                 ALERTA                 ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.WriteLine(
                ">> " + mensaje
            );

            Pausa();
        }


        private void Pausa()
        {
            Console.ForegroundColor =
                ConsoleColor.White;

            Console.WriteLine();

            Console.Write(
                ">> PRESIONE ENTER PARA CONTINUAR..."
            );

            Console.ReadLine();

            Console.BackgroundColor =
                ConsoleColor.Black;

            Console.ForegroundColor =
                ConsoleColor.Red;
        }



        private void RestaurarPantalla()
        {
            Console.BackgroundColor =
                ConsoleColor.Black;

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.Clear();
        }
    }


    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            Console.OutputEncoding =
                System.Text.Encoding.UTF8;

            Console.Title =
                "NEXUS V2.0 - MATRIX SYSTEM";

            Console.BackgroundColor =
                ConsoleColor.Black;

            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.Clear();

            Console.WriteLine(
                "╔════════════════════════════════════════╗"
            );

            Console.WriteLine(
                "║       REGISTRO DE EXPLORADOR NEXUS     ║"
            );

            Console.WriteLine(
                "╚════════════════════════════════════════╝"
            );

            Console.ForegroundColor =
                ConsoleColor.White;

            Console.Write(
                ">> NOMBRE: "
            );

            string nombre =
                Console.ReadLine();

            Console.Write(
                ">> EDAD: "
            );

            int edad =
                LeerEntero();

            int energia;

            do
            {
                Console.Write(
                    ">> ENERGÍA INICIAL (1 - 100): "
                );

                energia =
                    LeerEntero();

                if (energia < 1 ||
                    energia > 100)
                {
                    Console.ForegroundColor =
                        ConsoleColor.Red;

                    Console.WriteLine(
                        "⚠ LA ENERGÍA DEBE ESTAR ENTRE 1 Y 100."
                    );

                    Console.ForegroundColor =
                        ConsoleColor.White;
                }

            } while (
                energia < 1 ||
                energia > 100
            );


            int conexion;

            do
            {
                Console.Write(
                    ">> NIVEL DE CONEXIÓN (1 - 100): "
                );

                conexion =
                    LeerEntero();

                if (conexion < 1 ||
                    conexion > 100)
                {
                    Console.ForegroundColor =
                        ConsoleColor.Red;

                    Console.WriteLine(
                        "⚠ LA CONEXIÓN DEBE ESTAR ENTRE 1 Y 100."
                    );

                    Console.ForegroundColor =
                        ConsoleColor.White;
                }

            } while (
                conexion < 1 ||
                conexion > 100
            );


            Explorador explorador =
                new Explorador(
                    nombre,
                    edad,
                    energia,
                    conexion
                );


            NexusGame juego =
                new NexusGame(explorador);

            juego.Iniciar();
        }

        static int LeerEntero()
        {
            int numero;

            while (!int.TryParse(
                Console.ReadLine(),
                out numero))
            {
                Console.ForegroundColor =
                    ConsoleColor.Red;

                Console.Write(
                    ">> INGRESE UN NÚMERO VÁLIDO: "
                );

                Console.ForegroundColor =
                    ConsoleColor.White;
            }

            return numero;
        }
    }
}


