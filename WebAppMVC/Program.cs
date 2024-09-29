using Microsoft.EntityFrameworkCore;
using WebAppMVC;
using WebAppMVC.Controllers;
using WebAppMVC.Models;

var builder = WebApplication.CreateBuilder(args);

string conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(conn));  

builder.Services.AddControllersWithViews();

//builder.Services.AddHostedService<ConsumeScopedServiceHostedService>();
//builder.Services.AddScoped<IScopedProcessingService, ScopedProcessingService>();

var app = builder.Build();

app.MapDefaultControllerRoute();

app.Run();


