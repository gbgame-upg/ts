using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.ViewModels
{
    public class HangHoaViewModel : BaseViewModel
    {
        // ── Phân quyền ───────────────────────────────────────────────
        public bool CoQuyenSua => UserSession.Current.Quyen_SuaHangHoa;

        // ── Danh sách & lựa chọn ─────────────────────────────────────
        private ObservableCollection<HangHoa> _danhSach;
        public ObservableCollection<HangHoa> DanhSach
        {
            get => _danhSach;
            set => SetProperty(ref _danhSach, value);
        }

        private HangHoa _selected;
        public HangHoa Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        private string _tuKhoa = "";
        public string TuKhoa
        {
            get => _tuKhoa;
            set => SetProperty(ref _tuKhoa, value);
        }

        // ── Commands ──────────────────────────────────────────────────
        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public HangHoaViewModel()
        {
            DanhSach = new ObservableCollection<HangHoa>();

            LoadCommand = new RelayCommand(_ => LoadData());
            SearchCommand = new RelayCommand(_ => Search());
            AddCommand = new RelayCommand(_ => Add(), _ => CoQuyenSua);
            UpdateCommand = new RelayCommand(_ => Update(), _ => CoQuyenSua && Selected != null);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => CoQuyenSua && Selected != null);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var dt = DatabaseConnection.GetDataTableByProc("sp_GetDanhSachHangHoa");
                DanhSach.Clear();
                foreach (DataRow row in dt.Rows) DanhSach.Add(MapRow(row));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Search()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TuKhoa)) { LoadData(); return; }
                var dt = DatabaseConnection.GetDataTableByProc("sp_TimKiemHangHoa",
                    new[] { new Microsoft.Data.SqlClient.SqlParameter("@TuKhoa", TuKhoa) });
                DanhSach.Clear();
                foreach (DataRow row in dt.Rows) DanhSach.Add(MapRow(row));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Add()
        {
            var dialog = new Views.HangHoaDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    HangHoaDAL.Add(dialog.KetQua);
                    MessageBox.Show("Thêm thành công!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void Update()
        {
            if (Selected == null) return;
            var dialog = new Views.HangHoaDialog(Selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    HangHoaDAL.Update(dialog.KetQua);
                    MessageBox.Show("Cập nhật thành công!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void Delete()
        {
            if (Selected == null) return;
            if (MessageBox.Show($"Xóa '{Selected.TenHang}'?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                HangHoaDAL.Delete(Selected.MaHang);
                MessageBox.Show("Đã xóa!", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private HangHoa MapRow(DataRow row) => new HangHoa
        {
            MaHang = Convert.ToInt32(row["MaHang"]),
            TenHang = row["TenHang"].ToString(),
            TenLoai = row["TenLoai"].ToString(),
            TenDVT = row["TenDVT"].ToString(),
            GiaNhap = Convert.ToDecimal(row["GiaNhap"]),
            GiaBan = Convert.ToDecimal(row["GiaBan"]),
            SoLuongTon = Convert.ToInt32(row["SoLuongTon"]),
            MucToiThieu = Convert.ToInt32(row["MucToiThieu"]),
            TrangThaiTon = row["TrangThaiTon"].ToString()
        };
    }
}