using System;
using System.Collections.Generic;

namespace NexusWindowsForm
{
    public sealed class Explorer
    {
        public string Name { get; }
        public int Age { get; }
        public int Life { get; private set; } = 100;
        public int Energy { get; private set; }
        public int Connection { get; private set; }

        public Explorer(string name, int age, int energy, int connection)
        {
            Name = name;
            Age = age;
            Energy = Math.Clamp(energy, 1, 100);
            Connection = Math.Clamp(connection, 1, 100);
        }

        public bool SpendEnergy(int amount)
        {
            if (Energy < amount) return false;
            Energy = Math.Max(0, Energy - amount);
            Connection = Math.Max(0, Connection - 5);
            return true;
        }

        public void RecoverEnergy(int amount)
        {
            Energy = Math.Min(100, Energy + amount);
            Connection = Math.Min(100, Connection + 10);
        }

        public void TakeDamage(int amount) => Life = Math.Max(0, Life - amount);
        public void RecoverLife(int amount) => Life = Math.Min(100, Life + amount);
    }

    public sealed class Inventory
    {
        private readonly Dictionary<string, int> items = new(StringComparer.OrdinalIgnoreCase)
        {
            ["CELULA DE ENERGIA"] = 0,
            ["RECURSO"] = 0
        };

        public IReadOnlyDictionary<string, int> Items => items;

        public void Add(string name)
        {
            if (!items.ContainsKey(name)) items[name] = 0;
            items[name]++;
        }

        public bool Use(string name)
        {
            if (!items.TryGetValue(name, out int amount) || amount <= 0) return false;
            items[name]--;
            return true;
        }
    }

    public sealed class Dimension
    {
        private readonly string[,] map =
        {
            { "E", ".", ".", ".", ".", ".", ".", ".", ".", "." },
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

        public string Name { get; }
        public Dimension(string name) => Name = name;
        public string GetCell(int x, int y) => map[y, x];
        public void SetCell(int x, int y, string value) => map[y, x] = value;
    }

    public sealed class GameNotice
    {
        public string Title { get; init; } = "";
        public string Message { get; init; } = "";
        public string Type { get; init; } = "info";
        public bool CombatStarted { get; init; }
        public bool CoreFound { get; init; }
        public bool MissionCompleted { get; init; }
        public bool Defeat { get; init; }
    }

