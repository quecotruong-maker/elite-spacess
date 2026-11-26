using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHoaDon { get; set; }

        [Display(Name = "Đặt phòng")]
        public int MaDatPhong { get; set; }
        public DatPhong? DatPhong { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [Display(Name = "Ngày lập")]
        public DateTime NgayLap { get; set; } = DateTime.Now;

        [Display(Name = "Đã thanh toán")]
        public bool DaThanhToan { get; set; }
    }
}