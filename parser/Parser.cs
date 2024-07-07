using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
//using Supabase.Postgrest.Attributes;
//using Supabase.Postgrest.Models;
using System.Text.RegularExpressions;
using Npgsql;



namespace Praktika
{
    class Parser 
    {
        async static Task Main()
        {
            Parser parser = new Parser();
            //parser.ParserHTML();
            //await parser.AsyncConnectAndAdd();
            //parser.ConnectAndAdd();
            var pathPDF = await parser.DownloadPDF();
            var data = parser.ParserPDF(pathPDF);
            foreach (var row in data) 
            { 
                foreach(var item in row) Console.WriteLine(item);
            }
        }

        public async Task<string> DownloadPDF()
        {
            string rosstat = "https://16.rosstat.gov.ru";
            var Source = new HtmlWeb().Load(rosstat + "/naselenie#");
            string pdfUrl = Source.DocumentNode
                .SelectSingleNode("/html/body/main/section[2]/div/div/div/div/div/div/div[2]/div/div[2]/div/div[2]/div/div/div/div/div[1]/a[@href]")
                .GetAttributeValue("href", "null");
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(rosstat + pdfUrl);
            using var fileStream = new FileStream("C:\\Users\\эхо\\source\\repos\\demographic_database_of_Tatarstan\\doc.pdf", FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
            return fileStream.Name;
        }

        /*попытка реализовать соединение и запись в асинхронноме режиме*/
        //public async Task AsyncConnectAndAdd()
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

        /*public void ConnectAndAdd()
        //{
        //    List<List<int>> data = ParserPDF();
        //    Console.WriteLine($"count rows {data.Count}");
        //    //строка sql-запроса на добавление данных
        //    string sql = @"INSERT INTO demographics(year, born, died, arrival, departure)" +
        //            "VALUES(@year, @born, @died, @arrival, @departure)";
        //    //строка для соединения с БД
        //    string connString = "Host=aws-0-eu-central-1.pooler.supabase.com;" +
        //        "Username=postgres.hljapwtpzmqjovchyylz;" +
        //        "Password=MM0yI98jmB7eQwDQ;" +
        //        "Database=postgres";
        //    //установление соединения
        //    using NpgsqlConnection conn = new NpgsqlConnection(connString);
        //    {
        //        //открываем соединение
        //        conn.Open();
        //        //версия сервера PostgreSQL
        //        Console.WriteLine($"Postgre ver {conn.PostgreSqlVersion.ToString()}");
        //        try
        //        {
        //            //создание нового источника данных
        //            using var dataSource = NpgsqlDataSource.Create(connString);
        //            foreach (var row in data)
        //            {
        //                //создание команды
        //                using var cmd = dataSource.CreateCommand(sql);
        //                Console.WriteLine($"y={row[0]}, b={row[1]}, d={row[2]}, ar={row[3]}, de={row[4]}");
        //                //параметры команды и их значения
        //                cmd.Parameters.AddWithValue("@year", row[0]);
        //                cmd.Parameters.AddWithValue("@born", row[1]);
        //                cmd.Parameters.AddWithValue("@died", row[2]);
        //                cmd.Parameters.AddWithValue("@arrival", row[3]);
        //                cmd.Parameters.AddWithValue("@departure", row[4]);
        //                //выполнение команды
        //                cmd.ExecuteNonQuery();
        //            }
        //            Console.WriteLine("add data");
        //        }
        //        catch (NpgsqlException ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //        }
        //    }
        }*/

        public List<List<int>> ParserPDF(string path)
        {
            List<List<int>> rows = new List<List<int>>();
            //преобразование из пдф в строку
            PdfReader pdf = new PdfReader(path);
            string text = "";
            for (var i = 1; i <= pdf.NumberOfPages; ++i)
            {
                SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                text += PdfTextExtractor.GetTextFromPage(pdf, i, strategy);
            }
            pdf.Close();
            //получение из отчета данных о рождаемости и смертности
            string demographics = text.Substring(text.IndexOf(Regex.Match(text, @"\d{4}г\. \d{4}г\.").Value), text.IndexOf("     в том числе детей") - text.IndexOf(Regex.Match(text, @"\d{4}г\. \d{4}г\.").Value));
            List<string> demographicsSplit = new List<string>(demographics.Split('\n', StringSplitOptions.RemoveEmptyEntries));
            demographicsSplit.RemoveAll(string.IsNullOrWhiteSpace);
            //сами данные год, родилось, умеро (используются списки, ибо в отчетах данные за 2 года)
            List<string> years = new Regex(@"г\.", RegexOptions.Compiled)
                .Replace(demographicsSplit[demographicsSplit.IndexOf(Regex.Match(demographics, @"\d{4}г\. \d{4}г\.").Value)], "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            string bornString = "";
            string diedString = "";
            foreach (var item in demographicsSplit)
            {
                if (Regex.IsMatch(item, @"Родившихся"))
                    bornString = item;
                if (Regex.IsMatch(item, @"Умерших"))
                    diedString = item;
            }          
            List<string> listBorn = new List<string>(demographicsSplit[demographicsSplit.IndexOf(bornString)].Split(' ', 4).SkipLast(1).Skip(1));
            List<string> listDied = new List<string>(demographicsSplit[demographicsSplit.IndexOf(diedString)].Split(' ', 4).SkipLast(1).Skip(1));
            //получение данных об миграции
            string migration = text.Substring(text.IndexOf("Миграция – всего "), text.IndexOf(Regex.Match(text,@"миграционный прирост").Value) - text.IndexOf("Миграция – всего "));
            List<string> migrationSplit = new List<string>(migration.Split(new char[] { '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            migrationSplit.RemoveAll(string.IsNullOrWhiteSpace);
            string departureStr = "";
            string arrivalStr = "";
            foreach (var item in migrationSplit) 
            { 
                if(Regex.IsMatch(item, @"число прибытий"))
                    arrivalStr = item;
                if (Regex.IsMatch(item, @"число выбытий"))
                    departureStr = item;
            }
            //данные о прибывших и уехавших из региона
            List<string> listArrival = new Regex(@"([а-я])|(\d+,\d+)",RegexOptions.IgnoreCase)
                .Replace(migrationSplit[migrationSplit.IndexOf(arrivalStr)], "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> listDeparture = new Regex(@"([а-я])|(\d+,\d+)", RegexOptions.IgnoreCase)
                .Replace(migrationSplit[migrationSplit.IndexOf(departureStr)], "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            //добавление данных за каждый год общий список
            for (var i = 0; i < 2; i++)
            {
                rows.Add(new List<int>((new int[] { Convert.ToInt32(years[i]), Convert.ToInt32(listBorn[i]), Convert.ToInt32(listDied[i]), Convert.ToInt32(listArrival[i]), Convert.ToInt32(listDeparture[i]) })));
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
