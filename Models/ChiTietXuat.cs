namespace QLHangTonKho.Models
{
    public class ChiTietXuat
    {
        public int MaCTXuat { get; set; }
        public int MaPhieuXuat { get; set; }
        public int MaHang { get; set; }
        public string TenHang { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}