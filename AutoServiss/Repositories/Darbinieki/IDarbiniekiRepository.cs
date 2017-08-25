using System.Collections.Generic;
using System.Threading.Tasks;
using AutoServiss.Database;

namespace AutoServiss.Repositories.Darbinieki
{
    public interface IDarbiniekiRepository
    {
        Task<List<Darbinieks>> AllDarbiniekiAsync();
        Task<Darbinieks> GetDarbinieksAsync(int id);
        Task<Darbinieks> GetDarbinieksByUserNameAsync(string username);
        Task<string> GetPasswordByEmailAsync(string email);
        Task<int> InsertDarbinieksAsync(Darbinieks darbinieks);
        Task<int> UpdateDarbinieksAsync(Darbinieks darbinieks);
        Task<int> DeleteDarbinieksAsync(int id);
        Task<bool> LockUnlockAsync(int id);
        Task ChangePasswordAsync(int id, string password);
        Task ChangeAvatarAsync(int id, string avatarPath);
    }
}
