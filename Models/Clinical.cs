using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppointmentSchedulingSystem.Models;

public class Clinical
{
        public int Id { get; set; }
        [Required]

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name should be between 2 and 100 characters.")]
        public string Name { get; set; }
        public List<Doctor>? Doctors { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

}
