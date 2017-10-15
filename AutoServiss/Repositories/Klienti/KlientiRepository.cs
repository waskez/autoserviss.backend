using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoServiss.Database;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using AutoServiss.Models;

namespace AutoServiss.Repositories.Klienti
{
    public class KlientiRepository : IKlientiRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructor

        public KlientiRepository(
            IOptions<AppSettings> settings,
            IMemoryCache memoryCache,
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _context = context;
        }

        #endregion

        #region Klienti
                
        public async Task<List<Klients>> VisasFiziskasPersonasAsync()
        {
            var cacheKey = "CUSTOMERS-NATURAL-LIST";
            List<Klients> list;
            if (!_memoryCache.TryGetValue(cacheKey, out list))
            {
                list = await _context.Klienti.AsNoTracking()
                    .Where(k => k.Veids == KlientaVeids.FiziskaPersona)
                    .Include(k => k.Adreses)
                    .ToListAsync();

                _memoryCache.Set(cacheKey, list,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }

            return list.OrderBy(p => p.Nosaukums).ToList();
        }
        
        public async Task<List<Klients>> VisasJuridiskasPersonasAsync()
        {
            var cacheKey = "CUSTOMERS-LEGAL-LIST";
            List<Klients> list;
            if (!_memoryCache.TryGetValue(cacheKey, out list))
            {
                list = await _context.Klienti.AsNoTracking()
                    .Where(k => k.Veids == KlientaVeids.JuridiskaPersona)
                    .Include(k => k.Adreses)
                    .ToListAsync();

                _memoryCache.Set(cacheKey, list,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }

            return list.OrderBy(p => p.Nosaukums).ToList();
        }

        public async Task<List<Klients>> SearchKlients(string term)
        {
            var customers = await _context.Klienti.AsNoTracking()
                .Where(c => c.Nosaukums.ToLower().Contains(term.ToLower()) || c.Talrunis.Contains(term))
                .ToListAsync();

            return customers.OrderBy(c => c.Nosaukums).ToList();
        }              

        public async Task<Klients> GetKlientsAsync(int id, string[] includes = null)
        {
            var query = _context.Klienti.Where(c => c.Id == id);
            if (includes != null)
            {
                for (var i = 0; i < includes.Length; i++)
                {
                    query = query.Include(includes[i]);
                }
            }
            return await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<int> InsertKlientsAsync(Klients klients)
        {
            if(klients.Veids == KlientaVeids.FiziskaPersona)
            {
                _memoryCache.Remove("CUSTOMERS-NATURAL-LIST");
            }
            else if(klients.Veids == KlientaVeids.JuridiskaPersona)
            {
                _memoryCache.Remove("CUSTOMERS-LEGAL-LIST");
            }
            else
            {
                throw new BadRequestException("Nezināms klienta veids");
            }

            await _context.Klienti.AddAsync(klients);
            await _context.SaveChangesAsync();
            return klients.Id;
        }

        public async Task<int> UpdateKlientsAsync(Klients klients)
        {            
            if (klients.Veids == KlientaVeids.FiziskaPersona)
            {
                _memoryCache.Remove("CUSTOMERS-NATURAL-LIST");
            }
            else if (klients.Veids == KlientaVeids.JuridiskaPersona)
            {
                _memoryCache.Remove("CUSTOMERS-LEGAL-LIST");
            }
            else
            {
                throw new BadRequestException("Nezināms klienta veids");
            }

            var customer = await _context.Klienti
                .Where(c => c.Id == klients.Id)
                .Include(c => c.Adreses)
                .Include(c => c.Bankas)
                .FirstOrDefaultAsync();
            if (customer == null)
            {
                throw new BadRequestException("Klients neeksistē");
            }

            var result = 0;

            #region Adreses

            var addrToBeRemoved = (from a in customer.Adreses where !klients.Adreses.Any(c=> c.Id > 0 && c.Id == a.Id) select a).ToList();
            var addrToBeAdded = (from a in klients.Adreses where !customer.Adreses.Any(e => a.Id == e.Id) select a).ToList();

            if (addrToBeRemoved.Count > 0) // izdzēšam noņemtās adreses
            {
                foreach (var a in addrToBeRemoved)
                {
                    customer.Adreses.Remove(a);
                }
            }

            if (addrToBeAdded.Count > 0) // pievienojam jaunās
            {
                foreach (var a in addrToBeAdded)
                {
                    customer.Adreses.Add(new KlientaAdrese
                    {
                        Veids = a.Veids,
                        Nosaukums = a.Nosaukums
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajās adresēs
            result = await _context.SaveChangesAsync();

            foreach(var addr in customer.Adreses)
            {
                var na = klients.Adreses.Where(a => a.Id == addr.Id).FirstOrDefault();
                if(na != null)
                {
                    addr.Veids = na.Veids;
                    addr.Nosaukums = na.Nosaukums;
                }               
            }

            #endregion

            #region Bankas

            if(klients.Veids == KlientaVeids.JuridiskaPersona)
            {
                var banksToBeRemoved = (from b in customer.Bankas where !klients.Bankas.Any(c => c.Id > 0 && c.Id == b.Id) select b).ToList();
                var banksToBeAdded = (from b in klients.Bankas where !customer.Bankas.Any(e => b.Id == e.Id) select b).ToList();

                if (banksToBeRemoved.Count > 0) // izdzēšam noņemtās bankas
                {
                    foreach (var b in banksToBeRemoved)
                    {
                        customer.Bankas.Remove(b);
                    }
                }

                if (banksToBeAdded.Count > 0) // pievienojam jaunās
                {
                    foreach (var b in banksToBeAdded)
                    {
                        customer.Bankas.Add(new KlientaBanka
                        {
                            Nosaukums = b.Nosaukums,
                            Kods = b.Kods,
                            Konts = b.Konts
                        });
                    }
                }

                // lai atjauninātu izmaiņas esošajās bankās
                result += await _context.SaveChangesAsync();

                foreach (var bank in customer.Bankas)
                {
                    var nb = klients.Bankas.Where(b => b.Id == bank.Id).FirstOrDefault();
                    if (nb != null)
                    {
                        bank.Nosaukums = nb.Nosaukums;
                        bank.Konts = nb.Konts;
                        bank.Kods = nb.Kods;
                    }
                }
            }            

            #endregion

            #region Dati

            customer.Nosaukums = klients.Nosaukums;
            customer.RegNumurs = klients.RegNumurs;
            customer.PvnNumurs = klients.PvnNumurs;
            customer.Epasts = klients.Epasts;
            customer.Talrunis = klients.Talrunis;
            customer.Piezimes = klients.Piezimes;

            #endregion

            result += await _context.SaveChangesAsync();
            return result;
        }

        public async Task<int> DeleteKlientsAsync(int id)
        {
            var klients = await _context.Klienti.Where(c => c.Id == id).FirstOrDefaultAsync();
            if(klients == null)
            {
                throw new BadRequestException("Klients neeksistē");
            }

            if (klients.Veids == KlientaVeids.FiziskaPersona)
            {
                _memoryCache.Remove("CUSTOMERS-NATURAL-LIST");
            }
            else if (klients.Veids == KlientaVeids.JuridiskaPersona)
            {
                _memoryCache.Remove("CUSTOMERS-LEGAL-LIST");
            }
            else
            {
                throw new BadRequestException("Nezināms klienta veids");
            }

            klients.IsDeleted = true;
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Markas un Modeļi

        public async Task<List<Marka>> MarkasAsync()
        {
            var cacheKey = "VEHICLE-BRANDS";
            List<Marka> list;
            if (!_memoryCache.TryGetValue(cacheKey, out list))
            {
                list = await _context.Markas.AsNoTracking().ToListAsync();

                _memoryCache.Set(cacheKey, list,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }

            return list.OrderBy(p => p.Nosaukums).ToList();
        }

        public async Task<List<Modelis>> ModeliAsync(int markasId)
        {
            var cacheKey = $"VEHICLE-MODELS-{markasId}";
            List<Modelis> list;
            if (!_memoryCache.TryGetValue(cacheKey, out list))
            {
                list = await _context.Modeli.AsNoTracking()
                    .Where(m=>m.MarkasId == markasId)
                    .ToListAsync();

                _memoryCache.Set(cacheKey, list,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30)));
            }

            return list.OrderBy(p => p.Nosaukums).ToList();
        }

        #endregion

        #region Transportlīdzekļi

        public async Task<Transportlidzeklis> GetTransportlidzeklisAsync(int id, string[] includes = null)
        {
            var query = _context.Transportlidzekli.Where(c => c.Id == id);
            if (includes != null)
            {
                for (var i = 0; i < includes.Length; i++)
                {
                    query = query.Include(includes[i]);
                }
            }
            return await query.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<List<TransportlidzeklaVesture>> GetTransportlidzeklaVesture(int id)
        {
            var list = new List<TransportlidzeklaVesture>();

            var servisaLapas = await _context.ServisaLapas.AsNoTracking()
                .Where(s => s.TransportlidzeklaId == id)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.RezervesDalas)
                .OrderByDescending(s => s.Datums)
                .ToListAsync();

            foreach(var sl in servisaLapas)
            {
                list.Add(new TransportlidzeklaVesture
                {
                    Id = sl.Id,
                    Datums = sl.Datums,
                    PaveiktieDarbi = sl.PaveiktieDarbi,
                    RezervesDalas = sl.RezervesDalas
                });
            }

            return list;
        }

        public async Task<List<Transportlidzeklis>> SearchTransportlidzeklisAsync(string term)
        {
            return await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Numurs.ToUpper().Contains(term.ToUpper()))
                .ToListAsync();
        }

        public async Task<int> InsertTransportlidzeklisAsync(Transportlidzeklis transportlidzeklis)
        {
            // pārbaudam vai šāds numurs jau eksistē
            var exist = await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Numurs == transportlidzeklis.Numurs)
                .CountAsync();
            if (exist > 0)
            {
                throw new BadRequestException("Transportlīdzeklis ar šādu reģistrācijas numuru jau eksistē");
            }

            await _context.Transportlidzekli.AddAsync(transportlidzeklis);
            await _context.SaveChangesAsync();
            return transportlidzeklis.Id;
        }

        public async Task<int> UpdateTransportlidzeklisAsync(Transportlidzeklis transportlidzeklis)
        {
            // numura maiņas gadījumā - pārbaudam vai šāds numurs jau eksistē
            var exist = await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Id != transportlidzeklis.Id && t.Numurs == transportlidzeklis.Numurs)
                .CountAsync();
            if(exist > 0)
            {
                throw new BadRequestException("Transportlīdzeklis ar šādu reģistrācijas numuru jau eksistē");
            }

            var vehicle = await _context.Transportlidzekli.Where(t => t.Id == transportlidzeklis.Id).FirstAsync();

            vehicle.Numurs = transportlidzeklis.Numurs;
            vehicle.Marka = transportlidzeklis.Marka;
            vehicle.Modelis = transportlidzeklis.Modelis;
            vehicle.Krasa = transportlidzeklis.Krasa;
            vehicle.Gads = transportlidzeklis.Gads;
            vehicle.Vin = transportlidzeklis.Vin;
            vehicle.Tips = transportlidzeklis.Tips;
            vehicle.Tilpums = transportlidzeklis.Tilpums;
            vehicle.Variants = transportlidzeklis.Variants;
            vehicle.Versija = transportlidzeklis.Versija;
            vehicle.Degviela = transportlidzeklis.Degviela;
            vehicle.Jauda = transportlidzeklis.Jauda;
            vehicle.PilnaMasa = transportlidzeklis.PilnaMasa;
            vehicle.Pasmasa = transportlidzeklis.Pasmasa;
            vehicle.Piezimes = transportlidzeklis.Piezimes;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteTransportlidzeklisAsync(int id)
        {
            var vehicle = await _context.Transportlidzekli.Where(t => t.Id == id).FirstOrDefaultAsync();
            if (vehicle == null)
            {
                throw new BadRequestException($"Transportlīdzeklis ar Id={id} netika atrasts");
            }
            vehicle.IsDeleted = true;
            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}