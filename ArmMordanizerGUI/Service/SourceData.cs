using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArmMordanizerGUI.Service
{
    public class SourceData
    {
        private IConfiguration Configuration;
        private string srctempTablePrefix = "TMP_RAW_"; 

        public SourceData(IConfiguration _configuration)
        {
            Configuration = _configuration;

        }
        public List<SelectListItem> GetSourceData(string v)
        {
            try
            {
                DataTable srcObjTable = new DataTable();
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);

                string srcQuery = @"SELECT name,system_type_name FROM sys.dm_exec_describe_first_result_set( N'SELECT * FROM " + v + "', NULL, 0 )";
                using (SqlCommand cmd = new SqlCommand(srcQuery, con))
                {
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
                objSourceList.Insert(0, new SelectListItem() { Value = "<ignore>", Text = "<ignore>" });
                return objSourceList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<SelectListItem> GetSourceTableInfo()
        {
            try
            {

                DataTable srcObjTable = new DataTable();
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);

                string srcQuery = @"SELECT TABLE_NAME,TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME like'TMP_RAW_%'";
                using (SqlCommand cmd = new SqlCommand(srcQuery, con))
                {

                    con.Open();
                    srcObjTable.Load(cmd.ExecuteReader());
                    con.Close();
                }
                List<string> objSourceTables = new List<string>();
                foreach (DataRow row in srcObjTable.Rows)
                {
                    objSourceTables.Add(String.Concat(row["TABLE_SCHEMA"].ToString(), ".", row["TABLE_NAME"].ToString()));

                }
                List<SelectListItem> objSourceTableList = objSourceTables.Select(x => new SelectListItem()
                {
                    Text = x,
                    Value = x
                }
                ).ToList();
                return objSourceTableList;
            }
            catch (Exception )
            {
                throw ;
            }
        }
    }
}
