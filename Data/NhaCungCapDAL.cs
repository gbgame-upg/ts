using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Models;

namespace QLHangTonKho.Data
{
    public class NhaCungCapDAL
    {
        // ── Lấy tất cả ───────────────────────────────────────────────
        public static List<NhaCungCap> GetAll()
        {
            var list = new List<NhaCungCap>();
            var dt = DatabaseConnection.GetDataTable(
                "SELECT MaNCC, TenNCC, DiaChi, DienThoai, Email, TrangThai FROM NhaCungCap ORDER BY MaNCC");
            foreach (DataRow row in dt.Rows)
                list.Add(MapRow(row));
            return list;
        }

        // ── Chỉ lấy NCC đang hợp tác (dùng cho ComboBox phiếu nhập) ─
        public static List<NhaCungCap> GetActive()
        {
            var list = new List<NhaCungCap>();
            var dt = DatabaseConnection.GetDataTable(
                "SELECT MaNCC, TenNCC, DiaChi, DienThoai, Email, TrangThai FROM NhaCungCap WHERE TrangThai = 1 ORDER BY TenNCC");
            foreach (DataRow row in dt.Rows)
                list.Add(MapRow(row));
            return list;
        }

        // ── Tìm kiếm ─────────────────────────────────────────────────
        public static List<NhaCungCap> Search(string tuKhoa)
        {
            var list = new List<NhaCungCap>();
            var dt = DatabaseConnection.GetDataTable(
                "SELECT MaNCC, TenNCC, DiaChi, DienThoai, Email, TrangThai FROM NhaCungCap WHERE TenNCC LIKE @TuKhoa OR DienThoai LIKE @TuKhoa OR Email LIKE @TuKhoa",
                new[] { new SqlParameter("@TuKhoa", $"%{tuKhoa}%") });
            foreach (DataRow row in dt.Rows)
                list.Add(MapRow(row));
            return list;
        }

        // ── Thêm ─────────────────────────────────────────────────────
        public static int Add(NhaCungCap n)
        {
            return DatabaseConnection.ExecuteNonQuery(
                "INSERT INTO NhaCungCap (TenNCC, DiaChi, DienThoai, Email, TrangThai) VALUES (@TenNCC, @DiaChi, @DienThoai, @Email, 1)",
                new[]
                {
                    new SqlParameter("@TenNCC",    n.TenNCC),
                    new SqlParameter("@DiaChi",    (object)n.DiaChi    ?? DBNull.Value),
                    new SqlParameter("@DienThoai", (object)n.DienThoai ?? DBNull.Value),
                    new SqlParameter("@Email",     (object)n.Email     ?? DBNull.Value),
                });
        }

        // ── Cập nhật ─────────────────────────────────────────────────
        public static int Update(NhaCungCap n)
        {
            return DatabaseConnection.ExecuteNonQuery(
                "UPDATE NhaCungCap SET TenNCC=@TenNCC, DiaChi=@DiaChi, DienThoai=@DienThoai, Email=@Email, TrangThai=@TrangThai WHERE MaNCC=@MaNCC",
                new[]
                {
                    new SqlParameter("@TenNCC",    n.TenNCC),
                    new SqlParameter("@DiaChi",    (object)n.DiaChi    ?? DBNull.Value),
                    new SqlParameter("@DienThoai", (object)n.DienThoai ?? DBNull.Value),
                    new SqlParameter("@Email",     (object)n.Email     ?? DBNull.Value),
                    new SqlParameter("@TrangThai", n.TrangThai),
                    new SqlParameter("@MaNCC",     n.MaNCC),
                });
        }

        // ── Xóa mềm ──────────────────────────────────────────────────
        public static int Delete(int maNCC)
        {
            return DatabaseConnection.ExecuteNonQuery(
                "UPDATE NhaCungCap SET TrangThai = 0 WHERE MaNCC = @MaNCC",
                new[] { new SqlParameter("@MaNCC", maNCC) });
        }

        // ── Map DataRow → NhaCungCap ──────────────────────────────────
        private static NhaCungCap MapRow(DataRow row) => new NhaCungCap
        {
            MaNCC = Convert.ToInt32(row["MaNCC"]),
            TenNCC = row["TenNCC"].ToString(),
            DiaChi = row["DiaChi"].ToString(),
            DienThoai = row["DienThoai"].ToString(),
            Email = row["Email"].ToString(),
            TrangThai = Convert.ToBoolean(row["TrangThai"])
        };
    }
}