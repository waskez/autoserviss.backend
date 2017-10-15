using AutoServiss.Database;
using AutoServiss.Helpers;
using AutoServiss.Repositories.Serviss;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiss.Controllers
{
    public class ServissController : ControllerBase
    {
        #region Fields

        private readonly IServissRepository _repository;

        #endregion

        #region Constructor

        public ServissController(IServissRepository repository)
        {
            _repository = repository;
        }

        #endregion

        [HttpGet]
        [Route("service/vehicle/{id}/sheet")]
        public async Task<IActionResult> VehicleSheet(int id)
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
                // gadījumā ja kāds no esošajiem servisa lapas mehāniķiem (darbiniekiem) ir izdzēsts
                companyMechanics = sheetCompany.Mehaniki.Union(sheet.Mehaniki, new MehanikiComparer()).ToList();
            }
            return StatusCode(200, new { sheet = sheet, companies = companiesWithMechanics, mechanics = companyMechanics });              
        }

        [ModelStateValidationFilter]
        [HttpPost]
        [Route("service/sheet")]
        public async Task<IActionResult> InsertSheet([FromBody]ServisaLapa sheet)
        {
            if (sheet.Mehaniki.Count == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nav norādīti Mehāniķi" } });
            }                

            var result = await _repository.InsertServisaLapaAsync(sheet);
            return StatusCode(200, new { id = result.ToString(), message = "Izveidota jauna servisa lapa" });
        }

        [ModelStateValidationFilter]
        [HttpPut]
        [Route("service/sheet")]
        public async Task<IActionResult> UpdateSheet([FromBody]ServisaLapa sheet)
        {
            if (sheet.Id == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nepareizs servisa lapas identifikators" } });
            }
            if (sheet.Mehaniki.Count == 0)
            {
                return StatusCode(400, new { messages = new List<string> { "Nav norādīti Mehāniķi" } });
            }

            var result = await _repository.UpdateServisaLapaAsync(sheet);
            if (result > 0)
            {
                return StatusCode(200, new { message = "Servisa lapas dati atjaunināti" });
            }
            return StatusCode(200, new { message = "Nav izmaiņu ko saglabāt" });
        }

        [HttpGet]
        [Route("service/repair")]
        public async Task<IActionResult> UnderRepair()
        {
            return StatusCode(200, new { vehicles = await _repository.PaslaikRemontaAsync() });
        }

        [HttpGet]
        [Route("service/sheet/{id}/print")]
        public async Task<IActionResult> PrintSheet(int id)
        {
            var pdf = await _repository.PrintServisaLapaAsync(id);
            var output = new MemoryStream();
            output.Write(pdf, 0, pdf.Length);
            output.Position = 0;

            return new FileStreamResult(output, "application/pdf");
        }
    }
}