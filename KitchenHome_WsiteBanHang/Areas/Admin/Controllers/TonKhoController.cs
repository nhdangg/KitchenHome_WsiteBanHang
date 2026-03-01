using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TonKhoController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public TonKhoController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

       

        // ================================
        // EDIT
        // ================================
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
        public async Task<IActionResult> Edit(
            int khoId,
            int bienTheId,
            TonKho model,
            int returnKhoId)
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
                model.NgayCapNhat = DateTime.Now;

                _context.Update(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Kho",
                    new { area = "Admin", id = returnKhoId });
            }

            return View(model);
        }

        //=============================
        // GET: Admin/TonKho/ImportExcel
        public IActionResult ImportExcel()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("Vui lòng chọn file Excel");

            var errors = new List<string>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        string maKho = worksheet.Cells[row, 1].Text.Trim();
                        string sku = worksheet.Cells[row, 2].Text.Trim();
                        string slTonText = worksheet.Cells[row, 3].Text.Trim();
                        string slGiuText = worksheet.Cells[row, 4].Text.Trim();

                        if (!int.TryParse(slTonText, out int soLuongTon) ||
                            !int.TryParse(slGiuText, out int soLuongGiu))
                        {
                            errors.Add($"Dòng {row}: Số lượng không hợp lệ");
                            continue;
                        }

                        var kho = await _context.Khos
                            .FirstOrDefaultAsync(x => x.MaKho == maKho);

                        if (kho == null)
                        {
                            errors.Add($"Dòng {row}: Kho '{maKho}' không tồn tại");
                            continue;
                        }

                        var bienThe = await _context.BienTheSanPhams
                            .FirstOrDefaultAsync(x => x.Sku == sku);

                        if (bienThe == null)
                        {
                            errors.Add($"Dòng {row}: SKU '{sku}' không tồn tại");
                            continue;
                        }

                        var tonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(x =>
                                x.KhoId == kho.KhoId &&
                                x.BienTheId == bienThe.BienTheId);

                        if (tonKho == null)
                        {
                            tonKho = new TonKho
                            {
                                KhoId = kho.KhoId,
                                BienTheId = bienThe.BienTheId,
                                SoLuongTon = soLuongTon,
                                SoLuongGiuCho = soLuongGiu,
                                MucDatHangLai = 0,
                                NgayCapNhat = DateTime.Now
                            };

                            _context.TonKhos.Add(tonKho);
                        }
                        else
                        {
                            tonKho.SoLuongTon = soLuongTon;
                            tonKho.SoLuongGiuCho = soLuongGiu;
                            tonKho.NgayCapNhat = DateTime.Now;
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            if (errors.Any())
            {
                return Content(string.Join("\n", errors), "text/plain");
            }

            return Content("Import thành công!");
        }

    }
}