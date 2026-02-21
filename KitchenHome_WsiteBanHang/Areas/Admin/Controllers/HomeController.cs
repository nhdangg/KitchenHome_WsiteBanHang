using Microsoft.AspNetCore.Mvc;
using KitchenHome_WsiteBanHang.Models.Context;
using System;
using System.Linq;

namespace KitchenHome_WsiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly DbConnect_KitchenHome_WsiteBanHang _db;

        public HomeController(DbConnect_KitchenHome_WsiteBanHang db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            /* ===============================
             * 1️⃣ TỔNG QUAN
             * =============================== */

            ViewBag.TongDonHang = _db.DonHangs.Count();

            ViewBag.DoanhThu = _db.DonHangs
                .Where(x => x.TrangThai == "HOAN_TAT" || x.TrangThai == "DA_GIAO")
                .Sum(x => (decimal?)x.TongTien) ?? 0;

            ViewBag.TongKhachHang = _db.KhachHangs.Count();
            ViewBag.TongSanPham = _db.SanPhams.Count();


            /* ===============================
             * 2️⃣ HOÀN TIỀN
             * =============================== */

            ViewBag.TongTienHoan = _db.HoanTiens
                .Where(x => x.TrangThai == "DA_HOAN")
                .Sum(x => (decimal?)x.SoTienHoan) ?? 0;

            ViewBag.TienChoHoan = _db.HoanTiens
                .Where(x => x.TrangThai == "CHO_HOAN")
                .Sum(x => (decimal?)x.SoTienHoan) ?? 0;

            ViewBag.SoLuotHoan = _db.HoanTiens.Count();

            ViewBag.HoanTienHomNay = _db.HoanTiens
                .Where(x => x.TrangThai == "DA_HOAN"
                         && x.NgayTao.Date == DateTime.Today)
                .Sum(x => (decimal?)x.SoTienHoan) ?? 0;


            /* ===============================
             * 3️⃣ TRẢ HÀNG
             * =============================== */

            ViewBag.TongTraHang = _db.TraHangs.Count();

            ViewBag.TraHangDaDuyet = _db.TraHangs
                .Count(x => x.TrangThai == "DA_DUYET" || x.TrangThai == "HOAN_TAT");

            ViewBag.TraHangTuChoi = _db.TraHangs
                .Count(x => x.TrangThai == "TU_CHOI");


            /* ===============================
             * 4️⃣ DOANH THU 7 NGÀY
             * =============================== */

            var doanhThu7Ngay = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .Select(d => new
                {
                    Ngay = d.ToString("dd/MM"),
                    Tong = _db.DonHangs
                        .Where(x => x.NgayDat.Date == d &&
                               (x.TrangThai == "HOAN_TAT" || x.TrangThai == "DA_GIAO"))
                        .Sum(x => (decimal?)x.TongTien) ?? 0
                }).ToList();

            ViewBag.ChartLabels = Newtonsoft.Json.JsonConvert.SerializeObject(
                doanhThu7Ngay.Select(x => x.Ngay)
            );
            ViewBag.ChartData = Newtonsoft.Json.JsonConvert.SerializeObject(
                doanhThu7Ngay.Select(x => x.Tong)
            );


            /* ===============================
             * 5️⃣ HOÀN TIỀN 7 NGÀY
             * =============================== */

            var hoanTien7Ngay = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .Select(d => new
                {
                    Ngay = d.ToString("dd/MM"),
                    Tong = _db.HoanTiens
                        .Where(x => x.TrangThai == "DA_HOAN"
                                 && x.NgayTao.Date == d)
                        .Sum(x => (decimal?)x.SoTienHoan) ?? 0
                }).ToList();

            ViewBag.RefundLabels = Newtonsoft.Json.JsonConvert.SerializeObject(
                hoanTien7Ngay.Select(x => x.Ngay)
            );
            ViewBag.RefundData = Newtonsoft.Json.JsonConvert.SerializeObject(
                hoanTien7Ngay.Select(x => x.Tong)
            );


            /* ===============================
             * 6️⃣ TRẠNG THÁI ĐƠN
             * =============================== */

            var status = _db.DonHangs
                .GroupBy(x => x.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToList();

            ViewBag.StatusLabels = Newtonsoft.Json.JsonConvert.SerializeObject(
                status.Select(x => x.TrangThai)
            );
            ViewBag.StatusData = Newtonsoft.Json.JsonConvert.SerializeObject(
                status.Select(x => x.SoLuong)
            );


            /* ===============================
             * 7️⃣ ĐƠN HÀNG MỚI
             * =============================== */

            ViewBag.DonHangMoi = _db.DonHangs
                .OrderByDescending(x => x.NgayDat)
                .Take(10)
                .ToList();

            return View();
        }
    }
}
