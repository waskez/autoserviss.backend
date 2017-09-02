using AutoServiss.Database;
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
    public class ServissController : ControllerBase
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
                var companiesWithMechanics = await _repository.GetUznemumiArMehanikiem();
                var sheet = await _repository.TransportlidzeklaServisaLapaAsync(id);
                
                var companyMechanics = new List<Mehanikis>();
                if(sheet.Id == 0) // jaunai lapai aizpildam ar pirmo uzņēmumu un tā mehāniķiem
                {
                    var firstCompany = companiesWithMechanics.First();
                    sheet.UznemumaId = firstCompany.Id;
                    companyMechanics = firstCompany.Mehaniki;
                }
                else // esošajai servisa lapai aizpildam uzņēmuma mehāniķu sarakstu
                {
                    var sheetCompany = companiesWithMechanics.Where(c => c.Id == sheet.UznemumaId).Single();
                    companyMechanics = sheetCompany.Mehaniki;
                    // gadījumā ja kāds no esošajiem servisa lapas mehāniķiem (darbiniekiem) ir izdzēsts
                    companyMechanics.AddRange(sheet.Mehaniki.Where(sm => sheetCompany.Mehaniki.All(m => m.Id != sm.Id)));
                }
                return Ok(new { sheet = sheet, companies = companiesWithMechanics, mechanics = companyMechanics });              
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
                if (sheet.UznemumaId == 0)
                {
                    throw new CustomException("Nepareizs uzņēmuma identifikators");
                }
                if (sheet.Mehaniki.Count == 0)
                {
                    throw new CustomException("Nav norādīti Mehāniķi");
                }                

                var result = await _repository.InsertServisaLapaAsync(sheet);
                return Ok(new { id = result.ToString(), message = "Izveidota jauna servisa lapa" });
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
                if (sheet.UznemumaId == 0)
                {
                    throw new CustomException("Nepareizs uzņēmuma identifikators");
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
                return Ok(new { vehicles = vehicles});
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