
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Models;
using QuanLyKhachSan.Data;

[Authorize(Roles = "Admin,LeTan")]
public class KhachHangsController : Controller
{
    private readonly ApplicationDbContext _context;

    public KhachHangsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: KhachHangs
    public async Task<IActionResult> Index(string? tuKhoa)
    {
        var query = _context.KhachHang.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            tuKhoa = tuKhoa.Trim();

            query = query.Where(k =>
                k.HoTen.Contains(tuKhoa) ||
                k.SoDienThoai.Contains(tuKhoa) ||
                (k.CCCD != null && k.CCCD.Contains(tuKhoa)));
        }

        ViewBag.TuKhoa = tuKhoa;

        var danhSach = await query
            .OrderBy(k => k.HoTen)
            .ToListAsync();

        // Lấy danh sách khách hàng đang lưu trú.
        var danhSachDangThue = await _context.DatPhong
            .Where(d => d.TrangThai == "Đã check-in")
            .Select(d => d.MaKhach)
            .Distinct()
            .ToListAsync();

        // View sử dụng danh sách này để hiển thị trạng thái.
        ViewBag.DangThueIds = danhSachDangThue.ToHashSet();

        return View(danhSach);
    }

    // GET: KhachHangs/Details/5
    public async Task<IActionResult> Details(int? makhach)
    {
        if (makhach == null)
            return NotFound();

        var khachhang = await _context.KhachHang
            .FirstOrDefaultAsync(k => k.MaKhach == makhach);

        if (khachhang == null)
            return NotFound();

        ViewBag.DangThue = await _context.DatPhong.AnyAsync(d =>
            d.MaKhach == makhach &&
            d.TrangThai == "Đã check-in");

        return View(khachhang);
    }

    // GET: KhachHangs/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: KhachHangs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaKhach,HoTen,CCCD,SoDienThoai,Email,DiaChi")]
        KhachHang khachhang)
    {
        if (ModelState.IsValid)
        {
            _context.KhachHang.Add(khachhang);
            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] =
                $"Đã thêm khách hàng '{khachhang.HoTen}' thành công.";

            return RedirectToAction(nameof(Index));
        }

        return View(khachhang);
    }

    // GET: KhachHangs/Edit/5
    public async Task<IActionResult> Edit(int? makhach)
    {
        if (makhach == null)
            return NotFound();

        var khachhang = await _context.KhachHang.FindAsync(makhach);

        if (khachhang == null)
            return NotFound();

        return View(khachhang);
    }

    // POST: KhachHangs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? makhach,
        [Bind("MaKhach,HoTen,CCCD,SoDienThoai,Email,DiaChi")]
        KhachHang khachhang)
    {
        if (makhach == null || makhach != khachhang.MaKhach)
            return NotFound();

        if (ModelState.IsValid)
        {
            var khachHangTrongDb = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.MaKhach == makhach);

            if (khachHangTrongDb == null)
                return NotFound();

            khachHangTrongDb.HoTen = khachhang.HoTen;
            khachHangTrongDb.CCCD = khachhang.CCCD;
            khachHangTrongDb.SoDienThoai = khachhang.SoDienThoai;
            khachHangTrongDb.Email = khachhang.Email;
            khachHangTrongDb.DiaChi = khachhang.DiaChi;

            await _context.SaveChangesAsync();

            TempData["ThongBaoThanhCong"] =
                $"Đã cập nhật khách hàng '{khachHangTrongDb.HoTen}' thành công.";

            return RedirectToAction(nameof(Index));
        }

        return View(khachhang);
    }

    // GET: KhachHangs/Delete/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? makhach)
    {
        if (makhach == null)
            return NotFound();

        var khachhang = await _context.KhachHang
            .FirstOrDefaultAsync(k => k.MaKhach == makhach);

        if (khachhang == null)
            return NotFound();

        return View(khachhang);
    }

    // POST: KhachHangs/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? makhach)
    {
        if (makhach == null)
            return NotFound();

        var khachhang = await _context.KhachHang
            .FirstOrDefaultAsync(k => k.MaKhach == makhach);

        if (khachhang == null)
            return NotFound();

        // Không xóa khách hàng đã có lịch sử đặt phòng.
        bool coLichSuDatPhong = await _context.DatPhong
            .AnyAsync(d => d.MaKhach == makhach);

        if (coLichSuDatPhong)
        {
            TempData["ThongBaoLoi"] =
                $"Không thể xóa khách hàng '{khachhang.HoTen}' vì khách hàng đã có lịch sử đặt phòng.";

            return RedirectToAction(nameof(Index));
        }

        _context.KhachHang.Remove(khachhang);
        await _context.SaveChangesAsync();

        TempData["ThongBaoThanhCong"] =
            $"Đã xóa khách hàng '{khachhang.HoTen}' thành công.";

        return RedirectToAction(nameof(Index));
    }
}
