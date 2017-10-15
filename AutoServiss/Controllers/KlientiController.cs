using System.Threading.Tasks;
using AutoServiss.Database;
using AutoServiss.Repositories.Klienti;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoServiss.Models;
using System.Linq;
using System.Collections.Generic;

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
            return StatusCode(200, new { customers = await _repository.VisasFiziskasPersonasAsync() });          
        }

        [HttpGet]
        [Route("customers/legal")]
        public async Task<IActionResult> ListLegal()
        {
            return StatusCode(200, new { customers = await _repository.VisasJuridiskasPersonasAsync() });
        }

        [ModelStateValidationFilter]
        [HttpPost]
        [Route("customers/search")]
        public async Task<IActionResult> SearchCustomer([FromBody]SearchTerm term)
        {
            return StatusCode(200, new { klienti = await _repository.SearchKlients(term.Value) });
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
            return StatusCode(200, new { customer = await _repository.GetKlientsAsync(id, details) });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("customers")]
        public async Task<IActionResult> InsertCustomer([FromBody]Klients klients)
        {
            var result = await _repository.InsertKlientsAsync(klients);
            return StatusCode(200, new { id = result.ToString(), message = "Izveidots jauns klients" });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("customers")]
        public async Task<IActionResult> UpdateCustomer([FromBody]Klients klients)
        {
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
            if(result > 0)
            {
                return StatusCode(200, new { message = "Klients izdzēsts" });
            }
            return StatusCode(400, new { messages = new List<string> { "Klients netika izdzēsts" } });            
        }

        #endregion

        #region Markas un Modeļi

        [HttpGet]
        [Route("markas")]
        public async Task<IActionResult> GetMarkas()
        {
            return StatusCode(200, new { markas = await _repository.MarkasAsync() });
        }

        [HttpGet]
        [Route("modeli/{id}")]
        public async Task<IActionResult> GetModeli(int id)
        {
            return StatusCode(200, new { modeli = await _repository.ModeliAsync(id) });
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
                return StatusCode(200, new { customer = klients, markas = markas });
            }
            
            var tehnika = await _repository.GetTransportlidzeklisAsync(vehicleId);
            var markasId = markas.Where(m => m.Nosaukums == tehnika.Marka).Select(m => m.Id).First();
            var modeli = await _repository.ModeliAsync(markasId);
            return StatusCode(200, new { customer = klients, vehicle = tehnika, markas = markas, modeli = modeli });               
        }

        [HttpGet]
        [Route("vehicle/{id}/history")]
        public async Task<IActionResult> GetVehicleHistory(int id)
        {
            var vehicle = await _repository.GetTransportlidzeklisAsync(id, new string[] { "Klients" });
            var klients = vehicle.Klients;
            var vesture = await _repository.GetTransportlidzeklaVesture(id);
            return StatusCode(200, new { vehicle = vehicle, customer = klients, history = vesture });
        }

        [ModelStateValidationFilter]
        [HttpPost]
        [Route("vehicles/search")]
        public async Task<IActionResult> SearchVehicle([FromBody]SearchTerm term)
        {
            var vehicles = await _repository.SearchTransportlidzeklisAsync(term.Value);
            return StatusCode(200, new { transportlidzekli = vehicles });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [Route("vehicles")]
        public async Task<IActionResult> InsertVehicle([FromBody]Transportlidzeklis transportlidzeklis)
        {
            var result = await _repository.InsertTransportlidzeklisAsync(transportlidzeklis);
            return StatusCode(201, new { id = result, message = "Izveidots jauns transportlīdzeklis" });
        }

        [ModelStateValidationFilter]
        [Authorize(Policy = "Admin")]
        [HttpPut]
        [Route("vehicles")]
        public async Task<IActionResult> UpdateVehicle([FromBody]Transportlidzeklis transportlidzeklis)
        {
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
            if (result > 0)
            {
                return StatusCode(200, new { message = "Transportlīdzeklis izdzēsts" });
            }
            return StatusCode(400, new { messages = new List<string> { "Transportlīdzeklis netika izdzēsts" } });
        }

        #endregion
    }
}