using Microsoft.EntityFrameworkCore;

namespace EloDoacoes.Models
{

    public enum DonationStatusEnum
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
        public DonationStatusEnum Name { get; set; } = DonationStatusEnum.Available;
    }
}
