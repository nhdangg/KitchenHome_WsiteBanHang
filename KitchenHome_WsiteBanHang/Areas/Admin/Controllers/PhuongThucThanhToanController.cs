using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhuongThucThanhToanController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public PhuongThucThanhToanController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =======================
        // GET: Admin/PhuongThucThanhToan
        // =======================
        public async Task<IActionResult> Index()
        {
            var list = await _context.PhuongThucThanhToans
                .OrderBy(x => x.TenPhuongThuc)
                .ToListAsync();

            return View(list);
        }

        // =======================
        // GET: Admin/PhuongThucThanhToan/Create
        // =======================
        public IActionResult Create()
        {
            return View();
        }

        // =======================
        // POST: Admin/PhuongThucThanhToan/Create
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhuongThucThanhToan model)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.PhuongThucThanhToans
                    .AnyAsync(x => x.MaPhuongThuc == model.MaPhuongThuc);

                if (exists)
                {
                    ModelState.AddModelError("MaPhuongThuc", "Mã phương thức đã tồn tại");
                    return View(model);
                }

                model.DangHoatDong = true;

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =======================
        // GET: Admin/PhuongThucThanhToan/Edit/5
        // =======================
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.PhuongThucThanhToans.FindAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        // =======================
        // POST: Admin/PhuongThucThanhToan/Edit/5
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PhuongThucThanhToan model)
        {
            if (id != model.PhuongThucId)
                return NotFound();

            if (ModelState.IsValid)
            {
                bool exists = await _context.PhuongThucThanhToans
                    .AnyAsync(x => x.MaPhuongThuc == model.MaPhuongThuc
                                && x.PhuongThucId != id);

                if (exists)
                {
                    ModelState.AddModelError("MaPhuongThuc", "Mã phương thức đã tồn tại");
                    return View(model);
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =======================
        // POST: Admin/PhuongThucThanhToan/Toggle/5
        // =======================
        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            var item = await _context.PhuongThucThanhToans.FindAsync(id);
            if (item == null)
                return NotFound();

            item.DangHoatDong = !item.DangHoatDong;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =======================
        // GET: Admin/PhuongThucThanhToan/Delete/5
        // =======================
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.PhuongThucThanhToans
                .Include(x => x.ThanhToans)
                .FirstOrDefaultAsync(x => x.PhuongThucId == id);

            if (item == null)
                return NotFound();

            // đang dùng cho thanh toán → không cho xóa
            if (item.ThanhToans.Any())
            {
                TempData["Error"] = "Không thể xóa vì phương thức đang được sử dụng";
                return RedirectToAction(nameof(Index));
            }

            _context.PhuongThucThanhToans.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
