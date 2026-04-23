namespace ConquiTap.Models;

// ─── Catálogos ───────────────────────────────────────────────────────────────

public class Asociacion
{
    public int    Id     { get; set; }
    public string Nombre { get; set; } = "";
    public bool   Activo { get; set; } = true;
    public override string ToString() => Nombre;
}

public class Zona
{
    public int    Id           { get; set; }
    public string Nombre       { get; set; } = "";
    public int    AsociacionId { get; set; }
    public bool   Activo       { get; set; } = true;
    public override string ToString() => Nombre;
}

public class Distrito
{
    public int    Id     { get; set; }
    public string Nombre { get; set; } = "";
    public int    ZonaId { get; set; }
    public bool   Activo { get; set; } = true;
    public override string ToString() => Nombre;
}

public class Iglesia
{
    public int    Id         { get; set; }
    public string Nombre     { get; set; } = "";
    public int    DistritoId { get; set; }
    public bool   Activo     { get; set; } = true;
    public override string ToString() => Nombre;
}

public class Especialidad
{
    public int    Id        { get; set; }
    public string Nombre    { get; set; } = "";
    public string Categoria { get; set; } = "";
    public bool   Activo    { get; set; } = true;
    public override string ToString() => Nombre;
}

// ─── Usuario ─────────────────────────────────────────────────────────────────

public class Usuario
{
    public int       Id             { get; set; }
    public string    NombreUsuario  { get; set; } = "";
    public string    ContrasenaHash { get; set; } = "";
    public string    Correo         { get; set; } = "";
    /// <summary>Miembro | Directivo | Administrador</summary>
    public string    Categoria      { get; set; } = "Miembro";
    public bool      Activo         { get; set; } = true;
    public DateTime  FechaCreacion  { get; set; }
    public DateTime? UltimoAcceso   { get; set; }

    public bool EsAdministrador => Categoria == "Administrador";
    public bool EsDirectivo     => Categoria is "Directivo" or "Administrador";

    public override string ToString() => NombreUsuario;
}

// ─── Club ────────────────────────────────────────────────────────────────────

public class Club
{
    public int     Id           { get; set; }
    public string  Nombre       { get; set; } = "";
    public int?    IglesiaId    { get; set; }
    public string  IglesiaNombre{ get; set; } = "";
    public int?    DirectorId   { get; set; }
    public string  DirectorNombre{ get; set; } = "";
    public int?    AnoFundacion { get; set; }
    /// <summary>Aventureros | Conquistadores | Guias</summary>
    public string  TipoClub     { get; set; } = "Conquistadores";
    public bool    Activo       { get; set; } = true;

    public int     TotalMiembros { get; set; }
    public override string ToString() => Nombre;
}

// ─── Miembro ─────────────────────────────────────────────────────────────────

public class Miembro
{
    public int       Id               { get; set; }
    public string    Nombres          { get; set; } = "";
    public string    Apellidos        { get; set; } = "";
    public string    Cedula           { get; set; } = "";
    public string    Telefono         { get; set; } = "";
    public string    Correo           { get; set; } = "";
    public int?      UsuarioId        { get; set; }
    public int?      AsociacionId     { get; set; }
    public string    AsociacionNombre { get; set; } = "";
    public int?      ZonaId           { get; set; }
    public string    ZonaNombre       { get; set; } = "";
    public int?      DistritoId       { get; set; }
    public string    DistritoNombre   { get; set; } = "";
    public int?      IglesiaId        { get; set; }
    public string    IglesiaNombre    { get; set; } = "";
    /// <summary>Miembro | Directivo</summary>
    public string    Categoria        { get; set; } = "Miembro";
    public string    Puesto           { get; set; } = "";
    public string    Clase            { get; set; } = "";
    public DateTime? FechaInvestidura { get; set; }
    public int?      ClubId           { get; set; }
    public string    ClubNombre       { get; set; } = "";
    /// <summary>M | F</summary>
    public string    Sexo             { get; set; } = "";
    public DateTime? FechaNacimiento  { get; set; }
    public DateTime? FechaBautismo    { get; set; }
    public bool      Activo           { get; set; } = true;
    public DateTime  FechaRegistro    { get; set; }

    public List<Especialidad> Especialidades { get; set; } = new();

    public string NombreCompleto => $"{Nombres} {Apellidos}";

    public int? Edad => FechaNacimiento.HasValue
        ? (int?)((DateTime.Today - FechaNacimiento.Value).TotalDays / 365.25)
        : null;

    public override string ToString() => NombreCompleto;
}
