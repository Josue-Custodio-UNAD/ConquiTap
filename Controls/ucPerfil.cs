using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Controls;

public class ucPerfil : UserControl
{
    private readonly UsuarioRepository _uRepo = new();
    private readonly MiembroRepository _mRepo = new();
    private Label   lblStatus      = null!;
    private TextBox txtContrasena  = null!;
    private TextBox txtNuevaCon    = null!;
    private TextBox txtConfirmarCon= null!;

    public ucPerfil()
    {
        BackColor = AppColors.MainBg;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;

        var usr = SessionManager.UsuarioActual!;
        var mbr = SessionManager.MiembroActual;

        // ── Avatar card ──────────────────────────────────────────────────
        var pnlAvatar = new Panel
        {
            BackColor = Color.White,
            Bounds    = new Rectangle(0, 0, 340, 200)
        };

        var lblCircle = new Label
        {
            Text      = usr.NombreUsuario.Length > 0 ? usr.NombreUsuario[0].ToString().ToUpper() : "?",
            Font      = new Font("Segoe UI", 36, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = AppColors.Denim,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(20, 20, 80, 80)
        };

        pnlAvatar.Controls.Add(lblCircle);
        pnlAvatar.Controls.Add(new Label { Text = SessionManager.NombreParaMostrar, Font = new Font("Segoe UI", 13, FontStyle.Bold), ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(116, 28) });
        pnlAvatar.Controls.Add(new Label { Text = SessionManager.RolParaMostrar,   Font = AppColors.FontBody, ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(116, 56) });
        pnlAvatar.Controls.Add(new Label { Text = "Usuario: " + usr.NombreUsuario, Font = AppColors.FontSmall, ForeColor = AppColors.TextMuted, AutoSize = true, Location = new Point(116, 80) });
        pnlAvatar.Controls.Add(new Label { Text = "Correo: " + (usr.Correo.Length > 0 ? usr.Correo : "—"), Font = AppColors.FontSmall, ForeColor = AppColors.TextMuted, AutoSize = true, Location = new Point(116, 100) });
        pnlAvatar.Controls.Add(new Label { Text = "Desde: " + usr.FechaCreacion.ToString("dd/MM/yyyy"), Font = AppColors.FontSmall, ForeColor = AppColors.TextMuted, AutoSize = true, Location = new Point(116, 120) });

        Controls.Add(pnlAvatar);

        // ── Datos de miembro (si existe) ──────────────────────────────────
        if (mbr != null)
        {
            var pnlMbr = new Panel { BackColor = Color.White, Bounds = new Rectangle(0, 212, 340, 220) };
            pnlMbr.Controls.Add(SectionTitle("Datos del Club", 8, 8));

            int y = 36;
            void Row(string lbl, string val) {
                pnlMbr.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(12, y) });
                pnlMbr.Controls.Add(new Label { Text = val.Length > 0 ? val : "—", Font = AppColors.FontSmall, ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(130, y) });
                y += 22;
            }
            Row("Club:", mbr.ClubNombre);
            Row("Clase:", mbr.Clase);
            Row("Puesto:", mbr.Puesto);
            Row("Categoría:", mbr.Categoria);
            Row("Iglesia:", mbr.IglesiaNombre);
            Row("Investidura:", mbr.FechaInvestidura?.ToString("dd/MM/yyyy") ?? "");
            Controls.Add(pnlMbr);
        }

        // ── Cambiar contraseña ────────────────────────────────────────────
        int panelX = 352;
        var pnlPass = new Panel { BackColor = Color.White, Bounds = new Rectangle(panelX, 0, 360, 300) };
        pnlPass.Controls.Add(SectionTitle("Cambiar Contraseña", 16, 14));

        int py = 44;

        void AddPassField(string lbl, ref TextBox field, int yy) {
            pnlPass.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9), ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(16, yy) });
            field = new TextBox { Location = new Point(16, yy + 20), Width = 320, Font = AppColors.FontBody, BorderStyle = BorderStyle.FixedSingle, UseSystemPasswordChar = true };
            pnlPass.Controls.Add(field);
        }

        AddPassField("Contraseña actual",      ref txtContrasena,   py);   py += 52;
        AddPassField("Nueva contraseña",        ref txtNuevaCon,     py);   py += 52;
        AddPassField("Confirmar nueva",         ref txtConfirmarCon, py);   py += 52;

        lblStatus = new Label { Bounds = new Rectangle(16, py, 320, 22), Font = new Font("Segoe UI", 9), ForeColor = AppColors.Danger };
        pnlPass.Controls.Add(lblStatus);
        py += 28;

        var btnCambiar = new Button
        {
            Text      = "Cambiar Contraseña",
            Bounds    = new Rectangle(16, py, 200, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Denim,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnCambiar.FlatAppearance.BorderSize = 0;
        btnCambiar.Click += BtnCambiar_Click;
        pnlPass.Controls.Add(btnCambiar);

        Controls.Add(pnlPass);
    }

    private static Label SectionTitle(string text, int x, int y) => new()
    {
        Text      = text,
        Font      = new Font("Segoe UI", 11, FontStyle.Bold),
        ForeColor = AppColors.Denim,
        AutoSize  = true,
        Location  = new Point(x, y)
    };

    private void BtnCambiar_Click(object? sender, EventArgs e)
    {
        lblStatus.ForeColor = AppColors.Danger;
        lblStatus.Text = "";

        string actual   = txtContrasena.Text;
        string nueva    = txtNuevaCon.Text;
        string confirmar= txtConfirmarCon.Text;

        var usr = SessionManager.UsuarioActual!;
        if (!PasswordHelper.VerifyPassword(actual, usr.ContrasenaHash))
        {
            lblStatus.Text = "La contraseña actual es incorrecta."; return;
        }
        if (!PasswordHelper.IsPasswordValid(nueva, out string msg))
        {
            lblStatus.Text = msg; return;
        }
        if (nueva != confirmar)
        {
            lblStatus.Text = "Las contraseñas nuevas no coinciden."; return;
        }

        try
        {
            new UsuarioRepository().CambiarContrasena(usr.Id, nueva);
            // Actualizar el hash en sesión
            usr.ContrasenaHash = PasswordHelper.HashPassword(nueva);
            lblStatus.ForeColor = AppColors.Success;
            lblStatus.Text = "✓ Contraseña actualizada correctamente.";
            txtContrasena.Clear(); txtNuevaCon.Clear(); txtConfirmarCon.Clear();
            DatabaseHelper.LogActividad(usr.Id, "Cambió contraseña");
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Error: " + ex.Message;
        }
    }
}
