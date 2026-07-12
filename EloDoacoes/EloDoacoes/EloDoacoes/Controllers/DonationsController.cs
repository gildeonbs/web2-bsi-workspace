using EloDoacoes.Data;
using EloDoacoes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace EloDoacoes.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly EloDoacoesContext _context;
        // Category selector and image upload/remove support added to Create and Edit actions. (patched)

        public DonationsController(EloDoacoesContext context)
        {
            _context = context;
        }

        // GET: Donations/MyDonations - Display user's donations with server-side pagination
        // Requires authentication. Shows ALL donations regardless of status.
        public async Task<IActionResult> MyDonations(int page = 1)
        {
            const int pageSize = 9; // Fixed page size: 9 donations per page (3x3 grid)

            // Ensure valid page number
            if (page < 1) page = 1;

            // Ensure user is authenticated (protected by [Authorize] class-level attribute)
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyDonations", "Donations") });
            }

            // Extract current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyDonations", "Donations") });
            }

            // Build LINQ query: only donations where DonorUserId == currentUserId (no status filter)
            var query = _context.Donations
                .AsNoTracking()
                .Include(d => d.DonationImages)
                .Include(d => d.DonationStatus)
                .Include(d => d.User)
                .Where(d => d.User.UserID == currentUserId)
                .OrderByDescending(d => d.RegistrationDate);

            // Get total count BEFORE pagination
            var totalCount = await query.CountAsync();

            // Apply server-side pagination: Skip and Take
            var donations = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map donations to view model cards
            var items = new List<ViewModels.DonationCardViewModel>();
            foreach (var d in donations)
            {
                var mainImage = d.DonationImages?.OrderBy(di => di.DisplayOrder).FirstOrDefault();
                string base64 = null;
                if (mainImage?.ImageData != null)
                {
                    base64 = Convert.ToBase64String(mainImage.ImageData);
                }

                items.Add(new ViewModels.DonationCardViewModel
                {
                    DonationId = d.DonationID,
                    Title = d.Title,
                    ShortDescription = d.Description?.Length > 120 
                        ? d.Description.Substring(0, 117) + "..." 
                        : d.Description,
                    ImageBase64 = base64,
                    DonationStatus = d.DonationStatus?.Name.ToString() ?? string.Empty,
                    IsOwner = true
                });
            }

            // Create and return paged result ViewModel
            var model = new ViewModels.MyDonationsViewModel
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };

            return View(model);
        }

        // GET: Donations/MyAdoptions
        public async Task<IActionResult> MyAdoptions()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyAdoptions", "Donations") });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var adoptions = await _context.Reservations
                .Include(r => r.ReservationStatus)
                .Include(r => r.Donation)
                    .ThenInclude(d => d.DonationImages)
                .Include(r => r.Donation)
                    .ThenInclude(d => d.Category)
                .Include(r => r.Donation)
                    .ThenInclude(d => d.DonationStatus)
                .Include(r => r.Donation)
                    .ThenInclude(d => d.User)
                .Where(r => r.User != null && r.User.UserID == currentUserId)
                .OrderByDescending(r => r.ReservationDate)
                .AsNoTracking()
                .ToListAsync();

            return View(adoptions);
        }

        // POST: Donations/CancelAdoption
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAdoption(int reservationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Donation)
                .FirstOrDefaultAsync(r => r.ReservationID == reservationId && r.User.UserID == currentUserId);

            if (reservation != null)
            {
                var cancelledStatus = await _context.ReservationsStatuses
                    .FirstOrDefaultAsync(rs => rs.Name == ReservationStatusNameEnum.Cancelled);

                if (cancelledStatus != null)
                {
                    reservation.ReservationStatus = cancelledStatus;
                    _context.Reservations.Update(reservation);

                    if (reservation.Donation != null)
                    {
                        var availableStatus = await _context.DonationStatuses
                            .FirstOrDefaultAsync(ds => ds.Name == DonationStatusNameEnum.Available);
                        if (availableStatus != null)
                        {
                            reservation.Donation.DonationStatus = availableStatus;
                            _context.Donations.Update(reservation.Donation);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Reserva de adoção cancelada com sucesso.";
                }
            }

            return RedirectToAction(nameof(MyAdoptions));
        }

        // POST: Donations/Reserve
        // Allow anonymous so we can redirect unauthenticated users to login preserving returnUrl
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Reserve(int donationId)
        {
            // Load donation and ensure it exists
            var donation = await _context.Donations
                .Include(d => d.DonationStatus)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DonationID == donationId);

            if (donation == null)
            {
                return NotFound();
            }

            // If user not authenticated, redirect to login preserving return URL to the donation details
            if (User?.Identity?.IsAuthenticated != true)
            {
                var returnUrl = Url.Action("Details", "Donations", new { id = donationId });
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            // Parse current user id from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                // If claim missing or invalid, force login
                var returnUrl = Url.Action("Index", "Home");
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            // Prevent user from reserving their own donation
            if (donation.User != null && donation.User.UserID == currentUserId)
            {
                TempData["ErrorMessage"] = "Você não pode reservar sua própria doação.";
                return RedirectToAction("Index", "Home");
            }

            // Prevent duplicate reservations from the same user on this donation
            bool alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.Donation.DonationID == donationId
                            && r.User != null && r.User.UserID == currentUserId
                            && r.ReservationStatus != null
                            && (r.ReservationStatus.Name == ReservationStatusNameEnum.Pending || r.ReservationStatus.Name == ReservationStatusNameEnum.Confirmed));

            if (alreadyReserved)
            {
                TempData["ErrorMessage"] = "Você já possui uma reserva ativa para esta doação.";
                return RedirectToAction("Details", new { id = donationId });
            }

            // Create reservation
            var reservationStatus = await _context.ReservationsStatuses.FirstOrDefaultAsync(rs => rs.Name == ReservationStatusNameEnum.Pending);
            var interestedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserID == currentUserId);

            var reservation = new Reservation
            {
                ReservationDate = DateTime.UtcNow,
                Donation = donation,
                User = interestedUser,
                ReservationStatus = reservationStatus
            };

            _context.Reservations.Add(reservation);

            // Optionally mark donation as reserved
            var reservedStatus = await _context.DonationStatuses.FirstOrDefaultAsync(ds => ds.Name == DonationStatusNameEnum.Reserved);
            if (reservedStatus != null)
            {
                donation.DonationStatus = reservedStatus;
                _context.Donations.Update(donation);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reserva de adoção realizada com sucesso! Acompanhe o status abaixo.";

            return RedirectToAction(nameof(MyAdoptions));
        }

        // GET: Donations
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;

            var query = _context.Donations
                .Include(d => d.DonationImages)
                .Include(d => d.Category)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.Title.Contains(searchString)
                                      || (d.Description != null && d.Description.Contains(searchString))
                                      || (d.Category != null && d.Category.Name.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(d => d.RegistrationDate);
                    break;
                case "title_asc":
                    query = query.OrderBy(d => d.Title);
                    break;
                case "title_desc":
                    query = query.OrderByDescending(d => d.Title);
                    break;
                case "date_desc":
                default:
                    query = query.OrderByDescending(d => d.RegistrationDate);
                    break;
            }

            return View(await query.ToListAsync());
        }

        // GET: Donations/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var donation = await _context.Donations
            //    .FirstOrDefaultAsync(m => m.DonationID == id);

            var donation = await _context.Donations
                .Include(d => d.User)
                .Include(d => d.Category)
                .Include(d => d.DonationStatus)
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.ReservationStatus)
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.User)
                .Include(d => d.DonationImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.DonationID == id);


            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // POST: Donations/ApproveReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReservation(int reservationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _context.Reservations
                .Include(r => r.Donation)
                    .ThenInclude(d => d.User)
                .Include(r => r.Donation)
                    .ThenInclude(d => d.DonationStatus)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationID == reservationId);

            if (reservation == null || reservation.Donation == null || reservation.Donation.User == null || reservation.Donation.User.UserID != currentUserId)
            {
                TempData["ErrorMessage"] = "Apenas o proprietário da doação pode aprovar uma reserva.";
                return RedirectToAction("Index", "Home");
            }

            // Mark this reservation as Confirmed
            var confirmedStatus = await _context.ReservationsStatuses
                .FirstOrDefaultAsync(rs => rs.Name == ReservationStatusNameEnum.Confirmed);
            if (confirmedStatus != null)
            {
                reservation.ReservationStatus = confirmedStatus;
                _context.Reservations.Update(reservation);
            }

            // Cancel other pending reservations for this same donation
            var cancelledStatus = await _context.ReservationsStatuses
                .FirstOrDefaultAsync(rs => rs.Name == ReservationStatusNameEnum.Cancelled);
            if (cancelledStatus != null)
            {
                var otherReservations = await _context.Reservations
                    .Where(r => r.Donation.DonationID == reservation.Donation.DonationID && r.ReservationID != reservationId && r.ReservationStatus.Name == ReservationStatusNameEnum.Pending)
                    .ToListAsync();

                foreach (var other in otherReservations)
                {
                    other.ReservationStatus = cancelledStatus;
                    _context.Reservations.Update(other);
                }
            }

            // Change donation status to Completed so it gets out from the feed!
            var completedStatus = await _context.DonationStatuses
                .FirstOrDefaultAsync(ds => ds.Name == DonationStatusNameEnum.Completed);
            if (completedStatus == null)
            {
                completedStatus = new DonationStatus { Name = DonationStatusNameEnum.Completed };
                _context.DonationStatuses.Add(completedStatus);
                await _context.SaveChangesAsync();
            }

            reservation.Donation.DonationStatus = completedStatus;
            _context.Donations.Update(reservation.Donation);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Reserva aprovada para {reservation.User?.Name ?? reservation.User?.Email}! A doação foi concluída e removida do feed.";

            return RedirectToAction("Details", "Donations", new { id = reservation.Donation.DonationID });
        }

        // POST: Donations/CompleteDonation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteDonation(int donationId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var donation = await _context.Donations
                .Include(d => d.User)
                .Include(d => d.DonationStatus)
                .FirstOrDefaultAsync(d => d.DonationID == donationId && d.User.UserID == currentUserId);

            if (donation == null)
            {
                return NotFound();
            }

            var completedStatus = await _context.DonationStatuses
                .FirstOrDefaultAsync(ds => ds.Name == DonationStatusNameEnum.Completed);
            if (completedStatus == null)
            {
                completedStatus = new DonationStatus { Name = DonationStatusNameEnum.Completed };
                _context.DonationStatuses.Add(completedStatus);
                await _context.SaveChangesAsync();
            }

            donation.DonationStatus = completedStatus;
            _context.Donations.Update(donation);

            // Whenever donation status changes to Completed, update pending reservations to Confirmed
            var confirmedStatus = await _context.ReservationsStatuses
                .FirstOrDefaultAsync(rs => rs.Name == ReservationStatusNameEnum.Confirmed);
            if (confirmedStatus != null)
            {
                var pendingReservations = await _context.Reservations
                    .Where(r => r.Donation.DonationID == donationId && r.ReservationStatus.Name == ReservationStatusNameEnum.Pending)
                    .ToListAsync();

                foreach (var res in pendingReservations)
                {
                    res.ReservationStatus = confirmedStatus;
                    _context.Reservations.Update(res);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Doação marcada como Concluída! As reservas pendentes foram atualizadas para Confirmadas.";

            return RedirectToAction("Details", new { id = donationId });
        }

        // GET: Donations/Create
        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.AsNoTracking().ToList(), "CategoryID", "Name");
            return View();
        }

        // POST: Donations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DonationID,Title,Description")] Donation donation, int? categoryId, Microsoft.AspNetCore.Http.IFormFileCollection images)
        {
            // Validate user is authenticated
            if (User?.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Donations") });
            }

            // Extract current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Create", "Donations") });
            }

            if (ModelState.IsValid)
            {
                // Set the UserID to the current authenticated user
                donation.UserID = currentUserId;

                // Set RegistrationDate automatically to current local time
                donation.RegistrationDate = DateTime.Now;

                // Set default status to "Available"
                var availableStatus = await _context.DonationStatuses
                    .FirstOrDefaultAsync(ds => ds.Name == DonationStatusNameEnum.Available);
                if (availableStatus != null)
                {
                    donation.DonationStatus = availableStatus;
                }

                if (categoryId.HasValue)
                {
                    var cat = await _context.Categories.FindAsync(categoryId.Value);
                    if (cat != null) donation.Category = cat;
                }

                // add donation first to get its ID
                _context.Add(donation);
                await _context.SaveChangesAsync();

                if (images != null && images.Count > 0)
                {
                    foreach (var file in images)
                    {
                        if (file != null && file.Length > 0)
                        {
                            using var ms = new System.IO.MemoryStream();
                            await file.CopyToAsync(ms);
                            var img = new DonationImage
                            {
                                DonationId = donation.DonationID,
                                ImageData = ms.ToArray(),
                                ContentType = file.ContentType ?? "application/octet-stream",
                                FileName = file.FileName,
                                DisplayOrder = 0
                            };
                            _context.DonationImages.Add(img);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Doação criada com sucesso!";
                return RedirectToAction(nameof(MyDonations));
            }
            ViewBag.CategoryList = new SelectList(_context.Categories.AsNoTracking().ToList(), "CategoryID", "Name", categoryId);
            return View(donation);
        }

        // GET: Donations/Edit/5
        // Edit action includes images and category selection for the form.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.DonationImages)
                .Include(d => d.Category)
                .FirstOrDefaultAsync(d => d.DonationID == id);
            if (donation == null)
            {
                return NotFound();
            }
            ViewBag.CategoryList = new SelectList(_context.Categories.AsNoTracking().ToList(), "CategoryID", "Name", donation.Category?.CategoryID);
            return View(donation);
        }

        // POST: Donations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DonationID,Title,Description")] Donation donation, int? categoryId, Microsoft.AspNetCore.Http.IFormFileCollection images, int[] removeImageIds)
        {
            if (id != donation.DonationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Donations.Include(d => d.DonationImages).FirstOrDefaultAsync(d => d.DonationID == id);
                    if (existing == null) return NotFound();

                    existing.Title = donation.Title;
                    existing.Description = donation.Description;

                    if (categoryId.HasValue)
                    {
                        var cat = await _context.Categories.FindAsync(categoryId.Value);
                        existing.Category = cat;
                    }

                    // remove selected images
                    if (removeImageIds != null && removeImageIds.Length > 0)
                    {
                        foreach (var imgId in removeImageIds)
                        {
                            var img = await _context.DonationImages.FindAsync(imgId);
                            if (img != null && img.DonationId == existing.DonationID)
                            {
                                _context.DonationImages.Remove(img);
                            }
                        }
                    }

                    // add uploaded images
                    if (images != null && images.Count > 0)
                    {
                        foreach (var file in images)
                        {
                            if (file != null && file.Length > 0)
                            {
                                using var ms = new System.IO.MemoryStream();
                                await file.CopyToAsync(ms);
                                var img = new DonationImage
                                {
                                    DonationId = existing.DonationID,
                                    ImageData = ms.ToArray(),
                                    ContentType = file.ContentType ?? "application/octet-stream",
                                    FileName = file.FileName,
                                    DisplayOrder = 0
                                };
                                _context.DonationImages.Add(img);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(MyDonations));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonationExists(donation.DonationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.CategoryList = new SelectList(_context.Categories.AsNoTracking().ToList(), "CategoryID", "Name", categoryId);
            return View(donation);
        }

        // GET: Donations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.Category)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.DonationID == id);
            if (donation == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId) || donation.User == null || donation.User.UserID != currentUserId)
            {
                TempData["ErrorMessage"] = "Você não tem permissão para excluir esta doação.";
                return RedirectToAction("Index", "Home");
            }

            return View(donation);
        }

        // POST: Donations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var donation = await _context.Donations
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DonationID == id && d.User.UserID == currentUserId);

            if (donation != null)
            {
                // Remove dependent reservations first to prevent SQL FK constraint exception
                var reservations = await _context.Reservations
                    .Where(r => r.Donation.DonationID == donation.DonationID)
                    .ToListAsync();
                if (reservations.Any())
                {
                    _context.Reservations.RemoveRange(reservations);
                }

                // Remove dependent images first
                var images = await _context.DonationImages
                    .Where(di => di.DonationId == donation.DonationID)
                    .ToListAsync();
                if (images.Any())
                {
                    _context.DonationImages.RemoveRange(images);
                }

                _context.Donations.Remove(donation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Doação excluída com sucesso!";
            }
            return RedirectToAction(nameof(MyDonations));
        }

        private bool DonationExists(int id)
        {
            return _context.Donations.Any(e => e.DonationID == id);
        }
    }
}
