using System;
using System.Threading;

namespace NexusTrainingSystem
{
    // ==========================================
    // ENTIDAD: EXPLORADOR (POO Encapsulado)
    // ==========================================
    public class Explorador
    {
        // Propiedades Automáticas
        public string Nombre { get; private set; }
        public string Realidad { get; private set; }
        public int Edad { get; private set; }

        // Propiedades con Encapsulamiento y Control de Rangos (0 - 100)
        private int _vida;
        public int Vida
        {
            get => _vida;
            private set => _vida = Math.Min(Math.Max(value, 0), 100);
        }

        private int _energia;
        public int Energia
        {
            get => _energia;
            private set => _energia = Math.Min(Math.Max(value, 0), 100);
        }

        private int _estabilidad;
        public int Estabilidad
        {
            get => _estabilidad;
            private set => _estabilidad = Math.Min(Math.Max(value, 0), 100);
        }

        private int _peligro;
        public int Peligro
        {
            get => _peligro;
            private set => _peligro = Math.Min(Math.Max(value, 0), 100);
        }

        // Constructor
        public Explorador(string nombre, string realidad, int edad, int energiaInicial, int estabilidadInicial)
        {
            Nombre = nombre;
            Realidad = realidad;
            Edad = edad;
            Vida = 100;
            Energia = energiaInicial;
            Estabilidad = estabilidadInicial;
            Peligro = 10;
        }

        // Regla de Negocio: Validación de Inmersión Biométrica
        public bool EsAptoParaInmersion()
        {
            return Edad >= 18 && Energia >= 30 && Estabilidad >= 30;
        }

        // Métodos de Comportamiento / Lógica del Dominio
        public void RecibirDano(int cantidad)
        {
            Vida -= cantidad;
        }

        public void ConsumirEnergia(int cantidad)
        {
            Energia -= cantidad;
        }

        public void RecargarEnergia(int cantidad)
        {
            Energia += cantidad;
        }

        public void AjustarEstabilidad(int cantidad)
        {
            Estabilidad += cantidad;
        }

        public void AjustarPeligro(int cantidad)
        {
            Peligro += cantidad;
        }

        public bool EstaVivo() => Vida > 0;

        public bool EstadoCritico()
        {
            return Energia <= 15 && Estabilidad <= 15 && Peligro >= 70;
        }
    }

    // ==========================================
    // ENTIDAD: MAPA (Gestión de Cartografía)
    // ==========================================
    public class Mapa
    {
        public string NombreRealidad { get; private set; }
        private readonly int[,] _matriz;

        public Mapa(string nombreRealidad, int[,] matriz)
        {
            NombreRealidad = nombreRealidad;
            _matriz = matriz;
        }

        public void RenderizarAnimado()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ NEXUS // RADAR DE ESCANEO CARTOGRÁFICO: {TruncarOAjustar(NombreRealidad, 20),-20} ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.ResetColor();

            int filas = _matriz.GetLength(0);
            int columnas = _matriz.GetLength(1);
            int totalCeldas = filas * columnas;
            int celdasExploradas = 0;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║                                                              ║");

            for (int i = 0; i < filas; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("║                  ");

                for (int j = 0; j < columnas; j++)
                {
                    int celda = _matriz[i, j];
                    if (celda != 0) celdasExploradas++;

                    Thread.Sleep(30);

                    switch (celda)
                    {
                        case 1:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(" [B] ");
                            Console.Beep(1000, 40);
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" ($) ");
                            Console.Beep(1200, 40);
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write(" (!) ");
                            Console.Beep(400, 60);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write("  .  ");
                            Console.Beep(600, 15);
                            break;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("                     ║");
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("║                                                              ║");

            double cobertura = ((double)celdasExploradas / totalCeldas) * 100;

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║ COBERTURA DE CARTOGRAFÍA DETECTADA: {cobertura,5:F1}%                  ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\nSIMBOLOGÍA DE CAMPO: [B] Base  |  ($) Recurso  |  (!) Anomalía  |  . Zona Vacía");
            Console.ResetColor();
        }

        private static string TruncarOAjustar(string texto, int maxLongitud)
        {
            if (string.IsNullOrEmpty(texto)) return new string(' ', maxLongitud);
            if (texto.Length > maxLongitud) return texto.Substring(0, maxLongitud);
            return texto;
        }
    }

    // ==========================================
    // CONTROLADOR: SISTEMA NEXUS
    // ==========================================
    public class SistemaNexus
    {
        private Explorador _exploradorActual;
        private readonly Random _rng = new Random();

