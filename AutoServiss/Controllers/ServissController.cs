using AutoServiss.Database;
using AutoServiss.Models;
using AutoServiss.Repositories.Klienti;
using AutoServiss.Repositories.Serviss;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiss.Controllers
{
    public class ServissController : Controller
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IServissRepository _repository;

        #endregion

        #region Constructor

        public ServissController(
            ILogger<ServissController> logger,
            IServissRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        #endregion

        [HttpGet]
        [Route("service/vehicle/{id}/sheet")]
        public async Task<IActionResult> VehicleSheet(int id)
        {
            try
            {
                var mehaniki = await _repository.GetMehanikiAsync();
                var sheet = await _repository.TransportlidzeklaServisaLapaAsync(id);
                // pārbaudam vai starp ServisaLapas mehāniķiem ir kāds kurš ir izdzēsts no datubāzes
                var izdzestieMehaniki = sheet.Mehaniki.Where(sm => !mehaniki.Any(m=> m.Id == sm.Id)).ToList();
                mehaniki.AddRange(izdzestieMehaniki);

                return Json(new { sheet = sheet, mechanics = mehaniki });              
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

        [HttpPost]
        [Route("service/sheet")]
        public async Task<IActionResult> InsertSheet([FromBody]ServisaLapa sheet)
        {
            try
            {
                if (sheet == null)
                {
                    throw new CustomException("Objekts ServisaLapa ir null");
                }
                if (sheet.Mehaniki.Count == 0)
                {
                    throw new CustomException("Nav norādīti Mehāniķi");
                }                

                var result = await _repository.InsertServisaLapaAsync(sheet);
                return Json(new { id = result.ToString(), message = "Izveidota jauna servisa lapa" });
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

        [HttpPut]
        [Route("service/sheet")]
        public async Task<IActionResult> UpdateSheet([FromBody]ServisaLapa sheet)
        {
            try
            {
                if (sheet == null)
                {
                    throw new CustomException("Objekts ServisaLapa ir null");
                }
                if (sheet.Id == 0)
                {
                    throw new CustomException("Nepareizs servisa lapas identifikators");
                }
                if (sheet.Mehaniki.Count == 0)
                {
                    throw new CustomException("Nav norādīti Mehāniķi");
                }

                var result = await _repository.UpdateServisaLapaAsync(sheet);
                if (result > 0)
                {
                    return StatusCode(200, new { message = "Servisa lapas dati atjaunināti" });
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

        [HttpGet]
        [Route("service/repair")]
        public async Task<IActionResult> UnderRepair()
        {
            try
            {
                var vehicles = await _repository.PaslaikRemontaAsync();
                return Json(new { vehicles = vehicles});
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
        [Route("service/sheet/{id}/print")]
        public async Task<IActionResult> PrintSheet(int id)
        {
            try
            {
                var pdf = await _repository.PrintServisaLapaAsync(id);
                var output = new MemoryStream();
                output.Write(pdf, 0, pdf.Length);
                output.Position = 0;

                return new FileStreamResult(output, "application/pdf");
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