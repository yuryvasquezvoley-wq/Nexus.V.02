namespace NexusWindowsForm.Persistencia;

public sealed class EstadoPartida
{
    public string NombreCadete { get; set; } = "CADETE";
    public int Edad { get; set; }
    public string Realidad { get; set; } = "REALIDAD 102";
    public float PosicionX { get; set; }
    public float PosicionY { get; set; }
    public int Vida { get; set; }
    public int Energia { get; set; }
    public int Conexion { get; set; }
    public int CelulasMedicas { get; set; }
    public float Resistencia { get; set; }
    public bool NaveReparada { get; set; }
    public bool RecuperacionUsada { get; set; }
    public List<string> NucleosRecolectados { get; set; } = new();
    public List<EstadoEnemigo> Enemigos { get; set; } = new();
    public List<EstadoObjeto> Objetos { get; set; } = new();
    public List<EstadoPortal> Portales { get; set; } = new();
}

public sealed class EstadoEnemigo
{
    public string Id { get; set; } = "";
    public string Realidad { get; set; } = "REALIDAD 102";
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public float X { get; set; }
    public float Y { get; set; }
    public float DestinoX { get; set; }
    public float DestinoY { get; set; }
    public int Vida { get; set; }
    public int VidaMaxima { get; set; }
    public float Velocidad { get; set; }
    public bool EsJefe { get; set; }
    public bool EsZombie { get; set; }
    public bool Derrotada { get; set; }
}

public sealed class EstadoObjeto
{
    public string Realidad { get; set; } = "REALIDAD 102";
    public string Tipo { get; set; } = "ENERGIA";
    public int Indice { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
}

public sealed class EstadoPortal
{
    public string Realidad { get; set; } = "REALIDAD 102";
    public float X { get; set; }
    public float Y { get; set; }
}
