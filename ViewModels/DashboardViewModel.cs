using NoorSardPlatform.Models;

namespace NoorSardPlatform.ViewModels
{
    public class DashboardViewModel
    {
        public List<Participant> Participants { get; set; } = new();

        public int ParticipantsCount { get; set; }

        public decimal TotalCompletedParts { get; set; }

        public decimal TotalTargetParts { get; set; }

        public double OverallPercentage { get; set; }

        public int YearMemorizationCompletedCount { get; set; }

        public int BronzeMedalCount { get; set; }

        public int SilverMedalCount { get; set; }

        public int GoldMedalCount { get; set; }

        public List<string> ChartLabels { get; set; } = new();

        public List<int> ChartValues { get; set; } = new();
    }
}