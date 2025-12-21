import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from "@fullcalendar/interaction";
import { Checkbox } from '@progress/kendo-react-inputs';
import { IsNullOrEmpty, SetMaskOnOff, FormatDate } from '../../../Basic/SDOExtension';
import { SetCodeService } from './SetCodeService';
import { showGlobalConfirmBox, showGlobalMessageBox } from "../../../Route/RootMiddleware";
import '../../../Css/custom/FullCalendar.main.css';
export const WorkingDay = () => {
    // 查詢行事曆日期區間
    const [queryDate, setQueryDate] = React.useState({
        startDate: "",
        endDate: ""
    });
    // 顯示於行事曆的工作日資料
    const [calendarData, setCalendarData] = React.useState([]);
    // 紀錄異動資料
    const editedData = React.useRef([]);
    const [isDataChange, setIsDataChange] = React.useState(false);
    const calendarRef = React.useRef(null);
    // 是否有明年工作日
    const [haveNextYear, setHaveNextYear] = React.useState(false);

    /**
     * 載入工作日資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await SetCodeService.loadWorkingDay(queryDate.startDate, queryDate.endDate);

        let calendarEvents = [];
        data.forEach(x => {
            let tempData = {
                id: FormatDate(x.DATE, 'YYYY-MM-DD'),
                title: "工作日",
                date: FormatDate(x.DATE, 'YYYY-MM-DD'),
                check: x.IS_WORKING,
            }
            calendarEvents.push(tempData);
        });
        setCalendarData([...calendarEvents]);
        editedData.current = [];
        setIsDataChange(false);
        SetMaskOnOff(false);
    }

    /**
     * 確認年度工作日
     */
    const checkYear = async () => {
        SetMaskOnOff(true);
        // 若無今年工作日載入頁面時產生
        let count = await SetCodeService.loadWorkingDayCountByYear(new Date().getFullYear());
        if (count == 0) {
            await SetCodeService.generateWorkingDay(new Date().getFullYear());
        }
        // 判斷有無明年工作日，控制按鈕是否可點
        let nextYear = await SetCodeService.loadWorkingDayCountByYear(new Date().getFullYear() + 1);
        if (nextYear == 0) {
            setHaveNextYear(false);
        }
        else {
            setHaveNextYear(true);
        }
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        checkYear();
    }, [])

    React.useEffect(() => {
        if (!IsNullOrEmpty(queryDate.startDate)) {
            if (isDataChange) {
                showGlobalConfirmBox("資料有異動，是否要存檔", () => {
                    save();
                }, () => {
                    loadData();
                })
            }
            else {
                loadData();
            }
        }
    }, [queryDate])

    /**
     * 產生明年度工作日
     */
    const generateWorkingDay = async () => {
        SetMaskOnOff(true);
        let saveResult = await SetCodeService.generateWorkingDay(new Date().getFullYear() + 1);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => loadData());
            setHaveNextYear(true);
        }
    }

    /**
     * 存檔
     */
    const save = async () => {
        SetMaskOnOff(true);
        let saveResult = await SetCodeService.saveWorkingDay(editedData.current);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => loadData());
        }
    }

    /**
    * 設定行事曆內格式
    */
    const checkBoxContent = (e) => {
        let data = e.event._def;
        return (
            <>
                <Checkbox
                    id={data.publicId}
                    label={data.title}
                    onClick={(e) => {
                        setSelectDataItem(data);
                    }}
                    checked={data.extendedProps.check}
                />
                <span>{data.extendedProps.name}</span>
            </>
        )
    }

    /**
    * 選取資料
    */
    const setSelectDataItem = (prop) => {
        let setSchedule = calendarData;
        setSchedule.some(function (item) {
            if (item.id === prop.publicId) {
                item.check = (item.check) ? false : true;
                // 紀錄異動資料
                if (editedData.current.some(ei => ei.DATE === prop.publicId)) {
                    let newItems = editedData.current.map(x => x.DATE === prop.publicId ? { DATE: prop.publicId, IS_WORKING: item.check } : x);
                    editedData.current = newItems;
                }
                else {
                    editedData.current.push({ DATE: prop.publicId, IS_WORKING: item.check });
                }
                return true;
            }
            return false;
        });
        setCalendarData([...calendarData]);
        setIsDataChange(true);
    }

    return (
        <>
            <PageContainer style={{
                height: '100%',
                overflow: 'auto'
            }}
                toolbar={
                    <>
                        <div style={{ width: "700px" }}>
                            <Button title="存檔" onClick={save} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={loadData} >取消</Button>
                            <Button title="產生明年工作日" onClick={generateWorkingDay} disabled={haveNextYear}>產生明年工作日</Button>

                        </div>
                        <h3>如欲改變填報週期，請於填報週期開始前完成設定，才可生效。（於填報週期內改變設定，無法生效）</h3>
                    </>
                }
            >
                <FullCalendar
                    plugins={[dayGridPlugin, interactionPlugin]}
                    buttonText={{
                        today: '今天'
                    }}
                    headerToolbar={{
                        left: "prevYear,prev,next,nextYear",
                        center: "title",
                        right: "today",
                    }}
                    ref={calendarRef}
                    initialView="dayGridMonth"
                    locale='zh-tw'
                    views={
                        {
                            dayGridMonth: {
                                titleFormat: function (date) {
                                    return FormatDate(date.date.marker, 'tYY年MM月');
                                }
                            }
                        }
                    }
                    contentHeight={300}
                    events={calendarData}
                    eventContent={checkBoxContent}
                    datesSet={(dateInfo) => {
                        let startDate = FormatDate(dateInfo.start, 'YYYY-MM-DD');
                        let endDate = FormatDate(dateInfo.end, 'YYYY-MM-DD');
                        setQueryDate({ startDate: startDate, endDate: endDate });
                    }}
                />
            </PageContainer>
        </>
    )
}
export default WorkingDay;