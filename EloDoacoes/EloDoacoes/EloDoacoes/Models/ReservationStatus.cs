using Microsoft.EntityFrameworkCore;

namespace EloDoacoes.Models
{
    public enum ReservationStatusNameEnum
    {
        Pending,
        Confirmed,
        Cancelled
    }

    [Index(nameof(Name), IsUnique = true)]
    public class ReservationStatus
    {
        public int ReservationStatusID { get; set; }
        public ReservationStatusNameEnum Name { get; set; } = ReservationStatusNameEnum.Pending;
    }
}