        private readonly Mapa _mapaIris07 = new Mapa("IRIS-07", new int[5, 5] {
            { 0, 0, 0, 3, 0 },
            { 0, 1, 0, 0, 0 },
            { 0, 0, 2, 0, 0 },
            { 3, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 2 }
        });

        private readonly Mapa _mapaIris12 = new Mapa("IRIS-12", new int[5, 5] {
            { 3, 3, 0, 0, 2 },
            { 0, 1, 0, 3, 0 },
            { 2, 0, 0, 0, 0 },
            { 0, 0, 3, 0, 2 },
            { 0, 2, 0, 0, 3 }
        });

        public void Iniciar()
        {
            MostrarPantallaDeCarga();

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      SISTEMA DE INMERSIÓN NEXUS // REGISTRO DE CADETE v1.0   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            EscribirLento("[ALERTA]: Identidad no detectada en la red local. Iniciando protocolo...");
            Console.ResetColor();
            Console.WriteLine();

            // Captura de Datos
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" IDENTIFICADOR BIOMÉTRICO (Nombre completo/Alias): ");
            Console.ForegroundColor = ConsoleColor.White;
            string nombre = Console.ReadLine();
            SimularEscaneoRegistro("Sujeto", nombre);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" DESTINO DE DESPLIEGUE (Sector/Realidad Asignada): ");
            Console.ForegroundColor = ConsoleColor.White;
            string realidad = Console.ReadLine();
            SimularEscaneoRegistro("Coordenadas Vectoriales", realidad);

            int edad = LeerEnteroValido(" CICLO VITAL COMPLETADO (Edad cronológica): ");
            SimularEscaneoRegistro("Madurez biológica", edad.ToString());

            int energia = LeerEnteroValido(" NIVEL DE ENERGÍA NEURAL (0 - 100%): ");
            SimularEscaneoRegistro("Carga Celular", energia + "%");

            int estabilidad = LeerEnteroValido(" ÍNDICE DE ANCLAJE PSÍQUICO / ESTABILIDAD (0 - 100%): ");
            SimularEscaneoRegistro("Sincronización Mental", estabilidad + "%");

            // Instanciación de la Entidad Explorador
            _exploradorActual = new Explorador(nombre, realidad, edad, energia, estabilidad);

            Console.WriteLine("\n--------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Cyan;
            EscribirLento("Sincronizando constantes biológicas con la matriz NEXUS...", 20);
            Thread.Sleep(400);

            if (!_exploradorActual.EsAptoParaInmersion())
            {
                Console.Beep(400, 300);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[INMERSIÓN RECHAZADA]: PARÁMETROS FUERA DE RANGO SEGURO.");
                Console.WriteLine(" -> Fallo crítico: El cadete presenta incoherencias en la firma vital.");
                Console.WriteLine(" Requisitos: Edad >= 18 | Energía >= 30% | Estabilidad >= 30%");
                Console.ResetColor();
                Console.WriteLine("\nPresione cualquier tecla para salir...");
                Console.ReadKey(true);
                return;
            }

            Console.Beep(1200, 150);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[INMERSIÓN AUTORIZADA]: Enlace neuronal completado con éxito.");
            Console.WriteLine($"[SISTEMA]: Cadete '{_exploradorActual.Nombre}' sincronizado en {_exploradorActual.Realidad}.");
            Console.ResetColor();
            Console.WriteLine("\nPresione cualquier tecla para iniciar el entrenamiento...");
            Console.ReadKey(true);

            EjecutarBuclePrincipal();
        }

