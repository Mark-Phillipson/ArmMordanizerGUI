using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArmMordanizerGUI.Service
{
    public class DestinationData
    {
        //private ConnectionDb _connectionDB;
        private IConfiguration Configuration;

        public DestinationData(IConfiguration _configuration)
        {
            //_connectionDB = new ConnectionDb();
            Configuration = _configuration;


        }
        public List<SelectListItem> GetDestinationData(string v)
        {
            DataTable desObjTable = new DataTable();

            List<TableColumns> objDestinationColumns = new List<TableColumns>();
            string desQuery = @"SELECT name,system_type_name FROM sys.dm_exec_describe_first_result_set( N'SELECT * FROM "+v+"', NULL, 0 )";
            //string cName = "Data Source=192.168.250.57; Initial Catalog=AmmsOnlineCountry;User ID=sa;Password=Online@Ammsdb";
            string connString = this.Configuration.GetConnectionString("DefaultConnection");
            SqlConnection con = new SqlConnection(connString);
            using (SqlCommand cmd = new SqlCommand(desQuery, con))
            {
                //cmd.Parameters.AddWithValue("@TableName", v);

                con.Open();
                desObjTable.Load(cmd.ExecuteReader());
                con.Close();
            }
            foreach (DataRow row in desObjTable.Rows)
            {
                objDestinationColumns.Add(new TableColumns { ColumnName = row["name"].ToString(), ColumnType = row["system_type_name"].ToString() });

            }

            List<SelectListItem> objDestinationList = objDestinationColumns.Select(x => new SelectListItem()
            {
                Text = x.ColumnName,
                Value = x.ColumnName
            }
            ).ToList();
            objDestinationList.Insert(0, new SelectListItem() { Value = "0", Text = "-- Please select your Column --" });
            return objDestinationList;
        }
    }
}
