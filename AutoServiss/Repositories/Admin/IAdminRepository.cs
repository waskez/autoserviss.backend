using AutoServiss.Database;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Admin
{
    public interface IAdminRepository
    {
        #region Markas

        Task<int> InsertMarkaAsync(Marka marka);
        Task<int> UpdateMarkaAsync(Marka marka);
        Task<int> DeleteMarkaAsync(int id);

        #endregion

        #region Markas

        Task<int> InsertModelisAsync(Modelis modelis);
        Task<int> UpdateModelisAsync(Modelis modelis);
        Task<int> DeleteModelisAsync(int id);

        #endregion
    }
}
