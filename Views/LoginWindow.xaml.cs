using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Data;
using QLHangTonKho.Helpers;
using QLHangTonKho.Models;

namespace QLHangTonKho.Views
{
    public partial class LoginWindow : Window
    {
        private bool _isLoginTab = true;

        public LoginWindow()
        {
            InitializeComponent();
            txtUsername.Focus();

            // Drag to move
            this.MouseLeftButtonDown += (s, e) => DragMove();
        }

        // ── Chuyển tab ───────────────────────────────────────────────
        private void TabLogin_Click(object sender, RoutedEventArgs e)
        {
            _isLoginTab = true;
            panelLogin.Visibility = Visibility.Visible;
            panelRegister.Visibility = Visibility.Collapsed;
            lblSubtitle.Text = "Chào mừng đến Hệ thống Quản lý Kho";
            // Underline active
            SetTabStyle(true);
        }

        private void TabRegister_Click(object sender, RoutedEventArgs e)
        {
            _isLoginTab = false;
            panelLogin.Visibility = Visibility.Collapsed;
            panelRegister.Visibility = Visibility.Visible;
            lblSubtitle.Text = "Tạo tài khoản nhân viên mới";
            SetTabStyle(false);
        }

        private void SetTabStyle(bool loginActive)
        {
            // Đơn giản dùng FontWeight để phân biệt tab active
            btnTabLogin.FontWeight = loginActive ? FontWeights.Bold : FontWeights.Normal;
            btnTabRegister.FontWeight = loginActive ? FontWeights.Normal : FontWeights.Bold;
        }

        // ── Đăng nhập ────────────────────────────────────────────────
        private void btnDangNhap_Click(object sender, RoutedEventArgs e)
        {
            lblLoginError.Visibility = Visibility.Collapsed;

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowLoginError("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return;
            }

            try
            {
                string hash = AuthHelper.HashPassword(password);
                var dt = DatabaseConnection.GetDataTableByProc("sp_DangNhap", new[]
                {
                    new SqlParameter("@TenDangNhap", username),
                    new SqlParameter("@MatKhauHash", hash)
                });

                if (dt.Rows.Count == 0)
                {
                    ShowLoginError("Tên đăng nhập hoặc mật khẩu không đúng.\nHoặc tài khoản đã bị khóa.");
                    return;
                }

                // Lưu session
                var row = dt.Rows[0];
                var s = UserSession.Current;
                s.MaTK = Convert.ToInt32(row["MaTK"]);
                s.TenDangNhap = row["TenDangNhap"].ToString();
                s.HoTen = row["HoTen"].ToString();
                s.Email = row["Email"].ToString();
                s.TenNhom = row["TenNhom"].ToString();
                s.IsLoggedIn = true;

                s.Quyen_XemHangHoa = Convert.ToBoolean(row["Quyen_XemHangHoa"]);
                s.Quyen_SuaHangHoa = Convert.ToBoolean(row["Quyen_SuaHangHoa"]);
                s.Quyen_XemNCC = Convert.ToBoolean(row["Quyen_XemNCC"]);
                s.Quyen_SuaNCC = Convert.ToBoolean(row["Quyen_SuaNCC"]);
                s.Quyen_XemPhieuNhap = Convert.ToBoolean(row["Quyen_XemPhieuNhap"]);
                s.Quyen_TaoPhieuNhap = Convert.ToBoolean(row["Quyen_TaoPhieuNhap"]);
                s.Quyen_XemPhieuXuat = Convert.ToBoolean(row["Quyen_XemPhieuXuat"]);
                s.Quyen_TaoPhieuXuat = Convert.ToBoolean(row["Quyen_TaoPhieuXuat"]);
                s.Quyen_XemBaoCao = Convert.ToBoolean(row["Quyen_XemBaoCao"]);
                s.Quyen_QuanLyTaiKhoan = Convert.ToBoolean(row["Quyen_QuanLyTaiKhoan"]);

                // Mở MainWindow
                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ShowLoginError($"Lỗi kết nối:\n{ex.Message}");
            }
        }

        // ── Đăng ký ──────────────────────────────────────────────────
        private void btnDangKy_Click(object sender, RoutedEventArgs e)
        {
            lblRegError.Visibility = Visibility.Collapsed;

            string hoTen = txtRegHoTen.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string username = txtRegUsername.Text.Trim();
            string password = txtRegPassword.Password;

            if (string.IsNullOrWhiteSpace(hoTen))
            { ShowRegError("Vui lòng nhập họ tên."); return; }
            if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
            { ShowRegError("Tên đăng nhập tối thiểu 4 ký tự."); return; }
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            { ShowRegError("Mật khẩu tối thiểu 6 ký tự."); return; }

            try
            {
                string hash = AuthHelper.HashPassword(password);
                var dt = DatabaseConnection.GetDataTableByProc("sp_DangKy", new[]
                {
                    new SqlParameter("@TenDangNhap", username),
                    new SqlParameter("@MatKhauHash", hash),
                    new SqlParameter("@HoTen",       hoTen),
                    new SqlParameter("@Email",       string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email),
                });

                if (dt.Rows.Count > 0)
                {
                    int ketQua = Convert.ToInt32(dt.Rows[0]["KetQua"]);
                    string msg = dt.Rows[0]["ThongBao"].ToString();

                    if (ketQua == -1)
                    { ShowRegError(msg); return; }

                    MessageBox.Show($"✅ {msg}\nBạn có thể đăng nhập bằng tài khoản vừa tạo.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Tự chuyển sang tab đăng nhập
                    txtUsername.Text = username;
                    TabLogin_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                ShowRegError($"Lỗi:\n{ex.Message}");
            }
        }

        private void ShowLoginError(string msg)
        {
            lblLoginError.Text = msg;
            lblLoginError.Visibility = Visibility.Visible;
        }

        private void ShowRegError(string msg)
        {
            lblRegError.Text = msg;
            lblRegError.Visibility = Visibility.Visible;
        }

        private void btnThoat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}