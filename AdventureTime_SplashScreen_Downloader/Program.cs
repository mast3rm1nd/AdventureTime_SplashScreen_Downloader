using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Threading;

namespace AdventureTime_SplashScreen_Downloader
{
    class Program
    {
        public static Episode[] all_episodes;
        public static int images_to_download = 0;
        public static bool is_images_ready = false;

        public static int pages_of_episodes_to_download = 0;
        public static bool is_pages_of_episodes_ready = false;

        public static int what_to_download = 0;
        public static int where_to_place = 0;
        internal static int seasonsCount = 7;

        static void Main(string[] args)
        {
            #region input
            Console.Title = "Adventure Time Episodes Data Extractor v. 0.5";
            Console.WriteLine("Данная программа позволяет скачивать данные о эпизодах Времени Приключений с сайта advetime.ru");           
            Console.WriteLine();
            Console.WriteLine("Что скачивать?");
            Console.WriteLine("[1] Только обложки.");
            Console.WriteLine("[2] Обложки и бонусные картинки.");
            Console.WriteLine("[3] Обложки, бонусные картинки и описания серий.");
            do
            {
                Console.Write("Ваш выбор: ");
                what_to_download = Convert.ToInt32(Console.ReadLine());
            } while (!(what_to_download > 0 && what_to_download <= 3));


            Console.WriteLine();


            Console.WriteLine("Куда скачиваем?");
            Console.WriteLine("[1] В папки по сезонам и эпизодам.");
            Console.WriteLine("[2] В папки по сезонам.");
            Console.WriteLine("[3] Всё в одну папку.");
            do
            {
                Console.Write("Ваш выбор: ");
                where_to_place = Convert.ToInt32(Console.ReadLine());
            } while (!(where_to_place > 0 && where_to_place <= 3));

            Console.WriteLine();
            #endregion


            Console.CursorVisible = false;

            Get_All_Data();


            #region waiting
            while(!is_pages_of_episodes_ready)
            {
                Thread.Sleep(10);
            }

            unsafe
            {
                fixed (int* pages_of_episodes_to_download_pointer = &pages_of_episodes_to_download)
                {
                    //WaitForData(is_pages_of_epidodes_ready);
                    DrawProgressBar(pages_of_episodes_to_download, pages_of_episodes_to_download_pointer);          //progress for episodes
                    Console.WriteLine();
                    Console.WriteLine();

                    //Console.WriteLine();
                }
            }


            while (!is_images_ready)
            {
                Thread.Sleep(10);
            }


            unsafe
            {
                fixed (int* images_to_download_pointer = &images_to_download)
                {
                    Console.WriteLine("Downloading images...");
                    DrawProgressBar(images_to_download, images_to_download_pointer);                                //progress for images
                }
            }
            #endregion

            Console.WriteLine("\n\nСкачивание завершено, скачанные файлы находятся в папке \"Время Приключений\" :)");

            Console.Read();
        }


        public static async Task<Episode[]> Get_Episodes_DataAsync()
        {
            var html_of_all_Series_Pages = await DownloadHelper.GetHtml_of_all_Episodes_PagesAsync();

            pages_of_episodes_to_download = html_of_all_Series_Pages.Length;

            var all_epidodes_data = new List<Episode>();

            foreach(string episode_html in html_of_all_Series_Pages)
            {
                if (episode_html != "")
                    all_epidodes_data.Add(Episode.Extract_Episode_Data_From_HTML(episode_html));
                else
                    pages_of_episodes_to_download--;
            }

            //all_episodes = all_epidodes_data.ToArray();
            is_images_ready = true;
            return all_epidodes_data.ToArray();
        }

        public static async void Get_All_Data()
        {
            Episode[] episodes = await Get_Episodes_DataAsync();

            
            foreach (Episode episode in episodes)
            {
                var path_to_place = "";
                var description_filename = "";
                var cover_filename = "";
                var bonus_images_prefix = "";
                //var episode_folder = "";
                //Console.WriteLine("[1] В папки по сезонам и эпизодам.");
                //Console.WriteLine("[2] В папки по сезонам.");
                //Console.WriteLine("[3] Всё в одну папку.");
                switch(where_to_place)
                {
                    case 1: path_to_place = "./Время Приключений/Сезон " + episode.season.ToString() + "/" + TextHelper.Remove_Restricted_Filename_Chars(episode.title) + "/";
                            description_filename = "Описание серии.txt";
                            if(episode.cover_and_images.Length != 0)
                                cover_filename = "Cover." + TextHelper.GetFileExtension(episode.cover_and_images[0]);
                            bonus_images_prefix = "Bonus_Image_";
                            break;

                    case 2: path_to_place = "./Время Приключений/Сезон " + episode.season.ToString() + "/"; 
                            description_filename = TextHelper.Remove_Restricted_Filename_Chars(episode.title) + ".txt";
                            if (episode.cover_and_images.Length != 0)
                                cover_filename = TextHelper.Remove_Restricted_Filename_Chars(episode.title) + "_Cover." + TextHelper.GetFileExtension(episode.cover_and_images[0]);
                            bonus_images_prefix = TextHelper.Remove_Restricted_Filename_Chars(episode.title) + "_Bonus_Image_";
                            break;

                    case 3: path_to_place = "./Время Приключений" + "/";
                            description_filename = "Сезон " + episode.season.ToString() + "_" + TextHelper.Remove_Restricted_Filename_Chars(episode.title) + ".txt";
                            if (episode.cover_and_images.Length != 0)
                                cover_filename = "Сезон " + episode.season.ToString() + "_" + TextHelper.Remove_Restricted_Filename_Chars(episode.title) + "_Cover." + TextHelper.GetFileExtension(episode.cover_and_images[0]);
                            bonus_images_prefix = "Сезон " + episode.season.ToString() + "_" + TextHelper.Remove_Restricted_Filename_Chars(episode.title) + "_Bonus_Image_";
                            break;
                }


                if (!Directory.Exists(path_to_place))
                    Directory.CreateDirectory(path_to_place);



                //////////////////////  ОПИСАНИЯ СЕРИЙ  //////////////////////
                if (what_to_download == 3)
                {
                    if (!File.Exists(path_to_place + description_filename))
                        File.WriteAllText(path_to_place + description_filename, episode.description);
                }
                

                //////////////////////  КАРТИНКИ  //////////////////////
                for (int i = 0; i < episode.cover_and_images.Length; i++)
                {
                    //скачиваем картинки
                    using (WebClient client = new WebClient())
                    {
                        var image_path = "";
                        if (i == 0)
                            image_path = path_to_place + cover_filename;
                        else
                            image_path = path_to_place + bonus_images_prefix + i.ToString() + "." + TextHelper.GetFileExtension(episode.cover_and_images[i]);

                        if (!File.Exists(image_path))
                        {
                            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                            client.DownloadFileAsync(new Uri(episode.cover_and_images[i]), image_path);
                        }
                        else
                            images_to_download--;
                            
                    }
                }
            }
        }



