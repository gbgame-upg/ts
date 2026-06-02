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
    public class BaoCaoViewModel : BaseViewModel
    {
        // ── Bộ lọc ngày ──────────────────────────────────────────────
        private DateTime? _tuNgay;
        public DateTime? TuNgay
        {
            get => _tuNgay;
            set => SetProperty(ref _tuNgay, value);
        }

        private DateTime? _denNgay;
        public DateTime? DenNgay
        {
            get => _denNgay;
            set => SetProperty(ref _denNgay, value);
        }

        // ── Thống kê nhanh (stat cards) ──────────────────────────────
        private int _tongHangHoa;
        public int TongHangHoa
        {
            get => _tongHangHoa;
            set => SetProperty(ref _tongHangHoa, value);
        }

        private decimal _tongNhap;
        public decimal TongNhap
        {
            get => _tongNhap;
            set => SetProperty(ref _tongNhap, value);
        }

        private decimal _tongXuat;
        public decimal TongXuat
        {
            get => _tongXuat;
            set => SetProperty(ref _tongXuat, value);
        }

        private int _soHangSapHet;
        public int SoHangSapHet
        {
            get => _soHangSapHet;
            set => SetProperty(ref _soHangSapHet, value);
        }

        // ── Bảng kết quả (BaoCaoView) ────────────────────────────────
        private string _tieuDeBang = "📊 Chọn loại báo cáo để xem kết quả";
        public string TieuDeBang
        {
            get => _tieuDeBang;
            set => SetProperty(ref _tieuDeBang, value);
        }

        private DataView _ketQua;
        public DataView KetQua
        {
            get => _ketQua;
            set => SetProperty(ref _ketQua, value);
        }

        // ── Danh sách hàng sắp hết (TongQuanView) ────────────────────
        private ObservableCollection<HangHoa> _danhSachSapHet;
        public ObservableCollection<HangHoa> DanhSachSapHet
        {
            get => _danhSachSapHet;
            set => SetProperty(ref _danhSachSapHet, value);
        }

        // ── Commands ──────────────────────────────────────────────────
        public ICommand XemBaoCaoCommand { get; }
        public ICommand XemHangSapHetCommand { get; }
        public ICommand LoadCommand { get; }

        // ── Constructor ───────────────────────────────────────────────
        public BaoCaoViewModel()
        {
            // Mặc định lọc trong tháng hiện tại
            TuNgay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DenNgay = DateTime.Today;

            DanhSachSapHet = new ObservableCollection<HangHoa>();

            XemBaoCaoCommand = new RelayCommand(_ => XemBaoCao());
            XemHangSapHetCommand = new RelayCommand(_ => XemHangSapHet());
            LoadCommand = new RelayCommand(_ => LoadThongKe());

            // Load thống kê ngay khi khởi tạo
            LoadThongKe();
        }

        // ── Load toàn bộ thống kê nhanh ──────────────────────────────
        private void LoadThongKe()
        {
            try
            {
                LoadTongHangHoa();
                LoadTongNhapXuat();
                LoadHangSapHet();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu thống kê:\n{ex.Message}",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Đếm tổng hàng hóa đang hoạt động ────────────────────────
        private void LoadTongHangHoa()
        {
            var result = DatabaseConnection.ExecuteScalar(
                "SELECT COUNT(*) FROM HangHoa WHERE TrangThai = 1");
            TongHangHoa = result != null ? Convert.ToInt32(result) : 0;
        }

        // ── Tổng tiền nhập / xuất (không lọc ngày — tổng toàn thời gian) ──
        private void LoadTongNhapXuat()
        {
            var nhap = DatabaseConnection.ExecuteScalar(
                "SELECT ISNULL(SUM(TongTien), 0) FROM PhieuNhap");
            TongNhap = nhap != null ? Convert.ToDecimal(nhap) : 0;

            var xuat = DatabaseConnection.ExecuteScalar(
                "SELECT ISNULL(SUM(TongTien), 0) FROM PhieuXuat");
            TongXuat = xuat != null ? Convert.ToDecimal(xuat) : 0;
        }

        // ── Tải danh sách hàng sắp hết (dùng sp_HangSapHet) ─────────
        private void LoadHangSapHet()
        {
            var dt = DatabaseConnection.GetDataTableByProc("sp_HangSapHet");

            DanhSachSapHet.Clear();
            SoHangSapHet = dt.Rows.Count;

            foreach (DataRow row in dt.Rows)
            {
                DanhSachSapHet.Add(new HangHoa
                {
                    MaHang = Convert.ToInt32(row["MaHang"]),
                    TenHang = row["TenHang"].ToString(),
                    TenLoai = row["TenLoai"].ToString(),
                    TenDVT = row["TenDVT"].ToString(),
                    SoLuongTon = Convert.ToInt32(row["SoLuongTon"]),
                    MucToiThieu = Convert.ToInt32(row["MucToiThieu"])
                });
            }
        }

        // ── Xem báo cáo nhập/xuất theo khoảng ngày ──────────────────
        private void XemBaoCao()
        {
            if (TuNgay == null || DenNgay == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ ngày bắt đầu và kết thúc.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TuNgay > DenNgay)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Lấy báo cáo nhập + xuất gộp chung trong khoảng ngày
                string sql = @"
                    SELECT 
                        N'Nhập kho'         AS LoaiPhieu,
                        pn.MaPhieuNhap      AS MaPhieu,
                        CONVERT(NVARCHAR,pn.NgayNhap,103) AS NgayThucHien,
                        ncc.TenNCC          AS DoiTac,
                        h.TenHang,
                        ct.SoLuong,
                        ct.DonGia,
                        ct.ThanhTien
                    FROM PhieuNhap pn
                    INNER JOIN NhaCungCap ncc ON pn.MaNCC = ncc.MaNCC
                    INNER JOIN ChiTietNhap ct ON pn.MaPhieuNhap = ct.MaPhieuNhap
                    INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                    WHERE CAST(pn.NgayNhap AS DATE) BETWEEN @TuNgay AND @DenNgay

                    UNION ALL

                    SELECT 
                        N'Xuất kho'         AS LoaiPhieu,
                        px.MaPhieuXuat      AS MaPhieu,
                        CONVERT(NVARCHAR,px.NgayXuat,103) AS NgayThucHien,
                        ISNULL(px.LyDo, N'Không rõ') AS DoiTac,
                        h.TenHang,
                        ct.SoLuong,
                        ct.DonGia,
                        ct.ThanhTien
                    FROM PhieuXuat px
                    INNER JOIN ChiTietXuat ct ON px.MaPhieuXuat = ct.MaPhieuXuat
                    INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                    WHERE CAST(px.NgayXuat AS DATE) BETWEEN @TuNgay AND @DenNgay

                    ORDER BY NgayThucHien DESC";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@TuNgay",  TuNgay.Value.Date),
                    new SqlParameter("@DenNgay", DenNgay.Value.Date)
                };

                var dt = DatabaseConnection.GetDataTable(sql, parameters);
                KetQua = dt.DefaultView;

                TieuDeBang = $"📊 Báo cáo nhập/xuất từ {TuNgay:dd/MM/yyyy} đến {DenNgay:dd/MM/yyyy} — {dt.Rows.Count} dòng";

                // Cập nhật lại tổng nhập xuất theo khoảng ngày đang lọc
                CapNhatTongTheoNgay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy báo cáo:\n{ex.Message}",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Cập nhật stat card TongNhap/TongXuat theo bộ lọc ngày ───
        private void CapNhatTongTheoNgay()
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TuNgay",  TuNgay.Value.Date),
                new SqlParameter("@DenNgay", DenNgay.Value.Date)
            };

            var nhap = DatabaseConnection.ExecuteScalar(
                "SELECT ISNULL(SUM(TongTien),0) FROM PhieuNhap WHERE CAST(NgayNhap AS DATE) BETWEEN @TuNgay AND @DenNgay",
                parameters);
            TongNhap = nhap != null ? Convert.ToDecimal(nhap) : 0;

            var xuat = DatabaseConnection.ExecuteScalar(
                "SELECT ISNULL(SUM(TongTien),0) FROM PhieuXuat WHERE CAST(NgayXuat AS DATE) BETWEEN @TuNgay AND @DenNgay",
                parameters);
            TongXuat = xuat != null ? Convert.ToDecimal(xuat) : 0;
        }

        // ── Xem danh sách hàng sắp hết trong bảng KetQua ────────────
        private void XemHangSapHet()
        {
            try
            {
                var dt = DatabaseConnection.GetDataTableByProc("sp_HangSapHet");
                KetQua = dt.DefaultView;
                TieuDeBang = $"⚠️ Hàng hóa sắp hết tồn kho — {dt.Rows.Count} mặt hàng cần nhập thêm";

                // Cập nhật lại SoHangSapHet và danh sách TongQuanView
                SoHangSapHet = dt.Rows.Count;
                DanhSachSapHet.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    DanhSachSapHet.Add(new HangHoa
                    {
                        MaHang = Convert.ToInt32(row["MaHang"]),
                        TenHang = row["TenHang"].ToString(),
                        TenLoai = row["TenLoai"].ToString(),
                        TenDVT = row["TenDVT"].ToString(),
                        SoLuongTon = Convert.ToInt32(row["SoLuongTon"]),
                        MucToiThieu = Convert.ToInt32(row["MucToiThieu"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lấy danh sách hàng sắp hết:\n{ex.Message}",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}