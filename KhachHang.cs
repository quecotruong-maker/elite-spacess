
using System.ComponentModel.DataAnnotations;
using QuanLyKhachSan.Data;

namespace QuanLyKhachSan.Models
{
    public class KhachHang
    {
        [Key]
        public int MaKhach { get; set; }

        [Display(Name = "Tài khoản đăng nhập")]
        public string? IdentityUserId { get; set; }

        public ApplicationUser? ApplicationUser { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "CCCD/Hộ chiếu")]
        public string? CCCD { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }
}
