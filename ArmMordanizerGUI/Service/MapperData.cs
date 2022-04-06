using ArmMordanizerGUI.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArmMordanizerGUI.Service
{
    public class MapperData
    {
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

            Mapper mapper = new Mapper();
            mapper.sourceSelectList = new SelectList(objSourceList, "Text", "Value");
            mapper.desTinationSelectList = new SelectList(objDestinationList1, "Text", "Value");
            mapper.mapTables = objMapList;
            return mapper;
        }
    }
}
