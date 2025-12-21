import React, { useState, useEffect, useRef } from "react";
import { DatePicker, Calendar, CalendarHeaderTitle, CalendarViewEnum, CalendarCell, CalendarNavigationItem, DateInput, ToggleButton, DateTimePicker, MultiViewCalendar } from '@progress/kendo-react-dateinputs';
import { Error } from '@progress/kendo-react-labels';
import { FormatDate, IsNullOrEmpty, twDateRegex } from "../../Basic/SDOExtension";
import TextInput from "../Input/TextInput";
import { Reveal } from '@progress/kendo-react-animation';
import { ButtonGroup } from "@progress/kendo-react-buttons";
import { TimeSelector } from "@progress/kendo-react-dateinputs/dist/npm/timepicker/TimeSelector";
const TwDatePicker = props => {

    /*const [timepick, setTimepick] = useState({
        visible: false,
        top: '0px',
        left: '0px'
    });
  
    const setTimepickState = (timepick) => {
        setTimepick(timepick);
    }

    const ToggleButtonGroup = prop => {
        return <ButtonGroup>
            <ToggleButton {...prop} onClick={() => { prop.onClick(); setTimepickState({ visible: false }); }}>
                <span className="k-icon k-i-calendar"></span>
            </ToggleButton>
            {props.timepick && <ToggleButton
                onClick={(e) => {
                    let dom = document.getElementById(props.id).getClientRects()[0];
                    setTimepickState({
                        visible: !timepick.visible,
                        top: timepick.visible ? timepick.top : (dom.y + dom.height) + 'px',
                        left: timepick.visible ? timepick.left : (dom.x) + 'px'
                    });
                }}
            >
                <span className="k-icon k-i-clock" title="選擇時間"></span>
            </ToggleButton>}
        </ButtonGroup>
    }*/

    return (
        <div>
            {/* 直接依照是否有開timepick去切換元件就好 */}
            {
                props.timepick ?
                    <DateTimePicker
                        id={props.id}
                        {...props}
                        min={props.min ?? new Date('1911-01-01')}
                        format={props.format ?? 'yyy/MM/dd HH:mm:ss'}
                        calendar={TwCalendar}
                        dateInput={TwDateInput}
                        disabled={props.disabled}
                    />
                    :
                    <DatePicker
                        id={props.id}
                        {...props}
                        min={props.min ?? new Date('1911-01-01')}
                        format={props.format ?? 'yyy/MM/dd'}
                        calendar={TwCalendar}
                        dateInput={TwDateInput}
                        disabled={props.disabled}
                    />
            }
            <Error>{props.error}</Error>
            {/*<DatePicker
                id={props.id}
                {...props}
                min={props.min ?? new Date('1911-01-01')}
                format={props.format ?? (props.timepick ? 'yyy/MM/dd HH:mm:ss' : 'yyy/MM/dd')}
                calendar={TwCalendar}
                dateInput={TwDateInput}
                toggleButton={ToggleButtonGroup}
                disabled={props.disabled}
            />
            <Error>{props.error}</Error>
            <Reveal style={{ position: 'fixed', top: timepick.top, left: timepick.left }}>
                {timepick.visible ?
                    <div className="k-animation-container k-animation-container-relative k-group k-reset k-animation-container-shown">
                        <div className="k-popup k-child-animation-container">
                            <TimeSelector
                                value={props.value}
                                cancelButton={false}
                                format='HH:mm:ss'
                                onChange={(e) => {
                                    //避免觸發form submit
                                    e.syntheticEvent.preventDefault()
                                    props.onChange(e);
                                    setTimepickState({ visible: false });
                                }}
                            ></TimeSelector>
                        </div>
                    </div> : null}
            </Reveal>*/}
        </div >
    );
}

const TwDateInput = props => {
    let [twDate, setTwDate] = useState(null);

    //西元轉民國日期: Date        e.g. 2020-08-08 => 109-08-08 
    const toTwDate = () => {
        if (!isNaN(Date.parse(props.value))) {
            return FormatDate(props.value, props.format.replace("yyy", "tYY").replace("dd", "DD"));
        } else {
            return null;
        }
    }

    //民國轉西元日期: Date
    const fromTwDate = (date) => {
        let newDate = date.split(/[-/]/);
        newDate[0] = (parseInt(newDate[0]) + 1911).toString();
        return new Date(newDate.join('-'));
    }

    useEffect(() => {
        setTwDate(props.value ? toTwDate() : null);
    }, [props])

    return (
        <TextInput
            value={twDate ?? ''}
            onChange={(e) => {
                if (twDateRegex.test(e.value) && !isNaN(Date.parse(e.value))) {
                    e.value = fromTwDate(e.value)
                    props.onChange(e);
                } else if (IsNullOrEmpty(e.value)) {
                    e.value = null
                    props.onChange(e);
                }
                setTwDate(e.value)
            }}
            style={{ width: "100%" }}
        />
    )
}

const TwCalendar = props => {
    if (isNaN(Date.parse(props.value)))
        props = { ...props, value: null };

    return (
        <MultiViewCalendar
            {...props}
            views={1}
            headerTitle={TwCalendarHeaderTitle}
            cell={TwCalendarCell}
        />
    );
}

const TwCalendarNavigationItem = props => {
    let children = props.children;
    if (children.length > 3) children -= 1911;
    return (
        <CalendarNavigationItem {...props} children={children} />
    )
}

const TwCalendarHeaderTitle = props => {
    let value;
    // 日期選擇view
    if (props.view === CalendarViewEnum.month) {
        value = `民國 ${parseInt(props.value.split(' ')[1]) - 1911} 年 ${props.value.split(' ')[0]}`;
    }
    // 月份選擇view
    else if (props.view === CalendarViewEnum.year) {
        value = `民國 ${parseInt(props.value) - 1911} 年`;
    }
    // 年份/年代選擇view
    else {
        value = `民國 ${parseInt(props.value.split(' ')[0]) - 1911} ${props.value.split(' ')[1]} ${parseInt(props.value.split(' ')[2]) - 1911} 年`;
    }
    return (
        <CalendarHeaderTitle
            {...props}
            children={value}
            value={value}
        />
    )
}

const TwCalendarCell = props => {
    let child = (
        props.view === CalendarViewEnum.decade ||
            props.view === CalendarViewEnum.century ?
            props.children - 1911 :
            props.children
    );
    return (
        <CalendarCell
            {...props}
            children={child}
        />
    )
}


export default TwDatePicker;