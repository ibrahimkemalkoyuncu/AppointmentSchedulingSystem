
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
    public List<Appointment>? Appointments { get; set; } // Doktorun randevuları
}
