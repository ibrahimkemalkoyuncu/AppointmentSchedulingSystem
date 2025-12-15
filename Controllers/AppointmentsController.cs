using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppointmentSchedulingSystem.Data;
using AppointmentSchedulingSystem.Models;

namespace AppointmentSchedulingSystem.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
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
            ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name");
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
                // Doktorun varsayılan randevu süresini getir
                var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
                var duration = doctor?.AppointmentDuration ?? 30; // Varsayılan 30 dk
                appointment.EndDate = appointment.AppointmentDate.AddMinutes(duration);

                // Mesai Saati Kontrolü (Örn: 09:00 - 17:00)
                // Randevu 09:00'dan önce başlayamaz ve 17:00'dan sonra bitemez.
                var workStart = new TimeSpan(9, 0, 0);
                var workEnd = new TimeSpan(17, 0, 0);

                if (appointment.AppointmentDate.TimeOfDay < workStart || appointment.EndDate.TimeOfDay > workEnd)
                {
                    ModelState.AddModelError("", "Randevular 09:00 - 17:00 saatleri arasında olmalıdır.");
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

                // 1. Doktor Çakışma Kontrolü
                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.DoctorId == appointment.DoctorId &&
                                a.AppointmentDate < appointment.EndDate &&
                                a.EndDate > appointment.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    ModelState.AddModelError("", "Bu tarih ve saatte doktorun başka bir randevusu bulunmaktadır.");
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

                // 2. Hasta Çakışma Kontrolü (Hasta aynı anda iki yerde olamaz)
                var patientConflict = await _context.Appointments
                    .Where(a => a.PatientId == appointment.PatientId &&
                                a.AppointmentDate < appointment.EndDate &&
                                a.EndDate > appointment.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (patientConflict != null)
                {
                    ModelState.AddModelError("", "Hastanın bu saatte başka bir doktorla randevusu bulunmaktadır.");
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
                // Doktorun varsayılan randevu süresini getir
                var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
                var duration = doctor?.AppointmentDuration ?? 30; // Varsayılan 30 dk
                appointment.EndDate = appointment.AppointmentDate.AddMinutes(duration);

                // Mesai Saati Kontrolü (Örn: 09:00 - 17:00)
                // Randevu 09:00'dan önce başlayamaz ve 17:00'dan sonra bitemez.
                var workStart = new TimeSpan(9, 0, 0);
                var workEnd = new TimeSpan(17, 0, 0);

                if (appointment.AppointmentDate.TimeOfDay < workStart || appointment.EndDate.TimeOfDay > workEnd)
                {
                    ModelState.AddModelError("", "Randevular 09:00 - 17:00 saatleri arasında olmalıdır.");
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

                // 1. Doktor Çakışma Kontrolü
                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.Id != appointment.Id && // Kendi kendisiyle çakışmasını önle
                                a.DoctorId == appointment.DoctorId &&
                                a.AppointmentDate < appointment.EndDate &&
                                a.EndDate > appointment.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    ModelState.AddModelError("", "Bu tarih ve saatte doktorun başka bir randevusu bulunmaktadır.");
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

                // 2. Hasta Çakışma Kontrolü
                var patientConflict = await _context.Appointments
                    .Where(a => a.Id != appointment.Id && // Kendi kendisini hariç tut
                                a.PatientId == appointment.PatientId &&
                                a.AppointmentDate < appointment.EndDate &&
                                a.EndDate > appointment.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (patientConflict != null)
                {
                    ModelState.AddModelError("", "Hastanın bu saatte başka bir doktorla randevusu bulunmaktadır.");
                    ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Name", appointment.PatientId);
                    ViewData["DoctorId"] = new SelectList(_context.Doctors, "Id", "Name", appointment.DoctorId);
                    return View(appointment);
                }

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
