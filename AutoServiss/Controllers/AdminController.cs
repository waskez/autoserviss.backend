using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoServiss.Repositories.Admin;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AutoServiss.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger _logger;
        private readonly IAdminRepository _repository;
        private readonly IHostingEnvironment _environment;

        public AdminController(
            ILogger<AdminController> logger,
            IAdminRepository repository,
            IHostingEnvironment environment)
        {
            _logger = logger;
            _repository = repository;
            _environment = environment;
        }

        [HttpGet]
        [Route("admin/logs")]
        public IActionResult Logs(DateTime date)
        {
            try
            {
                var logFolderPath = Path.Combine(_environment.ContentRootPath, "Logs");
                var logPath = Path.Combine(logFolderPath, $"log-{date:yyyyMMdd}.txt");
                if (!System.IO.File.Exists(logPath))
                {
                    logPath = Path.Combine(logFolderPath, $"{date:yyyy-MM}", $"log-{date:yyyyMMdd}.txt"); // mapēs
                }
                return Content(ReadLogFile(logPath).ToString());
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
                if (exc.InnerException != null)
                {
                    _logger.LogError(exc.InnerException.Message);
                }
                return StatusCode(500, new { message = exc.Message });
            }
        }

        private StringBuilder ReadLogFile(string path)
        {
            var text = new StringBuilder();
            if (System.IO.File.Exists(path))
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        text.AppendLine(line);
                    }
                }
            }
            return text;
        }
    }
}
