using AppointmentSchedulingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSchedulingSystem.Data;

public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; } // Hastalar tablosu
        public DbSet<Doctor> Doctors { get; set; } // Doktorlar tablosu
        public DbSet<Clinical> Clinicals { get; set; } // Klinikler tablosu
        public DbSet<Appointment> Appointments { get; set; } // Randevular tablosu
    }

