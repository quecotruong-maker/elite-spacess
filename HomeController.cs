
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("LeTan"))
            {
                var homNay = DateTime.Today;
                var ngayMai = homNay.AddDays(1);
                var ngayKetThuc = homNay.AddDays(7);

                var tongPhong = await _context.Phong.CountAsync();

                var phongTrong = await _context.Phong
                    .CountAsync(p => p.TrangThai == "Trống");

                var phongDangO = await _context.Phong
                    .CountAsync(p => p.TrangThai == "Đang ở");

                var phongDangDon = await _context.Phong
                    .CountAsync(p => p.TrangThai == "Đang dọn");

                var phongBaoTri = await _context.Phong
                    .CountAsync(p => p.TrangThai == "Bảo trì");

                var datPhongHomNay = await _context.DatPhong
                    .CountAsync(d =>
                        d.TrangThai == "Đã đặt" &&
                        d.NgayDen >= homNay &&
                        d.NgayDen < ngayMai);

                var khachDangLuuTru = await _context.DatPhong
                    .CountAsync(d => d.TrangThai == "Đã check-in");

                var sapDen = await _context.DatPhong
                    .CountAsync(d =>
                        d.TrangThai == "Đã đặt" &&
                        d.NgayDen >= homNay &&
                        d.NgayDen < ngayMai);

                var sapDi = await _context.DatPhong
                    .CountAsync(d =>
                        d.TrangThai == "Đã check-in" &&
                        d.NgayDi >= homNay &&
                        d.NgayDi < ngayMai);

                var tyLeLapDay = tongPhong > 0
                    ? Math.Round((double)phongDangO / tongPhong * 100, 1)
                    : 0;

                var datPhongTuan = await _context.DatPhong
                    .Include(d => d.KhachHang)
                    .Include(d => d.Phong)
                    .Where(d =>
                        (d.TrangThai == "Đã đặt" ||
                         d.TrangThai == "Đã check-in") &&
                        d.NgayDi >= homNay &&
                        d.NgayDen < ngayKetThuc)
                    .OrderBy(d => d.NgayDen)
                    .Take(20)
                    .ToListAsync();

                ViewBag.TongPhong = tongPhong;
                ViewBag.PhongTrong = phongTrong;
                ViewBag.PhongDangO = phongDangO;
                ViewBag.PhongDangDon = phongDangDon;
                ViewBag.PhongBaoTri = phongBaoTri;

                ViewBag.DatPhongHomNay = datPhongHomNay;
                ViewBag.KhachDangLuuTru = khachDangLuuTru;
                ViewBag.TyLeLapDay = tyLeLapDay;

                ViewBag.SapDen = sapDen;
                ViewBag.SapDi = sapDi;

                ViewBag.DatPhongTuan = datPhongTuan;

                // LỊCH ĐẶT PHÒNG THÁNG HIỆN TẠI
                var ngayDauThang = new DateTime(
                    homNay.Year,
                    homNay.Month,
                    1
                );

                var ngayCuoiThang = ngayDauThang.AddMonths(1);

                var danhSachPhongLich = await _context.Phong
                    .Include(p => p.LoaiPhong)
                    .OrderBy(p => p.SoPhong)
                    .ToListAsync();

                var danhSachDatPhongLich = await _context.DatPhong
                    .Include(d => d.KhachHang)
                    .Where(d =>
                        (d.TrangThai == "Đã đặt" ||
                         d.TrangThai == "Đã check-in") &&
                        d.NgayDi >= ngayDauThang &&
                        d.NgayDen < ngayCuoiThang)
                    .OrderBy(d => d.NgayDen)
                    .ToListAsync();

                var lichDatPhong = danhSachPhongLich.Select(p =>
                {
                    var datPhongCuaPhong = danhSachDatPhongLich
                        .Where(d => d.MaPhong == p.MaPhong)
                        .ToList();

                    var dangCoKhach = datPhongCuaPhong.Any(d =>
                        d.TrangThai == "Đã check-in" &&
                        d.NgayDen.Date <= homNay &&
                        d.NgayDi.Date > homNay);

                    var coDatPhong = datPhongCuaPhong.Any(d =>
                        d.TrangThai == "Đã đặt" &&
                        d.NgayDi.Date > homNay);

                    string trangThaiHienThi;

                    if (p.TrangThai == "Bảo trì")
                    {
                        trangThaiHienThi = "Bảo trì";
                    }
                    else if (p.TrangThai == "Đang dọn")
                    {
                        trangThaiHienThi = "Đang dọn";
                    }
                    else if (dangCoKhach)
                    {
                        trangThaiHienThi = "Đang ở";
                    }
                    else if (coDatPhong)
                    {
                        trangThaiHienThi = "Đã đặt";
                    }
                    else
                    {
                        trangThaiHienThi = "Trống";
                    }

                    return new LichDatPhongViewModel
                    {
                        MaPhong = p.MaPhong,
                        SoPhong = p.SoPhong,

                        TenLoaiPhong = p.LoaiPhong != null
                            ? p.LoaiPhong.TenLoai
                            : "",

                        TrangThaiPhong = trangThaiHienThi,

                        DanhSachDatPhong = datPhongCuaPhong
                            .Select(d => new LichDatPhongItem
                            {
                                MaDatPhong = d.MaDatPhong,

                                TenKhachHang = d.KhachHang != null
                                    ? d.KhachHang.HoTen
                                    : "Khách hàng",

                                NgayDen = d.NgayDen,
                                NgayDi = d.NgayDi,
                                TrangThai = d.TrangThai
                            })
                            .ToList()
                    };
                })
                .ToList();

                ViewBag.LichDatPhong = lichDatPhong;

                if (User.IsInRole("Admin"))
                    return View();

                return View("LeTan");
            }

            // KHÁCH HÀNG VÀ KHÁCH CHƯA ĐĂNG NHẬP
            var danhSachPhong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .Where(p => p.TrangThai != "Bảo trì")
                .OrderBy(p => p.SoPhong)
                .ToListAsync();

            return View("DanhSachPhong", danhSachPhong);
        }

        // ==========================================
        // BÁO CÁO VÀ THỐNG KÊ - CHỈ ADMIN
        // ==========================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BaoCao(
            DateTime? tuNgay,
            DateTime? denNgay)
        {
            var homNay = DateTime.Today;

            var batDau = tuNgay?.Date
                ?? new DateTime(homNay.Year, homNay.Month, 1);

            var ketThuc = denNgay?.Date
                ?? batDau.AddMonths(1).AddDays(-1);

            if (ketThuc < batDau)
            {
                TempData["Loi"] =
                    "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.";

                return RedirectToAction(nameof(BaoCao));
            }

            var ngayKetThucDocQuyen = ketThuc.AddDays(1);

            // LƯỢT ĐẶT PHÒNG
            var danhSachDatPhong = await _context.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.LoaiPhong)
                .Where(d =>
                    d.NgayDen >= batDau &&
                    d.NgayDen < ngayKetThucDocQuyen)
                .ToListAsync();

            int tongLuotDat = danhSachDatPhong.Count;

            int luotDaHuy = danhSachDatPhong
                .Count(d => d.TrangThai == "Đã hủy");

            int luotThanhCong = danhSachDatPhong
                .Count(d => d.TrangThai != "Đã hủy");

            // DOANH THU
            decimal doanhThu = await _context.ThanhToan
                .Where(t =>
                    t.TrangThai == "ThanhCong" &&
                    t.NgayTao >= batDau &&
                    t.NgayTao < ngayKetThucDocQuyen)
                .SumAsync(t => (decimal?)t.SoTien) ?? 0;

            // DỮ LIỆU GIAO DỊCH CHO BIỂU ĐỒ
            var giaoDichTheoThang = await _context.ThanhToan
                .Where(t =>
                    t.TrangThai == "ThanhCong" &&
                    t.NgayTao >= batDau &&
                    t.NgayTao < ngayKetThucDocQuyen)
                .Select(t => new
                {
                    t.NgayTao,
                    t.SoTien
                })
                .ToListAsync();

            var danhSachThang = new List<DateTime>();

            var thangHienTai = new DateTime(
                batDau.Year,
                batDau.Month,
                1
            );

            var thangCuoi = new DateTime(
                ketThuc.Year,
                ketThuc.Month,
                1
            );

            while (thangHienTai <= thangCuoi)
            {
                danhSachThang.Add(thangHienTai);
                thangHienTai = thangHienTai.AddMonths(1);
            }

            var nhanBieuDo = danhSachThang
                .Select(t => t.ToString("MM/yyyy"))
                .ToList();

            var doanhThuTheoThang = danhSachThang
                .Select(thang => giaoDichTheoThang
                    .Where(g =>
                        g.NgayTao.Year == thang.Year &&
                        g.NgayTao.Month == thang.Month)
                    .Sum(g => g.SoTien))
                .ToList();

            var luotDatTheoThang = danhSachThang
                .Select(thang => danhSachDatPhong
                    .Count(d =>
                        d.NgayDen.Year == thang.Year &&
                        d.NgayDen.Month == thang.Month))
                .ToList();

            ViewBag.NhanBieuDo =
                System.Text.Json.JsonSerializer.Serialize(nhanBieuDo);

            ViewBag.DoanhThuTheoThang =
                System.Text.Json.JsonSerializer.Serialize(doanhThuTheoThang);

            ViewBag.LuotDatTheoThang =
                System.Text.Json.JsonSerializer.Serialize(luotDatTheoThang);

            // TỶ LỆ LẤP ĐẦY THEO SỐ ĐÊM PHÒNG
            var tongPhong = await _context.Phong.CountAsync();

            var danhSachLuuTru = await _context.DatPhong
                .Where(d =>
                    d.TrangThai != "Đã hủy" &&
                    d.TrangThai != "Chờ thanh toán cọc" &&
                    d.NgayDen < ngayKetThucDocQuyen &&
                    d.NgayDi > batDau)
                .Select(d => new
                {
                    d.NgayDen,
                    d.NgayDi
                })
                .ToListAsync();

            int tongDemPhong = 0;

            foreach (var datPhong in danhSachLuuTru)
            {
                var ngayNhan = datPhong.NgayDen.Date < batDau
                    ? batDau
                    : datPhong.NgayDen.Date;

                var ngayTra = datPhong.NgayDi.Date > ngayKetThucDocQuyen
                    ? ngayKetThucDocQuyen
                    : datPhong.NgayDi.Date;

                tongDemPhong += Math.Max(
                    0,
                    (ngayTra - ngayNhan).Days
                );
            }

            int soNgayBaoCao =
                (ngayKetThucDocQuyen - batDau).Days;

            double tyLeLapDay = tongPhong > 0 && soNgayBaoCao > 0
                ? Math.Round(
                    (double)tongDemPhong /
                    (tongPhong * soNgayBaoCao) * 100,
                    1)
                : 0;

            // LOẠI PHÒNG ĐƯỢC ĐẶT NHIỀU NHẤT
            var topLoaiPhong = danhSachDatPhong
                .Where(d =>
                    d.TrangThai != "Đã hủy" &&
                    d.Phong?.LoaiPhong != null)
                .GroupBy(d => d.Phong!.LoaiPhong!.TenLoai)
                .Select(g => new
                {
                    TenLoai = g.Key,
                    SoLuot = g.Count()
                })
                .OrderByDescending(g => g.SoLuot)
                .ToList();

            ViewBag.TuNgay = batDau.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = ketThuc.ToString("yyyy-MM-dd");

            ViewBag.TongLuotDat = tongLuotDat;
            ViewBag.LuotThanhCong = luotThanhCong;
            ViewBag.LuotDaHuy = luotDaHuy;

            ViewBag.DoanhThu = doanhThu;

            ViewBag.TongPhong = tongPhong;
            ViewBag.TongDemPhong = tongDemPhong;
            ViewBag.TyLeLapDay = tyLeLapDay;

            ViewBag.TopLoaiPhong = topLoaiPhong;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // KHÁCH HÀNG XEM THÔNG TIN CÁ NHÂN
        [AllowAnonymous]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Forbid();

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            return View(khachHang);
        }

        // KHÁCH HÀNG CẬP NHẬT THÔNG TIN CÁ NHÂN
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatThongTinCaNhan(
            [Bind("HoTen,CCCD,SoDienThoai,Email,DiaChi")] KhachHang model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return Challenge();

            if (!User.IsInRole("KhachHang"))
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Forbid();

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == userId);

            if (khachHang == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View("ThongTinCaNhan", khachHang);

            var taiKhoan = await _userManager.FindByIdAsync(userId);

            if (taiKhoan == null)
                return NotFound();

            taiKhoan.HoTen = model.HoTen.Trim();

            taiKhoan.Email = string.IsNullOrWhiteSpace(model.Email)
                ? null
                : model.Email.Trim();

            taiKhoan.UserName = taiKhoan.Email;
            taiKhoan.PhoneNumber = model.SoDienThoai.Trim();

            var ketQua = await _userManager.UpdateAsync(taiKhoan);

            if (!ketQua.Succeeded)
            {
                foreach (var error in ketQua.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ThongTinCaNhan", khachHang);
            }

            khachHang.HoTen = model.HoTen.Trim();
            khachHang.CCCD = model.CCCD?.Trim();
            khachHang.SoDienThoai = model.SoDienThoai.Trim();
            khachHang.Email = model.Email?.Trim();
            khachHang.DiaChi = model.DiaChi?.Trim();

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                "Cập nhật thông tin cá nhân thành công.";

            return RedirectToAction(nameof(ThongTinCaNhan));
        }

        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
