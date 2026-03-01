using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace WsiteBanHang_GiaDung.Areas.Thu_Kho.Controllers
{
    [Area("Thu_Kho")]
    public class NhatKyKhoController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public NhatKyKhoController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =========================================================
        // INDEX (GIỮ NGUYÊN)
        // =========================================================
        public async Task<IActionResult> Index(int? khoId, string? searchString)
        {
            var query = _context.NhatKyKhos
                .AsNoTracking()
                .Include(n => n.Kho)
                .Include(n => n.BienThe)
                    .ThenInclude(b => b.SanPham)
                .AsQueryable();

            if (khoId.HasValue)
            {
                query = query.Where(x => x.KhoId == khoId.Value);
                ViewBag.SelectedKhoId = khoId.Value;
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(x =>
                    x.MaNhatKy.Contains(searchString) ||
                    x.BienThe.Sku.Contains(searchString));

                ViewBag.SearchString = searchString;
            }

            ViewBag.ListKho = new SelectList(
                await _context.Khos
                    .Where(k => k.DangHoatDong)
                    .OrderBy(k => k.TenKho)
                    .ToListAsync(),
                "KhoId",
                "TenKho",
                khoId
            );

            var result = await query
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(result);
        }

        // =========================================================
        // CREATE
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhatKyKho model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 🔥 TỰ SINH MÃ PHIẾU
                model.MaNhatKy = "NK" + DateTime.Now.ToString("yyyyMMddHHmmss");
                model.NgayTao = DateTime.Now;

                // Tìm tồn kho
                var tonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(t =>
                        t.KhoId == model.KhoId &&
                        t.BienTheId == model.BienTheId);

                if (tonKho == null)
                {
                    tonKho = new TonKho
                    {
                        KhoId = model.KhoId,
                        BienTheId = model.BienTheId,
                        SoLuongTon = 0,
                        SoLuongGiuCho = 0
                    };
                    _context.TonKhos.Add(tonKho);
                }

                // 🔥 CHẶN XUẤT ÂM
                if (model.LoaiPhatSinh == "XUAT")
                {
                    if (tonKho.SoLuongTon < model.SoLuong)
                    {
                        ModelState.AddModelError("", "❌ Không đủ tồn kho để xuất.");
                        LoadDropdowns();
                        return View(model);
                    }

                    tonKho.SoLuongTon -= model.SoLuong;
                }
                else
                {
                    tonKho.SoLuongTon += model.SoLuong;
                }

                _context.NhatKyKhos.Add(model);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================
        // LOAD DROPDOWN
        // =========================================================
        private void LoadDropdowns()
        {
            ViewBag.KhoId = new SelectList(_context.Khos, "KhoId", "TenKho");
            ViewBag.BienTheId = new SelectList(_context.BienTheSanPhams, "BienTheId", "Sku");

            ViewBag.LoaiPhatSinh = new SelectList(new List<string>
            {
                "NHAP",
                "XUAT",
                "DIEU_CHINH"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTonKho(int khoId, int bienTheId)
        {
            var ton = await _context.TonKhos
                .Where(t => t.KhoId == khoId && t.BienTheId == bienTheId)
                .Select(t => t.SoLuongTon)
                .FirstOrDefaultAsync();

            return Json(ton);
        }
    }
}