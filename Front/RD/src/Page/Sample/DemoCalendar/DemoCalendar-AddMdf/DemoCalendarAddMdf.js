import React, { useState, useEffect, useContext } from 'react';
import { IsNullOrEmpty, GetOrgValue, FormatDate } from '../../../../Basic/SDOExtension'
import { Button } from "@progress/kendo-react-buttons";
import { Error } from '@progress/kendo-react-labels';
import TextInput from '../../../../Components/Input/TextInput'
import TwDatePicker from '../../../../Components/DateInputs/TwDatePicker'
import OrgSelect from '../../../../Components/Selector/Orgselector'
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../../Components/Dialogs/ConfirmBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import DemoCalendarService from '../../DemoCalendar/democalendar.service'
import { RadioGroup } from "@progress/kendo-react-inputs";

const AddMdf = (props) => {
    //新增 or 修改
    const IsAdd = IsNullOrEmpty(props.calendarId) ? true : false
    const [IsReadOnly, setIsReadOnly] = useState(false)
    const [calendarType, setCalendarType] = useState([])
    const [calendarEventData, setCalendarEventData] = useState({
        CALENDAR_ID: Number(props.calendarId),
        CALENDAR_TITLE: "",
        CALENDAR_START_DATE: new Date(),
        CALENDAR_END_DATE: new Date(),
        CALENDAR_TYPE: "0",
        CALENDAR_ORG: [],
        CALENDAR_CONTENT: ""
    });

    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得行事曆狀態
        getCalendarType()
        setIsReadOnly(true)
        if (!IsAdd) {
            //是否有權限能夠編輯行事曆
            IsEditCalendarEvent(props.calendarId)
            //取得行事曆ByCalendarId
            getCalendarEventByCalendarId(props.calendarId)
        }
    }, []);

    //是否有權限能夠編輯行事曆
    const IsEditCalendarEvent = async (calendarId) => {
        setIsReadOnly(await DemoCalendarService.IsEditCalendarEvent(calendarId))
    }

    //取得行事曆狀態
    const getCalendarType = async () => {
        setCalendarType(await DemoCalendarService.getCalendarType())
    }

    //取得行事曆ByCalendarId
    const getCalendarEventByCalendarId = async (calendarId) => {
        setCalendarEventData(await DemoCalendarService.getCalendarEventByCalendarId(calendarId))
    }

    const isDelte = (calendarId) => {
        showConfirmBox("進行刪除作業，確定嗎？", () => { deleteCalendarEvent(calendarId) })
    }

    //刪除行事曆
    const deleteCalendarEvent = async (calendarId) => {
        let response = await DemoCalendarService.deleteCalendarEvent(calendarId)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                }
            }
        })
    }

    //存檔
    const submit = async (data) => {
        //資料處理
        let calendarEventData = { ...data }
        //取得Org object value
        calendarEventData.CALENDAR_ORG = GetOrgValue(calendarEventData.CALENDAR_ORG).toString()
        calendarEventData.CALENDAR_START_DATE = FormatDate(calendarEventData.CALENDAR_START_DATE, 'YYYY-MM-DD')
        calendarEventData.CALENDAR_END_DATE = FormatDate(calendarEventData.CALENDAR_END_DATE, 'YYYY-MM-DD')
        //新增or修改 行事曆
        let response;
        if (IsAdd) {
            response = await DemoCalendarService.insertCalendarEvent(calendarEventData)
        } else {
            response = await DemoCalendarService.updateCalendarEvent(calendarEventData)
        }
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                }
            }
        })
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        CALENDAR_TITLE: Yup.string()
            .required('標題必填'),
        CALENDAR_START_DATE: Yup.date()
            .required('起始時間必填').nullable(),
        CALENDAR_END_DATE: Yup.date()
            .required('結束時間必填').nullable()
            .min(Yup.ref('CALENDAR_START_DATE'), "結束時間不可早於起始時間"),
        CALENDAR_ORG: Yup.string()
            .required('公告部門必填'),
        CALENDAR_CONTENT: Yup.string()
            .required('內容必填')
    });

    return (
        <WindowBox title={IsReadOnly ? props.title : "檢視行事曆"} onClose={props.closeWindow}>
            <Formik
                initialValues={calendarEventData}
                validationSchema={validateField}
                onSubmit={(data) => submit(data)}
                //允許重複賦予初始值
                enableReinitialize
            >
                {props => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleSubmit,
                        handleReset,
                        handleChange,
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit} onReset={handleReset}>
                            <div className="fn-buttons">
                                {IsReadOnly &&
                                    <>
                                        <Button type="submit">存檔</Button>
                                        <Button type="reset">重設</Button>
                                        {!IsAdd && <Button type="button" onClick={() => isDelte(values.CALENDAR_ID)}>刪除</Button>}
                                    </>
                                }

                            </div>
                            <table>
                                <tbody>
                                    <tr>
                                        <th>標題</th>
                                        <td>
                                            <TextInput
                                                name="CALENDAR_TITLE"
                                                value={values.CALENDAR_TITLE}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.CALENDAR_TITLE}
                                            />
                                        </td>
                                    </tr>
                                    <tr >
                                        <th>起始時間</th>
                                        <td >
                                            <TwDatePicker
                                                name="CALENDAR_START_DATE"
                                                format={"yyy/MM/dd"}
                                                onChange={handleChange}
                                                value={values.CALENDAR_START_DATE}
                                                error={errors.CALENDAR_START_DATE}
                                            />
                                        </td>
                                    </tr>
                                    <tr >
                                        <th>結束時間</th>
                                        <td >
                                            <TwDatePicker
                                                name="CALENDAR_END_DATE"
                                                format={"yyy/MM/dd"}
                                                onChange={handleChange}
                                                value={values.CALENDAR_END_DATE}
                                                error={errors.CALENDAR_END_DATE}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>狀態</th>
                                        <td>
                                            <RadioGroup
                                                name='CALENDAR_TYPE'
                                                data={calendarType}
                                                layout={"horizontal"}
                                                value={values.CALENDAR_TYPE}
                                                onChange={(e) => setValues({ ...values, CALENDAR_TYPE: e.value })}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>公告部門</th>
                                        <td >
                                            <OrgSelect
                                                name="CALENDAR_ORG"
                                                multiple={false}
                                                value={values.CALENDAR_ORG}
                                                error={errors.CALENDAR_ORG}
                                                setValue={(data) => setValues({ ...values, CALENDAR_ORG: data })} // data 為object ，再"送出"的時候再取得value
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>內容</th>
                                        <td >
                                            <textarea
                                                className="k-textarea"
                                                name="CALENDAR_CONTENT"
                                                value={values.CALENDAR_CONTENT}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                            />
                                            {<Error>{errors.CALENDAR_CONTENT}</Error>}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </WindowBox>
    )
}

export default AddMdf;