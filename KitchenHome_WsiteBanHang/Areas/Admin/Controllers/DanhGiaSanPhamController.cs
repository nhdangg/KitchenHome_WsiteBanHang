using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DanhGiaSanPhamController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public DanhGiaSanPhamController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // ============================
        // INDEX - TÍCH HỢP TÌM KIẾM VÀ LỌC
        // ============================
        public async Task<IActionResult> Index(string search, bool? status)
        {
            // Lưu lại giá trị để hiển thị trên Form tìm kiếm
            ViewBag.Search = search;
            ViewBag.Status = status;

            var query = _context.DanhGiaSanPhams
                .Include(d => d.SanPham)
                .Include(d => d.KhachHang)
                .AsQueryable();

            // 1. Lọc theo từ khóa (Tên sản phẩm hoặc tên khách hàng)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.SanPham.TenSanPham.Contains(search) ||
                                         d.KhachHang.HoTen.Contains(search) ||
                                         d.TieuDe.Contains(search));
            }

            // 2. Lọc theo trạng thái hiển thị
            if (status.HasValue)
            {
                query = query.Where(d => d.HienThi == status.Value);
            }

            var data = await query.OrderByDescending(d => d.NgayTao).ToListAsync();

            return View(data);
        }

        // ============================
        // DETAILS
        // ============================
        public async Task<IActionResult> Details(long id)
        {
            var dg = await _context.DanhGiaSanPhams
                .Include(d => d.SanPham)
                .Include(d => d.KhachHang)
                .FirstOrDefaultAsync(d => d.DanhGiaId == id);

            if (dg == null) return NotFound();
            return View(dg);
        }

        // ============================
        // CREATE (GET)
        // ============================
        public IActionResult Create()
        {
            ViewBag.SanPhamId = new SelectList(_context.SanPhams, "SanPhamId", "TenSanPham");
            ViewBag.KhachHangId = new SelectList(_context.KhachHangs, "KhachHangId", "HoTen");

            return View(new DanhGiaSanPham
            {
                SoSao = 5,              // mặc định 5 sao
                HienThi = true
            });
        }

        // ============================
        // CREATE (POST) – BẮT LỖI
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGiaSanPham model)
        {
            // BỎ QUA kiểm tra lỗi của các đối tượng liên quan (chỉ quan tâm ID)
            ModelState.Remove("SanPham");
            ModelState.Remove("KhachHang");
            ModelState.Remove("DonHang");

            // Bẫy lỗi thủ công cho các trường dữ liệu
            if (model.SanPhamId <= 0)
                ModelState.AddModelError("SanPhamId", "Vui lòng chọn sản phẩm cần đánh giá.");

            if (string.IsNullOrWhiteSpace(model.TieuDe))
                ModelState.AddModelError("TieuDe", "Tiêu đề không được để trống.");

            if (string.IsNullOrWhiteSpace(model.NoiDung))
                ModelState.AddModelError("NoiDung", "Nội dung đánh giá không được để trống.");

            if (model.SoSao < 1 || model.SoSao > 5)
                ModelState.AddModelError("SoSao", "Vui lòng chọn mức độ đánh giá.");

            if (ModelState.IsValid)
            {
                try
                {
                    model.NgayTao = DateTime.Now;
                    _context.DanhGiaSanPhams.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi Database: " + ex.Message);
                }
            }

            // Nếu có lỗi, load lại dữ liệu cho Dropdown
            ViewBag.SanPhamId = new SelectList(_context.SanPhams, "SanPhamId", "TenSanPham", model.SanPhamId);
            ViewBag.KhachHangId = new SelectList(_context.KhachHangs, "KhachHangId", "HoTen", model.KhachHangId);

            return View(model);
        }
        // ============================
        // EDIT (GET)
        // ============================
        public async Task<IActionResult> Edit(long id)
        {
            var dg = await _context.DanhGiaSanPhams.FindAsync(id);
            if (dg == null) return NotFound();
            return View(dg);
        }

        // ============================
        // EDIT (POST) – BẮT LỖI
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, DanhGiaSanPham model)
        {
            if (id != model.DanhGiaId) return BadRequest();

            // KHẮC PHỤC LỖI 2: Loại bỏ kiểm tra các đối tượng liên quan để ModelState hợp lệ
            ModelState.Remove("SanPham");
            ModelState.Remove("KhachHang");
            ModelState.Remove("DonHang");

            if (model.SoSao < 1 || model.SoSao > 5)
                ModelState.AddModelError("SoSao", "Số sao phải từ 1 đến 5");

            if (ModelState.IsValid)
            {
                // KHẮC PHỤC LỖI 1: Tìm bản ghi gốc từ Database để cập nhật
                var dg = await _context.DanhGiaSanPhams.FindAsync(id);
                if (dg == null) return NotFound();

                try
                {
                    // Cập nhật từng trường
                    dg.SoSao = model.SoSao;
                    dg.TieuDe = model.TieuDe;
                    dg.NoiDung = model.NoiDung;
                    dg.HienThi = model.HienThi;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                }
            }

            // KHẮC PHỤC LỖI HIỂN THỊ: Nếu lỗi, phải nạp lại thông tin Sản phẩm/Khách hàng để hiển thị lại View
            // Nếu không nạp lại, các dòng @Model.KhachHang?.HoTen sẽ bị trống
            model = await _context.DanhGiaSanPhams
                .Include(d => d.SanPham)
                .Include(d => d.KhachHang)
                .FirstOrDefaultAsync(d => d.DanhGiaId == id);

            return View(model);
        }

        // ============================
        // DELETE
        // ============================
        public async Task<IActionResult> Delete(long id)
        {
            var dg = await _context.DanhGiaSanPhams
                .Include(d => d.SanPham)
                .Include(d => d.KhachHang)
                .FirstOrDefaultAsync(d => d.DanhGiaId == id);

            if (dg == null) return NotFound();
            return View(dg);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var dg = await _context.DanhGiaSanPhams.FindAsync(id);
            if (dg == null) return NotFound();

            _context.DanhGiaSanPhams.Remove(dg);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ============================
        // HÀM PHỤ
        // ============================
        private void LoadDropdown()
        {
            ViewBag.SanPhamID = _context.SanPhams.ToList();
            ViewBag.KhachHangID = _context.KhachHangs.ToList();
        }
    }
}
