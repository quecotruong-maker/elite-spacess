namespace QuanLyKhachSan.Models
{
    public class LichDatPhongViewModel
    {
        public int MaPhong { get; set; }

        public string SoPhong { get; set; } = "";

        public string TenLoaiPhong { get; set; } = "";
        public string TrangThaiPhong { get; set; } = "";

        public List<LichDatPhongItem> DanhSachDatPhong { get; set; } = new();
    }

    public class LichDatPhongItem
    {
        public int MaDatPhong { get; set; }

        public string TenKhachHang { get; set; } = "";

        public DateTime NgayDen { get; set; }

        public DateTime NgayDi { get; set; }

        public string TrangThai { get; set; } = "";
    }
}