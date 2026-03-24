using ClosedXML.Parser;
using KitchenHome_WsiteBanHang.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ChatbotController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly DbConnect_KitchenHome_WsiteBanHang _context;

    public ChatbotController(HttpClient httpClient, DbConnect_KitchenHome_WsiteBanHang context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Ask(string message)
    {
        var apiKey = "AIzaSyC-Yt9EnmtBsUZ8kYu3r9sJbF4PrNvCY90";
        // Bạn vẫn dùng API Key cũ đó, chỉ đổi tên Model trong chuỗi URL này thôi
        // Bạn hãy đổi chính xác thành dòng này:
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        string msgLower = message.ToLower();
        string keyword = "";

        // Bắt từ khóa
        if (msgLower.Contains("chảo")) keyword = "chảo";
        else if (msgLower.Contains("nồi")) keyword = "nồi";
        else if (msgLower.Contains("bếp")) keyword = "bếp";
        else if (msgLower.Contains("máy xay")) keyword = "máy xay";
        else if (msgLower.Contains("dao")) keyword = "dao";
        else if (msgLower.Contains("kéo")) keyword = "kéo";
        else if (msgLower.Contains("thớt")) keyword = "thớt";

        string thongTinSanPham = "";

        if (!string.IsNullOrEmpty(keyword))
        {
            var dsSanPham = await _context.BienTheSanPhams
                .AsNoTracking()
                .Include(b => b.SanPham)
                .Where(b => b.DangHoatDong == true &&
                            b.SanPham.DangHoatDong == true &&
                            b.SanPham.TenSanPham.Contains(keyword))
                .Take(5)
                .ToListAsync();

            if (dsSanPham.Any())
            {
                thongTinSanPham = "Danh sách sản phẩm:\n";
                foreach (var bt in dsSanPham)
                {
                    string tenFull = bt.SanPham.TenSanPham;
                    if (!string.IsNullOrEmpty(bt.TenBienThe)) tenFull += " - " + bt.TenBienThe;

                    decimal giaThucTe = bt.GiaKhuyenMai.HasValue && bt.GiaKhuyenMai > 0 ? bt.GiaKhuyenMai.Value : bt.GiaBan;

                    // 1. LẤY LINK ẢNH (Xử lý nếu sản phẩm chưa có ảnh)
                    string anh = "";
                    if (string.IsNullOrEmpty(bt.SanPham.AnhDaiDien))
                    {
                        // Đường dẫn đến ảnh mặc định nếu sản phẩm không có ảnh
                        anh = "/IMAGE/default-product.png";
                    }
                    else
                    {
                        // Nếu Database chỉ lưu tên file, hãy nối với thư mục chứa ảnh sản phẩm
                        // Lưu ý: Kiểm tra xem Database của bạn lưu "tenfile.jpg" hay "/IMAGE/Img_SanPham/tenfile.jpg"
                        if (bt.SanPham.AnhDaiDien.StartsWith("/"))
                        {
                            anh = bt.SanPham.AnhDaiDien;
                        }
                        else
                        {
                            // Giả sử ảnh nằm trong thư mục Img_SanPham
                            anh = "/IMAGE/Img_SanPham/" + bt.SanPham.AnhDaiDien;
                        }
                    }

                    // 2. TẠO LINK CHI TIẾT SẢN PHẨM (Sử dụng cột Slug)
                    // Lưu ý: Sửa lại "/SanPham/ChiTiet/" cho đúng với đường dẫn thật trên web của bạn
                    string link = $"/Home/ChiTiet/{bt.SanPham.Slug}";

                    // Truyền data cho AI
                    thongTinSanPham += $"[Tên: {tenFull} | Giá: {giaThucTe:N0} | Ảnh: {anh} | Link: {link}]\n";
                }
            }
            else
            {
                thongTinSanPham = $"Hiện tại cửa hàng đang tạm hết hàng các sản phẩm liên quan đến '{keyword}'.";
            }
        }

        // =========================================================
        // BƯỚC 2: RA LỆNH CHO AI VẼ GIAO DIỆN BẰNG HTML
        // =========================================================
        var systemPrompt = $@"Bạn là chatbot bán hàng KitchenHome Bot.
Trách nhiệm: Trả lời thân thiện, nhiệt tình và NGẮN GỌN bằng tiếng Việt.
Dữ liệu sản phẩm: {thongTinSanPham}

LUẬT RẤT QUAN TRỌNG: 
Nếu khách hỏi mua đồ và có dữ liệu, bạn BẮT BUỘC phải hiển thị mỗi sản phẩm bằng đúng đoạn mã HTML dưới đây để web vẽ ra cái khung có ảnh click được. Đừng dùng text thường.
Mã HTML mẫu (Chỉ thay thế thông tin tương ứng từ dữ liệu):
<div style='margin-top:8px; padding:10px; background:#fff; border:1px solid #dee2e6; border-radius:8px; display:flex; gap:12px; align-items:center;'>
    <a href='[Link]' target='_blank'>
        <img src='[Ảnh]' style='width:60px; height:60px; object-fit:cover; border-radius:6px; border:1px solid #eee;'/>
    </a>
    <div style='display:flex; flex-direction:column; line-height:1.4;'>
        <a href='[Link]' target='_blank' style='font-weight:600; color:#0d6efd; text-decoration:none; font-size:14px;'>[Tên]</a>
        <span style='color:#dc3545; font-weight:700; font-size:14px;'>[Giá] VNĐ</span>
    </div>
</div>

Khách hỏi: {message}";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = systemPrompt } } } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"
        );

        var response = await _httpClient.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) return Json(new { reply = "Lỗi từ Google: " + responseText });

        try
        {
            using var doc = JsonDocument.Parse(responseText);
            var reply = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return Json(new { reply });
        }
        catch (Exception ex)
        {
            return Json(new { reply = "Lỗi: " + ex.Message });
        }
    }
}