        static void Completed(object sender, AsyncCompletedEventArgs e)
        {
            //MessageBox.Show("Download completed!");
            images_to_download--;
        }


        public unsafe static void WaitForData(bool* is_data_ready)                                            //ДОПИЛИТЬ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            //var total_pages_of_episodes_to_download = 0;
            //var total_images_to_download = 0;

            while (!*is_data_ready)
            {               
                Thread.Sleep(5);
            }
        }





        unsafe static void DrawProgressBar(int total_count, int* reference)
        {            
            
            //var one_percent_delay = duration_seconds * 1000 / 100;

            

            var delay_between_updates = 10;

            //Console.WriteLine();
            Console.Write("[");

            
            var progress_bar_position = Console.CursorLeft;

            Console.CursorLeft = Console.CursorLeft + 100 / 4;                      //jump to end of progress bar

            Console.Write("] ");
            var percents_position = Console.CursorLeft;
            var prev_percents_done = 0;

            do
            {
                var ref_ = *reference;

                while (Console.KeyAvailable)                                            //supresses output of keys that were pressed during scan
                {
                    Console.ReadKey(true);
                }
                //var percents_done = (int)Math.Round( (double)(total_count - *reference) * 100 / total_count ); //(27 - 26) * 100 / 27
                var percents_done = (total_count - *reference + 1) * 100 / total_count; //(27 - 26) * 100 / 27

                if (percents_done < prev_percents_done) return;

                var progress_chars_count = (percents_done - prev_percents_done * 4) / 4;


                Console.CursorLeft = percents_position;
                Console.Write("{0}% done ({1} of {2})", percents_done, total_count - *reference, total_count);

                for (int i = 0; i < progress_chars_count; i++)
                {
                    Console.CursorLeft = progress_bar_position;
                    Console.Write("#");
                    progress_bar_position++;

                    prev_percents_done = percents_done / 4;
                }


                //if (percents_done % 4 == 0)
                //if (percents_done != prev_percents_done)
                //{
                //    Console.CursorLeft = progress_bar_position;
                //    Console.Write("#");

                //    prev_percents_done = percents_done;

                //    progress_bar_position++;
                //}

                Thread.Sleep(delay_between_updates);
            } while (*reference > 0);                                                      //пока не осталось сделать 0 чего-бы то ни-было

            Console.WriteLine();
            Console.WriteLine();
        }


        static void DrawProgressBar(int total_count, int reference)
        {

            //var one_percent_delay = duration_seconds * 1000 / 100;



            var delay_between_updates = 10;

            //Console.WriteLine();
            Console.Write("[");


            var progress_bar_position = Console.CursorLeft;

            Console.CursorLeft = Console.CursorLeft + 100 / 4;                      //jump to end of progress bar

            Console.Write("] ");
            var percents_position = Console.CursorLeft;
            var prev_percents_done = 0;

            while (reference != 0)                                                      //пока не осталось сделать 0 чего-бы то ни-было
            {
                while (Console.KeyAvailable)                                            //supresses output of keys that were pressed during scan
                {
                    Console.ReadKey(true);
                }
                //var percents_done = (int)Math.Round( (double)(total_count - *reference) * 100 / total_count ); //(27 - 26) * 100 / 27
                var percents_done = (total_count - reference + 1) * 100 / total_count; //(27 - 26) * 100 / 27

                var progress_chars_count = (percents_done - prev_percents_done * 4) / 4;


                Console.CursorLeft = percents_position;
                Console.Write("{0}% done ({1} of {2})", percents_done, total_count - reference, total_count);

                for (int i = 0; i < progress_chars_count; i++)
                {
                    Console.CursorLeft = progress_bar_position;
                    Console.Write("#");
                    progress_bar_position++;

                    prev_percents_done = percents_done / 4;
                }


                //if (percents_done % 4 == 0)
                //if (percents_done != prev_percents_done)
                //{
                //    Console.CursorLeft = progress_bar_position;
                //    Console.Write("#");

                //    prev_percents_done = percents_done;

                //    progress_bar_position++;
                //}

                Thread.Sleep(delay_between_updates);
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        //unsafe static void test(int* num)
        //{
        //    Console.WriteLine( *num );
        //}
    }
}
