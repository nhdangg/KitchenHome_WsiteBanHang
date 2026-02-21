using System;
using System.Collections.Generic;

namespace KitchenHome_WsiteBanHang.ViewModels
{
    public class BoSuuTapItemVM
    {
        public int SanPhamID { get; set; }
        public string TenSanPham { get; set; }
        public string Slug { get; set; }
        public string AnhDaiDien { get; set; }

        public decimal GiaBan { get; set; }
        public decimal? GiaKhuyenMai { get; set; }

        public bool DangKhuyenMai => GiaKhuyenMai.HasValue && GiaKhuyenMai < GiaBan;
    }

    public class BoSuuTapViewModel
    {
        public string TieuDe { get; set; }
        public string KieuBoSuuTap { get; set; } // noi-bat | ban-chay | khuyen-mai | moi

        public IEnumerable<BoSuuTapItemVM> SanPhams { get; set; }

        // Phân trang
        public int TrangHienTai { get; set; }
        public int TongTrang { get; set; }
        public int PageSize { get; set; }
    }
}
