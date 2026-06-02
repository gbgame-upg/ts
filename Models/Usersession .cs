namespace QLHangTonKho.Models
{
    /// <summary>
    /// Singleton — lưu thông tin tài khoản đang đăng nhập.
    /// Truy cập ở bất kỳ đâu: UserSession.Current.HoTen
    /// </summary>
    public class UserSession
    {
        private static UserSession _instance;
        public static UserSession Current => _instance ??= new UserSession();

        // ── Thông tin cơ bản ──────────────────────────────────────
        public int    MaTK          { get; set; }
        public string TenDangNhap   { get; set; }
        public string HoTen         { get; set; }
        public string Email         { get; set; }
        public string TenNhom       { get; set; }
        public bool   IsLoggedIn     { get; set; }

        // ── Quyền hạn ────────────────────────────────────────────
        public bool Quyen_XemHangHoa    { get; set; }
        public bool Quyen_SuaHangHoa    { get; set; }
        public bool Quyen_XemNCC        { get; set; }
        public bool Quyen_SuaNCC        { get; set; }
        public bool Quyen_XemPhieuNhap  { get; set; }
        public bool Quyen_TaoPhieuNhap  { get; set; }
        public bool Quyen_XemPhieuXuat  { get; set; }
        public bool Quyen_TaoPhieuXuat  { get; set; }
        public bool Quyen_XemBaoCao     { get; set; }
        public bool Quyen_QuanLyTaiKhoan{ get; set; }

        // ── Helper ───────────────────────────────────────────────
        public bool IsAdmin => TenNhom == "Admin";

        public void Logout()
        {
            _instance = new UserSession();
        }
    }
}