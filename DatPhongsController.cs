
using QuanLyKhachSan.Services;
using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "Admin,LeTan")]
    public class DatPhongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly ThongBaoService _thongBaoService;

        public DatPhongsController(
            ApplicationDbContext context,
            EmailService emailService,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _emailService = emailService;
            _thongBaoService = thongBaoService;
        }

        // ==========================================
        // KHÁCH HÀNG: PHÒNG CỦA TÔI
        // ==========================================

        [AllowAnonymous]
        public async Task<IActionResult> PhongCuaToi()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
            {
                TempData["Loi"] = "Không tìm thấy thông tin khách hàng.";
                return View(new List<DatPhong>());
            }

            var danhSach = await _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .Where(d => d.MaKhach == khachHang.MaKhach)
                .OrderByDescending(d => d.NgayDen)
                .ToListAsync();

            return View(danhSach);
        }

        // ==========================================
        // KHÁCH HÀNG: CHI TIẾT ĐẶT PHÒNG CỦA TÔI
        // ==========================================

        [AllowAnonymous]
        public async Task<IActionResult> ChiTietCuaToi(int? id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            var datPhong = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(d =>
                    d.MaDatPhong == id &&
                    d.MaKhach == khachHang.MaKhach);

            if (datPhong == null)
                return NotFound();

            return View(datPhong);
        }

        // ==========================================
        // ADMIN VÀ LỄ TÂN: DANH SÁCH ĐẶT PHÒNG
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var ds = _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .OrderByDescending(d => d.NgayDen);

            return View(await ds.ToListAsync());
        }

        // ==========================================
        // LỊCH ĐẶT PHÒNG
        // ==========================================

        public async Task<IActionResult> LichDatPhong()
        {
            var danhSachPhong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .OrderBy(p => p.SoPhong)
                .ToListAsync();

            var danhSachDatPhong = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Where(d => d.TrangThai == "Đã đặt"
                         || d.TrangThai == "Đã check-in"
                         || d.TrangThai == "Chờ thanh toán cọc")
                .ToListAsync();

            var model = danhSachPhong.Select(p => new LichDatPhongViewModel
            {
                MaPhong = p.MaPhong,
                SoPhong = p.SoPhong,
                TenLoaiPhong = p.LoaiPhong != null
                    ? p.LoaiPhong.TenLoai : "",

                DanhSachDatPhong = danhSachDatPhong
                    .Where(d => d.MaPhong == p.MaPhong)
                    .Select(d => new LichDatPhongItem
                    {
                        MaDatPhong = d.MaDatPhong,
                        TenKhachHang = d.KhachHang != null
                            ? d.KhachHang.HoTen : "",
                        NgayDen = d.NgayDen,
                        NgayDi = d.NgayDi,
                        TrangThai = d.TrangThai
                    }).ToList()
            }).ToList();

            return View(model);
        }

        // ==========================================
        // CHI TIẾT ĐẶT PHÒNG
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var d = await _context.DatPhong
                .Include(x => x.KhachHang)
                .Include(x => x.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(m => m.MaDatPhong == id);

            if (d == null)
                return NotFound();

            var lichSuDoiPhong = await _context.LichSuDatPhongs
                .Where(x => x.MaDatPhong == id)
                .OrderBy(x => x.NgayBatDau)
                .ThenBy(x => x.ThoiGianThayDoi)
                .ToListAsync();

            ViewBag.LichSuDoiPhong = lichSuDoiPhong;

            return View(d);
        }

        // ==========================================
        // NẠP DANH SÁCH KHÁCH HÀNG VÀ PHÒNG
        // ==========================================

        private void NapDanhSach(
            int? maKhach = null,
            int? maPhong = null)
        {
            ViewData["MaKhach"] = new SelectList(
                _context.KhachHang.OrderBy(k => k.HoTen),
                "MaKhach",
                "HoTen",
                maKhach);

            var dsPhong = _context.Phong
                .Include(p => p.LoaiPhong)
                .Where(p => p.TrangThai != "Bảo trì")
                .OrderBy(p => p.SoPhong)
                .Select(p => new
                {
                    p.MaPhong,
                    Ten = p.SoPhong + " - " + p.LoaiPhong!.TenLoai
                })
                .ToList();

            ViewData["MaPhong"] = new SelectList(
                dsPhong,
                "MaPhong",
                "Ten",
                maPhong);
        }

        // ==========================================
        // KIỂM TRA NGÀY VÀ TRÙNG PHÒNG
        // ==========================================

        private async Task KiemTraDatPhong(DatPhong d)
        {
            DateTime ngayDen = d.NgayDen.Date;
            DateTime ngayDi = d.NgayDi.Date;
            DateTime homNay = DateTime.Today;

            if (ngayDen < homNay)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ngày nhận phòng không được ở trong quá khứ.");
            }

            if (ngayDi <= ngayDen)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ngày trả phòng phải sau ngày nhận phòng ít nhất 1 ngày.");

                return;
            }

            var phong = await _context.Phong
                .FirstOrDefaultAsync(p => p.MaPhong == d.MaPhong);

            if (phong == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Phòng được chọn không tồn tại.");

                return;
            }

            if (phong.TrangThai == "Bảo trì")
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Phòng đang bảo trì, vui lòng chọn phòng khác.");

                return;
            }

            bool biTrung = await _context.DatPhong.AnyAsync(x =>
                x.MaDatPhong != d.MaDatPhong &&
                x.MaPhong == d.MaPhong &&
                (x.TrangThai == "Đã đặt" ||
                 x.TrangThai == "Đã check-in" ||
                 x.TrangThai == "Chờ thanh toán cọc") &&
                x.NgayDen.Date < ngayDi &&
                x.NgayDi.Date > ngayDen);

            if (biTrung)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Phòng này đã có người đặt trong khoảng ngày bạn chọn. Hãy chọn phòng hoặc ngày khác.");
            }
        }

        // ==========================================
        // CẬP NHẬT TRẠNG THÁI PHÒNG
        // ==========================================

        private async Task CapNhatTrangThaiPhong(int maPhong)
        {
            var phong = await _context.Phong.FindAsync(maPhong);

            if (phong == null)
                return;

            if (phong.TrangThai == "Bảo trì")
                return;

            DateTime homNay = DateTime.Today;

            bool dangCoKhach = await _context.DatPhong.AnyAsync(d =>
                d.MaPhong == maPhong &&
                d.TrangThai == "Đã check-in");

            if (dangCoKhach)
            {
                phong.TrangThai = "Đang ở";
                return;
            }

            bool daDuocDat = await _context.DatPhong.AnyAsync(d =>
                d.MaPhong == maPhong &&
                d.TrangThai == "Đã đặt" &&
                d.NgayDen.Date <= homNay &&
                d.NgayDi.Date > homNay);

            phong.TrangThai = daDuocDat ? "Đã đặt" : "Trống";
        }

        // ==========================================
        // THÊM ĐẶT PHÒNG - GET
        // ==========================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Create(
            int? maPhong,
            DateTime? ngayDen,
            DateTime? ngayDi)
        {
            var d = new DatPhong
            {
                MaPhong = maPhong ?? 0,
                NgayDen = ngayDen?.Date ?? DateTime.Today,
                NgayDi = ngayDi?.Date ??
                    (ngayDen?.Date.AddDays(1) ?? DateTime.Today.AddDays(1))
            };

            NapDanhSach(d.MaKhach, maPhong);

            if (maPhong.HasValue)
            {
                var phong = await _context.Phong
                    .Include(p => p.LoaiPhong)
                    .FirstOrDefaultAsync(p => p.MaPhong == maPhong.Value);

                ViewBag.ThongTinPhong = phong;

                var cacDonDat = await _context.DatPhong
                    .Where(dp => dp.MaPhong == maPhong.Value
                        && (dp.TrangThai == "Đã đặt"
                            || dp.TrangThai == "Đã check-in"
                            || dp.TrangThai == "Chờ thanh toán cọc"))
                    .Select(dp => new
                    {
                        dp.NgayDen,
                        dp.NgayDi
                    })
                    .ToListAsync();

                ViewBag.LichDatPhong = cacDonDat.Select(dp => new
                {
                    NgayDen = dp.NgayDen.ToString("yyyy-MM-dd"),
                    NgayDi = dp.NgayDi.ToString("yyyy-MM-dd")
                }).ToList();
            }
            else
            {
                ViewBag.LichDatPhong = new List<object>();
            }

            return View(d);
        }

        // ==========================================
        // LƯU ĐẶT PHÒNG - POST
        // ==========================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaDatPhong,MaKhach,MaPhong,NgayDen,NgayDi,TrangThai,GhiChu")]
            DatPhong datPhong)
        {
            if (User.Identity == null ||
                !User.Identity.IsAuthenticated)
                return Challenge();

            bool laKhachHang = User.IsInRole("KhachHang");
            bool laNhanVien = User.IsInRole("Admin")
                           || User.IsInRole("LeTan");

            if (!laKhachHang && !laNhanVien)
                return Forbid();

            if (laKhachHang)
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Forbid();

                var khachHang = await _context.KhachHang
                    .FirstOrDefaultAsync(k =>
                        k.IdentityUserId == userId);

                if (khachHang == null)
                    return NotFound();

                if (string.IsNullOrWhiteSpace(khachHang.HoTen) ||
                    string.IsNullOrWhiteSpace(khachHang.CCCD) ||
                    string.IsNullOrWhiteSpace(khachHang.SoDienThoai))
                {
                    TempData["Loi"] =
                        "Vui lòng cập nhật đầy đủ họ tên, CCCD/Hộ chiếu và số điện thoại trước khi đặt phòng.";

                    return RedirectToAction("Index", "HoSo");
                }

                datPhong.MaKhach = khachHang.MaKhach;
            }

            datPhong.TrangThai = laKhachHang
                ? "Chờ thanh toán cọc"
                : "Đã đặt";

            ModelState.Remove(nameof(DatPhong.TrangThai));

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            await KiemTraDatPhong(datPhong);

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();

                NapDanhSach(datPhong.MaKhach, datPhong.MaPhong);

                var phongLoi = await _context.Phong
                    .Include(p => p.LoaiPhong)
                    .FirstOrDefaultAsync(p => p.MaPhong == datPhong.MaPhong);

                ViewBag.ThongTinPhong = phongLoi;

                return View(datPhong);
            }

            var thongTinPhong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == datPhong.MaPhong);

            if (thongTinPhong?.LoaiPhong == null)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không tìm thấy thông tin loại phòng.");

                NapDanhSach(datPhong.MaKhach, datPhong.MaPhong);
                ViewBag.ThongTinPhong = thongTinPhong;

                return View(datPhong);
            }

            _context.DatPhong.Add(datPhong);

            await _context.SaveChangesAsync();

            if (laKhachHang)
            {
                int soDem = (datPhong.NgayDi.Date -
                             datPhong.NgayDen.Date).Days;

                decimal tongTien =
                    thongTinPhong.LoaiPhong.GiaMotDem * soDem;

                decimal tienCoc = Math.Round(
                    tongTien * 0.30m,
                    0,
                    MidpointRounding.AwayFromZero);

                var thanhToan = new ThanhToan
                {
                    MaDatPhong = datPhong.MaDatPhong,
                    SoTien = tienCoc,
                    LoaiThanhToan = "TienCoc",
                    PhuongThuc = "ChuyenKhoanMoPhong",
                    TrangThai = "ChoThanhToan",
                    NgayTao = DateTime.Now
                };

                _context.ThanhToan.Add(thanhToan);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    "Đơn đặt phòng đã được tạo. Vui lòng thanh toán tiền cọc 30% để xác nhận.";

                return RedirectToAction(
                    nameof(ThanhToanCoc),
                    new { id = datPhong.MaDatPhong });
            }

            await CapNhatTrangThaiPhong(datPhong.MaPhong);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["ThongBao"] = "Đặt phòng thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // KHÁCH HÀNG: HIỂN THỊ TRANG THANH TOÁN CỌC
        // ==========================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ThanhToanCoc(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            var datPhong = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(d =>
                    d.MaDatPhong == id &&
                    d.MaKhach == khachHang.MaKhach);

            if (datPhong == null)
                return NotFound();

            var thanhToan = await _context.ThanhToan
                .FirstOrDefaultAsync(t =>
                    t.MaDatPhong == id &&
                    t.LoaiThanhToan == "TienCoc");

            if (thanhToan == null)
                return NotFound();

            int soDem = (datPhong.NgayDi.Date -
                         datPhong.NgayDen.Date).Days;

            decimal giaMotDem =
                datPhong.Phong?.LoaiPhong?.GiaMotDem ?? 0;

            decimal tongTien = giaMotDem * soDem;

            var model = new ThanhToanCocViewModel
            {
                MaDatPhong = datPhong.MaDatPhong,
                MaThanhToan = thanhToan.MaThanhToan,
                HoTenKhachHang = khachHang.HoTen,
                SoPhong = Convert.ToString(datPhong.Phong?.SoPhong) ?? "",
                TenLoaiPhong = datPhong.Phong?.LoaiPhong?.TenLoai ?? "",
                NgayDen = datPhong.NgayDen,
                NgayDi = datPhong.NgayDi,
                SoDem = soDem,
                GiaMotDem = giaMotDem,
                TongTien = tongTien,
                TienCoc = thanhToan.SoTien,
                PhanTramCoc = 30,
                TrangThaiThanhToan = thanhToan.TrangThai
            };

            return View(model);
        }

        // ==========================================
        // KHÁCH HÀNG: XÁC NHẬN THANH TOÁN MÔ PHỎNG
        // ==========================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToanMoPhong(int id)
        {
            if (User.Identity?.IsAuthenticated != true)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(d =>
                    d.MaDatPhong == id &&
                    d.MaKhach == khachHang.MaKhach);

            if (datPhong == null)
                return NotFound();

            var thanhToan = await _context.ThanhToan
                .FirstOrDefaultAsync(t =>
                    t.MaDatPhong == id &&
                    t.LoaiThanhToan == "TienCoc");

            if (thanhToan == null)
                return NotFound();

            if (thanhToan.TrangThai == "ThanhCong" &&
                datPhong.TrangThai == "Đã đặt")
            {
                await transaction.RollbackAsync();

                TempData["ThongBao"] =
                    "Khoản tiền cọc này đã được xác nhận trước đó.";

                return RedirectToAction(nameof(PhongCuaToi));
            }

            if (datPhong.TrangThai != "Chờ thanh toán cọc" ||
                thanhToan.TrangThai != "ChoThanhToan")
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Đơn đặt phòng không ở trạng thái chờ thanh toán.";

                return RedirectToAction(nameof(PhongCuaToi));
            }

            bool biTrung = await _context.DatPhong.AnyAsync(x =>
                x.MaDatPhong != datPhong.MaDatPhong &&
                x.MaPhong == datPhong.MaPhong &&
                (x.TrangThai == "Đã đặt" ||
                 x.TrangThai == "Đã check-in") &&
                x.NgayDen.Date < datPhong.NgayDi.Date &&
                x.NgayDi.Date > datPhong.NgayDen.Date);

            if (biTrung)
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Phòng đã phát sinh đơn đặt khác. Vui lòng liên hệ khách sạn.";

                return RedirectToAction(nameof(PhongCuaToi));
            }

            thanhToan.TrangThai = "ThanhCong";
            thanhToan.NgayThanhToan = DateTime.Now;
            thanhToan.MaGiaoDich =
                "MOPHONG-" + Guid.NewGuid().ToString("N")[..12].ToUpper();

            datPhong.TrangThai = "Đã đặt";

            await _context.SaveChangesAsync();

            await CapNhatTrangThaiPhong(datPhong.MaPhong);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            try
            {
                await _thongBaoService.TaoThongBaoAsync(
                    datPhong.MaKhach,
                    datPhong.MaDatPhong,
                    "DatPhongThanhCong",
                    "Đặt phòng thành công",
                    $"Thanh toán cọc mô phỏng thành công. " +
                    $"Phòng: {datPhong.Phong?.SoPhong}. " +
                    $"Ngày nhận: {datPhong.NgayDen:dd/MM/yyyy}. " +
                    $"Ngày trả: {datPhong.NgayDi:dd/MM/yyyy}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Lỗi tạo thông báo đặt phòng: " + ex.Message);
            }

            try
            {
                var emailNhan = User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrWhiteSpace(emailNhan))
                {
                    var tenKhach = System.Net.WebUtility.HtmlEncode(
                        khachHang.HoTen ?? "Quý khách");

                    var soPhong = System.Net.WebUtility.HtmlEncode(
                        Convert.ToString(datPhong.Phong?.SoPhong) ?? "");

                    var noiDung = $@"
                        <h2>HOTEL QUAN LY KHACH SAN</h2>
                        <p>Xin chào <b>{tenKhach}</b>,</p>
                        <p>Bạn đã hoàn tất bước xác nhận thanh toán cọc mô phỏng.</p>
                        <p><b>Số phòng:</b> {soPhong}</p>
                        <p><b>Ngày nhận phòng:</b> {datPhong.NgayDen:dd/MM/yyyy}</p>
                        <p><b>Ngày trả phòng:</b> {datPhong.NgayDi:dd/MM/yyyy}</p>
                        <p><b>Tiền cọc mô phỏng:</b> {thanhToan.SoTien:N0} VNĐ</p>
                        <p><b>Trạng thái đặt phòng:</b> Đã đặt</p>
                        <p><i>Lưu ý: Đây là thanh toán mô phỏng phục vụ mục đích trình diễn, không phải giao dịch ngân hàng thực tế.</i></p>
                        <p>Cảm ơn bạn đã lựa chọn khách sạn!</p>";

                    await _emailService.GuiEmailAsync(
                        emailNhan,
                        "Xác nhận đặt phòng thành công",
                        noiDung);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Lỗi gửi email xác nhận đặt phòng: " + ex.Message);
            }

            TempData["ThongBao"] =
                "Xác nhận thanh toán mô phỏng thành công. Đặt phòng của bạn đã được xác nhận.";

            return RedirectToAction(nameof(PhongCuaToi));
        }

        // ==========================================
        // HỖ TRỢ NẠP DANH SÁCH PHÒNG CÓ THỂ ĐỔI
        // ==========================================

        private async Task NapDanhSachPhongDoi(
            int maDatPhong,
            int maPhongDangDung,
            DateTime ngayDen,
            DateTime ngayDi,
            int? maPhongMoi = null)
        {
            var danhSachPhong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .Where(p =>
                    p.MaPhong != maPhongDangDung &&
                    p.TrangThai != "Bảo trì" &&
                    p.TrangThai != "Đang dọn" &&
                    p.TinhTrangVeSinh == "Đã kiểm tra")
                .OrderBy(p => p.SoPhong)
                .ToListAsync();

            var phongCoTheChon = new List<object>();

            foreach (var phong in danhSachPhong)
            {
                bool biTrung = await _context.DatPhong.AnyAsync(d =>
                    d.MaDatPhong != maDatPhong &&
                    d.MaPhong == phong.MaPhong &&
                    (d.TrangThai == "Đã đặt" ||
                     d.TrangThai == "Đã check-in" ||
                     d.TrangThai == "Chờ thanh toán cọc") &&
                    d.NgayDen.Date < ngayDi.Date &&
                    d.NgayDi.Date > ngayDen.Date);

                if (!biTrung)
                {
                    phongCoTheChon.Add(new
                    {
                        MaPhong = phong.MaPhong,
                        TenPhong = phong.SoPhong + " - " +
                            (phong.LoaiPhong?.TenLoai ?? "") +
                            " - " +
                            (phong.LoaiPhong?.GiaMotDem.ToString("N0") ?? "0") +
                            " đ/đêm"
                    });
                }
            }

            ViewBag.DanhSachPhong = new SelectList(
                phongCoTheChon,
                "MaPhong",
                "TenPhong",
                maPhongMoi);
        }

        // ==========================================
        // ĐỔI PHÒNG - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> DoiPhong(int? id)
        {
            if (id == null)
                return NotFound();

            var datPhong = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(d => d.MaDatPhong == id);

            if (datPhong == null)
                return NotFound();

            if (datPhong.TrangThai != "Đã đặt" &&
                datPhong.TrangThai != "Đã check-in")
            {
                TempData["Loi"] =
                    "Chỉ được đổi phòng đối với đơn đã đặt hoặc đang có khách lưu trú.";

                return RedirectToAction(nameof(Index));
            }

            if (datPhong.TrangThai == "Đã check-in" &&
                datPhong.NgayDi.Date <= DateTime.Today)
            {
                TempData["Loi"] =
                    "Không thể đổi phòng vì ngày trả phòng đã đến.";

                return RedirectToAction(nameof(Index));
            }

            var model = new DoiPhongViewModel
            {
                MaDatPhong = datPhong.MaDatPhong,
                SoPhongCu = datPhong.Phong?.SoPhong ?? "",
                TenKhachHang = datPhong.KhachHang?.HoTen ?? "",
                NgayDen = datPhong.NgayDen,
                NgayDi = datPhong.NgayDi,
                TrangThaiDatPhong = datPhong.TrangThai
            };

            await NapDanhSachPhongDoi(
                datPhong.MaDatPhong,
                datPhong.MaPhong,
                datPhong.NgayDen,
                datPhong.NgayDi);

            return View(model);
        }

        // ==========================================
        // ĐỔI PHÒNG - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiPhong(
            DoiPhongViewModel model)
        {
            var datPhong = await _context.DatPhong
                .Include(d => d.KhachHang)
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(d =>
                    d.MaDatPhong == model.MaDatPhong);

            if (datPhong == null)
                return NotFound();

            if (datPhong.TrangThai != "Đã đặt" &&
                datPhong.TrangThai != "Đã check-in")
            {
                TempData["Loi"] =
                    "Đơn đặt phòng không còn đủ điều kiện đổi phòng.";

                return RedirectToAction(nameof(Index));
            }

            if (datPhong.TrangThai == "Đã check-in" &&
                datPhong.NgayDi.Date <= DateTime.Today)
            {
                TempData["Loi"] =
                    "Không thể đổi phòng vì ngày trả phòng đã đến.";

                return RedirectToAction(nameof(Index));
            }

            if (datPhong.MaPhong == model.MaPhongMoi)
            {
                ModelState.AddModelError(
                    nameof(model.MaPhongMoi),
                    "Phòng mới phải khác phòng hiện tại.");
            }

            if (!ModelState.IsValid)
            {
                model.SoPhongCu = datPhong.Phong?.SoPhong ?? "";
                model.TenKhachHang = datPhong.KhachHang?.HoTen ?? "";
                model.NgayDen = datPhong.NgayDen;
                model.NgayDi = datPhong.NgayDi;
                model.TrangThaiDatPhong = datPhong.TrangThai;

                await NapDanhSachPhongDoi(
                    datPhong.MaDatPhong,
                    datPhong.MaPhong,
                    datPhong.NgayDen,
                    datPhong.NgayDi,
                    model.MaPhongMoi);

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var goc = await _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(d =>
                    d.MaDatPhong == model.MaDatPhong);

            if (goc == null || goc.Phong == null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (goc.TrangThai != "Đã đặt" &&
                goc.TrangThai != "Đã check-in")
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Trạng thái đặt phòng đã thay đổi, không thể đổi phòng.";

                return RedirectToAction(nameof(Index));
            }

            var phongMoi = await _context.Phong
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p =>
                    p.MaPhong == model.MaPhongMoi);

            if (phongMoi == null ||
                phongMoi.TrangThai == "Bảo trì" ||
                phongMoi.TrangThai == "Đang dọn" ||
                phongMoi.TinhTrangVeSinh != "Đã kiểm tra" ||
                phongMoi.LoaiPhong == null)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    nameof(model.MaPhongMoi),
                    "Phòng mới không tồn tại, đang bảo trì hoặc chưa được kiểm tra vệ sinh.");

                model.SoPhongCu = goc.Phong.SoPhong;
                model.TenKhachHang = goc.KhachHang?.HoTen ?? "";
                model.NgayDen = goc.NgayDen;
                model.NgayDi = goc.NgayDi;
                model.TrangThaiDatPhong = goc.TrangThai;

                await NapDanhSachPhongDoi(
                    goc.MaDatPhong,
                    goc.MaPhong,
                    goc.NgayDen,
                    goc.NgayDi,
                    model.MaPhongMoi);

                return View(model);
            }

            bool biTrung = await _context.DatPhong.AnyAsync(d =>
                d.MaDatPhong != goc.MaDatPhong &&
                d.MaPhong == phongMoi.MaPhong &&
                (d.TrangThai == "Đã đặt" ||
                 d.TrangThai == "Đã check-in" ||
                 d.TrangThai == "Chờ thanh toán cọc") &&
                d.NgayDen.Date < goc.NgayDi.Date &&
                d.NgayDi.Date > goc.NgayDen.Date);

            if (biTrung)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    nameof(model.MaPhongMoi),
                    "Phòng mới đã có khách đặt trong khoảng thời gian này.");

                model.SoPhongCu = goc.Phong.SoPhong;
                model.TenKhachHang = goc.KhachHang?.HoTen ?? "";
                model.NgayDen = goc.NgayDen;
                model.NgayDi = goc.NgayDi;
                model.TrangThaiDatPhong = goc.TrangThai;

                await NapDanhSachPhongDoi(
                    goc.MaDatPhong,
                    goc.MaPhong,
                    goc.NgayDen,
                    goc.NgayDi,
                    model.MaPhongMoi);

                return View(model);
            }

            int maPhongCu = goc.MaPhong;
            bool dangCoKhach = goc.TrangThai == "Đã check-in";

            decimal giaPhongCu =
                goc.Phong.LoaiPhong?.GiaMotDem ?? 0;

            decimal giaPhongMoi =
                phongMoi.LoaiPhong.GiaMotDem;

            DateTime ngayBatDau = dangCoKhach
                ? DateTime.Today
                : goc.NgayDen.Date;

            var lichSu = new LichSuDatPhong
            {
                MaDatPhong = goc.MaDatPhong,
                MaPhongCu = maPhongCu,
                SoPhongCu = goc.Phong.SoPhong,
                MaPhongMoi = phongMoi.MaPhong,
                SoPhongMoi = phongMoi.SoPhong,
                NgayBatDau = ngayBatDau,
                GiaPhongCu = giaPhongCu,
                GiaPhongMoi = giaPhongMoi,
                NguoiThucHienId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier),
                TenNguoiThucHien = User.Identity?.Name ?? "Nhân viên",
                LyDo = model.LyDo?.Trim(),
                ThoiGianThayDoi = DateTime.Now
            };

            _context.LichSuDatPhongs.Add(lichSu);

            goc.MaPhong = phongMoi.MaPhong;

            if (dangCoKhach)
            {
                goc.Phong.TinhTrangVeSinh = "Chưa dọn";
                goc.Phong.TrangThai = "Đang dọn";

                phongMoi.TrangThai = "Đang ở";
            }

            await _context.SaveChangesAsync();

            if (!dangCoKhach)
            {
                await CapNhatTrangThaiPhong(maPhongCu);
                await CapNhatTrangThaiPhong(phongMoi.MaPhong);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["ThongBao"] =
                $"Đổi từ phòng {lichSu.SoPhongCu} sang phòng {lichSu.SoPhongMoi} thành công!";

            return RedirectToAction(
                nameof(Details),
                new { id = goc.MaDatPhong });
        }

        // ==========================================
        // SỬA ĐẶT PHÒNG - GET
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var d = await _context.DatPhong.FindAsync(id);

            if (d == null)
                return NotFound();

            if (d.TrangThai != "Đã đặt")
            {
                TempData["Loi"] =
                    "Chỉ sửa được đơn có trạng thái 'Đã đặt'.";

                return RedirectToAction(nameof(Index));
            }

            NapDanhSach(d.MaKhach, d.MaPhong);

            return View(d);
        }

        // ==========================================
        // SỬA ĐẶT PHÒNG - POST
        // ĐÃ BỔ SUNG GIAO DỊCH SERIALIZABLE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaDatPhong,MaKhach,MaPhong,NgayDen,NgayDi,GhiChu")]
            DatPhong datPhong)
        {
            if (id != datPhong.MaDatPhong)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var goc = await _context.DatPhong
                .FirstOrDefaultAsync(d => d.MaDatPhong == id);

            if (goc == null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (goc.TrangThai != "Đã đặt")
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Chỉ sửa được đơn có trạng thái 'Đã đặt'.";

                return RedirectToAction(nameof(Index));
            }

            datPhong.TrangThai = goc.TrangThai;

            await KiemTraDatPhong(datPhong);

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();

                NapDanhSach(datPhong.MaKhach, datPhong.MaPhong);

                return View(datPhong);
            }

            int maPhongCu = goc.MaPhong;

            goc.MaKhach = datPhong.MaKhach;
            goc.MaPhong = datPhong.MaPhong;
            goc.NgayDen = datPhong.NgayDen.Date;
            goc.NgayDi = datPhong.NgayDi.Date;
            goc.GhiChu = datPhong.GhiChu;

            await _context.SaveChangesAsync();

            await CapNhatTrangThaiPhong(maPhongCu);
            await CapNhatTrangThaiPhong(goc.MaPhong);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["ThongBao"] =
                "Cập nhật đặt phòng thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // KHÁCH HÀNG: HỦY ĐẶT PHÒNG CỦA TÔI
        // ==========================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyCuaToi(int id)
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

            var datPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .FirstOrDefaultAsync(d =>
                    d.MaDatPhong == id &&
                    d.MaKhach == khachHang.MaKhach);

            if (datPhong == null)
                return NotFound();

            if (datPhong.TrangThai != "Đã đặt" &&
                datPhong.TrangThai != "Chờ thanh toán cọc")
            {
                TempData["Loi"] =
                    "Không thể hủy đơn đặt phòng ở trạng thái hiện tại.";

                return RedirectToAction(nameof(PhongCuaToi));
            }

            int maPhong = datPhong.MaPhong;

            datPhong.TrangThai = "Đã hủy";

            var thanhToan = await _context.ThanhToan
                .FirstOrDefaultAsync(t =>
                    t.MaDatPhong == id &&
                    t.LoaiThanhToan == "TienCoc");

            if (thanhToan != null &&
                thanhToan.TrangThai == "ChoThanhToan")
            {
                thanhToan.TrangThai = "ThatBai";
            }

            await _context.SaveChangesAsync();

            await CapNhatTrangThaiPhong(maPhong);
            await _context.SaveChangesAsync();

            try
            {
                await _thongBaoService.TaoThongBaoAsync(
                    datPhong.MaKhach,
                    datPhong.MaDatPhong,
                    "HuyDatPhong",
                    "Hủy đặt phòng thành công",
                    $"Bạn đã hủy đặt phòng {datPhong.Phong?.SoPhong}. " +
                    $"Ngày nhận phòng: {datPhong.NgayDen:dd/MM/yyyy}. " +
                    $"Ngày trả phòng: {datPhong.NgayDi:dd/MM/yyyy}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Lỗi tạo thông báo hủy đặt phòng: " + ex.Message);
            }

            try
            {
                var emailNhan = User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrWhiteSpace(emailNhan))
                {
                    var tenKhach = System.Net.WebUtility.HtmlEncode(
                        khachHang.HoTen ?? "Quý khách");

                    var soPhong = System.Net.WebUtility.HtmlEncode(
                        Convert.ToString(datPhong.Phong?.SoPhong) ?? "");

                    var noiDung = $@"
                        <h2>HOTEL QUAN LY KHACH SAN</h2>
                        <p>Xin chào <b>{tenKhach}</b>,</p>
                        <p>Yêu cầu hủy đặt phòng của bạn đã được thực hiện thành công.</p>
                        <p><b>Số phòng:</b> {soPhong}</p>
                        <p><b>Ngày nhận phòng:</b> {datPhong.NgayDen:dd/MM/yyyy}</p>
                        <p><b>Ngày trả phòng:</b> {datPhong.NgayDi:dd/MM/yyyy}</p>
                        <p><b>Trạng thái:</b> Đã hủy</p>
                        <p>Cảm ơn bạn đã quan tâm đến khách sạn!</p>";

                    await _emailService.GuiEmailAsync(
                        emailNhan,
                        "Thông báo hủy đặt phòng",
                        noiDung);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Lỗi gửi email thông báo hủy phòng: " + ex.Message);
            }

            TempData["ThongBao"] =
                "Bạn đã hủy đặt phòng thành công.";

            return RedirectToAction(nameof(PhongCuaToi));
        }

        // ==========================================
        // ADMIN VÀ LỄ TÂN: HỦY ĐẶT PHÒNG
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var d = await _context.DatPhong.FindAsync(id);

            if (d == null)
                return NotFound();

            if (d.TrangThai != "Đã đặt")
            {
                TempData["Loi"] =
                    "Chỉ hủy được đơn có trạng thái 'Đã đặt'.";

                return RedirectToAction(nameof(Index));
            }

            int maPhong = d.MaPhong;

            d.TrangThai = "Đã hủy";

            await _context.SaveChangesAsync();

            await CapNhatTrangThaiPhong(maPhong);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Đã hủy đơn đặt phòng và cập nhật trạng thái phòng.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ADMIN VÀ LỄ TÂN: CHECK-IN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var d = await _context.DatPhong
                .Include(x => x.Phong)
                .FirstOrDefaultAsync(x => x.MaDatPhong == id);

            if (d == null)
                return NotFound();

            if (d.TrangThai != "Đã đặt")
            {
                TempData["Loi"] =
                    "Chỉ check-in được đơn có trạng thái 'Đã đặt'.";

                return RedirectToAction(nameof(Index));
            }

            if (d.NgayDen.Date > DateTime.Today ||
                d.NgayDi.Date <= DateTime.Today)
            {
                TempData["Loi"] =
                    "Chỉ được check-in trong thời gian đặt phòng.";

                return RedirectToAction(nameof(Index));
            }

            if (d.Phong == null ||
                d.Phong.TrangThai == "Bảo trì")
            {
                TempData["Loi"] =
                    "Phòng đang bảo trì, không thể nhận khách.";

                return RedirectToAction(nameof(Index));
            }

            if (d.Phong.TinhTrangVeSinh != "Đã kiểm tra")
            {
                TempData["Loi"] =
                    $"Phòng {d.Phong.SoPhong} chưa được vệ sinh và kiểm tra. " +
                    "Vui lòng hoàn tất quy trình vệ sinh trước khi check-in.";

                return RedirectToAction(nameof(Index));
            }

            d.TrangThai = "Đã check-in";
            d.Phong.TrangThai = "Đang ở";

            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Check-in thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // ADMIN VÀ LỄ TÂN: CHECK-OUT VÀ LẬP HÓA ĐƠN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var d = await _context.DatPhong
                .Include(x => x.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .FirstOrDefaultAsync(x => x.MaDatPhong == id);

            if (d == null)
                return NotFound();

            if (d.TrangThai != "Đã check-in")
            {
                TempData["Loi"] =
                    "Chỉ check-out được đơn có trạng thái 'Đã check-in'.";

                return RedirectToAction(nameof(Index));
            }

            var lichSuDoiPhong = await _context.LichSuDatPhongs
                .Where(x => x.MaDatPhong == id)
                .OrderBy(x => x.NgayBatDau)
                .ThenBy(x => x.ThoiGianThayDoi)
                .ToListAsync();

            DateTime ngayBatDauTinhTien = d.NgayDen.Date;

            decimal giaDangApDung = lichSuDoiPhong.Count > 0
                ? lichSuDoiPhong.First().GiaPhongCu
                : d.Phong?.LoaiPhong?.GiaMotDem ?? 0;

            decimal tongTien = 0;

            foreach (var ls in lichSuDoiPhong)
            {
                DateTime ngayChuyen = ls.NgayBatDau.Date;

                if (ngayChuyen < d.NgayDen.Date)
                    ngayChuyen = d.NgayDen.Date;

                if (ngayChuyen > d.NgayDi.Date)
                    ngayChuyen = d.NgayDi.Date;

                int soDemPhongCu = Math.Max(
                    0,
                    (ngayChuyen - ngayBatDauTinhTien).Days);

                tongTien += soDemPhongCu * giaDangApDung;

                ngayBatDauTinhTien = ngayChuyen;
                giaDangApDung = ls.GiaPhongMoi;
            }

            int soDemPhongCuoi = Math.Max(
                0,
                (d.NgayDi.Date - ngayBatDauTinhTien).Days);

            tongTien += soDemPhongCuoi * giaDangApDung;

            var hoaDon = await _context.HoaDon
                .FirstOrDefaultAsync(h => h.MaDatPhong == id);

            if (hoaDon == null)
            {
                hoaDon = new HoaDon
                {
                    MaDatPhong = id,
                    TongTien = tongTien,
                    NgayLap = DateTime.Now,
                    DaThanhToan = false
                };

                _context.HoaDon.Add(hoaDon);
            }
            else
            {
                hoaDon.TongTien = tongTien;
            }

            decimal tienCoc = await _context.ThanhToan
                .Where(t =>
                    t.MaDatPhong == id &&
                    t.LoaiThanhToan == "TienCoc" &&
                    t.TrangThai == "ThanhCong")
                .SumAsync(t => (decimal?)t.SoTien) ?? 0;

            decimal tienConLai = Math.Max(0, tongTien - tienCoc);

            var thanhToanConLai = await _context.ThanhToan
                .FirstOrDefaultAsync(t =>
                    t.MaDatPhong == id &&
                    t.LoaiThanhToan == "ThanhToanConLai");

            if (tienConLai > 0)
            {
                if (thanhToanConLai == null)
                {
                    _context.ThanhToan.Add(
                        new ThanhToan
                        {
                            MaDatPhong = id,
                            SoTien = tienConLai,
                            LoaiThanhToan = "ThanhToanConLai",
                            PhuongThuc = "ChuyenKhoanMoPhong",
                            TrangThai = "ChoThanhToan",
                            NgayTao = DateTime.Now
                        });
                }
                else if (thanhToanConLai.TrangThai == "ChoThanhToan")
                {
                    thanhToanConLai.SoTien = tienConLai;
                }
            }

            d.TrangThai = "Đã check-out";

            if (d.Phong != null)
            {
                d.Phong.TinhTrangVeSinh = "Chưa dọn";
            }

            await _context.SaveChangesAsync();

            await CapNhatTrangThaiPhong(d.MaPhong);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Details",
                "HoaDons",
                new { id = hoaDon.MaHoaDon });
        }

        // ==========================================
        // ADMIN: XÓA ĐẶT PHÒNG
        // ==========================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var d = await _context.DatPhong
                .Include(x => x.KhachHang)
                .Include(x => x.Phong)
                .FirstOrDefaultAsync(m => m.MaDatPhong == id);

            if (d == null)
                return NotFound();

            return View(d);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var d = await _context.DatPhong.FindAsync(id);

            if (d == null)
                return RedirectToAction(nameof(Index));

            if (d.TrangThai == "Đã check-in")
            {
                TempData["Loi"] =
                    "Không thể xóa đơn đang có khách ở. Hãy check-out trước.";

                return RedirectToAction(nameof(Index));
            }

            if (await _context.HoaDon.AnyAsync(h =>
                h.MaDatPhong == id))
            {
                TempData["Loi"] =
                    "Không thể xóa đơn đã có hóa đơn.";

                return RedirectToAction(nameof(Index));
            }

            if (await _context.ThanhToan.AnyAsync(t =>
                t.MaDatPhong == id))
            {
                TempData["Loi"] =
                    "Đơn đã có thông tin thanh toán. Hãy hủy đơn thay vì xóa.";

                return RedirectToAction(nameof(Index));
            }

            int maPhong = d.MaPhong;

            _context.DatPhong.Remove(d);

            await _context.SaveChangesAsync();

            await CapNhatTrangThaiPhong(maPhong);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Đã xóa đơn đặt phòng và cập nhật trạng thái phòng.";

            return RedirectToAction(nameof(Index));
        }
    }
}
