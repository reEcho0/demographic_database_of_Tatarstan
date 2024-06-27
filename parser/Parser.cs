using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Praktika
{
    class Parser
    {
        static void Main()
        {
            PdfReader[] arr_pdf = { new PdfReader("C:\\Users\\mega0\\OneDrive\\Рабочий стол\\практика\\parser\\доклад 2019-2018.pdf"),
                new PdfReader("C:\\Users\\mega0\\OneDrive\\Рабочий стол\\практика\\parser\\доклад 2021-2020.pdf") };
            foreach (PdfReader pdf in arr_pdf)
            {
                string text = "";
                for (var i = 1; i <= pdf.NumberOfPages; ++i)
                {
                    SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    text += PdfTextExtractor.GetTextFromPage(pdf, i, strategy);
                }
                pdf.Close();
                var demographics = text.Substring(text.LastIndexOf("ДЕМОГРАФИЯ"), text.IndexOf("     в том числе детей") - text.LastIndexOf("ДЕМОГРАФИЯ"));
                //Console.WriteLine(demographics + "\n
                List<string> demographics_split = new List<string>(demographics.Split('\n', StringSplitOptions.RemoveEmptyEntries));
                demographics_split.RemoveAll(string.IsNullOrWhiteSpace);
                //var years = new Regex(@"г\.", RegexOptions.Compiled).Replace(demographics_split[4], "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                //var born = demographics_split[10].Split(' ', 4).SkipLast(1);
                
                //var died = demographics_split[11].Split(' ', 4).SkipLast(1);
                
                var migration = text.Substring(text.IndexOf("Миграция населения"), text.IndexOf("с другими территориями") - text.IndexOf("Миграция населения"));
                List<string> migration_split = new List<string>(migration.Split('\n', StringSplitOptions.RemoveEmptyEntries));
                migration_split.RemoveAll(string.IsNullOrWhiteSpace);

                for (var i = 0; i < migration_split.Count; i++)
                {
                    Console.WriteLine(i + "\t" + migration_split[i]);
                }
            }
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

    class Demographics
    {
        public int Year { get; set; }
        public int Born { get; set; }
        public int Died { get; set; }
        public Demographics(int year, int born, int died)
        {
            Year = year;
            Born = born;
            Died = died;
        }
    }
}
