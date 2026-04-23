using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Forms;

public class frmMiembroDetalle : Form
{
    private readonly Miembro? _miembro;
    private readonly MiembroRepository _repo = new();
    private readonly bool _isNew;

    // ── Campos ───────────────────────────────────────────────────────────────
    private TextBox        txtNombres        = null!;
    private TextBox        txtApellidos      = null!;
    private TextBox        txtCedula         = null!;
    private TextBox        txtTelefono       = null!;
    private TextBox        txtCorreo         = null!;
    private ComboBox       cmbSexo           = null!;
    private ComboBox       cmbCategoria      = null!;
    private TextBox        txtPuesto         = null!;
    private ComboBox       cmbClase          = null!;
    private CheckBox       chkNacimiento     = null!;
    private DateTimePicker dtpNacimiento     = null!;
    private CheckBox       chkBautismo       = null!;
    private DateTimePicker dtpBautismo       = null!;
    private CheckBox       chkInvestidura    = null!;
    private DateTimePicker dtpInvestidura    = null!;
    private ComboBox       cmbAsociacion     = null!;
    private ComboBox       cmbZona           = null!;
    private ComboBox       cmbDistrito       = null!;
    private ComboBox       cmbIglesia        = null!;
    private ComboBox       cmbClub           = null!;
    private CheckedListBox lstEspecialidades = null!;
    private Label          lblStatus         = null!;

    // ── Constantes de layout ─────────────────────────────────────────────────
    // Contenido: scroll de 688px → padding 8px c/lado → útil 672px
    private const int PX  = 8;    // x inicio
    private const int FW  = 672;  // ancho total disponible
    private const int CW  = 330;  // ancho de columna (dos columnas con gap de 12)
    private const int CX2 = 342;  // x de la columna derecha (330+12)
    private const int LH  = 18;   // alto de etiqueta
    private const int IH  = 28;   // alto de input/combo
    private const int RH  = 54;   // alto de fila (label+input+gap)

    public frmMiembroDetalle(Miembro? miembro)
    {
        _miembro = miembro;
        _isNew   = miembro == null;
        InitializeComponent();
        LoadCatalogos();
        if (!_isNew) PopulateForm();
    }

