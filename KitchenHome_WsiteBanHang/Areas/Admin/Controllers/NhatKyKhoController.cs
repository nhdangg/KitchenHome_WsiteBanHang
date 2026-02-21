using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WsiteBanHang_GiaDung.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhatKyKhoController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public NhatKyKhoController(DbConnect_KitchenHome_WsiteBanHang context): base(context)
        {
            _context = context;
        }

        // 1. Xem lịch sử nhập xuất
        public async Task<IActionResult> Index(int? khoId, string searchString)
        {
            var logs = _context.NhatKyKhos
                               .Include(n => n.Kho)
                               .Include(n => n.BienThe)
                                   .ThenInclude(b => b.SanPham)
                               .AsQueryable();

            if (khoId.HasValue)
            {
                logs = logs.Where(x => x.KhoId == khoId);
                ViewBag.KhoID = khoId;
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(x =>
                    x.MaNhatKy.Contains(searchString) ||
                    x.BienThe.Sku.Contains(searchString));
            }

            ViewBag.ListKho = await _context.Khos.ToListAsync();

            return View(await logs.OrderByDescending(x => x.NgayTao).ToListAsync());
        }

        // 2. Giao diện tạo phiếu
        public async Task<IActionResult> Create()
        {
            ViewBag.KhoId = new SelectList(
                await _context.Khos.Where(k => k.DangHoatDong).ToListAsync(),
                "KhoId",
                "TenKho"
            );

            var products = await _context.BienTheSanPhams
                .Where(b => b.DangHoatDong)
                .Select(b => new
                {
                    b.BienTheId,
                    DisplayText = b.Sku + " - " +
                                  b.SanPham.TenSanPham +
                                  " (" + b.TenBienThe + ")"
                }).ToListAsync();

            ViewBag.BienTheId = new SelectList(products, "BienTheId", "DisplayText");

            return View();
        }

        // 3. Xử lý Nhập / Xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhatKyKho model)
        {
            if (ModelState.IsValid)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // A. Ghi nhật ký
                    model.NgayTao = DateTime.Now;
                    model.MaNhatKy = "NK" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    _context.NhatKyKhos.Add(model);

                    // B. Cập nhật tồn kho
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
                            SoLuongGiuCho = 0,
                            MucDatHangLai = 10,
                            NgayCapNhat = DateTime.Now
                        };
                        _context.TonKhos.Add(tonKho);
                    }

                    if (model.LoaiPhatSinh == "NHAP")
                    {
                        tonKho.SoLuongTon += model.SoLuong;
                    }
                    else if (model.LoaiPhatSinh == "XUAT")
                    {
                        if (tonKho.SoLuongTon < model.SoLuong)
                        {
                            throw new Exception("Số lượng tồn kho không đủ để xuất!");
                        }
                        tonKho.SoLuongTon -= model.SoLuong;
                    }

                    tonKho.NgayCapNhat = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    SetAlert($"Tạo phiếu {model.LoaiPhatSinh} thành công!", "success");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi xử lý: " + ex.Message);
                }
            }

            // Load lại dropdown khi lỗi
            ViewBag.KhoId = new SelectList(
                  await _context.Khos.Where(k => k.DangHoatDong).ToListAsync(),
                  "KhoId",
                  "TenKho"
              );


            var products = await _context.BienTheSanPhams
                .Select(b => new
                {
                    b.BienTheId,
                    DisplayText = b.Sku + " - " + b.SanPham.TenSanPham
                }).ToListAsync();

            ViewBag.BienTheId = new SelectList(products, "BienTheId", "DisplayText");

            return View(model);
        }

        // 4. Chi tiết phiếu
        public async Task<IActionResult> Details(long id)
        {
            var item = await _context.NhatKyKhos
                .Include(n => n.Kho)
                .Include(n => n.BienThe)
                    .ThenInclude(b => b.SanPham)
                .FirstOrDefaultAsync(x => x.NhatKyKhoId == id);

            if (item == null)
                return NotFound();

            return View(item);
        }
    }
}
