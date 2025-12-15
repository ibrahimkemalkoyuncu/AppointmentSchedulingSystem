using AppointmentSchedulingSystem.Data;
using AppointmentSchedulingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSchedulingSystem.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DateTime> CalculateEndDateAsync(int doctorId, DateTime startDate)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            var duration = doctor?.AppointmentDuration ?? 30;
            return startDate.AddMinutes(duration);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateAppointmentAsync(Appointment appointment)
        {
            // 1. EndDate Hesaplama (Eğer set edilmemişse)
            if (appointment.EndDate == default || appointment.EndDate <= appointment.AppointmentDate)
            {
                 appointment.EndDate = await CalculateEndDateAsync(appointment.DoctorId, appointment.AppointmentDate);
            }

            // 2. Mesai Saati Kontrolü (09:00 - 17:00)
            var workStart = new TimeSpan(9, 0, 0);
            var workEnd = new TimeSpan(17, 0, 0);

            if (appointment.AppointmentDate.TimeOfDay < workStart || appointment.EndDate.TimeOfDay > workEnd)
            {
                return (false, "Randevular 09:00 - 17:00 saatleri arasında olmalıdır.");
            }

            // 3. Doktor Çakışma Kontrolü
            var doctorConflict = await _context.Appointments
                .Where(a => a.Id != appointment.Id && // Kendisi hariç (Update durumu için)
                            a.DoctorId == appointment.DoctorId &&
                            a.AppointmentDate < appointment.EndDate &&
                            a.EndDate > appointment.AppointmentDate)
                .AnyAsync();

            if (doctorConflict)
            {
                return (false, "Bu tarih ve saatte doktorun başka bir randevusu bulunmaktadır.");
            }

            // 4. Hasta Çakışma Kontrolü
            var patientConflict = await _context.Appointments
                .Where(a => a.Id != appointment.Id && // Kendisi hariç
                            a.PatientId == appointment.PatientId &&
                            a.AppointmentDate < appointment.EndDate &&
                            a.EndDate > appointment.AppointmentDate)
                .AnyAsync();

            if (patientConflict)
            {
                return (false, "Hastanın bu saatte başka bir doktorla randevusu bulunmaktadır.");
            }

            return (true, null);
        }
    }
}
