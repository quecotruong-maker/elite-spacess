
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "KhachHang")]
    public class DonDatCuaKhachController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonDatCuaKhachController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách đơn đặt phòng của khách hàng đang đăng nhập
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == user.Id);

            if (khachHang == null)
                return View(new List<DatPhong>());

            var danhSachDatPhong = await _context.DatPhong
                .Include(d => d.Phong)
                .Where(d => d.MaKhach == khachHang.MaKhach)
                .OrderByDescending(d => d.MaDatPhong)
                .ToListAsync();

            return View(danhSachDatPhong);
        }

        // Hiển thị trang đặt phòng
        [HttpGet]
        public async Task<IActionResult> DatPhong(int maPhong)
        {
            var phong = await _context.Phong
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);

            if (phong == null || phong.TrangThai != "Trống")
            {
                TempData["ThongBao"] =
                    "Phòng không tồn tại hoặc hiện không còn trống.";

                return RedirectToAction("Index", "Home");
            }

            ViewBag.MaPhong = phong.MaPhong;
            ViewBag.SoPhong = phong.SoPhong;
            ViewBag.TenLoai = phong.LoaiPhong?.TenLoai;
            ViewBag.GiaMotDem = phong.LoaiPhong?.GiaMotDem;
            ViewBag.SoNguoiToiDa = phong.LoaiPhong?.SoNguoiToiDa;

            return View();
        }


        // Xử lý khi khách xác nhận đặt phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatPhong(
            int MaPhong,
            DateTime NgayDen,
            DateTime NgayDi,
            string? GhiChu)
        {
            // Kiểm tra ngày đặt phòng
            if (NgayDen.Date < DateTime.Today ||
                NgayDi.Date <= NgayDen.Date)
            {
                TempData["ThongBao"] =
                    "Ngày đặt phòng không hợp lệ. Vui lòng kiểm tra lại.";

                return RedirectToAction("DatPhong", new { maPhong = MaPhong });
            }

            // Lấy tài khoản khách hàng đang đăng nhập
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Tìm hồ sơ khách hàng
            var khachHang = await _context.KhachHang
                .FirstOrDefaultAsync(k => k.IdentityUserId == user.Id);

            if (khachHang == null)
                return Forbid();

            // Kiểm tra phòng
            var phong = await _context.Phong
                .FirstOrDefaultAsync(p => p.MaPhong == MaPhong);

            if (phong == null || phong.TrangThai != "Trống")
            {
                TempData["ThongBao"] =
                    "Phòng hiện không còn trống. Vui lòng chọn phòng khác.";

                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra lịch đặt phòng bị trùng
            var biTrung = await _context.DatPhong.AnyAsync(d =>
                d.MaPhong == MaPhong &&
                (d.TrangThai == "Đã đặt" ||
                 d.TrangThai == "Đã check-in") &&
                d.NgayDen < NgayDi.Date &&
                d.NgayDi > NgayDen.Date);

            if (biTrung)
            {
                TempData["ThongBao"] =
                    "Phòng đã có người đặt trong khoảng thời gian này.";

                return RedirectToAction("Index", "Home");
            }

            // Tạo đơn đặt phòng
            var datPhong = new DatPhong
            {
                MaKhach = khachHang.MaKhach,
                MaPhong = MaPhong,
                NgayDen = NgayDen.Date,
                NgayDi = NgayDi.Date,
                TrangThai = "Đã đặt",
                GhiChu = GhiChu
            };

            _context.DatPhong.Add(datPhong);
            await _context.SaveChangesAsync();

            // Thông báo mã đặt phòng
            TempData["ThongBao"] =
                "Đặt phòng thành công! Mã đặt phòng của bạn là: "
                + datPhong.MaDatPhong;

            return RedirectToAction("Index");
        }


    }
}
