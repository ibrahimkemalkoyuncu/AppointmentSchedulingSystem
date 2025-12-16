using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentSchedulingSystem.Data;
using AppointmentSchedulingSystem.Models;

namespace AppointmentSchedulingSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetDoctorPerformance()
        {
            var data = await _context.Appointments
                .Include(a => a.Doctor)
                .GroupBy(a => a.Doctor.Name + " " + a.Doctor.Surname)
                .Select(g => new { DoctorName = g.Key, Count = g.Count() })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<JsonResult> GetClinicDistribution()
        {
            var data = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.Clinical)
                .GroupBy(a => a.Doctor.Clinical.Name)
                .Select(g => new { ClinicName = g.Key, Count = g.Count() })
                .ToListAsync();

            return Json(data);
        }
    }
}
