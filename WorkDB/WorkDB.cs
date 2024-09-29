using Npgsql;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace DemographicDB
{
    public class WorkDB
    {
        public async static Task Main()
        {
            //var db = new WorkDB();
            //List<List<int>> data = await new ParserPDF().Work();
            ////foreach (var item in data)
            ////{
            ////    await Console.Out.WriteLineAsync($"{item[0]}   {item[1]}   {item[2]}   {item[3]}   {item[4]}");
            ////}
            //NpgsqlDataSource source = await db.ConnectDB();
            //await db.AddData(data,source);
            await UpdateData();
        }

        public static async Task UpdateData()
        {
            NpgsqlDataSource source = await new WorkDB().ConnectDB();
            string sql = @"select @DateUpdate,@Year from Demographics" +
                "ORDER BY Year DESC"+
                "LIMIT 1";
            await using var cmd = source.CreateCommand(sql);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            Console.WriteLine(reader[0]); 
            //DateTime lastUpdateDB = DateTime.Now;
            //HtmlDocument rosstat = new HtmlWeb().Load("https://16.rosstat.gov.ru/naselenie#");
            //string aboutPDF = rosstat.DocumentNode.SelectSingleNode("/html/body/main/section[2]/div/div/div/div/div/div/div[2]/div/div[2]/div/div[2]/div/div/div/div/div[2]/div[2]").InnerHtml;
            //DateTime dataDownloadPDF = DateTime.Parse(aboutPDF.Substring(aboutPDF.IndexOf(Regex.Match(aboutPDF, @"\d{2}\.\d{2}.\d{4}").Value)));
            //if (lastUpdateDB < dataDownloadPDF)
            //{
            //    Console.WriteLine(true);
            //}
            //else 
            //{
            //    Console.WriteLine(false);
            //}
        }

        public async Task<NpgsqlDataSource> ConnectDB()
        {
            string connString = "Host=aws-0-eu-central-1.pooler.supabase.com;" +
                "Username=postgres.hljapwtpzmqjovchyylz;" +
                "Password=MM0yI98jmB7eQwDQ;" +
                "Database=postgres";
            await using var conn = new NpgsqlConnection(connString);
            {
                await conn.OpenAsync();
                try 
                { 
                    await Console.Out.WriteLineAsync($"postgre ver {conn.PostgreSqlVersion}");
                    var dataSource = NpgsqlDataSource.Create(connString);
                    return dataSource;
                }
                catch (NpgsqlException ex) 
                { 
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            } 
        }
        public async Task AddData(List<List<int>> data, NpgsqlDataSource source)
        {
            var dataSourse = source;
            var sql = @"INSERT INTO Demographics(year, born, died, arrival, departure)" +
                    "VALUES(@year, @born, @died, @arrival, @departure)";
            foreach (var row in data) 
            {
                await Console.Out.WriteLineAsync("add data");
                await using var cmd = dataSourse.CreateCommand(sql);
                cmd.Parameters.AddWithValue("@year", row[0]);
                cmd.Parameters.AddWithValue("@born", row[1]);
                cmd.Parameters.AddWithValue("@died", row[2]);
                cmd.Parameters.AddWithValue("@arrival", row[3]);
                cmd.Parameters.AddWithValue("@departure", row[4]);
                await cmd.ExecuteNonQueryAsync();
            }
            await Console.Out.WriteLineAsync("data received");
        }
    }
}
