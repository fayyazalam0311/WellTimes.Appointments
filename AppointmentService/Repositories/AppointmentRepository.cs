using AppointmentService.Data;
using AppointmentService.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentDbContext _context;

        public AppointmentRepository(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TryCreateAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var rowsAffected = await _context.Slots
                .Where(s => s.Id == appointment.SlotId && !s.IsBooked)
                .ExecuteUpdateAsync(set => set.SetProperty(s => s.IsBooked, true), cancellationToken);

            if (rowsAffected == 0)
            {
                return false;
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return true;
        }

        public async Task<Appointment?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<List<Appointment>> GetByPatientAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Slot)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.Slot.StartTimeUtc)
                .ToListAsync(cancellationToken);
        }
        public async Task<List<Appointment>> GetByDoctorAsync(
            Guid doctorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Slot)
                .Where(a => a.Slot.DoctorId == doctorId)
                .OrderBy(a => a.Slot.StartTimeUtc)
                .ToListAsync(cancellationToken);
        }
        public async Task UpdateAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }


    }
}
