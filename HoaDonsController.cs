
using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "Admin,LeTan")]
    public class HoaDonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DANH SÁCH HÓA ĐƠN
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var ds = _context.HoaDon
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.KhachHang)
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.Phong)
                .OrderByDescending(h => h.NgayLap);

            return View(await ds.ToListAsync());
        }

        // ==========================================
        // KHÁCH HÀNG: HÓA ĐƠN CỦA TÔI
        // ==========================================

        [AllowAnonymous]
        public async Task<IActionResult> HoaDonCuaToi()
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Forbid();

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k =>
                    k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            var dsHoaDon = await _context.HoaDon
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.Phong)
                        .ThenInclude(p => p!.LoaiPhong)

                // Tải toàn bộ lịch sử thanh toán của đặt phòng
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.ThanhToans)

                .Where(h =>
                    h.DatPhong != null &&
                    h.DatPhong.MaKhach == khachHang.MaKhach)

                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(dsHoaDon);
        }

        // ==========================================
        // CHI TIẾT HÓA ĐƠN
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var hoaDon = await _context.HoaDon
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.KhachHang)
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.Phong)
                        .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(h =>
                    h.MaHoaDon == id);

            if (hoaDon == null)
                return NotFound();

            ViewBag.CacKhoanThanhToan =
                await _context.ThanhToan
                    .Where(t =>
                        t.MaDatPhong == hoaDon.MaDatPhong)
                    .OrderBy(t => t.NgayTao)
                    .ToListAsync();

            return View(hoaDon);
        }


        // ==========================================
        // KHÁCH HÀNG: CHI TIẾT HÓA ĐƠN CỦA TÔI
        // ==========================================

        [AllowAnonymous]
        public async Task<IActionResult> ChiTietCuaToi(int id)
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Forbid();

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k =>
                    k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            var hoaDon = await _context.HoaDon
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.Phong)
                        .ThenInclude(p => p!.LoaiPhong)
                .Include(h => h.DatPhong)
                    .ThenInclude(d => d!.KhachHang)
                .FirstOrDefaultAsync(h =>
                    h.MaHoaDon == id &&
                    h.DatPhong != null &&
                    h.DatPhong.MaKhach == khachHang.MaKhach);

            if (hoaDon == null)
                return NotFound();

            ViewBag.CacKhoanThanhToan =
                await _context.ThanhToan
                    .Where(t => t.MaDatPhong == hoaDon.MaDatPhong)
                    .OrderBy(t => t.NgayTao)
                    .ToListAsync();

            return View(hoaDon);
        }


        // ==========================================
        // XÁC NHẬN THANH TOÁN PHẦN CÒN LẠI
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(
            int id,
            string phuongThuc)
        {
            if (phuongThuc != "ChuyenKhoanMoPhong" &&
                phuongThuc != "TienMat")
            {
                TempData["Loi"] =
                    "Phương thức thanh toán không hợp lệ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var hoaDon = await _context.HoaDon
                .FirstOrDefaultAsync(h =>
                    h.MaHoaDon == id);

            if (hoaDon == null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (hoaDon.DaThanhToan)
            {
                await transaction.RollbackAsync();

                TempData["ThongBao"] =
                    "Hóa đơn này đã được thanh toán đầy đủ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            if (hoaDon.TongTien <= 0)
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Tổng tiền hóa đơn không hợp lệ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            int maDatPhong = hoaDon.MaDatPhong;

            var cacKhoanThanhToan = await _context.ThanhToan
                .Where(t => t.MaDatPhong == maDatPhong)
                .ToListAsync();

            decimal tienDaThu = cacKhoanThanhToan
                .Where(t => t.TrangThai == "ThanhCong")
                .Sum(t => t.SoTien);

            decimal tienConPhaiThu =
                Math.Max(0, hoaDon.TongTien - tienDaThu);

            if (tienConPhaiThu <= 0)
            {
                hoaDon.DaThanhToan = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    "Hóa đơn đã được thanh toán đầy đủ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var thanhToanConLai = cacKhoanThanhToan
                .FirstOrDefault(t =>
                    t.LoaiThanhToan == "ThanhToanConLai" &&
                    t.TrangThai != "ThanhCong");

            if (thanhToanConLai == null)
            {
                thanhToanConLai = new ThanhToan
                {
                    MaDatPhong = maDatPhong,
                    SoTien = tienConPhaiThu,
                    LoaiThanhToan = "ThanhToanConLai",
                    PhuongThuc = phuongThuc,
                    TrangThai = "ChoThanhToan",
                    NgayTao = DateTime.Now
                };

                _context.ThanhToan.Add(
                    thanhToanConLai);
            }
            else
            {
                thanhToanConLai.SoTien =
                    tienConPhaiThu;

                thanhToanConLai.PhuongThuc =
                    phuongThuc;
            }

            thanhToanConLai.TrangThai = "ThanhCong";
            thanhToanConLai.NgayThanhToan =
                DateTime.Now;

            thanhToanConLai.MaGiaoDich =
                (phuongThuc == "TienMat"
                    ? "THUMAT-"
                    : "MOPHONG-") +
                Guid.NewGuid().ToString("N")[..12]
                    .ToUpper();

            hoaDon.DaThanhToan =
                tienDaThu + thanhToanConLai.SoTien
                >= hoaDon.TongTien;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            if (hoaDon.DaThanhToan)
            {
                TempData["ThongBao"] =
                    phuongThuc == "TienMat"
                        ? "Đã xác nhận thu tiền mặt. Hóa đơn đã thanh toán đầy đủ."
                        : "Đã xác nhận thanh toán mô phỏng. Hóa đơn đã thanh toán đầy đủ.";
            }
            else
            {
                TempData["ThongBao"] =
                    "Đã ghi nhận khoản thanh toán.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}
