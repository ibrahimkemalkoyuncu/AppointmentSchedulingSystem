using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentSchedulingSystem.Data;
using AppointmentSchedulingSystem.Models;

namespace AppointmentSchedulingSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Dashboard Metrics
        ViewBag.TotalPatients = await _context.Patients.CountAsync();
        ViewBag.TotalDoctors = await _context.Doctors.CountAsync();

        var today = DateTime.Today;
        ViewBag.TodaysAppointments = await _context.Appointments
            .Where(a => a.AppointmentDate.Date == today)
            .CountAsync();

        // Next 5 Appointments
        var upcomingAppointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Where(a => a.AppointmentDate >= DateTime.Now && a.Status == "Beklemede")
            .OrderBy(a => a.AppointmentDate)
            .Take(5)
            .ToListAsync();

        return View(upcomingAppointments);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    
}
