using AutoServiss.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Firma
{
    public interface IFirmaRepository
    {
        Task<List<Klients>> VisasFirmasAsync();
        Task<Klients> GetFirmaAsync(int id);
        Task<int> InsertFirmaAsync(Klients firma);
        Task<int> UpdateFirmaAsync(Klients firma);
        Task<int> DeleteFirmaAsync(int id);
    }
}
