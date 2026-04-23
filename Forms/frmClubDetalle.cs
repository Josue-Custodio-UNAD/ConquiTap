using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Forms;

public class frmClubDetalle : Form
{
    private readonly Club? _club;
    private readonly ClubRepository _repo = new();
    private readonly bool _isNew;

    private TextBox  txtNombre   = null!;
    private ComboBox cmbTipo     = null!;
    private TextBox  txtAno      = null!;
    private ComboBox cmbIglesia  = null!;
    private ComboBox cmbDirector = null!;
    private Label    lblStatus   = null!;

    private const int PX = 16;
    private const int IH = 28;
    private const int LH = 18;
    private const int RH = 54;

    public frmClubDetalle(Club? club)
    {
        _club  = club;
        _isNew = club == null;
        InitializeComponent();
        LoadCatalogos();
        if (!_isNew) PopulateForm();
    }

    private void InitializeComponent()
    {
        Text            = _isNew ? "Nuevo Club" : "Editar Club";
        Size            = new Size(500, 430);
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
            Text      = _isNew ? "➕  Nuevo Club" : "✎  Editar Club",
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
            Size      = new Size(260, 20)
        };

        var btnCancelar = new Button
        {
            Text      = "Cancelar",
            Size      = new Size(100, 34),
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
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Denim,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += BtnGuardar_Click;

        // Posición inicial fija (form es de ancho fijo: 500)
        btnCancelar.Location = new Point(500 - PX - 100,          10);
        btnGuardar.Location  = new Point(500 - PX - 100 - 12 - 120, 10);

        pnlFooter.Controls.AddRange(new Control[] { lblStatus, btnGuardar, btnCancelar });

        // ── Cuerpo ────────────────────────────────────────────────────────
        var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(PX) };

        int y = PX;
        int w = 500 - PX * 2 - 2; // ancho útil

        void Lbl(string text, int lx, int ly) =>
            pnl.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(lx, ly) });

        // Fila 1: Nombre (full-width)
        Lbl("Nombre del Club *", PX, y);
        txtNombre = new TextBox { Location = new Point(PX, y + LH), Size = new Size(w, IH), Font = AppColors.FontBody, BorderStyle = BorderStyle.FixedSingle };
        pnl.Controls.Add(txtNombre);
        y += RH;

        // Fila 2: Tipo | Año
        int cw2 = (w - 12) / 2;
        Lbl("Tipo de Club *",    PX,          y);
        Lbl("Año de Fundación",  PX + cw2 + 12, y);

        cmbTipo = new ComboBox { Location = new Point(PX, y + LH), Size = new Size(cw2, IH), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppColors.FontBody };
        cmbTipo.Items.AddRange(new object[] { "Aventureros", "Conquistadores", "Guias" });
        cmbTipo.SelectedIndex = 1;
        pnl.Controls.Add(cmbTipo);

        txtAno = new TextBox { Location = new Point(PX + cw2 + 12, y + LH), Size = new Size(cw2, IH), Font = AppColors.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ej: 1995" };
        pnl.Controls.Add(txtAno);
        y += RH;

        // Fila 3: Iglesia (full-width)
        Lbl("Iglesia", PX, y);
        cmbIglesia = new ComboBox { Location = new Point(PX, y + LH), Size = new Size(w, IH), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppColors.FontBody };
        pnl.Controls.Add(cmbIglesia);
        y += RH;

        // Fila 4: Director (full-width)
        Lbl("Director del Club", PX, y);
        cmbDirector = new ComboBox { Location = new Point(PX, y + LH), Size = new Size(w, IH), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppColors.FontBody };
        pnl.Controls.Add(cmbDirector);

        Controls.Add(pnl);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        AcceptButton = btnGuardar;
    }

    private void LoadCatalogos()
    {
        cmbIglesia.Items.Clear();
        cmbIglesia.Items.Add(new Iglesia { Id = 0, Nombre = "(Sin iglesia)" });
        foreach (var ig in _repo.ObtenerIglesias()) cmbIglesia.Items.Add(ig);
        cmbIglesia.SelectedIndex = 0;

        cmbDirector.Items.Clear();
        cmbDirector.Items.Add(new Miembro { Id = 0, Nombres = "(Sin director)", Apellidos = "" });
        foreach (var m in _repo.ObtenerDirectoresPosibles()) cmbDirector.Items.Add(m);
        cmbDirector.SelectedIndex = 0;
    }

    private void PopulateForm()
    {
        var c = _club!;
        txtNombre.Text = c.Nombre;
        txtAno.Text    = c.AnoFundacion?.ToString() ?? "";

        for (int i = 0; i < cmbTipo.Items.Count; i++)
            if (cmbTipo.Items[i]?.ToString() == c.TipoClub) { cmbTipo.SelectedIndex = i; break; }

        for (int i = 0; i < cmbIglesia.Items.Count; i++)
            if (cmbIglesia.Items[i] is Iglesia ig && ig.Id == c.IglesiaId) { cmbIglesia.SelectedIndex = i; break; }

        for (int i = 0; i < cmbDirector.Items.Count; i++)
            if (cmbDirector.Items[i] is Miembro m && m.Id == c.DirectorId) { cmbDirector.SelectedIndex = i; break; }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        lblStatus.ForeColor = AppColors.Danger;
        lblStatus.Text = "";

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            lblStatus.Text = "El nombre del club es obligatorio."; return;
        }

        int? ano = null;
        if (!string.IsNullOrWhiteSpace(txtAno.Text))
        {
            if (!int.TryParse(txtAno.Text.Trim(), out int a) || a < 1800 || a > DateTime.Now.Year)
            {
                lblStatus.Text = "Año de fundación inválido."; return;
            }
            ano = a;
        }

        var c = _club ?? new Club();
        c.Nombre       = txtNombre.Text.Trim();
        c.TipoClub     = cmbTipo.SelectedItem?.ToString() ?? "Conquistadores";
        c.AnoFundacion = ano;
        c.IglesiaId    = cmbIglesia.SelectedItem  is Iglesia ig  && ig.Id  > 0 ? ig.Id  : null;
        c.DirectorId   = cmbDirector.SelectedItem is Miembro mbr && mbr.Id > 0 ? mbr.Id : null;

        try
        {
            if (_isNew)
            {
                int id = _repo.Crear(c);
                DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Creó club: {c.Nombre}", "Clubes", id);
            }
            else
            {
                _repo.Actualizar(c);
                DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Editó club: {c.Nombre}", "Clubes", c.Id);
            }
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { lblStatus.Text = "Error al guardar: " + ex.Message; }
    }
}
