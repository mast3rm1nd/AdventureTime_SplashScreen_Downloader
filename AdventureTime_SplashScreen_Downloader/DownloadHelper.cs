using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AdventureTime_SplashScreen_Downloader
{
    class DownloadHelper
    {
        public static async Task<string> DownloadHtmlAsync(string uri, Encoding encoding)
        {
            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
            //request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0";
            //request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //request.Headers.Set("Accept-Language", "en-US,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            //request.Headers.Set ("Accept-Encoding", "gzip, deflate");
            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;         //добавляет заголовок для gzip, переключает режим декомпрессии
            
            request.KeepAlive = true;
            
            try
            {
                Debug.WriteLine("Downloading " + uri);
                var response = await request.GetResponseAsync() as HttpWebResponse;
                Debug.WriteLine("Finished " + uri);
                if (!uri.Contains("category/sezon"))
                    Program.pages_of_episodes_to_download--; //если в ссылке нет признака страницы сезона, значит скачиваем эпизоды

                StreamReader sr = new StreamReader(response.GetResponseStream(), encoding);
                string html = sr.ReadToEnd();
                return html;
            }
            catch
            {
                return "";
            }
        }
        ///////////////////////////////////     SEASONS     ///////////////////////////////////
        static async Task<string[]> GetHtml_of_all_Seasons_1st_PagesAsync(int from_season, int to_season)                                                          //all pages with index == 1
        {
            Console.WriteLine("Downloading HTML of seasons pages...");

            var html_of_all_Seasons_Pages_Task = new List<Task<string>>();
            for (int i = from_season; i <= to_season; i++)
            {
                html_of_all_Seasons_Pages_Task.Add(DownloadHtmlAsync("http://advetime.ru/category/sezon-" + i.ToString(), Encoding.UTF8));
            }
            var html_of_all_Seasons_Pages = await Task.WhenAll(html_of_all_Seasons_Pages_Task);

            return html_of_all_Seasons_Pages;
        }


        static async Task<string[]> GetHtml_of_all_Seasons_PagesAsync()                                      //all pages with index > 1
        {
            var html_of_1st_pages_of_all_seasons = new List<string>();
            html_of_1st_pages_of_all_seasons.AddRange(await GetHtml_of_all_Seasons_1st_PagesAsync(1, Program.seasonsCount));                                      //TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            var result = new List<string>();
            result.AddRange(html_of_1st_pages_of_all_seasons);

            foreach (string season_1st_page_html in html_of_1st_pages_of_all_seasons)
            {
                if (season_1st_page_html.Contains("pagination-last"))
                {
                    int pages_count = Get_pages_count(season_1st_page_html);
                    int season = Get_season_num(season_1st_page_html);

                    var html_of_all_additional_Season_pages_Task = new List<Task<string>>();
                    for (int page_index = 2; page_index <= pages_count; page_index++)
                    {
                        html_of_all_additional_Season_pages_Task.Add(DownloadHtmlAsync("http://advetime.ru/category/sezon-" + season.ToString() + "/next/" + page_index.ToString(), Encoding.UTF8));
                    }
                    var html_of_all_additional_Season_pages = await Task.WhenAll(html_of_all_additional_Season_pages_Task);             //additional pages of current season (in if statement)

                    result.AddRange(html_of_all_additional_Season_pages);
                }                    
            }

            Console.WriteLine("All seasons pages downloaded.");
            Console.WriteLine();
            return result.ToArray();
        }
        ///////////////////////////////////     /SEASONS     //////////////////////////////////
        


        //static string[] Get_All_Episodes_Pages_Links()
        //{
        //    var all_Episodes_Pages_Links = new List<string>();

        //    //var complete_html_of_all_seasons_pages = new List<string>();
        //    //complete_html_of_all_seasons_pages.Add(Get_All_Episodes_Pages_Links)


        //    return all_Episodes_Pages_Links.ToArray();
        //}

        public static async Task<string[]> GetHtml_of_all_Episodes_PagesAsync()
        {            
            var htmlS_of_all_pages_of_all_seasons = await GetHtml_of_all_Seasons_PagesAsync();

            //List<string> html_of_all_Episodes_Pages = new List<string>();

            //получить ссылки на страницы эпизодов из htmlS_of_all_pages_of_all_seasons
            Console.WriteLine("Downloading HTML of epdisodes pages...");
            var episodes_links = new List<string>();
            foreach(string page_of_season in htmlS_of_all_pages_of_all_seasons)
            {
                MatchCollection m_epidodes_links = Regex.Matches(page_of_season, "<a href=\"(http://advetime.ru/.+)\"><img src=\"", RegexOptions.Multiline);

                foreach (Match m in m_epidodes_links)
                {
                    episodes_links.Add(m.Groups[1].Value);                          //TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                }//добавление всех ссылок на эпизоды с текущей страницы сезона (page_of_season) 
            }


            Program.pages_of_episodes_to_download = episodes_links.Count;
            Program.is_pages_of_episodes_ready = true;

            //добавлять ссылки для скачивания html страницы каждого эпизода
            var html_of_all_Episodes_Pages_task = new List<Task<string>>();
            foreach(string episode_link in episodes_links)
            {
                html_of_all_Episodes_Pages_task.Add(DownloadHtmlAsync(episode_link, Encoding.UTF8));
            }


            //TODO апдейтер статуса загрузки страниц серий???



            var html_of_all_Episodes_Pages = await Task.WhenAll(html_of_all_Episodes_Pages_task);

            
            //Console.WriteLine("All epdisodes pages downloaded.");
            return html_of_all_Episodes_Pages;
        }





        private static int Get_season_num(string season_1st_page_html)
        {
            MatchCollection season_m = Regex.Matches(season_1st_page_html, "sezon-(\\d+)");

            var test = season_m[0].Groups[0].Value[0];        
            var test2 = season_m[0].Groups[1].Value[0]; //TESTING

            return Convert.ToInt32(season_m[0].Groups[1].Value[0].ToString());
        }

        private static int Get_pages_count(string season_1st_page_html)
        {
            //pagination-last.+next\/(\d+)
            //Regex reged_total_pages_count = @"pagination-last.+next\/(\d+";
            MatchCollection pages_count_m = Regex.Matches(season_1st_page_html, "pagination-last.+next/(\\d+)");

            var test = pages_count_m[0].Groups[0].Value[0];                                                                         //TESTING
            var test2 = pages_count_m[0].Groups[1].Value[0];

            return Convert.ToInt32(pages_count_m[0].Groups[1].Value[0].ToString());
        }

        
    }
}
