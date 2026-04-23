using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Forms;

public class frmLogin : Form
{
    private TextBox txtUsuario = null!;
    private TextBox txtContrasena = null!;
    private Button  btnLogin = null!;
    private Button  btnMostrarContrasena = null!;
    private Label   lblError = null!;
    private Panel   pnlCard = null!;

    public frmLogin()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // ── Ventana ──────────────────────────────────────────────────────────
        Text            = "ConquiTap — Iniciar Sesión";
        Size            = new Size(420, 560);
        MinimumSize     = Size;
        MaximumSize     = Size;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = AppColors.Denim;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        Font            = AppColors.FontBody;

        // ── Fondo superior (logo) ─────────────────────────────────────────
        var pnlTop = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 160,
            BackColor = Color.Transparent
        };


        var lblIcon = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 40, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 20, 420, 70)
        };

        var lblAppName = new Label
        {
            Text      = "ConquiTap",
            Font      = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 88, 420, 36)
        };

        var lblSubtitle = new Label
        {
            Text      = "Sistema de Gestión de Clubes - IASD, ADONE",
            Font      = new Font("Segoe UI", 9),
            ForeColor = AppColors.SidebarSubtext,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 122, 420, 24)
        };

        pnlTop.Controls.AddRange(new Control[] { lblIcon, lblAppName, lblSubtitle });

        // ── Tarjeta de login ──────────────────────────────────────────────
        pnlCard = new Panel
        {
            BackColor    = Color.White,
            Bounds       = new Rectangle(24, 168, 372, 340),
            Padding      = new Padding(28)
        };

        // Redondear la tarjeta (región redondeada)
        pnlCard.Paint += (s, e) =>
        {
            e.Graphics.Clear(Color.White);
        };

        int y = 28;
        int w = 316;
        int x = 28;

        // Usuario
        var lblUsuario = new Label
        {
            Text      = "Usuario",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = AppColors.TextSecondary,
            Bounds    = new Rectangle(x, y, w, 20)
        };
        y += 22;

        txtUsuario = new TextBox
        {
            Bounds      = new Rectangle(x, y, w, 32),
            Font        = new Font("Segoe UI", 11),
            BorderStyle = BorderStyle.FixedSingle,
            ForeColor   = AppColors.TextPrimary,
            PlaceholderText = "Ingresa tu usuario"
        };
        y += 44;

        // Contraseña
        var lblContrasena = new Label
        {
            Text      = "Contraseña",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = AppColors.TextSecondary,
            Bounds    = new Rectangle(x, y, w, 20)
        };
        y += 22;

        txtContrasena = new TextBox
        {
            Bounds          = new Rectangle(x, y, w - 40, 32),
            Font            = new Font("Segoe UI", 11),
            BorderStyle     = BorderStyle.FixedSingle,
            ForeColor       = AppColors.TextPrimary,
            UseSystemPasswordChar = true,
            PlaceholderText = "Ingresa tu contraseña"
        };

        btnMostrarContrasena = new Button
        {
            Text      = "👁",
            Bounds    = new Rectangle(x + w - 36, y, 36, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.BorderLight,
            ForeColor = AppColors.TextSecondary,
            Font      = new Font("Segoe UI", 11),
            Cursor    = Cursors.Hand
        };
        btnMostrarContrasena.FlatAppearance.BorderSize = 0;
        btnMostrarContrasena.Click += (_, _) =>
        {
            txtContrasena.UseSystemPasswordChar = !txtContrasena.UseSystemPasswordChar;
            btnMostrarContrasena.Text = txtContrasena.UseSystemPasswordChar ? "👁" : "🙈";
        };
        y += 44;

        // Error
        lblError = new Label
        {
            Text      = "",
            Font      = new Font("Segoe UI", 9),
            ForeColor = AppColors.Danger,
            Bounds    = new Rectangle(x, y, w, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        y += 28;

        // Botón Login
        btnLogin = new Button
        {
            Text      = "Iniciar Sesión",
            Bounds    = new Rectangle(x, y, w, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Denim,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Click += BtnLogin_Click;
        y += 54;

        // Versión
        var lblVersion = new Label
        {
            Text      = $"v{System.Configuration.ConfigurationManager.AppSettings["AppVersion"]}",
            Font      = new Font("Segoe UI", 8),
            ForeColor = AppColors.TextMuted,
            Bounds    = new Rectangle(x, y, w, 18),
            TextAlign = ContentAlignment.MiddleCenter
        };

        pnlCard.Controls.AddRange(new Control[]
        {
            lblUsuario, txtUsuario,
            lblContrasena, txtContrasena, btnMostrarContrasena,
            lblError, btnLogin, lblVersion
        });

        Controls.AddRange(new Control[] { pnlTop, pnlCard });

        // Copiar derechos al fondo
        var lblCopy = new Label
        {
            Text      = "© 2024 ADONE — Iglesia Adventista del Séptimo Día",
            Font      = new Font("Segoe UI", 8),
            ForeColor = AppColors.SidebarSubtext,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(0, 520, 420, 22)
        };
        Controls.Add(lblCopy);

        // Enter para login
        AcceptButton = btnLogin;

        // Hover effect on card
        ApplyCardShadow();
    }

    private void ApplyCardShadow()
    {
        pnlCard.Paint += (sender, e) =>
        {
            var g = e.Graphics;
            using var pen = new Pen(AppColors.BorderLight, 1);
            g.DrawRectangle(pen, 0, 0, pnlCard.Width - 1, pnlCard.Height - 1);
        };
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        lblError.Text = "";
        string usuario    = txtUsuario.Text.Trim();
        string contrasena = txtContrasena.Text;

        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
        {
            lblError.Text = "Por favor complete todos los campos.";
            return;
        }

        btnLogin.Enabled = false;
        btnLogin.Text    = "Verificando...";

        try
        {
            var repo    = new UsuarioRepository();
            var usuario_obj = repo.Autenticar(usuario, contrasena);

            if (usuario_obj == null)
            {
                lblError.Text    = "Usuario o contraseña incorrectos.";
                txtContrasena.Clear();
                txtUsuario.Focus();
                return;
            }

            // Cargar miembro asociado si existe
            var miembroRepo = new MiembroRepository();
            var miembro     = miembroRepo.ObtenerPorUsuarioId(usuario_obj.Id);

            SessionManager.IniciarSesion(usuario_obj, miembro);
            DatabaseHelper.LogActividad(usuario_obj.Id, "Inicio de sesión");

            var main = new frmMain();
            Hide();
            main.ShowDialog();
            Close();
        }
        catch (Exception ex)
        {
            lblError.Text = "Error al conectar: " + ex.Message;
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text    = "Iniciar Sesión";
        }
    }
}
