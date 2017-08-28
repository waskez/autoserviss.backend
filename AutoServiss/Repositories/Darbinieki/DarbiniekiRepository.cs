using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using AutoServiss.Database;
using AutoServiss.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AutoServiss.Repositories.Darbinieki
{
    public class DarbiniekiRepository : IDarbiniekiRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _memoryCache;
        
        #endregion

        #region Constructor

        public DarbiniekiRepository(
            IOptions<AppSettings> settings,
            IMemoryCache memoryCache, 
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _context = context;
        }

        #endregion

        #region Public methods

        public async Task<List<Darbinieks>> AllDarbiniekiAsync()
        {
            var cacheKey = "EMPLOYEES-LIST";
            List<Darbinieks> employees;
            if (!_memoryCache.TryGetValue(cacheKey, out employees))
            {
                employees = new List<Darbinieks>();

                employees = await _context.Darbinieki.AsNoTracking()
                    .Where(d => d.Izdzests == false)
                    .ToListAsync();
                foreach(var emp in employees)
                {
                    emp.Parole = null; // paroles nesūtam
                }               

                _memoryCache.Set(cacheKey, employees, 
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }
            return employees;
        }

        public async Task<Darbinieks> GetDarbinieksAsync(int id)
        {
            return await _context.Darbinieki
                .Where(d => d.Izdzests == false && d.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Darbinieks> GetDarbinieksByUserNameAsync(string username)
        {
            return await _context.Darbinieki.AsNoTracking()
                .Where(d => d.Izdzests == false && d.Lietotajvards == username)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetPasswordByEmailAsync(string email)
        {
            var user = await _context.Darbinieki.AsNoTracking()
                .Where(d => d.Izdzests == false && d.Epasts == email)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                throw new CustomException("Darbinieks ar šādu e-pasta adresi neeksistē");
            }
            if (!user.Aktivs)
            {
                throw new CustomException("Jūsu konts ir bloķēts");
            }

            return PasswordValidator.Decrypt(user.Parole, _settings.EncryptionKey);
        }

        public async Task<int> InsertDarbinieksAsync(Darbinieks darbinieks)
        {
            if (!string.IsNullOrEmpty(darbinieks.Lietotajvards))
            {
                //pārbaudam vai lietotājvārds jau eksistē
                var exist = await GetDarbinieksByUserNameAsync(darbinieks.Lietotajvards);
                if (exist != null)
                {
                    throw new CustomException($"Lietotājvārds {darbinieks.Lietotajvards} jau eksistē");
                }

                //pārbaudam vai e-pasta adrese jau eksistē
                if(!string.IsNullOrEmpty(darbinieks.Epasts))
                {
                    var user = await _context.Darbinieki.AsNoTracking()
                        .Where(d => d.Epasts == darbinieks.Epasts)
                        .FirstOrDefaultAsync();
                    if (user != null)
                    {
                        throw new CustomException("Šāda e-pasta adrese jau eksistē");
                    }
                }

                var validateResult = PasswordValidator.Validate(darbinieks.Parole);
                if (validateResult != "OK")
                {
                    throw new CustomException(validateResult);
                }

                darbinieks.Parole = PasswordValidator.Encrypt(darbinieks.Parole, _settings.EncryptionKey);
            }
            else
            {
                darbinieks.Aktivs = false;
                darbinieks.Administrators = false;
            }

            _memoryCache.Remove("EMPLOYEES-LIST");

            await _context.Darbinieki.AddAsync(darbinieks);
            await _context.SaveChangesAsync();
            return darbinieks.Id;
        }

        public async Task<int> UpdateDarbinieksAsync(Darbinieks darbinieks)
        {
            var needClearPassword = false;

            if (!string.IsNullOrEmpty(darbinieks.Lietotajvards))
            {
                //pārbaudam vai lietotājvārds jau eksistē - gadījumā ja tiek izmainīts
                var exist = await GetDarbinieksByUserNameAsync(darbinieks.Lietotajvards);
                if (exist != null && exist.Id != darbinieks.Id)
                {
                    throw new CustomException($"Lietotājvārds {darbinieks.Lietotajvards} jau eksistē");
                }
            }
            else
            {
                needClearPassword = true;
            }

            //pārbaudam vai e-pasta adrese jau eksistē
            if (!string.IsNullOrEmpty(darbinieks.Epasts))
            {
                var user = await _context.Darbinieki.Where(d => d.Id != darbinieks.Id && d.Epasts == darbinieks.Epasts).FirstOrDefaultAsync();
                if (user != null)
                {
                    throw new CustomException("Šāda e-pasta adrese jau eksistē");
                }
            }            

            _memoryCache.Remove("EMPLOYEES-LIST");
            
            var oldDarbinieks = await GetDarbinieksAsync(darbinieks.Id);

            oldDarbinieks.PilnsVards = darbinieks.PilnsVards;
            oldDarbinieks.Epasts = darbinieks.Epasts;
            oldDarbinieks.Talrunis = darbinieks.Talrunis;
            oldDarbinieks.Aktivs = darbinieks.Aktivs;
            oldDarbinieks.Administrators = darbinieks.Administrators;

            // ja vecajos datos nav lietotājvārda, bet jaunajos ir - nepieciešams saglabāt arī paroli
            if (string.IsNullOrEmpty(oldDarbinieks.Lietotajvards) && !string.IsNullOrEmpty(darbinieks.Lietotajvards))
            {
                oldDarbinieks.Parole = PasswordValidator.Encrypt(darbinieks.Parole, _settings.EncryptionKey);               
            }
            else
            {
                // ja nav lietotājvārda - dzēšam arī paroli
                if (needClearPassword)
                {
                    oldDarbinieks.Parole = null;
                    oldDarbinieks.Aktivs = false;
                    oldDarbinieks.Administrators = false;
                }
            }

            oldDarbinieks.Lietotajvards = darbinieks.Lietotajvards;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteDarbinieksAsync(int id)
        {
            _memoryCache.Remove("EMPLOYEES-LIST");

            var darbinieks = await GetDarbinieksAsync(id);
            darbinieks.Izdzests = true;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> LockUnlockAsync(int id)
        {
            var employee = await GetDarbinieksAsync(id);
            employee.Aktivs = !employee.Aktivs;
            await _context.SaveChangesAsync();
            return employee.Aktivs;
        }

        public async Task ChangePasswordAsync(int id, string password)
        {
            var validateResult = PasswordValidator.Validate(password);
            if (validateResult != "OK")
            {
                throw new CustomException(validateResult);
            }

            var darbinieks = await GetDarbinieksAsync(id);
            if(!string.IsNullOrEmpty(darbinieks.Lietotajvards))
            {
                if(!string.IsNullOrEmpty(darbinieks.Parole))
                {
                    if (password == PasswordValidator.Decrypt(darbinieks.Parole, _settings.EncryptionKey))
                    {
                        throw new CustomException("Parole nedrīkst būt tāda pati kā iepriekšējā");
                    }
                }
            }
            else
            {
                throw new CustomException("Darbiniekam nav lietotājvārda");
            }

            darbinieks.Parole = password;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeAvatarAsync(int id, string avatarPath)
        {
            var employee = await GetDarbinieksAsync(id);
            employee.Avatar = avatarPath;
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}