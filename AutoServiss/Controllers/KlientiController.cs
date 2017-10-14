using System.Threading.Tasks;
using AutoServiss.Database;
using AutoServiss.Repositories.Klienti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoServiss.Models;
using System.Linq;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class KlientiController : ControllerBase
    {
        #region Fields

        private readonly IKlientiRepository _repository;

        #endregion

        #region Constructor

        public KlientiController(IKlientiRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region Klienti

        [HttpGet]
        [Route("customers/natural")]
        public async Task<IActionResult> ListNatural()
        {
            var naturalPersons = await _repository.VisasFiziskasPersonasAsync();
            return Ok(new { customers = naturalPersons });          
        }

        [HttpGet]
        [Route("customers/legal")]
        public async Task<IActionResult> ListLegal()
        {
            var legalPersons = await _repository.VisasJuridiskasPersonasAsync();
            return Ok(new { customers = legalPersons });
        }

        [HttpPost]
        [Route("customers/search")]
        public async Task<IActionResult> SearchCustomer([FromBody]SearchTerm term)
        {
            var customers = await _repository.SearchKlients(term.Value);
            return Ok(new { klienti = customers });
        }

        [HttpGet]
        [Route("customers/{id}/{data}")]
        public async Task<IActionResult> GetCustomer(int id, string data)
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
            return Ok(new { customer = customer });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("customers")]
        public async Task<IActionResult> InsertCustomer([FromBody]Klients klients)
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
            return Ok(new { id = result.ToString(), message = "Izveidots jauns klients" });
        }

        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("customers")]
        public async Task<IActionResult> UpdateCustomer([FromBody]Klients klients)
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

        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [Route("customers/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var result = await _repository.DeleteKlientsAsync(id);
            if(result == 0)
            {
                throw new CustomException("Klients netika izdzēsts");
            }
            return StatusCode(200, new { message = "Klients izdzēsts" });
        }

        #endregion

        #region Markas un Modeļi

        [HttpGet]
        [Route("markas")]
        public async Task<IActionResult> GetMarkas()
        {
            var markas = await _repository.MarkasAsync();
            return Ok(new { markas = markas });
        }

        [HttpGet]
        [Route("modeli/{id}")]
        public async Task<IActionResult> GetModeli(int id)
        {
            var modeli = await _repository.ModeliAsync(id);
            return Ok(new { modeli = modeli });
        }

        #endregion

        #region Transportlīdzekļi

        [HttpGet]
        [Route("customers/{customerId}/vehicles/{vehicleId}")]
        public async Task<IActionResult> GetVehicle(int customerId, int vehicleId)
        {
            var markas = await _repository.MarkasAsync();
            var klients = await _repository.GetKlientsAsync(customerId);
            if(vehicleId == 0)
            {
                return Ok(new { customer = klients, markas = markas });
            }
            
            var tehnika = await _repository.GetTransportlidzeklisAsync(vehicleId);
            var markasId = markas.Where(m => m.Nosaukums == tehnika.Marka).Select(m => m.Id).First();
            var modeli = await _repository.ModeliAsync(markasId);
            return Ok(new { customer = klients, vehicle = tehnika, markas = markas, modeli = modeli });               
        }

        [HttpGet]
        [Route("vehicle/{id}/history")]
        public async Task<IActionResult> GetVehicleHistory(int id)
        {
            var vehicle = await _repository.GetTransportlidzeklisAsync(id, new string[] { "Klients" });
            var klients = vehicle.Klients;
            var vesture = await _repository.GetTransportlidzeklaVesture(id);
            return Ok(new { vehicle = vehicle, customer = klients, history = vesture });
        }

        [HttpPost]
        [Route("vehicles/search")]
        public async Task<IActionResult> SearchVehicle([FromBody]SearchTerm term)
        {
            var vehicles = await _repository.SearchTransportlidzeklisAsync(term.Value);
            return Ok(new { transportlidzekli = vehicles });
        }

        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("vehicles")]
        public async Task<IActionResult> InsertVehicle([FromBody]Transportlidzeklis transportlidzeklis)
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

        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("vehicles")]
        public async Task<IActionResult> UpdateVehicle([FromBody]Transportlidzeklis transportlidzeklis)
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

        [Authorize(Policy = "Admin")]
        [HttpDelete]
        [Route("vehicles/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var result = await _repository.DeleteTransportlidzeklisAsync(id);
            if (result == 0)
            {
                throw new CustomException("Transportlīdzeklis netika izdzēsts");
            }

            return StatusCode(200, new { message = "Transportlīdzeklis izdzēsts" });
        }

        #endregion
    }
}