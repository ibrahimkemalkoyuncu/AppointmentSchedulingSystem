
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AppointmentSchedulingSystem.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; }// Adı

    [Required] 
    public string Surname { get; set; } // Soyadı
    public string Specialization { get; set; } // Uzmanlık alanı

    // Foreign Key for Clinical
    public int ClinicalId { get; set; }
    public Clinical? Clinical { get; set; }

    // Randevu süresi (Dakika cinsinden). Varsayılan: 30
    [Range(5, 120)]
    public int AppointmentDuration { get; set; } = 30;

    public List<Appointment>? Appointments { get; set; } // Doktorun randevuları
}
