//@ts-check
import React, { Fragment, useContext, useEffect, useState } from 'react';
import FullCalendar from '@fullcalendar/react'
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from "@fullcalendar/interaction";
import DemoCalendarService from '../DemoCalendar/democalendar.service'
import AddMdf from '../DemoCalendar/DemoCalendar-AddMdf/DemoCalendarAddMdf'
import { FormatDate } from '../../../Basic/SDOExtension'
import { Checkbox } from '@progress/kendo-react-inputs';
import TextInput from '../../../Components/Input/TextInput';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';

const DemoCalendar = () => {
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, calendarId: "", title: "" })
    const { showConfirmBox } = useContext(ConfirmBoxContext);

    const [meetData,setMeetData] = useState([]);

    useEffect(() => {
        getCalendarEvents();
    },[]);
    
    //取得行事曆
    const getCalendarEvents = async () => {
        setMeetData(await DemoCalendarService.getCalendarEvents())
        return meetData
    }

    const eventClick = (e) => {
        //setIsAddMdf({ visible: true, calendarId: e.event.id, title: "修改" })
    }

    const eventInput = (e) => {
    }

    const rvwCheckBox = (e) =>{
        let data = e.event._def;
        return(
        <Fragment>
            <Checkbox
                id={data.publicId}
                //name="test"
                //onChange={(e) => setValues({ ...values, SET_DECISION: e.value })}
                label={data.extendedProps.isAM?"上午":"下午"}
                onClick={(e)=>{
                    showConfirmBox("確認是否參加會議?",() => {
                        var result = [];
                        meetData.forEach((v,i)=>{
                        })
                    })            
                }}
                checked={data.checked}
            />
            <TextInput
            style={{width:"100%"}}
            id={data.publicId}
            onChange={eventInput}
            ></TextInput>
        </Fragment>
        )
    }

    return (
        <>
            <FullCalendar
                plugins={[dayGridPlugin, interactionPlugin]}
                buttonText={{
                    today: '今天'
                }}
                headerToolbar={{
                    left: "prev,next",
                    center: "title",
                    right: "today,addCalendarEvent",
                }}
                customButtons={{
                    addCalendarEvent: {
                        text: '新增行事曆',
                        click: function () {
                            setIsAddMdf({ visible: true, calendarId: '', title: "新增" })
                        }
                    }
                }}
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
                events={meetData}
                eventTextColor={'#000000'}
                //eventClick={eventClick}
                eventContent={rvwCheckBox}
            />
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({...isAddMdf,visible:false})}
                calendarId={isAddMdf.calendarId} />}
        </>
    );
}

export default DemoCalendar;
