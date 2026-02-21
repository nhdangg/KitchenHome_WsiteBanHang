using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Areas.Admin.Models;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyVaiTroController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public QuanLyVaiTroController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =======================
        // 1️⃣ DANH SÁCH VAI TRÒ
        // =======================
        public IActionResult Index()
        {
            var list = _context.VaiTros
                .OrderBy(x => x.MaVaiTro)
                .ToList();

            return View(list);
        }

        // =======================
        // 2️⃣ CHI TIẾT VAI TRÒ
        // =======================
        public IActionResult Details(int id)
        {
            var vaiTro = _context.VaiTros
                .Include(v => v.TaiKhoans)
                .FirstOrDefault(v => v.VaiTroId == id);

            if (vaiTro == null)
                return NotFound();

            var vm = new VaiTroDetailVM
            {
                VaiTro = vaiTro,
                TaiKhoans = vaiTro.TaiKhoans.ToList()
            };

            return View(vm);
        }

        // =======================
        // 3️⃣ THÊM VAI TRÒ
        // =======================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VaiTro model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool exists = _context.VaiTros.Any(x => x.MaVaiTro == model.MaVaiTro);
            if (exists)
            {
                ModelState.AddModelError("MaVaiTro", "Mã vai trò đã tồn tại.");
                return View(model);
            }

            _context.VaiTros.Add(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =======================
        // 4️⃣ SỬA VAI TRÒ
        // =======================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vaiTro = _context.VaiTros.Find(id);
            if (vaiTro == null)
                return NotFound();

            return View(vaiTro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, VaiTro model)
        {
            if (id != model.VaiTroId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            bool exists = _context.VaiTros
                .Any(x => x.MaVaiTro == model.MaVaiTro && x.VaiTroId != id);

            if (exists)
            {
                ModelState.AddModelError("MaVaiTro", "Mã vai trò đã tồn tại.");
                return View(model);
            }

            _context.Update(model);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =======================
        // 5️⃣ XÓA VAI TRÒ
        // =======================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var vaiTro = _context.VaiTros
                .Include(v => v.TaiKhoans)
                .FirstOrDefault(v => v.VaiTroId == id);

            if (vaiTro == null)
                return NotFound();

            if (vaiTro.TaiKhoans.Any())
            {
                TempData["ERROR"] = "Không thể xóa vai trò đang được gán cho tài khoản.";
                return RedirectToAction(nameof(Index));
            }

            return View(vaiTro);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var vaiTro = _context.VaiTros.Find(id);
            if (vaiTro == null)
                return NotFound();

            _context.VaiTros.Remove(vaiTro);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
