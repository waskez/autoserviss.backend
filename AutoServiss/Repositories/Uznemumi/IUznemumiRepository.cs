using AutoServiss.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Uznemumi
{
    public interface IUznemumiRepository
    {
        #region Uzņēmumi

        Task<List<Uznemums>> AllUznemumiAsync();
        Task<Uznemums> GetUznemumsAsync(int id);
        Task<int> InsertUznemumsAsync(Uznemums firma);
        Task<int> UpdateUznemumsAsync(Uznemums firma);
        Task<int> DeleteDarbiniekiBezUznemumaAsync(int id);
        Task<int> DeleteUznemumsAsync(int id);

        #endregion

        #region Darbinieki

        Task<List<Darbinieks>> SearchDarbinieksAsync(string term, int id);
        Task<Uznemums> GetUznemumsForDarbinieksAsync(int id);
        Task<Darbinieks> GetDarbinieksForEditAsync(int companyId, int employeeId);
        Task<Darbinieks> GetDarbinieksByUserNameAsync(string username);
        Task<string> GetPasswordByEmailAsync(string email);
        Task<int> AppendDarbinieksAsync(int companyId, int employeeId);
        Task<int> InsertDarbinieksAsync(Darbinieks darbinieks);
        Task<int> UpdateDarbinieksAsync(Darbinieks darbinieks);
        Task<int> DeleteDarbinieksAsync(int companyId, int employeeId);
        Task<bool> LockUnlockAsync(int id);
        Task ChangePasswordAsync(int id, string password);
        Task ChangeAvatarAsync(int id, string avatarPath);

        #endregion
    }
}
