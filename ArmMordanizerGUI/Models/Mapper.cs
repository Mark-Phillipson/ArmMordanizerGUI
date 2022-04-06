using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArmMordanizerGUI.Models
{
    public class Mapper
    {
        public SelectList sourceSelectList { get; set; }
        public SelectList desTinationSelectList { get; set; }
        public List<MapTable> mapTables { get; set; }

    }
}
