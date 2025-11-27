using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KTXReportSystem.Data;
using KTXReportSystem.Models;
using Microsoft.EntityFrameworkCore;
using KTXReportSystem.Services;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace KTXReportSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportService _reportService;

        public IndexModel(ApplicationDbContext context, ReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public User? CurrentUser { get; set; }

        // Biến thông báo (TempData)
        [TempData] public string LoginError { get; set; } = string.Empty;
        [TempData] public string LoginSuccess { get; set; } = string.Empty;
        [TempData] public string ReportError { get; set; } = string.Empty;
        [TempData] public string ReportSuccess { get; set; } = string.Empty;
        [TempData] public string ActionError { get; set; } = string.Empty;
        [TempData] public string ActionSuccess { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var userIdCode = HttpContext.Session.GetString("UserIdCode");
            if (!string.IsNullOrEmpty(userIdCode))
            {
                CurrentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserIdCode == userIdCode);
            }
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("UserIdCode");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLogin(string Username, string Password)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) return RedirectToPage();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserIdCode.ToUpper() == Username.Trim().ToUpper());

            if (user == null || user.PasswordHash != Password.Trim())
            {
                LoginError = "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return RedirectToPage();
            }

            if (user.Role == "Pending")
            {
                LoginError = "Tài khoản đang chờ Quản lý xét duyệt.";
                return RedirectToPage();
            }

            HttpContext.Session.SetString("UserIdCode", user.UserIdCode);
            LoginSuccess = $"Xin chào {user.FullName}!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRegisterAsync(string RegUserIdCode, string RegFullName, string RegPassword, string RegBuilding, string RegRoomNum, string RegStudentInfo)
        {
            if (await _context.Users.AnyAsync(u => u.UserIdCode == RegUserIdCode))
            {
                ActionError = "Tài khoản đã tồn tại.";
                return RedirectToPage();
            }

            // Ghép Tòa và Phòng (VD: K1 + 101 => K1101)
            string fullRoomId = (!string.IsNullOrEmpty(RegBuilding) && !string.IsNullOrEmpty(RegRoomNum))
                                ? $"{RegBuilding}{RegRoomNum}" : null;

            _context.Users.Add(new User
            {
                UserIdCode = RegUserIdCode,
                FullName = RegFullName,
                PasswordHash = RegPassword,
                RoomId = fullRoomId,
                StudentInfo = RegStudentInfo,
                Role = "Pending"
            });
            await _context.SaveChangesAsync();
            ActionSuccess = "Đăng ký thành công! Vui lòng chờ duyệt.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            var uid = HttpContext.Session.GetString("UserIdCode"); if (uid == null) return RedirectToPage();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserIdCode == uid);
            if (user == null || user.PasswordHash != OldPassword || NewPassword != ConfirmPassword)
            {
                ActionError = "Thông tin không đúng."; return RedirectToPage();
            }
            user.PasswordHash = NewPassword;
            await _context.SaveChangesAsync();
            ActionSuccess = "Đổi mật khẩu thành công!";
            return RedirectToPage();
        }

        // XỬ LÝ GỬI BÁO CÁO (CÓ FILE ẢNH)
        public async Task<IActionResult> OnPostSubmitReportAsync([FromForm] Report newReport, IFormFile EvidenceFile)
        {
            try
            {
                newReport.SubmissionDate = DateTime.Now;
                // Tự động lấy mã tòa (2 ký tự đầu)
                newReport.BuildingCode = newReport.RoomId?.Length >= 2 ? newReport.RoomId.Substring(0, 2) : "K1";

                // Chuyển File sang Base64
                if (EvidenceFile != null && EvidenceFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await EvidenceFile.CopyToAsync(ms);
                        var fileBytes = ms.ToArray();
                        string mimeType = EvidenceFile.ContentType;
                        string base64 = Convert.ToBase64String(fileBytes);
                        newReport.EvidenceBase64 = $"data:{mimeType};base64,{base64}";
                    }
                }

                await _reportService.SubmitReportAsync(newReport);
                ReportSuccess = "Gửi báo cáo thành công!";
            }
            catch (Exception ex)
            {
                ReportError = "Lỗi: " + ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveUserAsync(int UserId)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user != null && user.Role == "Pending")
            {
                user.Role = "student";
                if (!string.IsNullOrEmpty(user.RoomId) && !await _context.RoomStatuses.AnyAsync(r => r.RoomId == user.RoomId))
                {
                    string bCode = user.RoomId.Length >= 2 ? user.RoomId.Substring(0, 2) : "K1";
                    _context.RoomStatuses.Add(new RoomStatus { RoomId = user.RoomId, BuildingCode = bCode });
                }
                await _context.SaveChangesAsync(); ActionSuccess = "Đã duyệt!";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectUserAsync(int UserId)
        {
            var user = await _context.Users.FindAsync(UserId);
            if (user != null) { _context.Users.Remove(user); await _context.SaveChangesAsync(); ActionSuccess = "Đã từ chối."; }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAssignReportAsync(int ReportId, string AssignedToUserIdCode)
        {
            await _reportService.ApproveReportAsync(ReportId, AssignedToUserIdCode); ActionSuccess = "Đã phân công!"; return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int ReportId, string TechnicianUpdate, string NewStatus)
        {
            var uid = HttpContext.Session.GetString("UserIdCode"); if (uid == null) return RedirectToPage();
            await _reportService.UpdateReportStatusAsync(ReportId, uid, NewStatus, TechnicianUpdate); ActionSuccess = "Đã cập nhật!"; return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteReportAsync(int ReportId)
        {
            var uid = HttpContext.Session.GetString("UserIdCode"); if (uid == null) return RedirectToPage();
            await _reportService.UpdateReportStatusAsync(ReportId, uid, "Completed", "Đã hoàn thành."); ActionSuccess = "Đã hoàn thành!"; return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelReportAsync(int ReportId)
        {
            var uid = HttpContext.Session.GetString("UserIdCode"); if (uid == null) return RedirectToPage();
            await _reportService.UpdateReportStatusAsync(ReportId, uid, "Canceled", "Đã hủy."); ActionSuccess = "Đã hủy."; return RedirectToPage();
        }
    }
}