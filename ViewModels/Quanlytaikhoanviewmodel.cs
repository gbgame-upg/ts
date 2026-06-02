using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Data;

namespace QLHangTonKho.ViewModels
{
    public class TaiKhoanItem
    {
        public int MaTK { get; set; }
        public string TenDangNhap { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string TenNhom { get; set; }
        public bool TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayDangNhapCuoi { get; set; }
    }

    public class QuanLyTaiKhoanViewModel : BaseViewModel
    {
        private ObservableCollection<TaiKhoanItem> _danhSach;
        public ObservableCollection<TaiKhoanItem> DanhSach
        {
            get => _danhSach;
            set => SetProperty(ref _danhSach, value);
        }

        private TaiKhoanItem _selected;
        public TaiKhoanItem Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand KhoaCommand { get; }
        public ICommand MoKhoaCommand { get; }
        public ICommand DoiAdminCommand { get; }
        public ICommand DoiNhanVienCommand { get; }

        public QuanLyTaiKhoanViewModel()
        {
            DanhSach = new ObservableCollection<TaiKhoanItem>();

            LoadCommand = new RelayCommand(_ => Load());
            KhoaCommand = new RelayCommand(_ => DoiTrangThai(false), _ => Selected != null && Selected.TrangThai);
            MoKhoaCommand = new RelayCommand(_ => DoiTrangThai(true), _ => Selected != null && !Selected.TrangThai);
            DoiAdminCommand = new RelayCommand(_ => DoiNhom(1), _ => Selected != null);
            DoiNhanVienCommand = new RelayCommand(_ => DoiNhom(2), _ => Selected != null);

            Load();
        }

        private void Load()
        {
            try
            {
                var dt = DatabaseConnection.GetDataTableByProc("sp_GetDanhSachTaiKhoan");
                DanhSach.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    DanhSach.Add(new TaiKhoanItem
                    {
                        MaTK = Convert.ToInt32(row["MaTK"]),
                        TenDangNhap = row["TenDangNhap"].ToString(),
                        HoTen = row["HoTen"].ToString(),
                        Email = row["Email"].ToString(),
                        TenNhom = row["TenNhom"].ToString(),
                        TrangThai = Convert.ToBoolean(row["TrangThai"]),
                        NgayTao = Convert.ToDateTime(row["NgayTao"]),
                        NgayDangNhapCuoi = row["NgayDangNhapCuoi"] == DBNull.Value
                                           ? (DateTime?)null
                                           : Convert.ToDateTime(row["NgayDangNhapCuoi"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoiTrangThai(bool trangThai)
        {
            if (Selected == null) return;
            string action = trangThai ? "mở khóa" : "khóa";
            var cf = MessageBox.Show($"Bạn có chắc muốn {action} tài khoản '{Selected.TenDangNhap}'?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (cf != MessageBoxResult.Yes) return;

            try
            {
                DatabaseConnection.ExecuteStoredProcedure("sp_DoiTrangThaiTK", new[]
                {
                    new SqlParameter("@MaTK",      Selected.MaTK),
                    new SqlParameter("@TrangThai", trangThai),
                });
                MessageBox.Show($"Đã {action} tài khoản thành công!", "OK",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DoiNhom(int maNhom)
        {
            if (Selected == null) return;
            string tenNhom = maNhom == 1 ? "Admin" : "Nhân viên";
            var cf = MessageBox.Show($"Đổi '{Selected.TenDangNhap}' thành {tenNhom}?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (cf != MessageBoxResult.Yes) return;

            try
            {
                DatabaseConnection.ExecuteStoredProcedure("sp_DoiNhomQuyen", new[]
                {
                    new SqlParameter("@MaTK",   Selected.MaTK),
                    new SqlParameter("@MaNhom", maNhom),
                });
                MessageBox.Show($"Đã đổi nhóm quyền thành công!", "OK",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}