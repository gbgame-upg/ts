using System;
using System.Collections.Generic;
using System.Text;

namespace QLHangTonKho.Models
{

    public class ChiTietNhap
    {
        public int MaCTNhap { get; set; }
        public int MaPhieuNhap { get; set; }
        public int MaHang { get; set; }
        public string TenHang { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
