using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models.Context;


namespace KitchenHome_WsiteBanHang.Areas.ThuKho.Controllers
{
    [Area("Thu_Kho")]
    public class HomeController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public HomeController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ThuKhoDashboardViewModel();

            model.TongSanPham = await _context.SanPhams.CountAsync();
            model.TongBienThe = await _context.BienTheSanPhams.CountAsync();
            model.TongKho = await _context.Khos.CountAsync();

            model.TongSoLuongTon = await _context.TonKhos.SumAsync(x => (int?)x.SoLuongTon) ?? 0;
            model.TongSoLuongGiuCho = await _context.TonKhos.SumAsync(x => (int?)x.SoLuongGiuCho) ?? 0;

            model.DanhSachKho = await _context.Khos
                .Select(k => new KhoThongKeViewModel
                {
                    KhoID = k.KhoId,
                    TenKho = k.TenKho,
                    SoBienThe = _context.TonKhos.Count(t => t.KhoId == k.KhoId),
                    TongTon = _context.TonKhos
                        .Where(t => t.KhoId == k.KhoId)
                        .Sum(t => (int?)t.SoLuongTon) ?? 0,
                    TongGiuCho = _context.TonKhos
                        .Where(t => t.KhoId == k.KhoId)
                        .Sum(t => (int?)t.SoLuongGiuCho) ?? 0
                })
                .ToListAsync();

            return View(model);
        }
    }
}