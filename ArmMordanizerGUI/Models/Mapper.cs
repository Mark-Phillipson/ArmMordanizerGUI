using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArmMordanizerGUI.Models
{
    public class Mapper
    {
        public SelectList sourceSelectList { get; set; }
        public SelectList desTinationSelectList { get; set; }
        public List<MapTable> mapTables { get; set; }
        public SelectList sourcetableSelectList { get; set; }
        public SelectList desTinationTableSelectList { get; set; }
        public string sourceTableName { get; set; }
        public string destinationTableName { get; set; }

    }
}
