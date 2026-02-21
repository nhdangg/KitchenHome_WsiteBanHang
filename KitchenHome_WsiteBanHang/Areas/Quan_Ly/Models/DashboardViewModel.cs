namespace KitchenHome_WsiteBanHang.Areas.Quan_Ly.Models
{
    public class DashboardViewModel
    {
        public int TongDonHang { get; set; }
        public int TongKhachHang { get; set; }
        public int TongSanPham { get; set; }
        public decimal TongDoanhThu { get; set; }

        public List<TopSanPhamViewModel> TopSanPham { get; set; }
        public List<DoanhThuNgayViewModel> DoanhThu7Ngay { get; set; }
    }

    public class TopSanPhamViewModel
    {
        public string TenSanPham { get; set; }
        public int TongSoLuong { get; set; }
        public decimal TongTien { get; set; }
    }

    public class DoanhThuNgayViewModel
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
    }
}