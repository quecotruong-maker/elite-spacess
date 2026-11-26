using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan.Models
{
    public class LoaiPhong
    {
        [Key]
        public int MaLoaiPhong { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Tên loại phòng")]
        public string TenLoai { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá một đêm")]
        public decimal GiaMotDem { get; set; }

        [Display(Name = "Số người tối đa")]
        public int SoNguoiToiDa { get; set; }

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        public ICollection<Phong> DanhSachPhong { get; set; } = new List<Phong>();
    }
}