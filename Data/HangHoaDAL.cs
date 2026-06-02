using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Models;

namespace QLHangTonKho.Data
{
    public class HangHoaDAL
    {
        // ── Lấy tất cả hàng hóa (từ vw_TonKhoHienTai qua SP) ────────
        public static List<HangHoa> GetAll()
        {
            var list = new List<HangHoa>();
            var dt = DatabaseConnection.GetDataTableByProc("sp_GetDanhSachHangHoa");
            foreach (DataRow row in dt.Rows)
                list.Add(MapRow(row));
            return list;
        }

        // ── Tìm kiếm theo tên ────────────────────────────────────────
        public static List<HangHoa> Search(string tuKhoa)
        {
            var list = new List<HangHoa>();
            var dt = DatabaseConnection.GetDataTableByProc("sp_TimKiemHangHoa",
                new[] { new SqlParameter("@TuKhoa", tuKhoa) });
            foreach (DataRow row in dt.Rows)
                list.Add(MapRow(row));
            return list;
        }

        // ── Thêm hàng hóa ────────────────────────────────────────────
        public static int Add(HangHoa h)
        {
            string sql = @"INSERT INTO HangHoa (TenHang, MaLoai, MaDVT, GiaNhap, GiaBan, SoLuongTon, MucToiThieu, MoTa, TrangThai)
                           VALUES (@TenHang, @MaLoai, @MaDVT, @GiaNhap, @GiaBan, @SoLuongTon, @MucToiThieu, @MoTa, 1)";
            return DatabaseConnection.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@TenHang",     h.TenHang),
                new SqlParameter("@MaLoai",      h.MaLoai),
                new SqlParameter("@MaDVT",       h.MaDVT),
                new SqlParameter("@GiaNhap",     h.GiaNhap),
                new SqlParameter("@GiaBan",      h.GiaBan),
                new SqlParameter("@SoLuongTon",  h.SoLuongTon),
                new SqlParameter("@MucToiThieu", h.MucToiThieu),
                new SqlParameter("@MoTa",        (object)h.MoTa ?? DBNull.Value),
            });
        }

        // ── Cập nhật hàng hóa ────────────────────────────────────────
        public static int Update(HangHoa h)
        {
            string sql = @"UPDATE HangHoa SET TenHang=@TenHang, MaLoai=@MaLoai, MaDVT=@MaDVT,
                           GiaNhap=@GiaNhap, GiaBan=@GiaBan, MucToiThieu=@MucToiThieu, MoTa=@MoTa
                           WHERE MaHang=@MaHang";
            return DatabaseConnection.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@TenHang",     h.TenHang),
                new SqlParameter("@MaLoai",      h.MaLoai),
                new SqlParameter("@MaDVT",       h.MaDVT),
                new SqlParameter("@GiaNhap",     h.GiaNhap),
                new SqlParameter("@GiaBan",      h.GiaBan),
                new SqlParameter("@MucToiThieu", h.MucToiThieu),
                new SqlParameter("@MoTa",        (object)h.MoTa ?? DBNull.Value),
                new SqlParameter("@MaHang",      h.MaHang),
            });
        }

        // ── Xóa mềm (TrangThai = 0) ──────────────────────────────────
        public static int Delete(int maHang)
        {
            return DatabaseConnection.ExecuteNonQuery(
                "UPDATE HangHoa SET TrangThai = 0 WHERE MaHang = @MaHang",
                new[] { new SqlParameter("@MaHang", maHang) });
        }

        // ── Lấy danh sách loại hàng ──────────────────────────────────
        public static List<LoaiHang> GetDanhSachLoai()
        {
            var list = new List<LoaiHang>();
            var dt = DatabaseConnection.GetDataTable("SELECT MaLoai, TenLoai FROM LoaiHang ORDER BY TenLoai");
            foreach (DataRow row in dt.Rows)
                list.Add(new LoaiHang { MaLoai = Convert.ToInt32(row["MaLoai"]), TenLoai = row["TenLoai"].ToString() });
            return list;
        }

        // ── Lấy danh sách đơn vị tính ────────────────────────────────
        public static DataTable GetDanhSachDVT()
        {
            return DatabaseConnection.GetDataTable("SELECT MaDVT, TenDVT FROM DonViTinh ORDER BY TenDVT");
        }

        // ── Map DataRow → HangHoa ─────────────────────────────────────
        private static HangHoa MapRow(DataRow row) => new HangHoa
        {
            MaHang = Convert.ToInt32(row["MaHang"]),
            TenHang = row["TenHang"].ToString(),
            TenLoai = row.Table.Columns.Contains("TenLoai") ? row["TenLoai"].ToString() : "",
            TenDVT = row.Table.Columns.Contains("TenDVT") ? row["TenDVT"].ToString() : "",
            GiaNhap = Convert.ToDecimal(row["GiaNhap"]),
            GiaBan = Convert.ToDecimal(row["GiaBan"]),
            SoLuongTon = Convert.ToInt32(row["SoLuongTon"]),
            MucToiThieu = Convert.ToInt32(row["MucToiThieu"]),
            TrangThaiTon = row.Table.Columns.Contains("TrangThaiTon") ? row["TrangThaiTon"].ToString() : ""
        };
    }
}