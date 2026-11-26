
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan.Models
{
    public class HoSoKhachHangViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "CCCD/Hộ chiếu")]
        public string? CCCD { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }
}
