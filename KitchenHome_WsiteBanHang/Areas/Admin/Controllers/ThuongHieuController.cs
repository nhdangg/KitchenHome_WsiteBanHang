using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WsiteBanHang_GiaDung.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThuongHieuController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public ThuongHieuController(DbConnect_KitchenHome_WsiteBanHang context)
            : base(context)
        {
            _context = context;
        }

        // ===================== DANH SÁCH =====================
        // ===================== DANH SÁCH =====================
        public async Task<IActionResult> Index(string searchString, string countryFilter, int page = 1)
        {
            // 1. Cấu hình phân trang
            int pageSize = 8; // Số bản ghi trên mỗi trang

            // 2. Lấy danh sách quốc gia cho Dropdown
            var countryQuery = await _context.ThuongHieus
                .Where(x => !string.IsNullOrEmpty(x.QuocGia))
                .Select(x => x.QuocGia)
                .Distinct()
                .ToListAsync();
            ViewBag.Countries = countryQuery;

            // 3. Khởi tạo truy vấn
            var query = _context.ThuongHieus
                                .Include(x => x.SanPhams)
                                .AsQueryable();

            // 4. Lọc dữ liệu
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x => x.TenThuongHieu.Contains(searchString.Trim()));
            }
            if (!string.IsNullOrEmpty(countryFilter))
            {
                query = query.Where(x => x.QuocGia == countryFilter);
            }

            // 5. Tính toán phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Đảm bảo trang hiện tại không vượt quá giới hạn
            page = page < 1 ? 1 : (page > totalPages && totalPages > 0 ? totalPages : page);

            var list = await query.Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            // 6. Gửi dữ liệu ra View
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentCountry"] = countryFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        // ===================== CHI TIẾT =====================
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.ThuongHieus
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.ThuongHieuId == id);

            if (item == null)
                return NotFound();

            ViewBag.SoLuongSanPham = item.SanPhams.Count;
            return View(item);
        }

        // ===================== THÊM MỚI =====================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThuongHieu model)
        {
            if (ModelState.IsValid)
            {
                bool isExist = await _context.ThuongHieus
                    .AnyAsync(x => x.TenThuongHieu == model.TenThuongHieu);

                if (isExist)
                {
                    ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu đã tồn tại");
                    return View(model);
                }

                model.DangHoatDong = true;

                _context.ThuongHieus.Add(model);
                await _context.SaveChangesAsync();

                SetAlert("Thêm thương hiệu thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ===================== SỬA =====================
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.ThuongHieus.FindAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ThuongHieu model)
        {
            if (ModelState.IsValid)
            {
                var dbItem = await _context.ThuongHieus.FindAsync(model.ThuongHieuId);
                if (dbItem == null)
                    return NotFound();

                dbItem.TenThuongHieu = model.TenThuongHieu;
                dbItem.QuocGia = model.QuocGia;
                dbItem.DangHoatDong = model.DangHoatDong;

                await _context.SaveChangesAsync();

                SetAlert("Cập nhật thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ===================== XÓA =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ThuongHieus
                .Include(x => x.SanPhams)
                .FirstOrDefaultAsync(x => x.ThuongHieuId == id);

            if (item == null)
                return NotFound();

            if (item.SanPhams.Any())
            {
                SetAlert("Không thể xóa: Thương hiệu đang có sản phẩm!", "error");
            }
            else
            {
                _context.ThuongHieus.Remove(item);
                await _context.SaveChangesAsync();
                SetAlert("Đã xóa thương hiệu", "success");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
