using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Thu_Ngan.Controllers
{
    [Area("Thu_Ngan")]
    public class HomeController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public HomeController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // Tổng đơn hôm nay
            var soDonHomNay = await _context.DonHangs
                .Where(x => x.NgayDat.Date == today)
                .CountAsync();

            // Doanh thu hôm nay
            var doanhThuHomNay = await _context.DonHangs
                .Where(x => x.NgayDat.Date == today &&
                       (x.TrangThai == "HOAN_TAT" || x.TrangThai == "DA_GIAO"))
                .SumAsync(x => (decimal?)x.TongTien) ?? 0;

            // Đơn chờ xác nhận
            var donChoXacNhan = await _context.DonHangs
                .Where(x => x.TrangThai == "CHO_XAC_NHAN")
                .CountAsync();

            // Top 5 bán chạy
            var topBanChay = await _context.VwTopBanChay
                .Take(5)
                .ToListAsync();

            // Tổng doanh thu toàn hệ thống
            var tongDoanhThu = await _context.DonHangs
                .Where(x => x.TrangThai == "HOAN_TAT" || x.TrangThai == "DA_GIAO")
                .SumAsync(x => (decimal?)x.TongTien) ?? 0;


            ViewBag.SoDonHomNay = soDonHomNay;
            ViewBag.DoanhThuHomNay = doanhThuHomNay;
            ViewBag.DonChoXacNhan = donChoXacNhan;
            ViewBag.TopBanChay = topBanChay;
            ViewBag.TongDoanhThu = tongDoanhThu;

            return View();
        }
    }
}