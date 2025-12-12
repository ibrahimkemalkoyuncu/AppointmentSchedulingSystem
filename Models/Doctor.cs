
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

    public List<Appointment>? Appointments { get; set; } // Doktorun randevuları
}
