using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.Views
{
    // Wrapper thêm TonKho để hiển thị trong DataGrid
    public class ChiTietXuatRow : ChiTietXuat
    {
        public int TonKho { get; set; }
    }

    public partial class PhieuXuatDialog : Window
    {
        public string LyDo { get; private set; }
        public string GhiChu { get; private set; }

        // Trả về ChiTietXuat (không cần TonKho)
        private ObservableCollection<ChiTietXuatRow> _rows = new ObservableCollection<ChiTietXuatRow>();
        public System.Collections.Generic.List<ChiTietXuat> DanhSachChiTiet =>
            _rows.Select(r => new ChiTietXuat
            {
                MaHang = r.MaHang,
                TenHang = r.TenHang,
                SoLuong = r.SoLuong,
                DonGia = r.DonGia
            }).ToList();

        private System.Collections.Generic.List<HangHoa> _danhSachHang;

        public PhieuXuatDialog()
        {
            InitializeComponent();
            dgChiTiet.ItemsSource = _rows;
            NapComboBox();
        }

        private void NapComboBox()
        {
            try
            {
                _danhSachHang = HangHoaDAL.GetAll();
                cboHang.ItemsSource = _danhSachHang;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh mục:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnThemDong_Click(object sender, RoutedEventArgs e)
        {
            if (cboHang.SelectedItem == null)
            { MessageBox.Show("Vui lòng chọn hàng hóa!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!int.TryParse(txtSoLuong.Text, out int sl) || sl <= 0)
            { MessageBox.Show("Số lượng phải lớn hơn 0!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (!decimal.TryParse(txtDonGia.Text, out decimal dg) || dg < 0)
            { MessageBox.Show("Đơn giá không hợp lệ!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            var hang = (HangHoa)cboHang.SelectedItem;

            // Kiểm tra tồn kho trước khi thêm
            if (sl > hang.SoLuongTon)
            {
                MessageBox.Show($"Số lượng xuất ({sl}) vượt quá tồn kho ({hang.SoLuongTon})!",
                    "Không đủ hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _rows.Add(new ChiTietXuatRow
            {
                MaHang = hang.MaHang,
                TenHang = hang.TenHang,
                TonKho = hang.SoLuongTon,
                SoLuong = sl,
                DonGia = dg
            });
            CapNhatTong();

            cboHang.SelectedIndex = -1;
            txtSoLuong.Text = "1";
            txtDonGia.Text = "0";
        }

        private void btnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            if (dgChiTiet.SelectedItem is ChiTietXuatRow row)
            {
                _rows.Remove(row);
                CapNhatTong();
            }
        }

        private void CapNhatTong()
        {
            decimal tong = _rows.Sum(x => x.ThanhTien);
            lblTong.Text = $"Tổng: {tong:N0} ₫";
        }

        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLyDo.Text))
            { MessageBox.Show("Vui lòng nhập lý do xuất!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (_rows.Count == 0)
            { MessageBox.Show("Vui lòng thêm ít nhất 1 mặt hàng!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            LyDo = txtLyDo.Text.Trim();
            GhiChu = txtGhiChu.Text.Trim();
            DialogResult = true;
        }

        private void btnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}