using System.Text;
using System.Text.RegularExpressions;

public static class StringHelper
{
    public static string ToSlug(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        input = input.ToLower().Trim();
        // Chuyển tiếng Việt có dấu thành không dấu
        input = Regex.Replace(input, "[áàảãạâấầẩẫậăắằẳẵặ]", "a");
        input = Regex.Replace(input, "[éèẻẽẹêếềểễệ]", "e");
        input = Regex.Replace(input, "[iíìỉĩị]", "i");
        input = Regex.Replace(input, "[óòỏõọôốồổỗộơớờởỡợ]", "o");
        input = Regex.Replace(input, "[úùủũụưứừửữự]", "u");
        input = Regex.Replace(input, "[ýỳỷỹỵ]", "y");
        input = Regex.Replace(input, "[đ]", "d");

        // Loại bỏ ký tự đặc biệt, giữ lại chữ cái, số và dấu gạch ngang
        input = Regex.Replace(input, "[^a-z0-9\\-]", "-");
        // Loại bỏ nhiều dấu gạch ngang liên tiếp
        input = Regex.Replace(input, "-+", "-").Trim('-');

        return input;
    }
}