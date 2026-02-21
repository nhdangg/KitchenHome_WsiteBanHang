using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public SanPhamController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? danhMucId,
            List<int>? thuongHieuIds,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy,
            string searchString,
            int page = 1)
        {
            int pageSize = 9; // Số sản phẩm trên 1 trang

            // 1. Query cơ bản (Eager loading các bảng cần thiết)
            var query = _context.SanPhams
                .Include(p => p.BienTheSanPhams)
                .Include(p => p.DanhMuc)
                .Include(p => p.ThuongHieu)
                .Include(p => p.DanhGiaSanPhams) // Để tính sao
                .Where(p => p.DangHoatDong)
                .AsQueryable();

            // 2. Lọc theo Danh mục
            if (danhMucId.HasValue)
            {
                query = query.Where(p => p.DanhMucId == danhMucId);
            }

            // 3. Lọc theo Thương hiệu (Checkbox nhiều lựa chọn)
            if (thuongHieuIds != null && thuongHieuIds.Any())
            {
                query = query.Where(p => p.ThuongHieuId.HasValue && thuongHieuIds.Contains(p.ThuongHieuId.Value));
            }

            // 4. Lọc theo Giá (Dựa trên biến thể có giá thấp nhất hoặc giá bán chính)
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.BienTheSanPhams.Any(bt => bt.GiaBan >= minPrice.Value));
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.BienTheSanPhams.Any(bt => bt.GiaBan <= maxPrice.Value));
            }

            // 5. Tìm kiếm từ khóa
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.TenSanPham.Contains(searchString));
            }

            // 6. Sắp xếp
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.BienTheSanPhams.Min(bt => bt.GiaBan));
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.BienTheSanPhams.Min(bt => bt.GiaBan));
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.TenSanPham);
                    break;
                default: // new
                    query = query.OrderByDescending(p => p.NgayTao);
                    break;
            }

            // 7. Phân trang
            var totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 8. Chuẩn bị ViewModel
            var model = new SanPhamIndexViewModel
            {
                DanhSachSanPham = products,
                DanhSachDanhMuc = await _context.DanhMucs.Where(d => d.DangHoatDong).ToListAsync(),
                DanhSachThuongHieu = await _context.ThuongHieus.Where(t => t.DangHoatDong).ToListAsync(),

                // Gán lại giá trị filter để View hiển thị
                DanhMucId = danhMucId,
                ThuongHieuIds = thuongHieuIds ?? new List<int>(),
                GiaMin = minPrice,
                GiaMax = maxPrice,
                SortBy = sortBy,
                SearchString = searchString,
                PageIndex = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return View(model);
        }
    }
}