using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using AutoServiss.Database;
using AutoServiss.Models;

namespace AutoServiss.Repositories.Statuss
{
    public class StatussRepository : IStatussRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructor

        public StatussRepository(AutoServissDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        #endregion

        public async Task<SodienasStatuss> SodienasStatussAsync()
        {
            var cacheKey = "TODAY-STATUS";
            if (!_memoryCache.TryGetValue(cacheKey, out SodienasStatuss statuss))
            {
                statuss = new SodienasStatuss
                {
                    JuridiskasPersonas = await _context.Klienti.AsNoTracking()
                        .Where(k => k.Veids == KlientaVeids.JuridiskaPersona)
                        .CountAsync(),
                    FiziskasPersonas = await _context.Klienti.AsNoTracking()
                        .Where(k => k.Veids == KlientaVeids.FiziskaPersona)
                        .CountAsync(),
                    Transportlidzekli = await _context.Transportlidzekli.AsNoTracking().CountAsync(),
                    Remonti = await _context.ServisaLapas.AsNoTracking()
                        .Where(r => r.Apmaksata != null)
                        .CountAsync()
                };

                _memoryCache.Set(cacheKey, statuss,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }              
            return statuss;
        }


        public async Task<List<RemontuVesture>> RemontuVestureAsync(HistoryParameters parameters)
        {
            var query = _context.ServisaLapas
                .Include(s => s.Klients)
                .Include(s => s.Transportlidzeklis)
                .Where(s=>s.Apmaksata != null)
                .AsQueryable();

            query = query.OrderByDescending(q => q.Datums); // jaunākās servisa lapas pirmās

            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                query = query.Where(l => l.Klients.Nosaukums.ToLower().Contains(parameters.Filter.ToLower()) ||
                    l.Transportlidzeklis.Numurs.Contains(parameters.Filter.ToUpper()));
            }

            var items = await query.Skip((parameters.Page) * parameters.PageSize).Take(parameters.PageSize).ToListAsync();

            return items.Select(i => new RemontuVesture
            {
                Datums = i.Datums,
                KlientaId = i.KlientaId,
                Klients = i.Klients.Nosaukums,
                TransportlidzeklaId = i.TransportlidzeklaId,
                Marka  = i.Transportlidzeklis.Marka + " " + i.Transportlidzeklis.Modelis,
                Numurs = i.Transportlidzeklis.Numurs,
                ServisaLapasId = i.Id
            }).ToList();
        }
    }
}