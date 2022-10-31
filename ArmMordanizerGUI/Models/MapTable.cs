using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArmMordanizerGUI.Models
{
    public class MapTable
    {
        public string? sourceColumn { get; set; }
        public SelectList? SourceColumns { get; set; }
        public string? targetColumn { get; set; }
        public SelectList? TargetColumns { get; set; }
        public string? SourceColumnType { get; set; }
        public string? TargetColumnType { get; set; }
        public string? Status { get; set; }
        public bool PreventDuplicates { get; set; }

    }
}