        private void EjecutarBuclePrincipal()
        {
            bool conectado = true;

            while (conectado)
            {
                MostrarMenuHUD();

                string opcion = Console.ReadLine();
                Console.ResetColor();

                switch (opcion)
                {
                    case "1":
                        ProcesarExploracion();
                        break;

                    case "2":
                        ProcesarAnalisisAnomalia();
                        break;

                    case "3":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\n[REPORTE NEXUS]: Explorador {_exploradorActual.Nombre} en sector {_exploradorActual.Realidad}.");
                        Console.WriteLine($"Vida: {_exploradorActual.Vida}% | Energía: {_exploradorActual.Energia}% | Estabilidad: {_exploradorActual.Estabilidad}% | Peligro: {_exploradorActual.Peligro}%");
                        Console.ResetColor();
                        break;

                    case "4":
                        _exploradorActual.RecargarEnergia(25);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n[RECARGA COMPLETADA]: Canalizando energía de la red (+25%).");
                        Console.ResetColor();
                        break;

                    case "5":
                        ProcesarSubmenuMapas();
                        break;

                    case "6":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nCerrando sesión de entrenamiento y guardando registros...");
                        Console.ResetColor();
                        conectado = false;
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\n[OPERACIÓN INVÁLIDA]: Opción no reconocida por el sistema.");
                        Console.ResetColor();
                        break;
                }

                // Verificaciones de estado del explorador
                if (!_exploradorActual.EstaVivo() && conectado)
                {
                    Console.Beep(200, 700);
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("\n========================================");
                    Console.WriteLine("  SOPORTE VITAL COLAPSADO - CADETE CAÍDO");
                    Console.WriteLine("========================================");
                    Console.WriteLine("La vida del operador ha llegado a 0. Desconexión forzada por muerte biométrica.");
                    Console.ResetColor();
                    conectado = false;
                }
                else if (_exploradorActual.EstadoCritico() && conectado)
                {
                    Console.Beep(300, 500);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n========================================");
                    Console.WriteLine(" ALERTA DE INTERFERENCIA CRÍTICA IRIS");
                    Console.WriteLine("========================================");
                    Console.WriteLine("Entorno colapsado. NEXUS ha forzado la desconexión inmediata.");
                    Console.ResetColor();
                    conectado = false;
                }

                if (conectado)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey(true);
                }
            }

            Console.WriteLine("\nSesión finalizada. Presione cualquier tecla para salir...");
            Console.ReadKey(true);
        }

