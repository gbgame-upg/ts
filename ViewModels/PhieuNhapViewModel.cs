using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.ViewModels
{
    public class PhieuNhapViewModel : BaseViewModel
    {
        // ── Danh sách phiếu (Master) ──────────────────────────────────
        private ObservableCollection<PhieuNhap> _danhSachPhieu;
        public ObservableCollection<PhieuNhap> DanhSachPhieu
        {
            get => _danhSachPhieu;
            set => SetProperty(ref _danhSachPhieu, value);
        }

        // ── Phiếu đang chọn ──────────────────────────────────────────
        private PhieuNhap _selectedPhieu;
        public PhieuNhap SelectedPhieu
        {
            get => _selectedPhieu;
            set
            {
                SetProperty(ref _selectedPhieu, value);
                LoadChiTiet(value?.MaPhieuNhap);
            }
        }

        // ── Chi tiết phiếu (Detail) ───────────────────────────────────
        private ObservableCollection<ChiTietNhap> _chiTietPhieu;
        public ObservableCollection<ChiTietNhap> ChiTietPhieu
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
        public PhieuNhapViewModel()
        {
            DanhSachPhieu = new ObservableCollection<PhieuNhap>();
            ChiTietPhieu = new ObservableCollection<ChiTietNhap>();

            LoadCommand = new RelayCommand(_ => LoadDanhSach());
            TaoPhieuCommand = new RelayCommand(_ => TaoPhieu());

            LoadDanhSach();
        }

        // ── Load danh sách phiếu nhập ────────────────────────────────
        private void LoadDanhSach()
        {
            try
            {
                string sql = @"SELECT pn.MaPhieuNhap, pn.MaNCC, ncc.TenNCC,
                                      pn.NgayNhap, pn.TongTien, pn.GhiChu
                               FROM PhieuNhap pn
                               INNER JOIN NhaCungCap ncc ON pn.MaNCC = ncc.MaNCC
                               ORDER BY pn.NgayNhap DESC";

                var dt = DatabaseConnection.GetDataTable(sql);
                DanhSachPhieu.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    DanhSachPhieu.Add(new PhieuNhap
                    {
                        MaPhieuNhap = Convert.ToInt32(row["MaPhieuNhap"]),
                        MaNCC = Convert.ToInt32(row["MaNCC"]),
                        TenNCC = row["TenNCC"].ToString(),
                        NgayNhap = Convert.ToDateTime(row["NgayNhap"]),
                        TongTien = Convert.ToDecimal(row["TongTien"]),
                        GhiChu = row["GhiChu"].ToString()
                    });
                }
                ChiTietPhieu.Clear();
                TongTienPhieu = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải phiếu nhập:\n{ex.Message}", "Lỗi",
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
                string sql = @"SELECT ct.MaCTNhap, ct.MaPhieuNhap, ct.MaHang,
                                      h.TenHang, ct.SoLuong, ct.DonGia, ct.ThanhTien
                               FROM ChiTietNhap ct
                               INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                               WHERE ct.MaPhieuNhap = @MaPN";

                var dt = DatabaseConnection.GetDataTable(sql,
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@MaPN", maPhieu.Value) });

                foreach (DataRow row in dt.Rows)
                {
                    ChiTietPhieu.Add(new ChiTietNhap
                    {
                        MaCTNhap = Convert.ToInt32(row["MaCTNhap"]),
                        MaPhieuNhap = Convert.ToInt32(row["MaPhieuNhap"]),
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

        // ── Tạo phiếu nhập mới ───────────────────────────────────────
        private void TaoPhieu()
        {
            var dialog = new Views.PhieuNhapDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Gọi SP tạo phiếu nhập, lấy MaPhieuNhap output
                    var paramMaNCC = new Microsoft.Data.SqlClient.SqlParameter("@MaNCC", dialog.MaNCC);
                    var paramGhiChu = new Microsoft.Data.SqlClient.SqlParameter("@GhiChu", (object)dialog.GhiChu ?? DBNull.Value);
                    var paramMaPhieu = new Microsoft.Data.SqlClient.SqlParameter("@MaPhieuNhap", System.Data.SqlDbType.Int)
                    { Direction = System.Data.ParameterDirection.Output };

                    using (var conn = DatabaseConnection.GetConnection())
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("sp_ThemPhieuNhap", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(paramMaNCC);
                        cmd.Parameters.Add(paramGhiChu);
                        cmd.Parameters.Add(paramMaPhieu);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }

                    int maPhieuMoi = Convert.ToInt32(paramMaPhieu.Value);

                    // Thêm từng dòng chi tiết
                    foreach (var ct in dialog.DanhSachChiTiet)
                    {
                        DatabaseConnection.ExecuteStoredProcedure("sp_ThemChiTietNhap", new[]
                        {
                            new Microsoft.Data.SqlClient.SqlParameter("@MaPhieuNhap", maPhieuMoi),
                            new Microsoft.Data.SqlClient.SqlParameter("@MaHang",      ct.MaHang),
                            new Microsoft.Data.SqlClient.SqlParameter("@SoLuong",     ct.SoLuong),
                            new Microsoft.Data.SqlClient.SqlParameter("@DonGia",      ct.DonGia),
                        });
                    }

                    MessageBox.Show($"Tạo phiếu nhập #{maPhieuMoi} thành công!\nTồn kho đã được cập nhật tự động.",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDanhSach();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi tạo phiếu nhập:\n{ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}