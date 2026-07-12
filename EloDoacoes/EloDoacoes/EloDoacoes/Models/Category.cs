using Microsoft.EntityFrameworkCore;

namespace EloDoacoes.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
