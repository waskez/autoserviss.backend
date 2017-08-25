using System;
using System.Threading.Tasks;
using AutoServiss.Database;
using AutoServiss.Repositories.Klienti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoServiss.Models;
using System.Linq;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class KlientiController : Controller
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IKlientiRepository _repository;

        #endregion

        #region Constructor

        public KlientiController(
            ILogger<KlientiController> logger,
            IKlientiRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        #endregion

        #region Klienti

        [HttpGet]
        [Route("customers/natural")]
        public async Task<IActionResult> ListNatural()
        {
            try
            {
                var naturalPersons = await _repository.VisasFiziskasPersonasAsync();
                return Json(new { customers = naturalPersons });          
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
        [Route("customers/legal")]
        public async Task<IActionResult> ListLegal()
        {
            try
            {
                var legalPersons = await _repository.VisasJuridiskasPersonasAsync();
                return Json(new { customers = legalPersons });
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
        [Route("customers/search")]
        public async Task<IActionResult> SearchCustomer([FromBody]SearchTerm term)
        {
            try
            {
                var customers = await _repository.SearchKlients(term.Value);
                return Json(new { klienti = customers });
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
        [Route("customers/{id}/{data}")]
        public async Task<IActionResult> GetCustomer(int id, string data)
        {
            try
            {
                string[] details = null;
                if(data == "edit")
                {
                    details = new string[] { "Adreses", "Bankas" };
                }
                else if(data == "full")
                {
                    details = new string[] { "Adreses", "Bankas", "Transportlidzekli" };
                }
                var customer = await _repository.GetKlientsAsync(id, details);
                return Json(new { customer = customer });
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
        [Route("customers")]
        public async Task<IActionResult> InsertCustomer([FromBody]Klients klients)
        {
            try
            {
                if (klients == null)
                {
                    throw new CustomException("Objekts Klients ir null");
                }
                if (string.IsNullOrEmpty(klients.Nosaukums))
                {
                    throw new CustomException("Nav norādīts Nosaukums");
                }

                var result = await _repository.InsertKlientsAsync(klients);
                return Json(new { id = result.ToString(), message = "Izveidots jauns klients" });
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
        [Route("customers")]
        public async Task<IActionResult> UpdateCustomer([FromBody]Klients klients)
        {
            try
            {
                if (klients == null)
                {
                    throw new CustomException("Objekts Klients ir null");
                }
                if (string.IsNullOrEmpty(klients.Nosaukums))
                {
                    throw new CustomException("Nav norādīts Nosaukums");
                }

                var result = await _repository.UpdateKlientsAsync(klients);
                if(result > 0)
                {
                    return StatusCode(200, new { message = "Klienta dati atjaunināti" });
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
        [Route("customers/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _repository.DeleteKlientsAsync(id);
                if(result == 0)
                {
                    throw new CustomException("Klients netika izdzēsts");
                }
                return StatusCode(200, new { message = "Klients izdzēsts" });
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

        #region Markas un Modeļi

        [HttpGet]
        [Route("markas")]
        public async Task<IActionResult> GetMarkas()
        {
            try
            {
                var markas = await _repository.MarkasAsync();
                return Json(new { markas = markas });
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
        [Route("modeli/{id}")]
        public async Task<IActionResult> GetModeli(int id)
        {
            try
            {
                var modeli = await _repository.ModeliAsync(id);
                return Json(new { modeli = modeli });
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

        #region Transportlīdzekļi

        [HttpGet]
        [Route("customers/{customerId}/vehicles/{vehicleId}")]
        public async Task<IActionResult> GetVehicle(int customerId, int vehicleId)
        {
            try
            {
                var markas = await _repository.MarkasAsync();
                var klients = await _repository.GetKlientsAsync(customerId);
                if(vehicleId == 0)
                {
                    return Json(new { customer = klients, markas = markas });
                }
                else
                {                    
                    var tehnika = await _repository.GetTransportlidzeklisAsync(vehicleId);
                    var markasId = markas.Where(m => m.Nosaukums == tehnika.Marka).Select(m => m.Id).First();
                    var modeli = await _repository.ModeliAsync(markasId);
                    return Json(new { customer = klients, vehicle = tehnika, markas = markas, modeli = modeli });
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

        [HttpPost]
        [Route("vehicles/search")]
        public async Task<IActionResult> SearchVehicle([FromBody]SearchTerm term)
        {
            try
            {
                var vehicles = await _repository.SearchTransportlidzeklisAsync(term.Value);
                return Json(new { transportlidzekli = vehicles });
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
        [Route("vehicles")]
        public async Task<IActionResult> InsertVehicle([FromBody]Transportlidzeklis transportlidzeklis)
        {
            try
            {
                if (transportlidzeklis == null)
                {
                    throw new CustomException("Objekts Transportlidzeklis ir null");
                }
                if (string.IsNullOrEmpty(transportlidzeklis.Numurs))
                {
                    throw new CustomException("Nav norādīts Reģistrācijas numurs");
                }
                if (string.IsNullOrEmpty(transportlidzeklis.Marka))
                {
                    throw new CustomException("Nav norādīta Marka");
                }
                if (string.IsNullOrEmpty(transportlidzeklis.Modelis))
                {
                    throw new CustomException("Nav norādīta Modelis");
                }
                if (transportlidzeklis.KlientaId == 0)
                {
                    throw new CustomException("Nav norādīts KlientaId");
                }

                var result = await _repository.InsertTransportlidzeklisAsync(transportlidzeklis);
                return StatusCode(201, new { id = result, message = "Izveidots jauns transportlīdzeklis" });
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
        [Route("vehicles")]
        public async Task<IActionResult> UpdateVehicle([FromBody]Transportlidzeklis transportlidzeklis)
        {
            try
            {
                if (transportlidzeklis == null)
                {
                    throw new CustomException("Objekts Transportlidzeklis ir null");
                }
                if (string.IsNullOrEmpty(transportlidzeklis.Numurs))
                {
                    throw new CustomException("Nav norādīts Reģistrācijas numurs");
                }
                if (string.IsNullOrEmpty(transportlidzeklis.Marka))
                {
                    throw new CustomException("Nav norādīta Marka");
                }
                if (string.IsNullOrEmpty(transportlidzeklis.Modelis))
                {
                    throw new CustomException("Nav norādīta Modelis");
                }

                var result = await _repository.UpdateTransportlidzeklisAsync(transportlidzeklis);
                if (result > 0)
                {
                    return StatusCode(200, new { message = "Transportlīdzekļa dati atjaunināti" });
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
        [Route("vehicles/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var result = await _repository.DeleteTransportlidzeklisAsync(id);
                if (result == 0)
                {
                    throw new CustomException("Transportlīdzeklis netika izdzēsts");
                }

                return StatusCode(200, new { message = "Transportlīdzeklis izdzēsts" });
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