using ArmMordanizerGUI.Models;

using MessagePack.Formatters;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

using System.Data;
using System.Data.Common;

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
        internal Mapper GetMapper(List<SelectListItem> objDestinationList1, List<SelectListItem> objSourceList, bool listOnlyUsedColumns)
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
                    mapTable.sourceColumn = "<ignore>";
                    mapTable.targetColumn = objColumn.Text;
                }
                mapTable.SourceColumns = BuildIndividualSelectList(objMapList, objSourceList, "source", mapTable.sourceColumn, listOnlyUsedColumns);

                mapTable.TargetColumns = BuildIndividualSelectList(objMapList, objDestinationList1, "destination", mapTable.targetColumn, listOnlyUsedColumns);

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
                return $"Data Saved Successfull.  {sql}";
            }
            catch (Exception ex)
            {
                return "Database Insertion failed. Please See the exception Message" + ex.Message.ToString();
            }
        }

        internal bool IsSrcDesExists(string sourceTableName, string destinationTableName)
        {
            if (sourceTableName == null || destinationTableName == null)
            {
                return false;
            }
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
            if (sourceTableName == null)
            {
                throw new ArgumentNullException(nameof(sourceTableName));
            }
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
                    return $"Data Saved Successfull.  {sql}";
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
            bool preventDuplicate = false;
            foreach (var item in obj.mapTables)
            {
                if (item.targetColumn != null && item.sourceColumn != null && item.targetColumn != "<ignore>" && item.sourceColumn != "<ignore>")
                {
                    selectSQl = selectSQl + item.targetColumn + ",";

                    if (item.PreventDuplicates == false)
                    {
                        valuesSql = $"{valuesSql} a.{item.sourceColumn},";
                    }
                    else
                    {
                        preventDuplicate = true;
                        valuesSql = $"{valuesSql}CASE WHEN a.rc>1 THEN CONCAT({item.sourceColumn}, ' - ',a.rc) WHEN a.rc=1 THEN a.{item.sourceColumn} END AS {item.sourceColumn},";
                    }
                }
            }

            selectSQl = selectSQl.Remove(selectSQl.Length - 1, 1);
            selectSQl = selectSQl + ") ";

            valuesSql = valuesSql.Remove(valuesSql.Length - 1, 1);
            string subQuerySql = " FROM ( SELECT "; string partitionByCode = "";
            if (preventDuplicate == false)
            {
                valuesSql = valuesSql + " FROM " + obj.sourceTableName + " a";
            }
            else
            {
                foreach (var item in obj.mapTables)
                {
                    if (item.targetColumn != null && item.sourceColumn != null && item.targetColumn != "<ignore>" && item.sourceColumn != "<ignore>")
                    {
                        subQuerySql = $"{subQuerySql} {item.sourceColumn},";
                        if (item.PreventDuplicates)
                        {
                            partitionByCode = $"ROW_NUMBER() OVER ( PARTITION BY {item.sourceColumn} ORDER BY {item.sourceColumn} DESC) AS rc FROM {obj.sourceTableName}) a";
                        }
                    }
                }
                subQuerySql = $"{subQuerySql} {partitionByCode}";
                valuesSql = $"{valuesSql} {subQuerySql}";
            }
            finalSQL = selectSQl + valuesSql;
            return finalSQL;
        }

        public (List<MapTable>, bool PurgeBeforeInsert) GetConfiguarationData(string srcTableName, string desTableName)
        {
            List<MapTable> list = new List<MapTable>();
            try
            {
                string existingSql = "";
                string connString = this.Configuration.GetConnectionString("DefaultConnection");

                SqlConnection con = new SqlConnection(connString);
                SqlDataReader? sqlDataReader = null;
                string sql = $"SELECT SQL, PurgeBeforeInsert FROM MapperConfiguration WHERE SourceTable = '{srcTableName}' AND DestinationTable = '{desTableName}' AND IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    //cmd.Parameters.AddWithValue("@SourceTable", srcTableName);
                    //cmd.Parameters.AddWithValue("@DestinationTable", desTableName);

                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    sqlDataReader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                }
                var dataTable = new DataTable();
                if (sqlDataReader != null)
                    dataTable.Load(sqlDataReader);
                DataRow row = dataTable.Rows[0];
                existingSql = row.Field<string>("SQL") ?? "";
                list = GetMapTableInfo(existingSql);
                bool purgeBeforeInsert = row.Field<bool>("PurgeBeforeInsert");
                con.Close();
                return (list, purgeBeforeInsert);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ;
            }
        }

        private List<MapTable> GetMapTableInfo(string existingSql)
        {
            bool containsPartitionBy = false;
            if (existingSql.Contains("PARTITION BY"))
            {
                containsPartitionBy = true;
            }
            //INSERT INTO MasterCalendar (Date,Year,HolidayType)  SELECT Date, Year, HolidayType FROM MasterCalendar
            List<MapTable> mapTableList = new List<MapTable>();
            string[] data = existingSql.Split(new[] { "SELECT" }, StringSplitOptions.None);


            string[] desTemp = data[0].Split(new[] { "(" }, StringSplitOptions.None);
            desTemp[1] = desTemp[1].Replace("&nbsp;", " ").Replace(")", "").Replace("\r", "").Replace("\n", "");
            desTemp = desTemp[1].Split(new[] { "," }, StringSplitOptions.None);
            int count = 0;
            string[] srcTemp = data[1].Split(new[] { "FROM" }, StringSplitOptions.None);
            if (containsPartitionBy)
            {
                var fields = "";
                string[] temporary = data[1].Split(new[] { "CASE WHEN" }, StringSplitOptions.TrimEntries);
                fields = temporary[0];
                count = fields.Count(c => c == ',');
                string[] temporary2 = temporary[1].Split(new[] { "END AS" }, StringSplitOptions.TrimEntries);
                fields = $"{fields}{temporary2[1]}";
                fields = fields.Replace("FROM (", "").Replace("a.", "");
                srcTemp = fields.Split(new[] { "," }, StringSplitOptions.TrimEntries);
            }
            else
            {
                srcTemp = srcTemp[0].Split(new[] { "," }, StringSplitOptions.None);
            }

            for (int i = 0; i < srcTemp.Length; i++)
            {
                MapTable obj = new MapTable();
                obj.sourceColumn = srcTemp[i].Trim();
                obj.targetColumn = desTemp[i].Trim();
                if ((count) == i)
                {
                    obj.PreventDuplicates = true;
                }
                else
                {
                    obj.PreventDuplicates = false;
                }
                mapTableList.Add(obj);
            }

            return mapTableList;

        }

        internal Mapper GetMapper(List<SelectListItem> objDestinationList, List<SelectListItem> objSourceList, List<MapTable> objMapTable, bool listOnlyUsedColumns)
        {
            List<MapTable> objMapList = new List<MapTable>();
            foreach (var objColumn in objMapTable)
            {
                MapTable mapTable = new MapTable();
                mapTable.sourceColumn = objColumn.sourceColumn;
                mapTable.targetColumn = objColumn.targetColumn;
                mapTable.PreventDuplicates = objColumn.PreventDuplicates;
                objMapList.Add(mapTable);
            }
            int count = objMapList.Count;

            List<SelectListItem> result = objDestinationList
                .ExceptBy(objMapList.Select(msg => msg.targetColumn), msg => msg.Text)
                .ToList();

            if (objDestinationList.Count > count)
            {
                for (int i = 0; i < result.Count; i++)
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
                        mapTable.sourceColumn = "<ignore>";
                        mapTable.targetColumn = result[i].Text;
                    }

                    //mapTable.sourceColumn = "";
                    //mapTable.targetColumn = "";
                    objMapList.Add(mapTable);
                }
            }
            foreach (MapTable mapTable in objMapList)
            {
                mapTable.SourceColumns = BuildIndividualSelectList(objMapList, objSourceList, "source", mapTable.sourceColumn, listOnlyUsedColumns);

                mapTable.TargetColumns = BuildIndividualSelectList(objMapList, objDestinationList, "destination", mapTable.targetColumn, listOnlyUsedColumns);

            }


            //objMapList.RemoveAt(objMapList.Count - 1);
            Mapper mapper = new Mapper();

            //These should no longer be used 
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList, "Text", "Value");

            mapper.mapTables = objMapList;

            return mapper;
        }

        private SelectList? BuildIndividualSelectList(List<MapTable> objMapList, List<SelectListItem> originalSelectListItems, string fieldType, string? currentField, bool listOnlyUsedColumns)
        {
            SelectList? selectList = null;
            List<SelectListItem>? selectListItems = new List<SelectListItem>();
            //Make ignore at the top of the list
            SelectListItem selectListItemIgnore = new SelectListItem() { Text = "<ignore>", Value = "<ignore>" };
            selectListItems.Add(selectListItemIgnore);

            if ((currentField?.Trim() != "<ignore>" || listOnlyUsedColumns == false)) //Add list item for the current field
            {
                SelectListItem? selectListItem = originalSelectListItems.FirstOrDefault(f => f.Text == currentField);
                if (selectListItem?.Text != null) { selectListItems.Add(selectListItem); }
            }
            foreach (var item in originalSelectListItems)
            {
                MapTable? mapTable = null;
                if (fieldType == "source")
                {
                    mapTable = objMapList.FirstOrDefault(f => f.sourceColumn == item.Text);
                }
                else if (fieldType == "destination")
                {
                    mapTable = objMapList.FirstOrDefault(f => f.targetColumn == item.Text);
                }
                if ((mapTable == null && item.Text.Length > 0 && item.Text != "<ignore>") || (listOnlyUsedColumns == false && item.Text != "<ignore>")) //Only include if not already selected in the mapping
                {
                    item.Value = item.Text;
                    selectListItems.Add(item);
                }
            }
            selectList = new SelectList(selectListItems, "Text", "Value");
            return selectList;
        }
        public bool HasDuplicates<T>(List<T> listToTest)
        {
            var hashSet = new HashSet<T>();

            for (var i = 0; i < listToTest.Count; ++i)
            {
                if (!hashSet.Add(listToTest[i])) return true;
            }
            return false;
        }

        public string CheckForDuplicates(List<MapTable> mapTables)
        {
            string result = "";
            List<string> sourceFields = new List<string>();
            foreach (var mapTable in mapTables)
            {
                string sourceField = mapTable.sourceColumn ?? "";
                if (sourceField != "<ignore>")
                {
                    sourceFields.Add(sourceField);
                }
            }
            if (HasDuplicates(sourceFields))
            {
                result = $"{result} Duplicates found in the source columns!".Trim();
            }
            sourceFields = new List<string>();
            foreach (var mapTable in mapTables)
            {
                string destinationColumn = mapTable.targetColumn ?? "";
                if (destinationColumn != "<ignore>")
                {
                    sourceFields.Add(destinationColumn);
                }
            }
            if (HasDuplicates(sourceFields))
            {
                result = $"{result} Duplicates found in the destination columns!".Trim();
            }
            if (result != "")
            {
                result = $"{result} Saving has been aborted!".Trim();

            }
            return result;
        }
    }
}
