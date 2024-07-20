using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Npgsql.EntityFrameworkCore;
using WebAppMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WebAppMVC.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext db;
        public HomeController(ApplicationContext context)
        {
            this.db = context;
        }

        //public async Task<IActionResult> Index()
        //{
        //    return View(await db.Demographics.ToListAsync());
        //}

        public IActionResult Index()
        {
            ViewBag.Data =  Json(db.Demographics);
            return View();
        }
    }
}
