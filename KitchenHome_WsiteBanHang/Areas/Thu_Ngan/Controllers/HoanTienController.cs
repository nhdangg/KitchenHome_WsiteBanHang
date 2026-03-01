using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Thu_Ngan.Controllers
{
    [Area("Thu_Ngan")]
    public class HoanTienController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public HoanTienController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH
        // ==========================================
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var query = _context.HoanTiens
                .Include(h => h.ThanhToan)
                    .ThenInclude(t => t.DonHang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(h =>
                    h.ThanhToan.DonHang.MaDonHang.Contains(searchString) ||
                    h.LyDo.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            query = query.OrderByDescending(h => h.NgayTao);

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = trangThai;

            return View(await query.ToListAsync());
        }

        // ==========================================
        // 2. CHI TIẾT
        // ==========================================
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                    .ThenInclude(t => t.DonHang)
                .Include(h => h.ThanhToan)
                    .ThenInclude(t => t.PhuongThuc)
                .FirstOrDefaultAsync(h => h.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            return View(hoanTien);
        }


        // ==========================================
        // 3. CHỈNH SỬA (CHỈ KHI CHƯA HOÀN)
        // ==========================================
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                    .ThenInclude(t => t.DonHang)
                .FirstOrDefaultAsync(h => h.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            ViewData["ThanhToanInfo"] =
                $"{hoanTien.ThanhToan.DonHang.MaDonHang} - {hoanTien.ThanhToan.SoTien:N0} VNĐ";

            return View(hoanTien);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, HoanTien model)
        {
            if (id != model.HoanTienId)
                return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                .FirstOrDefaultAsync(h => h.HoanTienId == id);

            if (hoanTien == null)
                return NotFound();

            // 🚨 CHẶN SỬA KHI ĐÃ HOÀN
            if (hoanTien.TrangThai == "DA_HOAN")
            {
                TempData["Error"] = "Yêu cầu đã hoàn tiền, không thể chỉnh sửa.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            if (ModelState.IsValid)
            {
                // Không cho sửa ThanhToanId, NgayTao, TrangThai
                hoanTien.SoTienHoan = model.SoTienHoan;
                hoanTien.LyDo = model.LyDo;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ThanhToanInfo"] =
                $"{hoanTien.ThanhToan?.DonHang?.MaDonHang} - {hoanTien.ThanhToan?.SoTien:N0} VNĐ";

            return View(model);
        }

        // ==========================================
        // 4. XÁC NHẬN ĐÃ HOÀN
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> XacNhanHoan(long id)
        {
            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                .ThenInclude(t => t.HoanTiens)
                .FirstOrDefaultAsync(h => h.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            if (hoanTien.TrangThai != "CHO_HOAN")
                return BadRequest("Yêu cầu không hợp lệ.");

            hoanTien.TrangThai = "DA_HOAN";

            var thanhToan = hoanTien.ThanhToan;

            decimal tongDaHoan = thanhToan.HoanTiens
                .Where(h => h.TrangThai == "DA_HOAN" || h.HoanTienId == id)
                .Sum(h => h.SoTienHoan);

            if (tongDaHoan >= thanhToan.SoTien)
            {
                thanhToan.TrangThai = "DA_HOAN_TOAN_BO";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xác nhận hoàn tiền!";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. XÓA (CHỈ KHI CHƯA HOÀN)
        // ==========================================
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                .ThenInclude(t => t.DonHang)
                .FirstOrDefaultAsync(h => h.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            if (hoanTien.TrangThai == "DA_HOAN")
            {
                TempData["Error"] = "Không thể xóa yêu cầu đã hoàn.";
                return RedirectToAction(nameof(Index));
            }

            return View(hoanTien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var hoanTien = await _context.HoanTiens.FindAsync(id);

            if (hoanTien != null && hoanTien.TrangThai != "DA_HOAN")
            {
                _context.HoanTiens.Remove(hoanTien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa yêu cầu hoàn tiền.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // HELPER
        // ==========================================
        private async Task ReloadSelectList(long? selectedId = null)
        {
            var list = await _context.ThanhToans
                .Include(t => t.DonHang)
                .Where(t => t.TrangThai == "THANH_CONG")
                .Select(t => new
                {
                    t.ThanhToanId,
                    Display = $"{t.DonHang.MaDonHang} - {t.SoTien:N0} VNĐ"
                }).ToListAsync();

            ViewData["ThanhToanId"] = new SelectList(list, "ThanhToanId", "Display", selectedId);
            ViewBag.TrangThaiList = GetTrangThaiList();
        }

        private List<SelectListItem> GetTrangThaiList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "CHO_HOAN", Text = "Chờ hoàn tiền" },
                new SelectListItem { Value = "DA_HOAN", Text = "Đã hoàn tiền" },
                new SelectListItem { Value = "THAT_BAI", Text = "Hoàn tiền thất bại" }
            };
        }
    }
}