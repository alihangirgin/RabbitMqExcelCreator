using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMqExcelCreator.Hubs;

namespace RabbitMqExcelCreator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHubContext<FileHub> _fileHubContext;

        public FileController(AppDbContext appDbContext, IHubContext<FileHub> fileHubContext)
        {
            _appDbContext = appDbContext;
            _fileHubContext = fileHubContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAsync([FromForm] IFormFile iFormFile, [FromForm] Guid userFileId)
        {
            if (iFormFile is not { Length: > 0 }) return BadRequest();

            var userFile = await _appDbContext.UserFiles.FirstOrDefaultAsync(x => x.Id == userFileId);
            if (userFile == null) return BadRequest();

            var filePath = $"{userFile.FileName}.{Path.GetExtension(iFormFile.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await using FileStream stream = new(path, FileMode.Create);
            await iFormFile.CopyToAsync(stream);

            userFile.FilePath = filePath;
            userFile.CreatedAt = DateTime.Now;
            userFile.FileStatus = FileStatus.Completed;
            _appDbContext.Update(userFile);
            await _appDbContext.SaveChangesAsync();

            await _fileHubContext.Clients.User(userFile.UserId).SendAsync("FileUploadComplete");

            return Ok();
        }
    }
}
