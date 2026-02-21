using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonViTinhController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public DonViTinhController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =======================
        // GET: Admin/DonViTinh
        // =======================
        public async Task<IActionResult> Index()
        {
            var list = await _context.DonViTinhs
                .OrderBy(x => x.TenDonViTinh)
                .ToListAsync();

            return View(list);
        }

        // =======================
        // GET: Admin/DonViTinh/Create
        // =======================
        public IActionResult Create()
        {
            return View();
        }

        // =======================
        // POST: Admin/DonViTinh/Create
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonViTinh model)
        {
            if (ModelState.IsValid)
            {
                // kiểm tra trùng tên
                bool exists = await _context.DonViTinhs
                    .AnyAsync(x => x.TenDonViTinh == model.TenDonViTinh);

                if (exists)
                {
                    ModelState.AddModelError("", "Tên đơn vị tính đã tồn tại");
                    return View(model);
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =======================
        // GET: Admin/DonViTinh/Edit/5
        // =======================
        public async Task<IActionResult> Edit(int id)
        {
            var donViTinh = await _context.DonViTinhs.FindAsync(id);
            if (donViTinh == null)
                return NotFound();

            return View(donViTinh);
        }

        // =======================
        // POST: Admin/DonViTinh/Edit/5
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DonViTinh model)
        {
            if (id != model.DonViTinhId)
                return NotFound();

            if (ModelState.IsValid)
            {
                bool exists = await _context.DonViTinhs
                    .AnyAsync(x => x.TenDonViTinh == model.TenDonViTinh
                                && x.DonViTinhId != id);

                if (exists)
                {
                    ModelState.AddModelError("", "Tên đơn vị tính đã tồn tại");
                    return View(model);
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // =======================
        // GET: Admin/DonViTinh/Delete/5
        // =======================
        public async Task<IActionResult> Delete(int id)
        {
            var donViTinh = await _context.DonViTinhs
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.DonViTinhId == id);

            if (donViTinh == null)
                return NotFound();

            // nếu đang dùng cho sản phẩm → không cho xóa
            if (donViTinh.SanPhams.Any())
            {
                TempData["Error"] = "Không thể xóa vì đơn vị tính đang được sử dụng";
                return RedirectToAction(nameof(Index));
            }

            _context.DonViTinhs.Remove(donViTinh);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
