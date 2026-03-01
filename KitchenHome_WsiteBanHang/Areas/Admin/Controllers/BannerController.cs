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
    public class BannerController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BannerController(
            DbConnect_KitchenHome_WsiteBanHang context,
            IWebHostEnvironment webHostEnvironment
        ) : base(context)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Banner
        public async Task<IActionResult> Index()
        {
            var list = await _context.Banners
                .OrderBy(x => x.ThuTu)
                .ToListAsync();

            return View(list);
        }

        // GET: Admin/Banner/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Banner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = "";

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errors += $"Field: {state.Key}\n";
                        errors += $" - {error.ErrorMessage}\n";
                    }
                }

                return Content(errors, "text/plain");
            }
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string fileName = Path.GetFileName(ImageFile.FileName);

                    string uploadPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Image",
                        "Banner"
                    );

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    model.Anh = fileName;
                }
                else
                {
                    ModelState.AddModelError("Anh", "Vui lòng chọn ảnh banner");
                    return View(model);
                }

                _context.Banners.Add(model);
                await _context.SaveChangesAsync();

                SetAlert("Thêm banner thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Admin/Banner/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }

        // POST: Admin/Banner/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Banners.FindAsync(id);
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.Anh))
                {
                    string path = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Image",
                        "Banner",
                        item.Anh
                    );

                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                _context.Banners.Remove(item);
                await _context.SaveChangesAsync();

                SetAlert("Xóa banner thành công", "success");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Banner/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }
        // POST: Admin/Banner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner model, IFormFile? ImageFile)
        {
            if (id != model.BannerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null)
                {
                    return NotFound();
                }

                // --- CẬP NHẬT CÁC FIELD ---
                banner.TieuDe = model.TieuDe;
                banner.Text = model.Text;
                banner.Link = model.Link;
                banner.ThuTu = model.ThuTu;
                banner.IsActive = model.IsActive;

                // --- NẾU UP ẢNH MỚI ---
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(banner.Anh))
                    {
                        string oldPath = Path.Combine(
                            _webHostEnvironment.WebRootPath,
                            "Image",
                            "Banner",
                            banner.Anh
                        );

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Lưu ảnh mới
                    string fileName = Path.GetFileName(ImageFile.FileName);

                    string uploadPath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Image",
                        "Banner"
                    );

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    banner.Anh = fileName;
                }

                _context.Update(banner);
                await _context.SaveChangesAsync();

                SetAlert("Cập nhật banner thành công", "success");
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

    }
}
