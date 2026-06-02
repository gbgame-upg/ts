using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.Views
{
    public partial class PhieuNhapDialog : Window
    {
        // ── Output cho ViewModel ──────────────────────────────────────
        public int MaNCC { get; private set; }
        public string GhiChu { get; private set; }
        public ObservableCollection<ChiTietNhap> DanhSachChiTiet { get; }
            = new ObservableCollection<ChiTietNhap>();

        public PhieuNhapDialog()
        {
            InitializeComponent();
            dgChiTiet.ItemsSource = DanhSachChiTiet;
            NapComboBox();
        }

        // ── Nạp ComboBox ─────────────────────────────────────────────
        private void NapComboBox()
        {
            try
            {
                cboNCC.ItemsSource = NhaCungCapDAL.GetActive();
                cboHang.ItemsSource = HangHoaDAL.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Thêm dòng chi tiết ───────────────────────────────────────
        private void btnThemDong_Click(object sender, RoutedEventArgs e)
        {
            if (cboHang.SelectedItem == null)
            { MessageBox.Show("Vui lòng chọn hàng hóa!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!int.TryParse(txtSoLuong.Text, out int sl) || sl <= 0)
            { MessageBox.Show("Số lượng phải lớn hơn 0!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!decimal.TryParse(txtDonGia.Text, out decimal dg) || dg < 0)
            { MessageBox.Show("Đơn giá không hợp lệ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            var hang = (HangHoa)cboHang.SelectedItem;
            DanhSachChiTiet.Add(new ChiTietNhap
            {
                MaHang = hang.MaHang,
                TenHang = hang.TenHang,
                SoLuong = sl,
                DonGia = dg
            });
            CapNhatTong();

            // Reset fields
            cboHang.SelectedIndex = -1;
            txtSoLuong.Text = "1";
            txtDonGia.Text = "0";
        }

        // ── Xóa dòng đã chọn ─────────────────────────────────────────
        private void btnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (dgChiTiet.SelectedItem is ChiTietNhap ct)
            {
                DanhSachChiTiet.Remove(ct);
                CapNhatTong();
            }
        }

        // ── Cập nhật label tổng tiền ──────────────────────────────────
        private void CapNhatTong()
        {
            decimal tong = DanhSachChiTiet.Sum(x => x.ThanhTien);
            lblTong.Text = $"Tổng: {tong:N0} ₫";
        }

        // ── Lưu ───────────────────────────────────────────────────────
        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboNCC.SelectedValue == null)
            { MessageBox.Show("Vui lòng chọn nhà cung cấp!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (DanhSachChiTiet.Count == 0)
            { MessageBox.Show("Vui lòng thêm ít nhất 1 mặt hàng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            MaNCC = Convert.ToInt32(cboNCC.SelectedValue);
            GhiChu = txtGhiChu.Text.Trim();
            DialogResult = true;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}