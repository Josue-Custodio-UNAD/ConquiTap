using ConquiTap.Helpers;
using ConquiTap.Models;
using System.Data;

namespace ConquiTap.Repositories;

public class MiembroRepository
{
    // ─── Consultas ───────────────────────────────────────────────────────────

    private const string SelectBase = @"
        SELECT m.Id, m.Nombres, m.Apellidos, m.Cedula, m.Telefono, m.Correo,
               m.UsuarioId, m.AsociacionId, a.Nombre AS AsociacionNombre,
               m.ZonaId, z.Nombre AS ZonaNombre,
               m.DistritoId, d.Nombre AS DistritoNombre,
               m.IglesiaId, ig.Nombre AS IglesiaNombre,
               m.Categoria, m.Puesto, m.Clase,
               m.FechaInvestidura, m.ClubId, c.Nombre AS ClubNombre,
               m.Sexo, m.FechaNacimiento, m.FechaBautismo,
               m.Activo, m.FechaRegistro
        FROM Miembros m
        LEFT JOIN Asociaciones a ON m.AsociacionId = a.Id
        LEFT JOIN Zonas z        ON m.ZonaId       = z.Id
        LEFT JOIN Distritos d    ON m.DistritoId    = d.Id
        LEFT JOIN Iglesias ig    ON m.IglesiaId     = ig.Id
        LEFT JOIN Clubes c       ON m.ClubId        = c.Id";

    public List<Miembro> ObtenerTodos(bool soloActivos = true)
    {
        string where = soloActivos ? " WHERE m.Activo = 1" : "";
        var dt = DatabaseHelper.ExecuteQuery(SelectBase + where + " ORDER BY m.Apellidos, m.Nombres", []);
        return dt.Rows.Cast<DataRow>().Select(MapRow).ToList();
    }

    public List<Miembro> Buscar(string termino, int? clubId = null, string? categoria = null)
    {
        string sql = SelectBase + @"
            WHERE m.Activo = 1
              AND (@T IS NULL OR m.Nombres + ' ' + m.Apellidos LIKE '%' + @T + '%'
                   OR m.Cedula LIKE '%' + @T + '%')
              AND (@ClubId IS NULL OR m.ClubId = @ClubId)
              AND (@Cat    IS NULL OR m.Categoria = @Cat)
            ORDER BY m.Apellidos, m.Nombres";

        var dt = DatabaseHelper.ExecuteQuery(sql, new()
        {
            ["@T"]     = string.IsNullOrWhiteSpace(termino) ? (object?)null : termino,
            ["@ClubId"]= clubId,
            ["@Cat"]   = string.IsNullOrWhiteSpace(categoria) ? (object?)null : categoria
        });
        return dt.Rows.Cast<DataRow>().Select(MapRow).ToList();
    }

    public Miembro? ObtenerPorId(int id)
    {
        var dt = DatabaseHelper.ExecuteQuery(SelectBase + " WHERE m.Id = @Id", new() { ["@Id"] = id });
        if (dt.Rows.Count == 0) return null;
        var m = MapRow(dt.Rows[0]);
        m.Especialidades = ObtenerEspecialidades(id);
        return m;
    }

    public Miembro? ObtenerPorUsuarioId(int usuarioId)
    {
        var dt = DatabaseHelper.ExecuteQuery(SelectBase + " WHERE m.UsuarioId = @U", new() { ["@U"] = usuarioId });
        return dt.Rows.Count > 0 ? MapRow(dt.Rows[0]) : null;
    }

    public List<Especialidad> ObtenerEspecialidades(int miembroId)
    {
        const string sql = @"
            SELECT e.Id, e.Nombre, e.Categoria
            FROM Especialidades e
            INNER JOIN MiembroEspecialidades me ON e.Id = me.EspecialidadId
            WHERE me.MiembroId = @Id ORDER BY e.Nombre";
        var dt = DatabaseHelper.ExecuteQuery(sql, new() { ["@Id"] = miembroId });
        return dt.Rows.Cast<DataRow>().Select(r => new Especialidad
        {
            Id       = (int)r["Id"],
            Nombre   = r["Nombre"].ToString()!,
            Categoria= r["Categoria"].ToString()!
        }).ToList();
    }

    // ─── CRUD ────────────────────────────────────────────────────────────────

