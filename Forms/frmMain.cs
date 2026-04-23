using ConquiTap.Controls;
using ConquiTap.Helpers;

namespace ConquiTap.Forms;

public class frmMain : Form
{
    // ── Sidebar ──────────────────────────────────────────────────────────────
    private Panel  pnlSidebar     = null!;
    private Panel  pnlTopBar      = null!;
    private Panel  pnlContent     = null!;
    private Button btnToggle      = null!;
    private Label  lblPageTitle   = null!;
    private Label  lblUserName    = null!;
    private Label  lblUserRole    = null!;

    private bool _sidebarExpanded = true;

    // Menú items: (icono MDL2, texto, nombre)
    private readonly (string Icon, string Text, string Name)[] _menuItems =
    {
        ("\uE80F", "Dashboard",  "dashboard"),
        ("\uE716", "Miembros",   "miembros"),
        ("\uE77B", "Clubes",     "clubes"),
        ("\uE77B", "Mi Perfil",  "perfil"),
        ("\uE728", "Usuarios",   "usuarios"),
        ("\uE713", "Sistema",    "sistema")
    };

    private readonly Dictionary<string, Button> _menuButtons = new();
    private string _seccionActual = "dashboard";
    private UserControl? _currentControl;

    public frmMain()
    {
        InitializeComponent();
        NavTo("dashboard");
    }

    private void InitializeComponent()
    {
        Text            = "ConquiTap — ADONE";
        Size            = new Size(1200, 720);
        MinimumSize     = new Size(900, 600);
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = AppColors.MainBg;
        Font            = AppColors.FontBody;
        WindowState     = FormWindowState.Maximized;

        // IMPORTANTE: el orden de adición determina el z-order del docking.
        // Fill primero, luego Top, luego Left (Left se aplica primero al tener mayor z-order).
        BuildContentArea();
        BuildTopBar();
        BuildSidebar();
    }

    // ─── Sidebar ─────────────────────────────────────────────────────────────

    private void BuildSidebar()
    {
        pnlSidebar = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = AppColors.SidebarWidthExpanded,
            BackColor = AppColors.SidebarBg
        };

        // Logo / nombre app
        var pnlLogo = new Panel
        {
            Bounds    = new Rectangle(0, 0, AppColors.SidebarWidthExpanded, AppColors.HeaderHeight),
            BackColor = AppColors.SidebarBg
        };

        var lblLogo = new Label
        {
            Text      = "ConquiTap",
            Font      = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Bounds    = new Rectangle(16, 0, 180, AppColors.HeaderHeight),
            Name      = "lblLogoText"
        };
        pnlLogo.Controls.Add(lblLogo);
        pnlSidebar.Controls.Add(pnlLogo);

        // Separador
        var sep = new Panel
        {
            BackColor = AppColors.SidebarHover,
            Bounds    = new Rectangle(12, AppColors.HeaderHeight, AppColors.SidebarWidthExpanded - 24, 1)
        };
        pnlSidebar.Controls.Add(sep);

        // Etiqueta sección
        var lblMenu = new Label
        {
            Text      = "MENÚ",
            Font      = new Font("Segoe UI", 7, FontStyle.Bold),
            ForeColor = AppColors.SidebarSubtext,
            Bounds    = new Rectangle(16, AppColors.HeaderHeight + 12, 160, 20),
            Name      = "lblMenuLabel"
        };
        pnlSidebar.Controls.Add(lblMenu);

        // Botones de menú
        int yPos = AppColors.HeaderHeight + 38;
        foreach (var item in _menuItems)
        {
            // Respetar permisos
            if (item.Name == "usuarios" && !SessionManager.EsDirectivo) continue;
            if (item.Name == "sistema"  && !SessionManager.EsAdministrador) continue;

            var btn = CreateMenuButton(item.Icon, item.Text, item.Name);
            btn.Location = new Point(0, yPos);
            pnlSidebar.Controls.Add(btn);
            _menuButtons[item.Name] = btn;
            yPos += AppColors.MenuItemHeight;
        }

        // Información de usuario al fondo
        var pnlUser = new Panel
        {
            BackColor = Color.FromArgb(20, Color.Black),
            Height    = 72,
            Dock      = DockStyle.Bottom
        };

