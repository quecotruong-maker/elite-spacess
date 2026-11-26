
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "Admin,LeTan")]
    public class PhongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhongsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // NHÂN VIÊN: ĐÁNH DẤU PHÒNG ĐÃ DỌN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhDauDaDon(int id)
        {
            var phong = await _context.Phong.FindAsync(id);

            if (phong == null)
                return NotFound();

            if (phong.TinhTrangVeSinh == "Chưa dọn" ||
                phong.TinhTrangVeSinh == "Bẩn")
            {
                phong.TinhTrangVeSinh = "Đã dọn";

                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    $"Phòng {phong.SoPhong} đã được vệ sinh.";
            }
            else
            {
                TempData["Loi"] =
                    "Phòng không ở trạng thái cần dọn.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // NHÂN VIÊN: XÁC NHẬN ĐÃ KIỂM TRA
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDaKiemTra(int id)
        {
            var phong = await _context.Phong.FindAsync(id);

            if (phong == null)
                return NotFound();

            if (phong.TinhTrangVeSinh == "Đã dọn" ||
                phong.TinhTrangVeSinh == "Sạch")
            {
                phong.TinhTrangVeSinh = "Đã kiểm tra";

                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    $"Phòng {phong.SoPhong} đã được kiểm tra và sẵn sàng đón khách.";
            }
            else
            {
                TempData["Loi"] =
                    "Phòng cần được dọn trước khi xác nhận kiểm tra.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // DANH SÁCH PHÒNG
        // ==========================================

        public async Task<IActionResult> Index(
            string? tuKhoa,
            string? trangThai,
            int? maLoaiPhong)
        {
            var query = _context.Phong
                .Include(p => p.LoaiPhong)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();

                query = query.Where(p => p.SoPhong.Contains(tk));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            if (maLoaiPhong.HasValue)
            {
                query = query.Where(p => p.MaLoaiPhong == maLoaiPhong);
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;

            ViewBag.DanhSachLoai = new SelectList(
                _context.LoaiPhong,
                "MaLoaiPhong",
                "TenLoai",
                maLoaiPhong);

            return View(
                await query.OrderBy(p => p.SoPhong).ToListAsync());
        }

        // ==========================================
        // TÌM PHÒNG TRỐNG THEO NGÀY
        // ==========================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TimPhongTrong(
            DateTime? ngayDen,
            DateTime? ngayDi)
        {
            ViewBag.NgayDen = ngayDen?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.NgayDi = ngayDi?.ToString("yyyy-MM-dd") ?? "";

            bool daTimKiem = ngayDen.HasValue && ngayDi.HasValue;
            ViewBag.DaTimKiem = daTimKiem;

            if (!daTimKiem)
            {
                return View(new List<Phong>());
            }

            DateTime tuNgay = ngayDen.Value.Date;
            DateTime denNgay = ngayDi.Value.Date;

            if (tuNgay < DateTime.Today)
            {
                ViewBag.Loi =
                    "Ngày nhận phòng không được ở trong quá khứ.";

                return View(new List<Phong>());
            }

            if (tuNgay >= denNgay)
            {
                ViewBag.Loi =
                    "Ngày trả phòng phải sau ngày nhận phòng.";

                return View(new List<Phong>());
            }

            // Loại trừ phòng:
            // - Đang bảo trì
            // - Đã đặt
            // - Đã check-in
            // - Đang chờ thanh toán cọc
            //
            // Đơn chờ thanh toán cọc vẫn giữ chỗ tạm thời
            // để tránh khách khác đặt trùng ngày.

            var phongTrong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .Where(p =>
                    p.TrangThai != "Bảo trì" &&
                    !_context.DatPhong.Any(d =>
                        d.MaPhong == p.MaPhong &&
                        (
                            d.TrangThai == "Đã đặt" ||
                            d.TrangThai == "Đã check-in" ||
                            d.TrangThai == "Chờ thanh toán cọc"
                        ) &&
                        d.NgayDen < denNgay &&
                        d.NgayDi > tuNgay
                    )
                )
                .OrderBy(p => p.SoPhong)
                .ToListAsync();

            return View(phongTrong);
        }

        // ==========================================
        // CHI TIẾT PHÒNG
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var phong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(m => m.MaPhong == id);

            if (phong == null)
                return NotFound();

            return View(phong);
        }

        // ==========================================
        // THÊM PHÒNG - GET
        // ==========================================

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["MaLoaiPhong"] = new SelectList(
                _context.LoaiPhong,
                "MaLoaiPhong",
                "TenLoai");

            return View();
        }

        // ==========================================
        // THÊM PHÒNG - POST
        // ==========================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaPhong,SoPhong,MaLoaiPhong,TrangThai")] Phong phong,
            IFormFile? hinhAnh)
        {
            if (await _context.Phong.AnyAsync(
                p => p.SoPhong == phong.SoPhong))
            {
                ModelState.AddModelError(
                    nameof(Phong.SoPhong),
                    "Số phòng này đã tồn tại.");
            }

            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                var duoiFile = Path.GetExtension(
                    hinhAnh.FileName).ToLowerInvariant();

                var dinhDangHopLe = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!dinhDangHopLe.Contains(duoiFile))
                {
                    ModelState.AddModelError(
                        "hinhAnh",
                        "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
                }
                else if (hinhAnh.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "hinhAnh",
                        "Ảnh không được vượt quá 5 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                phong.TinhTrangVeSinh =
                    phong.TrangThai == "Bảo trì"
                    ? "Chưa dọn"
                    : "Đã kiểm tra";

                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    var thuMuc = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "rooms");

                    Directory.CreateDirectory(thuMuc);

                    var tenFile = Guid.NewGuid().ToString("N") +
                        Path.GetExtension(hinhAnh.FileName)
                            .ToLowerInvariant();

                    var duongDan = Path.Combine(thuMuc, tenFile);

                    using (var stream = new FileStream(
                        duongDan, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(stream);
                    }

                    phong.HinhAnh = "/images/rooms/" + tenFile;
                }

                _context.Add(phong);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    $"Đã thêm phòng {phong.SoPhong}.";

                return RedirectToAction(nameof(Index));
            }

            ViewData["MaLoaiPhong"] = new SelectList(
                _context.LoaiPhong,
                "MaLoaiPhong",
                "TenLoai",
                phong.MaLoaiPhong);

            return View(phong);
        }

        // ==========================================
        // SỬA PHÒNG - GET
        // ==========================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var phong = await _context.Phong.FindAsync(id);

            if (phong == null)
                return NotFound();

            ViewData["MaLoaiPhong"] = new SelectList(
                _context.LoaiPhong,
                "MaLoaiPhong",
                "TenLoai",
                phong.MaLoaiPhong);

            return View(phong);
        }

        // ==========================================
        // SỬA PHÒNG - POST
        // ==========================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaPhong,SoPhong,MaLoaiPhong,TrangThai")] Phong phong,
            IFormFile? hinhAnh)
        {
            if (id != phong.MaPhong)
                return NotFound();

            var phongCu = await _context.Phong
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phongCu == null)
                return NotFound();

            if (await _context.Phong.AnyAsync(p =>
                p.SoPhong == phong.SoPhong && p.MaPhong != id))
            {
                ModelState.AddModelError(
                    nameof(Phong.SoPhong),
                    "Số phòng này đã tồn tại.");
            }

            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                var duoiFile = Path.GetExtension(
                    hinhAnh.FileName).ToLowerInvariant();

                var dinhDangHopLe = new[]
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };

                if (!dinhDangHopLe.Contains(duoiFile))
                {
                    ModelState.AddModelError(
                        "hinhAnh",
                        "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
                }
                else if (hinhAnh.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "hinhAnh",
                        "Ảnh không được vượt quá 5 MB.");
                }
            }

            if (ModelState.IsValid)
            {
                phongCu.SoPhong = phong.SoPhong;
                phongCu.MaLoaiPhong = phong.MaLoaiPhong;

                if (phong.TrangThai == "Đang dọn")
                {
                    phongCu.TrangThai = "Trống";
                    phongCu.TinhTrangVeSinh = "Chưa dọn";
                }
                else
                {
                    phongCu.TrangThai = phong.TrangThai;

                    if (phong.TrangThai == "Bảo trì")
                    {
                        phongCu.TinhTrangVeSinh = "Chưa dọn";
                    }
                }

                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    var thuMuc = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "images",
                        "rooms");

                    Directory.CreateDirectory(thuMuc);

                    var tenFile = Guid.NewGuid().ToString("N") +
                        Path.GetExtension(hinhAnh.FileName)
                            .ToLowerInvariant();

                    var duongDan = Path.Combine(thuMuc, tenFile);

                    using (var stream = new FileStream(
                        duongDan, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(stream);
                    }

                    phongCu.HinhAnh = "/images/rooms/" + tenFile;
                }

                await _context.SaveChangesAsync();

                TempData["ThongBao"] =
                    $"Đã cập nhật phòng {phongCu.SoPhong}.";

                return RedirectToAction(nameof(Index));
            }

            phong.HinhAnh = phongCu.HinhAnh;
            phong.TinhTrangVeSinh = phongCu.TinhTrangVeSinh;

            ViewData["MaLoaiPhong"] = new SelectList(
                _context.LoaiPhong,
                "MaLoaiPhong",
                "TenLoai",
                phong.MaLoaiPhong);

            return View(phong);
        }

        // ==========================================
        // XÓA PHÒNG - GET
        // ==========================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var phong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(m => m.MaPhong == id);

            if (phong == null)
                return NotFound();

            return View(phong);
        }

        // ==========================================
        // XÓA PHÒNG - POST
        // ==========================================

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phong = await _context.Phong.FindAsync(id);

            if (phong == null)
                return RedirectToAction(nameof(Index));

            if (await _context.DatPhong.AnyAsync(d => d.MaPhong == id))
            {
                TempData["Loi"] =
                    $"Không thể xóa phòng {phong.SoPhong} vì đã có lịch sử đặt phòng. Nếu không dùng nữa, hãy đổi trạng thái sang 'Bảo trì'.";

                return RedirectToAction(nameof(Index));
            }

            _context.Phong.Remove(phong);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                $"Đã xóa phòng {phong.SoPhong}.";

            return RedirectToAction(nameof(Index));
        }
    }
}
