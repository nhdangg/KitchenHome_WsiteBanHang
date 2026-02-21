using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public ThanhToanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ... (Các hàm Index, Details giữ nguyên) ...
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var query = _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.PhuongThuc)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.MaGiaoDich.Contains(searchString) || t.DonHang.MaDonHang.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(t => t.TrangThai == trangThai);
            }

            query = query.OrderByDescending(t => t.NgayTao);

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = trangThai;

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var thanhToan = await _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.PhuongThuc)
                .Include(t => t.NhatKyCongThanhToans)
                .Include(t => t.HoanTiens)
                .FirstOrDefaultAsync(m => m.ThanhToanId == id);

            if (thanhToan == null) return NotFound();

            return View(thanhToan);
        }

        // ==========================================
        // 3. TẠO THANH TOÁN MỚI (CREATE)
        // ==========================================
        public IActionResult Create()
        {
            ViewData["DonHangId"] = new SelectList(_context.DonHangs, "DonHangId", "MaDonHang");
            ViewData["PhuongThucId"] = new SelectList(_context.PhuongThucThanhToans, "PhuongThucId", "TenPhuongThuc");
            ViewBag.TrangThaiList = GetTrangThaiList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThanhToanId,DonHangId,PhuongThucId,SoTien,TrangThai,MaGiaoDich,NgayThanhToan")] ThanhToan thanhToan)
        {
            // --- [FIX LỖI] Bỏ qua kiểm tra object liên kết, chỉ cần kiểm tra ID ---
            ModelState.Remove("DonHang");
            ModelState.Remove("PhuongThuc");
            // --------------------------------------------------------------------

            if (ModelState.IsValid)
            {
                try
                {
                    thanhToan.NgayTao = DateTime.Now;

                    if (thanhToan.TrangThai == "THANH_CONG" && thanhToan.NgayThanhToan == null)
                    {
                        thanhToan.NgayThanhToan = DateTime.Now;
                    }

                    _context.Add(thanhToan);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Tạo giao dịch mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null) msg += " | Chi tiết: " + ex.InnerException.Message;
                    TempData["Error"] = "Lỗi hệ thống: " + msg;
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);
            }

            ViewData["DonHangId"] = new SelectList(_context.DonHangs, "DonHangId", "MaDonHang", thanhToan.DonHangId);
            ViewData["PhuongThucId"] = new SelectList(_context.PhuongThucThanhToans, "PhuongThucId", "TenPhuongThuc", thanhToan.PhuongThucId);
            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(thanhToan);
        }

        // ==========================================
        // 4. CẬP NHẬT THANH TOÁN (EDIT)
        // ==========================================
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var thanhToan = await _context.ThanhToans.FindAsync(id);
            if (thanhToan == null) return NotFound();

            ViewData["DonHangId"] = new SelectList(_context.DonHangs, "DonHangId", "MaDonHang", thanhToan.DonHangId);
            ViewData["PhuongThucId"] = new SelectList(_context.PhuongThucThanhToans, "PhuongThucId", "TenPhuongThuc", thanhToan.PhuongThucId);
            ViewBag.TrangThaiList = GetTrangThaiList();

            return View(thanhToan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("ThanhToanId,DonHangId,PhuongThucId,SoTien,TrangThai,MaGiaoDich,NgayThanhToan,NgayTao")] ThanhToan thanhToan)
        {
            if (id != thanhToan.ThanhToanId) return NotFound();

            // --- [FIX LỖI] Bỏ qua kiểm tra object liên kết ---
            ModelState.Remove("DonHang");
            ModelState.Remove("PhuongThuc");
            // -----------------------------------------------

            if (ModelState.IsValid)
            {
                try
                {
                    if (thanhToan.TrangThai == "THANH_CONG" && thanhToan.NgayThanhToan == null)
                    {
                        thanhToan.NgayThanhToan = DateTime.Now;
                    }

                    _context.Update(thanhToan);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật trạng thái thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThanhToanExists(thanhToan.ThanhToanId))
                    {
                        TempData["Error"] = "Giao dịch không tồn tại hoặc đã bị xóa.";
                        return NotFound();
                    }
                    else throw;
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    if (ex.InnerException != null) msg += " | Chi tiết: " + ex.InnerException.Message;
                    TempData["Error"] = "Lỗi cập nhật: " + msg;
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join("; ", errors);
            }

            ViewData["DonHangId"] = new SelectList(_context.DonHangs, "DonHangId", "MaDonHang", thanhToan.DonHangId);
            ViewData["PhuongThucId"] = new SelectList(_context.PhuongThucThanhToans, "PhuongThucId", "TenPhuongThuc", thanhToan.PhuongThucId);
            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(thanhToan);
        }

        // ... (Hàm Delete giữ nguyên) ...
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();
            var thanhToan = await _context.ThanhToans
                .Include(t => t.DonHang)
                .Include(t => t.PhuongThuc)
                .FirstOrDefaultAsync(m => m.ThanhToanId == id);
            if (thanhToan == null) return NotFound();
            return View(thanhToan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var thanhToan = await _context.ThanhToans.FindAsync(id);
                if (thanhToan != null)
                {
                    _context.ThanhToans.Remove(thanhToan);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa giao dịch vĩnh viễn.";
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null) msg += " | " + ex.InnerException.Message;
                TempData["Error"] = "Không thể xóa (Có thể do ràng buộc dữ liệu hoàn tiền/nhật ký). Chi tiết: " + msg;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ThanhToanExists(long id)
        {
            return _context.ThanhToans.Any(e => e.ThanhToanId == id);
        }

        private List<SelectListItem> GetTrangThaiList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "CHO_THANH_TOAN", Text = "Chờ thanh toán" },
                new SelectListItem { Value = "THANH_CONG", Text = "Thành công" },
                new SelectListItem { Value = "THAT_BAI", Text = "Thất bại" },
                new SelectListItem { Value = "HUY", Text = "Đã hủy" }
            };
        }
    }
}