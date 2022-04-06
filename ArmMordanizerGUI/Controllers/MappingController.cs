using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArmMordanizerGUI.Controllers
{
    public class MappingController : Controller
    {
        private ApplicationDbContext _db;
        public MappingController(ApplicationDbContext db)
        {
            _db = db;
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
            List<TableColumns> objSourceColumns = new List<TableColumns>();
            objSourceColumns.Add(new TableColumns { ColumnName = "A", ColumnType = "nvarchar" });
            objSourceColumns.Add(new TableColumns { ColumnName = "B", ColumnType = "nvarchar" });
            objSourceColumns.Add(new TableColumns { ColumnName = "C", ColumnType = "nvarchar" });
            objSourceColumns.Add(new TableColumns { ColumnName = "D", ColumnType = "nvarchar" });


            var objSourceList = objSourceColumns.Select(x => new SelectListItem()
            {
                Text= x.ColumnName,
                Value = x.ColumnName
            }
            ).ToList();
            objSourceList.Insert(0, new SelectListItem() { Value = "0", Text = "-- Please select your Column --" });


            List<TableColumns> objDestinationColumns = new List<TableColumns>();
            objDestinationColumns.Add(new TableColumns { ColumnName = "A", ColumnType = "nvarchar" });
            objDestinationColumns.Add(new TableColumns { ColumnName = "B", ColumnType = "nvarchar" });
            objDestinationColumns.Add(new TableColumns { ColumnName = "C", ColumnType = "nvarchar" });
            objDestinationColumns.Add(new TableColumns { ColumnName = "D", ColumnType = "nvarchar" });

            var objDestinationList = objDestinationColumns.Select(x => new SelectListItem()
            {
                Text = x.ColumnName,
                Value = x.ColumnName
            }
            ).ToList();
            objDestinationList.Insert(0, new SelectListItem() { Value = "0", Text = "-- Please select your Column --" });

            List<MapTable> objMapList = new List<MapTable>();
            foreach (var objColumn in objDestinationColumns)
            {
                MapTable mapTable = new MapTable(); 
                mapTable.sourceColumn = "";
                mapTable.targetColumn = "";
                objMapList.Add(mapTable);  
            }

            Mapper mapper = new Mapper();
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList, "Text", "Value");
            mapper.mapTables = objMapList;


            return View(mapper);
        }
    }
}
