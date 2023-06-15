using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Playground.Shared.Services;
using Playground.Shared.Models;

namespace client_sandbox.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService service = new AppointmentService();

        [HttpGet]
        public IEnumerable<Appointment> Get()
        {
            return service.GetAppointments();
        }

        [HttpGet("StartTime")]
        public DateTime GetStartTime()
        {
            return service.GetStartTime();
        }
    }
}
