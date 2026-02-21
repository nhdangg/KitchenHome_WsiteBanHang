using KitchenHome_WsiteBanHang.Models;

namespace KitchenHome_WsiteBanHang.Models.ViewModels
{
    public class SanPhamIndexViewModel
    {
        // Danh sách sản phẩm đã phân trang
        public List<SanPham> DanhSachSanPham { get; set; } =null!;

        // Dữ liệu cho Sidebar (Bộ lọc)
        public List<DanhMuc> DanhSachDanhMuc { get; set; } = null!;
        public List<ThuongHieu> DanhSachThuongHieu { get; set; } = null!;

        // Các giá trị đang chọn (để giữ trạng thái sau khi reload)
        public int? DanhMucId { get; set; }
        public List<int> ThuongHieuIds { get; set; } = new List<int>();
        public decimal? GiaMin { get; set; }
        public decimal? GiaMax { get; set; }
        public string SortBy { get; set; } = null!; // price_asc, price_desc, new, name
        public string SearchString { get; set; } = null!;

        // Phân trang
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}