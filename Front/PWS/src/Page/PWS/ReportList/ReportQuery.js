import React, { useState, useEffect, useRef } from 'react';
import { Formik } from "formik";
import { PageContainer } from "../../../Basic/PageContainer";
import { GetHistory } from '../../../Basic/BasicData';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from "@progress/kendo-react-buttons";
import {getUnitList} from './ReportService';
import ReportService from './ReportService';
import { GetBasicData } from "../../../Basic/BasicData";

// 查詢-統計報表
const ReportQuery = (props) => {
    const {
        location: {
            state: {
                id,
                name,
                rpt_id
            } = {
                id: "",
                name: "",
            }
        }
    } = props;
    // 查詢條件
    const [queryData, setQueryData] = useState(ReportService.initData);
    const formRef = useRef(null);
    // 下拉選單資料
    const [ddlData, setDdlData] = useState([]);
    // 機關單位下拉
    const [unitData, setUnitData] = useState([]);
    // 是否能選取
    const [isChoose, setIsChoose] = useState(false);

    /**
     * 取得下拉選單資料
     * @returns
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 取年度下拉
        let ddl = await ReportService.getPageData();
        setDdlData(ddl);
        // 預設當年度
        let queryData = { ...ReportService.initData };
        // 是否能選取
        let choose = await ReportService.Ischoose();
        setIsChoose(choose);
        let orgId = await GetBasicData('orgId');
        setQueryData({
            ...queryData,
            STATISTICS_ID: id,
            STATISTICS_NAME: name,
            RPT_ID: rpt_id,
            OU_ID: orgId,
            FUND_OU_ID: orgId,
        })

        SetMaskOnOff(false);
    }

    // 初始畫面資料
    useEffect(() => {
        loadData();
    }, [])

    /**
     * 機關單位下拉選單
     * @returns 
     */
    const unitList = async() => {
        let orgId = await GetBasicData('orgId');
        let data = await getUnitList(orgId);
        data.unshift({ text: "請選擇", value: null });
        setUnitData(data);
    }

    // 機關單位下拉選單
    useEffect(() => {
        unitList();
    }, [queryData.OU_ID])

    // 匯出按鈕
    const buildButton = () => {
        return (
            <>
                <Button type="button" title='匯出報表' className="k-button-lighten" onClick={() => { handleSubmit() }} >匯出報表</Button>
            </>
        )
    }

    // 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    /**
     * 提交表單
     * @param {*} data 
     */
    const submit = (data) => {
        ReportService.exportRPT({ ...data, STATISTICS_NAME: getStatisticsName(data) });
    }

    /**
     * 組成報表名稱
     * @param {object} data 查詢條件
     * @returns 
     */
    const getStatisticsName = (data) => {
        let titleWithDate = `桃園市政府${data.PWS_YEAR}先期計畫`;

        // 報表編號
        switch (data.STATISTICS_ID) {
            case 1:
                return `${titleWithDate}重大施政計畫先期審查表`;
            case 2:
                return `${titleWithDate}重大施政計畫專案小組先期審查結果彙整表（局處計畫列表）公務預算`
            case 3:
                return `${titleWithDate}重大施政計畫專案小組先期審查結果彙整表（局處計畫列表）基金預算`
            case 4:
                return `${titleWithDate}重大施政計畫審查結果彙整表（全部局處總表）`
            case 5:
                return `${titleWithDate}重大施政計畫審查結果彙整表（全部基金總表）`
            case 6:
                return `${titleWithDate}辦理性別影響評估計畫一覽表`
            case 7:
                return `${titleWithDate}先期審查-委託研究計畫先期審查計畫表(管考+機關)`
            case 8:
                return `${titleWithDate}先期審查-委託研究計畫審查結果彙整表(管考)`
            case 9:
                return `${titleWithDate}先期審查-委託研究計畫審查結果彙整表(機關)`
            default:
                return '';
        }
    }

    return (
        <PageContainer style={{
            height: '100%',
            overflow: 'auto'
        }}
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">報表列印</h3>
                    <Button title='回報表清單' className="k-button" onClick={() => { GetHistory().push('/Home/Report') }} >回報表清單</Button>
                    {buildButton()}
                </>
            }>
            <Formik
                initialValues={queryData}
                innerRef={formRef}
                onSubmit={(data) => submit(data)}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                    } = prop;
                    return (
                        <form>
                            <table>
                                <tr>
                                    <th>報表名稱</th>
                                    <td>{name}</td>
                                </tr>
                                <tr >
                                    <th>年度</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="PROJECT_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.PWS_YEAR}
                                                onChange={(e) => { setQueryData({ ...queryData, PWS_YEAR: e.target.value }) }}
                                            />
                                        </div>
                                    </td>
                                </tr>
                                <tr >
                                    <th>計畫送出</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="IS_SEND"
                                                data={ddlData.IS_SEND}
                                                textField={"SET_VALUE"}
                                                dataItemKey={"SET_TYPE"}
                                                value={values.IS_SEND}
                                                onChange={(e) => { setQueryData({ ...queryData, IS_SEND: e.target.value }) }}
                                            />
                                        </div>
                                    </td>
                                </tr>
                                <tr >
                                    <th>優先順序</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="SEND_STATUS"
                                                data={ddlData.SEND_STATUS}
                                                textField={"SET_VALUE"}
                                                dataItemKey={"SET_TYPE"}
                                                value={values.SEND_STATUS}
                                                onChange={(e) => { setQueryData({ ...queryData, SEND_STATUS: e.target.value }) }}
                                            />
                                        </div>
                                    </td>
                                </tr>
                                <tr >
                                    <th>專案小組審核</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="AUDIT_STATUS"
                                                data={ddlData.AUDIT_STATUS}
                                                textField={"SET_VALUE"}
                                                dataItemKey={"SET_TYPE"}
                                                value={values.AUDIT_STATUS}
                                                onChange={(e) => { setQueryData({ ...queryData, AUDIT_STATUS: e.target.value }) }}
                                            />
                                        </div>
                                    </td>
                                </tr>
                                { (id == 2 || id == 1) &&
                                <>
                                    <tr >
                                        <th>選擇主管機關</th>
                                        <td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="OU_ID"
                                                    data={ddlData.OU_ID}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.OU_ID}
                                                    onChange={(e) => { setQueryData({ ...queryData, OU_ID: e.target.value, OU_NAME: e.target.text }) }}
                                                    disabled={!isChoose}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr >
                                        <th>選擇機關單位</th>
                                        <td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="CREATEUNITOUID"
                                                    data={unitData}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.CREATEUNITOUID}
                                                    onChange={(e) => { setQueryData({ ...queryData, CREATEUNITOUID: e.target.value, CREATEUNITNAME: e.target.text }) }}
                                                    disabled={!isChoose}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                </>
                                    
                                }
                                { id == 3 &&
                                    <tr >
                                        <th>基金主管機關</th>
                                        <td>
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="FUND_OU_ID"
                                                    data={ddlData.FUND_OU_ID}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.FUND_OU_ID}
                                                    onChange={(e) => { setQueryData({ ...queryData, FUND_OU_ID: e.target.value, FUND_OU_NAME: e.target.text }) }}
                                                    disabled={!isChoose}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                }
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </PageContainer>
    )
}
export default ReportQuery;
