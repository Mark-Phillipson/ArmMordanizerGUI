using ArmMordanizerGUI.Models;
using Microsoft.EntityFrameworkCore;

namespace ArmMordanizerGUI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Mapping> Mappings { get; set; }
    }
}
