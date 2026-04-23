using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace ConquiTap.Helpers;

public static class DatabaseHelper
{
    private static string? _connectionString;

    public static string ConnectionString
    {
        get
        {
            _connectionString ??= ConfigurationManager.ConnectionStrings["ConquiTapDB"]?.ConnectionString
                ?? "Server=localhost\\SQLEXPRESS;Database=ConquiTap;Trusted_Connection=True;TrustServerCertificate=True;";
            return _connectionString;
        }
        set => _connectionString = value;
    }

    public static SqlConnection GetConnection() => new(ConnectionString);

    // ─── Verificaciones iniciales ────────────────────────────────────────────

    public static bool TestConnection()
    {
        try
        {
            using var conn = GetConnection();
            conn.Open();
            return true;
        }
        catch { return false; }
    }

    public static bool AdminExists()
    {
        try
        {
            const string sql = "SELECT COUNT(*) FROM Usuarios WHERE Categoria = 'Administrador' AND Activo = 1";
            var result = ExecuteScalar(sql, []);
            return result is int count && count > 0;
        }
        catch { return false; }
    }

    // ─── Métodos de ejecución ────────────────────────────────────────────────

    public static int ExecuteNonQuery(string sql, Dictionary<string, object?> parameters)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = BuildCommand(conn, sql, parameters);
        return cmd.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(string sql, Dictionary<string, object?> parameters)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = BuildCommand(conn, sql, parameters);
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value ? null : result;
    }

    public static DataTable ExecuteQuery(string sql, Dictionary<string, object?> parameters)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = BuildCommand(conn, sql, parameters);
        using var adapter = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);
        return dt;
    }

    public static DataTable ExecuteStoredProcedure(string procedureName, Dictionary<string, object?>? parameters = null)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(procedureName, conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        if (parameters != null)
            foreach (var p in parameters)
                cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
        using var adapter = new SqlDataAdapter(cmd);
        var dt = new DataTable();
        adapter.Fill(dt);
        return dt;
    }

    public static void LogActividad(int? usuarioId, string accion, string? tabla = null, int? registroId = null)
    {
        try
        {
            const string sql = @"
                INSERT INTO LogActividad (UsuarioId, Accion, Tabla, RegistroId)
                VALUES (@UsuarioId, @Accion, @Tabla, @RegistroId)";
            ExecuteNonQuery(sql, new()
            {
                ["@UsuarioId"]  = usuarioId,
                ["@Accion"]     = accion,
                ["@Tabla"]      = tabla,
                ["@RegistroId"] = registroId
            });
        }
        catch { /* Log errors should not crash the app */ }
    }

    // ─── Privado ─────────────────────────────────────────────────────────────

    private static SqlCommand BuildCommand(SqlConnection conn, string sql, Dictionary<string, object?> parameters)
    {
        var cmd = new SqlCommand(sql, conn);
        foreach (var p in parameters)
            cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
        return cmd;
    }
}
