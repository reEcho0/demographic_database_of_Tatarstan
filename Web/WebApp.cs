using Web;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

using (PostgresContext db = new PostgresContext())
{
    var dataset = db.Demographics.ToList();
    foreach (var item in dataset)
    {
        Console.WriteLine(item.Year);
    }
    //app.Run(async (context) =>
    //{
    //    foreach (var item in dataset)
    //    {
    //        await context.Response.WriteAsJsonAsync(item);
    //    }
    //});
}

app.MapGet("/", () => "Hello World!");

app.Run();
