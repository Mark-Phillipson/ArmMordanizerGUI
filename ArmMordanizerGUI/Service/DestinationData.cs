using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArmMordanizerGUI.Service
{
    public class DestinationData
    {
        private IConfiguration Configuration;

        public DestinationData(IConfiguration _configuration)
        {
            Configuration = _configuration;


        }
        public List<SelectListItem> GetDestinationData(string v)
        {
            DataTable desObjTable = new DataTable();

            List<TableColumns> objDestinationColumns = new List<TableColumns>();
            string desQuery = @"SELECT name,system_type_name FROM sys.dm_exec_describe_first_result_set( N'SELECT * FROM "+v+"', NULL, 0 )";
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
            objDestinationList.Insert(0, new SelectListItem() { Value = "", Text = "-- Please select your Column --" });
            return objDestinationList;
        }

        internal List<SelectListItem> GetDestinationTableInfo()
        {

            try
            {

                DataTable desObjTable = new DataTable();
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);

                //string srcQuery = @"SELECT TABLE_NAME,TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES";
                string srcQuery = @"SELECT [Table],[Schema] FROM [dbo].[A_TablePermit] WHERE [Uploadable] = 1";

                using (SqlCommand cmd = new SqlCommand(srcQuery, con))
                {

                    con.Open();
                    desObjTable.Load(cmd.ExecuteReader());
                    con.Close();
                }
                List<string> objDestinationTables = new List<string>();
                foreach (DataRow row in desObjTable.Rows)
                {
                    objDestinationTables.Add(String.Concat(row["Schema"].ToString(), ".","[", row["Table"].ToString(),"]"));

                }
                List<SelectListItem> objDestinationTableList = objDestinationTables.Select(x => new SelectListItem()
                {
                    Text = x,
                    Value = x
                }
                ).ToList();
                return objDestinationTableList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
