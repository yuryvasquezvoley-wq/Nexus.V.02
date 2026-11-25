using Microsoft.Data.SqlClient;
using System.Data;

namespace NexusWindowsForm.Persistencia;

/// <summary>Persists a single NEXUS save slot in SQL Server.</summary>
public sealed class RepositorioPartidasSql
{
    private const int SlotId = 1;

    public static string CadenaConexionConfigurada =>
        Environment.GetEnvironmentVariable("NEXUS_SQL_CONNECTION") ??
        @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=NexusGame;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5";

    public void Guardar(EstadoPartida estado)
    {
        ArgumentNullException.ThrowIfNull(estado);
        Validar(estado);

        using SqlConnection conexion = new(CadenaConexionConfigurada);
        conexion.Open();
        using SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            using (SqlCommand comando = new("""
                UPDATE dbo.Partida
                   SET NombreCadete=@nombre, Edad=@edad, Realidad=@realidad,
                       PosicionX=@x, PosicionY=@y, Vida=@vida, Energia=@energia,
                       Conexion=@conexion, CelulasMedicas=@celulas, Resistencia=@resistencia,
                       NaveReparada=@reparada, RecuperacionUsada=@recuperacion,
                       FechaGuardado=SYSUTCDATETIME()
                 WHERE SlotId=@slot;
                IF @@ROWCOUNT = 0
                    INSERT dbo.Partida
                        (SlotId, NombreCadete, Edad, Realidad, PosicionX, PosicionY,
                         Vida, Energia, Conexion, CelulasMedicas, Resistencia,
                         NaveReparada, RecuperacionUsada, FechaGuardado)
                    VALUES
                        (@slot, @nombre, @edad, @realidad, @x, @y, @vida, @energia,
                         @conexion, @celulas, @resistencia, @reparada, @recuperacion,
                         SYSUTCDATETIME());
                """, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@slot", SlotId);
                comando.Parameters.AddWithValue("@nombre", estado.NombreCadete);
                comando.Parameters.AddWithValue("@edad", estado.Edad);
                comando.Parameters.AddWithValue("@realidad", estado.Realidad);
                comando.Parameters.AddWithValue("@x", estado.PosicionX);
                comando.Parameters.AddWithValue("@y", estado.PosicionY);
                comando.Parameters.AddWithValue("@vida", estado.Vida);
                comando.Parameters.AddWithValue("@energia", estado.Energia);
                comando.Parameters.AddWithValue("@conexion", estado.Conexion);
                comando.Parameters.AddWithValue("@celulas", estado.CelulasMedicas);
                comando.Parameters.AddWithValue("@resistencia", estado.Resistencia);
                comando.Parameters.AddWithValue("@reparada", estado.NaveReparada);
                comando.Parameters.AddWithValue("@recuperacion", estado.RecuperacionUsada);
                comando.ExecuteNonQuery();
            }

            Ejecutar(conexion, transaccion, "DELETE dbo.NucleoPartida WHERE SlotId=@slot;", c => c.Parameters.AddWithValue("@slot", SlotId));
            Ejecutar(conexion, transaccion, "DELETE dbo.EnemigoPartida WHERE SlotId=@slot;", c => c.Parameters.AddWithValue("@slot", SlotId));
            Ejecutar(conexion, transaccion, "DELETE dbo.ObjetoPartida WHERE SlotId=@slot;", c => c.Parameters.AddWithValue("@slot", SlotId));
            Ejecutar(conexion, transaccion, "DELETE dbo.PortalPartida WHERE SlotId=@slot;", c => c.Parameters.AddWithValue("@slot", SlotId));

            foreach (string nucleo in estado.NucleosRecolectados.Distinct(StringComparer.Ordinal))
                Ejecutar(conexion, transaccion,
                    "INSERT dbo.NucleoPartida (SlotId, NucleoId) VALUES (@slot, @id);",
                    c => { c.Parameters.AddWithValue("@slot", SlotId); c.Parameters.AddWithValue("@id", nucleo); });

            foreach (EstadoEnemigo enemigo in estado.Enemigos)
                Ejecutar(conexion, transaccion, """
                    INSERT dbo.EnemigoPartida
                        (SlotId, EnemigoId, Realidad, Nombre, Tipo, PosicionX, PosicionY,
                         DestinoX, DestinoY, Vida, VidaMaxima, Velocidad, EsJefe, EsZombie, Derrotada)
                    VALUES
                        (@slot, @id, @realidad, @nombre, @tipo, @x, @y,
                         @destinoX, @destinoY, @vida, @vidaMaxima, @velocidad, @esJefe, @esZombie, @derrotada);
                    """, c =>
                    {
                        c.Parameters.AddWithValue("@slot", SlotId);
                        c.Parameters.AddWithValue("@id", enemigo.Id);
                        c.Parameters.AddWithValue("@realidad", enemigo.Realidad);
                        c.Parameters.AddWithValue("@nombre", enemigo.Nombre);
                        c.Parameters.AddWithValue("@tipo", enemigo.Tipo);
                        c.Parameters.AddWithValue("@x", enemigo.X);
                        c.Parameters.AddWithValue("@y", enemigo.Y);
                        c.Parameters.AddWithValue("@destinoX", enemigo.DestinoX);
                        c.Parameters.AddWithValue("@destinoY", enemigo.DestinoY);
                        c.Parameters.AddWithValue("@vida", enemigo.Vida);
                        c.Parameters.AddWithValue("@vidaMaxima", enemigo.VidaMaxima);
                        c.Parameters.AddWithValue("@velocidad", enemigo.Velocidad);
                        c.Parameters.AddWithValue("@esJefe", enemigo.EsJefe);
                        c.Parameters.AddWithValue("@esZombie", enemigo.EsZombie);
                        c.Parameters.AddWithValue("@derrotada", enemigo.Derrotada);
                    });

            foreach (EstadoObjeto objeto in estado.Objetos)
                Ejecutar(conexion, transaccion,
                    "INSERT dbo.ObjetoPartida (SlotId, Realidad, Tipo, Indice, PosicionX, PosicionY) VALUES (@slot, @realidad, @tipo, @indice, @x, @y);",
                    c =>
                    {
                        c.Parameters.AddWithValue("@slot", SlotId);
                        c.Parameters.AddWithValue("@realidad", objeto.Realidad);
                        c.Parameters.AddWithValue("@tipo", objeto.Tipo);
                        c.Parameters.AddWithValue("@indice", objeto.Indice);
                        c.Parameters.AddWithValue("@x", objeto.X);
                        c.Parameters.AddWithValue("@y", objeto.Y);
                    });

            foreach (EstadoPortal portal in estado.Portales)
                Ejecutar(conexion, transaccion,
                    "INSERT dbo.PortalPartida (SlotId, Realidad, PosicionX, PosicionY) VALUES (@slot, @realidad, @x, @y);",
                    c =>
                    {
                        c.Parameters.AddWithValue("@slot", SlotId);
                        c.Parameters.AddWithValue("@realidad", portal.Realidad);
                        c.Parameters.AddWithValue("@x", portal.X);
                        c.Parameters.AddWithValue("@y", portal.Y);
                    });

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public EstadoPartida? Cargar()
    {
        using SqlConnection conexion = new(CadenaConexionConfigurada);
        conexion.Open();
        EstadoPartida? estado;
        using (SqlCommand comando = new("""
            SELECT NombreCadete, Edad, Realidad, PosicionX, PosicionY, Vida, Energia,
                   Conexion, CelulasMedicas, Resistencia, NaveReparada, RecuperacionUsada
              FROM dbo.Partida WHERE SlotId=@slot;
            """, conexion))
        {
            comando.Parameters.AddWithValue("@slot", SlotId);
            using SqlDataReader lector = comando.ExecuteReader();
            if (!lector.Read()) return null;
            estado = new EstadoPartida
            {
                NombreCadete = lector.GetString(0), Edad = lector.GetInt32(1),
                Realidad = lector.GetString(2), PosicionX = Convert.ToSingle(lector.GetDouble(3)),
                PosicionY = Convert.ToSingle(lector.GetDouble(4)), Vida = lector.GetInt32(5),
                Energia = lector.GetInt32(6), Conexion = lector.GetInt32(7),
                CelulasMedicas = lector.GetInt32(8), Resistencia = Convert.ToSingle(lector.GetDouble(9)),
                NaveReparada = lector.GetBoolean(10), RecuperacionUsada = lector.GetBoolean(11)
            };
        }

        using (SqlCommand comando = new("SELECT NucleoId FROM dbo.NucleoPartida WHERE SlotId=@slot;", conexion))
        {
            comando.Parameters.AddWithValue("@slot", SlotId);
            using SqlDataReader lector = comando.ExecuteReader();
            while (lector.Read()) estado.NucleosRecolectados.Add(lector.GetString(0));
        }
        using (SqlCommand comando = new("""
            SELECT EnemigoId, Realidad, Nombre, Tipo, PosicionX, PosicionY, DestinoX, DestinoY,
                   Vida, VidaMaxima, Velocidad, EsJefe, EsZombie, Derrotada
              FROM dbo.EnemigoPartida WHERE SlotId=@slot;
            """, conexion))
        {
            comando.Parameters.AddWithValue("@slot", SlotId);
            using SqlDataReader lector = comando.ExecuteReader();
            while (lector.Read()) estado.Enemigos.Add(new EstadoEnemigo
            {
                Id = lector.GetString(0), Realidad = lector.GetString(1), Nombre = lector.GetString(2),
                Tipo = lector.GetString(3), X = Convert.ToSingle(lector.GetDouble(4)),
                Y = Convert.ToSingle(lector.GetDouble(5)), DestinoX = Convert.ToSingle(lector.GetDouble(6)),
                DestinoY = Convert.ToSingle(lector.GetDouble(7)), Vida = lector.GetInt32(8),
                VidaMaxima = lector.GetInt32(9), Velocidad = Convert.ToSingle(lector.GetDouble(10)),
                EsJefe = lector.GetBoolean(11), EsZombie = lector.GetBoolean(12), Derrotada = lector.GetBoolean(13)
            });
        }
        using (SqlCommand comando = new("SELECT Realidad, Tipo, Indice, PosicionX, PosicionY FROM dbo.ObjetoPartida WHERE SlotId=@slot;", conexion))
        {
            comando.Parameters.AddWithValue("@slot", SlotId);
            using SqlDataReader lector = comando.ExecuteReader();
            while (lector.Read()) estado.Objetos.Add(new EstadoObjeto
            {
                Realidad = lector.GetString(0), Tipo = lector.GetString(1), Indice = lector.GetInt32(2),
                X = Convert.ToSingle(lector.GetDouble(3)), Y = Convert.ToSingle(lector.GetDouble(4))
            });
        }
        using (SqlCommand comando = new("SELECT Realidad, PosicionX, PosicionY FROM dbo.PortalPartida WHERE SlotId=@slot;", conexion))
        {
            comando.Parameters.AddWithValue("@slot", SlotId);
            using SqlDataReader lector = comando.ExecuteReader();
            while (lector.Read()) estado.Portales.Add(new EstadoPortal
            {
                Realidad = lector.GetString(0), X = Convert.ToSingle(lector.GetDouble(1)), Y = Convert.ToSingle(lector.GetDouble(2))
            });
        }
        return estado;
    }

    private static void Ejecutar(SqlConnection conexion, SqlTransaction transaccion, string sql, Action<SqlCommand> agregarParametros)
    {
        using SqlCommand comando = new(sql, conexion, transaccion);
        agregarParametros(comando);
        comando.ExecuteNonQuery();
    }

    private static void Validar(EstadoPartida estado)
    {
        if (string.IsNullOrWhiteSpace(estado.NombreCadete) || estado.NombreCadete.Length > 50)
            throw new ArgumentException("El nombre debe tener entre 1 y 50 caracteres.");
        if (estado.Edad is < 1 or > 120 || estado.Realidad is not ("REALIDAD 102" or "REALIDAD 999"))
            throw new ArgumentException("La edad o la realidad guardada no son válidas.");
        if (estado.Vida is < 0 or > 100 || estado.Energia is < 0 or > 100 || estado.Conexion is < 0 or > 100 ||
            estado.CelulasMedicas < 0 || estado.Resistencia is < 0 or > 100)
            throw new ArgumentException("El estado del jugador contiene valores fuera de rango.");
        if (!float.IsFinite(estado.PosicionX) || !float.IsFinite(estado.PosicionY) ||
            estado.PosicionX is < 0 or > 5000 || estado.PosicionY is < 0 or > 5000)
            throw new ArgumentException("La posición del jugador no es válida.");
    }
}
