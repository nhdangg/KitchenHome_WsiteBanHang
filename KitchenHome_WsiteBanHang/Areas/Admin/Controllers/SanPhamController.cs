using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using KitchenHome_WsiteBanHang.Controllers;

namespace WsiteBanHang_GiaDung.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SanPhamController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly IWebHostEnvironment _env;

        public SanPhamController(
            DbConnect_KitchenHome_WsiteBanHang context,
            IWebHostEnvironment env) : base(context)
        {
            _context = context;
            _env = env;
        }

        // ======================= INDEX =======================
        public IActionResult Index(string searchString, int? page)
        {
            var query = _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.ThuongHieu)
                .Include(x => x.BienTheSanPhams) // <--- QUAN TRỌNG: Thêm dòng này để đếm số lượng SKU
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(x =>
                    x.TenSanPham.Contains(searchString) ||
                    x.MaSanPham.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;

            int pageSize = 5;
            int pageNumber = page ?? 1;

            IPagedList<SanPham> model =
                query.OrderByDescending(x => x.SanPhamId)
                     .ToPagedList(pageNumber, pageSize); // ✅ X.PagedList

            return View(model);
        }

        // ======================= DETAILS =======================
        public IActionResult Details(int id)
        {
            var sp = _context.SanPhams
                .Include(x => x.DanhMuc)
                .Include(x => x.ThuongHieu)
                .Include(x => x.DonViTinh)
                .Include(x => x.BienTheSanPhams)
                .FirstOrDefault(x => x.SanPhamId == id);

            if (sp == null) return NotFound();
            return View(sp);
        }

        // ======================= CREATE =======================
        public IActionResult Create()
        {

            LoadDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanPham sanPham, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                sanPham.MaSanPham ??= "SP" + DateTime.Now.ToString("yyyyMMddHHmmss");
                sanPham.Slug ??= sanPham.TenSanPham.ToLower().Replace(" ", "-");

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string path = Path.Combine(_env.WebRootPath, "Image/Image_SanPham", fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await ImageFile.CopyToAsync(stream);
                    sanPham.AnhDaiDien = fileName;
                }

                sanPham.NgayTao = DateTime.Now;
                sanPham.DangHoatDong = true;

                _context.SanPhams.Add(sanPham);
                await _context.SaveChangesAsync();

                SetAlert("Thêm sản phẩm thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            LoadDropdown(sanPham);
            return View(sanPham);
        }

        // ======================= EDIT =======================
        public IActionResult Edit(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();

            LoadDropdown(sp);
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SanPham sanPham, IFormFile ImageFile)
        {
            var spDB = await _context.SanPhams.FindAsync(sanPham.SanPhamId);
            if (spDB == null) return NotFound();

            if (ModelState.IsValid)
            {
                spDB.TenSanPham = sanPham.TenSanPham;
                spDB.DanhMucId = sanPham.DanhMucId;
                spDB.ThuongHieuId = sanPham.ThuongHieuId;
                spDB.DonViTinhId = sanPham.DonViTinhId;
                spDB.MoTaNgan = sanPham.MoTaNgan;
                spDB.MoTaChiTiet = sanPham.MoTaChiTiet;
                spDB.HienThiTrangChu = sanPham.HienThiTrangChu;
                spDB.DangHoatDong = sanPham.DangHoatDong;
                spDB.NgayCapNhat = DateTime.Now;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);
                    string path = Path.Combine(_env.WebRootPath, "Image/Image_SanPham", fileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    await ImageFile.CopyToAsync(stream);
                    spDB.AnhDaiDien = fileName;
                }

                await _context.SaveChangesAsync();
                SetAlert("Cập nhật thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            LoadDropdown(sanPham);
            return View(sanPham);
        }

        // ======================= DELETE (GET) =======================
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await _context.SanPhams
                .Include(x => x.DanhMuc)
                .FirstOrDefaultAsync(x => x.SanPhamId == id);

            if (sp == null) return NotFound();
            return View(sp);
        }

        // ======================= DELETE (POST) =======================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null)
            {
                _context.SanPhams.Remove(sp);
                await _context.SaveChangesAsync();
                SetAlert("Xóa sản phẩm thành công!", "success");
            }
            return RedirectToAction(nameof(Index));
        }

        // ======================= BIẾN THỂ =======================
        public IActionResult BienThe(int sanPhamId)
        {
            ViewBag.SanPham = _context.SanPhams.Find(sanPhamId);

            var list = _context.BienTheSanPhams
                .Where(x => x.SanPhamId == sanPhamId)
                .OrderBy(x => x.GiaBan)
                .ToList();

            return View(list);
        }

        public IActionResult CreateBienThe(int sanPhamId)
        {
            var sp = _context.SanPhams.Find(sanPhamId);
            if (sp == null) return NotFound();

            ViewBag.TenSanPham = sp.TenSanPham;

            return View(new BienTheSanPham
            {
                SanPhamId = sanPhamId,
                Sku = sp.MaSanPham + "-" + DateTime.Now.ToString("HHmm"),
                DangHoatDong = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBienThe(BienTheSanPham model)
        {
            if (_context.BienTheSanPhams.Any(x => x.Sku == model.Sku))
            {
                ModelState.AddModelError("Sku", "SKU đã tồn tại");
                ViewBag.TenSanPham = _context.SanPhams.Find(model.SanPhamId)?.TenSanPham;
                return View(model);
            }

            model.MaVach ??= model.Sku;
            model.SoLuongToiThieu = model.SoLuongToiThieu < 1 ? 1 : model.SoLuongToiThieu;
            model.ThuocTinhJson = string.IsNullOrEmpty(model.TenBienThe)
                ? "{}"
                : $"{{\"name\":\"{model.TenBienThe}\"}}";
            model.NgayTao = DateTime.Now;

            _context.BienTheSanPhams.Add(model);
            await _context.SaveChangesAsync();

            // Khởi tạo tồn kho
            var khoList = _context.Khos.Where(x => x.DangHoatDong).ToList();
            foreach (var kho in khoList)
            {
                _context.TonKhos.Add(new TonKho
                {
                    KhoId = kho.KhoId,
                    BienTheId = model.BienTheId,
                    SoLuongTon = 0,
                    SoLuongGiuCho = 0,
                    MucDatHangLai = model.SoLuongToiThieu,
                    NgayCapNhat = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();

            SetAlert("Thêm biến thể thành công", "success");
            return RedirectToAction("BienThe", new { sanPhamId = model.SanPhamId });
        }

        public IActionResult EditBienThe(int id)
        {
            var bt = _context.BienTheSanPhams
                .Include(x => x.SanPham)
                .FirstOrDefault(x => x.BienTheId == id);

            if (bt == null) return NotFound();
            ViewBag.TenSanPham = bt.SanPham.TenSanPham;
            return View(bt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBienThe(BienTheSanPham model)
        {
            var btDB = await _context.BienTheSanPhams.FindAsync(model.BienTheId);
            if (btDB == null) return NotFound();

            if (_context.BienTheSanPhams.Any(x => x.Sku == model.Sku && x.BienTheId != model.BienTheId))
            {
                ModelState.AddModelError("Sku", "SKU đã tồn tại");
                ViewBag.TenSanPham = btDB.SanPham?.TenSanPham;
                return View(model);
            }

            btDB.Sku = model.Sku;
            btDB.MaVach = model.MaVach ?? model.Sku;
            btDB.TenBienThe = model.TenBienThe;
            btDB.GiaBan = model.GiaBan;
            btDB.SoLuongToiThieu = model.SoLuongToiThieu;
            btDB.DangHoatDong = model.DangHoatDong;
            btDB.ThuocTinhJson = string.IsNullOrEmpty(model.TenBienThe)
                ? "{}"
                : $"{{\"name\":\"{model.TenBienThe}\"}}";

            await _context.SaveChangesAsync();
            SetAlert("Cập nhật biến thể thành công", "success");

            return RedirectToAction("BienThe", new { sanPhamId = btDB.SanPhamId });
        }

        // ======================= Hình ảnh sản phẩm(list) ================
        public IActionResult ThemHinhAnh(int bienTheId)
        {
            ViewBag.BienTheId = bienTheId;

            var list = _context.HinhAnhSanPhams
                .Where(x => x.BienTheId == bienTheId)
                .OrderBy(x => x.ThuTu)
                .ToList();

            return View(list);
        }
        

        // ======================= DROPDOWN =======================
        private void LoadDropdown(SanPham sp = null)
        {
            ViewBag.DanhMucId = new SelectList(_context.DanhMucs, "DanhMucId", "TenDanhMuc", sp?.DanhMucId);
            ViewBag.ThuongHieuId = new SelectList(_context.ThuongHieus, "ThuongHieuId", "TenThuongHieu", sp?.ThuongHieuId);
            ViewBag.DonViTinhId = new SelectList(_context.DonViTinhs, "DonViTinhId", "TenDonViTinh", sp?.DonViTinhId);
        }
    }
}
