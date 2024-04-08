using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace simecFacs
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string host = "localhost";
            int puerto = 3306;
            string usuario = "root";
            string contraseña = "";

            string cadenaConexionMySQL = $"Server={host};Port={puerto};Uid={usuario};Pwd={contraseña};";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(cadenaConexionMySQL));
        }
    }
}
