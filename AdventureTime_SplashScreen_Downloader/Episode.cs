using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace AdventureTime_SplashScreen_Downloader
{
    class Episode
    {
        public int season;
        public string title;
        public string description;
        public string[] cover_and_images;


        public static Episode Extract_Episode_Data_From_HTML (string html_of_episode_page)
        {
            var episode = new Episode();
            var html_to_work = TextHelper.Get_AllText_Between_Substrings(html_of_episode_page, "pagerate_header", "type type_page_comments", false);

            html_to_work = html_to_work.Replace("&nbsp;<span style=\"line-height: 1.5em;\">", "");


            MatchCollection season_title_m = Regex.Matches(html_of_episode_page, @"breadcrumbs.+http://advetime\.ru/category/sezon-(?<season_num>\d+).+&#8594;\s*(?<title>.+)</div>");
            episode.season = Convert.ToInt32(season_title_m[0].Groups["season_num"].Value);
            episode.title = season_title_m[0].Groups["title"].Value.Replace("&amp;", "&");




            //MatchCollection description_m = Regex.Matches(TextHelper.Get_AllText_Between_Substrings(html_of_episode_page, "pagerate_header", "type type_page_comments", false), "(?:</div><div>|</{0,1}p>|</b>|&nbsp;|21px;\">|\">)([-!?()А-яA-z0-9\"'\\s,.]{26,})+(?:</p><p>|<div class|</p>|</div>|21px;\">|</span></span></p><p><span face=\"Arial, Helvetica, Verdana, sans-serif\"><span style=\"font-size: 14px; line-height: 21px;\">)*([-!?()\"А-яA-z0-9'\\s,.]{20,})*(?:</p>)*");
            MatchCollection description_m = Regex.Matches(html_to_work, "(?:</div><div>|</{0,1}p>|</b>|&nbsp;|21px;\">|\">)([-!?()А-яёA-z0-9\"'\\s,.]{26,})+(?:</p><p>|<div class|</p>|</div>|21px;\">|</span></span></p><p><span face=\"Arial, Helvetica, Verdana, sans-serif\"><span style=\"font-size: 14px; line-height: 21px;\">)*");
            //var test = description_m[0].Groups[1].Value;
            //var test2 = description_m[0].Groups[0].Value;
            //if(html_of_episode_page.Contains("Сытый по горло выходками Джейка, его сын Ким Кил Вон принимает меры."))
            //{
            //    var test = description_m[0].Groups[1].Value;
            //    var test2 = description_m[0].Groups[0].Value;
            //}
            if (description_m.Count == 0)                
                episode.description = "";
            //else if (description_m.Count == 2)
            //{
            //    var test = description_m[0].Groups[1].Value;
            //    var test2 = description_m[0].Groups[2].Value;
            //    var test3 = description_m[1].Groups[1].Value;
            //    var test4 = description_m[1].Groups[2].Value;

            //    episode.description = description_m[0].Groups[1].Value + "\n\n" + description_m[1].Groups[1].Value;
            //}                
            else
                episode.description = description_m[0].Groups[1].Value;                                         //НЕПРАВИЛЬНО ПАРСИТСЯ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! предварительно обрезать странцу до определённого текста?


            
            
            var all_images = new List<string>();
            MatchCollection covers_m = Regex.Matches(html_to_work, "(http://advetime\\.ru/uploads/[0-9]+/[0-9-/A-z/\\.]+?)(?:\"\\sunselectable)");

            foreach(Match img_url in covers_m)
            {
                all_images.Add(img_url.Groups[1].Value);
            }

            Program.images_to_download += all_images.Count;

            episode.cover_and_images = all_images.ToArray();




            return episode;
        }
    }
}
