using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TextTemplating;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhMucController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public DanhMucController(DbConnect_KitchenHome_WsiteBanHang context): base(context)
        {
            _context = context;
        }

        // GET: Admin/DanhMuc
        public async Task<IActionResult> Index(
      string search,
      bool? status,
      int page = 1,
      int pageSize = 5)
        {
            var query = _context.DanhMucs
                .Include(d => d.DanhMucCha)
                .AsQueryable();

            // 🔍 Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.TenDanhMuc.Contains(search));
            }

            // 🧹 Lọc trạng thái
            if (status.HasValue)
            {
                query = query.Where(x => x.DangHoatDong == status.Value);
            }

            // 📊 Tổng bản ghi
            int totalItems = await query.CountAsync();

            // 📄 Phân trang
            var list = await query
                .OrderBy(x => x.ThuTu)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(list);
        }


        // GET: Admin/DanhMuc/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.DanhMucChaId = new SelectList(
                await _context.DanhMucs.Where(x => x.DangHoatDong).ToListAsync(),
                "DanhMucId",
                "TenDanhMuc"
            );
            return View();
        }

        // POST: Admin/DanhMuc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhMuc model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tạo slug nếu chưa có
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = StringHelper.ToSlug(model.TenDanhMuc);
                }

                // 2. Check trùng slug
                if (await _context.DanhMucs.AnyAsync(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("Slug", "Slug (URL) này đã tồn tại!");
                    await LoadDanhMucCha(model.DanhMucChaId);
                    return View(model);
                }

                model.NgayTao = DateTime.Now;

                _context.DanhMucs.Add(model);
                await _context.SaveChangesAsync();

                SetAlert("Thêm danh mục thành công!", "success");
                return RedirectToAction(nameof(Index));
            }

            await LoadDanhMucCha(model.DanhMucChaId);
            return View(model);
        }

        // GET: Admin/DanhMuc/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dm = await _context.DanhMucs.FindAsync(id);
            if (dm == null) return NotFound();

            await LoadDanhMucCha(dm.DanhMucChaId, id);
            return View(dm);
        }

        // POST: Admin/DanhMuc/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DanhMuc model)
        {
            if (ModelState.IsValid)
            {
                var dmDB = await _context.DanhMucs.FindAsync(model.DanhMucId);
                if (dmDB == null) return NotFound();

                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    model.Slug = StringHelper.ToSlug(model.TenDanhMuc);
                }

                if (await _context.DanhMucs.AnyAsync(x =>
                        x.Slug == model.Slug && x.DanhMucId != model.DanhMucId))
                {
                    ModelState.AddModelError("Slug", "Slug đã tồn tại!");
                    await LoadDanhMucCha(model.DanhMucChaId, model.DanhMucId);
                    return View(model);
                }

                dmDB.TenDanhMuc = model.TenDanhMuc;
                dmDB.Slug = model.Slug;
                dmDB.DanhMucChaId = model.DanhMucChaId;
                dmDB.ThuTu = model.ThuTu;
                dmDB.DangHoatDong = model.DangHoatDong;

                await _context.SaveChangesAsync();

                SetAlert("Cập nhật thành công!", "success");
                return RedirectToAction(nameof(Index));
            }

            await LoadDanhMucCha(model.DanhMucChaId, model.DanhMucId);
            return View(model);
        }

        // POST: Admin/DanhMuc/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dm = await _context.DanhMucs
                .Include(x => x.InverseDanhMucCha)
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.DanhMucId == id);

            if (dm == null) return RedirectToAction(nameof(Index));

            if (dm.InverseDanhMucCha.Any())
            {
                SetAlert("Không thể xóa: Danh mục đang chứa danh mục con!", "error");
                return RedirectToAction(nameof(Index));
            }

            if (dm.SanPhams.Any())
            {
                SetAlert("Không thể xóa: Danh mục đang chứa sản phẩm!", "error");
                return RedirectToAction(nameof(Index));
            }

            _context.DanhMucs.Remove(dm);
            await _context.SaveChangesAsync();

            SetAlert("Xóa danh mục thành công!", "success");
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/DanhMuc/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dm = await _context.DanhMucs
                .Include(x => x.DanhMucCha)
                .FirstOrDefaultAsync(x => x.DanhMucId == id);

            if (dm == null) return NotFound();

            return View(dm);
        }

        // ================== HÀM DÙNG CHUNG ==================
        private async Task LoadDanhMucCha(int? selectedId = null, int? excludeId = null)
        {
            var query = _context.DanhMucs.Where(x => x.DangHoatDong);

            if (excludeId.HasValue)
                query = query.Where(x => x.DanhMucId != excludeId);

            ViewBag.DanhMucChaId = new SelectList(
                await query.ToListAsync(),
                "DanhMucId",
                "TenDanhMuc",
                selectedId
            );
        }
    }
}
