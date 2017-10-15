﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoServiss.Repositories.Admin;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace AutoServiss.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _repository;
        private readonly IHostingEnvironment _environment;

        public AdminController(
            IAdminRepository repository,
            IHostingEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        [HttpGet]
        [Route("admin/logs")]
        public IActionResult Logs(DateTime date)
        {
            if(ModelState.IsValid)
            {
                var logFilesPath = Path.Combine(_environment.WebRootPath, "logs", $"{date:yyyyMMdd}.txt");
                if (!System.IO.File.Exists(logFilesPath))
                {
                    logFilesPath = Path.Combine(_environment.WebRootPath, "logs", $"{date:yyyy-MM}", $"{date:yyyyMMdd}.txt"); // mapēs
                }
                return Content(ReadLogFile(logFilesPath).ToString());
            }
            else
            {
                throw new BadRequestException("Nepareiza parametra \"date\" vērtība");
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