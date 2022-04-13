using Microsoft.Data.SqlClient;

namespace ArmMordanizerGUI.Data
{
    public class ConnectionDb
    {
        private static string cName = "";

        public SqlConnection con;
        public ConnectionDb()
        {
            con = new SqlConnection(cName);
        }
    }
}
