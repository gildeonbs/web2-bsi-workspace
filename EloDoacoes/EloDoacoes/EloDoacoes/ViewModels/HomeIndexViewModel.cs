namespace EloDoacoes.ViewModels
{
    /// <summary>
    /// ViewModel for the Home Index view displaying paginated available donations feed.
    /// </summary>
    public class HomeIndexViewModel : PagedResult<DonationCardViewModel>
    {
    }
}
