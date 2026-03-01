using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Quan_Ly")]
    public class DoanhThuController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public DoanhThuController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // GET: Admin/BaoCao/DoanhThuTheoNgay
        public async Task<IActionResult> Index(DateOnly? tuNgay, DateOnly? denNgay)
        {
            var query = _context.VwDoanhThuTheoNgays.AsQueryable();

            if (tuNgay.HasValue)
                query = query.Where(x => x.Ngay >= tuNgay.Value);

            if (denNgay.HasValue)
                query = query.Where(x => x.Ngay <= denNgay.Value);

            var data = await query
                .OrderByDescending(x => x.Ngay)
                .ToListAsync();

            return View(data);
        }
    }
}