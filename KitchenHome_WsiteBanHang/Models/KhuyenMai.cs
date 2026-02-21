using System;
using System.Collections.Generic;

namespace KitchenHome_WsiteBanHang.Models;

public partial class KhuyenMai
{
    public int KhuyenMaiId { get; set; }

    public string MaKhuyenMai { get; set; } = null!;

    public string TenKhuyenMai { get; set; } = null!;

    public string LoaiGiam { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }

    public DateTime BatDau { get; set; }

    public DateTime KetThuc { get; set; }

    public decimal DonHangToiThieu { get; set; }

    public decimal? GiamToiDa { get; set; }

    public bool DangHoatDong { get; set; }
}
