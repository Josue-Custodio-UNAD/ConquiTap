using ConquiTap.Forms;
using ConquiTap.Helpers;

namespace ConquiTap;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);


        if (!DatabaseHelper.TestConnection())
        {
            MessageBox.Show(
                "No se pudo conectar a la base de datos.\n\n" +
                "Verifique la cadena de conexión en el archivo App.config\n" +
                "y asegúrese de que SQL Server esté en ejecución.",
                "Error de Conexión — ConquiTap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!DatabaseHelper.AdminExists())
        {
            using var setup = new frmSetupAdmin();
            if (setup.ShowDialog() != DialogResult.OK)
                return;
        }

        Application.Run(new frmLogin());
    }
}
