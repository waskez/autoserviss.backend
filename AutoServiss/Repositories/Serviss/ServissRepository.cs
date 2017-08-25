using AutoServiss.Database;
using AutoServiss.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoServiss.Repositories.Serviss
{
    public class ServissRepository : IServissRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;

        #endregion

        #region Constructor

        public ServissRepository(
            IOptions<AppSettings> settings,
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        #endregion

        public async Task<Transportlidzeklis> GetTransportlidzeklisAsync(int id)
        {
            return await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Id == id)
                .Include(t => t.Klients)
                .FirstOrDefaultAsync();
        }

        public async Task<ServisaLapa> TransportlidzeklaServisaLapaAsync(int id)
        {
            // transportlīdzekļiem JSON tiek ignorēts Klients, tāpēc ielādējam atsevišķi
            var vehicle = await GetTransportlidzeklisAsync(id);

            var sheet = await _context.ServisaLapas.AsNoTracking()
                .Where(s => s.TransportlidzeklaId == id && s.Apmaksata == null)
                .Include(s => s.Defekti)
                .Include(s => s.RezervesDalas)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.ServisaLapasMehaniki)
                .FirstOrDefaultAsync();
            if(sheet == null)
            {               
                sheet = new ServisaLapa
                {
                    Id = 0,
                    Datums = DateTime.Now,
                    TransportlidzeklaId = vehicle.Id,
                    Defekti = new List<Defekts>(),
                    RezervesDalas = new List<RezervesDala>(),
                    PaveiktieDarbi = new List<Darbs>(),
                    ServisaLapasMehaniki = new List<ServisaLapasMehanikis>()
                };
            }
            sheet.Transportlidzeklis = vehicle;
            sheet.Klients = vehicle.Klients;
            return sheet;
        }

        public async Task<List<Mehanikis>> GetMehanikiAsync()
        {
            return await _context.Darbinieki.AsNoTracking()
                .Select(m => new Mehanikis { Id = m.Id, Name = m.PilnsVards })
                .ToListAsync();
        }

        public async Task<List<Mehanikis>> GetMehanikiAsync(List<int> mehanikuId)
        {
            return await _context.Darbinieki.AsNoTracking()
                .Where(d => mehanikuId.Contains(d.Id))
                .Select(m => new Mehanikis { Id = m.Id, Name = m.PilnsVards })
                .ToListAsync();
        }

        public async Task<int> InsertServisaLapaAsync(ServisaLapa sheet)
        {
            //javascript datumi ir UTC - pārvēršam uz LocalTime
            if(sheet.Datums.Kind == DateTimeKind.Utc)
            {
                sheet.Datums = sheet.Datums.ToLocalTime();
            }            
            if (sheet.Apmaksata.HasValue && sheet.Apmaksata.Value.Kind == DateTimeKind.Utc)
            {
                sheet.Apmaksata = sheet.Apmaksata.Value.ToLocalTime();
            }

            var servisaLapa = new ServisaLapa
            {
                Datums = sheet.Datums,
                Piezimes = sheet.Piezimes,
                Apmaksata = sheet.Apmaksata,
                Klients = sheet.Klients,
                TransportlidzeklaId = sheet.TransportlidzeklaId,
                Defekti = sheet.Defekti,
                RezervesDalas = sheet.RezervesDalas,
                PaveiktieDarbi = sheet.PaveiktieDarbi,
                ServisaLapasMehaniki = sheet.ServisaLapasMehaniki
            };

            await _context.ServisaLapas.AddAsync(servisaLapa);
            await _context.SaveChangesAsync();

            return servisaLapa.Id;
        }

        public async Task<int> UpdateServisaLapaAsync(ServisaLapa sheet)
        {
            var servisaLapa = await _context.ServisaLapas
                .Where(l => l.Id == sheet.Id)
                .Include(s => s.Defekti)
                .Include(s => s.RezervesDalas)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.ServisaLapasMehaniki)
                .FirstOrDefaultAsync();
            if (servisaLapa == null)
            {
                throw new CustomException("Servisa lapa neeksistē");
            }

            var result = 0;

            #region Defekti

            var defToBeRemoved = (from d in servisaLapa.Defekti where !sheet.Defekti.Any(s => s.Id > 0 && s.Id == d.Id) select d).ToList();
            var defToBeAdded = (from s in sheet.Defekti where !servisaLapa.Defekti.Any(d => s.Id == d.Id) select s).ToList();

            if (defToBeRemoved.Count > 0) // izdzēšam noņemtos defektus
            {
                foreach (var d in defToBeRemoved)
                {
                    servisaLapa.Defekti.Remove(d);
                }
            }

            if (defToBeAdded.Count > 0) // pievienojam jaunos
            {
                foreach (var d in defToBeAdded)
                {
                    servisaLapa.Defekti.Add(new Defekts
                    {
                        Veids = d.Veids,
                        Nosaukums = d.Nosaukums
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajos defektos
            result = await _context.SaveChangesAsync();

            foreach (var def in servisaLapa.Defekti)
            {
                var nd = sheet.Defekti.Where(d => d.Id == def.Id).FirstOrDefault();
                if (nd != null)
                {
                    def.Nosaukums = nd.Nosaukums;
                }
            }

            #endregion

            #region Paveiktais darbs

            var jobToBeRemoved = (from d in servisaLapa.PaveiktieDarbi where !sheet.PaveiktieDarbi.Any(s => s.Id > 0 && s.Id == d.Id) select d).ToList();
            var jobToBeAdded = (from s in sheet.PaveiktieDarbi where !servisaLapa.PaveiktieDarbi.Any(d => s.Id == d.Id) select s).ToList();

            if (jobToBeRemoved.Count > 0) // izdzēšam noņemtos darbus
            {
                foreach (var j in jobToBeRemoved)
                {
                    servisaLapa.PaveiktieDarbi.Remove(j);
                }
            }

            if (jobToBeAdded.Count > 0) // pievienojam jaunos
            {
                foreach (var j in jobToBeAdded)
                {
                    servisaLapa.PaveiktieDarbi.Add(new Darbs
                    {
                        Nosaukums = j.Nosaukums,
                        Skaits = j.Skaits,
                        Mervieniba = j.Mervieniba,
                        Cena = j.Cena
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajos darbos
            result = await _context.SaveChangesAsync();

            foreach (var job in servisaLapa.PaveiktieDarbi)
            {
                var nj = sheet.PaveiktieDarbi.Where(d => d.Id == job.Id).FirstOrDefault();
                if (nj != null)
                {
                    job.Nosaukums = nj.Nosaukums;
                    job.Skaits = nj.Skaits;
                    job.Mervieniba = nj.Mervieniba;
                    job.Cena = nj.Cena;
                }
            }

            #endregion

            #region Rezerves daļas

            var partsToBeRemoved = (from d in servisaLapa.RezervesDalas where !sheet.RezervesDalas.Any(s => s.Id > 0 && s.Id == d.Id) select d).ToList();
            var partsToBeAdded = (from s in sheet.RezervesDalas where !servisaLapa.RezervesDalas.Any(d => s.Id == d.Id) select s).ToList();

            if (partsToBeRemoved.Count > 0) // izdzēšam noņemtās rezerves daļas
            {
                foreach (var p in partsToBeRemoved)
                {
                    servisaLapa.RezervesDalas.Remove(p);
                }
            }

            if (partsToBeAdded.Count > 0) // pievienojam jaunās
            {
                foreach (var p in partsToBeAdded)
                {
                    servisaLapa.RezervesDalas.Add(new RezervesDala
                    {
                        Nosaukums = p.Nosaukums,
                        Skaits = p.Skaits,
                        Mervieniba = p.Mervieniba,
                        Cena = p.Cena
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajās rezerves daļās
            result = await _context.SaveChangesAsync();

            foreach (var part in servisaLapa.RezervesDalas)
            {
                var np = sheet.RezervesDalas.Where(d => d.Id == part.Id).FirstOrDefault();
                if (np != null)
                {
                    part.Nosaukums = np.Nosaukums;
                    part.Skaits = np.Skaits;
                    part.Mervieniba = np.Mervieniba;
                    part.Cena = np.Cena;
                }
            }

            #endregion

            #region Mehāniķi

            var mehToBeRemoved = (from d in servisaLapa.ServisaLapasMehaniki where !sheet.ServisaLapasMehaniki.Any(s => s.MehanikaId == d.MehanikaId) select d).ToList();
            var mehToBeAdded = (from s in sheet.ServisaLapasMehaniki where !servisaLapa.ServisaLapasMehaniki.Any(d => s.MehanikaId == d.MehanikaId) select s).ToList();

            if (mehToBeRemoved.Count > 0) // izdzēšam noņemtos mehāniķus
            {
                foreach (var m in mehToBeRemoved)
                {
                    servisaLapa.ServisaLapasMehaniki.Remove(m);
                }
            }

            if (mehToBeAdded.Count > 0) // pievienojam jaunos
            {
                foreach (var m in mehToBeAdded)
                {
                    servisaLapa.ServisaLapasMehaniki.Add(new ServisaLapasMehanikis
                    {
                        MehanikaId = m.MehanikaId
                    });
                }
            }

            #endregion

            //javascript datumi ir UTC - pārvēršam uz LocalTime
            if(sheet.Datums.Kind == DateTimeKind.Utc)
            {
                servisaLapa.Datums = sheet.Datums.ToLocalTime();
            }
            else
            {
                servisaLapa.Datums = sheet.Datums;
            }
            
            if (sheet.Apmaksata.HasValue && sheet.Apmaksata.Value.Kind == DateTimeKind.Utc)
            {
                servisaLapa.Apmaksata = sheet.Apmaksata.Value.ToLocalTime();
            }
            else
            {
                servisaLapa.Apmaksata = sheet.Apmaksata;
            }

            servisaLapa.Piezimes = sheet.Piezimes;         

            result += await _context.SaveChangesAsync();
            return result;
        }

        public async Task<List<Transportlidzeklis>> PaslaikRemontaAsync()
        {
            // remonta esošo auto id
            var vehiclesId = await _context.ServisaLapas.AsNoTracking()
                .Where(s => s.Apmaksata == null)
                .Select(t => t.TransportlidzeklaId)
                .ToListAsync();

            return await _context.Transportlidzekli.AsNoTracking()
                .Where(t => vehiclesId.Contains(t.Id))
                .ToListAsync();
        }
    }
}