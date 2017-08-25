using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoServiss.Database;

namespace AutoServiss.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;

        #endregion

        #region Constructor

        public AdminRepository(
            IOptions<AppSettings> settings,
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        #endregion
    }
}