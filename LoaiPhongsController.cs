using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoaiPhongsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoaiPhongsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách
        public async Task<IActionResult> Index()
        {
            var ds = await _context.LoaiPhong
                .Include(l => l.DanhSachPhong)
                .OrderBy(l => l.GiaMotDem)
                .ToListAsync();
            return View(ds);
        }

        // Chi tiết
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiPhong
                .FirstOrDefaultAsync(m => m.MaLoaiPhong == id);
            if (loai == null) return NotFound();

            return View(loai);
        }

        // Thêm (hiện form)
        public IActionResult Create()
        {
            return View();
        }

        // Thêm (lưu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLoaiPhong,TenLoai,GiaMotDem,SoNguoiToiDa,MoTa")] LoaiPhong loai)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loai);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loai);
        }

        // Sửa (hiện form)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiPhong.FindAsync(id);
            if (loai == null) return NotFound();

            return View(loai);
        }

        // Sửa (lưu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaLoaiPhong,TenLoai,GiaMotDem,SoNguoiToiDa,MoTa")] LoaiPhong loai)
        {
            if (id != loai.MaLoaiPhong) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiPhongExists(loai.MaLoaiPhong)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loai);
        }

        // Xóa (hiện xác nhận)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var loai = await _context.LoaiPhong
                .FirstOrDefaultAsync(m => m.MaLoaiPhong == id);
            if (loai == null) return NotFound();

            return View(loai);
        }

        // Xóa (thực hiện)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loai = await _context.LoaiPhong.FindAsync(id);
            if (loai == null) return RedirectToAction(nameof(Index));

            if (await _context.Phong.AnyAsync(p => p.MaLoaiPhong == id))
            {
                TempData["Loi"] = $"Không thể xóa loại '{loai.TenLoai}' vì vẫn còn phòng thuộc loại này.";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiPhong.Remove(loai);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã xóa loại phòng '{loai.TenLoai}'.";
            return RedirectToAction(nameof(Index));
        }

        private bool LoaiPhongExists(int id)
        {
            return _context.LoaiPhong.Any(e => e.MaLoaiPhong == id);
        }
    }
}