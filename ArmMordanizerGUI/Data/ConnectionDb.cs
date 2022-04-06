using Microsoft.Data.SqlClient;

namespace ArmMordanizerGUI.Data
{
    public class ConnectionDb
    {
        //private IConfiguration _configuration;
        //private readonly string _connectionString = ConfigurationManager.ConnectionStrings["ArmConnection"].ConnectionString;
        private static string cName = "Data Source=192.168.250.57; Initial Catalog=AmmsOnlineCountry;User ID=sa;Password=Online@Ammsdb";

        public SqlConnection con;
        public ConnectionDb()
        {
            //_configuration = configuration;
            //string connString = this._configuration.GetConnectionString("DefaultConnection");

            //string connString = this.Configuration.GetConnectionString("DefaultConnection");
            con = new SqlConnection(cName);
        }
    }
}
