public class ThuKhoDashboardViewModel
{
    public int TongSanPham { get; set; }
    public int TongBienThe { get; set; }
    public int TongKho { get; set; }
    public int TongSoLuongTon { get; set; }
    public int TongSoLuongGiuCho { get; set; }

    public List<KhoThongKeViewModel> DanhSachKho { get; set; }
}

public class KhoThongKeViewModel
{
    public int KhoID { get; set; }
    public string TenKho { get; set; }
    public int SoBienThe { get; set; }
    public int TongTon { get; set; }
    public int TongGiuCho { get; set; }
}