
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan.Models
{
    public class DatPhong
    {
        [Key]
        public int MaDatPhong { get; set; }

        [Display(Name = "Khách hàng")]
        public int MaKhach { get; set; }

        public KhachHang? KhachHang { get; set; }

        [Display(Name = "Phòng")]
        public int MaPhong { get; set; }

        public Phong? Phong { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đến")]
        public DateTime NgayDen { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đi")]
        public DateTime NgayDi { get; set; }

        // Đã đặt / Đã check-in / Đã check-out / Đã hủy
        [Required, StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đã đặt";

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // Danh sách các khoản thanh toán của đặt phòng
        public ICollection<ThanhToan> ThanhToans { get; set; }
            = new List<ThanhToan>();
    }
}
