using AutoServiss.Database;
using AutoServiss.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Uznemumi
{
    public class UznemumiRepository : IUznemumiRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructor

        public UznemumiRepository(
            IOptions<AppSettings> settings,
            IMemoryCache memoryCache,
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _context = context;
        }

        #endregion

        #region Uzņēmumi

        public async Task<List<Uznemums>> AllUznemumiAsync()
        {
            var cacheKey = "COMPANIES-LIST";
            if (!_memoryCache.TryGetValue(cacheKey, out List<Uznemums> list))
            {
                list = await _context.Uznemumi.AsNoTracking()
                    .Include(k => k.Adreses)
                    //.Include(k => k.Bankas)
                    .ToListAsync();

                _memoryCache.Set(cacheKey, list,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }

            return list.OrderBy(p => p.Nosaukums).ToList();
        }

        public async Task<Uznemums> GetUznemumsAsync(int id)
        {
            var firma = await _context.Uznemumi.AsNoTracking()
                 .Where(f => f.Id == id)
                 .Include(f => f.Adreses)
                 .Include(f => f.Bankas)
                 .FirstOrDefaultAsync();
            if (firma == null)
            {
                throw new BadRequestException($"Uzņēmums ar Id={id} netika atrasts");
            }

            firma.Darbinieki = await _context.UznemumaDarbinieki.AsNoTracking()
                .Where(d => d.UznemumaId == id)
                .Select(d => d.Darbinieks)
                .OrderBy(d => d.PilnsVards)
                .ToListAsync();
                
            return firma;
        }

        public async Task<int> InsertUznemumsAsync(Uznemums firma)
        {
            _memoryCache.Remove("COMPANIES-LIST");

            await _context.Uznemumi.AddAsync(firma);
            await _context.SaveChangesAsync();
            return firma.Id;
        }

        public async Task<int> UpdateUznemumsAsync(Uznemums firma)
        {
            _memoryCache.Remove("COMPANIES-LIST");

            var company = await _context.Uznemumi
                .Where(c => c.Id == firma.Id)
                .Include(c => c.Adreses)
                .Include(c => c.Bankas)
                .FirstOrDefaultAsync();
            if (company == null)
            {
                throw new BadRequestException($"Uzņēmums ar Id={firma.Id} netika atrasts");
            }

            var result = 0;

            #region Adreses

            var addrToBeRemoved = (from a in company.Adreses where !firma.Adreses.Any(c => c.Id > 0 && c.Id == a.Id) select a).ToList();
            var addrToBeAdded = (from a in firma.Adreses where !company.Adreses.Any(e => a.Id == e.Id) select a).ToList();

            if (addrToBeRemoved.Count > 0) // izdzēšam noņemtās adreses
            {
                foreach (var a in addrToBeRemoved)
                {
                    company.Adreses.Remove(a);
                }
            }

            if (addrToBeAdded.Count > 0) // pievienojam jaunās
            {
                foreach (var a in addrToBeAdded)
                {
                    company.Adreses.Add(new UznemumaAdrese
                    {
                        Veids = a.Veids,
                        Nosaukums = a.Nosaukums
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajās adresēs
            result = await _context.SaveChangesAsync();

            foreach (var addr in company.Adreses)
            {
                var na = firma.Adreses.Where(a => a.Id == addr.Id).FirstOrDefault();
                if (na != null)
                {
                    addr.Veids = na.Veids;
                    addr.Nosaukums = na.Nosaukums;
                }
            }

            #endregion

            #region Bankas

            var banksToBeRemoved = (from b in company.Bankas where !firma.Bankas.Any(c => c.Id > 0 && c.Id == b.Id) select b).ToList();
            var banksToBeAdded = (from b in firma.Bankas where !company.Bankas.Any(e => b.Id == e.Id) select b).ToList();

            if (banksToBeRemoved.Count > 0) // izdzēšam noņemtās bankas
            {
                foreach (var b in banksToBeRemoved)
                {
                    company.Bankas.Remove(b);
                }
            }

            if (banksToBeAdded.Count > 0) // pievienojam jaunās
            {
                foreach (var b in banksToBeAdded)
                {
                    company.Bankas.Add(new UznemumaBanka
                    {
                        Nosaukums = b.Nosaukums,
                        Kods = b.Kods,
                        Konts = b.Konts
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajās bankās
            result += await _context.SaveChangesAsync();

            foreach (var bank in company.Bankas)
            {
                var nb = firma.Bankas.Where(b => b.Id == bank.Id).FirstOrDefault();
                if (nb != null)
                {
                    bank.Nosaukums = nb.Nosaukums;
                    bank.Konts = nb.Konts;
                    bank.Kods = nb.Kods;
                }
            }

            #endregion

            #region Dati

            company.Nosaukums = firma.Nosaukums;
            company.RegNumurs = firma.RegNumurs;
            company.PvnNumurs = firma.PvnNumurs;
            company.Epasts = firma.Epasts;
            company.Talrunis = firma.Talrunis;
            company.Piezimes = firma.Piezimes;

            #endregion

            result += await _context.SaveChangesAsync();
            return result;
        }

        public async Task<int> DeleteUznemumsAsync(int id)
        {
            var firma = await _context.Uznemumi
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if(firma == null)
            {
                throw new BadRequestException($"Uzņēmums ar Id={id} netika atrasts");
            }

            _memoryCache.Remove("COMPANIES-LIST");

            firma.IsDeleted = true;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteDarbiniekiBezUznemumaAsync(int id)
        {
            var withoutCompany = await _context.Darbinieki
                .Include(d => d.Uznemumi)
                .Where(d => d.Uznemumi.Count == 0)
                .ToListAsync();
            _context.Darbinieki.RemoveRange(withoutCompany);
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Darbinieki

        public async Task<List<Darbinieks>> SearchDarbinieksAsync(string term, int id)
        {
            var employees = await _context.UznemumaDarbinieki.AsNoTracking()
                .Where(c => c.UznemumaId != id && c.Darbinieks.PilnsVards.ToLower().Contains(term.ToLower()))
                .Select(c => c.Darbinieks)
                .ToListAsync();

            return employees.OrderBy(c => c.PilnsVards).ToList();
        }

        public async Task<Uznemums> GetUznemumsForDarbinieksAsync(int id)
        {
            return await _context.Uznemumi.AsNoTracking()
                .Where(u => u.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Darbinieks> GetDarbinieksForEditAsync(int companyId, int employeeId)
        {
            var employee = await _context.Darbinieki. AsNoTracking()
                .Where(d => d.Id == employeeId)
                .FirstOrDefaultAsync();
            if (employee == null)
            {
                throw new BadRequestException($"Darbinieks ar Id={employeeId} netika atrasts");
            }

            employee.Uznemums = await GetUznemumsForDarbinieksAsync(companyId);
            if (employee.Uznemums == null)
            {
                throw new BadRequestException($"Darbinieka uzņēmums ar Id={companyId} netika atrasts");
            }

            return employee;
        }

        public async Task<Darbinieks> GetDarbinieksByUserNameAsync(string username)
        {
            return await _context.Darbinieki.AsNoTracking()
                .Where(d => d.Lietotajvards == username)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GetPasswordByEmailAsync(string email)
        {
            var user = await _context.Darbinieki.AsNoTracking()
                .Where(d => d.Epasts == email)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                throw new BadRequestException("Darbinieks ar šādu e-pasta adresi neeksistē");
            }
            if (!user.Aktivs)
            {
                throw new BadRequestException("Jūsu konts ir bloķēts");
            }

            return PasswordValidator.Decrypt(user.Parole, _settings.EncryptionKey);
        }

        public async Task<int> AppendDarbinieksAsync(int companyId, int employeeId)
        {
            // vispirms pārbaudam vai darbinieks jau ir piesaistīts
            var exist = await _context.UznemumaDarbinieki
                .Where(d => d.UznemumaId == companyId && d.DarbiniekaId == employeeId)
                .CountAsync();
            if(exist > 0)
            {
                throw new BadRequestException("Šads darbinieks jau ir uzņēmumā");
            }
            var uznemumaDarbinieks = new UznemumaDarbinieks { UznemumaId = companyId, DarbiniekaId = employeeId };
            await _context.UznemumaDarbinieki.AddAsync(uznemumaDarbinieks);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> InsertDarbinieksAsync(Darbinieks darbinieks)
        {
            if (!string.IsNullOrEmpty(darbinieks.Lietotajvards))
            {
                //pārbaudam vai lietotājvārds jau eksistē
                var exist = await GetDarbinieksByUserNameAsync(darbinieks.Lietotajvards);
                if (exist != null)
                {
                    throw new BadRequestException($"Lietotājvārds {darbinieks.Lietotajvards} jau eksistē");
                }

                //pārbaudam vai e-pasta adrese jau eksistē
                if (!string.IsNullOrEmpty(darbinieks.Epasts))
                {
                    var user = await _context.Darbinieki.AsNoTracking()
                        .Where(d => d.Epasts == darbinieks.Epasts)
                        .FirstOrDefaultAsync();
                    if (user != null)
                    {
                        throw new BadRequestException("Šāda e-pasta adrese jau eksistē");
                    }
                }

                var validateResult = PasswordValidator.Validate(darbinieks.Parole);
                if (validateResult != "OK")
                {
                    throw new BadRequestException(validateResult);
                }

                darbinieks.Parole = PasswordValidator.Encrypt(darbinieks.Parole, _settings.EncryptionKey);
            }
            else
            {
                darbinieks.Aktivs = false;
                darbinieks.Administrators = false;
            }

            _memoryCache.Remove("EMPLOYEES-LIST");

            darbinieks.Uznemumi = new List<UznemumaDarbinieks> { new UznemumaDarbinieks { UznemumaId = darbinieks.Uznemums.Id } };

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
                    throw new BadRequestException($"Lietotājvārds {darbinieks.Lietotajvards} jau eksistē");
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
                    throw new BadRequestException("Šāda e-pasta adrese jau eksistē");
                }
            }

            var oldDarbinieks = await _context.Darbinieki
                .Where(d => d.Id == darbinieks.Id)
                .FirstOrDefaultAsync();
            if(oldDarbinieks == null)
            {
                throw new BadRequestException($"Darbinieks ar Id={darbinieks.Id} netika atrasts");
            }

            oldDarbinieks.PilnsVards = darbinieks.PilnsVards;
            oldDarbinieks.Epasts = darbinieks.Epasts;
            oldDarbinieks.Talrunis = darbinieks.Talrunis;
            oldDarbinieks.Aktivs = darbinieks.Aktivs;
            oldDarbinieks.Administrators = darbinieks.Administrators;
            oldDarbinieks.Mehanikis = darbinieks.Mehanikis;

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

        public async Task<int> DeleteDarbinieksAsync(int companyId, int employeeId)
        {
            var darbinieks = await _context.Darbinieki
                .Where(d => d.Id == employeeId)
                .Include(d => d.Uznemumi)
                .FirstOrDefaultAsync();
            if(darbinieks == null)
            {
                throw new BadRequestException($"Darbinieks ar Id={employeeId} netika atrasts");
            }

            var employeeCompany = darbinieks.Uznemumi.Single(u => u.UznemumaId == companyId);
            _context.UznemumaDarbinieki.Remove(employeeCompany);
            _context.Darbinieki.Remove(darbinieks);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> LockUnlockAsync(int id)
        {
            var employee = await _context.Darbinieki
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();
            if (employee == null)
            {
                throw new BadRequestException($"Darbinieks ar Id={id} netika atrasts");
            }
            employee.Aktivs = !employee.Aktivs;
            await _context.SaveChangesAsync();
            return employee.Aktivs;
        }

        public async Task ChangePasswordAsync(int id, string password)
        {
            var validateResult = PasswordValidator.Validate(password);
            if (validateResult != "OK")
            {
                throw new BadRequestException(validateResult);
            }

            var darbinieks = await _context.Darbinieki
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();
            if (darbinieks == null)
            {
                throw new BadRequestException($"Darbinieks ar Id={id} netika atrasts");
            }
            if (!string.IsNullOrEmpty(darbinieks.Lietotajvards))
            {
                if (!string.IsNullOrEmpty(darbinieks.Parole))
                {
                    if (password == PasswordValidator.Decrypt(darbinieks.Parole, _settings.EncryptionKey))
                    {
                        throw new BadRequestException("Parole nedrīkst būt tāda pati kā iepriekšējā");
                    }
                }
            }
            else
            {
                throw new BadRequestException("Darbiniekam nav lietotājvārda");
            }

            darbinieks.Parole = PasswordValidator.Encrypt(password, _settings.EncryptionKey);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeAvatarAsync(int id, string avatarPath)
        {
            var employee = await _context.Darbinieki
                .Where(d => d.Id == id)
                .FirstOrDefaultAsync();
            if (employee == null)
            {
                throw new BadRequestException($"Darbinieks ar Id={id} netika atrasts");
            }
            employee.Avatar = avatarPath;
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}