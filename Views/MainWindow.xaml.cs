using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using QLHangTonKho.Models;
using QLHangTonKho.Views;

namespace QLHangTonKho.Views
{
    public partial class MainWindow : Window
    {
        private Button _activeButton;
        private UserSession _session => UserSession.Current;

        public MainWindow()
        {
            InitializeComponent();
            ApplyPermissions();
            // Mặc định mở Tổng quan
            btnTongQuan_Click(btnTongQuan, null);
        }

        // ── Ẩn/hiện menu và nút theo quyền ──────────────────────────
        private void ApplyPermissions()
        {
            var hoTen = _session?.HoTen ?? string.Empty;
            txtUserName.Text = hoTen;
            txtUserRole.Text = _session?.TenNhom ?? string.Empty;
            txtUserAvatar.Text = !string.IsNullOrEmpty(hoTen)
                                 ? hoTen[0].ToString().ToUpper() : "U";

            badgeNhom.Background = (_session?.IsAdmin ?? false)
                ? new SolidColorBrush(Color.FromRgb(37, 99, 235))   // xanh admin
                : new SolidColorBrush(Color.FromRgb(22, 163, 74));   // xanh nhân viên

            // Ẩn menu Quản lý TK nếu không có quyền
            btnQuanLyTK.Visibility = (_session?.Quyen_QuanLyTaiKhoan ?? false)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Navigation ───────────────────────────────────────────────
        private void btnTongQuan_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Tổng quan";
            contentArea.Content = new TongQuanView();
        }

        private void btnHangHoa_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Quản lý hàng hóa";
            contentArea.Content = new HangHoaView();
        }

        private void btnNCC_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Nhà cung cấp";
            contentArea.Content = new NhaCungCapView();
        }

        private void btnNhap_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Phiếu nhập kho";
            contentArea.Content = new PhieuNhapView();
        }

        private void btnXuat_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Phiếu xuất kho";
            contentArea.Content = new PhieuXuatView();
        }

        private void btnBaoCao_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Báo cáo thống kê";
            contentArea.Content = new BaoCaoView();
        }

        private void btnQuanLyTK_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(sender as Button);
            txtTieuDe.Text = "Quản lý tài khoản";
            contentArea.Content = new QuanLyTaiKhoanView();
        }

        // ── Đăng xuất ────────────────────────────────────────────────
        private void btnDangXuat_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn đăng xuất?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            UserSession.Current.Logout();
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void SetActiveButton(Button btn)
        {
            if (_activeButton != null)
                _activeButton.Background = Brushes.Transparent;
            if (btn != null)
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                _activeButton = btn;
            }
        }
    }
}