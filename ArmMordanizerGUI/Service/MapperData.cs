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
            objMapList.RemoveAt(objMapList.Count - 1);
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
                    //cmd.Parameters.AddWithValue("@UpdatedDate", null);



                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                return "Data Saved Successfull.";
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
                    int res = (int)cmd.ExecuteScalar();
                    if (res == 0)
                        isExists = false;
                    else
                        isExists = true;
                    con.Close();
                }
                return isExists;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string UpdateMappingData(Mapper obj)
        {
            string connString = this.Configuration.GetConnectionString("DefaultConnection");
            string updateSql = "UPDATE MapperConfiguration SET IsActive = 0,UpdatedDate= @UpdatedDate WHERE SourceTable = @SourceTable AND DestinationTable = @DestinationTable";
            string sql = GetQuery(obj);
            string insertSql = "INSERT INTO [MapperConfiguration] ([SourceTable],[DestinationTable],[SQL],[IsActive],[CreatedDate]) VALUES (@SourceTable,@DestinationTable,@SQL,@IsActive,@CreatedDate)";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlTransaction transaction = null;
                try
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    using (SqlCommand cmd = new SqlCommand(updateSql, conn, transaction)) 
                    {
                        cmd.Parameters.AddWithValue("@SourceTable", obj.sourceTableName);
                        cmd.Parameters.AddWithValue("@DestinationTable", obj.destinationTableName);
                        cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                        cmd.ExecuteNonQuery(); 
                    }
                    using (SqlCommand cmd = new SqlCommand(insertSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@SourceTable", obj.sourceTableName);
                        cmd.Parameters.AddWithValue("@DestinationTable", obj.destinationTableName);
                        cmd.Parameters.AddWithValue("@SQL", sql);
                        cmd.Parameters.AddWithValue("@IsActive", 1);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        cmd.ExecuteNonQuery(); 
                    }
                    transaction.Commit();
                    conn.Close();
                    return "Data Saved Successfull.";
                }
                catch (Exception ex)
                {
                    // Attempt to roll back the transaction.
                    try
                    {
                        return "Database Insertion failed. Please See the exception Message" + ex.Message.ToString();
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        return "Database Insertion failed. Please See the exception Message" + ex.Message.ToString();
                    }
                }
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
                list = GetMapTableInfo(existingSql);
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<MapTable> GetMapTableInfo(string existingSql)
        {
            //INSERT INTO MasterCalendar (Date,Year,HolidayType)  SELECT Date, Year, HolidayType FROM MasterCalendar
            List<MapTable> mapTableList = new List<MapTable>();
            string[] data = existingSql.Split(new[] { "SELECT" }, StringSplitOptions.None);


            string[] srcTemp = data[0].Split(new[] { "(" }, StringSplitOptions.None);
            srcTemp[1] = srcTemp[1].Replace("&nbsp;", " ").Replace(")", "").Replace("\r", "").Replace("\n", "");
            srcTemp = srcTemp[1].Split(new[] { "," }, StringSplitOptions.None);

            string[] desTemp = data[1].Split(new[] { "FROM" }, StringSplitOptions.None);
            desTemp = desTemp[0].Split(new[] { "," }, StringSplitOptions.None);

            for (int i = 0; i < srcTemp.Length; i++)
            {
                MapTable obj = new MapTable();
                obj.sourceColumn = srcTemp[i].Trim();
                obj.targetColumn = desTemp[i].Trim();
                mapTableList.Add(obj);
            }

            return mapTableList;

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
            int count = objMapList.Count;
            if (objDestinationList.Count > count)
            {
                for (int i = 0; i < objDestinationList.Count - count; i++)
                {
                    MapTable mapTable = new MapTable();
                    mapTable.sourceColumn = "";
                    mapTable.targetColumn = "";
                    objMapList.Add(mapTable);
                }
            }
            objMapList.RemoveAt(objMapList.Count - 1);
            Mapper mapper = new Mapper();
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList, "Text", "Value");
            mapper.mapTables = objMapList;

            return mapper;
        }
    }
}
