using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace KitchenHome_WsiteBanHang.Areas.Thu_Kho.Controllers
{
    [Area("Thu_Kho")]
 
    public class TonKhoController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public TonKhoController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =====================================================
        // EDIT (CHỈ ADMIN ĐƯỢC CHỈNH TRỰC TIẾP TỒN)
        // =====================================================

       
        public async Task<IActionResult> Edit(int khoId, int bienTheId, int returnKhoId)
        {
            var tonKho = await _context.TonKhos
                .Include(t => t.BienThe)
                .FirstOrDefaultAsync(t => t.KhoId == khoId && t.BienTheId == bienTheId);

            if (tonKho == null)
                return NotFound();

            ViewBag.ReturnKhoId = returnKhoId;

            return View(tonKho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public async Task<IActionResult> Edit(int khoId, int bienTheId, TonKho model, int returnKhoId)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(x => x.KhoId == khoId && x.BienTheId == bienTheId);

            if (tonKho == null)
                return NotFound();

            int tonCu = tonKho.SoLuongTon;
            int tonMoi = model.SoLuongTon;

            if (tonMoi < 0)
            {
                ModelState.AddModelError("", "Số lượng tồn không được âm.");
                return View(model);
            }

            if (tonMoi < model.SoLuongGiuCho)
            {
                ModelState.AddModelError("", "Tồn kho không thể nhỏ hơn số lượng giữ chỗ.");
                return View(model);
            }

            int chenhLech = tonMoi - tonCu;

            tonKho.SoLuongTon = tonMoi;
            tonKho.SoLuongGiuCho = model.SoLuongGiuCho;
            tonKho.MucDatHangLai = model.MucDatHangLai;
            tonKho.NgayCapNhat = DateTime.Now;

            if (chenhLech != 0)
            {
                _context.NhatKyKhos.Add(new NhatKyKho
                {
                    MaNhatKy = Guid.NewGuid().ToString("N"),
                    KhoId = khoId,
                    BienTheId = bienTheId,
                    LoaiPhatSinh = chenhLech > 0 ? "NHAP" : "XUAT",
                    SoLuong = Math.Abs(chenhLech),
                    LoaiThamChieu = "EDIT_TONKHO",
                    GhiChu = "Admin điều chỉnh tồn kho",
                    NgayTao = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Kho",
                new { area = "Admin", id = returnKhoId });
        }

        // =====================================================
        // NHẬP / XUẤT KHO (THỦ KHO & ADMIN)
        // =====================================================

     
        public async Task<IActionResult> NhapXuat(int khoId, int bienTheId, int returnKhoId)
        {
            var tonKho = await _context.TonKhos
                .Include(x => x.BienThe)
                .FirstOrDefaultAsync(x =>
                    x.KhoId == khoId &&
                    x.BienTheId == bienTheId);

            if (tonKho == null)
                return NotFound();

            ViewBag.ReturnKhoId = returnKhoId;

            return View(tonKho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
  
        public async Task<IActionResult> NhapXuat(
            int khoId,
            int bienTheId,
            string loai,
            int soLuong,
            string ghiChu,
            int returnKhoId)
        {
            var tonKho = await _context.TonKhos
                .FirstOrDefaultAsync(x =>
                    x.KhoId == khoId &&
                    x.BienTheId == bienTheId);

            if (tonKho == null)
                return NotFound();

            if (soLuong <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0";
                return RedirectToAction("NhapXuat",
                    new { khoId, bienTheId, returnKhoId });
            }

            if (loai == "XUAT" && tonKho.SoLuongTon < soLuong)
            {
                TempData["Error"] = "Không đủ tồn kho để xuất";
                return RedirectToAction("NhapXuat",
                    new { khoId, bienTheId, returnKhoId });
            }

            if (loai == "NHAP")
                tonKho.SoLuongTon += soLuong;
            else
                tonKho.SoLuongTon -= soLuong;

            tonKho.NgayCapNhat = DateTime.Now;

            _context.NhatKyKhos.Add(new NhatKyKho
            {
                MaNhatKy = Guid.NewGuid().ToString("N"),
                KhoId = khoId,
                BienTheId = bienTheId,
                LoaiPhatSinh = loai,
                SoLuong = soLuong,
                LoaiThamChieu = "NHAP_XUAT",
                GhiChu = ghiChu,
                NgayTao = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Thao tác thành công";

            return RedirectToAction("Details", "Kho",
                new { area = "Admin", id = returnKhoId });
        }

        // =====================================================
        // IMPORT EXCEL (GIỮ NGUYÊN)
        // =====================================================

        public IActionResult ImportExcel()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel";
                return RedirectToAction(nameof(ImportExcel));
            }

            int successCount = 0;
            var errors = new List<string>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null || worksheet.Dimension == null)
            {
                TempData["Error"] = "File Excel không có dữ liệu";
                return RedirectToAction(nameof(ImportExcel));
            }

            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string maKho = worksheet.Cells[row, 1].Text.Trim();
                string sku = worksheet.Cells[row, 2].Text.Trim();

                if (!int.TryParse(worksheet.Cells[row, 3].Text.Trim(), out int soLuongTon))
                {
                    errors.Add($"Dòng {row}: Số lượng không hợp lệ");
                    continue;
                }

                var kho = await _context.Khos.FirstOrDefaultAsync(x => x.MaKho == maKho);
                var bienThe = await _context.BienTheSanPhams.FirstOrDefaultAsync(x => x.Sku == sku);

                if (kho == null || bienThe == null)
                {
                    errors.Add($"Dòng {row}: Kho hoặc SKU không tồn tại");
                    continue;
                }

                var tonKho = await _context.TonKhos
                    .FirstOrDefaultAsync(x =>
                        x.KhoId == kho.KhoId &&
                        x.BienTheId == bienThe.BienTheId);

                int tonCu = tonKho?.SoLuongTon ?? 0;
                int chenhLech = soLuongTon - tonCu;

                if (tonKho == null)
                {
                    tonKho = new TonKho
                    {
                        KhoId = kho.KhoId,
                        BienTheId = bienThe.BienTheId,
                        SoLuongTon = soLuongTon,
                        NgayCapNhat = DateTime.Now
                    };
                    _context.TonKhos.Add(tonKho);
                }
                else
                {
                    tonKho.SoLuongTon = soLuongTon;
                    tonKho.NgayCapNhat = DateTime.Now;
                }

                if (chenhLech != 0)
                {
                    _context.NhatKyKhos.Add(new NhatKyKho
                    {
                        MaNhatKy = Guid.NewGuid().ToString("N"),
                        KhoId = kho.KhoId,
                        BienTheId = bienThe.BienTheId,
                        LoaiPhatSinh = chenhLech > 0 ? "NHAP" : "XUAT",
                        SoLuong = Math.Abs(chenhLech),
                        LoaiThamChieu = "IMPORT_EXCEL",
                        GhiChu = $"Import Excel dòng {row}",
                        NgayTao = DateTime.Now
                    });
                }

                successCount++;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Import thành công {successCount} dòng.";

            if (errors.Any())
                TempData["ErrorList"] = string.Join("<br/>", errors);

            return RedirectToAction(nameof(ImportExcel));
        }
    }
}