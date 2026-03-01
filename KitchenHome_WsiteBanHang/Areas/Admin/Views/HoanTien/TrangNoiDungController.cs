using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TrangNoiDungController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public TrangNoiDungController(DbConnect_KitchenHome_WsiteBanHang context) : base(context)
        {
            _context = context;
        }

        // ======================= INDEX =======================
        public async Task<IActionResult> Index()
        {
            var data = await _context.TrangNoiDungs
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();
            return View(data);
        }

        // ======================= DETAILS =======================
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.TrangNoiDungs.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // ======================= CREATE =======================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TrangNoiDung model)
        {
            if (ModelState.IsValid)
            {
                // Tự tạo mã trang (slug) nếu chưa nhập
                if (string.IsNullOrEmpty(model.MaTrang))
                {
                    // Giả sử bạn có class StringHelper.ToSlug()
                    // Nếu chưa có, có thể dùng: model.MaTrang = model.TieuDe.ToLower().Replace(" ", "-");
                    model.MaTrang = StringHelper.ToSlug(model.TieuDe);
                }

                // Check trùng mã
                bool isExist = await _context.TrangNoiDungs.AnyAsync(x => x.MaTrang == model.MaTrang);
                if (isExist)
                {
                    ModelState.AddModelError("MaTrang", "Mã trang (Slug) này đã tồn tại.");
                    return View(model);
                }

                model.NgayTao = DateTime.Now;
                model.DangHienThi = true;

                _context.TrangNoiDungs.Add(model);
                await _context.SaveChangesAsync();

                SetAlert("Tạo trang nội dung thành công", "success");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ======================= EDIT =======================
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.TrangNoiDungs.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TrangNoiDung model)
        {
            if (ModelState.IsValid)
            {
                var dbItem = await _context.TrangNoiDungs.FindAsync(model.TrangId); // Hoặc TrangID tùy tên cột
                if (dbItem == null) return NotFound();

                dbItem.TieuDe = model.TieuDe;
                dbItem.NoiDung = model.NoiDung; // Core tự động nhận HTML từ form
                dbItem.DangHienThi = model.DangHienThi;
                dbItem.NgayCapNhat = DateTime.Now;

                // Lưu ý: Không cập nhật MaTrang (Slug) để tránh gãy link SEO cũ

                await _context.SaveChangesAsync();
                SetAlert("Cập nhật trang thành công", "success");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ======================= DELETE =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TrangNoiDungs.FindAsync(id);
            if (item != null)
            {
                _context.TrangNoiDungs.Remove(item);
                await _context.SaveChangesAsync();
                SetAlert("Đã xóa trang nội dung!", "success");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}