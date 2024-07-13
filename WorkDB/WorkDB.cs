using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;



namespace DemographicDB
{
    public class WorkDB
    {
        public async static Task Main()
        {
            var db = new WorkDB();
            List<List<int>> data = await new ParserPDF().Work();
            //foreach (var item in data)
            //{
            //    await Console.Out.WriteLineAsync($"{item[0]}   {item[1]}   {item[2]}   {item[3]}   {item[4]}");
            //}
            NpgsqlDataSource source = await db.ConnectDB();
            await db.AddData(data,source);

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
