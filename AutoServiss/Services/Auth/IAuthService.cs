using AutoServiss.Database;
using System.Threading.Tasks;

namespace AutoServiss.Services.Auth
{
    public interface IAuthService
    {
        Task<Darbinieks> ValidateCredentialsAsync(string username, string password);
        Task<Darbinieks> ValidateRefreshTokenAsync(string token);
    }
}
