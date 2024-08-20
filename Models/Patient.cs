
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AppointmentSchedulingSystem.Models;

public class Patient
{
    public int Id { get; set; }
    
    [Required] 
    public string Name { get; set; }
    
    [Required]    
    public string Surname { get; set; }
    
    [Required]
    public string IdentityNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public List<Appointment>? Appointments { get; set; }
}

