using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ArmMordanizerGUI.Service
{
    public class MapperData
    {
        private IConfiguration Configuration;

        public MapperData(IConfiguration _configuration)
        {
            //_connectionDB = new ConnectionDb();
            Configuration = _configuration;

        }
        internal Mapper GetMapper(List<SelectListItem> objDestinationList1, List<SelectListItem> objSourceList)
        {
            List<MapTable> objMapList = new List<MapTable>();
            foreach (var objColumn in objDestinationList1)
            {
                MapTable mapTable = new MapTable();
                mapTable.sourceColumn = "";
                mapTable.targetColumn = "";
                objMapList.Add(mapTable);
            }
            objMapList.RemoveAt(objMapList.Count-2);
            Mapper mapper = new Mapper();
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList1, "Text", "Value");
            mapper.mapTables = objMapList;
            
            return mapper;
        }

        public string Save(Mapper obj)
        {
            try
            {
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);

                string sql = GetQuery(obj);
                string insertSql = "INSERT INTO [MapperConfiguration] ([SourceTable],[DestinationTable],[SQL],[IsActive],[CreatedDate]) VALUES (@SourceTable,@DestinationTable,@SQL,@IsActive,@CreatedDate)";

                using (SqlCommand cmd = new SqlCommand(insertSql, con))
                {
                    cmd.Parameters.AddWithValue("@SourceTable", obj.sourceTableName);
                    cmd.Parameters.AddWithValue("@DestinationTable", obj.destinationTableName);
                    cmd.Parameters.AddWithValue("@SQL", sql);
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);


                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                return "Insertion Successfull.";
            }
            catch (Exception ex)
            {
                return "Database Insertion failed. Please See the exception Message" + ex.Message.ToString(); 
            }
            
            

            
        }

        private string GetQuery(Mapper obj)
        {
            string selectSQl = "INSERT INTO " + obj.destinationTableName + " (";
            string valuesSql = "SELECT ";
            string finalSQL = "";
            foreach (var item in obj.mapTables)
            {
                if (item.targetColumn != null)
                    selectSQl = selectSQl + item.targetColumn + ",";
                if (item.sourceColumn != null)
                    valuesSql = valuesSql + item.sourceColumn + ",";
            }

            selectSQl = selectSQl.Remove(selectSQl.Length - 1, 1);
            selectSQl = selectSQl + ")" + System.Environment.NewLine;

            valuesSql = valuesSql.Remove(valuesSql.Length - 1, 1);
            valuesSql = valuesSql + " FROM " + obj.sourceTableName;

            finalSQL = selectSQl + valuesSql;
            return finalSQL;
        }
    }
}
