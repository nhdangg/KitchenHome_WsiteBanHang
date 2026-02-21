using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhatKyHeThongController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public NhatKyHeThongController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ============================
        // INDEX: TÍCH HỢP LỌC & PHÂN TRANG
        // ============================
        public async Task<IActionResult> Index(string search, string actionType, string tableName, DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            int pageSize = 20;
            var query = _context.NhatKyHeThongs
                .Include(n => n.TaiKhoan)
                .AsNoTracking() // Tăng tốc độ truy vấn vì chỉ để xem
                .AsQueryable();

            // Lưu lại giá trị lọc để hiển thị trên View
            ViewBag.Search = search;
            ViewBag.ActionType = actionType;
            ViewBag.TableName = tableName;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            // 1. Tìm kiếm theo khóa bản ghi hoặc tên tài khoản
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n => n.KhoaBanGhi.Contains(search) || n.TaiKhoan.TenDangNhap.Contains(search));
            }

            // 2. Lọc theo loại hành động (INSERT, UPDATE, DELETE...)
            if (!string.IsNullOrEmpty(actionType))
            {
                query = query.Where(n => n.HanhDong == actionType);
            }

            // 3. Lọc theo tên bảng
            if (!string.IsNullOrEmpty(tableName))
            {
                query = query.Where(n => n.TenBang == tableName);
            }

            // 4. Lọc theo thời gian
            if (fromDate.HasValue) query = query.Where(n => n.NgayTao >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(n => n.NgayTao <= toDate.Value.AddDays(1).AddSeconds(-1));

            // --- Xử lý phân trang ---
            var totalItems = await query.CountAsync();
            var data = await query
                .OrderByDescending(n => n.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Lấy danh sách danh mục Table và Action hiện có trong DB để làm Dropdown
            ViewBag.Tables = await _context.NhatKyHeThongs.Select(x => x.TenBang).Distinct().ToListAsync();
            ViewBag.Actions = await _context.NhatKyHeThongs.Select(x => x.HanhDong).Distinct().ToListAsync();

            return View(data);
        }

        // ============================
        // DETAILS: XEM CHI TIẾT DỮ LIỆU
        // ============================
        public async Task<IActionResult> Details(long id)
        {
            var log = await _context.NhatKyHeThongs
                .Include(n => n.TaiKhoan)
                .FirstOrDefaultAsync(n => n.NhatKyId == id);

            if (log == null) return NotFound();

            return View(log);
        }

        // ============================
        // DỌN DẸP LOG CŨ (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanLog(int months = 3)
        {
            try
            {
                var thresholdDate = DateTime.Now.AddMonths(-months);
                var oldLogs = _context.NhatKyHeThongs.Where(n => n.NgayTao < thresholdDate);

                int count = await oldLogs.CountAsync();
                if (count > 0)
                {
                    _context.NhatKyHeThongs.RemoveRange(oldLogs);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã dọn dẹp {count} bản ghi nhật ký cũ hơn {months} tháng.";
                }
                else
                {
                    TempData["Info"] = "Không có nhật ký nào đủ cũ để xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi dọn dẹp: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}