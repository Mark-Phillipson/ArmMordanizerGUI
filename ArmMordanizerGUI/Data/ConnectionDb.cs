using Microsoft.Data.SqlClient;

namespace ArmMordanizerGUI.Data
{
    public class ConnectionDb
    {
        //private IConfiguration Configuration;
        //private readonly string _connectionString = ConfigurationManager.ConnectionStrings["ArmConnection"].ConnectionString;
        private static string cName = "Data Source=192.168.250.57; Initial Catalog=AmmsOnlineCountry;User ID=sa;Password=Online@Ammsdb";

        public SqlConnection con;
        public ConnectionDb()
        {
            //string connString = this.Configuration.GetConnectionString("DefaultConnection");
            con = new SqlConnection(cName);
        }
    }
}
