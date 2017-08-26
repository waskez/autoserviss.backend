using AutoServiss.Database;
using AutoServiss.Repositories.Firma;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class FirmaController : Controller
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IFirmaRepository _repository;

        #endregion

        #region Constructor

        public FirmaController(
            ILogger<FirmaController> logger,
            IFirmaRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        #endregion

        [HttpGet]
        [Route("companies")]
        public async Task<IActionResult> List()
        {
            try
            {
                var firmas = await _repository.VisasFirmasAsync();
                return Json(new { companies = firmas });
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
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var firma = await _repository.GetFirmaAsync(id);
                return Json(new { company = firma });
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
        public async Task<IActionResult> Insert([FromBody]Klients firma)
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
                if (firma.Veids != KlientaVeids.ManaFirma)
                {
                    throw new CustomException("Nepareizs firmas Veids");
                }

                var result = await _repository.InsertFirmaAsync(firma);
                return Json(new { id = result.ToString(), message = "Izveidota jauna firma" });
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
        public async Task<IActionResult> Update([FromBody]Klients firma)
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
                if (firma.Veids != KlientaVeids.ManaFirma)
                {
                    throw new CustomException("Nepareizs firmas Veids");
                }

                var result = await _repository.UpdateFirmaAsync(firma);
                if (result > 0)
                {
                    return StatusCode(200, new { message = "Firmas dati atjaunināti" });
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _repository.DeleteFirmaAsync(id);
                if (result == 0)
                {
                    throw new CustomException("Firma netika izdzēsta");
                }
                return StatusCode(200, new { message = "Firma izdzēsta" });
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
