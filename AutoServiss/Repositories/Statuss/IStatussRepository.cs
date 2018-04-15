using AutoServiss.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Statuss
{
    public interface IStatussRepository
    {
        Task<SodienasStatuss> SodienasStatussAsync();
        Task<List<RemontuVesture>> RemontuVestureAsync(HistoryParameters parameters);
    }
}