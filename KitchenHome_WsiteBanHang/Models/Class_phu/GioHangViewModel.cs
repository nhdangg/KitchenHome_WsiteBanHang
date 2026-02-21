namespace KitchenHome_WsiteBanHang.Models.Class_phu
{
    public class CartItemVM
    {
        public int BienTheID { get; set; }
        public int SanPhamID { get; set; }
        public string TenSanPham { get; set; }
        public string TenBienThe { get; set; }
        public string AnhDaiDien { get; set; }
        public string SKU { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal GiaGoc { get; set; }

        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class GioHangViewModel
    {
        public List<CartItemVM> Items { get; set; } = new List<CartItemVM>();
        public decimal TongTienHang => Items.Sum(x => x.ThanhTien);
        public int TongSoLuong => Items.Sum(x => x.SoLuong);
    }
}
