import React, { useState, useRef, useEffect } from "react";
import { Formik, Form, Field } from 'formik';
import { FormatDate } from "../../../Basic/SDOExtension";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { Button } from '@progress/kendo-react-buttons';
import TextInput from '../../../Components/Input/TextInput';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import WindowBox from '../../../Components/Dialogs/WindowBox';

/**
 * 研究發展作業系統-委託研究計劃-執行情形管理
 * 執行情形管理
 * PPT P14
 * 計畫登入"編修"的資料跳出視窗
 * 下拉選和一些資料寫入測試 
 */

const ProjectExceManagerWindow = (props) => {
    //下拉選單資料
    const [dropDownData, setDropDownData] = useState([
        { text: '符合', value: 'A' },
        { text: '超前', value: 'B' },
        { text: '落後', value: 'C' },
    ]);
    //外部傳進setwin的資料
    const { closeWindow, windowData } = props;
    // 這裡假設grid資料從外部加載
    const [data, setData] = useState([]);
    //表單異動資料
    const formRef = useRef();
    //表單資料
    const [formData, setFormData] = useState({});

    // 表單初始化資料
    useEffect(() => {
        if(windowData){
            setFormData(windowData);
        }
    }, [windowData]);

    // 存檔
    const saveChanges = async (data) => {

        closeWindow();
    }

    // 表單取消
    const cancel = (event) => {
        event.preventDefault();
        setFormData({});
    };

    // 表單送出審核
    const check = (values) => {

        closeWindow();
    };

    // 表單檔案上傳
    const upload = (values) => {
        
    }; 

    // 日期 change 事件 (轉換格式)
    const onFormDatePickerChange = (e, setValues, values) => {
        let newObj = {};
        newObj[e.target.name] = e.target.value == null
            ? null
            : FormatDate(e.target.value, "YYYY-MM-DD");
        setValues({ ...values, ...newObj });
    }

        return (
            <WindowBox
            width={60}
            height={70}
            onClose={closeWindow}
            title={"編輯執行情形"}
            >
                    <Formik
                        initialValues={formData}
                        onSubmit={(data) => saveChanges(data)}
                        enableReinitialize={true}
                        innerRef={formRef}
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleChange,
                                handleSubmit,
                                setValues
                            } = props;
                            return (
                                <form>
                                    <div>
                                    <Button title="存檔" onClick={handleSubmit}>存檔</Button>
                                    <Button title="送審" onClick={check}>送審</Button>
                                    <Button title="取消" onClick={cancel}>取消</Button>
                                    </div>
                                    <table>
                                            <tr>
                                                <th>
                                                    計畫編號
                                                </th>
                                                <td colSpan={3}>
                                                    {values.PLAN_NO}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    計畫名稱
                                                </th>
                                                <td >
                                                    {values.PLAN_NAME}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    年度
                                                </th>
                                                <td >
                                                    {values.PLAN_YEAY}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    季度
                                                </th>
                                                <td >
                                                    {values.PLAN_QQ}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    委託單位
                                                </th>
                                                <td >
                                                    {values.REQUEST_UNIT}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    研究主持人
                                                </th>
                                                <td >
                                                    {values.REA_MASTER}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    評核指標
                                                </th>
                                                <td >
                                                    {values.PROJECT_NAME}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    執行情形簡述
                                                </th>
                                                <td >
                                                    <TextInput
                                                        name="EXE_SITUATION"
                                                        value={values.EXE_SITUATION}
                                                        onChange={handleChange}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    執行進度
                                                </th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="EXP_PRO"
                                                        data={dropDownData}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.EXP_PRO}
                                                        onChange={(e) => { setValues({ ...values, PROJECT: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    落後原因
                                                </th>
                                                <td>
                                                <PureHtmlTextAreaInput
                                                    rows={5}
                                                    name='EXP_RESULT'
                                                    value={values.EXP_RESULT}
                                                    error={errors.EXP_RESULT}
                                                    onChange={handleChange}
                                                    style={{ width: "100%" }}
                                                />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    解決對策
                                                </th>
                                                <td>
                                                <PureHtmlTextAreaInput
                                                    rows={5}
                                                    name='SOLUTION'
                                                    value={values.SOLUTION}
                                                    error={errors.SOLUTION}
                                                    onChange={handleChange}
                                                    style={{ width: "100%" }}
                                                />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    期中報告提出日
                                                </th>
                                                <td style={{ display: 'flex', alignItems: 'center' }}>
                                                    <TwDatePicker
                                                    name="CEN_DATE"
                                                    format="yyy/MM/dd"
                                                    onChange={(e) => {
                                                        onFormDatePickerChange(e, setValues, values);
                                                    }}
                                                    error={errors.CEN_DATE}
                                                    />
                                                    <Button title="檔案上傳" onClick={upload}>檔案上傳</Button>
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    期末報告提出日
                                                </th>
                                                <td style={{ display: 'flex', alignItems: 'center' }}>
                                                    <TwDatePicker
                                                    name="LAST_DATE"
                                                    format="yyy/MM/dd"
                                                    onChange={(e) => {
                                                        onFormDatePickerChange(e, setValues, values);
                                                    }}
                                                    error={errors.LAST_DATE}
                                                    />
                                                <Button title="檔案上傳" onClick={upload}>檔案上傳</Button>
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>
                                                    決標日
                                                </th>
                                                <td style={{ display: 'flex', alignItems: 'center' }}>
                                                    <TwDatePicker
                                                    name="NEW_DATE"
                                                    format="yyy/MM/dd"
                                                    onChange={(e) => {
                                                        onFormDatePickerChange(e, setValues, values);
                                                    }}
                                                    error={errors.NEW_DATE}
                                                    />
                                                <Button title="檔案上傳" onClick={upload}>檔案上傳</Button>
                                                </td>
                                            </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                    
        </WindowBox>
        )
        }
        export default ProjectExceManagerWindow;