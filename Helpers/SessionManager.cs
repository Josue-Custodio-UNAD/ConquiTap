using ConquiTap.Models;

namespace ConquiTap.Helpers;

/// <summary>
/// Mantiene el estado de la sesión actual del usuario.
/// </summary>
public static class SessionManager
{
    public static Usuario? UsuarioActual { get; private set; }
    public static Miembro? MiembroActual { get; private set; }

    public static bool EstaAutenticado    => UsuarioActual != null;
    public static bool EsAdministrador   => UsuarioActual?.Categoria == "Administrador";
    public static bool EsDirectivo       => UsuarioActual?.Categoria is "Directivo" or "Administrador";
    public static bool EsMiembroSimple   => UsuarioActual?.Categoria == "Miembro";

    public static void IniciarSesion(Usuario usuario, Miembro? miembro = null)
    {
        UsuarioActual = usuario;
        MiembroActual = miembro;
    }

    public static void CerrarSesion()
    {
        UsuarioActual = null;
        MiembroActual = null;
    }

    public static string NombreParaMostrar =>
        MiembroActual != null
            ? $"{MiembroActual.Nombres} {MiembroActual.Apellidos}"
            : UsuarioActual?.NombreUsuario ?? "Usuario";

    public static string RolParaMostrar => UsuarioActual?.Categoria switch
    {
        "Administrador" => "Administrador del Sistema",
        "Directivo"     => "Directivo de Club",
        "Miembro"       => "Miembro de Club",
        _               => "Invitado"
    };
}