        private void ProcesarExploracion()
        {
            if (_exploradorActual.Energia < 20)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR]: Energía insuficiente para explorar (Mínimo 20%).");
                Console.ResetColor();
                return;
            }

            if (_exploradorActual.Peligro > 60)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nZONA DE PELIGRO ALTO: ATAQUE DE HOSTILES EN CURSO");
                _exploradorActual.ConsumirEnergia(35);
                _exploradorActual.AjustarEstabilidad(-25);
                _exploradorActual.AjustarPeligro(10);
            }
            else if (_exploradorActual.Peligro >= 30)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nZONA DE IMPACTO MEDIO: CUIDADO CON LAS ANOMALÍAS");
                _exploradorActual.ConsumirEnergia(20);
                _exploradorActual.AjustarEstabilidad(-10);
                _exploradorActual.AjustarPeligro(15);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nEXPLORACIÓN SEGURA: SECTOR DESPEJADO");
                _exploradorActual.ConsumirEnergia(10);
                _exploradorActual.AjustarPeligro(20);
            }

            if (_rng.Next(1, 101) <= 50)
            {
                int dano = _rng.Next(15, 31);
                _exploradorActual.RecibirDano(dano);
                Console.Beep(350, 250);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[¡ENEMIGO DETECTADO!]: Te ha emboscado una entidad de la red.");
                Console.WriteLine($" -> Has recibido un impacto crítico: -{dano}% de Vida.");
                Console.ResetColor();
            }
        }

        private void ProcesarAnalisisAnomalia()
        {
            if (_exploradorActual.Energia < 20)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR]: Energía insuficiente para procesar la anomalía.");
                Console.ResetColor();
                return;
            }

            _exploradorActual.ConsumirEnergia(10);
            _exploradorActual.AjustarEstabilidad(10);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[ANÁLISIS COMPLETADO]: Estabilización lograda (+10% Estabilidad, -10% Energía).");
            Console.ResetColor();
        }

        private void ProcesarSubmenuMapas()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      NEXUS // MAPAS DE REALIDADES            ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║   [1] IRIS-07  // Valle de Cristal                           ║");
            Console.WriteLine("║   [2] IRIS-12  // Sector Abisal                              ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.Write("\nSeleccione una realidad a escanear: ");

            string subOpcion = Console.ReadLine();

            if (subOpcion == "1") _mapaIris07.RenderizarAnimado();
            else if (subOpcion == "2") _mapaIris12.RenderizarAnimado();
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR]: Realidad no encontrada o sin señal.");
                Console.ResetColor();
            }
        }

        // ==========================================
        // MÉTODOS VISUALES AUXILIARES DE INTERFAZ
        // ==========================================
        private void MostrarMenuHUD()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                  NEXUS // TERMINAL DE CONTROL                ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            Console.Write("║ OPERADOR: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{TruncarOAjustar(_exploradorActual.Nombre, 15),-15}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" SECTOR: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{TruncarOAjustar(_exploradorActual.Realidad, 26),-26}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ║");

            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");

            Console.Write("║ VIDA (HP):   "); DibujarBarraColoreada(_exploradorActual.Vida);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("║ ENERGÍA:     "); DibujarBarraColoreada(_exploradorActual.Energia);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("║ ESTABILIDAD:"); DibujarBarraColoreada(_exploradorActual.Estabilidad);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("║ PELIGRO:     "); DibujarBarraPeligro(_exploradorActual.Peligro);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║                       COMANDOS DE SISTEMA                    ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.Write("║ "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{TruncarOAjustar("[1] Explorar realidad actual", 60),-60}"); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(" ║");
            Console.Write("║ "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{TruncarOAjustar("[2] Analizar anomalía de la zona", 60),-60}"); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(" ║");
            Console.Write("║ "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{TruncarOAjustar("[3] Consultar reporte de estado", 60),-60}"); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(" ║");
            Console.Write("║ "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{TruncarOAjustar("[4] Reabastecer energía de la red", 60),-60}"); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(" ║");
            Console.Write("║ "); Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{TruncarOAjustar("[5] Cartografía y Mapas de Realidades", 60),-60}"); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(" ║");
            Console.Write("║ "); Console.ForegroundColor = ConsoleColor.Red; Console.Write($"{TruncarOAjustar("[6] Desconexión de emergencia / Salir", 60),-60}"); Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine(" ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n>> INGRESE CÓDIGO DE OPERACIÓN: ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void MostrarPantallaDeCarga()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         INICIONALIZANDO NÚCLEO DE SIMULACIÓN NEXUS v1.0        ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            string[] pasos = {
                "Cargando protocolos de inmersión...",
                "Estableciendo enlace con la red IRIS...",
                "Sincronizando matrices cartográficas...",
                "Verificando parámetros biométricos...",
                "Calibrando sensores de peligro entorno..."
            };

            for (int i = 0; i < pasos.Length; i++)
            {
                int porcentaje = (i + 1) * 20;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{porcentaje,3}%] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{pasos[i],-45}");

                Console.ForegroundColor = ConsoleColor.Green;
                for (int b = 0; b < 5; b++)
                {
                    Console.Write("█");
                    Thread.Sleep(25);
                }
                Console.ResetColor();
                Console.WriteLine(" [OK]");
                Thread.Sleep(100);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n[SISTEMA EN LÍNEA]: Presione cualquier tecla para acceder...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        private void DibujarBarraColoreada(int valor)
        {
            int bloques = Math.Min(Math.Max(valor, 0), 100) / 10;
            Console.Write("[");

            if (valor > 60) Console.ForegroundColor = ConsoleColor.Green;
            else if (valor > 25) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.Red;

            for (int i = 0; i < 10; i++) Console.Write(i < bloques ? "█" : "░");

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"] {valor,4}%                      ");
            Console.WriteLine("║");
            Console.ResetColor();
        }

        private void DibujarBarraPeligro(int valor)
        {
            int bloques = Math.Min(Math.Max(valor, 0), 100) / 10;
            Console.Write("[");

            if (valor > 60) Console.ForegroundColor = ConsoleColor.Red;
            else if (valor > 30) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.DarkGreen;

            for (int i = 0; i < 10; i++) Console.Write(i < bloques ? "█" : "░");

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"] {valor,4}%                      ");
            Console.WriteLine("║");
            Console.ResetColor();
        }

        private void EscribirLento(string texto, int velocidad = 15)
        {
            foreach (char c in texto)
            {
                Console.Write(c);
                Thread.Sleep(velocidad);
            }
            Console.WriteLine();
        }

        private void SimularEscaneoRegistro(string etiqueta, string valor)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($" -> Procesando {etiqueta} [{valor}]...");
            Thread.Sleep(200);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" [VERIFICADO]");
            Console.ResetColor();
            Console.Beep(800, 80);
        }

        private int LeerEnteroValido(string mensaje)
        {
            int numero;
            bool esValido;
            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(mensaje);
                Console.ForegroundColor = ConsoleColor.White;
                string entrada = Console.ReadLine();
                Console.ResetColor();

                esValido = int.TryParse(entrada, out numero);

                if (!esValido || numero < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" -> [ERROR DE LECTURA]: Valor no válido. Ingrese un entero positivo.");
                    Console.ResetColor();
                    esValido = false;
                }

            } while (!esValido);

            return numero;
        }

        private static string TruncarOAjustar(string texto, int maxLongitud)
        {
            if (string.IsNullOrEmpty(texto)) return new string(' ', maxLongitud);
            if (texto.Length > maxLongitud) return texto.Substring(0, maxLongitud);
            return texto;
        }
    }

    // ==========================================
    // PUNTO DE ENTRADA PRINCIPAL
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            SistemaNexus sistema = new SistemaNexus();
            sistema.Iniciar();
        }
    }
}