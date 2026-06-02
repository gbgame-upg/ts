namespace QLHangTonKho.Models
{
    public class HangHoa
    {
        public int MaHang { get; set; }
        public string TenHang { get; set; }
        public int MaLoai { get; set; }
        public string TenLoai { get; set; }
        public int MaDVT { get; set; }
        public string TenDVT { get; set; }
        public decimal GiaNhap { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
        public int MucToiThieu { get; set; }
        public string MoTa { get; set; }
        public bool TrangThai { get; set; }

        // Dùng cho cột Trạng thái trong DataGrid (binding từ vw_TonKhoHienTai)
        // Giá trị: "Bình thường" hoặc "Sắp hết hàng"
        public string TrangThaiTon { get; set; }
    }
}