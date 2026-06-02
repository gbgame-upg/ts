using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.ViewModels
{
    public class PhieuXuatViewModel : BaseViewModel
    {
        // ── Danh sách phiếu (Master) ──────────────────────────────────
        private ObservableCollection<PhieuXuat> _danhSachPhieu;
        public ObservableCollection<PhieuXuat> DanhSachPhieu
        {
            get => _danhSachPhieu;
            set => SetProperty(ref _danhSachPhieu, value);
        }

        // ── Phiếu đang chọn ──────────────────────────────────────────
        private PhieuXuat _selectedPhieu;
        public PhieuXuat SelectedPhieu
        {
            get => _selectedPhieu;
            set
            {
                SetProperty(ref _selectedPhieu, value);
                LoadChiTiet(value?.MaPhieuXuat);
            }
        }

        // ── Chi tiết phiếu (Detail) ───────────────────────────────────
        private ObservableCollection<ChiTietXuat> _chiTietPhieu;
        public ObservableCollection<ChiTietXuat> ChiTietPhieu
        {
            get => _chiTietPhieu;
            set => SetProperty(ref _chiTietPhieu, value);
        }

        // ── Tổng tiền phiếu đang chọn ────────────────────────────────
        private decimal _tongTienPhieu;
        public decimal TongTienPhieu
        {
            get => _tongTienPhieu;
            set => SetProperty(ref _tongTienPhieu, value);
        }

        // ── Commands ──────────────────────────────────────────────────
        public ICommand LoadCommand { get; }
        public ICommand TaoPhieuCommand { get; }

        // ── Constructor ───────────────────────────────────────────────
        public PhieuXuatViewModel()
        {
            DanhSachPhieu = new ObservableCollection<PhieuXuat>();
            ChiTietPhieu = new ObservableCollection<ChiTietXuat>();

            LoadCommand = new RelayCommand(_ => LoadDanhSach());
            TaoPhieuCommand = new RelayCommand(_ => TaoPhieu());

            LoadDanhSach();
        }

        // ── Load danh sách phiếu xuất ────────────────────────────────
        private void LoadDanhSach()
        {
            try
            {
                string sql = @"SELECT MaPhieuXuat, NgayXuat, LyDo, TongTien, GhiChu
                               FROM PhieuXuat ORDER BY NgayXuat DESC";

                var dt = DatabaseConnection.GetDataTable(sql);
                DanhSachPhieu.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    DanhSachPhieu.Add(new PhieuXuat
                    {
                        MaPhieuXuat = Convert.ToInt32(row["MaPhieuXuat"]),
                        NgayXuat = Convert.ToDateTime(row["NgayXuat"]),
                        LyDo = row["LyDo"].ToString(),
                        TongTien = Convert.ToDecimal(row["TongTien"]),
                        GhiChu = row["GhiChu"].ToString()
                    });
                }
                ChiTietPhieu.Clear();
                TongTienPhieu = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải phiếu xuất:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Load chi tiết khi chọn phiếu ────────────────────────────
        private void LoadChiTiet(int? maPhieu)
        {
            ChiTietPhieu.Clear();
            TongTienPhieu = 0;
            if (maPhieu == null) return;

            try
            {
                string sql = @"SELECT ct.MaCTXuat, ct.MaPhieuXuat, ct.MaHang,
                                      h.TenHang, ct.SoLuong, ct.DonGia, ct.ThanhTien
                               FROM ChiTietXuat ct
                               INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                               WHERE ct.MaPhieuXuat = @MaPX";

                var dt = DatabaseConnection.GetDataTable(sql,
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@MaPX", maPhieu.Value) });

                foreach (DataRow row in dt.Rows)
                {
                    ChiTietPhieu.Add(new ChiTietXuat
                    {
                        MaCTXuat = Convert.ToInt32(row["MaCTXuat"]),
                        MaPhieuXuat = Convert.ToInt32(row["MaPhieuXuat"]),
                        MaHang = Convert.ToInt32(row["MaHang"]),
                        TenHang = row["TenHang"].ToString(),
                        SoLuong = Convert.ToInt32(row["SoLuong"]),
                        DonGia = Convert.ToDecimal(row["DonGia"]),
                    });
                }

                TongTienPhieu = SelectedPhieu?.TongTien ?? 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết phiếu:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Tạo phiếu xuất mới ───────────────────────────────────────
        private void TaoPhieu()
        {
            var dialog = new Views.PhieuXuatDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var paramLyDo = new Microsoft.Data.SqlClient.SqlParameter("@LyDo", (object)dialog.LyDo ?? DBNull.Value);
                    var paramGhiChu = new Microsoft.Data.SqlClient.SqlParameter("@GhiChu", (object)dialog.GhiChu ?? DBNull.Value);
                    var paramMaPhieu = new Microsoft.Data.SqlClient.SqlParameter("@MaPhieuXuat", System.Data.SqlDbType.Int)
                    { Direction = System.Data.ParameterDirection.Output };

                    using (var conn = DatabaseConnection.GetConnection())
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_ThemPhieuXuat", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(paramLyDo);
                        cmd.Parameters.Add(paramGhiChu);
                        cmd.Parameters.Add(paramMaPhieu);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    int maPhieuMoi = Convert.ToInt32(paramMaPhieu.Value);

                    foreach (var ct in dialog.DanhSachChiTiet)
                    {
                        DatabaseConnection.ExecuteStoredProcedure("sp_ThemChiTietXuat", new[]
                        {
                            new Microsoft.Data.SqlClient.SqlParameter("@MaPhieuXuat", maPhieuMoi),
                            new Microsoft.Data.SqlClient.SqlParameter("@MaHang",      ct.MaHang),
                            new Microsoft.Data.SqlClient.SqlParameter("@SoLuong",     ct.SoLuong),
                            new Microsoft.Data.SqlClient.SqlParameter("@DonGia",      ct.DonGia),
                        });
                    }

                    MessageBox.Show($"Tạo phiếu xuất #{maPhieuMoi} thành công!\nTồn kho đã được cập nhật tự động.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDanhSach();
                }
                catch (Exception ex)
                {
                    // Trigger SQL sẽ throw lỗi nếu không đủ hàng
                    MessageBox.Show($"Lỗi tạo phiếu xuất:\n{ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}