        var lblAvatarCircle = new Label
        {
            Text      = SessionManager.NombreParaMostrar.Length > 0
                        ? SessionManager.NombreParaMostrar[0].ToString().ToUpper() : "?",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = AppColors.Ming,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds    = new Rectangle(10, 14, 44, 44)
        };

        lblUserName = new Label
        {
            Text      = SessionManager.NombreParaMostrar,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.White,
            Bounds    = new Rectangle(62, 18, 148, 18),
            Name      = "lblUserNameSide"
        };

        lblUserRole = new Label
        {
            Text      = SessionManager.RolParaMostrar,
            Font      = new Font("Segoe UI", 8),
            ForeColor = AppColors.SidebarSubtext,
            Bounds    = new Rectangle(62, 38, 148, 18),
            Name      = "lblUserRoleSide"
        };

        var btnLogout = new Button
        {
            Text      = "⏻",
            Font      = new Font("Segoe UI", 13),
            ForeColor = AppColors.SidebarSubtext,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Bounds    = new Rectangle(190, 22, 28, 28)
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (_, _) => Logout();
        btnLogout.MouseEnter += (_, _) => btnLogout.ForeColor = Color.White;
        btnLogout.MouseLeave += (_, _) => btnLogout.ForeColor = AppColors.SidebarSubtext;

        pnlUser.Controls.AddRange(new Control[] { lblAvatarCircle, lblUserName, lblUserRole, btnLogout });
        pnlSidebar.Controls.Add(pnlUser);

        Controls.Add(pnlSidebar);
    }

    private Button CreateMenuButton(string icon, string text, string name)
    {
        var btn = new Button
        {
            Name      = "btn_" + name,
            Tag       = name,
            Size      = new Size(AppColors.SidebarWidthExpanded, AppColors.MenuItemHeight),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(200, 255, 255, 255),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor    = Cursors.Hand,
            Padding   = new Padding(0)
        };
        btn.FlatAppearance.BorderSize    = 0;
        btn.FlatAppearance.MouseOverBackColor = AppColors.SidebarHover;

        // Dibujo personalizado: icono + texto
        btn.Paint += (sender, e) =>
        {
            var b      = (Button)sender!;
            bool active = (string)b.Tag! == _seccionActual;
            var g      = e.Graphics;
            g.Clear(active ? AppColors.SidebarActive : Color.Transparent);

            // Barra indicadora activa
            if (active)
                using (var brush = new SolidBrush(AppColors.Ming))
                    g.FillRectangle(brush, 0, 0, 4, b.Height);

            // Icono (Segoe MDL2 Assets)
            using var iconFont  = new Font("Segoe MDL2 Assets", 14);
            using var iconBrush = new SolidBrush(active ? Color.White : Color.FromArgb(180, 255, 255, 255));
            int iconX = _sidebarExpanded ? 16 : 14;
            g.DrawString(icon, iconFont, iconBrush, new RectangleF(iconX, 0, 28, b.Height),
                new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

            if (_sidebarExpanded)
            {
                using var textFont  = new Font("Segoe UI", 10, active ? FontStyle.Bold : FontStyle.Regular);
                using var textBrush = new SolidBrush(active ? Color.White : Color.FromArgb(210, 255, 255, 255));
                g.DrawString(text, textFont, textBrush, new RectangleF(50, 0, b.Width - 60, b.Height),
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            }
        };

        btn.Click += (_, _) => NavTo((string)btn.Tag!);
        return btn;
    }

    // ─── Top Bar ─────────────────────────────────────────────────────────────

    private void BuildTopBar()
    {
        pnlTopBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = AppColors.HeaderHeight,
            BackColor = Color.White
        };

        // Sombra inferior
        pnlTopBar.Paint += (_, e) =>
        {
            using var pen = new Pen(AppColors.BorderLight);
            e.Graphics.DrawLine(pen, 0, pnlTopBar.Height - 1, pnlTopBar.Width, pnlTopBar.Height - 1);
        };

        // Toggle sidebar — posición fija desde la izquierda del topbar (que ya NO incluye el área del sidebar)
        btnToggle = new Button
        {
            Text      = "☰",
            Font      = new Font("Segoe UI", 16),
            ForeColor = AppColors.TextSecondary,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Cursor    = Cursors.Hand,
            Bounds    = new Rectangle(8, 8, 40, 40)
        };
        btnToggle.FlatAppearance.BorderSize = 0;
        btnToggle.Click += ToggleSidebar;

        // Título de página
        lblPageTitle = new Label
        {
            Text      = "Dashboard",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = AppColors.TextPrimary,
            AutoSize  = true,
            Location  = new Point(56, 14)
        };

        // Saludo usuario (derecha)
        var lblGreet = new Label
        {
            Text      = $"Hola, {SessionManager.NombreParaMostrar}",
            Font      = AppColors.FontBody,
            ForeColor = AppColors.TextSecondary,
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right
        };
        lblGreet.Location = new Point(Width - lblGreet.PreferredWidth - 16, 18);

        pnlTopBar.Controls.AddRange(new Control[] { btnToggle, lblPageTitle, lblGreet });

        // Ajustar posición cuando cambie el tamaño
        pnlTopBar.Resize += (_, _) =>
        {
            lblGreet.Location = new Point(pnlTopBar.Width - lblGreet.PreferredWidth - 16, 18);
        };

        Controls.Add(pnlTopBar);
        // NO llamar BringToFront aquí — el sidebar (agregado después) debe tener mayor z-order
    }

    // ─── Área de contenido ───────────────────────────────────────────────────

    private void BuildContentArea()
    {
        pnlContent = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppColors.MainBg,
            Padding   = new Padding(16)
        };
        Controls.Add(pnlContent);
    }

