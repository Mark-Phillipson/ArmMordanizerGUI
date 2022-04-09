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

        //rivate ConnectionDb _connectionDB;
        private IConfiguration Configuration;


        public MappingController(ApplicationDbContext db, IConfiguration _configuration)
        {
            _db = db;
            Configuration = _configuration;
            _mapperData = new MapperData(_configuration);
            _sourceData = new SourceData(_configuration);
            _destinationData = new DestinationData(_configuration);
            //_connectionDB = dbC;
        }
        public IActionResult Index()
        {
            IEnumerable<Mapping> objCategoryList = _db.Mappings.ToList();
            return View(objCategoryList);
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mapping obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order cannot exactly match Name. ");
            }
            if (ModelState.IsValid)
            {
                _db.Mappings.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }


        public IActionResult MapTable()
        {
            Mapper mapper = new Mapper();
            mapper.sourcetableSelectList = new SelectList(_sourceData.GetSourceTableInfo(), "Text", "Value");
            mapper.desTinationTableSelectList = new SelectList(_destinationData.GetSourceTableInfo(), "Text", "Value");

            return View(mapper);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MapTable(Mapper obj)
        {
            //string msg = "";
            string msg = _mapperData.Save(obj);
            //    if (obj.mapTables.Count == obj.DisplayOrder.ToString())
            //    {
            //        ModelState.AddModelError("Name", "The Display Order cannot exactly match Name. ");
            //    }
            //    if (ModelState.IsValid)
            //    {
            //        _db.Mappings.Add(obj);
            //        _db.SaveChanges();
            //        return RedirectToAction("Index");
            //    }
            TempData["message"] = msg;
            return RedirectToAction("MapTable","Mapping");
            //return View(obj);
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
        public IActionResult MapPartial(string SrcTableName, string desTableName)
        {
            

            List<SelectListItem> objDestinationList = _destinationData.GetDestinationData(SrcTableName);
            List<SelectListItem> objSourceList = _sourceData.GetSourceData(desTableName);
            Mapper mapper = _mapperData.GetMapper(objDestinationList, objSourceList);
            mapper.sourceTableName = SrcTableName;
            mapper.destinationTableName = desTableName; 


            return PartialView("~/Views/Shared/_MapPartial.cshtml", mapper);
        }

    }
}
