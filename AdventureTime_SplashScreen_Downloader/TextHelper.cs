using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureTime_SplashScreen_Downloader
{
    class TextHelper
    {
        public static string Skip_AllText_After_Substring(string text, string substring, bool is_exclude_substring_from_result)
        {
            if (text.Length <= substring.Length || !text.Contains(substring))
                return text;

            if (is_exclude_substring_from_result)
                return text.Substring(0, text.IndexOf(substring));

            return text.Substring(0, text.IndexOf(substring) + substring.Length);
        }

        public static string Skip_AllText_Before_Substring(string text, string substring, bool is_exclude_substring_from_result)
        {
            if (text.Length <= substring.Length || !text.Contains(substring))
                return text;

            if (is_exclude_substring_from_result)
                return text.Substring(text.IndexOf(substring) + substring.Length, text.Length - (text.IndexOf(substring) + substring.Length));

            return text.Substring(text.IndexOf(substring), text.Length - text.IndexOf(substring));
        }

        public static string Get_AllText_Between_Substrings(string text, string start_substring, string end_substring, bool include_substrings_in_result)
        {
            if(text.Length <= start_substring.Length + end_substring.Length)
                return text;

            if(!text.Contains(start_substring) || !text.Contains(end_substring))
                return text;

            var start_copy_pos = include_substrings_in_result == false ? text.IndexOf(start_substring) + start_substring.Length : text.IndexOf(start_substring);
            var end_copy_pos = include_substrings_in_result == false ? text.IndexOf(end_substring) : text.IndexOf(end_substring) + end_substring.Length;

            return text.Substring(start_copy_pos, end_copy_pos - start_copy_pos);
        }

        public static string GetFileExtension(string file)
        {
            if(!file.Contains("."))
                return file;

            for(int i = file.Length -1;;i--)
            {
                if (file[i] == '.')
                    return file.Substring(i + 1, file.Length - i - 1);
            }
        }

        public static string Remove_Restricted_Filename_Chars(string filename)
        {
            var restricted_chars = "\\/:*?\"><|";

            foreach (char illegal_char in restricted_chars)
                filename = filename.Replace(illegal_char.ToString(), "");

            return filename;
        }
    }
}
