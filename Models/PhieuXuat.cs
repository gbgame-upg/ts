using System;
using System.Collections.Generic;

namespace QLHangTonKho.Models
{
    public class PhieuXuat
    {
        public int MaPhieuXuat { get; set; }
        public DateTime NgayXuat { get; set; }
        public string LyDo { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }
        public List<ChiTietXuat> ChiTiet { get; set; } = new List<ChiTietXuat>();
    }
}