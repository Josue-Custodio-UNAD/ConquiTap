using ConquiTap.Helpers;
using ConquiTap.Models;

namespace ConquiTap.Repositories;

public class UsuarioRepository
{
    // ─── Autenticación ───────────────────────────────────────────────────────

    public Usuario? Autenticar(string nombreUsuario, string contrasena)
    {
        const string sql = @"
            SELECT Id, NombreUsuario, ContrasenaHash, Correo, Categoria, Activo, FechaCreacion, UltimoAcceso
            FROM Usuarios
            WHERE NombreUsuario = @Usuario AND Activo = 1";

        var dt = DatabaseHelper.ExecuteQuery(sql, new() { ["@Usuario"] = nombreUsuario });
        if (dt.Rows.Count == 0) return null;

        var row = dt.Rows[0];
        string hash = row["ContrasenaHash"].ToString()!;

        if (!PasswordHelper.VerifyPassword(contrasena, hash)) return null;

        // Actualizar último acceso
        DatabaseHelper.ExecuteNonQuery(
            "UPDATE Usuarios SET UltimoAcceso = GETDATE() WHERE Id = @Id",
            new() { ["@Id"] = row["Id"] });

        return MapRow(row);
    }

    public bool ExisteNombreUsuario(string nombreUsuario, int excluirId = 0)
    {
        const string sql = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @U AND Id <> @Id";
        var result = DatabaseHelper.ExecuteScalar(sql, new() { ["@U"] = nombreUsuario, ["@Id"] = excluirId });
        return result is int n && n > 0;
    }

    // ─── CRUD ────────────────────────────────────────────────────────────────

    public List<Usuario> ObtenerTodos()
    {
        const string sql = @"
            SELECT Id, NombreUsuario, ContrasenaHash, Correo, Categoria, Activo, FechaCreacion, UltimoAcceso
            FROM Usuarios ORDER BY Categoria, NombreUsuario";

        var dt = DatabaseHelper.ExecuteQuery(sql, []);
        return dt.Rows.Cast<System.Data.DataRow>().Select(MapRow).ToList();
    }

    public Usuario? ObtenerPorId(int id)
    {
        const string sql = @"
            SELECT Id, NombreUsuario, ContrasenaHash, Correo, Categoria, Activo, FechaCreacion, UltimoAcceso
            FROM Usuarios WHERE Id = @Id";

        var dt = DatabaseHelper.ExecuteQuery(sql, new() { ["@Id"] = id });
        return dt.Rows.Count > 0 ? MapRow(dt.Rows[0]) : null;
    }

    public int Crear(Usuario u, string contrasena)
    {
        string hash = PasswordHelper.HashPassword(contrasena);
        const string sql = @"
            INSERT INTO Usuarios (NombreUsuario, ContrasenaHash, Correo, Categoria)
            OUTPUT INSERTED.Id
            VALUES (@U, @H, @Correo, @Cat)";

        var result = DatabaseHelper.ExecuteScalar(sql, new()
        {
            ["@U"]     = u.NombreUsuario,
            ["@H"]     = hash,
            ["@Correo"]= u.Correo,
            ["@Cat"]   = u.Categoria
        });
        return result is int id ? id : 0;
    }

    public bool Actualizar(Usuario u)
    {
        const string sql = @"
            UPDATE Usuarios
            SET NombreUsuario = @U, Correo = @Correo, Categoria = @Cat, Activo = @Activo
            WHERE Id = @Id";

        return DatabaseHelper.ExecuteNonQuery(sql, new()
        {
            ["@U"]     = u.NombreUsuario,
            ["@Correo"]= u.Correo,
            ["@Cat"]   = u.Categoria,
            ["@Activo"]= u.Activo,
            ["@Id"]    = u.Id
        }) > 0;
    }

    public bool CambiarContrasena(int usuarioId, string nuevaContrasena)
    {
        string hash = PasswordHelper.HashPassword(nuevaContrasena);
        return DatabaseHelper.ExecuteNonQuery(
            "UPDATE Usuarios SET ContrasenaHash = @H WHERE Id = @Id",
            new() { ["@H"] = hash, ["@Id"] = usuarioId }) > 0;
    }

    public bool Desactivar(int id)
    {
        return DatabaseHelper.ExecuteNonQuery(
            "UPDATE Usuarios SET Activo = 0 WHERE Id = @Id",
            new() { ["@Id"] = id }) > 0;
    }

    // ─── Mapeo ───────────────────────────────────────────────────────────────

    private static Usuario MapRow(System.Data.DataRow row) => new()
    {
        Id             = (int)row["Id"],
        NombreUsuario  = row["NombreUsuario"].ToString()!,
        ContrasenaHash = row["ContrasenaHash"].ToString()!,
        Correo         = row["Correo"] == DBNull.Value ? "" : row["Correo"].ToString()!,
        Categoria      = row["Categoria"].ToString()!,
        Activo         = (bool)row["Activo"],
        FechaCreacion  = (DateTime)row["FechaCreacion"],
        UltimoAcceso   = row["UltimoAcceso"] == DBNull.Value ? null : (DateTime?)row["UltimoAcceso"]
    };
}
