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
            _mapperData = new MapperData();
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
            List<SelectListItem> objSourceList = _sourceData.GetSourceData("TEMP_RAW_Transaction_1");
            List<SelectListItem> objDestinationList = _destinationData.GetDestinationData("TEMP_RAW_Transaction_2");

            Mapper mapper = _mapperData.GetMapper(objDestinationList, objSourceList);

            return View(mapper);
        }

    }
}
