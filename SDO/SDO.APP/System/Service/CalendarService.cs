using SDO.Dac;
using SDO.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDO.Utils;

namespace SDO.Services
{
    public class CalendarService : Service, ICalendarService
    {
        private readonly ICalendarDac dac;
        private readonly IUserProfile userProfile;
        private readonly IEmpOrgDac empOrgDac;

        public CalendarService(ICalendarDac dac, IUserProfile userProfile, IEmpOrgDac empOrgDac)
        {
            this.dac = dac;
            this.userProfile = userProfile;
            this.empOrgDac = empOrgDac;
        }

        public async Task<IList<CalendarModel>> Read()
        {
            return await dac.Read(
                    userProfile.GetLoginUser().USER_ID,
                    (await empOrgDac.ReadByUserId(userProfile.GetLoginUser().USER_ID))?.ORG_ID
                );
        }

        public async Task<CalendarDetailModel> ReadById(int calendarId)
        {
            return await dac.ReadById(calendarId, userProfile.GetLoginUser().USER_ID);
        }

        public async Task<RtnResultModel> Create(CalendarModel calendar)
        {
            await dac.Insert(calendar);
            return ChangeResult(ResultType.Success | ResultType.Insert);
        }

        public async Task<RtnResultModel> Update(CalendarModel calendar)
        {
            if (!await Editable(calendar.CALENDAR_ID))
                return ChangeResult(false, i18N.Message.R17);
            await dac.Update(calendar);
            return ChangeResult(ResultType.Success | ResultType.Update);
        }

        public async Task<RtnResultModel> Delete(int calendarId)
        {
            if (!await Editable(calendarId))
                return ChangeResult(false, i18N.Message.R17);

            await dac.Delete(calendarId);
            return ChangeResult(ResultType.Success | ResultType.Delete);
        }

        public async Task<bool> Editable(int calendarId)
        {
            return (await ReadById(calendarId))?.Editable ?? false;
        }
    }
}
