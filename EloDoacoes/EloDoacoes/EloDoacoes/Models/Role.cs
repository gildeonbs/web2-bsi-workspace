using Microsoft.EntityFrameworkCore;

namespace EloDoacoes.Models
{
    public enum RoleEnum
    {
        Admin,
        StandardUser
    }

    [Index(nameof(Name), IsUnique = true)]
    public class Role
    {
        public int RoleID { get; set; }
        public RoleEnum Name { get; set; }
    }
}
