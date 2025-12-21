import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const calendarUrl = getGlobalServerConfig().backEndUrl.get() + 'Calendar';

//是否有權限能夠編輯行事曆
const IsEditCalendarEvent = async (calendarId) => {
    let url = calendarUrl + '/Editable/' + calendarId
    let response = await api.Get(url)
    if (response.ok) {
        return await response.json();
    }
}

//取得行事曆
const getCalendarEvents = async () => {
    let response = await api.Get(calendarUrl)
    let calendarEvents = []
    if (response.ok) {
        let data = await response.json()
        data.forEach(item => {
            let color;
            //將取得的每個event根據CALENDAR_TYPE設置顏色
            switch (item.CALENDAR_TYPE) {
                case '0'://公開 紅色
                    color = '#FF8888';
                    break;
                case '1'://私人 藍色
                    color = '#77DDFF';
                    break;
                case '2'://部門 綠色
                    color = '#DDFF77';
                    break;
            }
            //設置各個event應有的屬性資料
            calendarEvents.push({
                id: item.CALENDAR_ID,
                title: item.CALENDAR_TITLE,
                start: new Date(item.CALENDAR_START_DATE.substring(0,11)+"09:00:00"),
                end: new Date(item.CALENDAR_START_DATE.substring(0,11)+"12:00:00"),
                allDay: false,
                color: color,
                isAM: true,
                checked:false,
            })
            calendarEvents.push({
                id: item.CALENDAR_ID,
                title: item.CALENDAR_TITLE,
                start: new Date(item.CALENDAR_START_DATE.substring(0,11)+"12:00:00"),
                end: new Date(item.CALENDAR_START_DATE.substring(0,11)+"15:00:00"),
                allDay: false,
                color: color,
                isAM: false,
                checked:false,
            })
        })
    }

    return calendarEvents
}

//取得行事曆狀態
const getCalendarType = async () => {
    let response = await api.Read(getGlobalServerConfig().backEndUrl.get() + 'SetParam/CalendarType');
    let calendarType = []
    if (response.ok) {
        let data = await response.json();
        data.forEach(item => {
            calendarType.push({ label: item.SET_VALUE, value: item.SET_TYPE })
        })
    }

    return calendarType;
}

//取得行事曆ByCalendarId
const getCalendarEventByCalendarId = async (calendarId) => {
    let calendarEvent = {
        CALENDAR_ID: "",
        CALENDAR_TITLE: "",
        CALENDAR_START_DATE: new Date(),
        CALENDAR_END_DATE: new Date(),
        CALENDAR_TYPE: "",
        CALENDAR_ORG: "",
        CALENDAR_CONTENT: ""
    }
    let url = calendarUrl + '/' + calendarId
    let response = await api.Get(url)
    if (response.ok) {
        let data = await response.json()
        calendarEvent = {
            CALENDAR_ID: data.CALENDAR_ID,
            CALENDAR_TITLE: data.CALENDAR_TITLE,
            CALENDAR_START_DATE: new Date(data.CALENDAR_START_DATE),
            CALENDAR_END_DATE: new Date(data.CALENDAR_END_DATE),
            CALENDAR_TYPE: data.CALENDAR_TYPE,
            // @ts-ignore
            CALENDAR_ORG: [{ ORG_DISPLAY: data.ORG_NAME, ORG_ID: data.CALENDAR_ORG }],
            CALENDAR_CONTENT: data.CALENDAR_CONTENT
        }
    }
    
    return calendarEvent
}

//新增行事曆
const insertCalendarEvent = async (calendarEventData) => {
    let response = await api.Post(calendarUrl, JSON.stringify(calendarEventData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//修改行事曆
const updateCalendarEvent = async (calendarEventData) => {
    let response = await api.Put(calendarUrl, JSON.stringify(calendarEventData))

    return {
        ok: response.ok,
        result: await response.json()
    };
}

//刪除行事曆 
const deleteCalendarEvent = async (calendarId) => {
    let url = calendarUrl + '/' + calendarId
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    }
}

const DemoCalendarService = {
    IsEditCalendarEvent: IsEditCalendarEvent,
    getCalendarEvents: getCalendarEvents,
    getCalendarType: getCalendarType,
    getCalendarEventByCalendarId: getCalendarEventByCalendarId,
    insertCalendarEvent: insertCalendarEvent,
    updateCalendarEvent: updateCalendarEvent,
    deleteCalendarEvent: deleteCalendarEvent
}

export default DemoCalendarService;