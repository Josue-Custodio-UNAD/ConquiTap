using ConquiTap.Forms;
using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Controls;

public class ucMiembros : UserControl
{
    private DataGridView  grid        = null!;
    private TextBox       txtBuscar   = null!;
    private ComboBox      cmbCategoria= null!;
    private Button        btnNuevo    = null!;
    private Button        btnEditar   = null!;
    private Button        btnEliminar = null!;
    private Button        btnRefresh  = null!;
    private Label         lblTotal    = null!;

    private readonly MiembroRepository _repo = new();
    private List<Miembro> _miembros = new();

    public ucMiembros()
    {
        BackColor = AppColors.MainBg;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;

        // ── Barra de herramientas ─────────────────────────────────────────
        var pnlToolbar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = Color.White,
            Padding   = new Padding(8, 8, 8, 0)
        };

        txtBuscar = new TextBox
        {
            PlaceholderText = "🔍  Buscar miembro por nombre o cédula...",
            Font            = AppColors.FontBody,
            BorderStyle     = BorderStyle.FixedSingle,
            Width           = 300,
            Location        = new Point(8, 10),
            Height          = 32
        };
        txtBuscar.TextChanged += (_, _) => ApplyFilter();

        var lblCat = new Label
        {
            Text      = "Categoría:",
            Font      = AppColors.FontBody,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(320, 14)
        };

        cmbCategoria = new ComboBox
        {
            Location       = new Point(390, 10),
            Width          = 140,
            DropDownStyle  = ComboBoxStyle.DropDownList,
            Font           = AppColors.FontBody
        };
        cmbCategoria.Items.AddRange(new object[] { "Todos", "Miembro", "Directivo" });
        cmbCategoria.SelectedIndex = 0;
        cmbCategoria.SelectedIndexChanged += (_, _) => ApplyFilter();

        lblTotal = new Label
        {
            Text      = "0 miembros",
            Font      = AppColors.FontSmall,
            ForeColor = AppColors.TextMuted,
            AutoSize  = true,
            Location  = new Point(544, 16),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left
        };

        // Botones de acción (alineados a la derecha)
        btnRefresh = CreateToolBtn("↻ Actualizar", AppColors.TextSecondary, null, 100);
        btnRefresh.Click += (_, _) => LoadData();

        btnNuevo = CreateToolBtn("+ Nuevo", AppColors.BtnSuccess, null, 90);
        btnNuevo.Click += BtnNuevo_Click;

        btnEditar = CreateToolBtn("✎ Editar", AppColors.BtnPrimary, null, 85);
        btnEditar.Click += BtnEditar_Click;

        btnEliminar = CreateToolBtn("✕ Eliminar", AppColors.BtnDanger, null, 90);
        btnEliminar.Click += BtnEliminar_Click;

        // Ocultar botones según permisos
        if (!SessionManager.EsDirectivo)
        {
            btnNuevo.Visible    = false;
            btnEditar.Visible   = false;
            btnEliminar.Visible = false;
        }

        pnlToolbar.Controls.AddRange(new Control[]
        {
            txtBuscar, lblCat, cmbCategoria, lblTotal,
            btnRefresh, btnNuevo, btnEditar, btnEliminar
        });

        // Posicionar botones al cargar Y en cada resize
        pnlToolbar.Resize += (_, _) => PositionToolbarButtons(pnlToolbar);
        pnlToolbar.HandleCreated += (_, _) => PositionToolbarButtons(pnlToolbar);

        pnlToolbar.Paint += (_, e) =>
        {
            using var pen = new Pen(AppColors.BorderLight);
            e.Graphics.DrawLine(pen, 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1);
        };

        // ── Grid ──────────────────────────────────────────────────────────
        grid = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        AppColors.ApplyDataGrid(grid);
        grid.DoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        // Columnas
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",         HeaderText = "ID",          Width = 50,  Visible = false });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre",     HeaderText = "Nombre",      FillWeight = 25 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cedula",     HeaderText = "Cédula",      FillWeight = 15 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Telefono",   HeaderText = "Teléfono",    FillWeight = 13 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Categoria",  HeaderText = "Categoría",   FillWeight = 10 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Clase",      HeaderText = "Clase",       FillWeight = 10 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Club",       HeaderText = "Club",        FillWeight = 15 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Iglesia",    HeaderText = "Iglesia",     FillWeight = 12 });

        Controls.Add(grid);
        Controls.Add(pnlToolbar);
        pnlToolbar.BringToFront();
    }

    private void PositionToolbarButtons(Panel parent)
    {
        int x = parent.Width - 12;
        foreach (var btn in new[] { btnEliminar, btnEditar, btnNuevo, btnRefresh })
        {
            if (!btn.Visible) continue;
            x -= btn.Width + 6;
            btn.Location = new Point(x, 10);
        }
    }

    private static Button CreateToolBtn(string text, Color color, Color? fg, int width)
    {
        var btn = new Button
        {
            Text      = text,
            Width     = width,
            Height    = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = fg ?? Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void LoadData()
    {
        try
        {
            _miembros = _repo.ObtenerTodos();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar miembros: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilter()
    {
        string termino = txtBuscar.Text.Trim();
        string cat     = cmbCategoria.SelectedItem?.ToString() ?? "Todos";

        var filtrados = _miembros
            .Where(m =>
                (string.IsNullOrEmpty(termino) ||
                 m.NombreCompleto.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                 m.Cedula.Contains(termino, StringComparison.OrdinalIgnoreCase)) &&
                (cat == "Todos" || m.Categoria == cat))
            .ToList();

        grid.Rows.Clear();
        foreach (var m in filtrados)
        {
            grid.Rows.Add(m.Id, m.NombreCompleto, m.Cedula, m.Telefono,
                          m.Categoria, m.Clase, m.ClubNombre, m.IglesiaNombre);
        }
        lblTotal.Text = $"{filtrados.Count} miembro(s)";
    }

    private Miembro? GetSelectedMiembro()
    {
        if (grid.SelectedRows.Count == 0) return null;
        int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
        return _repo.ObtenerPorId(id);
    }

    private void BtnNuevo_Click(object? sender, EventArgs e)
    {
        using var frm = new frmMiembroDetalle(null);
        if (frm.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnEditar_Click(object? sender, EventArgs e)
    {
        var m = GetSelectedMiembro();
        if (m == null) { MessageBox.Show("Selecciona un miembro.", "Aviso"); return; }
        using var frm = new frmMiembroDetalle(m);
        if (frm.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnEliminar_Click(object? sender, EventArgs e)
    {
        var m = GetSelectedMiembro();
        if (m == null) { MessageBox.Show("Selecciona un miembro.", "Aviso"); return; }

        var r = MessageBox.Show(
            $"¿Desactivar a {m.NombreCompleto}?\nEl registro no se eliminará permanentemente.",
            "Confirmar",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (r != DialogResult.Yes) return;

        if (_repo.Desactivar(m.Id))
        {
            DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Desactivó miembro: {m.NombreCompleto}", "Miembros", m.Id);
            LoadData();
        }
    }
}
