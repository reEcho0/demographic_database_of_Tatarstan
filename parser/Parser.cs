﻿using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Text.RegularExpressions;

namespace Praktika
{
    class Parser
    {
        static void Main()
        {
            //ConnectDB();
        }

        static async void ConnectDB()
        {
            Environment.SetEnvironmentVariable("URL", "https://hljapwtpzmqjovchyylz.supabase.co");
            Environment.SetEnvironmentVariable("KEY", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhsamFwd3Rwem1xam92Y2h5eWx6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3MTk1NzMwMjksImV4cCI6MjAzNTE0OTAyOX0._I4a8CradEGzeMN7Lq_KWbDeayDbaCq0Ru2AoUQuHx0");
            var url = Environment.GetEnvironmentVariable("URL");
            var key = Environment.GetEnvironmentVariable("KEY");
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            };
            var supabase = new Supabase.Client(url,key,options);
            await supabase.InitializeAsync();
            //var data = new List<Demographics>();
            //var rows = ParserPDF();
            //Console.WriteLine(rows.Count);
            //foreach (var row in rows)
            //{
            //    Console.WriteLine("Connect and insert");
            //    data.Add(new Demographics { Year = row[0], Born = row[1], Died = row[2], Arrival = row[3], Departure = row[4] });
            //    Console.WriteLine($"Year = {row[0]}, Born = {row[1]}, Died = {row[2]}, Arrival = {row[3]}, Departure = {row[4]}");
            //}
            //await supabase.From<Demographics>().Insert(data);
        }

