using ConquiTap.Helpers;

namespace ConquiTap.Controls;

public class ucDashboard : UserControl
{
    public ucDashboard()
    {
        BackColor = AppColors.MainBg;
        InitializeComponent();
        LoadData();
    }

    private Panel pnlStats   = null!;
    private Panel pnlRecent  = null!;

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        AutoScroll = true;

        // Bienvenida
        var lblWelcome = new Label
        {
            Text      = $"Bienvenido, {SessionManager.NombreParaMostrar}",
            Font      = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = AppColors.TextPrimary,
            AutoSize  = true,
            Location  = new Point(0, 4)
        };

        var lblDate = new Label
        {
            Text      = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                            new System.Globalization.CultureInfo("es-ES")),
            Font      = AppColors.FontBody,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(0, 34)
        };

        Controls.AddRange(new Control[] { lblWelcome, lblDate });

        // Sección de tarjetas de estadísticas
        pnlStats = new Panel
        {
            Location  = new Point(0, 72),
            Height    = 130,
            Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        Controls.Add(pnlStats);

        // Sección recientes
        var lblReciente = new Label
        {
            Text      = "Miembros registrados recientemente",
            Font      = AppColors.FontSubhead,
            ForeColor = AppColors.TextPrimary,
            AutoSize  = true,
            Location  = new Point(0, 216)
        };
        Controls.Add(lblReciente);

        pnlRecent = new Panel
        {
            Location  = new Point(0, 244),
            Height    = 300,
            BackColor = Color.White,
            Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        pnlRecent.Paint += (_, e) =>
        {
            using var pen = new Pen(AppColors.BorderLight);
            e.Graphics.DrawRectangle(pen, 0, 0, pnlRecent.Width - 1, pnlRecent.Height - 1);
        };
        Controls.Add(pnlRecent);

        // Ajustar anchos al resize
        Resize += (_, _) =>
        {
            pnlStats.Width  = Width - 32;
            pnlRecent.Width = Width - 32;
        };
    }

    private void LoadData()
    {
        try
        {
            // Estadísticas
            var dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetDashboardStats");
            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                pnlStats.Controls.Clear();
                int x = 0;

                AddStatCard(pnlStats, ref x, "Total Miembros",  row["TotalMiembros"].ToString()!,  AppColors.Denim,  "👥");
                AddStatCard(pnlStats, ref x, "Total Clubes",    row["TotalClubes"].ToString()!,    AppColors.Ming,   "🏕");
                AddStatCard(pnlStats, ref x, "Directivos",      row["TotalDirectivos"].ToString()!, Color.FromArgb(0,150,136), "⭐");
                if (SessionManager.EsAdministrador)
                    AddStatCard(pnlStats, ref x, "Usuarios",    row["TotalUsuarios"].ToString()!,   AppColors.BtnWarning, "🔑");
            }

            // Miembros recientes
            var dtR = DatabaseHelper.ExecuteStoredProcedure("sp_GetMiembrosRecientes", new() { ["@Top"] = 8 });
            BuildRecentGrid(dtR);
        }
        catch (Exception ex)
        {
            pnlStats.Controls.Add(new Label
            {
                Text      = "Error al cargar datos: " + ex.Message,
                ForeColor = AppColors.Danger,
                AutoSize  = true,
                Location  = new Point(4, 4)
            });
        }
    }

    private static void AddStatCard(Panel container, ref int x, string titulo, string valor, Color color, string emoji)
    {
        var card = new Panel
        {
            BackColor = Color.White,
            Bounds    = new Rectangle(x, 0, 195, 120),
            Cursor    = Cursors.Default
        };

        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            // borde izquierdo de color
            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, 0, 0, 5, card.Height);
            using var pen = new Pen(AppColors.BorderLight);
            g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        card.Controls.Add(new Label
        {
            Text      = emoji + "  " + titulo,
            Font      = new Font("Segoe UI", 9),
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Location  = new Point(16, 18)
        });

        card.Controls.Add(new Label
        {
            Text      = valor,
            Font      = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = color,
            AutoSize  = true,
            Location  = new Point(14, 42)
        });

        container.Controls.Add(card);
        x += 207;
    }

    private void BuildRecentGrid(System.Data.DataTable dt)
    {
        pnlRecent.Controls.Clear();

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            DataSource = dt
        };
        AppColors.ApplyDataGrid(grid);

        // Renombrar columnas
        grid.DataBindingComplete += (_, _) =>
        {
            if (grid.Columns.Contains("NombreCompleto")) grid.Columns["NombreCompleto"]!.HeaderText = "Nombre";
            if (grid.Columns.Contains("Categoria"))      grid.Columns["Categoria"]!.HeaderText      = "Categoría";
            if (grid.Columns.Contains("Clase"))          grid.Columns["Clase"]!.HeaderText          = "Clase";
            if (grid.Columns.Contains("Club"))           grid.Columns["Club"]!.HeaderText           = "Club";
            if (grid.Columns.Contains("FechaRegistro"))  grid.Columns["FechaRegistro"]!.HeaderText  = "Registrado";
            if (grid.Columns.Contains("Id"))             grid.Columns["Id"]!.Visible                = false;
        };

        pnlRecent.Controls.Add(grid);
    }
}
