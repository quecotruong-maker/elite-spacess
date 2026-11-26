
using System;

namespace QuanLyKhachSan.Models
{
    public class ThanhToanCocViewModel
    {
        public int MaDatPhong { get; set; }

        public int MaThanhToan { get; set; }

        public string HoTenKhachHang { get; set; } = string.Empty;

        public string SoPhong { get; set; } = string.Empty;

        public string TenLoaiPhong { get; set; } = string.Empty;

        public DateTime NgayDen { get; set; }

        public DateTime NgayDi { get; set; }

        public int SoDem { get; set; }

        public decimal GiaMotDem { get; set; }

        public decimal TongTien { get; set; }

        public decimal TienCoc { get; set; }

        public decimal PhanTramCoc { get; set; } = 30;

        public string TrangThaiThanhToan { get; set; } = "ChoThanhToan";
    }
}
