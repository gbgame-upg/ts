using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace QLHangTonKho.Data
{
    /// <summary>
    /// Singleton quản lý kết nối SQL Server.
    /// Dùng chung 1 connection string cho toàn ứng dụng.
    /// </summary>
    public class DatabaseConnection
    {
        // ── Connection string ─────────────────────────────────────────
        // Lấy từ App.config (khuyến nghị) hoặc hardcode để test
        private static readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["QLHangTonKhoDB"]?.ConnectionString
            ?? "MAY73\\SQLEXPRESS;Database=QLHangTonKho;Integrated Security=True;TrustServerCertificate=True;";

        // ── Tạo kết nối mới ──────────────────────────────────────────
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // ── Kiểm tra kết nối ─────────────────────────────────────────
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        // ── ExecuteNonQuery (INSERT / UPDATE / DELETE) ────────────────
        /// <summary>Trả về số dòng bị ảnh hưởng.</summary>
        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // ── ExecuteStoredProcedure ────────────────────────────────────
        public static int ExecuteStoredProcedure(string procName, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(procName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        // ── ExecuteScalar (lấy 1 giá trị) ────────────────────────────
        public static object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        // ── GetDataTable (SELECT → DataTable) ────────────────────────
        public static DataTable GetDataTable(string sql, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        // ── GetDataTableByProc (Stored Procedure → DataTable) ─────────
        public static DataTable GetDataTableByProc(string procName, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(procName, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}