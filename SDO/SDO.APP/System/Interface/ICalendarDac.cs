using SDO.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public interface ICalendarDac : IDac
    {
        Task Delete(int calendarId);
        Task Insert(CalendarModel calendar);
        Task<IList<CalendarModel>> Read(string userId, string orgId);
        Task<CalendarDetailModel> ReadById(int calendarId, string userId);
        Task Update(CalendarModel calendar);
    }
}