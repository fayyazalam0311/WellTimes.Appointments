using AppointmentService.Data;
using AppointmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Repositories
{
    public class SlotRepository : ISlotRepository
    {
        private readonly AppointmentDbContext _context;

        public SlotRepository(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Slot slot, CancellationToken cancellationToken = default)
        {
            _context.Add(slot);

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasOverlapAsync(
            Guid doctorId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default)
        {
            return await _context.Slots.AnyAsync(
                s => s.DoctorId == doctorId
                && s.StartTimeUtc < endUtc
                && s.EndTimeUtc > startUtc,
                cancellationToken);
        }

        public async Task<List<Slot>> GetAvailableAsync(
            DateTime fromUtc,
            CancellationToken cancellationToken = default)
        {
            return await _context.Slots
                .AsNoTracking()
                .Where(s => !s.IsBooked && s.StartTimeUtc > fromUtc)
                .OrderBy(s => s.StartTimeUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<Slot?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Slots
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id,
                cancellationToken);
        }

    }
}
