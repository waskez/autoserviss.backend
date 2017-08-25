using AutoServiss.Database;
using AutoServiss.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Serviss
{
    public interface IServissRepository
    {
        Task<Transportlidzeklis> GetTransportlidzeklisAsync(int id);
        Task<ServisaLapa> TransportlidzeklaServisaLapaAsync(int id);
        Task<List<Mehanikis>> GetMehanikiAsync();
        Task<List<Mehanikis>> GetMehanikiAsync(List<int> mehanikuId);
        Task<int> InsertServisaLapaAsync(ServisaLapa sheet);
        Task<int> UpdateServisaLapaAsync(ServisaLapa sheet);
        Task<List<Transportlidzeklis>> PaslaikRemontaAsync();
    }
}