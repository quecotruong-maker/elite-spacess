using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Services;
using QuanLyKhachSan.Models;
namespace QuanLyKhachSan.Controllers
{
    [Authorize(Roles = "KhachHang")]
    public class ThongBaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ThongBaoService _thongBaoService;

        public ThongBaoController(
            ApplicationDbContext context,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _thongBaoService = thongBaoService;
        }

        // Danh sách thông báo của khách hàng
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var khachHang = await _context.Set<KhachHang>()
                .FirstOrDefaultAsync(k =>
                    k.IdentityUserId == userId);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            var danhSach = await _thongBaoService
                .LayThongBaoCuaKhachAsync(khachHang.MaKhach);

            ViewBag.SoThongBaoChuaDoc =
                await _thongBaoService
                    .DemThongBaoChuaDocAsync(khachHang.MaKhach);

            return View(danhSach);
        }

        // Bấm vào thông báo để mở chi tiết đơn đặt phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mo(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var khachHang = await _context.Set<KhachHang>()
                .FirstOrDefaultAsync(k =>
                    k.IdentityUserId == userId);

            if (khachHang == null)
            {
                return NotFound();
            }

            var thongBao = await _context.ThongBaos
                .FirstOrDefaultAsync(t =>
                    t.MaThongBao == id &&
                    t.MaKhach == khachHang.MaKhach);

            if (thongBao == null)
            {
                return NotFound();
            }

            await _thongBaoService.DanhDauDaDocAsync(
                id,
                khachHang.MaKhach);

            return RedirectToAction(
                "ChiTietCuaToi",
                "DatPhongs",
                new { id = thongBao.MaDatPhong });
        }
    }
}