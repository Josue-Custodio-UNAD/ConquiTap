using ConquiTap.Forms;
using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Controls;

public partial class ucClubes : UserControl
{
    private DataGridView grid        = null!;
    private TextBox      txtBuscar   = null!;
    private ComboBox     cmbTipo     = null!;
    private Button       btnNuevo    = null!;
    private Button       btnEditar   = null!;
    private Button       btnEliminar = null!;
    private Label        lblTotal    = null!;

    private readonly ClubRepository _repo = new();
    private List<Club> _clubes = new();

    public ucClubes()
    {
        BackColor = AppColors.MainBg;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;

        // Toolbar
        var pnlToolbar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = Color.White,
            Padding   = new Padding(8)
        };
        pnlToolbar.Paint += (_, e) =>
        {
            using var pen = new Pen(AppColors.BorderLight);
            e.Graphics.DrawLine(pen, 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1);
        };

        txtBuscar = new TextBox
        {
            PlaceholderText = "🔍  Buscar club...",
            Font            = AppColors.FontBody,
            BorderStyle     = BorderStyle.FixedSingle,
            Width           = 250,
            Location        = new Point(8, 11),
            Height          = 30
        };
        txtBuscar.TextChanged += (_, _) => ApplyFilter();

        var lblTipo = new Label
        {
            Text      = "Tipo:",
            Font      = AppColors.FontBody,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(270, 14)
        };

        cmbTipo = new ComboBox
        {
            Location      = new Point(308, 11),
            Width         = 160,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppColors.FontBody
        };
        cmbTipo.Items.AddRange(new object[] { "Todos", "Aventureros", "Conquistadores", "Guias" });
        cmbTipo.SelectedIndex = 0;
        cmbTipo.SelectedIndexChanged += (_, _) => ApplyFilter();

        lblTotal = new Label
        {
            Text      = "0 clubes",
            Font      = AppColors.FontSmall,
            ForeColor = AppColors.TextMuted,
            AutoSize  = true,
            Location  = new Point(484, 16)
        };

        // Botones
        btnEliminar = CreateBtn("✕ Eliminar", AppColors.BtnDanger);
        btnEditar   = CreateBtn("✎ Editar", AppColors.BtnPrimary);
        btnNuevo    = CreateBtn("+ Nuevo Club", AppColors.BtnSuccess);

        if (!SessionManager.EsDirectivo)
        {
            btnNuevo.Visible = btnEditar.Visible = btnEliminar.Visible = false;
        }

        btnNuevo.Click    += BtnNuevo_Click;
        btnEditar.Click   += BtnEditar_Click;
        btnEliminar.Click += BtnEliminar_Click;

        pnlToolbar.Controls.AddRange(new Control[]
        {
            txtBuscar, lblTipo, cmbTipo, lblTotal,
            btnEliminar, btnEditar, btnNuevo
        });

        pnlToolbar.Resize += (_, _) =>
        {
            int rx = pnlToolbar.Width - 12;
            foreach (var b in new[] { btnEliminar, btnEditar, btnNuevo })
            {
                if (!b.Visible) continue;
                rx -= b.Width + 6;
                b.Location = new Point(rx, 11);
            }
        };
        pnlToolbar.HandleCreated += (_, _) =>
        {
            int rx = pnlToolbar.Width - 12;
            foreach (var b in new[] { btnEliminar, btnEditar, btnNuevo })
            {
                if (!b.Visible) continue;
                rx -= b.Width + 6;
                b.Location = new Point(rx, 11);
            }
        };

        // Tarjetas de estadísticas por tipo
        var pnlStats = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 0,   // se ajusta dinámicamente
            BackColor = AppColors.MainBg
        };

        // Grid
        grid = new DataGridView { Dock = DockStyle.Fill };
        AppColors.ApplyDataGrid(grid);
        grid.DoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",       HeaderText = "ID",        Visible = false });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre",   HeaderText = "Nombre Club",  FillWeight = 22 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipo",     HeaderText = "Tipo",         FillWeight = 14 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Iglesia",  HeaderText = "Iglesia",      FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Director", HeaderText = "Director",     FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fundacion",HeaderText = "Fundación",    FillWeight = 10 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Miembros", HeaderText = "Miembros",     FillWeight = 10 });

        Controls.Add(grid);
        Controls.Add(pnlToolbar);
        pnlToolbar.BringToFront();
    }

    private static Button CreateBtn(string text, Color color)
    {
        var b = new Button
        {
            Text      = text,
            Width     = Math.Max(90, text.Length * 8),
            Height    = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    private void LoadData()
    {
        try
        {
            _clubes = _repo.ObtenerTodos();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar clubes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilter()
    {
        string t   = txtBuscar.Text.Trim();
        string tip = cmbTipo.SelectedItem?.ToString() ?? "Todos";

        var filtrados = _clubes.Where(c =>
            (string.IsNullOrEmpty(t) || c.Nombre.Contains(t, StringComparison.OrdinalIgnoreCase)) &&
            (tip == "Todos" || c.TipoClub == tip)).ToList();

        grid.Rows.Clear();
        foreach (var c in filtrados)
            grid.Rows.Add(c.Id, c.Nombre, c.TipoClub, c.IglesiaNombre,
                          c.DirectorNombre, c.AnoFundacion?.ToString() ?? "-", c.TotalMiembros);

        lblTotal.Text = $"{filtrados.Count} club(es)";
    }

    private Club? GetSelected()
    {
        if (grid.SelectedRows.Count == 0) return null;
        int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
        return _repo.ObtenerPorId(id);
    }

    private void BtnNuevo_Click(object? s, EventArgs e)
    {
        using var frm = new frmClubDetalle(null);
        if (frm.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var c = GetSelected();
        if (c == null) { MessageBox.Show("Selecciona un club.", "Aviso"); return; }
        using var frm = new frmClubDetalle(c);
        if (frm.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnEliminar_Click(object? s, EventArgs e)
    {
        var c = GetSelected();
        if (c == null) { MessageBox.Show("Selecciona un club.", "Aviso"); return; }

        if (MessageBox.Show($"¿Desactivar el club '{c.Nombre}'?",
            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        if (_repo.Desactivar(c.Id))
        {
            DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Desactivó club: {c.Nombre}", "Clubes", c.Id);
            LoadData();
        }
    }
}
