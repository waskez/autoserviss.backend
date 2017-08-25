using AutoServiss.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Klienti
{
    public interface IKlientiRepository
    {
        #region Klienti

        Task<List<Klients>> VisasFiziskasPersonasAsync();
        Task<List<Klients>> VisasJuridiskasPersonasAsync();
        Task<List<Klients>> SearchKlients(string term);
        Task<Klients> GetKlientsAsync(int id, string[] includes = null);
        Task<int> InsertKlientsAsync(Klients klients);
        Task<int> UpdateKlientsAsync(Klients klients);
        Task<int> DeleteKlientsAsync(int id);

        #endregion

        #region Markas un Modeļi

        Task<List<Marka>> MarkasAsync();
        Task<List<Modelis>> ModeliAsync(int markasId);

        #endregion

        #region Transportlīdzekļi

        Task<Transportlidzeklis> GetTransportlidzeklisAsync(int id);
        Task<List<Transportlidzeklis>> SearchTransportlidzeklisAsync(string term);
        Task<int> InsertTransportlidzeklisAsync(Transportlidzeklis transportlidzeklis);
        Task<int> UpdateTransportlidzeklisAsync(Transportlidzeklis transportlidzeklis);
        Task<int> DeleteTransportlidzeklisAsync(int id);

        #endregion
    }
}