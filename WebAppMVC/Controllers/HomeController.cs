using Microsoft.AspNetCore.Mvc;
using WebAppMVC.Models;
using Microsoft.EntityFrameworkCore;


namespace WebAppMVC.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext db;
        public HomeController(ApplicationContext context)
        {
            this.db = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.Demographics.ToListAsync());
        }

        //public IActionResult Index()
        //{
        //    var dataset = db.Demographics.ToList();
        //    ViewBag.Label = dataset.Select(prop => new { label = prop.Year }).ToList();
        //    ViewBag.Born = dataset.Select(prop => new { born = prop.Born }).ToList();
        //    return View(dataset);
        //}
    }
}
