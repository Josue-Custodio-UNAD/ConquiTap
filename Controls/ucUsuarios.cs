using ConquiTap.Forms;
using ConquiTap.Helpers;
using ConquiTap.Models;
using ConquiTap.Repositories;

namespace ConquiTap.Controls;

public class ucUsuarios : UserControl
{
    private DataGridView grid     = null!;
    private Button       btnNuevo = null!;
    private Button       btnEditar= null!;
    private Button       btnDes   = null!;
    private Label        lblTotal = null!;

    private readonly UsuarioRepository _repo = new();
    private List<Usuario> _usuarios = new();

    public ucUsuarios()
    {
        BackColor = AppColors.MainBg;
        // Solo directivos y administradores pueden acceder
        if (!SessionManager.EsDirectivo)
        {
            Controls.Add(new Label
            {
                Text      = "⚠ No tienes permiso para ver esta sección.",
                Font      = AppColors.FontSubhead,
                ForeColor = AppColors.Danger,
                AutoSize  = true,
                Location  = new Point(20, 20)
            });
            return;
        }
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;

        var pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White, Padding = new Padding(8) };
        pnlToolbar.Paint += (_, e) => { using var p = new Pen(AppColors.BorderLight); e.Graphics.DrawLine(p, 0, pnlToolbar.Height - 1, pnlToolbar.Width, pnlToolbar.Height - 1); };

        var txtBuscar = new TextBox { PlaceholderText = "🔍 Buscar usuario...", Font = AppColors.FontBody, BorderStyle = BorderStyle.FixedSingle, Width = 240, Location = new Point(8, 11), Height = 30 };
        txtBuscar.TextChanged += (_, _) => ApplyFilter(txtBuscar.Text);

        lblTotal = new Label { Text = "0 usuarios", Font = AppColors.FontSmall, ForeColor = AppColors.TextMuted, AutoSize = true, Location = new Point(260, 16) };

        btnDes    = MakeBtn("✕ Desactivar", AppColors.BtnDanger);
        btnEditar = MakeBtn("✎ Editar",     AppColors.BtnPrimary);
        btnNuevo  = MakeBtn("+ Nuevo",       AppColors.BtnSuccess);

        // Solo admin puede crear/editar usuarios
        if (!SessionManager.EsAdministrador) { btnNuevo.Visible = btnEditar.Visible = btnDes.Visible = false; }

        btnNuevo.Click  += (_, _) => { using var f = new frmUsuarioDetalle(null); if (f.ShowDialog() == DialogResult.OK) LoadData(); };
        btnEditar.Click += BtnEditar_Click;
        btnDes.Click    += BtnDesactivar_Click;

        pnlToolbar.Controls.AddRange(new Control[] { txtBuscar, lblTotal, btnDes, btnEditar, btnNuevo });
        pnlToolbar.Resize += (_, _) =>
        {
            int rx = pnlToolbar.Width - 12;
            foreach (var b in new[] { btnDes, btnEditar, btnNuevo })
            { if (!b.Visible) continue; rx -= b.Width + 6; b.Location = new Point(rx, 11); }
        };
        pnlToolbar.HandleCreated += (_, _) =>
        {
            int rx = pnlToolbar.Width - 12;
            foreach (var b in new[] { btnDes, btnEditar, btnNuevo })
            { if (!b.Visible) continue; rx -= b.Width + 6; b.Location = new Point(rx, 11); }
        };

        grid = new DataGridView { Dock = DockStyle.Fill };
        AppColors.ApplyDataGrid(grid);
        grid.DoubleClick += BtnEditar_Click;

        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",        HeaderText = "ID",        Visible = false });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Usuario",   HeaderText = "Usuario",   FillWeight = 20 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Correo",    HeaderText = "Correo",    FillWeight = 25 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Categoria", HeaderText = "Rol",       FillWeight = 15 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UltimoAcc", HeaderText = "Último Acceso", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado",    HeaderText = "Estado",    FillWeight = 10 });

        Controls.Add(grid);
        Controls.Add(pnlToolbar);
        pnlToolbar.BringToFront();
    }

    private static Button MakeBtn(string t, Color c)
    {
        var b = new Button { Text = t, Width = Math.Max(80, t.Length * 8), Height = 30, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderSize = 0; return b;
    }

    private void LoadData()
    {
        _usuarios = _repo.ObtenerTodos();
        ApplyFilter("");
    }

    private void ApplyFilter(string t)
    {
        var lista = _usuarios.Where(u =>
            string.IsNullOrEmpty(t) ||
            u.NombreUsuario.Contains(t, StringComparison.OrdinalIgnoreCase) ||
            u.Correo.Contains(t, StringComparison.OrdinalIgnoreCase)).ToList();

        grid.Rows.Clear();
        foreach (var u in lista)
            grid.Rows.Add(u.Id, u.NombreUsuario, u.Correo, u.Categoria,
                          u.UltimoAcceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca",
                          u.Activo ? "Activo" : "Inactivo");
        lblTotal.Text = $"{lista.Count} usuario(s)";
    }

    private Usuario? GetSelected()
    {
        if (grid.SelectedRows.Count == 0) return null;
        int id = (int)grid.SelectedRows[0].Cells["Id"].Value;
        return _repo.ObtenerPorId(id);
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var u = GetSelected();
        if (u == null) { MessageBox.Show("Selecciona un usuario.", "Aviso"); return; }
        using var frm = new frmUsuarioDetalle(u);
        if (frm.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void BtnDesactivar_Click(object? s, EventArgs e)
    {
        var u = GetSelected();
        if (u == null) { MessageBox.Show("Selecciona un usuario.", "Aviso"); return; }
        if (u.Id == SessionManager.UsuarioActual?.Id) { MessageBox.Show("No puedes desactivar tu propia cuenta.", "Aviso"); return; }
        if (MessageBox.Show($"¿Desactivar a '{u.NombreUsuario}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        if (_repo.Desactivar(u.Id)) { DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, $"Desactivó usuario: {u.NombreUsuario}", "Usuarios", u.Id); LoadData(); }
    }
}
