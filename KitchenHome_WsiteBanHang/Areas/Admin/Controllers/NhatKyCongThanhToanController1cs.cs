using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhatKyCongThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public NhatKyCongThanhToanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ============================
        // 1️⃣ Danh sách toàn bộ log
        // ============================
        public async Task<IActionResult> Index()
        {
            var logs = await _context.NhatKyCongThanhToans
                .Include(x => x.ThanhToan)
                    .ThenInclude(tt => tt.DonHang)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(logs);
        }

        // ==================================
        // 2️⃣ Log theo DonHangID
        // ==================================
        public async Task<IActionResult> TheoDonHang(long donHangId)
        {
            var logs = await _context.NhatKyCongThanhToans
                .Include(x => x.ThanhToan)
                    .ThenInclude(tt => tt.DonHang)
                .Where(x => x.ThanhToan.DonHangId == donHangId)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.DonHangID = donHangId;
            return View("Index", logs);
        }

        // ==================================
        // 3️⃣ Log theo ThanhToanID
        // ==================================
        public async Task<IActionResult> TheoThanhToan(long thanhToanId)
        {
            var logs = await _context.NhatKyCongThanhToans
                .Include(x => x.ThanhToan)
                    .ThenInclude(tt => tt.DonHang)
                .Where(x => x.ThanhToanId == thanhToanId)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.ThanhToanID = thanhToanId;
            return View("Index", logs);
        }

        // ============================
        // 4️⃣ Chi tiết 1 log
        // ============================
        public async Task<IActionResult> Details(long id)
        {
            var log = await _context.NhatKyCongThanhToans
                .Include(x => x.ThanhToan)
                    .ThenInclude(tt => tt.DonHang)
                .FirstOrDefaultAsync(x => x.NhatKyId == id);

            if (log == null)
                return NotFound();

            return View(log);
        }
    }
}
