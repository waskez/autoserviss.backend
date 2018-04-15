using System.Threading.Tasks;
using AutoServiss.Models;
using AutoServiss.Repositories.Statuss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiss.Controllers
{
    [Authorize]
    public class StatussController : ControllerBase
    {
        #region Fields

        private readonly IStatussRepository _repository;

        #endregion

        #region Constructor

        public StatussController(IStatussRepository repository)
        {
            _repository = repository;
        }

        #endregion

        [HttpGet]
        [Route("status/count")]
        public async Task<IActionResult> Statuss()
        {
            var result = await _repository.SodienasStatussAsync();
            return StatusCode(200, new { status = result });
        }

        [HttpPost]
        [Route("status/repair/history")]
        public async Task<IActionResult> History([FromBody]HistoryParameters parameters)
        {
            var list = await _repository.RemontuVestureAsync(parameters);
            return StatusCode(200, new { history = list });
        }
    }
}
