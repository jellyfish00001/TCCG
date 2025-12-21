using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDO.Models;
using SDO.Services;

namespace SDO.Controllers
{
    [Authorize(Roles = "handUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService calendarService;
        public CalendarController(ICalendarService calendarService)
        {
            this.calendarService = calendarService;
        }
        
        [HttpGet]
        public async Task<IList<CalendarModel>> Read()
        {
            return await calendarService.Read();
        }

        [HttpGet("{calendarId}")]
        public async Task<CalendarDetailModel> ReadById(int calendarId)
        {
            return await calendarService.ReadById(calendarId);
        }

        //是否能夠編輯
        [HttpGet("[action]/{calendarId}")]
        public async Task<bool> Editable(int calendarId)
        {
            return await calendarService.Editable(calendarId);
        }

        [HttpPost]
        public async Task<RtnResultModel> Create(CalendarModel calendar)
        {
            return await calendarService.Create(calendar);
        }

        [HttpPut]
        public async Task<RtnResultModel> Update(CalendarModel calendar)
        {
            return await calendarService.Update(calendar);
        }

        [HttpDelete("{calendarId}")]
        public async Task<RtnResultModel> Delete(int calendarId)
        {
            return await calendarService.Delete(calendarId);
        }
    }
}