namespace ConquiTap;

/// <summary>Paleta de colores y constantes visuales de ConquiTap.</summary>
public static class AppColors
{
    // ── Colores institucionales (Iglesia Adventista del Séptimo Día) ─────────
    /// <summary>Denim #2f557f — color primario</summary>
    public static readonly Color Denim = ColorTranslator.FromHtml("#2f557f");

    /// <summary>Ming #3e8391 — color secundario</summary>
    public static readonly Color Ming = ColorTranslator.FromHtml("#3e8391");

    // ── Sidebar ──────────────────────────────────────────────────────────────
    public static readonly Color SidebarBg       = ColorTranslator.FromHtml("#243d5c");   // Denim oscuro
    public static readonly Color SidebarHover    = ColorTranslator.FromHtml("#2f557f");   // Denim
    public static readonly Color SidebarActive   = ColorTranslator.FromHtml("#3e8391");   // Ming
    public static readonly Color SidebarText     = Color.White;
    public static readonly Color SidebarSubtext  = ColorTranslator.FromHtml("#a8c0d6");

    // ── Fondo principal ──────────────────────────────────────────────────────
    public static readonly Color MainBg          = ColorTranslator.FromHtml("#f4f6f9");
    public static readonly Color CardBg          = Color.White;
    public static readonly Color HeaderBg        = Color.White;

    // ── Texto ────────────────────────────────────────────────────────────────
    public static readonly Color TextPrimary     = ColorTranslator.FromHtml("#2c3e50");
    public static readonly Color TextSecondary   = ColorTranslator.FromHtml("#6b7280");
    public static readonly Color TextMuted       = ColorTranslator.FromHtml("#9ca3af");

    // ── Botones ──────────────────────────────────────────────────────────────
    public static readonly Color BtnPrimary      = ColorTranslator.FromHtml("#2f557f");
    public static readonly Color BtnPrimaryHover = ColorTranslator.FromHtml("#243d5c");
    public static readonly Color BtnSuccess      = ColorTranslator.FromHtml("#3e8391");
    public static readonly Color BtnDanger       = ColorTranslator.FromHtml("#dc3545");
    public static readonly Color BtnWarning      = ColorTranslator.FromHtml("#f59e0b");

    // ── Bordes / divisores ───────────────────────────────────────────────────
    public static readonly Color BorderLight     = ColorTranslator.FromHtml("#e5e7eb");
    public static readonly Color BorderMedium    = ColorTranslator.FromHtml("#d1d5db");

    // ── Estados ──────────────────────────────────────────────────────────────
    public static readonly Color Success         = ColorTranslator.FromHtml("#22c55e");
    public static readonly Color Danger          = ColorTranslator.FromHtml("#ef4444");
    public static readonly Color Warning         = ColorTranslator.FromHtml("#f59e0b");
    public static readonly Color Info            = ColorTranslator.FromHtml("#3b82f6");

    // ── Tipografía ───────────────────────────────────────────────────────────
    public static readonly Font FontTitle   = new("Segoe UI", 22, FontStyle.Bold);
    public static readonly Font FontHeading = new("Segoe UI", 14, FontStyle.Bold);
    public static readonly Font FontSubhead = new("Segoe UI", 11, FontStyle.Bold);
    public static readonly Font FontBody    = new("Segoe UI", 10);
    public static readonly Font FontSmall   = new("Segoe UI", 9);
    public static readonly Font FontIcon    = new("Segoe MDL2 Assets", 16);
    public static readonly Font FontIconSm  = new("Segoe MDL2 Assets", 12);

    // ── Dimensiones de la barra lateral ──────────────────────────────────────
    public const int SidebarWidthExpanded  = 220;
    public const int SidebarWidthCollapsed = 52;
    public const int HeaderHeight          = 56;
    public const int MenuItemHeight        = 46;

    // ── Helpers de UI ────────────────────────────────────────────────────────

    /// <summary>Aplica estilo moderno a un Button primario.</summary>
    public static void ApplyPrimaryButton(Button btn, Color? bg = null)
    {
        btn.FlatStyle      = FlatStyle.Flat;
        btn.BackColor      = bg ?? BtnPrimary;
        btn.ForeColor      = Color.White;
        btn.Font           = new Font("Segoe UI", 10, FontStyle.Bold);
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor         = Cursors.Hand;
        btn.Height         = 38;

        btn.MouseEnter += (_, _) => btn.BackColor = DarkenColor(btn.BackColor, 15);
        btn.MouseLeave += (_, _) => btn.BackColor = bg ?? BtnPrimary;
    }

    /// <summary>Aplica estilo outline a un Button.</summary>
    public static void ApplyOutlineButton(Button btn, Color? color = null)
    {
        Color c = color ?? Denim;
        btn.FlatStyle      = FlatStyle.Flat;
        btn.BackColor      = Color.White;
        btn.ForeColor      = c;
        btn.Font           = new Font("Segoe UI", 10);
        btn.FlatAppearance.BorderColor = c;
        btn.FlatAppearance.BorderSize  = 1;
        btn.Cursor         = Cursors.Hand;
        btn.Height         = 38;
    }

    /// <summary>Aplica estilo moderno a un TextBox.</summary>
    public static void ApplyTextBox(TextBox tb)
    {
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.Font        = FontBody;
        tb.BackColor   = Color.White;
        tb.ForeColor   = TextPrimary;
        tb.Height      = 30;
    }

    /// <summary>Aplica estilo a un DataGridView.</summary>
    public static void ApplyDataGrid(DataGridView grid)
    {
        grid.BorderStyle             = BorderStyle.None;
        grid.BackgroundColor         = CardBg;
        grid.DefaultCellStyle.Font   = FontBody;
        grid.DefaultCellStyle.ForeColor        = TextPrimary;
        grid.DefaultCellStyle.BackColor        = CardBg;
        grid.DefaultCellStyle.SelectionBackColor   = ColorTranslator.FromHtml("#dbeafe");
        grid.DefaultCellStyle.SelectionForeColor   = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font    = new Font("Segoe UI", 10, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.BackColor   = Denim;
        grid.ColumnHeadersDefaultCellStyle.ForeColor   = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Padding    = new Padding(8, 0, 8, 0);
        grid.ColumnHeadersBorderStyle   = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight        = 40;
        grid.RowTemplate.Height         = 36;
        grid.GridColor                  = BorderLight;
        grid.CellBorderStyle            = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.AlternatingRowsDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#f8fafc");
        grid.EnableHeadersVisualStyles  = false;
        grid.SelectionMode              = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect                = false;
        grid.ReadOnly                   = true;
        grid.AllowUserToAddRows         = false;
        grid.AllowUserToDeleteRows      = false;
        grid.AutoSizeColumnsMode        = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible          = false;
    }

    private static Color DarkenColor(Color c, int amount)
    {
        return Color.FromArgb(
            Math.Max(0, c.R - amount),
            Math.Max(0, c.G - amount),
            Math.Max(0, c.B - amount));
    }
}
