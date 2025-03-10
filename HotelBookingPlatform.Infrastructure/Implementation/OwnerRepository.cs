namespace HotelBookingPlatform.Infrastructure.Implementation
{
    public class OwnerRepository : GenericRepository<Owner>, IOwnerRepository
    {
        public OwnerRepository(AppDbContext context)
            : base(context) { }
        public async Task<IEnumerable<Owner>> GetAllWithHotelsAsync()
        {
            return await _appDbContext.Owners
                .Include(h => h.Hotels)
                .ToListAsync();
        }

        public async Task<Owner> CreateAsync(Owner owner) {

            await _appDbContext.Owners.AddAsync(owner);
            await _appDbContext.SaveChangesAsync();
            return owner;
        }
    }
}