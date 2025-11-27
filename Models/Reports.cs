using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTXReportSystem.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; } // Primary Key

        public string RoomId { get; set; } = string.Empty;
        public string BuildingCode { get; set; } = string.Empty;
        public string ReporterUserIdCode { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        public string Urgency { get; set; } = "Normal";
        public string Status { get; set; } = "Pending_Manager";

        public string? AssignedToUserIdCode { get; set; }

        // --- CÁC TRƯỜNG CÒN THIẾU ĐÃ ĐƯỢC BỔ SUNG ---
        public string? TechnicianUpdate { get; set; } // Cập nhật tiến độ của thợ

        public string? EvidenceBase64 { get; set; } // Lưu ảnh/video dạng Base64
        // --------------------------------------------
    }
}