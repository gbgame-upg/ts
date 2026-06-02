using System;
using System.Data;
using System.Windows;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.Views
{
    public partial class HangHoaDialog : Window
    {
        public HangHoa KetQua { get; private set; }
        private readonly HangHoa _editItem;

        // ── Constructor: Thêm mới ──────────────────────────────────
        public HangHoaDialog()
        {
            InitializeComponent();
            NapComboBox();
        }

        // ── Constructor: Sửa ──────────────────────────────────────
        public HangHoaDialog(HangHoa hangHoa) : this()
        {
            _editItem = hangHoa;
            lblTitle.Text = "✏️ Cập nhật hàng hóa";
            txtTenHang.Text = hangHoa.TenHang;
            txtGiaNhap.Text = hangHoa.GiaNhap.ToString();
            txtGiaBan.Text = hangHoa.GiaBan.ToString();
            txtMucToiThieu.Text = hangHoa.MucToiThieu.ToString();
            txtMoTa.Text = hangHoa.MoTa;
            cboLoai.SelectedValue = hangHoa.MaLoai;
            cboDVT.SelectedValue = hangHoa.MaDVT;
        }

        // ── Nạp dữ liệu ComboBox ──────────────────────────────────
        private void NapComboBox()
        {
            try
            {
                cboLoai.ItemsSource = HangHoaDAL.GetDanhSachLoai();
                var dtDVT = HangHoaDAL.GetDanhSachDVT();
                cboDVT.ItemsSource = dtDVT.DefaultView;
                cboDVT.DisplayMemberPath = "TenDVT";
                cboDVT.SelectedValuePath = "MaDVT";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Lưu ───────────────────────────────────────────────────
        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtTenHang.Text))
            { MessageBox.Show("Vui lòng nhập tên hàng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (cboLoai.SelectedValue == null)
            { MessageBox.Show("Vui lòng chọn loại hàng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (cboDVT.SelectedValue == null)
            { MessageBox.Show("Vui lòng chọn đơn vị tính!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!decimal.TryParse(txtGiaNhap.Text, out decimal giaNhap) || giaNhap < 0)
            { MessageBox.Show("Giá nhập không hợp lệ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!decimal.TryParse(txtGiaBan.Text, out decimal giaBan) || giaBan < 0)
            { MessageBox.Show("Giá bán không hợp lệ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!int.TryParse(txtMucToiThieu.Text, out int mucToiThieu) || mucToiThieu < 0)
            { MessageBox.Show("Mức tối thiểu không hợp lệ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            KetQua = new HangHoa
            {
                MaHang = _editItem?.MaHang ?? 0,
                TenHang = txtTenHang.Text.Trim(),
                MaLoai = Convert.ToInt32(cboLoai.SelectedValue),
                MaDVT = Convert.ToInt32(cboDVT.SelectedValue),
                GiaNhap = giaNhap,
                GiaBan = giaBan,
                MucToiThieu = mucToiThieu,
                SoLuongTon = _editItem?.SoLuongTon ?? 0,
                MoTa = txtMoTa.Text.Trim()
            };

            DialogResult = true;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}