    public int Crear(Miembro m)
    {
        const string sql = @"
            INSERT INTO Miembros
              (Nombres, Apellidos, Cedula, Telefono, Correo, UsuarioId,
               AsociacionId, ZonaId, DistritoId, IglesiaId, Categoria,
               Puesto, Clase, FechaInvestidura, ClubId, Sexo, FechaNacimiento, FechaBautismo)
            OUTPUT INSERTED.Id
            VALUES
              (@Nombres, @Apellidos, @Cedula, @Tel, @Correo, @UsrId,
               @AsocId, @ZonaId, @DistId, @IglId, @Cat,
               @Puesto, @Clase, @Investidura, @ClubId, @Sexo, @Nac, @Baut)";

        var result = DatabaseHelper.ExecuteScalar(sql, BuildParams(m));
        return result is int id ? id : 0;
    }

    public bool Actualizar(Miembro m)
    {
        const string sql = @"
            UPDATE Miembros SET
              Nombres=@Nombres, Apellidos=@Apellidos, Cedula=@Cedula,
              Telefono=@Tel, Correo=@Correo, UsuarioId=@UsrId,
              AsociacionId=@AsocId, ZonaId=@ZonaId, DistritoId=@DistId, IglesiaId=@IglId,
              Categoria=@Cat, Puesto=@Puesto, Clase=@Clase,
              FechaInvestidura=@Investidura, ClubId=@ClubId, Sexo=@Sexo,
              FechaNacimiento=@Nac, FechaBautismo=@Baut
            WHERE Id=@Id";

        var p = BuildParams(m);
        p["@Id"] = m.Id;
        return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
    }

    public bool Desactivar(int id)
    {
        return DatabaseHelper.ExecuteNonQuery(
            "UPDATE Miembros SET Activo = 0 WHERE Id = @Id",
            new() { ["@Id"] = id }) > 0;
    }