    public sealed class NexusGameEngine
    {
        public Explorer Explorer { get; }
        public Inventory Inventory { get; } = new();
        public Dimension? CurrentDimension { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool MissionActive { get; private set; }
        public bool CoreFound { get; private set; }
        public bool IsDefeated => Explorer.Life <= 0;
        public bool InCombat { get; private set; }
        public int EnemyLife { get; private set; } = 30;

        public NexusGameEngine(Explorer explorer) => Explorer = explorer;

        public GameNotice StartMission(int dimensionIndex)
        {
            if (dimensionIndex < 0 || dimensionIndex > 1)
                return Notice("REALIDAD NO VÁLIDA", "Selecciona una realidad disponible.", "error");

            if (!Explorer.SpendEnergy(30))
                return Notice("ENERGÍA INSUFICIENTE", "Necesitas 30 puntos de energía para viajar.", "error");

            CurrentDimension = new Dimension(dimensionIndex == 0 ? "REALIDAD 102" : "REALIDAD 999");
            X = 0;
            Y = 0;
            CoreFound = false;
            MissionActive = true;
            InCombat = false;
            return Notice("MISIÓN INICIADA", $"Has ingresado a {CurrentDimension.Name}. Encuentra la zona de anomalía y registra el núcleo energético.", "success");
        }

        public GameNotice Move(int dx, int dy)
        {
            if (!MissionActive || CurrentDimension == null)
                return Notice("SIN EXPLORACIÓN", "Primero inicia una misión.", "warning");
            if (InCombat)
                return Notice("COMBATE ACTIVO", "Resuelve el combate antes de moverte.", "warning");

            int nx = X + dx;
            int ny = Y + dy;
            if (nx < 0 || nx > 9 || ny < 0 || ny > 9)
                return Notice("LÍMITE DEL MAPA", "No puedes salir del mapa dimensional.", "warning");
            if (!Explorer.SpendEnergy(2))
                return Notice("ENERGÍA INSUFICIENTE", "No tienes energía suficiente para moverte.", "error");

            X = nx;
            Y = ny;
            string cell = CurrentDimension.GetCell(X, Y);

            switch (cell)
            {
                case "*":
                    Inventory.Add("CELULA DE ENERGIA");
                    CurrentDimension.SetCell(X, Y, ".");
                    return Notice("RECURSO ENCONTRADO", "Has recolectado una célula de energía. Se agregó al inventario.", "success");
                case "+":
                case "#":
                    InCombat = true;
                    EnemyLife = 30;
                    return new GameNotice { Title = "ENTIDAD DIMENSIONAL", Message = "Un bicho dimensional bloquea tu camino. Puedes atacar o escapar.", Type = "combat", CombatStarted = true };
                case "R":
                    return Notice("ZONA DE ANOMALÍA", "Los sensores detectan una fuente de energía más adelante.", "warning");
                case "O":
                    return new GameNotice { Title = "NÚCLEO ENERGÉTICO", Message = "Has encontrado el núcleo energético. ¿Deseas registrarlo?", Type = "core", CoreFound = true };
                case "E":
                    return Notice("ZONA DE EXTRACCIÓN", "Punto de extracción de la nave.", "info");
                default:
                    return Notice("MOVIMIENTO REGISTRADO", $"Posición actual: ({X}, {Y}).", "info");
            }
        }

        public GameNotice Attack()
        {
            if (!InCombat) return Notice("SIN COMBATE", "No hay ninguna entidad enfrentándose al cadete.", "warning");
            EnemyLife = Math.Max(0, EnemyLife - 20);
            if (EnemyLife == 0)
            {
                InCombat = false;
                CurrentDimension!.SetCell(X, Y, ".");
                return Notice("ENTIDAD ELIMINADA", "El bicho dimensional ha sido derrotado. Puedes continuar.", "success");
            }

            Explorer.TakeDamage(10);
            if (Explorer.Life == 0)
            {
                MissionActive = false;
                InCombat = false;
                return new GameNotice { Title = "MISIÓN FALLIDA", Message = "El cadete ha perdido toda su vida.", Type = "error", Defeat = true };
            }

            return Notice("COMBATE", $"Has causado 20 de daño. La entidad contraatacó y causó 10 de daño. Vida del enemigo: {EnemyLife}/30.", "warning");
        }

        public GameNotice Escape()
        {
            if (!InCombat) return Notice("SIN COMBATE", "No hay una entidad de la que escapar.", "warning");
            InCombat = false;
            return Notice("RETIRADA", "Has escapado del bicho dimensional. Puedes continuar tu camino.", "success");
        }

        public GameNotice AnalyzeAnomaly()
        {
            if (!Explorer.SpendEnergy(20))
                return Notice("ENERGÍA INSUFICIENTE", "El análisis requiere 20 puntos de energía.", "error");
            return Notice("ANÁLISIS COMPLETADO", "Los sensores detectaron una anomalía dimensional. Costo: 20 puntos de energía.", "success");
        }

        public GameNotice RecoverEnergy()
        {
            Explorer.RecoverEnergy(20);
            return Notice("ENERGÍA RECUPERADA", "Se recuperaron 20 puntos de energía y mejoró la conexión.", "success");
        }

        public GameNotice RegisterCore()
        {
            if (CurrentDimension == null || CurrentDimension.GetCell(X, Y) != "O")
                return Notice("NÚCLEO NO DISPONIBLE", "Debes encontrar el núcleo energético para registrarlo.", "warning");

            CoreFound = true;
            MissionActive = false;
            CurrentDimension.SetCell(X, Y, "E");
            return new GameNotice { Title = "MISIÓN COMPLETADA", Message = "Núcleo energético registrado. Anomalía identificada y datos enviados a la nave. ¡Misión cumplida!", Type = "success", MissionCompleted = true };
        }

        public string[,] GetMapSnapshot()
        {
            var snapshot = new string[10, 10];
            if (CurrentDimension == null) return snapshot;
            for (int row = 0; row < 10; row++)
                for (int col = 0; col < 10; col++)
                    snapshot[row, col] = CurrentDimension.GetCell(col, row);
            return snapshot;
        }

        private static GameNotice Notice(string title, string message, string type) =>
            new() { Title = title, Message = message, Type = type };
    }
}
