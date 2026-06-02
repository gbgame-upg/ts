using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Models;

namespace QLHangTonKho.Data
{
    public class PhieuNhapDAL
    {
        // ── Lấy danh sách phiếu nhập ─────────────────────────────────
        public static List<PhieuNhap> GetAll()
        {
            var list = new List<PhieuNhap>();
            string sql = @"SELECT pn.MaPhieuNhap, pn.MaNCC, ncc.TenNCC,
                                  pn.NgayNhap, pn.TongTien, pn.GhiChu
                           FROM PhieuNhap pn
                           INNER JOIN NhaCungCap ncc ON pn.MaNCC = ncc.MaNCC
                           ORDER BY pn.NgayNhap DESC";
            var dt = DatabaseConnection.GetDataTable(sql);
            foreach (DataRow row in dt.Rows)
                list.Add(new PhieuNhap
                {
                    MaPhieuNhap = Convert.ToInt32(row["MaPhieuNhap"]),
                    MaNCC = Convert.ToInt32(row["MaNCC"]),
                    TenNCC = row["TenNCC"].ToString(),
                    NgayNhap = Convert.ToDateTime(row["NgayNhap"]),
                    TongTien = Convert.ToDecimal(row["TongTien"]),
                    GhiChu = row["GhiChu"].ToString()
                });
            return list;
        }

        // ── Lấy chi tiết theo mã phiếu ───────────────────────────────
        public static List<ChiTietNhap> GetChiTiet(int maPhieuNhap)
        {
            var list = new List<ChiTietNhap>();
            string sql = @"SELECT ct.MaCTNhap, ct.MaPhieuNhap, ct.MaHang,
                                  h.TenHang, ct.SoLuong, ct.DonGia, ct.ThanhTien
                           FROM ChiTietNhap ct
                           INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                           WHERE ct.MaPhieuNhap = @MaPN";
            var dt = DatabaseConnection.GetDataTable(sql,
                new[] { new SqlParameter("@MaPN", maPhieuNhap) });
            foreach (DataRow row in dt.Rows)
                list.Add(new ChiTietNhap
                {
                    MaCTNhap = Convert.ToInt32(row["MaCTNhap"]),
                    MaPhieuNhap = Convert.ToInt32(row["MaPhieuNhap"]),
                    MaHang = Convert.ToInt32(row["MaHang"]),
                    TenHang = row["TenHang"].ToString(),
                    SoLuong = Convert.ToInt32(row["SoLuong"]),
                    DonGia = Convert.ToDecimal(row["DonGia"]),
                });
            return list;
        }

        // ── Tạo phiếu nhập + chi tiết (dùng SP) ─────────────────────
        public static int ThemPhieu(int maNCC, string ghiChu, List<ChiTietNhap> chiTiet)
        {
            // Tạo phiếu nhập header
            var paramMaNCC = new SqlParameter("@MaNCC", maNCC);
            var paramGhiChu = new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value);
            var paramMaPhieu = new SqlParameter("@MaPhieuNhap", SqlDbType.Int)
            { Direction = ParameterDirection.Output };

            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("sp_ThemPhieuNhap", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(paramMaNCC);
                cmd.Parameters.Add(paramGhiChu);
                cmd.Parameters.Add(paramMaPhieu);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            int maPhieuMoi = Convert.ToInt32(paramMaPhieu.Value);

            // Thêm từng dòng chi tiết → trigger tự cập nhật SoLuongTon + TongTien
            foreach (var ct in chiTiet)
            {
                DatabaseConnection.ExecuteStoredProcedure("sp_ThemChiTietNhap", new[]
                {
                    new SqlParameter("@MaPhieuNhap", maPhieuMoi),
                    new SqlParameter("@MaHang",      ct.MaHang),
                    new SqlParameter("@SoLuong",     ct.SoLuong),
                    new SqlParameter("@DonGia",      ct.DonGia),
                });
            }

            return maPhieuMoi;
        }
    }
}