    public void GuardarEspecialidades(int miembroId, List<int> especialidadIds)
    {
        DatabaseHelper.ExecuteNonQuery(
            "DELETE FROM MiembroEspecialidades WHERE MiembroId = @Id",
            new() { ["@Id"] = miembroId });

        foreach (var espId in especialidadIds)
        {
            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO MiembroEspecialidades (MiembroId, EspecialidadId) VALUES (@M, @E)",
                new() { ["@M"] = miembroId, ["@E"] = espId });
        }
    }

    // ─── Catálogos ───────────────────────────────────────────────────────────

    public List<Asociacion> ObtenerAsociaciones()
    {
        var dt = DatabaseHelper.ExecuteQuery("SELECT Id, Nombre FROM Asociaciones WHERE Activo=1 ORDER BY Nombre", []);
        return dt.Rows.Cast<DataRow>().Select(r => new Asociacion { Id = (int)r["Id"], Nombre = r["Nombre"].ToString()! }).ToList();
    }

    public List<Zona> ObtenerZonas(int? asociacionId = null)
    {
        string sql = "SELECT Id, Nombre, AsociacionId FROM Zonas WHERE Activo=1" +
                     (asociacionId.HasValue ? " AND AsociacionId=@A" : "") + " ORDER BY Nombre";
        var dt = DatabaseHelper.ExecuteQuery(sql, asociacionId.HasValue ? new() { ["@A"] = asociacionId } : []);
        return dt.Rows.Cast<DataRow>().Select(r => new Zona { Id=(int)r["Id"], Nombre=r["Nombre"].ToString()!, AsociacionId=(int)r["AsociacionId"] }).ToList();
    }

    public List<Distrito> ObtenerDistritos(int? zonaId = null)
    {
        string sql = "SELECT Id, Nombre, ZonaId FROM Distritos WHERE Activo=1" +
                     (zonaId.HasValue ? " AND ZonaId=@Z" : "") + " ORDER BY Nombre";
        var dt = DatabaseHelper.ExecuteQuery(sql, zonaId.HasValue ? new() { ["@Z"] = zonaId } : []);
        return dt.Rows.Cast<DataRow>().Select(r => new Distrito { Id=(int)r["Id"], Nombre=r["Nombre"].ToString()!, ZonaId=(int)r["ZonaId"] }).ToList();
    }

    public List<Iglesia> ObtenerIglesias(int? distritoId = null)
    {
        string sql = "SELECT Id, Nombre, DistritoId FROM Iglesias WHERE Activo=1" +
                     (distritoId.HasValue ? " AND DistritoId=@D" : "") + " ORDER BY Nombre";
        var dt = DatabaseHelper.ExecuteQuery(sql, distritoId.HasValue ? new() { ["@D"] = distritoId } : []);
        return dt.Rows.Cast<DataRow>().Select(r => new Iglesia { Id=(int)r["Id"], Nombre=r["Nombre"].ToString()!, DistritoId=(int)r["DistritoId"] }).ToList();
    }

    public List<Especialidad> ObtenerTodasEspecialidades()
    {
        var dt = DatabaseHelper.ExecuteQuery("SELECT Id, Nombre, Categoria FROM Especialidades WHERE Activo=1 ORDER BY Categoria, Nombre", []);
        return dt.Rows.Cast<DataRow>().Select(r => new Especialidad { Id=(int)r["Id"], Nombre=r["Nombre"].ToString()!, Categoria=r["Categoria"].ToString()! }).ToList();
    }

    // ─── Privado ─────────────────────────────────────────────────────────────

    private static Dictionary<string, object?> BuildParams(Miembro m) => new()
    {
        ["@Nombres"]    = m.Nombres,
        ["@Apellidos"]  = m.Apellidos,
        ["@Cedula"]     = string.IsNullOrEmpty(m.Cedula)    ? null : m.Cedula,
        ["@Tel"]        = string.IsNullOrEmpty(m.Telefono)  ? null : m.Telefono,
        ["@Correo"]     = string.IsNullOrEmpty(m.Correo)    ? null : m.Correo,
        ["@UsrId"]      = m.UsuarioId,
        ["@AsocId"]     = m.AsociacionId,
        ["@ZonaId"]     = m.ZonaId,
        ["@DistId"]     = m.DistritoId,
        ["@IglId"]      = m.IglesiaId,
        ["@Cat"]        = m.Categoria,
        ["@Puesto"]     = string.IsNullOrEmpty(m.Puesto)    ? null : m.Puesto,
        ["@Clase"]      = string.IsNullOrEmpty(m.Clase)     ? null : m.Clase,
        ["@Investidura"]= m.FechaInvestidura,
        ["@ClubId"]     = m.ClubId,
        ["@Sexo"]       = string.IsNullOrEmpty(m.Sexo)      ? null : m.Sexo,
        ["@Nac"]        = m.FechaNacimiento,
        ["@Baut"]       = m.FechaBautismo
    };

    private static Miembro MapRow(DataRow r) => new()
    {
        Id               = (int)r["Id"],
        Nombres          = r["Nombres"].ToString()!,
        Apellidos        = r["Apellidos"].ToString()!,
        Cedula           = r["Cedula"]    == DBNull.Value ? "" : r["Cedula"].ToString()!,
        Telefono         = r["Telefono"]  == DBNull.Value ? "" : r["Telefono"].ToString()!,
        Correo           = r["Correo"]    == DBNull.Value ? "" : r["Correo"].ToString()!,
        UsuarioId        = r["UsuarioId"] == DBNull.Value ? null : (int?)r["UsuarioId"],
        AsociacionId     = r["AsociacionId"]== DBNull.Value? null: (int?)r["AsociacionId"],
        AsociacionNombre = r["AsociacionNombre"]== DBNull.Value? "": r["AsociacionNombre"].ToString()!,
        ZonaId           = r["ZonaId"]    == DBNull.Value ? null : (int?)r["ZonaId"],
        ZonaNombre       = r["ZonaNombre"]== DBNull.Value ? "" : r["ZonaNombre"].ToString()!,
        DistritoId       = r["DistritoId"]== DBNull.Value ? null : (int?)r["DistritoId"],
        DistritoNombre   = r["DistritoNombre"]== DBNull.Value? "": r["DistritoNombre"].ToString()!,
        IglesiaId        = r["IglesiaId"] == DBNull.Value ? null : (int?)r["IglesiaId"],
        IglesiaNombre    = r["IglesiaNombre"]== DBNull.Value? "": r["IglesiaNombre"].ToString()!,
        Categoria        = r["Categoria"].ToString()!,
        Puesto           = r["Puesto"]    == DBNull.Value ? "" : r["Puesto"].ToString()!,
        Clase            = r["Clase"]     == DBNull.Value ? "" : r["Clase"].ToString()!,
        FechaInvestidura = r["FechaInvestidura"]== DBNull.Value? null: (DateTime?)r["FechaInvestidura"],
        ClubId           = r["ClubId"]    == DBNull.Value ? null : (int?)r["ClubId"],
        ClubNombre       = r["ClubNombre"]== DBNull.Value ? "" : r["ClubNombre"].ToString()!,
        Sexo             = r["Sexo"]      == DBNull.Value ? "" : r["Sexo"].ToString()!,
        FechaNacimiento  = r["FechaNacimiento"]== DBNull.Value? null: (DateTime?)r["FechaNacimiento"],
        FechaBautismo    = r["FechaBautismo"]== DBNull.Value? null: (DateTime?)r["FechaBautismo"],
        Activo           = (bool)r["Activo"],
        FechaRegistro    = (DateTime)r["FechaRegistro"]
    };
}
