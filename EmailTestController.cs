
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyKhachSan.Services;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmailTestController : Controller
    {
        private readonly EmailService _emailService;

        public EmailTestController(EmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiEmail(string emailNhan)
        {
            if (string.IsNullOrWhiteSpace(emailNhan))
            {
                TempData["Loi"] = "Vui lòng nhập email người nhận.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _emailService.GuiEmailAsync(
                    emailNhan,
                    "Kiểm tra gửi email - HOTEL",
                    "<h2>Gửi email thành công!</h2>" +
                    "<p>Hệ thống quản lý khách sạn đã kết nối Gmail thành công.</p>"
                );

                TempData["ThongBao"] = "Gửi email thử nghiệm thành công!";
            }
            catch (Exception ex)
            {
                TempData["Loi"] = "Lỗi gửi email: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
