using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using ArmMordanizerGUI.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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


        public MappingController(ApplicationDbContext db, IConfiguration _configuration)
        {
            _db = db;
            Configuration = _configuration;
            _mapperData = new MapperData(_configuration);
            _sourceData = new SourceData(_configuration);
            _destinationData = new DestinationData(_configuration);
        }
        


        public IActionResult MapTable()
        {
            Mapper mapper = new Mapper();
            mapper.sourcetableSelectList = new SelectList(_sourceData.GetSourceTableInfo(), "Text", "Value");
            mapper.desTinationTableSelectList = new SelectList(_destinationData.GetDestinationTableInfo(), "Text", "Value");

            List<SelectListItem> reUseData = new List<SelectListItem>();
            reUseData.Add(new SelectListItem("Please Select Option", "0"));
            reUseData.Add(new SelectListItem("Yes", "1"));
            reUseData.Add(new SelectListItem("No", "2"));



            mapper.reUseConfiguration = new SelectList(reUseData, "Text", "Value"); ;

            return View(mapper);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MapTable(Mapper obj)
        {
            string msg = null;
            if (obj.reUseValue == 1)
            {
                bool isExists = _mapperData.IsSrcDesExists(obj.sourceTableName, obj.destinationTableName);
                if (!isExists)
                {
                    TempData["message"] = "Configuration Data Not Found.";
                    return RedirectToAction("MapTable", "Mapping");
                }
                else
                {
                    foreach (var item in obj.mapTables)
                    {
                        if (item.sourceColumn != null && item.targetColumn != null)
                            continue;
                        else if (item.sourceColumn == null && item.targetColumn == null)
                            continue ;
                        else
                            msg = "Source and Destination Column Mapping Missing";
                        if (item.SourceColumnType != item.TargetColumnType)
                            msg = "Source and Destination Column Type Does Not Match";
                    }
                    msg = _mapperData.UpdateMappingData(obj);
                }
            }
            else
            {
                msg = _mapperData.Save(obj);
                _mapperData.MoveFileToReUpload(obj.sourceTableName);

            }
            TempData["message"] = msg;
            return RedirectToAction("MapTable", "Mapping");
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
        public IActionResult MapPartial(string SrcTableName, string desTableName, string reUseValue)
        {
            Mapper mapper = new Mapper();
            List<SelectListItem> objDestinationList = _destinationData.GetDestinationData(desTableName);
            List<SelectListItem> objSourceList = _sourceData.GetSourceData(SrcTableName);

            string msg = null;
            if (reUseValue == "1")
            {
                bool isExists = _mapperData.IsSrcDesExists(SrcTableName, desTableName);
                if (!isExists)
                {
                    TempData["message"] = null;
                    List<MapTable> objMapList = new List<MapTable>();
                    mapper.mapTables = objMapList;
                    return PartialView("~/Views/Shared/_MapPartial.cshtml", mapper);
                }
                else
                {
                    List<MapTable> objMapTable = _mapperData.GetConfiguarationData(SrcTableName, desTableName);
                    mapper = _mapperData.GetMapper(objDestinationList, objSourceList, objMapTable);
                    mapper.sourceTableName = SrcTableName;
                    mapper.destinationTableName = desTableName;
                }
            }
            else if (reUseValue == "2")
            {
                mapper = _mapperData.GetMapper(objDestinationList, objSourceList);
                mapper.sourceTableName = SrcTableName;
                mapper.destinationTableName = desTableName;
            }



            return PartialView("~/Views/Shared/_MapPartial.cshtml", mapper);
        }

    }
}
