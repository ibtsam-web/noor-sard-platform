using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoorSardPlatform.Data;
using NoorSardPlatform.Models;
using Microsoft.AspNetCore.SignalR;
using NoorSardPlatform.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace NoorSardPlatform.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly IHubContext<DashboardHub> _dashboardHub;
        private readonly ApplicationDbContext _context;

        public SupervisorController(
            ApplicationDbContext context,
            IHubContext<DashboardHub> dashboardHub
        )
        {
            _context = context;
            _dashboardHub = dashboardHub;
        }

        [HttpGet]
        public async Task<IActionResult> Participant(int id)
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(item => item.Id == id);

            if (participant == null)
            {
                return NotFound();
            }

            return View(participant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgress(
            int id,
            int targetParts,
            int completedParts,
            bool bronzeMedal,
            bool silverMedal,
            bool goldMedal
        )
        {
            var participant = await _context.Participants
                .FirstOrDefaultAsync(item => item.Id == id);

            if (participant == null)
            {
                return NotFound();
            }

            targetParts = Math.Clamp(targetParts, 1, 30);
            completedParts = Math.Clamp(
                completedParts,
                0,
                targetParts
            );

            participant.TargetParts = targetParts;
            participant.CompletedParts = completedParts;
            participant.BronzeMedal = bronzeMedal;
            participant.SilverMedal = silverMedal;
            participant.GoldMedal = goldMedal;
            participant.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _dashboardHub.Clients.All.SendAsync(
                "DashboardUpdated"
            );

            TempData["SuccessMessage"] = "تم حفظ التحديث بنجاح ✓";

            return RedirectToAction(
                nameof(Participant),
                new { id = participant.Id }
            );
        }
    }
}