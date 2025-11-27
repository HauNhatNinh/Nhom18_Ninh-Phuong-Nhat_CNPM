using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTXReportSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserIdCode { get; set; } = string.Empty; // Mã SV, Mã QL, Mã KT

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // "student", "manager", "technician"

        // Thông tin thêm cho Sinh viên
        [StringLength(10)]
        public string? RoomId { get; set; } // Ví dụ: K1501

        [StringLength(255)]
        public string? StudentInfo { get; set; } // Lớp, Khoa...
    }
}