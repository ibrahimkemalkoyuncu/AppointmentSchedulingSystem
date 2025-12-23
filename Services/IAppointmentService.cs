using AppointmentSchedulingSystem.Models;

namespace AppointmentSchedulingSystem.Services
{
    public interface IAppointmentService
    {
        /// <summary>
        /// Randevu talebinin geçerli olup olmadığını (Çakışma, Mesai Saati) kontrol eder.
        /// </summary>
        /// <param name="appointment">Kontrol edilecek randevu nesnesi.</param>
        /// <returns>Geçerli ise (true, null), değilse (false, hata mesajı).</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidateAppointmentAsync(Appointment appointment);

        /// <summary>
        /// Doktorun randevu süresine göre bitiş zamanını hesaplar.
        /// </summary>
        Task<DateTime> CalculateEndDateAsync(int doctorId, DateTime startDate);
    }
}
