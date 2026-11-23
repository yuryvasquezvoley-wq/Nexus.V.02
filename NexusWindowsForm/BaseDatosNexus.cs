using System.Data;
using Microsoft.Data.SqlClient;

namespace NexusWindowsForm;

/// <summary>
/// Crea la base de datos NEXUS_DB si no existe y registra jugadores y cada ingreso al juego.
/// </summary>
internal static class BaseDatosNexus
{
    private const string NombreBaseDatos = "NEXUS_DB";
    private static readonly object BloqueoInicializacion = new();

    private static string CadenaConexion
    {
        get
        {
            string? configurada = Environment.GetEnvironmentVariable("NEXUS_SQL_CONNECTION");
            return string.IsNullOrWhiteSpace(configurada)
                ? @"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;"
                : configurada;
        }
    }

    public static SesionJugador RegistrarIngreso(
        string nombre,
        int edad,
        int energiaInicial,
        int vidaInicial,
        int conexionInicial,
        string realidad)
    {
        string nombreLimpio = nombre.Trim();
        if (nombreLimpio.Length == 0)
            throw new ArgumentException("El nombre del cadete es obligatorio.", nameof(nombre));

        AsegurarBaseDatos();

        using SqlConnection conexion = new(CadenaConexion);
        conexion.Open();
        using SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable);

        int idJugador;
        using (SqlCommand buscar = new(
            "SELECT IdJugador FROM dbo.Jugadores WITH (UPDLOCK, HOLDLOCK) WHERE NombreNormalizado = @NombreNormalizado;",
            conexion, transaccion))
        {
            buscar.Parameters.Add("@NombreNormalizado", SqlDbType.NVarChar, 120).Value = Normalizar(nombreLimpio);
            object? existente = buscar.ExecuteScalar();

            if (existente == null || existente == DBNull.Value)
            {
                using SqlCommand insertar = new(
                    @"INSERT INTO dbo.Jugadores
                      (NombreJugador, NombreNormalizado, Edad, FechaRegistro, UltimoIngreso,
                       CantidadIngresos, Nivel, Energia, Vida, Conexion, Realidad)
                      VALUES
                      (@Nombre, @Normalizado, @Edad, SYSUTCDATETIME(), SYSUTCDATETIME(),
                       1, 1, @Energia, @Vida, @Conexion, @Realidad);
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    conexion, transaccion);
                AgregarDatosJugador(insertar, nombreLimpio, edad, energiaInicial, vidaInicial, conexionInicial, realidad);
                idJugador = (int)insertar.ExecuteScalar()!;
            }
            else
            {
                idJugador = Convert.ToInt32(existente);
                using SqlCommand actualizar = new(
                    @"UPDATE dbo.Jugadores
                      SET NombreJugador = @Nombre, Edad = @Edad, UltimoIngreso = SYSUTCDATETIME(),
                          CantidadIngresos = CantidadIngresos + 1, Energia = @Energia,
                          Vida = @Vida, Conexion = @Conexion, Realidad = @Realidad
                      WHERE IdJugador = @IdJugador;",
                    conexion, transaccion);
                AgregarDatosJugador(actualizar, nombreLimpio, edad, energiaInicial, vidaInicial, conexionInicial, realidad);
                actualizar.Parameters.Add("@IdJugador", SqlDbType.Int).Value = idJugador;
                actualizar.ExecuteNonQuery();
            }
        }

        long idIngreso;
        using (SqlCommand insertarIngreso = new(
            @"INSERT INTO dbo.Ingresos
              (IdJugador, FechaIngreso, EnergiaInicial, VidaFinal, EnergiaFinal, ConexionFinal, Realidad)
              VALUES
              (@IdJugador, SYSUTCDATETIME(), @Energia, @Vida, @Energia, @Conexion, @Realidad);
              SELECT CAST(SCOPE_IDENTITY() AS bigint);",
            conexion, transaccion))
        {
            insertarIngreso.Parameters.Add("@IdJugador", SqlDbType.Int).Value = idJugador;
            insertarIngreso.Parameters.Add("@Energia", SqlDbType.Int).Value = energiaInicial;
            insertarIngreso.Parameters.Add("@Vida", SqlDbType.Int).Value = vidaInicial;
            insertarIngreso.Parameters.Add("@Conexion", SqlDbType.Int).Value = conexionInicial;
            insertarIngreso.Parameters.Add("@Realidad", SqlDbType.NVarChar, 30).Value = realidad;
            idIngreso = (long)insertarIngreso.ExecuteScalar()!;
        }

