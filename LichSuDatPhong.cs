
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan.Models
{
    public class LichSuDatPhong
    {
        [Key]
        public int MaLichSu { get; set; }

        [Required]
        public int MaDatPhong { get; set; }

        public DatPhong? DatPhong { get; set; }

        public int MaPhongCu { get; set; }

        [StringLength(20)]
        public string SoPhongCu { get; set; } = string.Empty;

        public int MaPhongMoi { get; set; }

        [StringLength(20)]
        public string SoPhongMoi { get; set; } = string.Empty;

        // Ngày bắt đầu tính tiền tại phòng mới
        public DateTime NgayBatDau { get; set; }

        // Giá mỗi đêm của phòng cũ tại thời điểm chuyển
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaPhongCu { get; set; }

        // Giá mỗi đêm của phòng mới tại thời điểm chuyển
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaPhongMoi { get; set; }

        [StringLength(450)]
        public string? NguoiThucHienId { get; set; }

        [StringLength(100)]
        public string TenNguoiThucHien { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LyDo { get; set; }

        public DateTime ThoiGianThayDoi { get; set; }
            = DateTime.Now;
    }
}
