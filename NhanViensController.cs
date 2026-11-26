using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NhanViensController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public NhanViensController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var ds = new List<NhanVienItem>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                ds.Add(new NhanVienItem
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    VaiTro = roles.FirstOrDefault() ?? "(chưa có vai trò)"
                });
            }
            return View(ds);
        }

        public IActionResult Create()
        {
            return View(new TaoNhanVienModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoNhanVienModel model)
        {
            if (model.VaiTro != "Admin" && model.VaiTro != "LeTan")
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ.");
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
            var kq = await _userManager.CreateAsync(user, model.MatKhau);
            if (!kq.Succeeded)
            {
                foreach (var e in kq.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.VaiTro);
            TempData["ThongBao"] = $"Đã tạo tài khoản {model.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiVaiTro(string id, string vaiTro)
        {
            if (vaiTro != "Admin" && vaiTro != "LeTan") return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Loi"] = "Không thể đổi vai trò của chính tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            var cu = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, cu);
            await _userManager.AddToRoleAsync(user, vaiTro);

            TempData["ThongBao"] = $"Đã đổi vai trò của {user.Email} thành {vaiTro}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Loi"] = "Không thể xóa chính tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);
            TempData["ThongBao"] = $"Đã xóa tài khoản {user.Email}.";
            return RedirectToAction(nameof(Index));
        }
    }
}