using System;
using System.Collections.Generic;

namespace QLHangTonKho.Models
{
    public class PhieuNhap
    {
        public int MaPhieuNhap { get; set; }
        public int MaNCC { get; set; }
        public string TenNCC { get; set; }
        public DateTime NgayNhap { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }
        public List<ChiTietNhap> ChiTiet { get; set; } = new List<ChiTietNhap>();
    }
}