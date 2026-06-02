using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using QLHangTonKho.Data;
using QLHangTonKho.Models;

namespace QLHangTonKho.ViewModels
{
    public class NhaCungCapViewModel : BaseViewModel
    {
        // ── Danh sách & lựa chọn ─────────────────────────────────────
        private ObservableCollection<NhaCungCap> _danhSach;
        public ObservableCollection<NhaCungCap> DanhSach
        {
            get => _danhSach;
            set => SetProperty(ref _danhSach, value);
        }

        private NhaCungCap _selected;
        public NhaCungCap Selected
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

        // ── Constructor ───────────────────────────────────────────────
        public NhaCungCapViewModel()
        {
            DanhSach = new ObservableCollection<NhaCungCap>();

            LoadCommand = new RelayCommand(_ => LoadData());
            SearchCommand = new RelayCommand(_ => Search());
            AddCommand = new RelayCommand(_ => Add());
            UpdateCommand = new RelayCommand(_ => Update(), _ => Selected != null);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => Selected != null);

            LoadData();
        }

        // ── Load ─────────────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                var dt = DatabaseConnection.GetDataTable(
                    "SELECT MaNCC, TenNCC, DiaChi, DienThoai, Email, TrangThai FROM NhaCungCap ORDER BY MaNCC");
                DanhSach.Clear();
                foreach (DataRow row in dt.Rows)
                    DanhSach.Add(MapRow(row));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Tìm kiếm ─────────────────────────────────────────────────
        private void Search()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TuKhoa)) { LoadData(); return; }

                var dt = DatabaseConnection.GetDataTable(
                    "SELECT MaNCC, TenNCC, DiaChi, DienThoai, Email, TrangThai FROM NhaCungCap WHERE TenNCC LIKE @TuKhoa OR DienThoai LIKE @TuKhoa OR Email LIKE @TuKhoa",
                    new[] { new SqlParameter("@TuKhoa", $"%{TuKhoa}%") });

                DanhSach.Clear();
                foreach (DataRow row in dt.Rows)
                    DanhSach.Add(MapRow(row));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Thêm mới ─────────────────────────────────────────────────
        private void Add()
        {
            var dialog = new Views.NhaCungCapDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var n = dialog.KetQua;
                    DatabaseConnection.ExecuteNonQuery(
                        "INSERT INTO NhaCungCap (TenNCC, DiaChi, DienThoai, Email, TrangThai) VALUES (@TenNCC, @DiaChi, @DienThoai, @Email, 1)",
                        new[]
                        {
                            new SqlParameter("@TenNCC",    n.TenNCC),
                            new SqlParameter("@DiaChi",    (object)n.DiaChi    ?? DBNull.Value),
                            new SqlParameter("@DienThoai", (object)n.DienThoai ?? DBNull.Value),
                            new SqlParameter("@Email",     (object)n.Email     ?? DBNull.Value),
                        });
                    MessageBox.Show("Thêm nhà cung cấp thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi thêm mới:\n{ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Cập nhật ─────────────────────────────────────────────────
        private void Update()
        {
            if (Selected == null) return;
            var dialog = new Views.NhaCungCapDialog(Selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var n = dialog.KetQua;
                    DatabaseConnection.ExecuteNonQuery(
                        "UPDATE NhaCungCap SET TenNCC=@TenNCC, DiaChi=@DiaChi, DienThoai=@DienThoai, Email=@Email, TrangThai=@TrangThai WHERE MaNCC=@MaNCC",
                        new[]
                        {
                            new SqlParameter("@TenNCC",    n.TenNCC),
                            new SqlParameter("@DiaChi",    (object)n.DiaChi    ?? DBNull.Value),
                            new SqlParameter("@DienThoai", (object)n.DienThoai ?? DBNull.Value),
                            new SqlParameter("@Email",     (object)n.Email     ?? DBNull.Value),
                            new SqlParameter("@TrangThai", n.TrangThai),
                            new SqlParameter("@MaNCC",     n.MaNCC),
                        });
                    MessageBox.Show("Cập nhật thành công!", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi cập nhật:\n{ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ── Xóa (soft delete) ────────────────────────────────────────
        private void Delete()
        {
            if (Selected == null) return;
            var confirm = MessageBox.Show(
                $"Xóa nhà cung cấp '{Selected.TenNCC}'?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                DatabaseConnection.ExecuteNonQuery(
                    "UPDATE NhaCungCap SET TrangThai = 0 WHERE MaNCC = @MaNCC",
                    new[] { new SqlParameter("@MaNCC", Selected.MaNCC) });
                MessageBox.Show("Đã xóa nhà cung cấp!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xóa:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Map DataRow → NhaCungCap ──────────────────────────────────
        private NhaCungCap MapRow(DataRow row) => new NhaCungCap
        {
            MaNCC = Convert.ToInt32(row["MaNCC"]),
            TenNCC = row["TenNCC"].ToString(),
            DiaChi = row["DiaChi"].ToString(),
            DienThoai = row["DienThoai"].ToString(),
            Email = row["Email"].ToString(),
            TrangThai = Convert.ToBoolean(row["TrangThai"])
        };
    }
}