    private void InitializeComponent()
    {
        Text            = _isNew ? "Nuevo Miembro" : "Editar Miembro";
        Size            = new Size(720, 740);
        MinimumSize     = new Size(720, 580);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox     = false;
        BackColor       = AppColors.MainBg;
        Font            = AppColors.FontBody;

        // ── Header ────────────────────────────────────────────────────────
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = AppColors.Denim };
        pnlHeader.Controls.Add(new Label
        {
            Text      = _isNew ? "➕  Nuevo Miembro" : "✎  Editar Miembro",
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(16, 14)
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
            Anchor    = AnchorStyles.Left | AnchorStyles.Top,
            Location  = new Point(12, 17),
            Size      = new Size(400, 20)
        };

        var btnCancelar = new Button
        {
            Text      = "Cancelar",
            Size      = new Size(100, 34),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
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
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppColors.Denim,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnGuardar.FlatAppearance.BorderSize = 0;
        btnGuardar.Click += BtnGuardar_Click;

        // Posicionar botones (inicial + resize)
        void PositionFooter()
        {
            btnCancelar.Location = new Point(pnlFooter.Width - 116, 10);
            btnGuardar.Location  = new Point(pnlFooter.Width - 248, 10);
        }
        pnlFooter.Resize += (_, _) => PositionFooter();
        pnlFooter.Controls.AddRange(new Control[] { lblStatus, btnGuardar, btnCancelar });
        Load += (_, _) => PositionFooter();

        // ── Scroll ────────────────────────────────────────────────────────
        var scroll = new Panel
        {
            Dock       = DockStyle.Fill,
            AutoScroll = true,
            Padding    = new Padding(PX, 8, PX, 8),
            BackColor  = AppColors.MainBg
        };

        int y = 8;

        // ═══════════════════════════════════════
        // 1. INFORMACIÓN PERSONAL
        // ═══════════════════════════════════════
        Section(scroll, "Información Personal", ref y);

        // Fila: Nombres | Apellidos  (CLAVE: guardar rowY antes de añadir ambos)
        Row2(scroll, "Nombres *", out txtNombres, "Apellidos *", out txtApellidos, ref y);

        // Fila: Cédula | Teléfono
        Row2(scroll, "Cédula", out txtCedula, "Teléfono", out txtTelefono, ref y);

        // Fila: Correo (full-width)
        Row1w(scroll, "Correo electrónico", out txtCorreo, ref y);

        // Fila: Sexo | Categoría | Puesto  (3 columnas manuales)
        {
            int ry = y;
            int cw3 = 208; int gap3 = 12;
            int x1 = PX, x2 = PX + cw3 + gap3, x3 = PX + (cw3 + gap3) * 2;
            int cw3last = FW - (cw3 + gap3) * 2;

            Lbl(scroll, "Sexo",      x1, ry);
            Lbl(scroll, "Categoría", x2, ry);
            Lbl(scroll, "Puesto",    x3, ry);

            cmbSexo      = ComboAt(scroll, x1, ry + LH, cw3);
            cmbCategoria = ComboAt(scroll, x2, ry + LH, cw3);
            txtPuesto    = TextAt(scroll,  x3, ry + LH, cw3last);

            cmbSexo.Items.AddRange(new object[] { "", "Masculino", "Femenino" });
            cmbCategoria.Items.AddRange(new object[] { "Miembro", "Directivo" });
            cmbCategoria.SelectedIndex = 0;
            y = ry + RH;
        }

        // Fila: Clase (izquierda)
        {
            int ry = y;
            Lbl(scroll, "Clase", PX, ry);
            cmbClase = ComboAt(scroll, PX, ry + LH, CW);
            cmbClase.Items.AddRange(new object[]
                { "", "Amigo", "Compañero", "Explorador", "Pionero",
                  "Excursionista", "Viajero", "Guía Mayor", "Guía" });
            y = ry + RH;
        }

        // Fila: Fecha nacimiento | Fecha bautismo
        {
            int ry = y;
            Lbl(scroll, "Fecha de nacimiento", PX,  ry);
            Lbl(scroll, "Fecha de bautismo",   CX2, ry);

            chkNacimiento = new CheckBox { Text = "Activar", Location = new Point(PX, ry + LH + 4), AutoSize = true, Font = AppColors.FontSmall };
            dtpNacimiento = new DateTimePicker { Location = new Point(PX + 70, ry + LH + 2), Width = CW - 74, Height = IH, Format = DateTimePickerFormat.Short, Enabled = false };
            chkNacimiento.CheckedChanged += (_, _) => dtpNacimiento.Enabled = chkNacimiento.Checked;

            chkBautismo = new CheckBox { Text = "Activar", Location = new Point(CX2, ry + LH + 4), AutoSize = true, Font = AppColors.FontSmall };
            dtpBautismo = new DateTimePicker { Location = new Point(CX2 + 70, ry + LH + 2), Width = CW - 74, Height = IH, Format = DateTimePickerFormat.Short, Enabled = false };
            chkBautismo.CheckedChanged += (_, _) => dtpBautismo.Enabled = chkBautismo.Checked;

            scroll.Controls.AddRange(new Control[] { chkNacimiento, dtpNacimiento, chkBautismo, dtpBautismo });
            y = ry + RH;
        }

        // Fila: Fecha investidura
        {
            int ry = y;
            Lbl(scroll, "Fecha de investidura", PX, ry);

            chkInvestidura = new CheckBox { Text = "Activar", Location = new Point(PX, ry + LH + 4), AutoSize = true, Font = AppColors.FontSmall };
            dtpInvestidura = new DateTimePicker { Location = new Point(PX + 70, ry + LH + 2), Width = 220, Height = IH, Format = DateTimePickerFormat.Short, Enabled = false };
            chkInvestidura.CheckedChanged += (_, _) => dtpInvestidura.Enabled = chkInvestidura.Checked;

            scroll.Controls.AddRange(new Control[] { chkInvestidura, dtpInvestidura });
            y = ry + RH;
        }

        // ═══════════════════════════════════════
        // 2. UBICACIÓN
        // ═══════════════════════════════════════
        y += 6;
        Section(scroll, "Ubicación", ref y);

        // Cuatro columnas: Asociación | Zona | Distrito | Iglesia
        {
            int ry = y;
            const int w4 = 156; const int g4 = 8;
            int xa = PX, xz = xa + w4 + g4, xd = xz + w4 + g4, xi = xd + w4 + g4;

            Lbl(scroll, "Asociación", xa, ry);
            Lbl(scroll, "Zona",       xz, ry);
            Lbl(scroll, "Distrito",   xd, ry);
            Lbl(scroll, "Iglesia",    xi, ry);

            cmbAsociacion = ComboAt(scroll, xa, ry + LH, w4);
            cmbZona       = ComboAt(scroll, xz, ry + LH, w4);
            cmbDistrito   = ComboAt(scroll, xd, ry + LH, w4);
            cmbIglesia    = ComboAt(scroll, xi, ry + LH, FW - (w4 + g4) * 3);

            cmbAsociacion.SelectedIndexChanged += (_, _) => OnAsociacionChanged();
            cmbZona.SelectedIndexChanged       += (_, _) => OnZonaChanged();
            cmbDistrito.SelectedIndexChanged   += (_, _) => OnDistritoChanged();
            y = ry + RH;
        }

        // ═══════════════════════════════════════
        // 3. CLUB
        // ═══════════════════════════════════════
        y += 6;
        Section(scroll, "Club", ref y);

        {
            int ry = y;
            Lbl(scroll, "Club al que pertenece", PX, ry);
            cmbClub = ComboAt(scroll, PX, ry + LH, 380);
            y = ry + RH;
        }

        // ═══════════════════════════════════════
        // 4. ESPECIALIDADES
        // ═══════════════════════════════════════
        y += 6;
        Section(scroll, "Especialidades", ref y);

        lstEspecialidades = new CheckedListBox
        {
            Location     = new Point(PX, y),
            Size         = new Size(FW, 116),
            Font         = AppColors.FontSmall,
            BorderStyle  = BorderStyle.FixedSingle,
            CheckOnClick = true,
            BackColor    = Color.White
        };
        scroll.Controls.Add(lstEspecialidades);
        y += 130;

        // Fijar tamaño mínimo del scroll
        scroll.AutoScrollMinSize = new Size(0, y + 16);

        Controls.Add(scroll);
        Controls.Add(pnlFooter);
        Controls.Add(pnlHeader);
        AcceptButton = btnGuardar;
    }

    // ── Catálogos ────────────────────────────────────────────────────────────

    private void LoadCatalogos()
    {
        try
        {
            cmbAsociacion.Items.Clear();
            cmbAsociacion.Items.Add(new Asociacion { Id = 0, Nombre = "(Seleccione)" });
            foreach (var a in _repo.ObtenerAsociaciones()) cmbAsociacion.Items.Add(a);
            cmbAsociacion.SelectedIndex = 0;

            ResetCombo(cmbZona,     new Zona     { Id = 0, Nombre = "(Seleccione)" });
            ResetCombo(cmbDistrito, new Distrito { Id = 0, Nombre = "(Seleccione)" });
            ResetCombo(cmbIglesia,  new Iglesia  { Id = 0, Nombre = "(Seleccione)" });

            var clubRepo = new ClubRepository();
            cmbClub.Items.Clear();
            cmbClub.Items.Add(new Club { Id = 0, Nombre = "(Sin club)" });
            foreach (var c in clubRepo.ObtenerTodos()) cmbClub.Items.Add(c);
            cmbClub.SelectedIndex = 0;

            lstEspecialidades.Items.Clear();
            foreach (var esp in _repo.ObtenerTodasEspecialidades())
                lstEspecialidades.Items.Add(esp);
        }
        catch (Exception ex) { lblStatus.Text = "Error cargando catálogos: " + ex.Message; }
    }

    private static void ResetCombo(ComboBox cmb, object placeholder)
    {
        cmb.Items.Clear();
        cmb.Items.Add(placeholder);
        cmb.SelectedIndex = 0;
    }

    private void OnAsociacionChanged()
    {
        ResetCombo(cmbZona, new Zona { Id = 0, Nombre = "(Seleccione)" });
        ResetCombo(cmbDistrito, new Distrito { Id = 0, Nombre = "(Seleccione)" });
        ResetCombo(cmbIglesia,  new Iglesia  { Id = 0, Nombre = "(Seleccione)" });
        if (cmbAsociacion.SelectedItem is Asociacion a && a.Id > 0)
            foreach (var z in _repo.ObtenerZonas(a.Id)) cmbZona.Items.Add(z);
    }

    private void OnZonaChanged()
    {
        ResetCombo(cmbDistrito, new Distrito { Id = 0, Nombre = "(Seleccione)" });
        ResetCombo(cmbIglesia,  new Iglesia  { Id = 0, Nombre = "(Seleccione)" });
        if (cmbZona.SelectedItem is Zona z && z.Id > 0)
            foreach (var d in _repo.ObtenerDistritos(z.Id)) cmbDistrito.Items.Add(d);
    }

    private void OnDistritoChanged()
    {
        ResetCombo(cmbIglesia, new Iglesia { Id = 0, Nombre = "(Seleccione)" });
        if (cmbDistrito.SelectedItem is Distrito d && d.Id > 0)
            foreach (var ig in _repo.ObtenerIglesias(d.Id)) cmbIglesia.Items.Add(ig);
    }

    // ── Poblar al editar ─────────────────────────────────────────────────────

    private void PopulateForm()
    {
        var m = _miembro!;
        txtNombres.Text   = m.Nombres;
        txtApellidos.Text = m.Apellidos;
        txtCedula.Text    = m.Cedula;
        txtTelefono.Text  = m.Telefono;
        txtCorreo.Text    = m.Correo;
        txtPuesto.Text    = m.Puesto;

        SelText(cmbCategoria, m.Categoria);
        SelText(cmbSexo,  m.Sexo == "M" ? "Masculino" : m.Sexo == "F" ? "Femenino" : "");
        SelText(cmbClase, m.Clase);

        if (m.FechaNacimiento.HasValue)  { chkNacimiento.Checked  = true; dtpNacimiento.Value  = m.FechaNacimiento.Value;  }
        if (m.FechaBautismo.HasValue)    { chkBautismo.Checked    = true; dtpBautismo.Value    = m.FechaBautismo.Value;    }
        if (m.FechaInvestidura.HasValue) { chkInvestidura.Checked = true; dtpInvestidura.Value = m.FechaInvestidura.Value; }

        // Especialidades
        for (int i = 0; i < lstEspecialidades.Items.Count; i++)
            if (lstEspecialidades.Items[i] is Especialidad esp && m.Especialidades.Any(e => e.Id == esp.Id))
                lstEspecialidades.SetItemChecked(i, true);

        // Ubicación — cascada
        if (m.AsociacionId > 0) { SelId(cmbAsociacion, m.AsociacionId!.Value); OnAsociacionChanged(); }
        if (m.ZonaId       > 0) { SelId(cmbZona,       m.ZonaId!.Value);       OnZonaChanged();       }
        if (m.DistritoId   > 0) { SelId(cmbDistrito,   m.DistritoId!.Value);   OnDistritoChanged();   }
        if (m.IglesiaId    > 0)   SelId(cmbIglesia,    m.IglesiaId!.Value);
        if (m.ClubId       > 0)   SelId(cmbClub,       m.ClubId!.Value);
    }

    // ── Guardar ──────────────────────────────────────────────────────────────

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        lblStatus.ForeColor = AppColors.Danger;
        lblStatus.Text = "";

        if (string.IsNullOrWhiteSpace(txtNombres.Text))   { lblStatus.Text = "El nombre es obligatorio.";   return; }
        if (string.IsNullOrWhiteSpace(txtApellidos.Text)) { lblStatus.Text = "El apellido es obligatorio."; return; }

        var m = _miembro ?? new Miembro();
        m.Nombres    = txtNombres.Text.Trim();
        m.Apellidos  = txtApellidos.Text.Trim();
        m.Cedula     = txtCedula.Text.Trim();
        m.Telefono   = txtTelefono.Text.Trim();
        m.Correo     = txtCorreo.Text.Trim();
        m.Puesto     = txtPuesto.Text.Trim();
        m.Categoria  = cmbCategoria.SelectedItem?.ToString() ?? "Miembro";
        m.Clase      = cmbClase.SelectedItem?.ToString() ?? "";

        string sx = cmbSexo.SelectedItem?.ToString() ?? "";
        m.Sexo = sx == "Masculino" ? "M" : sx == "Femenino" ? "F" : "";

        m.FechaNacimiento  = chkNacimiento.Checked  ? dtpNacimiento.Value  : null;
        m.FechaBautismo    = chkBautismo.Checked    ? dtpBautismo.Value    : null;
        m.FechaInvestidura = chkInvestidura.Checked ? dtpInvestidura.Value : null;

        m.AsociacionId = GetId(cmbAsociacion);
        m.ZonaId       = GetId(cmbZona);
        m.DistritoId   = GetId(cmbDistrito);
        m.IglesiaId    = GetId(cmbIglesia);
        m.ClubId       = GetId(cmbClub);

        try
        {
            int id = _isNew ? _repo.Crear(m) : (m.Id > 0 ? (int?)(_repo.Actualizar(m) ? m.Id : 0) : 0) ?? 0;
            if (_isNew)
                DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Creó miembro: {m.NombreCompleto}", "Miembros", id);
            else
            {
                id = m.Id;
                DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Editó miembro: {m.NombreCompleto}", "Miembros", id);
            }

            var espIds = lstEspecialidades.CheckedItems.Cast<Especialidad>().Select(esp => esp.Id).ToList();
            _repo.GuardarEspecialidades(id, espIds);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { lblStatus.Text = "Error al guardar: " + ex.Message; }
    }

    // ── Helpers de layout ────────────────────────────────────────────────────

    // Título de sección + línea separadora
    private static void Section(Panel p, string title, ref int y)
    {
        p.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = AppColors.Denim, AutoSize = true, Location = new Point(PX, y) });
        y += 22;
        p.Controls.Add(new Panel { BackColor = AppColors.BorderLight, Bounds = new Rectangle(PX, y, FW, 1) });
        y += 8;
    }

    // Etiqueta pequeña
    private static void Lbl(Panel p, string text, int x, int y) =>
        p.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(x, y) });

    // TextBox en posición
    private static TextBox TextAt(Panel p, int x, int y, int w)
    {
        var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, IH), Font = AppColors.FontBody, BorderStyle = BorderStyle.FixedSingle };
        p.Controls.Add(tb); return tb;
    }

    // ComboBox en posición
    private static ComboBox ComboAt(Panel p, int x, int y, int w)
    {
        var cb = new ComboBox { Location = new Point(x, y), Size = new Size(w, IH), DropDownStyle = ComboBoxStyle.DropDownList, Font = AppColors.FontBody };
        p.Controls.Add(cb); return cb;
    }

    // Fila de dos TextBox en columnas paralelas — Y se avanza UNA sola vez al final
    private void Row2(Panel p, string lbl1, out TextBox tb1, string lbl2, out TextBox tb2, ref int y)
    {
        int ry = y;
        Lbl(p, lbl1, PX,  ry); Lbl(p, lbl2, CX2, ry);
        tb1 = TextAt(p, PX,  ry + LH, CW);
        tb2 = TextAt(p, CX2, ry + LH, CW);
        y = ry + RH;
    }

    // Fila de un TextBox full-width
    private void Row1w(Panel p, string lbl, out TextBox tb, ref int y)
    {
        int ry = y;
        Lbl(p, lbl, PX, ry);
        tb = TextAt(p, PX, ry + LH, FW);
        y = ry + RH;
    }

    // ── Helpers de combo ─────────────────────────────────────────────────────

    private static void SelText(ComboBox c, string val)
    {
        for (int i = 0; i < c.Items.Count; i++)
            if (c.Items[i]?.ToString()?.Equals(val, StringComparison.OrdinalIgnoreCase) == true)
            { c.SelectedIndex = i; return; }
    }

    private static void SelId(ComboBox c, int id)
    {
        for (int i = 0; i < c.Items.Count; i++)
        {
            int iid = c.Items[i] switch
            {
                Asociacion a => a.Id,
                Zona z       => z.Id,
                Distrito d   => d.Id,
                Iglesia ig   => ig.Id,
                Club cl      => cl.Id,
                _            => -1
            };
            if (iid == id) { c.SelectedIndex = i; return; }
        }
    }

    private static int? GetId(ComboBox c) =>
        c.SelectedItem switch
        {
            Asociacion a when a.Id > 0 => a.Id,
            Zona z       when z.Id > 0 => z.Id,
            Distrito d   when d.Id > 0 => d.Id,
            Iglesia ig   when ig.Id > 0 => ig.Id,
            Club cl      when cl.Id > 0 => cl.Id,
            _ => null
        };
}
