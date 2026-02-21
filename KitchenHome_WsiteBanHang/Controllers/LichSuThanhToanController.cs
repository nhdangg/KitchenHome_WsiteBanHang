using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class LichSuThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public LichSuThanhToanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // /LichSuThanhToan/Index/5
        public async Task<IActionResult> Index(long id)
        {
            int? userId = HttpContext.Session.GetInt32("USER_ID");
            if (userId == null)
                return RedirectToAction("Index", "Home", new { area = "Login_Wsite" });

            var logs = await _context.NhatKyCongThanhToans
                .Include(x => x.ThanhToan)
                    .ThenInclude(t => t.PhuongThuc)
                .Include(x => x.ThanhToan)
                    .ThenInclude(t => t.DonHang)
                .Where(x =>
                    x.ThanhToan.DonHangId == id &&
                    x.ThanhToan.DonHang.TaiKhoanId == userId
                )
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.MaDonHang = id;
            return View(logs);
        }
    }
}