    // ─── Navegación ──────────────────────────────────────────────────────────

    public void NavTo(string section)
    {
        _seccionActual = section;

        // Actualizar título
        string titulo = section switch
        {
            "dashboard" => "Dashboard",
            "miembros"  => "Miembros",
            "clubes"    => "Clubes",
            "perfil"    => "Mi Perfil",
            "usuarios"  => "Usuarios",
            "sistema"   => "Sistema",
            _           => section
        };
        if (lblPageTitle != null) lblPageTitle.Text = titulo;

        // Refrescar botones
        foreach (var b in _menuButtons.Values) b.Invalidate();

        // Cargar control
        UserControl? newControl = section switch
        {
            "dashboard" => new ucDashboard(),
            "miembros"  => new ucMiembros(),
            "clubes"    => new ucClubes(),
            "perfil"    => new ucPerfil(),
            "usuarios"  => new ucUsuarios(),
            "sistema"   => new ucSistema(),
            _           => null
        };

        if (newControl == null) return;

        _currentControl?.Dispose();
        pnlContent.Controls.Clear();

        newControl.Dock = DockStyle.Fill;
        pnlContent.Controls.Add(newControl);
        _currentControl = newControl;
    }

    // ─── Toggle sidebar ──────────────────────────────────────────────────────

    private void ToggleSidebar(object? sender, EventArgs e)
    {
        _sidebarExpanded = !_sidebarExpanded;
        int targetWidth  = _sidebarExpanded
            ? AppColors.SidebarWidthExpanded
            : AppColors.SidebarWidthCollapsed;

        pnlSidebar.Width = targetWidth;

        // Mostrar/ocultar textos del sidebar
        foreach (Control c in pnlSidebar.Controls)
        {
            if (c.Name is "lblMenuLabel" or "lblMenuSection" or "lblLogoText")
                c.Visible = _sidebarExpanded;
        }

        // Ajustar ancho de botones de menú
        foreach (var btn in _menuButtons.Values)
        {
            btn.Width = targetWidth;
            btn.Invalidate();
        }

        // Ajustar visibilidad en panel de usuario
        foreach (Control c in pnlSidebar.Controls)
        {
            if (c is Panel p && p.Dock == DockStyle.Bottom)
                foreach (Control child in p.Controls)
                    if (child.Name is "lblUserNameSide" or "lblUserRoleSide")
                        child.Visible = _sidebarExpanded;
        }
    }

    private void Logout()
    {
        var r = MessageBox.Show(
            "¿Deseas cerrar sesión?",
            "Cerrar Sesión",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (r != DialogResult.Yes) return;

        DatabaseHelper.LogActividad(SessionManager.UsuarioActual?.Id, "Cierre de sesión");
        SessionManager.CerrarSesion();
        _currentControl?.Dispose();
        Close();
    }
}
