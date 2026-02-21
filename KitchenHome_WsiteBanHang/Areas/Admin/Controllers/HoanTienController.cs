using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context; // Thay đổi namespace này theo project thực tế của bạn

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoanTienController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public HoanTienController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DANH SÁCH HOÀN TIỀN
        // ==========================================
        public async Task<IActionResult> Index(string searchString, string trangThai)
        {
            var query = _context.HoanTiens
                .Include(h => h.ThanhToan)
                .ThenInclude(tt => tt.DonHang) // Include sâu vào Đơn hàng để lấy Mã Đơn
                .AsQueryable();

            // Tìm kiếm theo Mã đơn hàng hoặc Lý do hoàn tiền
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(h => h.ThanhToan.DonHang.MaDonHang.Contains(searchString) || h.LyDo.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            // Sắp xếp: Mới nhất lên đầu
            query = query.OrderByDescending(h => h.NgayTao);

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentStatus = trangThai;

            return View(await query.ToListAsync());
        }

        // ==========================================
        // 2. CHI TIẾT HOÀN TIỀN
        // ==========================================
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                .ThenInclude(tt => tt.DonHang)
                .Include(h => h.ThanhToan)
                .ThenInclude(tt => tt.PhuongThuc)
                .FirstOrDefaultAsync(m => m.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            return View(hoanTien);
        }

        // ==========================================
        // 3. TẠO YÊU CẦU HOÀN TIỀN
        // ==========================================
        public IActionResult Create()
        {
            // Chỉ load những thanh toán đã THÀNH CÔNG để hoàn tiền
            // Tạo SelectList hiển thị: "Mã Đơn - Số Tiền Gốc" để dễ chọn
            var thanhToanThanhCong = _context.ThanhToans
                .Include(tt => tt.DonHang)
                .Where(tt => tt.TrangThai == "THANH_CONG")
                .Select(tt => new {
                    tt.ThanhToanId,
                    DisplayInfo = $"{tt.DonHang.MaDonHang} - {tt.SoTien:N0} VNĐ"
                })
                .ToList();

            ViewData["ThanhToanId"] = new SelectList(thanhToanThanhCong, "ThanhToanId", "DisplayInfo");
            ViewBag.TrangThaiList = GetTrangThaiList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HoanTienId,ThanhToanId,SoTienHoan,TrangThai,LyDo")] HoanTien hoanTien)
        {
            // Bỏ qua validate navigation property
            ModelState.Remove("ThanhToan");

            if (ModelState.IsValid)
            {
                // Kiểm tra logic: Số tiền hoàn không được lớn hơn số tiền đã thanh toán gốc
                var thanhToanGoc = await _context.ThanhToans.FindAsync(hoanTien.ThanhToanId);

                if (thanhToanGoc != null)
                {
                    if (hoanTien.SoTienHoan > thanhToanGoc.SoTien)
                    {
                        ModelState.AddModelError("SoTienHoan", $"Số tiền hoàn không được lớn hơn số tiền gốc ({thanhToanGoc.SoTien:N0} VNĐ).");
                    }
                    else
                    {
                        hoanTien.NgayTao = DateTime.Now;
                        _context.Add(hoanTien);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Tạo yêu cầu hoàn tiền thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    ModelState.AddModelError("ThanhToanId", "Không tìm thấy giao dịch thanh toán gốc.");
                }
            }

            // Load lại data nếu lỗi
            var thanhToanThanhCong = _context.ThanhToans
                .Include(tt => tt.DonHang)
                .Where(tt => tt.TrangThai == "THANH_CONG")
                .Select(tt => new {
                    tt.ThanhToanId,
                    DisplayInfo = $"{tt.DonHang.MaDonHang} - {tt.SoTien:N0} VNĐ"
                })
                .ToList();

            ViewData["ThanhToanId"] = new SelectList(thanhToanThanhCong, "ThanhToanId", "DisplayInfo", hoanTien.ThanhToanId);
            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(hoanTien);
        }

        // ==========================================
        // 4. CẬP NHẬT HOÀN TIỀN
        // ==========================================
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                .ThenInclude(tt => tt.DonHang)
                .FirstOrDefaultAsync(h => h.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            // Hiển thị thông tin thanh toán gốc (không cho sửa ID thanh toán, chỉ hiển thị)
            ViewData["ThanhToanInfo"] = $"{hoanTien.ThanhToan.DonHang.MaDonHang} - {hoanTien.ThanhToan.SoTien:N0} VNĐ";

            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(hoanTien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("HoanTienId,ThanhToanId,SoTienHoan,TrangThai,LyDo,NgayTao")] HoanTien hoanTien)
        {
            if (id != hoanTien.HoanTienId) return NotFound();

            ModelState.Remove("ThanhToan");

            if (ModelState.IsValid)
            {
                try
                {
                    // Vẫn nên kiểm tra lại số tiền nếu người dùng sửa số tiền
                    var thanhToanGoc = await _context.ThanhToans.AsNoTracking().FirstOrDefaultAsync(t => t.ThanhToanId == hoanTien.ThanhToanId);
                    if (thanhToanGoc != null && hoanTien.SoTienHoan > thanhToanGoc.SoTien)
                    {
                        ModelState.AddModelError("SoTienHoan", $"Số tiền hoàn không được lớn hơn số tiền gốc ({thanhToanGoc.SoTien:N0} VNĐ).");
                        // Reload view
                        ViewData["ThanhToanInfo"] = $"Giao dịch #{hoanTien.ThanhToanId}";
                        ViewBag.TrangThaiList = GetTrangThaiList();
                        return View(hoanTien);
                    }

                    _context.Update(hoanTien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật yêu cầu hoàn tiền thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoanTienExists(hoanTien.HoanTienId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ThanhToanInfo"] = $"Giao dịch #{hoanTien.ThanhToanId}";
            ViewBag.TrangThaiList = GetTrangThaiList();
            return View(hoanTien);
        }

        // ==========================================
        // 5. XÓA HOÀN TIỀN
        // ==========================================
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var hoanTien = await _context.HoanTiens
                .Include(h => h.ThanhToan)
                .ThenInclude(tt => tt.DonHang)
                .FirstOrDefaultAsync(m => m.HoanTienId == id);

            if (hoanTien == null) return NotFound();

            return View(hoanTien);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var hoanTien = await _context.HoanTiens.FindAsync(id);
            if (hoanTien != null)
            {
                _context.HoanTiens.Remove(hoanTien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa yêu cầu hoàn tiền.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HoanTienExists(long id)
        {
            return _context.HoanTiens.Any(e => e.HoanTienId == id);
        }

        // Helper tạo danh sách trạng thái
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