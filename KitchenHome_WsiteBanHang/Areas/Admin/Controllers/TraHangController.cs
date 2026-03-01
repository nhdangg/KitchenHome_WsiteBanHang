using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitchenHome_WsiteBanHang.Models;
using KitchenHome_WsiteBanHang.Models.Context;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TraHangController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _context;

        public TraHangController(DbConnect_KitchenHome_WsiteBanHang context)
        {
            _context = context;
        }

        // =====================================================
        // 1. DANH SÁCH
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var list = await _context.TraHangs
                .Include(x => x.DonHang)
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            return View(list);
        }

        // =====================================================
        // 2. CHI TIẾT
        // =====================================================
        public async Task<IActionResult> Details(long id)
        {
            var traHang = await _context.TraHangs
                .Include(x => x.DonHang)
                .Include(x => x.ChiTietTraHangs)
                    .ThenInclude(ct => ct.BienThe)
                        .ThenInclude(bt => bt.SanPham)
                .FirstOrDefaultAsync(x => x.TraHangId == id);

            if (traHang == null) return NotFound();
            return View(traHang);
        }

        // =====================================================
        // 3. THÊM MỚI (CREATE)
        // =====================================================
        // 1. Hàm GET: Hiển thị trang lần đầu
        public async Task<IActionResult> Create(long? donHangId)
        {
            // Nạp danh sách vào ViewBag
            await LoadViewBagData();

            if (donHangId != null)
            {
                var donHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .FirstOrDefaultAsync(d => d.DonHangId == donHangId);
                ViewBag.DonHang = donHang;
            }
            return View();
        }

        // 2. Hàm POST: Xử lý khi bấm "Lưu yêu cầu"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
          TraHang traHang,
          List<ChiTietTraHang> ChiTiet,
          List<int> SelectedIndexes)
        {
            ModelState.Clear();

            if (SelectedIndexes == null || !SelectedIndexes.Any())
            {
                ModelState.AddModelError("", "Lỗi: Bạn chưa chọn bất kỳ sản phẩm nào để trả!");
                await LoadViewBagData();
                ViewBag.DonHang = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .FirstOrDefaultAsync(d => d.DonHangId == traHang.DonHangId);
                return View(traHang);
            }

            // 🔥 LỌC CHI TIẾT THEO DÒNG ĐƯỢC CHỌN
            var chiTietDaChon = ChiTiet
                .Where((x, index) => SelectedIndexes.Contains(index))
                .ToList();

            if (!chiTietDaChon.Any())
            {
                ModelState.AddModelError("", "Không có sản phẩm hợp lệ để trả.");
                return View(traHang);
            }

            // ==== LƯU DB ====
            traHang.MaTraHang = "TH" + DateTime.Now.ToString("yyyyMMddHHmmss");
            traHang.TrangThai = "YEU_CAU";
            traHang.NgayTao = DateTime.Now;

            _context.TraHangs.Add(traHang);
            await _context.SaveChangesAsync();

            foreach (var item in chiTietDaChon)
            {
                item.TraHangId = traHang.TraHangId;
                _context.ChiTietTraHangs.Add(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // Hàm phụ để dùng chung cho cả 2 nơi, tránh viết lặp code
        private async Task LoadViewBagData()
        {
            ViewBag.DanhSachDonHang = await _context.DonHangs
                .Select(d => new { d.DonHangId, d.MaDonHang })
                .ToListAsync();
        }

        // =====================================================
        // 4. CHỈNH SỬA (EDIT) - Chỉ cho phép khi trạng thái là YEU_CAU
        // =====================================================
        public async Task<IActionResult> Edit(long id)
        {
            var traHang = await _context.TraHangs
                .Include(x => x.ChiTietTraHangs)
                .FirstOrDefaultAsync(x => x.TraHangId == id);

            if (traHang == null) return NotFound();

            // Bảo vệ logic: Đã xử lý xong hoặc đang xử lý thì không cho sửa nội dung yêu cầu
            if (traHang.TrangThai != "YEU_CAU")
                return BadRequest("Đơn hàng đã được xử lý, không thể chỉnh sửa.");

            return View(traHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, TraHang traHang)
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
            if (id != traHang.TraHangId) return NotFound();

            var oldData = await _context.TraHangs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TraHangId == id);

            if (oldData == null) return NotFound();
            if (oldData.TrangThai != "YEU_CAU")
                return BadRequest("Không được sửa đơn hàng ở trạng thái này.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(traHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TraHangExists(traHang.TraHangId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(traHang);
        }

        // =====================================================
        // 5. XÓA (DELETE) - Chỉ xóa khi còn là YEU_CAU
        // =====================================================
        public async Task<IActionResult> Delete(long id)
        {
            var traHang = await _context.TraHangs
                .Include(x => x.DonHang)
                .FirstOrDefaultAsync(x => x.TraHangId == id);

            if (traHang == null) return NotFound();
            if (traHang.TrangThai != "YEU_CAU")
                return BadRequest("Chỉ có thể xóa yêu cầu ở trạng thái Chờ xác nhận.");

            return View(traHang);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var traHang = await _context.TraHangs
                .Include(x => x.ChiTietTraHangs)
                .FirstOrDefaultAsync(x => x.TraHangId == id);

            if (traHang == null) return NotFound();
            if (traHang.TrangThai != "YEU_CAU")
                return BadRequest("Không thể xóa đơn đã qua xử lý.");

            // Xóa chi tiết trước khi xóa đơn chính (ràng buộc khóa ngoại)
            _context.ChiTietTraHangs.RemoveRange(traHang.ChiTietTraHangs);
            _context.TraHangs.Remove(traHang);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // 6. HOÀN TẤT TRẢ HÀNG (QUY TRÌNH NHẬP KHO & HOÀN TIỀN)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanTat(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var traHang = await _context.TraHangs
                    .Include(x => x.DonHang)
                    .Include(x => x.ChiTietTraHangs)
                    .FirstOrDefaultAsync(x => x.TraHangId == id);

                if (traHang == null || traHang.TrangThai == "HOAN_TAT")
                    return BadRequest("Đơn trả hàng không hợp lệ.");

                // --- Bước 1: Nhập lại tồn kho ---
                foreach (var ct in traHang.ChiTietTraHangs)
                {
                    var tonKho = await _context.TonKhos.FirstOrDefaultAsync(t =>
                        t.KhoId == traHang.DonHang.KhoId &&
                        t.BienTheId == ct.BienTheId);

                    if (tonKho == null)
                    {
                        _context.TonKhos.Add(new TonKho
                        {
                            KhoId = traHang.DonHang.KhoId,
                            BienTheId = ct.BienTheId,
                            SoLuongTon = ct.SoLuong,
                            NgayCapNhat = DateTime.Now
                        });
                    }
                    else
                    {
                        tonKho.SoLuongTon += ct.SoLuong;
                        tonKho.NgayCapNhat = DateTime.Now;
                    }

                    // Lưu nhật ký kho
                    _context.NhatKyKhos.Add(new NhatKyKho
                    {
                        MaNhatKy = "TH-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        KhoId = traHang.DonHang.KhoId,
                        BienTheId = ct.BienTheId,
                        LoaiPhatSinh = "NHAP",
                        SoLuong = ct.SoLuong,
                        LoaiThamChieu = "TRA_HANG",
                        ThamChieuId = traHang.TraHangId,
                        NgayTao = DateTime.Now
                    });
                }

                // --- Bước 2: Tạo bản ghi Hoàn tiền ---
                var thanhToan = await _context.ThanhToans
                    .FirstOrDefaultAsync(t => t.DonHangId == traHang.DonHangId && t.TrangThai == "THANH_CONG");

                if (thanhToan != null && traHang.SoTienDuKienHoan > 0)
                {
                    _context.HoanTiens.Add(new HoanTien
                    {
                        ThanhToanId = thanhToan.ThanhToanId,
                        SoTienHoan = traHang.SoTienDuKienHoan,
                        TrangThai = "CHO_HOAN",
                        LyDo = "Hoàn trả hàng cho đơn: " + traHang.MaTraHang,
                        NgayTao = DateTime.Now
                    });
                }

                // Cập nhật trạng thái cuối cùng
                traHang.TrangThai = "HOAN_TAT";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi xử lý hệ thống.");
            }
        }

        private bool TraHangExists(long id)
        {
            return _context.TraHangs.Any(e => e.TraHangId == id);
        }
    }
}