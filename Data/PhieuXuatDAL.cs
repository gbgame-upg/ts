using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Models;

namespace QLHangTonKho.Data
{
    public class PhieuXuatDAL
    {
        // ── Lấy danh sách phiếu xuất ─────────────────────────────────
        public static List<PhieuXuat> GetAll()
        {
            var list = new List<PhieuXuat>();
            var dt = DatabaseConnection.GetDataTable(
                "SELECT MaPhieuXuat, NgayXuat, LyDo, TongTien, GhiChu FROM PhieuXuat ORDER BY NgayXuat DESC");
            foreach (DataRow row in dt.Rows)
                list.Add(new PhieuXuat
                {
                    MaPhieuXuat = Convert.ToInt32(row["MaPhieuXuat"]),
                    NgayXuat = Convert.ToDateTime(row["NgayXuat"]),
                    LyDo = row["LyDo"].ToString(),
                    TongTien = Convert.ToDecimal(row["TongTien"]),
                    GhiChu = row["GhiChu"].ToString()
                });
            return list;
        }

        // ── Lấy chi tiết theo mã phiếu ───────────────────────────────
        public static List<ChiTietXuat> GetChiTiet(int maPhieuXuat)
        {
            var list = new List<ChiTietXuat>();
            string sql = @"SELECT ct.MaCTXuat, ct.MaPhieuXuat, ct.MaHang,
                                  h.TenHang, ct.SoLuong, ct.DonGia, ct.ThanhTien
                           FROM ChiTietXuat ct
                           INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                           WHERE ct.MaPhieuXuat = @MaPX";
            var dt = DatabaseConnection.GetDataTable(sql,
                new[] { new SqlParameter("@MaPX", maPhieuXuat) });
            foreach (DataRow row in dt.Rows)
                list.Add(new ChiTietXuat
                {
                    MaCTXuat = Convert.ToInt32(row["MaCTXuat"]),
                    MaPhieuXuat = Convert.ToInt32(row["MaPhieuXuat"]),
                    MaHang = Convert.ToInt32(row["MaHang"]),
                    TenHang = row["TenHang"].ToString(),
                    SoLuong = Convert.ToInt32(row["SoLuong"]),
                    DonGia = Convert.ToDecimal(row["DonGia"]),
                });
            return list;
        }

        // ── Tạo phiếu xuất + chi tiết (dùng SP) ─────────────────────
        public static int ThemPhieu(string lyDo, string ghiChu, List<ChiTietXuat> chiTiet)
        {
            var paramLyDo = new SqlParameter("@LyDo", (object)lyDo ?? DBNull.Value);
            var paramGhiChu = new SqlParameter("@GhiChu", (object)ghiChu ?? DBNull.Value);
            var paramMaPhieu = new SqlParameter("@MaPhieuXuat", SqlDbType.Int)
            { Direction = ParameterDirection.Output };

            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new SqlCommand("sp_ThemPhieuXuat", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(paramLyDo);
                cmd.Parameters.Add(paramGhiChu);
                cmd.Parameters.Add(paramMaPhieu);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            int maPhieuMoi = Convert.ToInt32(paramMaPhieu.Value);

            // Trigger SQL sẽ kiểm tra tồn kho và throw lỗi nếu không đủ
            foreach (var ct in chiTiet)
            {
                DatabaseConnection.ExecuteStoredProcedure("sp_ThemChiTietXuat", new[]
                {
                    new SqlParameter("@MaPhieuXuat", maPhieuMoi),
                    new SqlParameter("@MaHang",      ct.MaHang),
                    new SqlParameter("@SoLuong",     ct.SoLuong),
                    new SqlParameter("@DonGia",      ct.DonGia),
                });
            }

            return maPhieuMoi;
        }
    }
}