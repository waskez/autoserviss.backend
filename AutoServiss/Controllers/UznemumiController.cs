using AutoServiss.Database;
using AutoServiss.Models;
using AutoServiss.Repositories.Uznemumi;
using AutoServiss.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class UznemumiController : ControllerBase
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IUznemumiRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _email;
        private readonly IHostingEnvironment _environment;

        #endregion

        #region Constructor

        public UznemumiController(
            ILogger<UznemumiController> logger,
            IUznemumiRepository repository,
            IEmailService email,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment environment)
        {
            _logger = logger;
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
            try
            {
                var firmas = await _repository.AllUznemumiAsync();
                return Ok(new { companies = firmas });
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

        [HttpGet]
        [Route("companies/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            try
            {
                var firma = await _repository.GetUznemumsAsync(id);
                return Ok(new { company = firma });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("companies")]
        public async Task<IActionResult> InsertCompany([FromBody]Uznemums firma)
        {
            try
            {
                if (firma == null)
                {
                    throw new CustomException("Objekts Firma ir null");
                }
                if (string.IsNullOrEmpty(firma.Nosaukums))
                {
                    throw new CustomException("Nav norādīts Nosaukums");
                }
                if (string.IsNullOrEmpty(firma.RegNumurs))
                {
                    throw new CustomException("Nav norādīts Reģistrācijas numurs");
                }
                if (string.IsNullOrEmpty(firma.PvnNumurs))
                {
                    throw new CustomException("Nav norādīts PVN maksātāja numurs");
                }
                if (string.IsNullOrEmpty(firma.Epasts))
                {
                    throw new CustomException("Nav norādīta E-pasta adrese");
                }
                if (string.IsNullOrEmpty(firma.Talrunis))
                {
                    throw new CustomException("Nav norādīts Tālrunis");
                }

                var result = await _repository.InsertUznemumsAsync(firma);
                return Ok(new { id = result.ToString(), message = "Izveidots jauns uzņēmums" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("companies")]
        public async Task<IActionResult> UpdateCompany([FromBody]Uznemums firma)
        {
            try
            {
                if (firma == null)
                {
                    throw new CustomException("Objekts Uzņēmums ir null");
                }
                if (string.IsNullOrEmpty(firma.Nosaukums))
                {
                    throw new CustomException("Nav norādīts Nosaukums");
                }
                if (string.IsNullOrEmpty(firma.RegNumurs))
                {
                    throw new CustomException("Nav norādīts Reģistrācijas numurs");
                }
                if (string.IsNullOrEmpty(firma.PvnNumurs))
                {
                    throw new CustomException("Nav norādīts PVN maksātāja numurs");
                }
                if (string.IsNullOrEmpty(firma.Epasts))
                {
                    throw new CustomException("Nav norādīta E-pasta adrese");
                }
                if (string.IsNullOrEmpty(firma.Talrunis))
                {
                    throw new CustomException("Nav norādīts Tālrunis");
                }

                var result = await _repository.UpdateUznemumsAsync(firma);
                if (result > 0)
                {
                    return StatusCode(200, new { message = "Uzņēmuma dati atjaunināti" });
                }
                return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [Route("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            try
            {
                var result = await _repository.DeleteUznemumsAsync(id);
                if (result == 0)
                {
                    throw new CustomException($"Uzņemums ar Id={id} netika izdzēsta");
                }
                // dzēšam arā darbiniekus kuri nav piesaistīti citam uzņēmumam
                await _repository.DeleteDarbiniekiBezUznemumaAsync(id);
                return StatusCode(200, new { message = "Uzņēmums izdzēsts" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        #endregion

        #region Darbinieki

        [HttpPost]
        [Route("employees/search")]
        public async Task<IActionResult> SearchEmployee([FromBody]SearchTerm term)
        {
            try
            {
                if(term.Id == 0)
                {
                    throw new CustomException("Nav norādīts uzņemuma identifikators");
                }
                var employees = await _repository.SearchDarbinieksAsync(term.Value, term.Id);
                return Ok(new { darbinieki = employees });
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

        [HttpGet]
        [Route("companies/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> GetEmployee(int companyId, int employeeId)
        {
            try
            {
                if(employeeId == 0) // jauns darbinieks - atgriežam uzņēmuma datus
                {
                    var emp = new Darbinieks
                    {
                        Uznemums = await _repository.GetUznemumsForDarbinieksAsync(companyId)
                    };
                    if (emp.Uznemums == null)
                    {
                        throw new CustomException($"Jaunā darbinieka uzņēmums ar Id={companyId} netika atrasts");
                    }
                    return Ok(new { employee = emp });
                }

                var employee = await _repository.GetDarbinieksForEditAsync(companyId, employeeId);
                employee.Parole = string.IsNullOrEmpty(employee.Lietotajvards) ? null : "ok"; //paroli nesūtam
                return Ok(new { employee = employee });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("companies/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> AppendEmployee(int companyId, int employeeId)
        {
            try
            {
                if (companyId == 0)
                {
                    throw new CustomException("Nepareizs uzņēmuma identifikators");
                }
                if (employeeId == 0)
                {
                    throw new CustomException("Nepareizs darbinieka identifikators");
                }

                var result = await _repository.AppendDarbinieksAsync(companyId, employeeId);
                if (result > 0)
                {
                    return StatusCode(200, new { message = "Pievienots jauns darbinieks" });
                }
                return StatusCode(400, new { message = "Neizdevās pievienot darbinieku" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("employees")]
        public async Task<IActionResult> InsertEmployee([FromBody]Darbinieks darbinieks)
        {
            try
            {
                if (darbinieks == null)
                {
                    throw new CustomException("Objekts Darbinieks ir null");
                }
                if (darbinieks.Uznemums.Id == 0)
                {
                    throw new CustomException("Nepareizs uzņēmuma identifikators");
                }
                if (string.IsNullOrEmpty(darbinieks.PilnsVards))
                {
                    throw new CustomException("Nav norādīts PilnsVards");
                }
                if (string.IsNullOrEmpty(darbinieks.Amats))
                {
                    throw new CustomException("Nav norādīts Amats");
                }
                if (!string.IsNullOrEmpty(darbinieks.Lietotajvards) && string.IsNullOrEmpty(darbinieks.Parole))
                {
                    throw new CustomException("Lietotājvārdam nav norādīta parole");
                }
                if (string.IsNullOrEmpty(darbinieks.Lietotajvards) && !string.IsNullOrEmpty(darbinieks.Parole))
                {
                    throw new CustomException("Parolei nav norādīts lietotājvārds");
                }

                var result = await _repository.InsertDarbinieksAsync(darbinieks);
                return Ok(new { id = result.ToString(), message = "Izveidots jauns darbinieks" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("employees")]
        public async Task<IActionResult> UpdateEmployee([FromBody]Darbinieks darbinieks)
        {
            try
            {
                if (darbinieks == null)
                {
                    throw new CustomException("Objekts Darbinieks ir null");
                }
                if (darbinieks.Uznemums.Id == 0)
                {
                    throw new CustomException("Nepareizs uzņēmuma identifikators");
                }
                if (string.IsNullOrEmpty(darbinieks.PilnsVards))
                {
                    throw new CustomException("Nav norādīts PilnsVards");
                }
                if (string.IsNullOrEmpty(darbinieks.Amats))
                {
                    throw new CustomException("Nav norādīts Amats");
                }

                var result = await _repository.UpdateDarbinieksAsync(darbinieks);
                if (result > 0)
                {
                    return StatusCode(200, new { message = "Darbinieka dati atjaunināti" });
                }
                return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [Route("companies/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> DeleteEmployee(int companyId, int employeeId)
        {
            try
            {
                if (companyId == 0)
                {
                    throw new CustomException("Nepareizs uzņēmuma identifikators");
                }
                if (employeeId == 0)
                {
                    throw new CustomException("Nepareizs darbinieka identifikators");
                }

                //lai neļautu dzēst pašam sevi
                var currentUserId = _httpContextAccessor.HttpContext.User.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .Single();

                if (currentUserId == employeeId.ToString())
                {
                    throw new CustomException("Nedrīkst dzēst savu kontu");
                }

                var result = await _repository.DeleteDarbinieksAsync(companyId, employeeId);
                if (result == 0)
                {
                    throw new CustomException("Darbinieks netika izdzēsts");
                }
                return StatusCode(200, new { message = "Darbinieks izdzēsts" }); ;
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [Route("employee/lock/{id}")]
        [HttpPut]
        public async Task<IActionResult> LockUnlock(int id)
        {
            try
            {
                //lai neļautu dzēst pašam sevi
                var currentUserId = _httpContextAccessor.HttpContext.User.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .Single();

                if (currentUserId == id.ToString())
                {
                    throw new CustomException("Nedrīkst bloķēt savu kontu");
                }

                var result = await _repository.LockUnlockAsync(id);
                if (result)
                {
                    return StatusCode(200, new { message = "Lietotāja konts atbloķēts" });
                }
                else
                {
                    return StatusCode(200, new { message = "Lietotāja konts bloķēts" });
                }
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Route("employee/pwd")]
        [HttpPut]
        public async Task<IActionResult> ChangePassword([FromBody]KeyValuePair<int, string> pwd)
        {
            try
            {
                await _repository.ChangePasswordAsync(pwd.Key, pwd.Value);
                return StatusCode(200, new { message = "Parole nomainīta" });
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [AllowAnonymous]
        [Route("employee/forgot")]
        [HttpPost]
        public async Task<IActionResult> SendPassword([FromBody]KeyValuePair<int, string> email)
        {
            try
            {
                var pwd = await _repository.GetPasswordByEmailAsync(email.Value);

                var to = new List<EmailAddress> { new EmailAddress(email.Value) };
                var body = $"Jūsu parole ir: {pwd}";

                var response = await _email.SendEmailAsync(to, "AutoServiss", body);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    return StatusCode(200, new { message = "Parole nosūtīta" });
                }

                throw new CustomException("TODO: Jāpstrādā neveiksmīga e-pasta nosūtīšana");
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        [Authorize(Policy = "Admin")]
        [Route("employee/avatar")]
        [HttpPost]
        public async Task<IActionResult> UploadAvatar()
        {
            try
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
                else
                {
                    throw new CustomException("Fails ir tukšs");
                }
            }
            catch (CustomException cexc)
            {
                _logger.LogWarning(cexc.Message);
                return StatusCode(400, new { message = cexc.Message });
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

        #endregion
    }
}