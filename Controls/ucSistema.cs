using ConquiTap.Helpers;
using ConquiTap.Models;

namespace ConquiTap.Controls;

public class ucSistema : UserControl
{
    public ucSistema()
    {
        BackColor = AppColors.MainBg;
        if (!SessionManager.EsAdministrador)
        {
            Controls.Add(new Label { Text = "⚠ Acceso restringido a administradores.", Font = AppColors.FontSubhead, ForeColor = AppColors.Danger, AutoSize = true, Location = new Point(20, 20) });
            return;
        }
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = AppColors.FontBody };
        tabs.TabPages.Add(BuildLogTab());
        tabs.TabPages.Add(BuildCatalogosTab());
        tabs.TabPages.Add(BuildInfoTab());
        Controls.Add(tabs);
    }

    // ── Log de actividad ──────────────────────────────────────────────────────

    private TabPage BuildLogTab()
    {
        var page = new TabPage("📋  Registro de Actividad") { BackColor = AppColors.MainBg, Padding = new Padding(8) };

        var pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.White };
        var dtpDesde = new DateTimePicker { Location = new Point(8, 8), Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-7) };
        var dtpHasta = new DateTimePicker { Location = new Point(160, 8), Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
        var btnFiltrar = new Button { Text = "Filtrar", Location = new Point(312, 8), Size = new Size(80, 26), FlatStyle = FlatStyle.Flat, BackColor = AppColors.Denim, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
        btnFiltrar.FlatAppearance.BorderSize = 0;
        pnlToolbar.Controls.AddRange(new Control[] { new Label { Text = "Desde:", AutoSize = true, Location = new Point(8, 12), Font = AppColors.FontSmall }, dtpDesde, new Label { Text = "Hasta:", AutoSize = true, Location = new Point(152, 12), Font = AppColors.FontSmall }, dtpHasta, btnFiltrar });

        var grid = new DataGridView { Dock = DockStyle.Fill };
        AppColors.ApplyDataGrid(grid);

        void Load()
        {
            try
            {
                const string sql = @"
                    SELECT la.Id, u.NombreUsuario AS Usuario, la.Accion, la.Tabla, la.RegistroId, la.Fecha
                    FROM LogActividad la
                    LEFT JOIN Usuarios u ON la.UsuarioId = u.Id
                    WHERE la.Fecha BETWEEN @D AND DATEADD(day,1,@H)
                    ORDER BY la.Fecha DESC";
                var dt = DatabaseHelper.ExecuteQuery(sql, new() { ["@D"] = dtpDesde.Value.Date, ["@H"] = dtpHasta.Value.Date });
                grid.DataSource = dt;
                grid.DataBindingComplete += (_, _) =>
                {
                    if (grid.Columns.Contains("Id")) grid.Columns["Id"]!.Visible = false;
                    if (grid.Columns.Contains("Fecha")) grid.Columns["Fecha"]!.HeaderText = "Fecha/Hora";
                    if (grid.Columns.Contains("Accion")) grid.Columns["Accion"]!.HeaderText = "Acción";
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        btnFiltrar.Click += (_, _) => Load();
        Load();

        page.Controls.Add(grid);
        page.Controls.Add(pnlToolbar);
        pnlToolbar.BringToFront();
        return page;
    }

    // ── Catálogos (Asociaciones, Zonas, etc.) ─────────────────────────────────

    private TabPage BuildCatalogosTab()
    {
        var page = new TabPage("🗂  Catálogos") { BackColor = AppColors.MainBg, Padding = new Padding(8) };

        var lblInfo = new Label
        {
            Text      = "Administra las asociaciones, zonas, distritos, iglesias y especialidades desde aquí.\r\n" +
                        "Use el botón correspondiente para gestionar cada catálogo.",
            Font      = AppColors.FontBody,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = false,
            Size      = new Size(600, 48),
            Location  = new Point(8, 8)
        };
        page.Controls.Add(lblInfo);

        int bx = 8; int by = 64;
        void AddCatBtn(string label, Action action)
        {
            var btn = new Button
            {
                Text      = label,
                Bounds    = new Rectangle(bx, by, 200, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = AppColors.Denim,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, _) => action();
            page.Controls.Add(btn);
            bx += 212;
            if (bx > 640) { bx = 8; by += 52; }
        }

        AddCatBtn("🌍 Asociaciones",   () => ShowCatalog("Asociaciones",  "SELECT Id, Nombre, Activo FROM Asociaciones ORDER BY Nombre"));
        AddCatBtn("🗺 Zonas",          () => ShowCatalog("Zonas",         "SELECT z.Id, z.Nombre, a.Nombre AS Asociacion, z.Activo FROM Zonas z LEFT JOIN Asociaciones a ON z.AsociacionId=a.Id ORDER BY z.Nombre"));
        AddCatBtn("📍 Distritos",      () => ShowCatalog("Distritos",     "SELECT d.Id, d.Nombre, z.Nombre AS Zona, d.Activo FROM Distritos d LEFT JOIN Zonas z ON d.ZonaId=z.Id ORDER BY d.Nombre"));
        AddCatBtn("⛪ Iglesias",       () => ShowCatalog("Iglesias",      "SELECT ig.Id, ig.Nombre, d.Nombre AS Distrito, ig.Activo FROM Iglesias ig LEFT JOIN Distritos d ON ig.DistritoId=d.Id ORDER BY ig.Nombre"));
        AddCatBtn("🏅 Especialidades", () => ShowCatalog("Especialidades","SELECT Id, Nombre, Categoria, Activo FROM Especialidades ORDER BY Categoria, Nombre"));

        return page;
    }

    private static void ShowCatalog(string title, string sql)
    {
        var frm = new Form
        {
            Text            = "Catálogo — " + title,
            Size            = new Size(700, 500),
            StartPosition   = FormStartPosition.CenterParent,
            BackColor       = AppColors.MainBg,
            Font            = AppColors.FontBody
        };

        var grid = new DataGridView { Dock = DockStyle.Fill };
        AppColors.ApplyDataGrid(grid);

        try
        {
            var dt = DatabaseHelper.ExecuteQuery(sql, []);
            grid.DataSource = dt;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }

        var pnlNote = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = Color.White, Padding = new Padding(8, 4, 0, 0) };
        pnlNote.Controls.Add(new Label { Text = "ℹ Para agregar/editar registros, use SQL Server Management Studio o solicite mejoras al desarrollador.", Font = AppColors.FontSmall, ForeColor = AppColors.TextMuted, AutoSize = true, Location = new Point(8, 8) });

        frm.Controls.Add(grid);
        frm.Controls.Add(pnlNote);
        frm.ShowDialog();
    }

    // ── Información del sistema ───────────────────────────────────────────────

    private static TabPage BuildInfoTab()
    {
        var page = new TabPage("ℹ  Información del Sistema") { BackColor = AppColors.MainBg };

        var pnl = new Panel { BackColor = Color.White, Bounds = new Rectangle(16, 16, 520, 300) };

        void Row(string lbl, string val, ref int y)
        {
            pnl.Controls.Add(new Label { Text = lbl, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = AppColors.TextSecondary, AutoSize = true, Location = new Point(16, y) });
            pnl.Controls.Add(new Label { Text = val, Font = AppColors.FontBody, ForeColor = AppColors.TextPrimary, AutoSize = true, Location = new Point(200, y) });
            y += 26;
        }

        int y = 16;
        Row("Aplicación:",        "ConquiTap v1.0.0",  ref y);
        Row("Hecho con ♥ por:",   "Josue Custodio, 2025-0042", ref y);
        Row("Organización:",      "ADONE - IASD", ref y);
        Row("Base de Datos:",     "Microsoft SQL Server", ref y);
        Row("Framework:",         ".NET 10 Windows Forms", ref y);
        Row("Sesión actual:",     SessionManager.NombreParaMostrar, ref y);
        Row("Rol:",               SessionManager.RolParaMostrar, ref y);
        Row("Fecha del sistema:", DateTime.Now.ToString("dd/MM/yyyy HH:mm"), ref y);

        page.Controls.Add(pnl);
        return page;
    }
}
