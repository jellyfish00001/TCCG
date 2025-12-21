using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Services
{
    public interface ICalendarService
    {
        Task<RtnResultModel> Create(CalendarModel calendar);
        Task<RtnResultModel> Delete(int calendarId);
        Task<IList<CalendarModel>> Read();
        Task<CalendarDetailModel> ReadById(int calendarId);
        Task<bool> Editable(int calendarId);
        Task<RtnResultModel> Update(CalendarModel calendar);
    }
}