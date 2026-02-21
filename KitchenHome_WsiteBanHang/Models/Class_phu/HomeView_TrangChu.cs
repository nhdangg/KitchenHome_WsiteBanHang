using System.Collections.Generic;
using KitchenHome_WsiteBanHang.Models; // Đảm bảo dòng này có để nhận diện các Model gốc

namespace KitchenHome_WsiteBanHang.Models.Class_phu
{
    public class HomeView_TrangChu
    {
        // --- 1. CÁC THÀNH PHẦN CŨ (Giữ nguyên) ---
        public List<Banner> DanhSachBanner { get; set; } = new List<Banner>();
        public List<SanPham> DanhSachSanPham { get; set; } = new List<SanPham>(); // Sản phẩm nổi bật
        public List<SanPham> DanhSachGiamGia { get; set; } = new List<SanPham>(); // Sản phẩm đang Sale
        public Dictionary<int, RatingInfo> Ratings { get; set; } = new Dictionary<int, RatingInfo>();

        // --- 2. CÁC THÀNH PHẦN MỚI (Bổ sung) ---

        // Hiển thị danh sách các nhóm hàng (Nồi, Chảo, Dao...) ở đầu trang
        public List<DanhMuc> DanhMucNoiBat { get; set; } = new List<DanhMuc>();

        // Hiển thị Logo các đối tác (Sunhouse, Elmich...) chạy slider
        public List<ThuongHieu> ThuongHieuDoiTac { get; set; } = new List<ThuongHieu>();

        // Hiển thị 3-4 bài viết mẹo vặt mới nhất ở cuối trang
        public List<TrangNoiDung> BaiVietMoi { get; set; } = new List<TrangNoiDung>();

        // Hiển thị các đánh giá 5 sao chất lượng (Testimonials)
        public List<DanhGiaSanPham> DanhGiaTieuBieu { get; set; } = new List<DanhGiaSanPham>();

        public List<SanPham> SanPhamBanChay { get; set; } = new List<SanPham>();
    }

    public class RatingInfo
    {
        public double AvgStar { get; set; }
        public int ReviewCount { get; set; }
    }
}