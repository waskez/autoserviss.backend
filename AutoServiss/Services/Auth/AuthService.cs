using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AutoServiss.Database;
using AutoServiss.Helpers;

namespace AutoServiss.Services.Auth
{
    public class AuthService : IAuthService
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;

        #endregion

        #region Constructor

        public AuthService(IOptions<AppSettings> settings, AutoServissDbContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        #endregion

        #region Public methods

        public async Task<Darbinieks> ValidateCredentialsAsync(string username, string password)
        {
            var pwd = PasswordValidator.Encrypt(password, _settings.EncryptionKey);

            var user = await _context.Darbinieki
                .Where(d => d.Lietotajvards == username && d.Parole == pwd)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                user.RefreshToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            return user;
        }

        public async Task<Darbinieks> ValidateRefreshTokenAsync(string token)
        {
            var user = await _context.Darbinieki
                .Where(d => d.Aktivs && d.RefreshToken == token)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                user.RefreshToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            return user;
        }

        #endregion
    }
}