        transaccion.Commit();
        return new SesionJugador(idJugador, idIngreso);
    }

    public static void ActualizarEstado(
        SesionJugador sesion,
        int energia,
        int vida,
        int conexionActual,
        string realidad)
    {
        AsegurarBaseDatos();

        using SqlConnection conexion = new(CadenaConexion);
        conexion.Open();
        using SqlTransaction transaccion = conexion.BeginTransaction();

        using (SqlCommand jugador = new(
            @"UPDATE dbo.Jugadores
              SET Energia = @Energia, Vida = @Vida, Conexion = @Conexion, Realidad = @Realidad
              WHERE IdJugador = @IdJugador;",
            conexion, transaccion))
        {
            jugador.Parameters.Add("@Energia", SqlDbType.Int).Value = energia;
            jugador.Parameters.Add("@Vida", SqlDbType.Int).Value = vida;
            jugador.Parameters.Add("@Conexion", SqlDbType.Int).Value = conexionActual;
            jugador.Parameters.Add("@Realidad", SqlDbType.NVarChar, 30).Value = realidad;
            jugador.Parameters.Add("@IdJugador", SqlDbType.Int).Value = sesion.IdJugador;
            jugador.ExecuteNonQuery();
        }

        using (SqlCommand ingreso = new(
            @"UPDATE dbo.Ingresos
              SET FechaSalida = SYSUTCDATETIME(), VidaFinal = @Vida,
                  EnergiaFinal = @Energia, ConexionFinal = @Conexion, Realidad = @Realidad
              WHERE IdIngreso = @IdIngreso;",
            conexion, transaccion))
        {
            ingreso.Parameters.Add("@Vida", SqlDbType.Int).Value = vida;
            ingreso.Parameters.Add("@Energia", SqlDbType.Int).Value = energia;
            ingreso.Parameters.Add("@Conexion", SqlDbType.Int).Value = conexionActual;
            ingreso.Parameters.Add("@Realidad", SqlDbType.NVarChar, 30).Value = realidad;
            ingreso.Parameters.Add("@IdIngreso", SqlDbType.BigInt).Value = sesion.IdIngreso;
            ingreso.ExecuteNonQuery();
        }

        transaccion.Commit();
    }

    private static void AgregarDatosJugador(
        SqlCommand comando,
        string nombre,
        int edad,
        int energia,
        int vida,
        int conexion,
        string realidad)
    {
        comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 120).Value = nombre;
        comando.Parameters.Add("@Normalizado", SqlDbType.NVarChar, 120).Value = Normalizar(nombre);
        comando.Parameters.Add("@Edad", SqlDbType.Int).Value = edad;
        comando.Parameters.Add("@Energia", SqlDbType.Int).Value = energia;
        comando.Parameters.Add("@Vida", SqlDbType.Int).Value = vida;
        comando.Parameters.Add("@Conexion", SqlDbType.Int).Value = conexion;
        comando.Parameters.Add("@Realidad", SqlDbType.NVarChar, 30).Value = realidad;
    }

    private static string Normalizar(string nombre) => nombre.Trim().ToUpperInvariant();

    private static void AsegurarBaseDatos()
    {
        lock (BloqueoInicializacion)
        {
            var constructor = new SqlConnectionStringBuilder(CadenaConexion);
            string baseDatosOriginal = constructor.InitialCatalog;
            constructor.InitialCatalog = "master";

            using (SqlConnection master = new(constructor.ConnectionString))
            {
                master.Open();
                using SqlCommand crearBase = new(
                    "IF DB_ID(N'NEXUS_DB') IS NULL CREATE DATABASE [NEXUS_DB];",
                    master);
                crearBase.ExecuteNonQuery();
            }

            constructor.InitialCatalog = NombreBaseDatos;
            using SqlConnection conexion = new(constructor.ConnectionString);
            conexion.Open();
            using SqlCommand crearTablas = new(
                @"IF OBJECT_ID(N'dbo.Jugadores', N'U') IS NULL
                  BEGIN
                    CREATE TABLE dbo.Jugadores
                    (
                      IdJugador INT IDENTITY(1,1) PRIMARY KEY,
                      NombreJugador NVARCHAR(120) NOT NULL,
                      NombreNormalizado NVARCHAR(120) NOT NULL UNIQUE,
                      Edad INT NOT NULL,
                      FechaRegistro DATETIME2 NOT NULL,
                      UltimoIngreso DATETIME2 NOT NULL,
                      CantidadIngresos INT NOT NULL CONSTRAINT DF_Jugadores_Cantidad DEFAULT 0,
                      Nivel INT NOT NULL CONSTRAINT DF_Jugadores_Nivel DEFAULT 1,
                      Energia INT NOT NULL,
                      Vida INT NOT NULL,
                      Conexion INT NOT NULL,
                      Realidad NVARCHAR(30) NOT NULL
                    );
                  END;

                  IF OBJECT_ID(N'dbo.Ingresos', N'U') IS NULL
                  BEGIN
                    CREATE TABLE dbo.Ingresos
                    (
                      IdIngreso BIGINT IDENTITY(1,1) PRIMARY KEY,
                      IdJugador INT NOT NULL,
                      FechaIngreso DATETIME2 NOT NULL,
                      FechaSalida DATETIME2 NULL,
                      EnergiaInicial INT NOT NULL,
                      VidaFinal INT NOT NULL,
                      EnergiaFinal INT NOT NULL,
                      ConexionFinal INT NOT NULL,
                      Realidad NVARCHAR(30) NOT NULL,
                      CONSTRAINT FK_Ingresos_Jugadores
                        FOREIGN KEY (IdJugador) REFERENCES dbo.Jugadores(IdJugador)
                    );
                    CREATE INDEX IX_Ingresos_IdJugador_Fecha
                      ON dbo.Ingresos(IdJugador, FechaIngreso DESC);
                  END;",
                conexion);
            crearTablas.ExecuteNonQuery();
        }
    }
}

internal sealed record SesionJugador(int IdJugador, long IdIngreso);
