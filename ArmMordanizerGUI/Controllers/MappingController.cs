using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using ArmMordanizerGUI.Service;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System.Data;

namespace ArmMordanizerGUI.Controllers
{
    public class MappingController : Controller
    {
        private ApplicationDbContext _db;
        private SourceData _sourceData;
        private DestinationData _destinationData;
        private MapperData _mapperData;

        private IConfiguration Configuration;
        private readonly DatabaseMetaDataService _databaseMetaDataService;

        public MappingController(ApplicationDbContext db, IConfiguration _configuration, DatabaseMetaDataService databaseMetaDataService)
        {
            _db = db;
            Configuration = _configuration;
            _databaseMetaDataService = databaseMetaDataService;
            _mapperData = new MapperData(_configuration);
            _sourceData = new SourceData(_configuration);
            _destinationData = new DestinationData(_configuration);
        }
        public IActionResult MapTable()
        {
            Mapper mapper = new Mapper();
            mapper.sourcetableSelectList = new SelectList(_sourceData.GetSourceTableInfo(), "Text", "Value");
            mapper.desTinationTableSelectList = new SelectList(_destinationData.GetDestinationTableInfo(), "Text", "Value");


            return View(mapper);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MapTable(Mapper obj)
        {
            obj.sourceTableName = obj.HiddenSourceTableName;
            string? msg = null;
            msg = _mapperData.CheckForDuplicates(obj.mapTables);
            if (msg != null && msg.Length > 0)
            {
                TempData["message"] = msg;
                return RedirectToAction("MapTable", "Mapping");
            }
            else
            {
                bool isExists = _mapperData.IsSrcDesExists(obj.sourceTableName, obj.destinationTableName);
                if (isExists)
                {
                    msg = _mapperData.UpdateMappingData(obj);
                }
                else
                {
                    msg = _mapperData.Save(obj);
                }

                _mapperData.MoveFileToReUpload(obj.sourceTableName);
                obj.destinationTableName = "dbo.Stock";

                TempData["message"] = msg;

                return RedirectToAction("MapTable", "Mapping");
            }
        }
        [HttpPost]
        public IActionResult ValidateColumns(string sourceTablename, string destinationTablename, string sourceField, string destinationField, string id)
        {
            var result = _databaseMetaDataService.ValidateMapping(sourceTablename, destinationTablename, sourceField, destinationField);
            return Content(result);
        }
        public IActionResult DestinationTableddlChange(Mapper obj)
        {
            List<SelectListItem> objDestinationList = _destinationData.GetDestinationData(obj.destinationTableName);
            obj.desTinationSelectList = new SelectList(objDestinationList, "Text", "Value");
            return View(obj);

        }
        public IActionResult SourceTableddlChange(Mapper obj)
        {
            List<SelectListItem> objSourceList = _sourceData.GetSourceData(obj.sourceTableName);
            obj.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            return View(obj);

        }
        [HttpPost]
        public IActionResult ShowSampleData(string tableName)
        {
            var sql = $"SELECT * FROM {tableName}";
            IEnumerable<ARMDatabaseColumn>? columns = _databaseMetaDataService.GetColumnNamesFromSql(sql, tableName);
            DataTable dataTable = _databaseMetaDataService.GetDataIntoDataTable(sql);
            var recordCount = _databaseMetaDataService.GetRecordCount(tableName);
            ViewBag.RecordCount = recordCount;
            if (columns != null)
            {
                ViewBag.Columns = columns.ToList();
            }
            ViewBag.TableName = tableName;
            //return Content("<h1> Testing</h1>");
            var result = PartialView("~/Views/Shared/_SampleData.cshtml", dataTable);
            return result;

        }

        public IActionResult MapPartial(string SrcTableName, string desTableName, bool listOnlyUsedColumns)
        {
            Mapper mapper = new Mapper();
            List<SelectListItem> objDestinationList = _destinationData.GetDestinationData(desTableName);
            List<SelectListItem> objSourceList = _sourceData.GetSourceData(SrcTableName);

            string? msg = null;
            bool isExists = _mapperData.IsSrcDesExists(SrcTableName, desTableName);


            if (!isExists)
            {
                TempData["message"] = null;
                mapper = _mapperData.GetMapper(objDestinationList, objSourceList, listOnlyUsedColumns);
                mapper.sourceTableName = SrcTableName;
                mapper.destinationTableName = desTableName;
            }
            else
            {
                List<MapTable> objMapTable = _mapperData.GetConfiguarationData(SrcTableName, desTableName);
                mapper = _mapperData.GetMapper(objDestinationList, objSourceList, objMapTable, listOnlyUsedColumns);
                mapper.sourceTableName = SrcTableName;
                mapper.destinationTableName = desTableName;
            }
            var mapTables = mapper.mapTables;
            foreach (MapTable item in mapTables)
            {
                if (item.sourceColumn != null && item.targetColumn != null)
                {
                    item.Status = _databaseMetaDataService.ValidateMapping(SrcTableName, desTableName, item.sourceColumn, item.targetColumn);
                }
            }
            mapper.mapTables = mapTables;


            return PartialView("~/Views/Shared/_MapPartial.cshtml", mapper);
        }
        public IActionResult Reset()
        {
            TempData["message"] = null;
            return RedirectToAction("MapTable", "Mapping");

        }
        [HttpGet]
        public FileStreamResult? CreateSqlFile(string sourceTable,string destinationTable)
        {
            string name = $"InsertIntoStatement.sql";
            string sql = "Testing";


            FileInfo info = new FileInfo(name);

            using (StreamWriter writer = info.CreateText())
            {
                writer.WriteLine(sql);
            }
            return File(info.OpenRead(), "text/plain");

        }

    }
}