using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoServiss.Database;
using AutoServiss.Repositories.Darbinieki;
using AutoServiss.Services.Email;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using SendGrid.Helpers.Mail;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class DarbiniekiController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDarbiniekiRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _email;
        private readonly IHostingEnvironment _environment;

        public DarbiniekiController(
            ILogger<DarbiniekiController> logger, 
            IDarbiniekiRepository repository,
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

        [HttpGet]
        [Route("employees")]
        public async Task<IActionResult> List()
        {
            try
            {
                var list = await _repository.AllDarbiniekiAsync();
                return Json(new { employees = list });
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
        [Route("employees/{id}")]
        public async Task<IActionResult> Get(int id)
        {            
            try
            {
                var emp = await _repository.GetDarbinieksAsync(id);
                if(emp == null)
                {
                    throw new CustomException("Darbinieks neeksistē");
                }
                emp.Parole = string.IsNullOrEmpty(emp.Lietotajvards) ? null : "ok"; //paroli nesūtam

                return Json(new { employee = emp });
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
        public async Task<IActionResult> Insert([FromBody]Darbinieks darbinieks)
        {
            try
            {
                if (darbinieks == null)
                {
                    throw new CustomException("Objekts Darbinieks ir null");
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
                return Json(new { id = result.ToString(), message = "Izveidots jauns darbinieks" });
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
        public async Task<IActionResult> Update([FromBody]Darbinieks darbinieks)
        {
            try
            {
                if (darbinieks == null)
                {
                    throw new CustomException("Objekts Darbinieks ir null");
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
        [Route("employees/{id}")]
        public async Task<IActionResult> Delete(int id)
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
                    throw new CustomException("Nedrīkst dzēst savu kontu");
                }

                var result = await _repository.DeleteDarbinieksAsync(id);
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
                if(result)
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

                if(response.StatusCode == System.Net.HttpStatusCode.Accepted)
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
        public async Task<IActionResult> UploadAvatarAsync()
        {
            try
            {
                var form = await _httpContextAccessor.HttpContext.Request.ReadFormAsync();
                var file = form.Files.First(x => x.Name == "avatar");
                var id = form.First(x => x.Key == "id").Value;

                if(file.Length > 0)
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
    }
}