using EloDoacoes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EloDoacoes.Data;
using EloDoacoes.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EloDoacoes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EloDoacoesContext _context;

        public HomeController(ILogger<HomeController> logger, EloDoacoesContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: Home/Index - display available donations feed with server-side pagination
        // Returns only donations with "Available" status, excluding current user's donations if authenticated
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 9; // Fixed page size: 9 donations per page (3x3 grid)

            // Ensure valid page number
            if (page < 1) page = 1;

            // Extract current user ID from claims (if authenticated)
            int? currentUserId = null;
            if (User?.Identity?.IsAuthenticated == true)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out var userId))
                {
                    currentUserId = userId;
                }
            }

            // Build LINQ query: exhibit Available or Reserved donations (get out once Completed/Cancelled or Confirmed reservation)
            var query = _context.Donations
                .AsNoTracking()
                .Include(d => d.DonationStatus)
                .Include(d => d.DonationImages)
                .Include(d => d.User)
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.ReservationStatus)
                .Where(d => (d.DonationStatus.Name == DonationStatusNameEnum.Available || d.DonationStatus.Name == DonationStatusNameEnum.Reserved)
                         && !d.Reservations.Any(r => r.ReservationStatus != null && r.ReservationStatus.Name == ReservationStatusNameEnum.Confirmed));

            // Filter: If user is authenticated, exclude their own donations
            if (currentUserId.HasValue)
            {
                query = query.Where(d => d.User != null && d.User.UserID != currentUserId.Value);
            }

            // Order by most recent first
            query = query.OrderByDescending(d => d.RegistrationDate);

            // Get total count BEFORE pagination
            var totalCount = await query.CountAsync();

            // Apply server-side pagination: Skip and Take
            var donations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map donations to view model cards
            var items = new List<DonationCardViewModel>();
            foreach (var d in donations)
            {
                var mainImage = d.DonationImages?.OrderBy(di => di.DisplayOrder).FirstOrDefault();
                string base64 = null;
                if (mainImage?.ImageData != null)
                {
                    base64 = Convert.ToBase64String(mainImage.ImageData);
                }

                int pendingCount = d.Reservations?.Count(r => r.ReservationStatus != null && r.ReservationStatus.Name == ReservationStatusNameEnum.Pending) ?? 0;
                string badgeText = pendingCount > 0 ? $"{pendingCount} interessado(s)" : string.Empty;

                items.Add(new DonationCardViewModel
                {
                    DonationId = d.DonationID,
                    Title = d.Title,
                    ShortDescription = d.Description?.Length > 120 
                        ? d.Description.Substring(0, 117) + "..." 
                        : d.Description,
                    ImageBase64 = base64,
                    DonationStatus = d.DonationStatus?.Name.ToString() ?? "Available",
                    IsOwner = (currentUserId.HasValue && d.User != null && d.User.UserID == currentUserId.Value),
                    ReservationsCount = pendingCount,
                    ReservationStatusBadge = badgeText
                });
            }

            // Create and return paged result ViewModel
            var model = new HomeIndexViewModel
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
