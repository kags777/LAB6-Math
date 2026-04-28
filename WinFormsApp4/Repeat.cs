using System;

namespace WinFormsApp4
{
    public static class StringExtensions
    {
        public static string Repeat(this string str, int count)
        {
            if (string.IsNullOrEmpty(str) || count <= 0)
                return string.Empty;

            return new string(str[0], count);
        }

        // Можно добавить и другие полезные методы
        public static string RepeatChar(char ch, int count)
        {
            return new string(ch, count);
        }
    }
}