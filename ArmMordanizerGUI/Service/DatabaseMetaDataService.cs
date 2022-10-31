using ArmMordanizerGUI.Models;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArmMordanizerGUI.Service
{
    public class DatabaseMetaDataService
    {
        private readonly IConfiguration _configuration;
        private string _connectionString;

        public DatabaseMetaDataService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IEnumerable<ARMDatabaseColumn>? GetColumnNamesFromSql(string sql, string tablename)
        {

            var columns = new List<ARMDatabaseColumn>();
            DataTable dataTable;
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                sqlCon.Open();
                var sqlCmd = sqlCon.CreateCommand();

                sqlCmd.CommandText = $"SET FMTONLY ON; {sql}; SET FMTONLY OFF"; // No data wanted, only schema  
                sqlCmd.CommandType = CommandType.Text;
                var reader = sqlCmd.ExecuteReader(CommandBehavior.SingleRow);
                dataTable = reader.GetSchemaTable();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                ARMDatabaseColumn column = new();
                column.ColumnName = row.Field<string>("ColumnName") ?? "ColumnNameNotFound";
                var columnSize = row.Field<int>("ColumnSize");
                column.ColumnSize = columnSize;
                column.DataType = row.Field<string>("DataTypeName");
                column.DataTypeFriendly = GetGeneralTypeAsString(column);
                column.Required = !row.Field<bool>("AllowDBNull");
                column.IsAutoIncrement = row.Field<bool>("IsAutoIncrement");
                column.IsIdentity = row.Field<bool>("IsIdentity");
                column.IsKey = row.Field<bool?>("IsKey") ?? false;
                column.Label = StringHelperService.AddSpacesToSentence(column.ColumnName ?? "");
                column.PropertyName = StringHelperService.RemoveUnsupportedCharacters(column.ColumnName ?? "").Replace("ID", "Id");
                string? largestValue = GetLargestValue(column, tablename);
                column.LargestValue = $"{largestValue} ({largestValue?.Length})";
                column.HasNulls = HasNulls(column, tablename);
                columns.Add(column);
            }
            return columns.ToList();
        }

        private string? GetLargestValue(ARMDatabaseColumn column, string tablename)
        {
            string sql = "";
            if (column.DataType == "nvarchar")
            {
                sql = $" SELECT TOP 1 {tablename}.[{column.ColumnName}] FROM {tablename} ORDER BY LEN({tablename}.[{column.ColumnName}]) DESC";
            }
            else if (column.DataType == "int" || column.DataType == "decimal" || column.DataType == "bigint")
            {
                sql = $" SELECT TOP 1 {tablename}.[{column.ColumnName}] FROM {tablename} ORDER BY {tablename}.[{column.ColumnName}] DESC";
            }
            else
            {
                return "";
            }
            DataTable? dataTable = new DataTable();
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                sqlCon.Open();
                var sqlCmd = sqlCon.CreateCommand();

                sqlCmd.CommandText = $"{sql};"; // No data wanted, only schema  
                sqlCmd.CommandType = CommandType.Text;
                SqlDataReader sqlDataReader;
                try
                {
                    sqlDataReader = sqlCmd.ExecuteReader(CommandBehavior.Default);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    throw;
                }
                if (sqlDataReader != null)
                    dataTable.Load(sqlDataReader);

            }
            if (dataTable.Rows.Count == 0)
            {
                return "";
            }
            DataRow dataRow = dataTable.Rows[0];
            string? result = dataRow[column.ColumnName].ToString();
            //string? result = dataRow[column.ColumnName].ToString();
            return result;
        }
        private bool HasNulls(ARMDatabaseColumn column, string tablename)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                sqlCon.Open();
                var sqlCmd = sqlCon.CreateCommand();
                var sqlText = "";
                if (column.DataType?.ToLower() == "nvarchar")
                {
                    sqlText = $"select Count (*) from {tablename} WHERE {tablename}.{column.ColumnName} IS NULL OR {tablename}.{column.ColumnName}=''";
                }
                else
                {
                    sqlText = $"select Count (*) from {tablename} WHERE {tablename}.{column.ColumnName} IS NULL ";

                }
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = sqlText;
                int rows = (int)sqlCmd.ExecuteScalar();
                return rows > 0;
            }
        }
        public DataTable GetDataIntoDataTable(string sql, string? searchTerm = null, string db = "ARM_CORE", int maxRows = 10)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                sqlCon.Open();
                SqlCommand? sqlCommand = sqlCon.CreateCommand();
                if (!sql.ToLower().Contains(" top ") && Regex.Matches(sql.ToLower(), "select").Count == 1)
                {
                    sql = sql.ToLower().Replace("select", $"select top {maxRows} ");
                }
                string? sqlText = $"{sql}";
                sqlCommand.CommandText = sqlText;
                sqlCommand.CommandType = CommandType.Text;
                SqlDataReader? sqlDataReader = null;
                try
                {
                    sqlDataReader = sqlCommand.ExecuteReader(CommandBehavior.SingleResult);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    throw;
                }
                var dataTable = new DataTable();
                if (sqlDataReader != null)
                    dataTable.Load(sqlDataReader);
                return dataTable;
            }
        }
        private static string GetGeneralTypeAsString(ARMDatabaseColumn column)
        {
            string columnSize = column.ColumnSize.ToString();
            if (column.ColumnSize == 2147483647)
            {
                columnSize = "MAX";
            }

            switch (column.DataType)
            {
                case "bit": return "True/False";
                case "char": return "Character";
                case "tinyint": return "Number";
                case "smallint": return "Number";
                case "int": return "Number";
                case "bigint": return "Number";
                case "float": return "Number";
                case "double": return "Number";
                case "decimal": return "Decimal";
                case "datetime": return "Date & Time";
                case "date": return "Date";
                case "uniqueidentifier": return "Unique ID";
                case "nvarchar": return $"String ({columnSize})";
                default: return "String";
            }
        }

        public string ValidateMapping(string sourceTablename, string destinationTablename, string sourceField, string destinationField)
        {
            var sourceColumns = GetColumnNamesFromSql($"SELECT * FROM {sourceTablename}", sourceTablename);

            var destinationColumns = GetColumnNamesFromSql($"SELECT * FROM {destinationTablename}", destinationTablename);
            if (sourceColumns == null || destinationColumns == null)
            {
                return "<span class='bg-danger'>Error matching all fields!</span>";
            }
            var sourceColumn = sourceColumns.FirstOrDefault(f => f.ColumnName == sourceField);
            var destinationColumn = destinationColumns.FirstOrDefault(f => f.ColumnName == destinationField);
            if (destinationColumn == null)
            {
                return "<span class='bg-dark'>Ignored</span>";
            }
            string ignored = "";
            if (sourceColumn == null)
            {
                ignored = "<span class='bg-dark p-1 border border-danger'>Ignored</span>";
            }
            string identity = "";
            if (destinationColumn != null && destinationColumn.IsIdentity)
            {
                identity = "<span style='color:red'><strong> Identity* </strong></span>";
            }
            string required = "";
            if (destinationColumn != null && destinationColumn.Required)
            {
                required = "<span style='color:red'><strong>Required* </strong></span>";
            }
            if (sourceColumn?.DataType == destinationColumn?.DataType)
            {
                string? columnSize = sourceColumn?.ColumnSize.ToString();
                if (columnSize == "2147483647")
                {
                    columnSize = "MAX";
                }
                if (sourceColumn?.ColumnSize > destinationColumn?.ColumnSize)
                {
                    if (sourceColumn?.LargestValue?.Length > destinationColumn?.ColumnSize)
                    {
                        return $"{ignored} {required}<span class='bg-danger border border-warning p-1'>Data truncation will occur! ({columnSize} to {destinationColumn.ColumnSize})</span>";
                    }
                    else
                    {
                        return $"{ignored} {required}<span class='bg-info border border-warning p-1'>Data truncation possible! ({columnSize} to {destinationColumn?.ColumnSize})</span>";

                    }
                }
                else
                {
                    return $"{ignored} {required}<span class='bg-success border border-warning p-1'>OK</span>";
                }
            }
            else
            {
                if (sourceColumn != null)
                {
                    return $"{ignored} {required}<span class='bg-warning border border-danger p-1' style=''>Warning Data Types do not match! ({sourceColumn?.DataType} to {destinationColumn?.DataType})</span>";
                }
                else
                {
                    return $"{ignored} {required}";
                }
            }
        }
        public int GetRecordCount(string tableName)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                sqlCon.Open();
                var sqlCmd = sqlCon.CreateCommand();
                var sqlText = $"select Count (*) from {tableName}";
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = sqlText;
                int rows = (int)sqlCmd.ExecuteScalar();
                return rows;
            }
        }
    }
}
