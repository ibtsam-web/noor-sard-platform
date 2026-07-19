using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoorSardPlatform.Data;
using NoorSardPlatform.ViewModels;

namespace NoorSardPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var participants = await _context.Participants
                .OrderBy(participant => participant.FullName)
                .ToListAsync();

            decimal totalTargetParts = participants.Sum(
                participant => participant.TargetParts
            );

            decimal totalCompletedParts = participants.Sum(
                participant => participant.CompletedParts
            );

            double overallPercentage = totalTargetParts > 0
                ? (double)(totalCompletedParts / totalTargetParts * 100)
                : 0;

            overallPercentage = Math.Min(overallPercentage, 100);

            var completedPartsGroups = participants
                .Where(participant => participant.CompletedParts > 0)
                .GroupBy(participant =>
                    (int)Math.Floor(participant.CompletedParts)
                )
                .OrderBy(group => group.Key)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                Participants = participants,

                ParticipantsCount = participants.Count,

                TotalCompletedParts = totalCompletedParts,

                TotalTargetParts = totalTargetParts,

                OverallPercentage = overallPercentage,

                YearMemorizationCompletedCount = participants.Count(
                    participant => participant.BronzeMedal
                ),

                BronzeMedalCount = participants.Count(
                    participant => participant.BronzeMedal
                ),

                SilverMedalCount = participants.Count(
                    participant => participant.SilverMedal
                ),

                GoldMedalCount = participants.Count(
                    participant => participant.GoldMedal
                ),

                ChartLabels = completedPartsGroups
                    .Select(group => $"{group.Key} جزء")
                    .ToList(),

                ChartValues = completedPartsGroups
                    .Select(group => group.Count())
                    .ToList()
            };

            return View(viewModel);
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true
        )]

        [HttpGet]
public async Task<IActionResult> DashboardData()
{
    var participants = await _context.Participants
        .AsNoTracking()
        .ToListAsync();

    decimal totalTargetParts = participants.Sum(
        participant => participant.TargetParts
    );

    decimal totalCompletedParts = participants.Sum(
        participant => participant.CompletedParts
    );

    double overallPercentage = totalTargetParts > 0
         ? (double)(totalCompletedParts / totalTargetParts * 100)
        : 0;

    overallPercentage = Math.Min(overallPercentage, 100);

    var completedPartsGroups = participants
        .Where(participant => participant.CompletedParts > 0)
        .GroupBy(participant =>
            (int)Math.Floor(participant.CompletedParts)
        )
        .OrderBy(group => group.Key)
        .ToList();

    return Json(new
    {
        participantsCount = participants.Count,

        totalCompletedParts,

        totalTargetParts,

        overallPercentage = Math.Round(overallPercentage),

        yearMemorizationCompletedCount = participants.Count(
            participant => participant.BronzeMedal
        ),

        bronzeMedalCount = participants.Count(
            participant => participant.BronzeMedal
        ),

        silverMedalCount = participants.Count(
            participant => participant.SilverMedal
        ),

        goldMedalCount = participants.Count(
            participant => participant.GoldMedal
        ),

        chartLabels = completedPartsGroups
            .Select(group => $"{group.Key} جزء")
            .ToList(),

        chartValues = completedPartsGroups
            .Select(group => group.Count())
            .ToList()
    });
}

        public IActionResult Error()
        {
            return View();
        }
    }
}