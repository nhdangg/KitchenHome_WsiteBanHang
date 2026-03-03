using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using KitchenHome_WsiteBanHang.Models.Context;
using ClosedXML.Excel; // <--- Nhớ thêm dòng này ở đầu file
using System.IO;       // <--- Để dùng MemoryStream

namespace KitchenHome_WsiteBanHang.Areas.Thu_Kho.Controllers
{
    [Area("Thu_Kho")]
    public class KhoController : BaseController
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public KhoController(DbConnect_KitchenHome_WsiteBanHang context): base(context) 
        {
            _context = context;
        }

        // 1. DANH SÁCH KHO
        public IActionResult Index()
        {
            ViewBag.Active = "Kho";
            var data = _context.Khos
                .OrderBy(k => k.TenKho)
                .ToList();

            return View(data);
        }

        // 2. CHI TIẾT KHO + TỒN KHO + TÌM KIẾM + PHÂN TRANG
        public IActionResult Details(int id, string searchString, int? page)
        {
            ViewBag.Active = "Kho";

            // 1. Lấy thông tin kho
            var kho = _context.Khos.Find(id);
            if (kho == null)
                return NotFound();

            // 2. Query tồn kho
            var query = _context.TonKhos
                .Include(t => t.BienThe)
                    .ThenInclude(b => b.SanPham)
                .Where(t => t.KhoId == id)
                .AsQueryable();

            // 3. Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(t =>
                    t.BienThe.Sku.Contains(searchString) ||
                    t.BienThe.SanPham.TenSanPham.Contains(searchString));
            }

            // 4. Sắp xếp
            query = query.OrderBy(t => t.SoLuongTon);

            // 5. Phân trang
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.DanhSachTonKho = query.ToPagedList(pageNumber, pageSize);
            ViewBag.CurrentFilter = searchString;

            return View(kho);
        }

        // 3. THÊM KHO
        public IActionResult Create()
        {
            ViewBag.Active = "Kho";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Kho model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.Khos.Any(x => x.MaKho == model.MaKho))
            {
                ModelState.AddModelError("MaKho", "Mã kho này đã tồn tại trong hệ thống.");
                return View(model);
            }

            model.DangHoatDong = true;
            _context.Khos.Add(model);
            _context.SaveChanges();

            SetAlert("Thêm kho hàng thành công!", "success");
            return RedirectToAction(nameof(Index));
        }

        // 4. SỬA KHO
        public IActionResult Edit(int id)
        {
            ViewBag.Active = "Kho";
            var kho = _context.Khos.Find(id);
            if (kho == null)
                return NotFound();

            return View(kho);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Kho model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dbKho = _context.Khos.Find(model.KhoId);
            if (dbKho == null)
                return NotFound();

            dbKho.TenKho = model.TenKho;
            dbKho.DiaChi = model.DiaChi;
            dbKho.DangHoatDong = model.DangHoatDong;

            _context.SaveChanges();
            SetAlert("Cập nhật thông tin kho thành công!", "success");

            return RedirectToAction(nameof(Index));
        }

        // 5. XÓA / NGỪNG HOẠT ĐỘNG KHO
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var kho = _context.Khos.Find(id);
            if (kho == null)
                return RedirectToAction(nameof(Index));

            // Kho còn hàng?
            bool conHang = _context.TonKhos
                .Any(t => t.KhoId == id && t.SoLuongTon > 0);

            if (conHang)
            {
                SetAlert("Không thể xóa: Kho này đang còn hàng tồn!", "error");
                return RedirectToAction(nameof(Index));
            }

            // Có lịch sử?
            bool coLichSu = _context.NhatKyKhos.Any(n => n.KhoId == id)
                         || _context.DonHangs.Any(d => d.KhoId == id);

            if (coLichSu)
            {
                kho.DangHoatDong = false;
                _context.SaveChanges();
                SetAlert("Kho đã có lịch sử, hệ thống chuyển sang trạng thái ngừng hoạt động.", "warning");
                return RedirectToAction(nameof(Index));
            }

            _context.Khos.Remove(kho);
            _context.SaveChanges();
            SetAlert("Đã xóa kho hàng vĩnh viễn.", "success");

            return RedirectToAction(nameof(Index));
        }

        // 6. XUẤT EXCEL BÁO CÁO TỒN KHO
        public async Task<IActionResult> ExportToExcel(int id)
        {
            // 1. Lấy thông tin kho
            var kho = await _context.Khos.FindAsync(id);
            if (kho == null) return NotFound();

            // 2. Lấy dữ liệu tồn kho (Lấy hết, KHÔNG phân trang)
            var data = await _context.TonKhos
                .Include(t => t.BienThe)
                    .ThenInclude(b => b.SanPham)
                .Where(t => t.KhoId == id)
                .OrderBy(t => t.BienThe.SanPham.TenSanPham) // Sắp xếp theo tên cho đẹp
                .ToListAsync();

            // 3. Khởi tạo Excel bằng ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("BaoCaoTonKho");

                // --- TẠO HEADER ---
                worksheet.Cell(1, 1).Value = "BÁO CÁO TỒN KHO: " + kho.TenKho.ToUpper();
                worksheet.Range(1, 1, 1, 6).Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                worksheet.Cell(2, 1).Value = "Ngày xuất: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                worksheet.Range(2, 1, 2, 6).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int currentRow = 4;

                // Tiêu đề cột
                worksheet.Cell(currentRow, 1).Value = "STT";
                worksheet.Cell(currentRow, 2).Value = "Mã SKU";
                worksheet.Cell(currentRow, 3).Value = "Tên Sản Phẩm";
                worksheet.Cell(currentRow, 4).Value = "Phân Loại";
                worksheet.Cell(currentRow, 5).Value = "Tổng Tồn";
                worksheet.Cell(currentRow, 6).Value = "Khả Dụng"; // (Tồn - Giữ chỗ)

                // Style cho Header cột
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // --- ĐỔ DỮ LIỆU ---
                int stt = 1;
                foreach (var item in data)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = stt++;
                    worksheet.Cell(currentRow, 2).Value = item.BienThe.Sku;
                    worksheet.Cell(currentRow, 3).Value = item.BienThe.SanPham.TenSanPham;
                    worksheet.Cell(currentRow, 4).Value = item.BienThe.TenBienThe;

                    // Số lượng
                    worksheet.Cell(currentRow, 5).Value = item.SoLuongTon;
                    worksheet.Cell(currentRow, 6).Value = (item.SoLuongTon - item.SoLuongGiuCho);
                }

                // --- FORMAT ---
                // Tự động chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                // Kẻ khung toàn bộ bảng dữ liệu
                var dataRange = worksheet.Range(4, 1, currentRow, 6);
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // 4. Xuất file ra stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    string fileName = $"BaoCaoTon_{kho.MaKho}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
    }
}
