import React, { useState, useRef, useEffect } from 'react';
import { Formik } from 'formik';
import Table from '../../../Css/custom/Table.module.css';
import { initQueryData, getAllDropDowns, projAwStatusDefaultList, getRunWayC } from "./ProjectAdjustListExecService";
import { PageContainer } from '../../../Basic/PageContainer';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { CascadeDropDown } from '../../../Components/Dropdowns/CascadeDropDown';
import { Button } from '@progress/kendo-react-buttons';
import { MultiSelect } from '@progress/kendo-react-dropdowns';

export const ProjectAdjustListExecQueryForm = props => {
    const { loadData, paging, setPaging, visible } = props;

    // 預設「調整撤銷狀態」值
    const defaultProjectAwStatusVal = useRef([]);
    // 清除次數
    const refreshCnt = useRef(0);
    //紀錄查詢資料
    const [queryData, setQueryData] = useState({
        ...initQueryData,
        refreshCnt: refreshCnt.current
    })
    // 查詢條件下拉式選單
    const [ddlData, setDdlData] = useState({
        PROJECT_AW_STATUS: [],
        PROJECT_YEAR: [],
        CP_KIND: [],
        SPEC_NOTE: []
    });

    useEffect(() => {
        loadFormData();
    }, []);

    /**
     * 載入資料
     */
    const loadFormData = async () => {
        SetMaskOnOff(true);
        //取得所有下拉清單資料
        let dropDownDatas = await getAllDropDowns();
        if (dropDownDatas) {
            await initForm(dropDownDatas);
            await loadData({ ...initQueryData, PROJECT_AW_STATUS: projAwStatusDefaultList });
        }
        SetMaskOnOff(false);
    }

    // 初始Form
    const initForm = async (dropDownDatas) => {
        // 載入下拉選單資料
        let dropDownData = {
            PROJECT_AW_STATUS: dropDownDatas[0],
            PROJECT_YEAR: dropDownDatas[3],
            CP_KIND: dropDownDatas[1],
            SPEC_NOTE: dropDownDatas[2],
        };
        setDdlData(dropDownData);

        // 設定調整撤銷狀態預設值
        let defaultList = dropDownDatas[0];
        if (dropDownDatas[0].length > 0) {
            defaultList = dropDownDatas[0].filter(x => projAwStatusDefaultList.indexOf(x.value) != -1);
            defaultProjectAwStatusVal.current = defaultList;
        }

        // 設定form預設值
        let defaultValue = {
            ...initQueryData,
            PROJECT_AW_STATUS: defaultList,
            CP_KIND: dropDownDatas[1][0] ? dropDownDatas[1][0].value : '',
            SPEC_NOTE: dropDownDatas[2][0] ? dropDownDatas[2][0].value : '',
        };
        setQueryData(defaultValue);
    }

    //查詢送出
    const onSubmit = async (data) => {
        // 查詢前將計畫狀態調整為API格式
        let projectAwStatus = data.PROJECT_AW_STATUS;
        loadData({ ...data, PROJECT_AW_STATUS: projectAwStatus.map(x => x.value) });
        // 設置 grid 頁碼回第一頁
        setPaging({ ...paging, skip: 0 });
    }

    //清除
    const clearQueryConditions = () => {
        refreshCnt.current += 1;
        setQueryData({
            ...initQueryData,
            PROJECT_AW_STATUS: defaultProjectAwStatusVal.current,
            refreshCnt: refreshCnt.current
        })
    }

    //點選後傳回外層點選的類型
    return (
        <React.Fragment>
            {
                visible &&
                <PageContainer>
                    <Formik
                        initialValues={queryData}
                        onSubmit={(data) => onSubmit(data)}
                        enableReinitialize
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleSubmit,
                                handleChange,
                            } = props;
                            return (
                                <form onSubmit={handleSubmit}>
                                    <div className="fn-buttons">
                                        <Button className='k-button-lighten' type="submit">查詢</Button>
                                        <Button className='k-button-lighten' type="button" onClick={clearQueryConditions}>清除</Button>
                                    </div>
                                    <table className={Table.fullWidth}>
                                        <tbody>
                                            <tr>
                                                <th >計畫年度</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        data={ddlData.PROJECT_YEAR}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.PROJECT_YEAR == 0 ? "" : values.PROJECT_YEAR}
                                                        onChange={(e) => {
                                                            setQueryData({
                                                                ...queryData,
                                                                PROJECT_YEAR: e.value.value == "" ? 0 : e.value.value
                                                            })
                                                        }}
                                                    />
                                                </td>
                                                <th>計畫名稱</th>
                                                <td width="40%">
                                                    <TextInput
                                                        name="PROJECT_NAME"
                                                        value={values.PROJECT_NAME}
                                                        onChange={handleChange}
                                                        error={errors.PROJECT_NAME}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PROJECT_NAME: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>調整撤銷狀態</th>
                                                <td>
                                                    <MultiSelect
                                                        popupSettings={
                                                            { className: "dropdown-text-size" }
                                                        }
                                                        data={ddlData.PROJECT_AW_STATUS}
                                                        textField="text"
                                                        dataItemKey="value"
                                                        onChange={(e) => {
                                                            // @ts-ignore
                                                            setQueryData({ ...queryData, PROJECT_AW_STATUS: e.target.value })
                                                        }}
                                                        value={values.PROJECT_AW_STATUS}
                                                        placeholder={"請選擇"}
                                                    />
                                                </td>
                                                <th>計畫編號</th>
                                                <td>
                                                    <TextInput
                                                        name="PROJECT_NO"
                                                        value={values.PROJECT_NO}
                                                        onChange={handleChange}
                                                        error={errors.PROJECT_NO}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PROJECT_NO: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>執行方式</th>
                                                <td>
                                                    <CascadeDropDown
                                                        fristDdlData={ddlData.CP_KIND}
                                                        firstDdlValue={values.CP_KIND}
                                                        firstDdlError={errors.CP_KIND}
                                                        firstColumn={"CP_KIND"}
                                                        secondDdlInitData={[{ text: "請選擇", value: "" }]}
                                                        secondDdlValue={values.RUNWAY_C}
                                                        secondDdlError={errors.RUNWAY_C}
                                                        secondColumn={"RUNWAY_C"}
                                                        secondDdlStyle={{ width: '250px' }}
                                                        getSecondDdlData={getRunWayC}
                                                        values={values}
                                                        setValues={(data) => {
                                                            setQueryData({ ...queryData, CP_KIND: data.CP_KIND, RUNWAY_C: data.RUNWAY_C })
                                                        }}
                                                    />
                                                </td>
                                                <th>特殊加註</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        data={ddlData.SPEC_NOTE}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.SPEC_NOTE}
                                                        onChange={(e) => { setQueryData({ ...queryData, SPEC_NOTE: e.value.value }) }}
                                                        style={{ width: '250px' }}
                                                    />
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </form>
                            );
                        }}
                    </Formik>
                </PageContainer>
            }
        </React.Fragment >
    )
};

export default ProjectAdjustListExecQueryForm;