using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EloDoacoes.Data;
using EloDoacoes.Models;

namespace EloDoacoes.Controllers
{
    public class DonationsController : Controller
    {
        private readonly EloDoacoesContext _context;
        // Category selector and image upload/remove support added to Create and Edit actions. (patched)

        public DonationsController(EloDoacoesContext context)
        {
            _context = context;
        }

        // GET: Donations
        public async Task<IActionResult> Index()
        {
            // include images so the Index view can show thumbnails
            var donations = await _context.Donations
                .Include(d => d.DonationImages)
                .AsNoTracking()
                .ToListAsync();
            return View(donations);
        }

        // GET: Donations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var donation = await _context.Donations
            //    .FirstOrDefaultAsync(m => m.DonationID == id);

            var donation = await _context.Donations
                .Include(d => d.Reservations)
                    .ThenInclude(r => r.ReservationStatus)
                .Include(d => d.DonationImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.DonationID == id);


            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
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
        public async Task<IActionResult> Create([Bind("DonationID,Title,Description,RegistrationDate")] Donation donation, int? categoryId, Microsoft.AspNetCore.Http.IFormFileCollection? images)
        {
            if (ModelState.IsValid)
            {
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
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("DonationID,Title,Description,RegistrationDate")] Donation donation, int? categoryId, Microsoft.AspNetCore.Http.IFormFileCollection? images, int[]? removeImageIds)
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
                    existing.RegistrationDate = donation.RegistrationDate;

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
                    return RedirectToAction(nameof(Index));
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
                .FirstOrDefaultAsync(m => m.DonationID == id);
            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // POST: Donations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation != null)
            {
                _context.Donations.Remove(donation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DonationExists(int id)
        {
            return _context.Donations.Any(e => e.DonationID == id);
        }
    }
}
