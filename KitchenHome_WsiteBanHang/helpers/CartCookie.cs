using Microsoft.AspNetCore.Http;
using System;

namespace KitchenHome_WsiteBanHang.Helpers
{
    public static class CartCookie
    {
        private const string COOKIE_NAME = "CART_KEY";

        public static string GetOrCreate(HttpContext context)
        {
            if (context == null) return null;

            // 1. Đọc cookie hiện có
            if (context.Request.Cookies.TryGetValue(COOKIE_NAME, out var existingValue)
                && !string.IsNullOrWhiteSpace(existingValue))
            {
                return existingValue;
            }

            // 2. Tạo key mới
            var newKey = Guid.NewGuid().ToString("N");

            // 3. Cấu hình cookie (AN TOÀN + CHẠY ĐƯỢC LOCAL)
            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,

                // ⚠️ CỰC KỲ QUAN TRỌNG
                Secure = context.Request.IsHttps, // HTTPS thì bật, HTTP thì tắt

                SameSite = SameSiteMode.Lax,

                // Ổn định hơn Expires
                MaxAge = TimeSpan.FromDays(30)
            };

            // 4. Ghi cookie
            context.Response.Cookies.Append(COOKIE_NAME, newKey, options);

            return newKey;
        }
    }
}
