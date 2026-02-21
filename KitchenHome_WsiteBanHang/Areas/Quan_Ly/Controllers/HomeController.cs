using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Areas.Quan_Ly.Models;

namespace KitchenHome_WsiteBanHang.Areas.Quan_Ly.Controllers
{
    [Area("Quan_Ly")]
    public class HomeController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public HomeController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            // Tổng đơn
            model.TongDonHang = await _context.DonHangs.CountAsync();

            // Tổng khách hàng
            model.TongKhachHang = await _context.KhachHangs.CountAsync();

            // Tổng sản phẩm
            model.TongSanPham = await _context.SanPhams.CountAsync();

            // Tổng doanh thu (đơn đã hoàn tất hoặc đã giao)
            model.TongDoanhThu = await _context.DonHangs
                .Where(x => x.TrangThai == "HOAN_TAT" || x.TrangThai == "DA_GIAO")
                .SumAsync(x => (decimal?)x.TongTien) ?? 0;

            // Top 5 sản phẩm bán chạy
            model.TopSanPham = await _context.ChiTietDonHangs
                .Include(x => x.BienThe)
                .ThenInclude(bt => bt.SanPham)
                .Where(x => x.DonHang.TrangThai == "HOAN_TAT" || x.DonHang.TrangThai == "DA_GIAO")
                .GroupBy(x => x.BienThe.SanPham.TenSanPham)
                .Select(g => new TopSanPhamViewModel
                {
                    TenSanPham = g.Key,
                    TongSoLuong = g.Sum(x => x.SoLuong),
                    TongTien = g.Sum(x => x.ThanhTien ?? 0)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .Take(5)
                .ToListAsync();

            // Doanh thu 7 ngày gần nhất
            var last7Days = DateTime.Now.Date.AddDays(-6);

            model.DoanhThu7Ngay = await _context.DonHangs
                .Where(x => (x.TrangThai == "HOAN_TAT" || x.TrangThai == "DA_GIAO")
                            && x.NgayDat >= last7Days)
                .GroupBy(x => x.NgayDat.Date)
                .Select(g => new DoanhThuNgayViewModel
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderBy(x => x.Ngay)
                .ToListAsync();

            return View(model);
        }
    }
}