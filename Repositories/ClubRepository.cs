using ConquiTap.Helpers;
using ConquiTap.Models;
using System.Data;

namespace ConquiTap.Repositories;

public class ClubRepository
{
    private const string SelectBase = @"
        SELECT c.Id, c.Nombre, c.IglesiaId, ISNULL(ig.Nombre,'') AS IglesiaNombre,
               c.DirectorId, ISNULL(m.Nombres+' '+m.Apellidos,'') AS DirectorNombre,
               c.AnoFundacion, c.TipoClub, c.Activo,
               (SELECT COUNT(*) FROM Miembros mb WHERE mb.ClubId = c.Id AND mb.Activo=1) AS TotalMiembros
        FROM Clubes c
        LEFT JOIN Iglesias ig ON c.IglesiaId  = ig.Id
        LEFT JOIN Miembros m  ON c.DirectorId = m.Id";

    public List<Club> ObtenerTodos(bool soloActivos = true)
    {
        string where = soloActivos ? " WHERE c.Activo = 1" : "";
        var dt = DatabaseHelper.ExecuteQuery(SelectBase + where + " ORDER BY c.TipoClub, c.Nombre", []);
        return dt.Rows.Cast<DataRow>().Select(MapRow).ToList();
    }

    public List<Club> Buscar(string termino, string? tipo = null)
    {
        string sql = SelectBase + @"
            WHERE c.Activo = 1
              AND (@T IS NULL OR c.Nombre LIKE '%'+@T+'%')
              AND (@Tipo IS NULL OR c.TipoClub = @Tipo)
            ORDER BY c.TipoClub, c.Nombre";

        var dt = DatabaseHelper.ExecuteQuery(sql, new()
        {
            ["@T"]   = string.IsNullOrWhiteSpace(termino) ? (object?)null : termino,
            ["@Tipo"]= string.IsNullOrWhiteSpace(tipo)    ? (object?)null : tipo
        });
        return dt.Rows.Cast<DataRow>().Select(MapRow).ToList();
    }

    public Club? ObtenerPorId(int id)
    {
        var dt = DatabaseHelper.ExecuteQuery(SelectBase + " WHERE c.Id = @Id", new() { ["@Id"] = id });
        return dt.Rows.Count > 0 ? MapRow(dt.Rows[0]) : null;
    }

    public int Crear(Club c)
    {
        const string sql = @"
            INSERT INTO Clubes (Nombre, IglesiaId, DirectorId, AnoFundacion, TipoClub)
            OUTPUT INSERTED.Id
            VALUES (@Nombre, @IglId, @DirId, @Ano, @Tipo)";

        var result = DatabaseHelper.ExecuteScalar(sql, BuildParams(c));
        return result is int id ? id : 0;
    }

    public bool Actualizar(Club c)
    {
        const string sql = @"
            UPDATE Clubes SET
              Nombre=@Nombre, IglesiaId=@IglId, DirectorId=@DirId,
              AnoFundacion=@Ano, TipoClub=@Tipo
            WHERE Id=@Id";

        var p = BuildParams(c);
        p["@Id"] = c.Id;
        return DatabaseHelper.ExecuteNonQuery(sql, p) > 0;
    }

    public bool Desactivar(int id)
    {
        return DatabaseHelper.ExecuteNonQuery(
            "UPDATE Clubes SET Activo = 0 WHERE Id = @Id",
            new() { ["@Id"] = id }) > 0;
    }

    public List<Iglesia> ObtenerIglesias()
    {
        var dt = DatabaseHelper.ExecuteQuery("SELECT Id, Nombre FROM Iglesias WHERE Activo=1 ORDER BY Nombre", []);
        return dt.Rows.Cast<DataRow>().Select(r => new Iglesia { Id=(int)r["Id"], Nombre=r["Nombre"].ToString()! }).ToList();
    }

    public List<Miembro> ObtenerDirectoresPosibles()
    {
        const string sql = @"
            SELECT Id, Nombres, Apellidos FROM Miembros
            WHERE Activo=1 AND Categoria='Directivo'
            ORDER BY Apellidos, Nombres";
        var dt = DatabaseHelper.ExecuteQuery(sql, []);
        return dt.Rows.Cast<DataRow>().Select(r => new Miembro
        {
            Id       = (int)r["Id"],
            Nombres  = r["Nombres"].ToString()!,
            Apellidos= r["Apellidos"].ToString()!
        }).ToList();
    }

    private static Dictionary<string, object?> BuildParams(Club c) => new()
    {
        ["@Nombre"]= c.Nombre,
        ["@IglId"] = c.IglesiaId,
        ["@DirId"] = c.DirectorId,
        ["@Ano"]   = c.AnoFundacion,
        ["@Tipo"]  = c.TipoClub
    };

    private static Club MapRow(DataRow r) => new()
    {
        Id            = (int)r["Id"],
        Nombre        = r["Nombre"].ToString()!,
        IglesiaId     = r["IglesiaId"]  == DBNull.Value ? null : (int?)r["IglesiaId"],
        IglesiaNombre = r["IglesiaNombre"].ToString()!,
        DirectorId    = r["DirectorId"] == DBNull.Value ? null : (int?)r["DirectorId"],
        DirectorNombre= r["DirectorNombre"].ToString()!,
        AnoFundacion  = r["AnoFundacion"]== DBNull.Value? null : (int?)r["AnoFundacion"],
        TipoClub      = r["TipoClub"].ToString()!,
        Activo        = (bool)r["Activo"],
        TotalMiembros = (int)r["TotalMiembros"]
    };
}
