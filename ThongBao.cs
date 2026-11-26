using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan.Models
{
    public class ThongBao
    {
        [Key]
        public int MaThongBao { get; set; }

        public int MaKhach { get; set; }

        public int MaDatPhong { get; set; }

        [Required]
        [StringLength(50)]
        public string LoaiThongBao { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public bool DaDoc { get; set; } = false;
    }
}