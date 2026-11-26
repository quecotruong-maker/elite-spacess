
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "KhachHang")]
    public class HoSoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HoSoController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: HoSo/Index
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound("Không tìm thấy hồ sơ khách hàng.");

            var model = new HoSoKhachHangViewModel
            {
                HoTen = khachHang.HoTen,
                CCCD = khachHang.CCCD,
                SoDienThoai = khachHang.SoDienThoai,
                Email = khachHang.Email,
                DiaChi = khachHang.DiaChi
            };

            return View(model);
        }

        // POST: HoSo/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            HoSoKhachHangViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound("Không tìm thấy hồ sơ khách hàng.");

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound("Không tìm thấy tài khoản đăng nhập.");

            // Cập nhật hồ sơ khách hàng
            khachHang.HoTen = model.HoTen.Trim();
            khachHang.CCCD = model.CCCD?.Trim();
            khachHang.SoDienThoai = model.SoDienThoai?.Trim();
            khachHang.DiaChi = model.DiaChi?.Trim();

            // Đồng bộ họ tên với tài khoản Identity
            user.HoTen = model.HoTen.Trim();

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] =
                "Cập nhật thông tin cá nhân thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}
