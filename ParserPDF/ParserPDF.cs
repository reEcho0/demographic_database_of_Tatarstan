using HtmlAgilityPack;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text.RegularExpressions;


namespace DemographicDB
{
    public class ParserPDF 
    {
        public static void Main()
        {
            //ParserPDF parser = new ParserPDF();
            //string path = await parser.DownloadPDF();
            //var data = parser.Parser(path);
            //parser.DeletePDF(path);
        }
        public async Task<List<List<int>>> Work()
        {
            ParserPDF parser = new ParserPDF();
            var pathPDF = await parser.DownloadPDF();
            var data = parser.Parser(pathPDF);
            //foreach (var row in data) 
            //{ 
            //    foreach(var item in row) Console.WriteLine(item);
            //}
            parser.DeletePDF(pathPDF);
            return data;
        }

        public async Task<string> DownloadPDF()
        {
            string rosstat = "https://16.rosstat.gov.ru";
            //вкладка на которой находится нужный пдф файл
            var Source = new HtmlWeb().Load(rosstat + "/naselenie#");
            //ссылка на файл и его имя
            string pdfUrl = Source.DocumentNode
                .SelectSingleNode("/html/body/main/section[2]/div/div/div/div/div/div/div[2]/div/div[2]/div/div[2]/div/div/div/div/div[1]/a[@href]")
                .GetAttributeValue("href", "null");
            string pdfName = Source.DocumentNode
                .SelectSingleNode("/html/body/main/section[2]/div/div/div/div/div/div/div[2]/div/div[2]/div/div[2]/div/div/div/div/div[2]/div[1]")
                .InnerHtml.Trim();
            //скачиваем и открываем файл
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(rosstat + pdfUrl);
            using var fileStream = new FileStream($"C:\\Users\\эхо\\source\\repos\\demographic_database_of_Tatarstan\\{pdfName}.pdf", FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
            return fileStream.Name;
        }

        public void DeletePDF(string path)
        {
           File.Delete(path);
        }

        public List<List<int>> Parser(string path)
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
            //получение из отчета фрагмент текста о рождаемости и смертности
            string demographics = text.Substring(text.IndexOf(Regex.Match(text, @"\d{4}г\. \d{4}г\.").Value), text.IndexOf("     в том числе детей") - text.IndexOf(Regex.Match(text, @"\d{4}г\. \d{4}г\.").Value));
            //разбивам на список строк
            List<string> demographicsSplit = new List<string>(demographics.Split('\n', StringSplitOptions.RemoveEmptyEntries));
            demographicsSplit.RemoveAll(string.IsNullOrWhiteSpace);
            //сами данные год, родилось, умеро (используются списки, ибо в отчетах данные за 2 года)
            List<string> years = new Regex(@"г\.", RegexOptions.Compiled)
                .Replace(demographicsSplit[demographicsSplit.IndexOf(Regex.Match(demographics, @"\d{4}г\. \d{4}г\.").Value)], "")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            //поиск конкретных строк о родившихся/умерших
            string bornString = "";
            string diedString = "";
            foreach (var item in demographicsSplit)
            {
                if (Regex.IsMatch(item, @"Родившихся"))
                    bornString = item;
                if (Regex.IsMatch(item, @"Умерших"))
                    diedString = item;
            }
            //из этих строк получаем данные
            List<string> listBorn = new List<string>(demographicsSplit[demographicsSplit.IndexOf(bornString)].Split(' ', 4).SkipLast(1).Skip(1));
            List<string> listDied = new List<string>(demographicsSplit[demographicsSplit.IndexOf(diedString)].Split(' ', 4).SkipLast(1).Skip(1));
            //получение данных об миграции
            string migration = text.Substring(text.IndexOf("Миграция – всего "), text.IndexOf(Regex.Match(text, @"миграционный прирост").Value) - text.IndexOf("Миграция – всего "));
            List<string> migrationSplit = new List<string>(migration.Split(new char[] { '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
            migrationSplit.RemoveAll(string.IsNullOrWhiteSpace);
            string departureStr = "";
            string arrivalStr = "";
            foreach (var item in migrationSplit)
            {
                if (Regex.IsMatch(item, @"число прибытий"))
                    arrivalStr = item;
                if (Regex.IsMatch(item, @"число выбытий"))
                    departureStr = item;
            }
            //данные о прибывших и уехавших из региона
            List<string> listArrival = new Regex(@"([а-я])|(\d+,\d+)", RegexOptions.IgnoreCase)
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
    }
}
