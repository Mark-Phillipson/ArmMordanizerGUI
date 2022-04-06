using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArmMordanizerGUI.Service
{
    public class SourceData
    {
        //private ConnectionDb _connectionDB;
        private IConfiguration Configuration;

        public SourceData(IConfiguration _configuration)
        {
            //_connectionDB = new ConnectionDb();
            Configuration = _configuration;


        }
        public List<SelectListItem> GetSourceData(string v)
        {
            DataTable srcObjTable = new DataTable();
            //string cName = "Data Source=192.168.250.57; Initial Catalog=AmmsOnlineCountry;User ID=sa;Password=Online@Ammsdb";
            string connString = this.Configuration.GetConnectionString("DefaultConnection");

            SqlConnection con = new SqlConnection(connString);

        string srcQuery = @"SELECT name,system_type_name FROM sys.dm_exec_describe_first_result_set( N'SELECT * FROM "+v+"', NULL, 0 )";
            using (SqlCommand cmd = new SqlCommand(srcQuery, con))
            {
                //cmd.Parameters.AddWithValue("@TableName", v);

                con.Open();
                srcObjTable.Load(cmd.ExecuteReader());
                con.Close();
            }
            List<TableColumns> objSourceColumns = new List<TableColumns>();
            foreach (DataRow row in srcObjTable.Rows)
            {
                objSourceColumns.Add(new TableColumns { ColumnName = row["name"].ToString(), ColumnType = row["system_type_name"].ToString() });

            }
            List<SelectListItem> objSourceList = objSourceColumns.Select(x => new SelectListItem()
            {
                Text = x.ColumnName,
                Value = x.ColumnName
            }
            ).ToList();
            objSourceList.Insert(0, new SelectListItem() { Value = "0", Text = "-- Please select your Column --" });
            return objSourceList;
        }
    }
}
