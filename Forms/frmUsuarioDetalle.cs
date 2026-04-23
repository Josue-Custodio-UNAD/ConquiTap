using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Forms;

public class frmUsuarioDetalle : Form
{
    private readonly Usuario? _usuario;
    private readonly bool _isNew;
    private readonly UsuarioRepository _repo = new();

    private TextBox  txtUsuario    = null!;
    private TextBox  txtCorreo     = null!;
    private ComboBox cmbCategoria  = null!;
    private TextBox  txtContrasena = null!;
    private TextBox  txtConfirmar  = null!;
    private CheckBox chkActivo     = null!;
    private Label    lblStatus     = null!;

    private const int PX = 16;
    private const int IH = 28;
    private const int LH = 18;
    private const int RH = 54;

    public frmUsuarioDetalle(Usuario? usuario)
    {
        _usuario = usuario;
        _isNew   = usuario == null;
        InitializeComponent();
        if (!_isNew) PopulateForm();
    }

    private void InitializeComponent()
    {
        Text            = _isNew ? "Nuevo Usuario" : "Editar Usuario";
        Size            = new Size(460, _isNew ? 440 : 480);
        MinimumSize     = Size;
        MaximumSize     = Size;
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = AppColors.MainBg;
        Font            = AppColors.FontBody;

        // ── Header ────────────────────────────────────────────────────────
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = AppColors.Denim };
        pnlHeader.Controls.Add(new Label
        {
            Text      = _isNew ? "➕  Nuevo Usuario" : "✎  Editar Usuario",
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(PX, 14)
        });

        // ── Footer ────────────────────────────────────────────────────────
        var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Color.White };
        pnlFooter.Paint += (_, e) =>
        {
            using var pen = new Pen(AppColors.BorderLight);
            e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
        };

        lblStatus = new Label
        {
            Font      = new Font("Segoe UI", 9),
            ForeColor = AppColors.Danger,
            AutoSize  = false,
            Location  = new Point(PX, 17),
            Size      = new Size(220, 20)
        };

        var btnCancelar = new Button
        {
            Text      = "Cancelar",
            Size      = new Size(100, 34),
            Location  = new Point(460 - PX - 100, 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.MainBg,
            ForeColor = AppColors.TextPrimary,
            Font      = AppColors.FontBody,
            Cursor    = Cursors.Hand
        };
        btnCancelar.FlatAppearance.BorderColor = AppColors.BorderMedium;
        btnCancelar.FlatAppearance.BorderSize  = 1;
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        var btnGuardar = new Button
        {
            Text      = "💾  Guardar",
            Size      = new Size(120, 34),
            Location  = new Point(460 - PX - 100 - 12 - 120, 10),
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Denim,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += BtnGuardar_Click;

        pnlFooter.Controls.AddRange(new Control[] { lblStatus, btnGuardar, btnCancelar });

        // ── Cuerpo ────────────────────────────────────────────────────────
        var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(PX) };

        int y   = PX;
        int w   = 460 - PX * 2 - 2;

        void Lbl(string text, int ly) =>
            pnl.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(PX, ly) });

        TextBox TB(int ly, bool password = false)
        {
            var tb = new TextBox
            {
                Location             = new Point(PX, ly),
                Size                 = new Size(w, IH),
                Font                 = AppColors.FontBody,
                BorderStyle          = BorderStyle.FixedSingle,
                UseSystemPasswordChar = password
            };
            pnl.Controls.Add(tb);
            return tb;
        }

        // Nombre de usuario
        Lbl("Nombre de usuario *", y);
        txtUsuario = TB(y + LH);
        y += RH;

        // Correo
        Lbl("Correo electrónico", y);
        txtCorreo = TB(y + LH);
        y += RH;

        // Rol (izquierda) — ancho medio
        int midW = (w - 12) / 2;
        Lbl("Rol *", y);
        cmbCategoria = new ComboBox { Location = new Point(PX, y + LH), Size = new Size(midW, IH), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppColors.FontBody };
        cmbCategoria.Items.AddRange(new object[] { "Miembro", "Directivo", "Administrador" });
        cmbCategoria.SelectedIndex = 0;
        pnl.Controls.Add(cmbCategoria);
        y += RH;

        // Contraseña
        string contraLabel = _isNew ? "Contraseña * (mín. 6 caracteres)" : "Nueva contraseña (dejar vacío para no cambiar)";
        Lbl(contraLabel, y);
        txtContrasena = TB(y + LH, password: true);
        y += RH;

        // Confirmar
        Lbl("Confirmar contraseña", y);
        txtConfirmar = TB(y + LH, password: true);
        y += RH;

        // Activo (solo al editar)
        if (!_isNew)
        {
            chkActivo = new CheckBox
            {
                Text      = "Usuario activo",
                Location  = new Point(PX, y + 4),
                AutoSize  = true,
                Font      = AppColors.FontBody,
                Checked   = true
            };
            pnl.Controls.Add(chkActivo);
        }

        Controls.Add(pnl);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        AcceptButton = btnGuardar;
    }

    private void PopulateForm()
    {
        var u = _usuario!;
        txtUsuario.Text = u.NombreUsuario;
        txtCorreo.Text  = u.Correo;
        chkActivo.Checked = u.Activo;

        for (int i = 0; i < cmbCategoria.Items.Count; i++)
            if (cmbCategoria.Items[i]?.ToString() == u.Categoria) { cmbCategoria.SelectedIndex = i; break; }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        lblStatus.ForeColor = AppColors.Danger;
        lblStatus.Text = "";

        string uName  = txtUsuario.Text.Trim();
        string correo = txtCorreo.Text.Trim();
        string contra = txtContrasena.Text;
        string conf   = txtConfirmar.Text;
        string cat    = cmbCategoria.SelectedItem?.ToString() ?? "Miembro";

        if (string.IsNullOrEmpty(uName))
        {
            lblStatus.Text = "El nombre de usuario es obligatorio."; return;
        }

        bool cambiarPass = !string.IsNullOrEmpty(contra);

        if (_isNew && string.IsNullOrEmpty(contra))
        {
            lblStatus.Text = "La contraseña es obligatoria."; return;
        }

        if (cambiarPass)
        {
            if (!PasswordHelper.IsPasswordValid(contra, out string msg)) { lblStatus.Text = msg; return; }
            if (contra != conf) { lblStatus.Text = "Las contraseñas no coinciden."; return; }
        }

        int excludeId = _isNew ? 0 : _usuario!.Id;
        if (_repo.ExisteNombreUsuario(uName, excludeId))
        {
            lblStatus.Text = "Ese nombre de usuario ya existe."; return;
        }

        try
        {
            if (_isNew)
            {
                var nuevo = new Usuario { NombreUsuario = uName, Correo = correo, Categoria = cat, Activo = true };
                int id = _repo.Crear(nuevo, contra);
                DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Creó usuario: {uName}", "Usuarios", id);
            }
            else
            {
                var u = _usuario!;
                u.NombreUsuario = uName;
                u.Correo        = correo;
                u.Categoria     = cat;
                u.Activo        = chkActivo?.Checked ?? true;
                _repo.Actualizar(u);
                if (cambiarPass) _repo.CambiarContrasena(u.Id, contra);
                DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Editó usuario: {uName}", "Usuarios", u.Id);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { lblStatus.Text = "Error: " + ex.Message; }
    }
}
