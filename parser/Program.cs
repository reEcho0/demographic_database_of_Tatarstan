using HtmlAgilityPack;
using System.Text;

namespace Praktika
{
    class Parser
    {
        static void Main()
        {
            // Загрузите HTML-документ из файла или URL-адреса

            var htmlDocument = new HtmlWeb().Load("https://gogov.ru/natural-increase/rt");

            // Выберите узлы(nodes) с помощью XPath 
            var actualDate = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='gu-data']//p//b").InnerHtml;
            //var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='gu-data']//ul//li");
            var birthrate = htmlDocument.DocumentNode.SelectNodes(
                "/html/body/script[11]");
            Console.WriteLine($"данные на {actualDate}\n");
            // Извлеките и отобразите данные 
            //foreach (var node in nodes)
            //{
            //    string str = node.InnerHtml.Remove(0, 3).Replace("</b>", string.Empty); ;                
            //    Console.WriteLine($"{str}\n");
            //}
            foreach(var br in birthrate)
            {
                Console.WriteLine(br.InnerText);
            }
            

        }
    }
}
//node.InnerHtml
//  /html/body/div[1]/div/div/div[3]/div/article/div/div[4]/div[2]/div[1]/div[1]/div/svg/g[7]/g[1]