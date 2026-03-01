using KitchenHome_WsiteBanHang.Controllers;
using KitchenHome_WsiteBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using KitchenHome_WsiteBanHang.Models.Context;
using ClosedXML.Excel; // <--- Nhớ thêm dòng này ở đầu file
using System.IO;       // <--- Để dùng MemoryStream

namespace WsiteBanHang_GiaDung.Areas.Quan_Ly.Controllers
{
    [Area("Quan_Ly")]
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
