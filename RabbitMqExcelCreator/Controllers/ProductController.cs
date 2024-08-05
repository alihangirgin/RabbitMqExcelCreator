using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMqExcelCreator.Models;
using RabbitMqExcelCreator.Services;

namespace RabbitMqExcelCreator.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly IRabbitMqClientService _rabbitMqClientService;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext appDbContext, IRabbitMqClientService rabbitMqClientService)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _rabbitMqClientService = rabbitMqClientService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateExcel()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) RedirectToAction("Files");
            UserFile userFile = new()
            {
                FileName = $"{Guid.NewGuid().ToString().Substring(1, 10)}",
                FileStatus = FileStatus.InProgress,
                UserId = user.Id,

            };
            await _appDbContext.AddAsync(userFile);
            await _appDbContext.SaveChangesAsync();

            _rabbitMqClientService.Connect();
            _rabbitMqClientService.PublishMessage(new CreateExcelMessage() { UserFileId = userFile.Id});

            TempData["startCreatingExcel"] = true;
            return RedirectToAction("Files");
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null) return View();

            var files = await _appDbContext.UserFiles.Where(x => x.UserId == user.Id).OrderByDescending(x=> x.CreatedAt).ToListAsync();
            return View(files);
        }
    }
}
