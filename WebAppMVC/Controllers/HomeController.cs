using Microsoft.AspNetCore.Mvc;
using WebAppMVC.Models;
using Microsoft.EntityFrameworkCore;
using DemographicDB;

namespace WebAppMVC.Controllers
{

    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        public HomeController(ApplicationContext context, ILogger<HomeController> log)
        {
            this.db = context;
            _logger = log;
            _logger.LogInformation($"HomeControler. Today: {DateTime.Now}");
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index View");
            return View(await db.Demographics.ToListAsync());
        }



    }

    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    internal class ScopedProcessingService : IScopedProcessingService
    {
        //private int executionCount = 0;
        ApplicationContext db;
        private readonly ILogger _logger;

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger, ApplicationContext context)
        {
            _logger = logger;
            this.db = context;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //executionCount++;

                _logger.LogInformation(
                    "after delay");

                await Task.Delay(10000);
                _logger.LogInformation(
                    "before delay");
                _logger.LogInformation($"first \'year\' in db ");
            }
        }
    }

    public class ConsumeScopedServiceHostedService : BackgroundService
    {
        private readonly ILogger<ConsumeScopedServiceHostedService> _logger;

        public ConsumeScopedServiceHostedService(IServiceProvider services,
            ILogger<ConsumeScopedServiceHostedService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is working.");

            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedProcessingService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
