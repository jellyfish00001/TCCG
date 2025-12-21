import React from 'react';
import { Formik } from "formik";
import { PageContainer } from "../../../Basic/PageContainer";
import { GetHistory } from '../../../Basic/BasicData';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TextInput from '../../../Components/Input/TextInput';
import { Button } from "@progress/kendo-react-buttons";
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { GetInitData, initDdlData, GetAllDropDowns, exportRPT } from './ReportsService';

/**
 * 查詢-統計報表
 * @param {*} props
 * @return {*} 
 */
const ReportsQuery = (props) => {
    const {
        location: {
            state: {
                isRDECRole,    // 是否為管考
                id,           // 報表編號
                name,        // 報表名稱
            } = {
                isRDECRole: "",
                id: "",
                name: "",
            }
        }
    } = props;

    /* 報表名稱 */
    /** @type {int} 季委託研究計畫列管表 */
    const RDSeasonReport = 1;
    /** @type {int} 本府委託研究計畫成果及運用情形調查列管表 */
    const RDPlanResultUsageSituations = 2;
    /** @type {int} 委託研究計畫執行情形調查表 */
    const RDPlanExecution = 3;
    /** @type {int} 參採情形總表 */
    const RDParticipating = 4;
    /** @type {int} 續列管委託研究計畫成果及運用情形調查表 */
    const RDPlanExecutionSurvey = 5;

    const [queryData, setQueryData] = React.useState([]); // initData
    
    const formRef = React.useRef(null);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(initDdlData);

    const loadData = async () => {
        SetMaskOnOff(true);
        let ddl = await GetAllDropDowns();
        setDdlData(ddl);
        let queryData = await GetInitData();
        setQueryData({
            ...queryData,
            REPORT_ID: id,
            REPORT_NAME: name,
        })
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 依據不同報表，顯示不同 buton
     */
    const buildButton = () => {
        if (id === RDSeasonReport || id === RDPlanResultUsageSituations) {
            return (
                <>
                    <Button type="button" title='列管表' className="k-button-lighten" onClick={() => { handleSubmit()}} >列管表</Button>
                </>
            )
        }
        else if (id === RDPlanExecution || id === RDPlanExecutionSurvey) {
            return (
                <>
                    <Button type="button" title='調查表' className="k-button-lighten" onClick={() => { handleSubmit()}} >調查表</Button>
                </>
            )
        }
        else if (id === RDParticipating) {
            return (
                <>
                    <Button type="button" title='統計表' className="k-button-lighten" onClick={() => { handleSubmit()}} >總表</Button>
                </>
            )
        }
    }

    // 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    const submit = (data) => {
        exportRPT({ ...data, STATISTICS_NAME: getStatisticsName(data) });
    }

    /**
     * 組成報表名稱
     * @param {object} data 查詢條件
     * @returns 
     */
    const getStatisticsName = (data) => {
        let titleWithDate = `桃園市政府${data.PLAN_YEAR}年委託研究`;

        // 報表編號
        switch (data.REPORT_ID) {
            case 1:
                return `${titleWithDate}季委託研究計畫列管表`;
            case 2:
                return `${titleWithDate}本府委託研究計劃成果及運用情形調查列管表`
            case 3:
                return `${titleWithDate}委託研究計畫執行情形調查表`
            case 4:
                return `${titleWithDate}參採情形總表`
            case 5:
                return `${titleWithDate}續列管委託研究計畫成果及運用情形調查表`
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
                        errors,
                        setValues
                    } = prop;
                    return (
                        <form>
                            <table>
                                <tr>
                                    <th>報表名稱</th>
                                    <td>表{id}：{name}</td>
                                </tr>
                                <tr>
                                    <th>填報年度</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="PLAN_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.PLAN_YEAR}
                                                onChange={(e) => { setValues({ ...values, PLAN_YEAR: e.target.value }) }}/>
                                        </div>
                                    </td>
                                </tr>
                                <tr style={id !== 1 ? null : { display: 'none' }}>
                                    <th>研究成果整體評估</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="SITUATION_TYPE"
                                            data={ddlData.SITUATION_TYPE}
                                            textField={"SET_VALUE"}
                                            dataItemKey={"SET_TYPE"}
                                            value={values.SITUATION_TYPE}
                                            onChange={(e) => { setValues({ ...values, SITUATION_TYPE: e.target.value }) }}/>
                                    </td>
                                </tr>
                                <tr style={id === 1 ? null : { display: 'none' }}>
                                    <th>季別</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="SEASON_TYPE"
                                            data={ddlData.SEASON_TYPE}
                                            textField={"SET_VALUE"}
                                            dataItemKey={"SET_TYPE"}
                                            value={values.SEASON_TYPE}
                                            error={errors.SEASON_TYPE}
                                            onChange={(e) => { setValues({ ...values, SEASON_TYPE: e.target.value }) }}/>
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>局處</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="ORG_ID"
                                            data={ddlData.ORG_ID}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.ORG_ID}
                                            onChange={(e) => { setValues({ ...values, ORG_ID: e.target.value }) }}/>
                                    </td>
                                </tr>
                                <tr>
                                    <th>計畫編號</th>
                                    <td>
                                        <TextInput
                                            name="PLAN_NO"
                                            style={{ width: '100%' }}
                                            value={values.PLAN_NO}
                                            onChange={(e) => { setValues({ ...values, PLAN_NO: e.target.value }) }}
                                            error={errors.PLAN_NO}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th>計畫名稱</th>
                                    <td>
                                        <TextInput
                                            name="PLAN_NAME"
                                            style={{ width: '100%' }}
                                            value={values.PLAN_NAME}
                                            onChange={(e) => { setValues({ ...values, PLAN_NAME: e.target.value }) }}
                                            error={errors.PLAN_NAME}
                                        />
                                    </td>
                                </tr>
                                <tr style={id === RDPlanResultUsageSituations || id === RDPlanExecutionSurvey ? null : { display: 'none' }}>
                                    <th>研究期程</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <TwDatePicker
                                                name="PLAN_START_DATE"
                                                format="yyy/MM/dd"
                                                onChange={(e) => { setValues({ ...values, PLAN_START_DATE: e.target.value }) }}
                                                value={values.PLAN_START_DATE}
                                            />
                                            ~
                                            <TwDatePicker
                                                name="PLAN_END_DATE"
                                                format="yyy/MM/dd"
                                                onChange={(e) => { setValues({ ...values, PLAN_END_DATE: e.target.value }) }}
                                                value={values.PLAN_END_DATE}
                                            />
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </PageContainer>
    )
}
export default ReportsQuery;