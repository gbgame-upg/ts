using System.Windows;
using QLHangTonKho.Models;

namespace QLHangTonKho.Views
{
    public partial class NhaCungCapDialog : Window
    {
        public NhaCungCap KetQua { get; private set; }
        private readonly NhaCungCap _editItem;

        public NhaCungCapDialog()
        {
            InitializeComponent();
        }

        public NhaCungCapDialog(NhaCungCap ncc) : this()
        {
            _editItem = ncc;
            lblTitle.Text = "✏️ Cập nhật nhà cung cấp";
            txtTenNCC.Text = ncc.TenNCC;
            txtDiaChi.Text = ncc.DiaChi;
            txtDienThoai.Text = ncc.DienThoai;
            txtEmail.Text = ncc.Email;
            chkTrangThai.IsChecked = ncc.TrangThai;
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenNCC.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhà cung cấp!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            KetQua = new NhaCungCap
            {
                MaNCC = _editItem?.MaNCC ?? 0,
                TenNCC = txtTenNCC.Text.Trim(),
                DiaChi = txtDiaChi.Text.Trim(),
                DienThoai = txtDienThoai.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                TrangThai = chkTrangThai.IsChecked == true
            };

            DialogResult = true;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}