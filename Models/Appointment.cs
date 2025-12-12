
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AppointmentSchedulingSystem.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }
    public DateTime AppointmentDate { get; set; } // Başlangıç Zamanı
    public DateTime EndDate { get; set; } // Bitiş Zamanı (Yeni)
    public string? Status { get; set; } // Örneğin: Beklemede, Onaylandı, İptal Edildi

    // Tıbbi Kayıtlar (Medical Records)
    [Display(Name = "Hasta Şikayeti")]
    public string? Complaint { get; set; } // Şikayet

    [Display(Name = "Doktor Tanısı")]
    public string? Diagnosis { get; set; } // Tanı

    [Display(Name = "Uygulanan Tedavi / Reçete")]
    public string? Treatment { get; set; } // Tedavi
}