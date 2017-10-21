using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using iTextSharp.text;
using iTextSharp.text.pdf;
using AutoServiss.Database;
using AutoServiss.Models;

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

        public async Task<List<ServisaLapasUznemums>> GetUznemumiArMehanikiem()
        {
            var companies = await _context.Uznemumi.AsNoTracking()
                .Include(u => u.UznemumaDarbinieki)
                .ThenInclude(d => d.Darbinieks)
                .ToListAsync();
            return companies.Select(c => new ServisaLapasUznemums
            {
                Id = c.Id,
                Nosaukums = c.Nosaukums,
                Mehaniki = c.UznemumaDarbinieki.Select(d => new Mehanikis
                {
                    Id = d.DarbiniekaId,
                    Nosaukums = d.Darbinieks.PilnsVards
                }).ToList()
            }).ToList();
        }

        public async Task<Transportlidzeklis> GetTransportlidzeklisArKlientuAsync(int id)
        {
            var vehicle = await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();
            if(vehicle == null)
            {
                throw new BadRequestException($"Transportlīdzeklis ar id={id} netika atrasts");
            }
            var customer = await _context.Klienti.AsNoTracking()
                .Where(k => k.Id == vehicle.KlientaId)
                .Include(k => k.Adreses)
                .Include(k => k.Bankas)
                .FirstOrDefaultAsync();
            vehicle.Klients = customer ?? throw new BadRequestException($"Klients ar id={vehicle.KlientaId} netika atrasts");
            return vehicle;
        }

        public async Task<ServisaLapa> TransportlidzeklaServisaLapaAsync(int id)
        {
            var sheet = await _context.ServisaLapas.AsNoTracking()
                .Where(s => s.TransportlidzeklaId == id && s.Apmaksata == null)
                .Include(s => s.Uznemums)
                .Include(s => s.Klients)
                .Include(s => s.Transportlidzeklis)
                .Include(s => s.Defekti)
                .Include(s => s.RezervesDalas)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.Mehaniki)
                .FirstOrDefaultAsync();
            if(sheet == null)
            {
                var vehicle = await GetTransportlidzeklisArKlientuAsync(id);
                sheet = new ServisaLapa
                {
                    Id = 0,
                    Datums = DateTime.Now,                    
                    KlientaId = vehicle.KlientaId,
                    Klients = vehicle.Klients,
                    TransportlidzeklaId = vehicle.Id,
                    Transportlidzeklis = vehicle,
                    Defekti = new List<Defekts>(),
                    RezervesDalas = new List<RezervesDala>(),
                    PaveiktieDarbi = new List<PaveiktaisDarbs>(),
                    Mehaniki = new List<Mehanikis>()
                };
            }
            return sheet;
        }

        public async Task<ServisaLapa> ServisaLapaAsync(int id)
        {
            var sheet = await _context.ServisaLapas.AsNoTracking()
                .Where(s => s.Id == id)
                .Include(s => s.Uznemums)
                .Include(s => s.Klients)
                .Include(s => s.Transportlidzeklis)
                .Include(s => s.Defekti)
                .Include(s => s.RezervesDalas)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.Mehaniki)
                .FirstOrDefaultAsync();
            if (sheet == null)
            {
                throw new BadRequestException($"Servisa lapa ar id={id} neeksistē");
            }
            return sheet;
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

            var newSheet = new ServisaLapa
            {
                Datums = sheet.Datums,
                Apmaksata = sheet.Apmaksata,
                UznemumaId = sheet.UznemumaId,
                TransportlidzeklaId = sheet.TransportlidzeklaId,
                KlientaId = sheet.KlientaId,
                Defekti = sheet.Defekti,
                RezervesDalas = sheet.RezervesDalas,
                PaveiktieDarbi = sheet.PaveiktieDarbi,
                Mehaniki = sheet.Mehaniki,
                KopejaSumma = sheet.KopejaSumma
            };

            await _context.ServisaLapas.AddAsync(newSheet);
            await _context.SaveChangesAsync();
            return newSheet.Id;
        }

        public async Task<int> UpdateServisaLapaAsync(ServisaLapa sheet)
        {
            var servisaLapa = await _context.ServisaLapas
                .Where(s => s.Id == sheet.Id)
                .Include(s => s.Defekti)
                .Include(s => s.RezervesDalas)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.Mehaniki)
                .FirstOrDefaultAsync();
            if (servisaLapa == null)
            {
                throw new BadRequestException($"Servisa lapa ar id={sheet.Id} netika atrasta");
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
                    servisaLapa.PaveiktieDarbi.Add(new PaveiktaisDarbs
                    {
                        Nosaukums = j.Nosaukums,
                        Skaits = j.Skaits,
                        Mervieniba = j.Mervieniba,
                        Cena = j.Cena
                    });
                }
            }

            // lai atjauninātu izmaiņas esošajos darbos
            result += await _context.SaveChangesAsync();

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
            result += await _context.SaveChangesAsync();

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

            var mehToBeRemoved = (from m in servisaLapa.Mehaniki where !sheet.Mehaniki.Any(s => s.Id == m.Id) select m).ToList();
            var mehToBeAdded = (from s in sheet.Mehaniki where !servisaLapa.Mehaniki.Any(d => s.Id == d.Id) select s).ToList();

            if (mehToBeRemoved.Count > 0) // izdzēšam noņemtos mehāniķus
            {
                foreach (var m in mehToBeRemoved)
                {
                    servisaLapa.Mehaniki.Remove(m);
                }
            }

            if (mehToBeAdded.Count > 0) // pievienojam jaunos
            {
                foreach (var m in mehToBeAdded)
                {
                    servisaLapa.Mehaniki.Add(new Mehanikis
                    {
                        Id = m.Id,
                        Nosaukums = m.Nosaukums
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

            servisaLapa.UznemumaId = sheet.UznemumaId;
            servisaLapa.Piezimes = sheet.Piezimes;
            servisaLapa.KopejaSumma = sheet.KopejaSumma;

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

        public async Task<byte[]> PrintServisaLapaAsync(int id)
        {
            var sheet = await _context.ServisaLapas.AsNoTracking()
                .Where(s => s.Id == id)
                .Include(s => s.Uznemums).ThenInclude(u=>u.Adreses)
                .Include(s => s.Klients)
                .Include(s => s.Transportlidzeklis)
                .Include(s => s.Defekti)
                .Include(s => s.PaveiktieDarbi)
                .Include(s => s.RezervesDalas)
                .FirstOrDefaultAsync();
            if(sheet == null)
            {
                throw new BadRequestException($"Servisa lapa ar id={id} neeksistē");
            }

            var pdfHelper = new PdfHelper();

            var pdfDocument = new Document();
            pdfDocument.SetPageSize(PageSize.A4);
            pdfDocument.SetMargins(40, 40, 40, 40);

            using (var outputMemoryStream = new MemoryStream())
            {
                var pdfWriter = PdfWriter.GetInstance(pdfDocument, outputMemoryStream);
                pdfDocument.Open();

                var table = new PdfPTable(1) { WidthPercentage = 100 };

                #region Firmas rekvizīti + virsraksts + automašīnas dati

                var cell = new PdfPCell(new Phrase(sheet.Uznemums.Nosaukums, pdfHelper.TekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(sheet.Uznemums.PvnNumurs, pdfHelper.TekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Border = 0
                };
                table.AddCell(cell);

                foreach(var a in sheet.Uznemums.Adreses)
                {
                    cell = new PdfPCell(new Phrase(a.Nosaukums, pdfHelper.TekstaFonts))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Border = 0
                    };
                    table.AddCell(cell);
                }                

                cell = new PdfPCell(new Phrase(sheet.Uznemums.Talrunis, pdfHelper.TekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("SERVISA LAPA", pdfHelper.VirsrakstaFonts))
                {
                    PaddingTop = 8f,
                    PaddingBottom = 8f,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase($"Automašīna: {sheet.Transportlidzeklis.Marka} {sheet.Transportlidzeklis.Modelis}, r/n: {sheet.Transportlidzeklis.Numurs}", pdfHelper.TekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Border = 0
                };
                table.AddCell(cell);

                pdfDocument.Add(table);

                #endregion

                #region Defekti

                var pieteiktie = sheet.Defekti.Where(d => d.Veids == DefektaVeids.Pieteiktais).ToList();
                var atrastie = sheet.Defekti.Where(d => d.Veids == DefektaVeids.Atrastais).ToList();
                var paliekosie = sheet.Defekti.Where(d => d.Veids == DefektaVeids.Paliekošais).ToList();

                if (pieteiktie.Count > 0)
                {
                    table = pdfHelper.DefektuTabula("Pieteiktie defekti", pieteiktie);
                    pdfDocument.Add(table);
                }

                if (atrastie.Count > 0)
                {
                    table = pdfHelper.DefektuTabula("Atrastie defekti", pieteiktie);
                    pdfDocument.Add(table);
                }

                if (paliekosie.Count > 0)
                {
                    table = pdfHelper.DefektuTabula("Paliekošie defekti", pieteiktie);
                    pdfDocument.Add(table);
                }

                #endregion

                #region Paveiktais darbs

                if (sheet.PaveiktieDarbi.Count > 0)
                {
                    table = pdfHelper.CenuTabulasHeader("Paveiktie darbi");

                    var counter = 1;
                    foreach(var job in sheet.PaveiktieDarbi)
                    {
                        table.AddCell(pdfHelper.TableCellCenter(counter.ToString()));
                        table.AddCell(pdfHelper.TableCellLeft(job.Nosaukums));
                        table.AddCell(pdfHelper.TableCellCenter(job.Skaits.ToString()));
                        table.AddCell(pdfHelper.TableCellCenter(job.Mervieniba));
                        table.AddCell(pdfHelper.TableCellCenter(job.Cena.ToString("F2")));

                        counter++;
                    }

                    table.AddCell(pdfHelper.TableCellTotalTitle());
                    table.AddCell(pdfHelper.TableCellTotalSum(sheet.PaveiktieDarbi.Select(d => d.Cena).Sum()));

                    pdfDocument.Add(table);
                }

                #endregion

                #region Rezerves daļas

                if (sheet.RezervesDalas.Count > 0)
                {
                    table = pdfHelper.CenuTabulasHeader("Rezerves daļas");

                    var counter = 1;
                    foreach (var part in sheet.RezervesDalas)
                    {
                        table.AddCell(pdfHelper.TableCellCenter(counter.ToString()));
                        table.AddCell(pdfHelper.TableCellLeft(part.Nosaukums));
                        table.AddCell(pdfHelper.TableCellCenter(part.Skaits.ToString("F2")));
                        table.AddCell(pdfHelper.TableCellCenter(part.Mervieniba));
                        table.AddCell(pdfHelper.TableCellCenter(part.Cena.ToString("F2")));

                        counter++;
                    }

                    table.AddCell(pdfHelper.TableCellTotalTitle());
                    table.AddCell(pdfHelper.TableCellTotalSum(sheet.RezervesDalas.Select(d=>d.Cena).Sum()));

                    pdfDocument.Add(table);
                }

                #endregion

                #region Kopējā summa + paraksti

                table = new PdfPTable(1)
                {
                    WidthPercentage = 100
                };

                cell = new PdfPCell()
                {
                    Colspan = 4,
                    PaddingBottom = 20f,
                    Border = 0
                };
                table.AddCell(cell);

                var summaDarbi = sheet.PaveiktieDarbi.Select(d => d.Cena).Sum();
                var summaDalas = sheet.RezervesDalas.Select(d => d.Cena).Sum();

                cell = new PdfPCell(new Phrase("Kopējā summa:  " + (summaDarbi + summaDalas).ToString("F2"), pdfHelper.VirsrakstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingBottom = 6f,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell()
                {
                    Colspan = 4,
                    PaddingBottom = 20f,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Servisa vadītājs ....................................................", pdfHelper.TekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 6f,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell()
                {
                    Colspan = 4,
                    PaddingBottom = 16f,
                    Border = 0
                };
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Klients ..................................................................", pdfHelper.TekstaFonts))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 6f,
                    Border = 0
                };
                table.AddCell(cell);

                pdfDocument.Add(table);

                #endregion

                pdfWriter.CloseStream = false;
                pdfDocument.Close();

                outputMemoryStream.Position = 0;
                byte[] file = outputMemoryStream.ToArray();

                return file;
            }
        }
    }
}