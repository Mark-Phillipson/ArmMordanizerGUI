using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace ArmMordanizerGUI.Service
{
    public class MapperData
    {
        private IConfiguration Configuration;
        private string fileLocationForReUploadPropertyName = "ReUpload";
        private string fileLocationForUpload = "UploadQueue";
        private string fileLocationForUploadCompleted = "UploadCompletePath";

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
                var sourceColumn = objSourceList.FirstOrDefault(x => x.Text == objColumn.Text);
                if (sourceColumn != null)
                {
                    mapTable.sourceColumn = sourceColumn.Text;
                    mapTable.targetColumn = objColumn.Text;
                }
                else
                {
                    mapTable.sourceColumn = "";
                    mapTable.targetColumn = objColumn.Text;
                }

                objMapList.Add(mapTable);
            }
            //objMapList.RemoveAt(objMapList.Count - 1);
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

                string sql = "SELECT COUNT(*) FROM MapperConfiguration WHERE SourceTable = @SourceTable AND DestinationTable = @DestinationTable AND IsActive = 1";

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

        internal void MoveFileToReUpload(string sourceTableName)
        {
            string fileName = GetFileName(sourceTableName.Split(new string[] { "." }, StringSplitOptions.None).Last());

            string fileLocationForReupload = GetFileLocation(fileLocationForReUploadPropertyName);
            if (!fileLocationForReupload.EndsWith("\\"))
            {
                fileLocationForReupload = fileLocationForReupload + "\\";
            }

            string fileLocationForQueue = GetFileLocation(fileLocationForUpload);
            if (!fileLocationForQueue.EndsWith("\\"))
            {
                fileLocationForQueue = fileLocationForQueue + "\\";
            }

            string fileLocationForUploadComplete = GetFileLocation(fileLocationForUploadCompleted);
            if (!fileLocationForUploadComplete.EndsWith("\\"))
            {
                fileLocationForUploadComplete = fileLocationForUploadComplete + "\\";
            }

            int isFileExists = CopyFileToReUploadFolder(fileName, fileLocationForReupload, fileLocationForUploadComplete);
            if (isFileExists == 0)
            {
                CopyFileToUploadQueue(fileName, fileLocationForQueue, fileLocationForUploadComplete);
            }

        }

        private void CopyFileToUploadQueue(string fileName, string fileLocationForUpload, string fileLocationForUploadComplete)
        {

            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(fileLocationForUploadComplete);

            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + fileName + "*.*");

            var filesCompleted = filesInDir[filesInDir.Length - 1].FullName;//Directory.GetFiles(fileLocationForUploadCompleted, fileName + "00" + ".*").FirstOrDefault();

            string fileToMove = fileLocationForUploadComplete + Path.GetFileName(filesCompleted);
            string moveTo = fileLocationForUpload;

            //moving file
            File.Copy(fileToMove, Path.Combine(fileLocationForUpload, fileName + Path.GetExtension(filesCompleted)), true);
        }

        private int CopyFileToReUploadFolder(string fileName, string fileLocationForReupload, string fileLocationForUploadCompleted)
        {
            if (!Directory.Exists(fileLocationForReupload))
                Directory.CreateDirectory(fileLocationForReupload);

            var files = Directory.GetFiles(fileLocationForReupload, fileName + ".*");

            DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(fileLocationForUploadCompleted);

            FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + fileName + "*.*");

            var filesCompleted = filesInDir[filesInDir.Length - 1].FullName;//Directory.GetFiles(fileLocationForUploadCompleted, fileName + "00" + ".*").FirstOrDefault();
            if (files.Length == 0)
            {
                string fileToMove = fileLocationForUploadCompleted + Path.GetFileName(filesCompleted);
                string moveTo = fileLocationForReupload;

                //moving file
                File.Copy(fileToMove, Path.Combine(fileLocationForReupload, fileName + Path.GetExtension(filesCompleted)), true);
                return 0;
            }
            return 1;
        }

        private string GetFileLocation(string propertyName)
        {
            string folderLocation = "";
            string connString = this.Configuration.GetConnectionString("DefaultConnection");
            //string sql = "Select PropertyValue from [SystemGlobalProperties] WHERE [PropertyName] = @propertyName";
            string sql = "select [dbo].[fnGlobalProperty](@propertyName) AS PropertyValue";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@propertyName", propertyName);

                        folderLocation = (string)cmd.ExecuteScalar();
                    }
                    conn.Close();
                }
                return folderLocation;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetFileName(string tableName)
        {
            string fileName = "";
            string connString = this.Configuration.GetConnectionString("DefaultConnection");
            string sql = "SELECT DISTINCT FileName From FileStore Where TableName = @tableName";
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@tableName", tableName);

                        fileName = (string)cmd.ExecuteScalar();
                    }
                    conn.Close();
                }
                return fileName;
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
                if (item.targetColumn != null && item.sourceColumn != null)
                {
                    selectSQl = selectSQl + item.targetColumn + ",";
                    valuesSql = valuesSql + item.sourceColumn + ",";
                }
                //if (item.sourceColumn != null && item.targetColumn != null)
                //valuesSql = valuesSql + item.sourceColumn + ",";
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


            string[] desTemp = data[0].Split(new[] { "(" }, StringSplitOptions.None);
            desTemp[1] = desTemp[1].Replace("&nbsp;", " ").Replace(")", "").Replace("\r", "").Replace("\n", "");
            desTemp = desTemp[1].Split(new[] { "," }, StringSplitOptions.None);

            string[] srcTemp = data[1].Split(new[] { "FROM" }, StringSplitOptions.None);
            srcTemp = srcTemp[0].Split(new[] { "," }, StringSplitOptions.None);

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

            List<SelectListItem> result = objDestinationList
                .ExceptBy(objMapList.Select(msg => msg.targetColumn), msg => msg.Text)
                .ToList();

            if (objDestinationList.Count > count)
            {
                for (int i = 1; i < result.Count; i++)
                {
                    MapTable mapTable = new MapTable();
                    var sourceColumn = objSourceList.FirstOrDefault(x => x.Text == result[i].Text);
                    //var desColumn = objMapList.FirstOrDefault(x => x.targetColumn == objDestinationList[i].Text);

                    if (sourceColumn != null)
                    {
                        mapTable.sourceColumn = sourceColumn.Text;
                        mapTable.targetColumn = result[i].Text;
                    }
                    else
                    {
                        mapTable.sourceColumn = "";
                        mapTable.targetColumn = result[i].Text;
                    }
                    //mapTable.sourceColumn = "";
                    //mapTable.targetColumn = "";
                    objMapList.Add(mapTable);
                }
            }
            //objMapList.RemoveAt(objMapList.Count - 1);
            Mapper mapper = new Mapper();
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList, "Text", "Value");
            mapper.mapTables = objMapList;

            return mapper;
        }
    }
}
