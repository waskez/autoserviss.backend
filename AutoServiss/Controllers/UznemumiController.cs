using AutoServiss.Database;
using AutoServiss.Models;
using AutoServiss.Repositories.Uznemumi;
using AutoServiss.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class UznemumiController : ControllerBase
    {
        #region Fields

        private readonly IUznemumiRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _email;
        private readonly IHostingEnvironment _environment;

        #endregion

        #region Constructor

        public UznemumiController(
            IUznemumiRepository repository,
            IEmailService email,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment environment)
        {
            _repository = repository;
            _email = email;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        #endregion

        #region Uzņēmumi

        [HttpGet]
        [Route("companies")]
        public async Task<IActionResult> ListCompanies()
        {
            return StatusCode(200, new { companies = await _repository.AllUznemumiAsync() });
        }

        [HttpGet]
        [Route("companies/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            return StatusCode(200, new { company = await _repository.GetUznemumsAsync(id) });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("companies")]
        public async Task<IActionResult> InsertCompany([FromBody]Uznemums firma)
        {
            var result = await _repository.InsertUznemumsAsync(firma);
            return StatusCode(200, new { id = result.ToString(), message = "Izveidots jauns uzņēmums" });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("companies")]
        public async Task<IActionResult> UpdateCompany([FromBody]Uznemums firma)
        {
            var result = await _repository.UpdateUznemumsAsync(firma);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Uzņēmuma dati atjaunināti" });
            }
            return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [Route("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var result = await _repository.DeleteUznemumsAsync(id);
            if (result > 0)
            {
                // dzēšam arā darbiniekus kuri nav piesaistīti citam uzņēmumam
                await _repository.DeleteDarbiniekiBezUznemumaAsync(id);
                return StatusCode(200, new { message = "Uzņēmums izdzēsts" });
            }            
            return StatusCode(400, new { messages = new List<string> { "Uzņemums netika izdzēsta" } });
        }

        #endregion

        #region Darbinieki

        [ModelStateValidationFilter]
        [HttpPost]
        [Route("employees/search")]
        public async Task<IActionResult> SearchEmployee([FromBody]SearchTerm term)
        {
            if(term.Id == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nav norādīts uzņemuma identifikators" } });
            }
            return StatusCode(200, new { darbinieki = await _repository.SearchDarbinieksAsync(term.Value, term.Id) });
        }

        [HttpGet]
        [Route("companies/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> GetEmployee(int companyId, int employeeId)
        {
            if(employeeId == 0) // jauns darbinieks - atgriežam uzņēmuma datus
            {
                var emp = new Darbinieks
                {
                    Uznemums = await _repository.GetUznemumsForDarbinieksAsync(companyId)
                };
                if (emp.Uznemums == null)
                {
                    return StatusCode(400, new { messages = new List<string> { "Jaunā darbinieka uzņēmums netika atrasts" } });
                }
                return StatusCode(200, new { employee = emp });
            }

            var employee = await _repository.GetDarbinieksForEditAsync(companyId, employeeId);
            employee.Parole = string.IsNullOrEmpty(employee.Lietotajvards) ? null : "ok"; //paroli nesūtam
            return Ok(new { employee = employee });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("companies/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> AppendEmployee(int companyId, int employeeId)
        {
            if (companyId == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs uzņēmuma identifikators" } });
            }
            if (employeeId == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs darbinieka identifikators" } });
            }

            var result = await _repository.AppendDarbinieksAsync(companyId, employeeId);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Pievienots jauns darbinieks" });
            }
            return StatusCode(400, new { message = "Neizdevās pievienot darbinieku" });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("employees")]
        public async Task<IActionResult> InsertEmployee([FromBody]Darbinieks darbinieks)
        {
            if (darbinieks.Uznemums.Id == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs uzņēmuma identifikators" } });
            }
            if (!string.IsNullOrEmpty(darbinieks.Lietotajvards) && string.IsNullOrEmpty(darbinieks.Parole))
            {
                return StatusCode(400, new { messages = new List<string> { "Lietotājvārdam nav norādīta parole" } });
            }
            if (string.IsNullOrEmpty(darbinieks.Lietotajvards) && !string.IsNullOrEmpty(darbinieks.Parole))
            {
                return StatusCode(400, new { messages = new List<string> { "Parolei nav norādīts lietotājvārds" } });
            }

            var result = await _repository.InsertDarbinieksAsync(darbinieks);
            return StatusCode(200, new { id = result.ToString(), message = "Izveidots jauns darbinieks" });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("employees")]
        public async Task<IActionResult> UpdateEmployee([FromBody]Darbinieks darbinieks)
        {
            if (darbinieks.Uznemums.Id == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs uzņēmuma identifikators" } });
            }

            var result = await _repository.UpdateDarbinieksAsync(darbinieks);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Darbinieka dati atjaunināti" });
            }
            return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [Route("companies/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(int companyId, int employeeId)
        {
            if (companyId == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs uzņēmuma identifikators" } });
            }
            if (employeeId == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs darbinieka identifikators" } });
            }

            //lai neļautu dzēst pašam sevi
            var currentUserId = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .Single();

            if (currentUserId == employeeId.ToString())
            {
                return StatusCode(400, new { messages = new List<string> { "Nedrīkst dzēst savu kontu" } });
            }

            var result = await _repository.DeleteDarbinieksAsync(companyId, employeeId);
            if (result == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Darbinieks netika izdzēsts" } });
            }
            return StatusCode(200, new { message = "Darbinieks izdzēsts" });
        }

        [Authorize(Policy = "Admin")]
        [Route("employee/lock/{id}")]
        [HttpPut]
        public async Task<IActionResult> LockUnlock(int id)
        {
            //lai neļautu dzēst pašam sevi
            var currentUserId = _httpContextAccessor.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value)
                .Single();

            if (currentUserId == id.ToString())
            {
                return StatusCode(400, new { messages = new List<string> { "Nedrīkst bloķēt savu kontu" } });
            }

            var result = await _repository.LockUnlockAsync(id);
            if (result)
            {
                return StatusCode(200, new { message = "Lietotāja konts atbloķēts" });
            }
            return StatusCode(200, new { message = "Lietotāja konts bloķēts" });
        }

        [Route("employee/pwd")]
        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody]KeyValuePair<int, string> pwd)
        {
            await _repository.ChangePasswordAsync(pwd.Key, pwd.Value);
            return StatusCode(200, new { message = "Parole nomainīta" });
        }

        [AllowAnonymous]
        [Route("employee/forgot")]
        [HttpPost]
        public async Task<IActionResult> SendPassword([FromBody]KeyValuePair<string, string> email)
        {
            var pwd = await _repository.GetPasswordByEmailAsync(email.Value);

            var to = new List<MailAddress> { new MailAddress(email.Value) };
            var body = $"Jūsu parole ir: {pwd}";

            await _email.SendEmailAsync(to, "AutoServiss", body);

            return StatusCode(200, new { message = "Parole nosūtīta" });
        }

        [Authorize(Policy = "Admin")]
        [Route("employee/avatar")]
        [HttpPost]
        public async Task<IActionResult> UploadAvatar()
        {
            var form = await _httpContextAccessor.HttpContext.Request.ReadFormAsync();
            var file = form.Files.First(x => x.Name == "avatar");
            var id = form.First(x => x.Key == "id").Value;

            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                var imgPath = $"img\\avatars\\{id}{extension}";
                var avatarPath = Path.Combine(_environment.WebRootPath, imgPath);
                using (var fileStream = new FileStream(avatarPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    await _repository.ChangeAvatarAsync(Convert.ToInt32(id), imgPath);
                }

                return StatusCode(200, new { avatar = imgPath });
            }
                
            throw new BadRequestException("Fails ir tukšs");                
        }

        #endregion
    }
}