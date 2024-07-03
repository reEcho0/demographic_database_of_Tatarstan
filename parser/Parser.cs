using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
//using Supabase.Postgrest.Attributes;
//using Supabase.Postgrest.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace Praktika
{
    class Parser 
    {
        public static void Main()
        {
            Parser parser = new Parser();
            //parser.AsyncConnectAndAdd();
            parser.ConnectAndAdd();
        }
        /*попытка реализовать соединение и запись в асинхронноме режиме*/ 

        //public async void AsyncConnectAndAdd()
        //{
        //    var data = ParserPDF();
        //    var sql = @"INSERT INTO Demographics(year, born, died, arrival, departure)" +
        //            "VALUES(@year, @born, @died, @arrival, @departure)";
        //    string connString = "Host=aws-0-eu-central-1.pooler.supabase.com;" +
        //        "Username=postgres.hljapwtpzmqjovchyylz;" +
        //        "Password=MM0yI98jmB7eQwDQ;" +
        //        "Database=postgres";
        //    await using var conn = new NpgsqlConnection(connString);
        //    {
        //        await conn.OpenAsync();
        //        await Console.Out.WriteLineAsync($"postgre ver {conn.PostgreSqlVersion}");
        //        try
        //        {
        //            await using var dataSource = NpgsqlDataSource.Create(connString);
        //            foreach (var row in data)
        //            {
        //                await using var cmd = dataSource.CreateCommand(sql);
        //                cmd.Parameters.AddWithValue("@year", row[0]);
        //                cmd.Parameters.AddWithValue("@born", row[1]);
        //                cmd.Parameters.AddWithValue("@died", row[2]);
        //                cmd.Parameters.AddWithValue("@arrival", row[3]);
        //                cmd.Parameters.AddWithValue("@departure", row[4]);
        //                await cmd.ExecuteNonQueryAsync();               
        //            }
        //            Console.WriteLine("add data");
        //        }
        //        catch (NpgsqlException ex) { Console.WriteLine($"Error: {ex.Message}"); }
        //    }
        //}

        public void ConnectAndAdd()
        {
            List<List<int>> data = ParserPDF();
            Console.WriteLine($"count rows {data.Count}");
            //строка sql-запроса на добавление данных
            string sql = @"INSERT INTO demographics(year, born, died, arrival, departure)" +
                    "VALUES(@year, @born, @died, @arrival, @departure)";
            //строка для соединения с БД
            string connString = "Host=aws-0-eu-central-1.pooler.supabase.com;" +
                "Username=postgres.hljapwtpzmqjovchyylz;" +
                "Password=MM0yI98jmB7eQwDQ;" +
                "Database=postgres";
            //установление соединения
            using NpgsqlConnection conn = new NpgsqlConnection(connString);
            {
                //открываем соединение
                conn.Open();
                //версия сервера PostgreSQL
                Console.WriteLine($"Postgre ver {conn.PostgreSqlVersion.ToString()}");
                try
                {
                    //создание нового источника данных
                    using var dataSource = NpgsqlDataSource.Create(connString);
                    foreach (var row in data)
                    {
                        //создание команды
                        using var cmd = dataSource.CreateCommand(sql);
                        Console.WriteLine($"y={row[0]}, b={row[1]}, d={row[2]}, ar={row[3]}, de={row[4]}");
                        //параметры команды и их значения
                        cmd.Parameters.AddWithValue("@year", row[0]);
                        cmd.Parameters.AddWithValue("@born", row[1]);
                        cmd.Parameters.AddWithValue("@died", row[2]);
                        cmd.Parameters.AddWithValue("@arrival", row[3]);
                        cmd.Parameters.AddWithValue("@departure", row[4]);
                        //выполнение команды
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("add data");
                }
                catch (NpgsqlException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public List<List<int>> ParserPDF()
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
                //данные о прибывших и уехавших из региона
                //(удаляем второе и четвертое значение, поскольку там данные в расчете на 1000 чел.)
                List<string> list_arrival = new List<string>(migration_split[5].Split(' ',4, StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                list_arrival.RemoveAt(1);
                List<string> list_departure = new List<string>( migration_split[6].Split(' ',4, StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                list_departure.RemoveAt(1);
                List<int> row_data = new List<int>();
                //добавление данных за каждый год общий список
                for (var i = 0; i < 2; i++) 
                {
                    rows.Add(new List<int>((new int[] { Convert.ToInt32(years[i]), Convert.ToInt32(list_born[i]), Convert.ToInt32(list_died[i]),Convert.ToInt32(list_arrival[i]), Convert.ToInt32(list_departure[i]) })));                    
                }
            }
            return rows;
        }
        //парсер сайта с данными о рождаемости смертности,
        //в дальнейшем переписать под другой ссайт и другие данные и вынести в другой файл
        public void ParserHTML()
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
}
