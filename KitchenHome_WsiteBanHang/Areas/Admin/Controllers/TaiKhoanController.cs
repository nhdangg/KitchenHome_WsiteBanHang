using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using BCrypt.Net;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public TaiKhoanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // --- 1. DANH SÁCH (INDEX) ---
        public async Task<IActionResult> Index()
        {
            var list = await _context.TaiKhoans
                .Include(t => t.VaiTros)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();
            return View(list);
        }

        // --- 2. THÊM MỚI (GET) ---
        public IActionResult Create()
        {
            ViewBag.VaiTros = new MultiSelectList(_context.VaiTros, "VaiTroId", "TenVaiTro");
            return View();
        }

        // --- 2. THÊM MỚI (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    TaiKhoan taiKhoan,
    string MatKhauRaw,
    int[] selectedRoles)
        {
            // ❗ BẮT BUỘC: bỏ validation MatKhauHash
            ModelState.Remove("MatKhauHash");

            if (ModelState.IsValid)
            {
                taiKhoan.NgayTao = DateTime.Now;

                // HASH mật khẩu
                taiKhoan.MatKhauHash = BCrypt.Net.BCrypt.HashPassword(MatKhauRaw);

                // Gán vai trò
                taiKhoan.VaiTros = new List<VaiTro>();
                if (selectedRoles != null)
                {
                    foreach (var roleId in selectedRoles)
                    {
                        var role = new VaiTro { VaiTroId = roleId };
                        _context.VaiTros.Attach(role);
                        taiKhoan.VaiTros.Add(role);
                    }
                }

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VaiTros = new MultiSelectList(_context.VaiTros, "VaiTroId", "TenVaiTro");
            return View(taiKhoan);
        }



        // --- 3. SỬA (GET) ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.VaiTros)
                .FirstOrDefaultAsync(m => m.TaiKhoanId == id);

            if (taiKhoan == null) return NotFound();

            var currentRoleIds = taiKhoan.VaiTros.Select(v => v.VaiTroId).ToArray();
            ViewBag.VaiTros = new MultiSelectList(_context.VaiTros, "VaiTroId", "TenVaiTro", currentRoleIds);

            return View(taiKhoan);
        }

        // --- 3. SỬA (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoan taiKhoan, int[] selectedRoles)
        {
            if (id != taiKhoan.TaiKhoanId) return NotFound();

            // Loại bỏ kiểm tra MatKhauHash khỏi ModelState để vượt qua lỗi Validation
            ModelState.Remove("MatKhauHash");

            if (ModelState.IsValid)
            {
                try
                {
                    // Tải bản ghi thực tế từ DB kèm VaiTros (Eager Loading)
                    var accountInDb = await _context.TaiKhoans
                        .Include(t => t.VaiTros)
                        .FirstOrDefaultAsync(t => t.TaiKhoanId == id);

                    if (accountInDb == null) return NotFound();

                    // Cập nhật thủ công các trường cho phép sửa
                    accountInDb.Email = taiKhoan.Email;
                    accountInDb.SoDienThoai = taiKhoan.SoDienThoai;
                    accountInDb.DangHoatDong = taiKhoan.DangHoatDong;
                    accountInDb.NgayCapNhat = DateTime.Now;

                    // Đồng bộ quan hệ Nhiều-Nhiều: Xóa cũ và gán lại
                    accountInDb.VaiTros.Clear();
                    if (selectedRoles != null)
                    {
                        foreach (var roleId in selectedRoles)
                        {
                            var role = await _context.VaiTros.FindAsync(roleId);
                            if (role != null) accountInDb.VaiTros.Add(role);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi cập nhật: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }
            ViewBag.VaiTros = new MultiSelectList(_context.VaiTros, "VaiTroId", "TenVaiTro", selectedRoles);
            return View(taiKhoan);
        }

        // --- 4. XÓA ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.VaiTros)
                .FirstOrDefaultAsync(m => m.TaiKhoanId == id);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.VaiTros)
                .FirstOrDefaultAsync(t => t.TaiKhoanId == id);

            if (taiKhoan != null)
            {
                _context.TaiKhoans.Remove(taiKhoan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}