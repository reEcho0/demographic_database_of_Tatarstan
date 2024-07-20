using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql.EntityFrameworkCore;
using WebAppMVC.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

string conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(conn));  

builder.Services.AddControllersWithViews();

var app = builder.Build();



app.MapDefaultControllerRoute();

app.Run();