        public static void ParserPDF()
        {
            List<List<int>> rows = new List<List<int>>();
            //список старых отчетов о демографии (и не только) в Татарстане
            PdfReader[] arr_pdf = { new PdfReader("C:\\Users\\эхо\\source\\repos\\demographic_database_of_Tatarstan\\доклад 2019-2018.pdf"),
                new PdfReader("C:\\Users\\эхо\\source\\repos\\demographic_database_of_Tatarstan\\доклад 2021-2020.pdf") };
            //перебор списка
            foreach (PdfReader pdf in arr_pdf)
            {
                //преобразование из пдф в строку
                string text = "";
                for (var i = 1; i <= pdf.NumberOfPages; ++i)
                {
                    SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    text += PdfTextExtractor.GetTextFromPage(pdf, i, strategy);
                }
                pdf.Close();
                //получение из отчета данных о рождаемости и смертности
                string demographics = text.Substring(text.LastIndexOf("ДЕМОГРАФИЯ"), text.IndexOf("     в том числе детей") - text.LastIndexOf("ДЕМОГРАФИЯ"));
                List<string> demographics_split = new List<string>(demographics.Split('\n', StringSplitOptions.RemoveEmptyEntries));
                demographics_split.RemoveAll(string.IsNullOrWhiteSpace);
                //сами данные год, родилось, умеро (используются списки, ибо в отчетах данные за 2 года)
                string[] years = new Regex(@"г\.", RegexOptions.Compiled).Replace(demographics_split[4], "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                List<string> list_born = new List<string>(demographics_split[10].Split(' ', 4).SkipLast(1).Skip(1));
                List<string> list_died = new List<string>(demographics_split[11].Split(' ', 4).SkipLast(1).Skip(1));
                //получение данных об миграции
                string migration = text.Substring(text.IndexOf("Миграция населения"), text.IndexOf("с другими территориями") - text.IndexOf("Миграция населения"));
                List<string> migration_split = new List<string>(migration.Split(new char[] { '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
                migration_split.RemoveAll(string.IsNullOrWhiteSpace);
                //костыль не забыть убрать
                /**/migration_split.Remove("1)");/**/
                //костыль не забыть убрать
                //убираем все слова и уменьшаем размер списка
                for (var i = 0; i < migration_split.Count; i++)
                {
                    migration_split[i] = new Regex(@"([а-я])|(\s{2,})", RegexOptions.IgnoreCase).Replace(migration_split[i], "");
                }
                migration_split.RemoveAll(string.IsNullOrWhiteSpace);
                //данные о прибывших и уехавших из региона (удаляем второе и четвертое значение, поскольку там данные в расчете на 1000 чел.)
                List<string> list_arrival = new List<string>(migration_split[5].Split(' ',4, StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                list_arrival.RemoveAt(1);
                List<string> list_departure = new List<string>( migration_split[6].Split(' ',4, StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                list_departure.RemoveAt(1);
                List<int> row_data = new List<int>();
                //возможно проблема в конверте
                for (var i = 0; i < 2; i++) 
                {
                    row_data.AddRange(new int[] { Convert.ToInt32(years[i]),
                        Convert.ToInt32(list_born[i]), Convert.ToInt32(list_died[i]),
                        Convert.ToInt32(list_arrival[i]), Convert.ToInt32(list_died[i]) });
                }
                rows.Add(row_data);
            }
            foreach (var row in rows) 
            {
                Console.WriteLine($"Year = {row[0]}, Born = {row[1]}, Died = {row[2]}, Arrival = {row[3]}, Departure = {row[4]}");
            }
            //return rows;
        }

        static void ParseHTML()
        {
            // Загрузите HTML-документ из файла или URL-адреса
            var htmlDocument = new HtmlWeb().Load("https://gogov.ru/natural-increase/rt");

            // Выберите узлы(nodes) с помощью XPath 
            // Самые свежие данные на текущий год, не подходят для статистики, используются для общей информации и наполнения
            var actualDate = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='gu-data']//p//b").InnerHtml;
            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='gu-data']//ul//li");
            
            // Получение конкретных данных за 9 лет + последние данные
            var data = htmlDocument.DocumentNode.SelectSingleNode(
                "/html/body/script[11]");
            
            // Извлечение и отображение данных 
            // Самые свежие данные            
            Console.WriteLine($"данные на {actualDate}\n");
            foreach (var node in nodes)
            {
                string str = Regex.Replace(node.InnerText,@"<b>(.*?)</b>",string.Empty);
                Console.WriteLine($"{str}");
            }
            Console.WriteLine();
            
            // Лист "года" (в идеале конвертировать из строки в инт)
            Match matchYears = Regex.Match(data.InnerHtml, @"\{ categories: \[(.*?)\]");
            List<string> years = Regex.Match(matchYears.ToString(),@"\[(.*?)\]")
                .ToString().Trim('[',']').Split(',').ToList();
            years.RemoveRange(years.Count - 2, 2);
            foreach (var year in years)
            {
                Console.Write($"{year.Trim('\'')}\t");
            }
            Console.WriteLine();
            
            // Словарь c данными для БД
            Regex patternBirthrate = new Regex( @"\{ name: (.*?)\]");
            MatchCollection birthrate = patternBirthrate.Matches(data.InnerHtml);           
            foreach (Match match in birthrate)
            {
                List<int> numData = Regex.Match(match.Value, @"\[(.*?)\]").ToString().Trim('[', ']')
                    .ToString().Split(',').Select(s=>int.Parse(s)).ToList();
                numData.RemoveAt(numData.Count - 1);
                Dictionary<string, List<int>> structured_data = new Dictionary<string, List<int>>()
                {
                    {Regex.Match(match.Value, @"\'(.*?)\'").ToString().Trim('\''), numData}
                };
                foreach(var structured in structured_data)
                {
                    Console.WriteLine(structured.Key);
                    foreach(var num in structured.Value)
                    {
                        Console.Write($"{num}\t");
                    }
                    Console.WriteLine();
                }
            }
        }
    }

    [Supabase.Postgrest.Attributes.Table("Demographics")]
    class Demographics : BaseModel
    {
        [PrimaryKey("Year")]
        public int Year { get; set; }

        [Supabase.Postgrest.Attributes.Column("Born")]
        public int Born { get; set; }

        [Supabase.Postgrest.Attributes.Column("Died")]
        public int Died { get; set; }

        [Supabase.Postgrest.Attributes.Column("Arrival")]
        public int Arrival {  get; set; }

        [Supabase.Postgrest.Attributes.Column("Departure")]
        public int Departure {  get; set; }
    }
}
