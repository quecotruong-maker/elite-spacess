
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan.Models
{
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }

        [Display(Name = "Đặt phòng")]
        public int MaDatPhong { get; set; }
        public DatPhong? DatPhong { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Số tiền")]
        public decimal SoTien { get; set; }

        // TienCoc / ThanhToanConLai
        [Required, StringLength(30)]
        [Display(Name = "Loại thanh toán")]
        public string LoaiThanhToan { get; set; } = "TienCoc";

        // ChuyenKhoanMoPhong
        [Required, StringLength(50)]
        [Display(Name = "Phương thức")]
        public string PhuongThuc { get; set; } = "ChuyenKhoanMoPhong";

        // ChoThanhToan / ThanhCong / ThatBai
        [Required, StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ChoThanhToan";

        [StringLength(100)]
        [Display(Name = "Mã giao dịch")]
        public string? MaGiaoDich { get; set; }

        [Display(Name = "Ngày thanh toán")]
        public DateTime? NgayThanhToan { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
