using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppointmentSchedulingSystem.Data;
using AppointmentSchedulingSystem.Models;
using AppointmentSchedulingSystem.Services;

namespace AppointmentSchedulingSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(ApplicationDbContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Appointments.Include(a => a.Doctor).Include(a => a.Patient);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Hastanın geçmiş tıbbi kayıtlarını getir (Tamamlanmış randevular)
            var history = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == appointment.PatientId &&
                            a.Status == "Tamamlandı" &&
                            a.Id != appointment.Id) // Şu anki randevu hariç
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.PatientHistory = history;

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            // Başlangıçta doktor listesi boş veya tümü olabilir.
            // UX açısından önce Klinik seçtirmek daha doğru, bu yüzden Doktor listesini boş gönderiyoruz (veya seçiniz uyarısı ile).
            // Ancak Edit durumunda veya Validasyon hatasında doluluk gerekebilir.
            ViewData["ClinicalId"] = new SelectList(_context.Clinicals, "Id", "Name");
            ViewData["DoctorId"] = new SelectList(new List<Doctor>(), "Id", "Name"); // Başlangıçta boş
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "IdentityNumber");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientId,DoctorId,AppointmentDate")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                // Bitiş tarihini hesapla
                appointment.EndDate = await _appointmentService.CalculateEndDateAsync(appointment.DoctorId, appointment.AppointmentDate);

                // Service Layer Validasyonu
                var validationResult = await _appointmentService.ValidateAppointmentAsync(appointment);
                if (!validationResult.IsValid)
                {
                    ModelState.AddModelError("", validationResult.ErrorMessage);
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

                appointment.Status = "Beklemede";
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "IdentityNumber", appointment.PatientId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId,DoctorId,AppointmentDate,Status")] Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Mevcut randevuyu veritabanından çek (Tıbbi verileri korumak için)
                var existingAppointment = await _context.Appointments.FindAsync(id);
                if (existingAppointment == null)
                {
                    return NotFound();
                }

                // Sadece izin verilen alanları güncelle
                existingAppointment.PatientId = appointment.PatientId;
                existingAppointment.DoctorId = appointment.DoctorId;
                existingAppointment.AppointmentDate = appointment.AppointmentDate;
                existingAppointment.Status = appointment.Status;

                // Bitiş tarihini hesapla
                existingAppointment.EndDate = await _appointmentService.CalculateEndDateAsync(existingAppointment.DoctorId, existingAppointment.AppointmentDate);

                // Service Layer Validasyonu
                var validationResult = await _appointmentService.ValidateAppointmentAsync(existingAppointment);
                if (!validationResult.IsValid)
                {
                    ModelState.AddModelError("", validationResult.ErrorMessage);
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

                try
                {
                    _context.Update(existingAppointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "IdentityNumber", appointment.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }

        // POST: Appointments/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Tamamlandı";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "İptal Edildi";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Consultation/5
        public async Task<IActionResult> Consultation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Consultation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Consultation(int id, [Bind("Id,Complaint,Diagnosis,Treatment")] Appointment appointmentData)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            // Tıbbi bilgileri güncelle
            appointment.Complaint = appointmentData.Complaint;
            appointment.Diagnosis = appointmentData.Diagnosis;
            appointment.Treatment = appointmentData.Treatment;

            // Muayene tamamlandı olarak işaretle
            appointment.Status = "Tamamlandı";

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                 if (!AppointmentExists(appointment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index)); // veya Doktorun kendi paneline yönlendirilebilir
        }

        // GET: Appointments/PrintPrescription/5
        public async Task<IActionResult> PrintPrescription(int? id)
        {
             if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }
    }
}
