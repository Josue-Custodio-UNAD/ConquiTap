using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Forms;

public class frmSetupAdmin : Form
{
    private TextBox txtUsuario     = null!;
    private TextBox txtCorreo      = null!;
    private TextBox txtContrasena  = null!;
    private TextBox txtConfirmar   = null!;
    private Button  btnCrear       = null!;
    private Label   lblMensaje     = null!;

    public frmSetupAdmin()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text            = "ConquiTap — Configuración Inicial";
        Size            = new Size(480, 560);
        MinimumSize     = Size;
        MaximumSize     = Size;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = AppColors.MainBg;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;

        // ── Encabezado ────────────────────────────────────────────────────
        var pnlHeader = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 100,
            BackColor = AppColors.Denim,
            Padding   = new Padding(24, 0, 0, 0)
        };

        var lblTitle = new Label
        {
            Text      = "✦  Bienvenido a ConquiTap",
            Font      = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(24, 24)
        };

        var lblSub = new Label
        {
            Text      = "Crea la cuenta de administrador para comenzar",
            Font      = AppColors.FontBody,
            ForeColor = AppColors.SidebarSubtext,
            AutoSize  = true,
            Location  = new Point(24, 60)
        };

        pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

        // ── Formulario ────────────────────────────────────────────────────
        var pnlForm = new Panel
        {
            BackColor = Color.White,
            Bounds    = new Rectangle(24, 120, 432, 380),
            Padding   = new Padding(24)
        };

        int y = 24; int x = 24; int w = 384;

        void AddLabel(string text)
        {
            pnlForm.Controls.Add(new Label
            {
                Text      = text,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppColors.TextSecondary,
                Bounds    = new Rectangle(x, y, w, 20)
            });
            y += 22;
        }

        TextBox AddTextBox(bool password = false)
        {
            var tb = new TextBox
            {
                Bounds      = new Rectangle(x, y, w, 30),
                Font        = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = password
            };
            pnlForm.Controls.Add(tb);
            y += 44;
            return tb;
        }

        AddLabel("Nombre de usuario *");
        txtUsuario = AddTextBox();

        AddLabel("Correo electrónico");
        txtCorreo  = AddTextBox();

        AddLabel("Contraseña * (mín. 6 caracteres)");
        txtContrasena = AddTextBox(password: true);

        AddLabel("Confirmar contraseña *");
        txtConfirmar = AddTextBox(password: true);

        lblMensaje = new Label
        {
            Bounds    = new Rectangle(x, y, w, 22),
            Font      = new Font("Segoe UI", 9),
            ForeColor = AppColors.Danger,
            TextAlign = ContentAlignment.MiddleCenter
        };
        pnlForm.Controls.Add(lblMensaje);
        y += 28;

        btnCrear = new Button
        {
            Text      = "Crear Administrador",
            Bounds    = new Rectangle(x, y, w, 42),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Denim,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnCrear.FlatAppearance.BorderSize = 0;
        btnCrear.Click += BtnCrear_Click;
        pnlForm.Controls.Add(btnCrear);

        Controls.AddRange(new Control[] { pnlHeader, pnlForm });
        AcceptButton = btnCrear;
    }

    private void BtnCrear_Click(object? sender, EventArgs e)
    {
        lblMensaje.Text      = "";
        lblMensaje.ForeColor = AppColors.Danger;

        string usuario   = txtUsuario.Text.Trim();
        string correo    = txtCorreo.Text.Trim();
        string contra    = txtContrasena.Text;
        string confirmar = txtConfirmar.Text;

        if (string.IsNullOrEmpty(usuario))
        {
            lblMensaje.Text = "El nombre de usuario es obligatorio."; return;
        }
        if (!PasswordHelper.IsPasswordValid(contra, out string msg))
        {
            lblMensaje.Text = msg; return;
        }
        if (contra != confirmar)
        {
            lblMensaje.Text = "Las contraseñas no coinciden."; return;
        }

        try
        {
            var repo = new UsuarioRepository();
            if (repo.ExisteNombreUsuario(usuario))
            {
                lblMensaje.Text = "Ese nombre de usuario ya existe."; return;
            }

            var admin = new Usuario
            {
                NombreUsuario = usuario,
                Correo        = correo,
                Categoria     = "Administrador",
                Activo        = true
            };

            int id = repo.Crear(admin, contra);
            if (id > 0)
            {
                lblMensaje.ForeColor = AppColors.Success;
                lblMensaje.Text      = "✓ Administrador creado correctamente.";
                btnCrear.Enabled     = false;

                Task.Delay(1500).ContinueWith(_ =>
                {
                    Invoke(() => { DialogResult = DialogResult.OK; Close(); });
                });
            }
            else
            {
                lblMensaje.Text = "No se pudo crear el usuario. Inténtelo de nuevo.";
            }
        }
        catch (Exception ex)
        {
            lblMensaje.Text = "Error: " + ex.Message;
        }
    }
}
