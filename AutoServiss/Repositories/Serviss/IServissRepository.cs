using AutoServiss.Database;
using AutoServiss.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Serviss
{
    public interface IServissRepository
    {
        Task<List<ServisaLapasUznemums>> GetUznemumiArMehanikiem();
        Task<Transportlidzeklis> GetTransportlidzeklisArKlientuAsync(int id);
        Task<ServisaLapa> TransportlidzeklaServisaLapaAsync(int id);
        Task<int> InsertServisaLapaAsync(ServisaLapa sheet);
        Task<int> UpdateServisaLapaAsync(ServisaLapa sheet);
        Task<List<Transportlidzeklis>> PaslaikRemontaAsync();

        Task<byte[]> PrintServisaLapaAsync(int id);
    }
}