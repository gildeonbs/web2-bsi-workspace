using Microsoft.EntityFrameworkCore;

namespace EloDoacoes.Models
{
    public enum DonationStatusNameEnum
    {
        Available,
        Reserved,
        Completed,
        Cancelled
    }

    [Index(nameof(Name), IsUnique = true)]
    public class DonationStatus
    {
        public int DonationStatusID { get; set; }
        public DonationStatusNameEnum Name { get; set; } = DonationStatusNameEnum.Available;
    }
}
