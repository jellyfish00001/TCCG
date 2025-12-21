import React from 'react';
import { Formik } from "formik";
import * as Yup from 'yup';
import { PageContainer } from "../../../Basic/PageContainer";
import { GetHistory } from '../../../Basic/BasicData';
import { SetMaskOnOff, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { CheckIsHANDRole } from "../../../Basic/CommonService";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TextInput from '../../../Components/Input/TextInput';
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { Button } from "@progress/kendo-react-buttons";
import { Checkbox } from '@progress/kendo-react-inputs';
import { MultiSelect } from "@progress/kendo-react-dropdowns";
import { RadioGroup } from "@progress/kendo-react-inputs";
import { Error } from '@progress/kendo-react-labels';

import StatisticsService from './StatisticsService';

// 查詢-統計報表
const StatisticsQuery = (props) => {
    const {
        location: {
            state: {
                isRDECRole,
                id,
                name,
            } = {
                isRDECRole: "",
                id: "",
                name: "",
            }
        }
    } = props;

    // 是否顯示統計年月

    const [queryData, setQueryData] = React.useState(StatisticsService.initData);
    const formRef = React.useRef(null);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(StatisticsService.initDdlData);
    // 紀錄 特殊加註 已選擇資料
    const [specNoteSelData, setSpecNoteSelData] = React.useState([]);
    // 紀錄 執行落後類型 已選擇資料
    const [delayTypeSelData, setDelayTypeSelData] = React.useState([]);
    // 紀錄 工作項目 已選擇資料
    const [ctrlCheckpontTypeSelData, setCtrlCheckpontTypeSelData] = React.useState([]);
    // 是否"只有"主辦權限
    const [isHANDRole, setIsHANDRole] = React.useState(false);
    // 紀錄 工程類計畫清單(用於表10)
    const engineeringProjects = React.useRef([]);

    const loadData = async () => {
        SetMaskOnOff(true);

        let checkIsHANDRole = await CheckIsHANDRole();
        let ddl = await StatisticsService.getPageData(checkIsHANDRole);
        setDdlData(ddl);
        setIsHANDRole(checkIsHANDRole)

        let queryData = { ...StatisticsService.initData };

        // 表五預設年度帶入上個月份年度
        if (id === 5) {
            queryData = { ...queryData, PROJECT_YEAR: queryData.STATISTICS_YEAR };
        }

        setQueryData({
            ...queryData,
            STATISTICS_ID: id,
            STATISTICS_NAME: name,
        })

        if (id === 10) {
            let engProjects = await StatisticsService.getEngineeringProjects();
            engineeringProjects.current = engProjects;
        }

        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    const buildButton = () => {
        if (id === 1) {
            return (
                <>
                    <Button type="button" title='案件統計表' className="k-button-lighten" onClick={() => { handleSubmit("Statistics") }} >案件統計表</Button>
                    <Button type="button" title='案件狀態表' className="k-button-lighten" onClick={() => { handleSubmit("Status") }} >案件狀態表</Button>
                    <Button type="button" title='落後案件表' className="k-button-lighten" onClick={() => { handleSubmit("Delay") }} >落後案件表</Button>
                </>
            )
        }
        else if (id === 2 || id === 3 || id === 10) {
            return (
                <>
                    <Button type="button" title='簡版' className="k-button-lighten" onClick={() => { beforeSubmit("Short") }} >簡版</Button>
                    <Button type="button" title='詳版' className="k-button-lighten" onClick={() => { beforeSubmit("Detailed") }} >詳版</Button>
                </>
            )
        }
        else if (id === 4) {
            return (
                <>
                    <Button type="button" title='統計表' className="k-button-lighten" onClick={() => { handleSubmit("1") }} >統計表</Button>
                    {isRDECRole && <Button title='挑案列表' className="k-button-lighten" onClick={() => { handleSubmit("2") }} >挑案列表</Button>}
                </>
            )
        }
        else if (id === 6 || id === 7) {
            return (
                <Button title='報表' className="k-button-lighten" onClick={() => { beforeSubmit() }} >報表</Button>
            )
        }
        else if (id === 13) {
            return (
                <>
                    <Button title='Word' className="k-button-lighten" onClick={() => { handleSubmit("Word") }} >統計表</Button>
                    <Button title='Excel' className="k-button-lighten" onClick={() => { handleSubmit("Excel") }} >一覽表</Button>
                </>
            )
        }
        else {
            return (
                <Button title='報表' className="k-button-lighten" onClick={() => { handleSubmit("報表") }} >報表</Button>
            )
        }
    }

    /**
     * 送出前處理
     * @param {string} type 
     */
    const beforeSubmit = (type = "") => {
        let requestData = { ...formRef.current.values };
        let queryMonth = requestData.STATISTICS_MONTH;
        // 報表6 、7 查詢月份預設傳入系統年月
        if (id === 6 || id === 7) {
            requestData.STATISTICS_YEAR = requestData.PROJECT_YEAR;
            let currentMonth = new Date().getMonth() + 1;
            queryMonth = currentMonth;
        }
        if (id === 10) {
            let projectNo = formRef.current.values.PROJECT_NO;
            if (IsNullOrEmpty(projectNo)) {
                showGlobalMessageBox("請輸入計畫編號");
                return;
            }
            let projectItem = engineeringProjects.current.find(x => x.PROJECT_NO === projectNo);
            if (projectItem === undefined || projectItem.CP_KIND === null) {
                showGlobalMessageBox("無檔案可下載");
                return;
            }

            if (type === "Short" && projectItem.CP_KIND !== "0") {
                showGlobalMessageBox("該計畫非工程類");
                return;
            }

            if (projectItem.PROJ_ADJ_ID === null) {
                showGlobalMessageBox("該計畫未申請調整紀錄");
                return;
            }
        }
        requestData = { ...requestData, Type: type, STATISTICS_MONTH: queryMonth };
        handleSubmit(type, requestData);
    }

    // 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = (type, requestData = null) => {
        if (requestData == null) {
            requestData = { ...formRef.current.values, Type: type }
        }
        if (formRef.current) {
            formRef.current.handleSubmit();
            formRef.current.setValues({ ...requestData });
        }
    }

    /**
     * MultiSelect onChange事件
     * @param {*} prop 
     * @param {*} field 
     * @param {*} e 
     */
    const onChange = (prop, field, e) => {
        const { values, setValues } = prop;
        let isOk = true;
        if (field === "SPEC_NOTE") {
            setSpecNoteSelData([...e.value]);
        }
        else if (field === "DELAY_TYPE") {
            setDelayTypeSelData([...e.value]);
        }
        else {
            if (e.value.length <= 4) {
                setCtrlCheckpontTypeSelData([...e.value]);
            }
            else {
                showGlobalMessageBox("工作項目最多選4個");
                isOk = false;
            }
        }

        if (isOk) {
            setValues({ ...values, [field]: [...e.value.map(x => x.SET_TYPE)], [`${[field]}_text`]: [...e.value.map(x => x.SET_VALUE)] });
        }
    };

    // 欄位驗證
    const validateField = Yup.object().shape({
        PROJECT_NO: Yup.string().nullable()
            .test('PROJECT_NO', '此為必填欄位', function (item) {
                return (id === 10 && IsNullOrEmpty(item)) ? false : true;
            }),
        CTRL_CHK_POINT_TYPE: Yup.array()
            .test("CTRL_CHK_POINT_TYPE", '此欄位為必填', function (item) {
                return (id === 7 || id === 8) && item.length === 0 ? false : true;
            }),
        MASTER_DEPT: Yup.string().nullable()
            .test('MASTER_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && id !== 10 && CheckDept(this.parent) ? false : true;
            }),
        EXEC_DEPT: Yup.string().nullable()
            .test('EXEC_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && id !== 10 && CheckDept(this.parent) ? false : true;
            }),
        ASS_DEPT: Yup.string().nullable()
            .test('ASS_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && id !== 10 && CheckDept(this.parent) ? false : true;
            }),
        AGCY_DEPT: Yup.string().nullable()
            .test('AGCY_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && id !== 10 && CheckDept(this.parent) ? false : true;
            }),
        TUBE_STATUS: Yup.string().nullable()
            .test('TUBE_STATUS', '此欄位為必填', function (item) {
                return id === 11 && IsNullOrEmpty(item) ? false : true;
            }),
    });

    const CheckDept = (dataItem) => {
        let data = [dataItem.MASTER_DEPT, dataItem.EXEC_DEPT, dataItem.ASS_DEPT, dataItem.AGCY_DEPT];
        return data.filter(x => !IsNullOrEmpty(x)).length === 0;
    }

    const submit = (data) => {
        StatisticsService.exportRPT({ ...data, STATISTICS_NAME: getStatisticsName(data) });
    }

    /**
     * 組成報表名稱
     * @param {object} data 查詢條件
     * @returns 
     */
    const getStatisticsName = (data) => {
        let title = `桃園市政府重大建設計畫`;
        let titleWithDate = `桃園市政府${data.STATISTICS_YEAR}年${data.STATISTICS_MONTH}月重大建設計畫`;

        let typeName = '';
        let specNoteString = !data.SPEC_NOTE_text || data.SPEC_NOTE_text.length === 0 ? 'B級管制' :
            data.SPEC_NOTE_text.join('、');
        // 報表編號
        switch (data.STATISTICS_ID) {
            case 1:
                typeName = data.Type === 'Statistics' ? '案件統計表' : data.Type === 'Status' ? '案件狀態表' : '落後案件表';
                return titleWithDate + specNoteString + typeName;
            case 2:
                typeName = data.Type === 'Short' ? "(簡表)" : "(詳表)"
                return `${titleWithDate + specNoteString}地區統計表${typeName}`;
            case 3:
                typeName = data.Type === 'Short' ? "(簡表)" : "(詳表)"
                return `${titleWithDate + specNoteString}機關統計表${typeName}`;
            case 4:
                return data.Type === '1' ? `${titleWithDate + specNoteString}連續落後統計表` : `${titleWithDate}挑案列表`;
            case 5:
                return `${titleWithDate}未完成填報清單`;
            case 6:
                return `${titleWithDate}檢核點屆期預告`;
            case 7:
                return `${titleWithDate}特定工作項目屆期情形`;
            case 8:
                return `${titleWithDate}落後案件特定工作項目逾期情形`;
            case 9:
                return `桃園市政府及所屬各機關重大公共建設計畫預算執行情形明細表`;
            case 10:
                typeName = data.Type === 'Short' ? "工程類計畫歷次調整審查表(簡版)" : "計畫歷次調整審查表(詳版)";
                return `${title}選項列管案件${typeName}`;
            case 11:
                return `${title}年終考核成績權重明細表`;
            case 12:
                return `${titleWithDate}平時管考意見備註統計表`;
            case 13:
                typeName = data.Type === "Word" ? "統計表" : "一覽表";
                return `桃園市政府${data.STATISTICS_YEAR}年${data.STATISTICS_MONTH}重大建設系統介接公共工程雲端服務網資料${typeName}`;
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
                    <h3 className="k-dialog-titlebar">統計報表</h3>
                    <Button title='回報表清單' className="k-button" onClick={() => { GetHistory().push('/Home/Statistics/StatisticsList') }} >回報表清單</Button>
                    {buildButton()}
                </>
            }>
            <Formik
                initialValues={queryData}
                validationSchema={validateField}
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
                                    <td>報表{id}：{name}</td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>年度</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="PROJECT_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.PROJECT_YEAR}
                                                onChange={(e) => { setValues({ ...values, PROJECT_YEAR: e.target.value }) }}
                                            />
                                            {id !== 11
                                                ?
                                                ddlData.PROJECT_YEAR_STATUS.map(x => {
                                                    return (
                                                        <>
                                                            &nbsp;
                                                            <Checkbox
                                                                checked={values.PROJECT_YEAR_STATUS === x.value}
                                                                value={x.value}
                                                                label={x.text}
                                                                onChange={(e) => {
                                                                    if (e.value) {
                                                                        setValues({ ...values, PROJECT_YEAR_STATUS: e.target.element.value });
                                                                    }
                                                                    else {
                                                                        setValues({ ...values, PROJECT_YEAR_STATUS: "" });
                                                                    }
                                                                }}
                                                            />
                                                        </>
                                                    )
                                                })
                                                :
                                                <>
                                                    &nbsp;至&nbsp;
                                                    <DropDownListWithValue
                                                        name="PROJECT_YEAR_E"
                                                        data={ddlData.PLAN_YEAR}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.PROJECT_YEAR_E}
                                                        onChange={(e) => { setValues({ ...values, PROJECT_YEAR_E: e.target.value }) }}
                                                    />
                                                </>
                                            }
                                        </div>
                                    </td>
                                </tr>
                                <tr style={(id !== 11 && id !== 10 && id !== 5 && id !== 6 && id !== 7 && id !== 8) ? null : { display: 'none' }}>
                                    <th>統計年月</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="STATISTICS_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.STATISTICS_YEAR}
                                                onChange={(e) => { setValues({ ...values, STATISTICS_YEAR: e.target.value }) }}
                                            />
                                            <DropDownListWithValue
                                                name="STATISTICS_MONTH"
                                                data={ddlData.Month}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.STATISTICS_MONTH}
                                                onChange={(e) => { setValues({ ...values, STATISTICS_MONTH: e.target.value }) }}
                                            />
                                        </div>
                                    </td>
                                </tr>
                                <tr style={id === 11 ? null : { display: 'none' }}>
                                    <th>結案年月</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="CLOSE_S_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.CLOSE_S_YEAR}
                                                onChange={(e) => { setValues({ ...values, CLOSE_S_YEAR: e.target.value }) }}
                                            />
                                            <DropDownListWithValue
                                                name="CLOSE_S_MONTH"
                                                data={ddlData.Month}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.CLOSE_S_MONTH}
                                                onChange={(e) => { setValues({ ...values, CLOSE_S_MONTH: e.target.value }) }}
                                            />
                                            &nbsp;至&nbsp;
                                            <DropDownListWithValue
                                                name="CLOSE_E_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.CLOSE_E_YEAR}
                                                onChange={(e) => { setValues({ ...values, CLOSE_E_YEAR: e.target.value }) }}
                                            />
                                            <DropDownListWithValue
                                                name="CLOSE_E_MONTH"
                                                data={ddlData.Month}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.CLOSE_E_MONTH}
                                                onChange={(e) => { setValues({ ...values, CLOSE_E_MONTH: e.target.value }) }}
                                            />
                                        </div>
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>主管機關</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="MASTER_DEPT"
                                            data={ddlData.MASTER_ORGAN}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.MASTER_DEPT}
                                            error={errors.MASTER_DEPT}
                                            onChange={(e) => { setValues({ ...values, MASTER_DEPT: e.target.value }) }}
                                        />
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>執行機關</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="EXEC_DEPT"
                                            data={ddlData.EXEC_ORGAN}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.EXEC_DEPT}
                                            error={errors.EXEC_DEPT}
                                            onChange={(e) => { setValues({ ...values, EXEC_DEPT: e.target.value }) }}
                                        />
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>協辦機關</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="ASS_DEPT"
                                            data={ddlData.ASS_ORGAN}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.ASS_DEPT}
                                            error={errors.ASS_DEPT}
                                            onChange={(e) => { setValues({ ...values, ASS_DEPT: e.target.value }) }}
                                        />
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>代辦機關</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="AGCY_DEPT"
                                            data={ddlData.AGCY_ORGAN}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.AGCY_DEPT}
                                            error={errors.AGCY_DEPT}
                                            onChange={(e) => { setValues({ ...values, AGCY_DEPT: e.target.value }) }}
                                        />
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>
                                        {id === 11
                                            ?
                                            <CommonTooltip title={"列管狀態"} content={"選擇未結案、撤銷列管時結案年月將不當條件使用。"} />
                                            :
                                            "列管狀態"
                                        }
                                    </th>
                                    <td>
                                        <DropDownListWithValue
                                            name="TUBE_STATUS"
                                            data={ddlData.TUBE_STATUS}
                                            textField={"SET_VALUE"}
                                            dataItemKey={"SET_TYPE"}
                                            value={values.TUBE_STATUS}
                                            error={errors.TUBE_STATUS}
                                            onChange={(e) => { setValues({ ...values, TUBE_STATUS: e.target.value }) }}
                                        />
                                    </td>
                                </tr>
                                <tr style={id !== 10 ? null : { display: 'none' }}>
                                    <th>特殊加註</th>
                                    <td>
                                        <MultiSelect
                                            popupSettings={
                                                { className: "dropdown-text-size" }
                                            }
                                            placeholder="請選擇   "
                                            data={ddlData.SPEC_NOTE}
                                            textField="SET_VALUE"
                                            dataItemKey="SET_TYPE"
                                            onChange={(e) => onChange(prop, "SPEC_NOTE", e)}
                                            value={specNoteSelData}
                                        />
                                    </td>
                                </tr>
                                <tr style={id === 9 ? null : { display: 'none' }}>
                                    <th>計畫總經費(元)</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <NumericTextInput
                                                name="PROJECT_EXS_S"
                                                inputType={'text'}
                                                width="150px"
                                                min={0}
                                                value={values.PROJECT_EXS_S}
                                                onChange={(e) => {
                                                    setValues({ ...values, PROJECT_EXS_S: e.target.value })
                                                }}
                                                WithFormik={true}
                                            />
                                            &nbsp;至&nbsp;
                                            <NumericTextInput
                                                name="PROJECT_EXS_E"
                                                inputType={'text'}
                                                width="150px"
                                                min={0}
                                                value={values.PROJECT_EXS_E}
                                                onChange={(e) => {
                                                    setValues({ ...values, PROJECT_EXS_E: e.target.value })
                                                }}
                                                WithFormik={true}
                                            />
                                            &nbsp;元
                                        </div>
                                    </td>
                                </tr>
                                <tr style={id === 9 ? null : { display: 'none' }}>
                                    <th>執行率(%)</th>
                                    <td>
                                        <DropDownListWithValue
                                            name="EXEC_RATE"
                                            data={ddlData.EXEC_RATE}
                                            style={{ width: "220px" }}
                                            textField={"SET_VALUE"}
                                            dataItemKey={"SET_TYPE"}
                                            value={values.EXEC_RATE}
                                            onChange={(e) => { setValues({ ...values, EXEC_RATE: e.target.value }) }}
                                        />
                                    </td>
                                </tr>
                                <tr style={id === 4 ? null : { display: 'none' }}>
                                    <th>執行落後類型</th>
                                    <td>
                                        <MultiSelect
                                            popupSettings={
                                                { className: "dropdown-text-size" }
                                            }
                                            placeholder="請選擇   "
                                            data={ddlData.DELAY_TYPE}
                                            textField="SET_VALUE"
                                            dataItemKey="SET_TYPE"
                                            onChange={(e) => onChange(prop, "DELAY_TYPE", e)}
                                            value={delayTypeSelData}
                                        />
                                    </td>
                                </tr>
                                <tr style={id === 10 ? null : { display: 'none' }}>
                                    <th className='addRedStar'>計畫編號</th>
                                    <td>
                                        <TextInput
                                            name="PROJECT_NO"
                                            style={{ width: '100%' }}
                                            value={values.PROJECT_NO}
                                            onChange={(e) => { setValues({ ...values, PROJECT_NO: e.target.value }) }}
                                            error={errors.PROJECT_NO}
                                        />
                                    </td>
                                </tr>
                                <tr style={id === 7 || id === 8 ? null : { display: 'none' }}>
                                    <th className='addRedStar'>工作項目</th>
                                    <td>
                                        <>
                                            <MultiSelect
                                                popupSettings={
                                                    { className: "dropdown-text-size" }
                                                }
                                                placeholder="請選擇   "
                                                data={ddlData.CTRL_CHK_POINT_TYPE}
                                                textField="SET_VALUE"
                                                dataItemKey="SET_TYPE"
                                                onChange={(e) => onChange(prop, "CTRL_CHK_POINT_TYPE", e)}
                                                value={ctrlCheckpontTypeSelData}
                                            />
                                            <Error>{errors.CTRL_CHK_POINT_TYPE}</Error>
                                        </>
                                    </td>
                                </tr>
                                <tr style={id === 1 || id === 2 || id === 3 || id === 12 ? null : { display: 'none' }}>
                                    <th>排序</th>
                                    <td>
                                        {
                                            id === 1 &&
                                            <div style={{ display: "flex", alignItems: "center" }}>
                                                <DropDownListWithValue
                                                    name="SORT_TYPE_1"
                                                    data={ddlData.SORT_TYPE_1}
                                                    style={{ width: "260px" }}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.SORT_TYPE_1}
                                                    onChange={(e) => { setValues({ ...values, SORT_TYPE_1: e.target.value }) }}
                                                />
                                                &nbsp;
                                                <Checkbox
                                                    label={'依機關群組'}
                                                    value={true}
                                                    checked={values.SORT_GROUPBY_DEPT}
                                                    onChange={(e) => {
                                                        setValues({ ...values, SORT_GROUPBY_DEPT: e.value });
                                                    }}
                                                />
                                            </div>
                                        }
                                        {
                                            id === 2 &&
                                            <RadioGroup
                                                name="SORT_TYPE_2"
                                                value={values.SORT_TYPE_2}
                                                data={ddlData.SORT_TYPE_2}
                                                layout={"horizontal"}
                                                onChange={(e) => setValues({ ...values, SORT_TYPE_2: e.value })}
                                            />
                                        }
                                        {
                                            id === 3 &&
                                            <RadioGroup
                                                name="SORT_TYPE_3"
                                                value={values.SORT_TYPE_3}
                                                data={ddlData.SORT_TYPE_3}
                                                layout={"horizontal"}
                                                onChange={(e) => setValues({ ...values, SORT_TYPE_3: e.value })}
                                            />
                                        }
                                    </td>
                                </tr>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </PageContainer >
    )
}
export default StatisticsQuery;
