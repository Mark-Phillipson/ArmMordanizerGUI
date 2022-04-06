using ArmMordanizerGUI.Data;
using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ArmMordanizerGUI.Controllers
{
    public class MappingController : Controller
    {
        private ApplicationDbContext _db;
        //rivate ConnectionDb _connectionDB;


        public MappingController(ApplicationDbContext db)
        {
            _db = db;
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
            string cName = "Data Source=192.168.250.57; Initial Catalog=AmmsOnlineCountry;User ID=sa;Password=Online@Ammsdb";
            SqlConnection con = new SqlConnection(cName);

            DataTable srcObjTable = new DataTable();
            DataTable desObjTable = new DataTable();


            string srcQuery = @"SELECT name,system_type_name FROM sys.dm_exec_describe_first_result_set( N'SELECT * FROM TEMP_RAW_Transaction_1', NULL, 0 )";
            using (SqlCommand cmd = new SqlCommand(srcQuery, con))
            {
                //cmd.Parameters.AddWithValue("@TableName", Tablename);

                con.Open();
                srcObjTable.Load(cmd.ExecuteReader());
                con.Close();
            }
            List<TableColumns> objSourceColumns = new List<TableColumns>();
            foreach (DataRow row in srcObjTable.Rows)
            {
                objSourceColumns.Add(new TableColumns { ColumnName = row["name"].ToString(), ColumnType = row["system_type_name"].ToString() });

            }
            var objSourceList = objSourceColumns.Select(x => new SelectListItem()
            {
                Text = x.ColumnName,
                Value = x.ColumnName
            }
            ).ToList();
            objSourceList.Insert(0, new SelectListItem() { Value = "0", Text = "-- Please select your Column --" });


            List<TableColumns> objDestinationColumns = new List<TableColumns>();
            string desQuery = @"SELECT name,system_type_name FROM sys.dm_exec_describe_first_result_set( N'SELECT * FROM TEMP_RAW_Transaction_2', NULL, 0 )";
            using (SqlCommand cmd = new SqlCommand(desQuery, con))
            {
                //cmd.Parameters.AddWithValue("@TableName", Tablename);

                con.Open();
                desObjTable.Load(cmd.ExecuteReader());
                con.Close();
            }
            foreach (DataRow row in desObjTable.Rows)
            {
                objDestinationColumns.Add(new TableColumns { ColumnName = row["name"].ToString(), ColumnType = row["system_type_name"].ToString() });

            }
            //objDestinationColumns.Add(new TableColumns { ColumnName = "A", ColumnType = "nvarchar" });
            //objDestinationColumns.Add(new TableColumns { ColumnName = "B", ColumnType = "nvarchar" });
            //objDestinationColumns.Add(new TableColumns { ColumnName = "C", ColumnType = "nvarchar" });
            //objDestinationColumns.Add(new TableColumns { ColumnName = "D", ColumnType = "nvarchar" });

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
