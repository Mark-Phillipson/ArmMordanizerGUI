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
            objMapList.RemoveAt(objMapList.Count - 2);
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

        internal bool IsSrcDesExists(string sourceTableName, string destinationTableName)
        {
            try
            {
                bool isExists;
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);

                string sql = "SELECT COUNT(*) FROM MapperConfiguration WHERE SourceTable = @SourceTable AND DestinationTable = @DestinationTable";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@SourceTable", sourceTableName);
                    cmd.Parameters.AddWithValue("@DestinationTable", destinationTableName);

                    con.Open();
                    isExists = (bool)cmd.ExecuteScalar();
                    con.Close();
                }
                return isExists;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        internal string? UpdateInsertMappingData(Mapper obj)
        {
            throw new NotImplementedException();
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

        public List<MapTable> GetConfiguarationData(string srcTableName, string desTableName)
        {
            List<MapTable> list = new List<MapTable>();
            try
            {
                string existingSql;
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);

                string sql = "SELECT SQL FROM MapperConfiguration WHERE SourceTable = @SourceTable AND DestinationTable = @DestinationTable AND IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@SourceTable", srcTableName);
                    cmd.Parameters.AddWithValue("@DestinationTable", desTableName);

                    con.Open();
                    existingSql = (string)cmd.ExecuteScalar();
                    con.Close();
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal Mapper GetMapper(List<SelectListItem> objDestinationList, List<SelectListItem> objSourceList, List<MapTable> objMapTable)
        {
            List<MapTable> objMapList = new List<MapTable>();
            foreach (var objColumn in objMapTable)
            {
                MapTable mapTable = new MapTable();
                mapTable.sourceColumn = objColumn.sourceColumn;
                mapTable.targetColumn = objColumn.targetColumn;
                objMapList.Add(mapTable);
            }
            if(objDestinationList.Count > objMapList.Count)
            {
                for (int i = 0; i < objDestinationList.Count - objMapList.Count; i++)
                {
                    MapTable mapTable = new MapTable();
                    mapTable.sourceColumn = "";
                    mapTable.targetColumn = "";
                    objMapList.Add(mapTable);
                }
            }
            objMapList.RemoveAt(objMapList.Count - 2);
            Mapper mapper = new Mapper();
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList, "Text", "Value");
            mapper.mapTables = objMapList;

            return mapper;
        }
    }
}
