using System.ComponentModel.DataAnnotations;

namespace KTXReportSystem.Models
{
    public class RoomStatus
    {
        [Key]
        public string RoomId { get; set; } = string.Empty; // Ví dụ: K1501

        public string BuildingCode { get; set; } = string.Empty; // Ví dụ: K1

        public string CurrentReportUrgency { get; set; } = "None";

        // --- BỔ SUNG TRƯỜNG THIẾU ---
        public DateTime? LastReportDate { get; set; }
        // -----------------------------
    }
}