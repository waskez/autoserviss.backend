using AutoServiss.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Firma
{
    public class FirmaRepository : IFirmaRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructor

        public FirmaRepository(
            IOptions<AppSettings> settings,
            IMemoryCache memoryCache,
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _context = context;
        }

        #endregion

        public async Task<List<Klients>> VisasFirmasAsync()
        {
            var cacheKey = "COMPANIES-LIST";
            List<Klients> list;
            if (!_memoryCache.TryGetValue(cacheKey, out list))
            {
                list = await _context.Klienti.AsNoTracking()
                    .Where(k => k.Veids == KlientaVeids.ManaFirma)
                    .Include(k => k.Adreses)
                    .Include(k => k.Bankas)
                    .ToListAsync();

                _memoryCache.Set(cacheKey, list,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }

            return list.OrderBy(p => p.Nosaukums).ToList();
        }

        public async Task<Klients> GetFirmaAsync(int id)
        {
            return await _context.Klienti.AsNoTracking()
                .Where(f => f.Id == id && f.Veids == KlientaVeids.ManaFirma)
                .Include(f => f.Adreses)
                .Include(f => f.Bankas)
                .FirstOrDefaultAsync();
        }

        public async Task<int> InsertFirmaAsync(Klients firma)
        {
            _memoryCache.Remove("COMPANIES-LIST");

            await _context.Klienti.AddAsync(firma);
            await _context.SaveChangesAsync();
            return firma.Id;
        }

        public async Task<int> UpdateFirmaAsync(Klients firma)
        {
            _memoryCache.Remove("COMPANIES-LIST");

            var company = await _context.Klienti
                .Where(c => c.Id == firma.Id && firma.Veids == KlientaVeids.ManaFirma)
                .Include(c => c.Adreses)
                .Include(c => c.Bankas)
                .FirstOrDefaultAsync();
            if (company == null)
            {
                throw new CustomException("Firma neeksistē");
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
                    company.Adreses.Add(new Adrese
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
                    company.Bankas.Add(new Banka
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

        public async Task<int> DeleteFirmaAsync(int id)
        {
            var firma = await _context.Klienti
                .Where(c => c.Id == id && c.Veids == KlientaVeids.ManaFirma)
                .FirstOrDefaultAsync();
            if (firma == null)
            {
                throw new CustomException("Firma neeksistē");
            }

            _memoryCache.Remove("COMPANIES-LIST");

            _context.Klienti.Remove(firma);
            return await _context.SaveChangesAsync();
        }
    }
}
