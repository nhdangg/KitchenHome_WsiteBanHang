using System.Collections.Generic;
using KitchenHome_WsiteBanHang.Models;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Models
{
    public class VaiTroDetailVM
    {
        public VaiTro VaiTro { get; set; } = null!;

        public List<TaiKhoan> TaiKhoans { get; set; } = new();

        public int TongTaiKhoan => TaiKhoans.Count;
    }
}
