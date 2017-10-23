using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoServiss.Repositories.Admin;
using System.Text;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using AutoServiss.Database;
using System.Collections.Generic;

namespace AutoServiss.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : ControllerBase
    {
        #region Fields

        private readonly IAdminRepository _repository;
        private readonly IHostingEnvironment _environment;

        #endregion

        #region Constructor

        public AdminController(
            IAdminRepository repository,
            IHostingEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        #endregion

        #region Markas

        [ModelStateValidationFilter]
        [HttpPost]
        [Route("admin/markas")]
        public async Task<IActionResult> InsertMarka([FromBody] Marka marka)
        {
            var result = await _repository.InsertMarkaAsync(marka);
            return StatusCode(200, new { id = result });
        }

        [ModelStateValidationFilter]
        [HttpPut]
        [Route("admin/markas")]
        public async Task<IActionResult> UpdateMarka([FromBody] Marka marka)
        {
            var result = await _repository.UpdateMarkaAsync(marka);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Markas dati atjaunināti" });
            }
            return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
        }

        [HttpDelete]
        [Route("admin/markas/{id}")]
        public async Task<IActionResult> DeleteMarka(int id)
        {
            var result = await _repository.DeleteMarkaAsync(id);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Marka izdzēsta" });
            }
            return StatusCode(400, new { messages = new List<string> { "Marka netika izdzēsta" } });
        }

        #endregion

        #region Modeļi

        [ModelStateValidationFilter]
        [HttpPost]
        [Route("admin/modeli")]
        public async Task<IActionResult> InsertModelis([FromBody] Modelis modelis)
        {
            var result = await _repository.InsertModelisAsync(modelis);
            return StatusCode(200, new { id = result });
        }

        [ModelStateValidationFilter]
        [HttpPut]
        [Route("admin/modeli")]
        public async Task<IActionResult> UpdateModelis([FromBody] Modelis modelis)
        {
            var result = await _repository.UpdateModelisAsync(modelis);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Modeļa dati atjaunināti" });
            }
            return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
        }

        [HttpDelete]
        [Route("admin/modeli/{id}")]
        public async Task<IActionResult> DeleteModelis(int id)
        {
            var result = await _repository.DeleteModelisAsync(id);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Modelis izdzēsts" });
            }
            return StatusCode(400, new { messages = new List<string> { "Modelis netika izdzēsts" } });
        }

        #endregion

        #region Logs

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

        #endregion
    }
}