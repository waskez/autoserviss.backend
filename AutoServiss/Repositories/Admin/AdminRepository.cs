using System.Linq;
using System.Threading.Tasks;
using AutoServiss.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AutoServiss.Repositories.Admin
{
    public class AdminRepository : IAdminRepository
    {
        #region Fields

        private readonly AutoServissDbContext _context;
        private readonly AppSettings _settings;
        private readonly IMemoryCache _memoryCache;

        #endregion

        #region Constructor

        public AdminRepository(
            IOptions<AppSettings> settings,
            IMemoryCache memoryCache,
            AutoServissDbContext context)
        {
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _context = context;
        }

        #endregion

        #region Markas

        public async Task<int> InsertMarkaAsync(Marka marka)
        {
            marka.Nosaukums = marka.Nosaukums.ToUpper();
            // pārbaudam vai marka jau esistē
            var exist = await _context.Markas.AsNoTracking()
                .Where(m => m.Nosaukums == marka.Nosaukums)
                .CountAsync();
            if(exist > 0)
            {
                throw new BadRequestException("Marka ar šādu nosaukumu jau eksistē");
            }

            _memoryCache.Remove("VEHICLE-BRANDS");

            _context.Markas.Add(marka);
            await _context.SaveChangesAsync();
            return marka.Id;
        }

        public async Task<int> UpdateMarkaAsync(Marka marka)
        {
            marka.Nosaukums = marka.Nosaukums.ToUpper();
            // pārbaudam vai marka jau esistē
            var exist = await _context.Markas.AsNoTracking()
                .Where(m => m.Id != marka.Id && m.Nosaukums == marka.Nosaukums)
                .CountAsync();
            if (exist > 0)
            {
                throw new BadRequestException("Marka ar šādu nosaukumu jau eksistē");
            }

            _memoryCache.Remove("VEHICLE-BRANDS");            

            var brand = await _context.Markas.Where(m => m.Id == marka.Id).FirstAsync();
            // nepieciešams atjaunināt arī transportlīdzekļu markas
            var vehicles = await _context.Transportlidzekli.Where(t => t.Marka == brand.Nosaukums).ToListAsync();
            vehicles.ForEach(v => v.Marka = marka.Nosaukums);

            brand.Nosaukums = marka.Nosaukums;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteMarkaAsync(int id)
        {
            var marka = await _context.Markas.Where(m => m.Id == id).FirstOrDefaultAsync();
            if (marka == null)
            {
                throw new BadRequestException($"Marka ar Id={id} netika atrasta");
            }

            //pārbaudam vai ir transportlīdzekļi ar šādu marku
            var vehicles = await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Marka == marka.Nosaukums)
                .CountAsync();
            if(vehicles > 0)
            {
                throw new BadRequestException("Nedrīkst dzēst marku kura piesaistīta transportlīdzeklim");
            }

            _memoryCache.Remove("VEHICLE-BRANDS");

            _context.Markas.Remove(marka);
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Modeļi

        public async Task<int> InsertModelisAsync(Modelis modelis)
        {
            modelis.Nosaukums = modelis.Nosaukums.ToUpper();
            // pārbaudam vai modelis jau esistē
            var exist = await _context.Modeli.AsNoTracking()
                .Where(m => m.MarkasId == modelis.MarkasId && m.Nosaukums == modelis.Nosaukums)
                .CountAsync();
            if (exist > 0)
            {
                throw new BadRequestException("Modelis ar šādu nosaukumu jau eksistē");
            }

            _memoryCache.Remove($"VEHICLE-MODELS-{modelis.MarkasId}");

            _context.Modeli.Add(modelis);
            await _context.SaveChangesAsync();
            return modelis.Id;
        }

        public async Task<int> UpdateModelisAsync(Modelis modelis)
        {
            modelis.Nosaukums = modelis.Nosaukums.ToUpper();
            // pārbaudam vai modelis jau esistē
            var exist = await _context.Modeli.AsNoTracking()
                .Where(m => m.MarkasId == modelis.MarkasId && m.Id != modelis.Id && m.Nosaukums == modelis.Nosaukums)
                .CountAsync();
            if (exist > 0)
            {
                throw new BadRequestException("Modelis ar šādu nosaukumu jau eksistē");
            }

            _memoryCache.Remove($"VEHICLE-MODELS-{modelis.MarkasId}");

            var model = await _context.Modeli.Where(m => m.Id == modelis.Id).FirstAsync();
            // nepieciešams atjaunināt arī transportlīdzekļu modeļus
            var vehicles = await _context.Transportlidzekli.Where(t => t.Marka == model.Nosaukums).ToListAsync();
            vehicles.ForEach(v => v.Marka = modelis.Nosaukums);

            model.Nosaukums = modelis.Nosaukums;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteModelisAsync(int id)
        {
            var modelis = await _context.Modeli.Where(m => m.Id == id).FirstOrDefaultAsync();
            if (modelis == null)
            {
                throw new BadRequestException($"Modelis ar Id={id} netika atrasta");
            }

            //pārbaudam vai ir transportlīdzekļi ar šādu modeli
            var vehicles = await _context.Transportlidzekli.AsNoTracking()
                .Where(t => t.Modelis == modelis.Nosaukums)
                .CountAsync();
            if (vehicles > 0)
            {
                throw new BadRequestException("Nedrīkst dzēst modeli kurš piesaistīts transportlīdzeklim");
            }

            _memoryCache.Remove($"VEHICLE-MODELS-{modelis.MarkasId}");

            _context.Modeli.Remove(modelis);
            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}