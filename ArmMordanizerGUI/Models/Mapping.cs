using System.ComponentModel.DataAnnotations;

namespace ArmMordanizerGUI.Models
{
    public class Mapping
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
