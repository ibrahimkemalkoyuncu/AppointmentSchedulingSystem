using AppointmentSchedulingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSchedulingSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Migrasyonları uygula (Veritabanını oluştur veya güncelle)
            context.Database.Migrate();

            // Look for any clinics.
            if (context.Clinicals.Any())
            {
                return;   // DB has been seeded
            }

            var clinics = new Clinical[]
            {
                new Clinical{Name="Dahiliye (İç Hastalıkları)", Address="Blok A, Kat 1", PhoneNumber="02125551001"},
                new Clinical{Name="Kardiyoloji", Address="Blok A, Kat 2", PhoneNumber="02125551002"},
                new Clinical{Name="Göz Hastalıkları", Address="Blok B, Kat 1", PhoneNumber="02125551003"},
                new Clinical{Name="Kulak Burun Boğaz (KBB)", Address="Blok B, Kat 2", PhoneNumber="02125551004"},
                new Clinical{Name="Psikiyatri", Address="Blok C, Zemin", PhoneNumber="02125551005"}
            };
            foreach (Clinical c in clinics)
            {
                context.Clinicals.Add(c);
            }
            context.SaveChanges();

            var dahiliye = context.Clinicals.First(c => c.Name == "Dahiliye (İç Hastalıkları)");
            var kardiyoloji = context.Clinicals.First(c => c.Name == "Kardiyoloji");
            var goz = context.Clinicals.First(c => c.Name == "Göz Hastalıkları");
            var kbb = context.Clinicals.First(c => c.Name == "Kulak Burun Boğaz (KBB)");
            var psikiyatri = context.Clinicals.First(c => c.Name == "Psikiyatri");

            var doctors = new Doctor[]
            {
                new Doctor{Name="Ahmet", Surname="Yılmaz", Specialization="Uzman Doktor", ClinicalId=dahiliye.Id, AppointmentDuration=15},
                new Doctor{Name="Ayşe", Surname="Kaya", Specialization="Doçent Doktor", ClinicalId=kardiyoloji.Id, AppointmentDuration=30},
                new Doctor{Name="Mehmet", Surname="Demir", Specialization="Operatör Doktor", ClinicalId=goz.Id, AppointmentDuration=20},
                new Doctor{Name="Fatma", Surname="Çelik", Specialization="Profesör Doktor", ClinicalId=kbb.Id, AppointmentDuration=20},
                new Doctor{Name="Ali", Surname="Öztürk", Specialization="Uzman Doktor", ClinicalId=psikiyatri.Id, AppointmentDuration=45},
                new Doctor{Name="Zeynep", Surname="Arslan", Specialization="Uzman Doktor", ClinicalId=dahiliye.Id, AppointmentDuration=15}
            };
            foreach (Doctor d in doctors)
            {
                context.Doctors.Add(d);
            }
            context.SaveChanges();

            var patients = new Patient[]
            {
                new Patient{Name="Can", Surname="Yıldız", IdentityNumber="11111111111", PhoneNumber="5551112233", Email="can@example.com"},
                new Patient{Name="Elif", Surname="Kara", IdentityNumber="22222222222", PhoneNumber="5554445566", Email="elif@example.com"},
                new Patient{Name="Burak", Surname="Beyaz", IdentityNumber="33333333333", PhoneNumber="5557778899", Email="burak@example.com"}
            };
            foreach (Patient p in patients)
            {
                context.Patients.Add(p);
            }
            context.SaveChanges();

            // Create some appointments for today
            var doctorAhmet = context.Doctors.First(d => d.Name == "Ahmet");
            var doctorAyse = context.Doctors.First(d => d.Name == "Ayşe");
            var patientCan = context.Patients.First(p => p.Name == "Can");
            var patientElif = context.Patients.First(p => p.Name == "Elif");

            var today = DateTime.Today.AddHours(9); // 09:00 Today

            var appointments = new Appointment[]
            {
                new Appointment{
                    PatientId=patientCan.Id,
                    DoctorId=doctorAhmet.Id,
                    AppointmentDate=today,
                    EndDate=today.AddMinutes(doctorAhmet.AppointmentDuration),
                    Status="Beklemede"
                },
                 new Appointment{
                    PatientId=patientElif.Id,
                    DoctorId=doctorAyse.Id,
                    AppointmentDate=today.AddHours(1), // 10:00
                    EndDate=today.AddHours(1).AddMinutes(doctorAyse.AppointmentDuration),
                    Status="Beklemede"
                },
                 new Appointment{
                    PatientId=patientCan.Id,
                    DoctorId=doctorAyse.Id,
                    AppointmentDate=today.AddHours(4), // 13:00 (Can can see another doctor later)
                    EndDate=today.AddHours(4).AddMinutes(doctorAyse.AppointmentDuration),
                    Status="Onaylandı"
                }
            };
            foreach (Appointment a in appointments)
            {
                context.Appointments.Add(a);
            }
            context.SaveChanges();
        }
    }
}
