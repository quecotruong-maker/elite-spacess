
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan.Data;
using QuanLyKhachSan.Models;

namespace QuanLyKhachSan.Background
{
    public class ThongBaoBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime _lanChay09 = DateTime.MinValue;
        private DateTime _lanChay18 = DateTime.MinValue;

        public ThongBaoBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;

                try
                {
                    // Chạy lúc 09:00
                    if (now.Hour == 9 && _lanChay09.Date != now.Date)
                    {
                        await TaoThongBao09GioAsync(stoppingToken);
                        _lanChay09 = now;
                    }

                    // Chạy lúc 18:00
                    if (now.Hour == 18 && _lanChay18.Date != now.Date)
                    {
                        await TaoThongBao18GioAsync(stoppingToken);
                        _lanChay18 = now;
                    }
                }
                catch (Exception ex)
                {
                    // Ghi lỗi ra cửa sổ Output/Debug
                    Console.WriteLine($"Lỗi tạo thông báo: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        // Tạo thông báo lúc 09:00
        private async Task TaoThongBao09GioAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            // Lấy các đơn đặt phòng chưa bị hủy
            var danhSach = await context.Set<DatPhong>()
                .Where(d => d.TrangThai == "Đã đặt" ||
                            d.TrangThai == "Đã check-in")
                .ToListAsync(token);

            DateTime homNay = DateTime.Today;

            foreach (var datPhong in danhSach)
            {
                // Ngày trả phòng hôm nay
                if (datPhong.NgayDi.Date == homNay)
                {
                    await TaoThongBaoAsync(
                        context,
                        datPhong,
                        "TraPhongHomNay",
                        "Nhắc nhở trả phòng",
                        "Hôm nay là ngày trả phòng của bạn. "
                        + "Vui lòng hoàn tất trả phòng trước 12:00."
                    );
                }

                // Ngày nhận phòng hôm nay
                if (datPhong.NgayDen.Date == homNay &&
                    datPhong.TrangThai == "Đã đặt")
                {
                    await TaoThongBaoAsync(
                        context,
                        datPhong,
                        "NhanPhongHomNay",
                        "Nhắc nhở nhận phòng",
                        "Hôm nay là ngày nhận phòng của bạn. "
                        + "Bạn có thể nhận phòng từ 14:00."
                    );
                }
            }
        }

        // Tạo thông báo lúc 18:00
        private async Task TaoThongBao18GioAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            DateTime ngayMai = DateTime.Today.AddDays(1);

            var danhSach = await context.Set<DatPhong>()
                .Where(d =>
                    d.NgayDi.Date == ngayMai &&
                    (d.TrangThai == "Đã đặt" ||
                     d.TrangThai == "Đã check-in"))
                .ToListAsync(token);

            foreach (var datPhong in danhSach)
            {
                await TaoThongBaoAsync(
                    context,
                    datPhong,
                    "TraPhongNgayMai",
                    "Nhắc nhở trả phòng ngày mai",
                    "Ngày mai là ngày trả phòng của bạn. "
                    + "Vui lòng chuẩn bị và hoàn tất trả phòng trước 12:00."
                );
            }
        }

        // Tạo thông báo và kiểm tra trùng lặp
        private async Task TaoThongBaoAsync(
            ApplicationDbContext context,
            DatPhong datPhong,
            string loaiThongBao,
            string tieuDe,
            string noiDung)
        {
            bool daTonTai = await context.ThongBaos.AnyAsync(t =>
                t.MaDatPhong == datPhong.MaDatPhong &&
                t.LoaiThongBao == loaiThongBao);

            if (daTonTai)
                return;

            var thongBao = new ThongBao
            {
                MaKhach = datPhong.MaKhach,
                MaDatPhong = datPhong.MaDatPhong,
                LoaiThongBao = loaiThongBao,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                DaDoc = false
            };

            context.ThongBaos.Add(thongBao);
            await context.SaveChangesAsync();
        }
    }
}
