using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoorSardPlatform.Models
{
    public class Participant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الحافظة مطلوب")]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Range(0, 30)]
        public decimal TargetParts { get; set; }

        [Range(0, 30)]
        public decimal CompletedParts { get; set; }

        public bool BronzeMedal { get; set; }

        public bool SilverMedal { get; set; }

        public bool GoldMedal { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        [NotMapped]
        public double CompletionPercentage
        {
            get
            {
                if (TargetParts <= 0)
                {
                    return 0;
                }

                double percentage =
                     (double)(CompletedParts / TargetParts * 100);

                return Math.Min(percentage, 100);
            }
        }

        [NotMapped]
        public bool HasCompletedTarget =>
            TargetParts > 0 && CompletedParts >= TargetParts;
    }
}