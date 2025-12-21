import React from "react";
import { Formik } from "formik";
import * as Yup from 'yup';
import GotoTopBtn, { scrollTop } from "../../../Components/Utils/GotoTopBtn";
import { DropDownListWithValue } from "../../../Components/Dropdowns/DropDownListWithValue";
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import TextInput from "../../../Components/Input/TextInput";
import NumericTextInput from "../../../Components/Input/NumericTextInput";
import { CascadeDropDown } from "../../../Components/Dropdowns/CascadeDropDown";
import { IsNullOrEmpty, SetMaskOnOff, FormatDate } from "../../../Basic/SDOExtension";
import { CheckIsRDECRole, CheckIsHANDRole } from "../../../Basic/CommonService";
import { PageContainer } from "../../../Basic/PageContainer";
import { GetBasicData } from "../../../Basic/BasicData";
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { MultiSelect } from "@progress/kendo-react-dropdowns";
import { Button } from "@progress/kendo-react-buttons";
import { Checkbox, RadioButton } from "@progress/kendo-react-inputs";
import { Error } from '@progress/kendo-react-labels';
import { getAllDropDowns, getOptionColumns, getQueryCondition } from "./UnitingQueryServer";
import { loadCheckPoint } from "../ProjectList/ProjectListService";
import UnitingQueryGrid from "./UnitingQueryGrid";

const UnitingQueryMain = (props) => {

    const { location: { state } } = props;


    // 查詢條件
    const [queryCondition, setQueryCondition] = React.useState({});
    // 選取的欄位
    const [selectedColumnKey, setSelectedColumnKey] = React.useState([]);
    const [tempSelectedColumnKey, setTempSelectedColumnKey] = React.useState([]);
    const [selectedAllKey, setSelectedAllKey] = React.useState([]);
    // 自選欄位
    const [optionColumns, setOptionColumns] = React.useState([]);
    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({});
    // 登入者是否具備管考權限(管考角色)
    const [isRDEC, setIsRDEC] = React.useState(false);
    // 要不要顯示查詢結果
    const [showQryResult, setShowQryResult] = React.useState(false);
    const formRef = React.useRef(null);
    // 是否"只有"主辦權限
    const [isHAND, setIsHAND] = React.useState(false);
    const [handOrgId, setHandOrgId] = React.useState("");

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        await getData();

        if (state) {
            handleSubmit();
        }
    }

    const getData = async () => {
        SetMaskOnOff(true);

        // 登入者是否具備管考權限(管考角色)
        let isRdec = await CheckIsRDECRole();
        // 登入者是否"只有"主辦權限(主辦角色)
        let isHand = await CheckIsHANDRole();
        let orgId = await GetBasicData("orgId");
        let ddl = await getAllDropDowns(isHand);
        let queryData = await getQueryCondition(isHand, orgId);
        let column = await getOptionColumns(isRdec);
        let selectedColumns = [];
        let selectedColumnTemps = [];
        column.forEach(x => {
            if (x.IsCheck) {
                selectedColumns.push(x.Key);
                selectedColumnTemps.push(x.Key);
            }
        })

        if (state) {
            let newData = setSSOCloudType(queryData, column, ddl);
            queryData = newData.QueryData;
            selectedColumns = newData.SelectedColumns;
        }

        setQueryCondition(queryData);
        setHandOrgId(orgId);
        setSelectedColumnKey([...selectedColumns]);
        setTempSelectedColumnKey([...selectedColumnTemps]);
        setIsRDEC(isRdec);
        setIsHAND(isHand);
        setDdlData(ddl);
        setOptionColumns(column);

        SetMaskOnOff(false);
    }

    // 設定 SSOCloud 帶過來的型態
    const setSSOCloudType = (queryData, column, ddl) => {
        let selectdList = [];
        switch (state.queryType) {
            case "1": // 已結案
                queryData.TUBE_STATUS = "S2";
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C", "CLOSED_OR_REVOKE"];
                break;
            case "2": // 進度符合
                queryData.DELAY_TYPE = ddl.DELAY_CLASS.filter(x => x.SET_TYPE === "D0");
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C"];
                break;
            case "3": // 進度落後
                queryData.DELAY_TYPE = ddl.DELAY_CLASS.filter(x => x.SET_TYPE !== "D0");
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C", "DELAY_KIND", "DELAY_CLASS_C", "DELAY_SUBCLASS_C", "DELAY_RESPON", "DELAY_CAUSAL", "SOLUTION", "COORDINATION", "DEADLINES"];
                break;
            case "4": // D1
                queryData.DELAY_TYPE = ddl.DELAY_CLASS.filter(x => x.SET_TYPE === "D1");
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C", "DELAY_KIND", "DELAY_CLASS_C", "DELAY_SUBCLASS_C", "DELAY_RESPON", "DELAY_CAUSAL", "SOLUTION", "COORDINATION", "DEADLINES"];
                break;
            case "5": // D2
                queryData.DELAY_TYPE = ddl.DELAY_CLASS.filter(x => x.SET_TYPE === "D2");
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C", "DELAY_KIND", "DELAY_CLASS_C", "DELAY_SUBCLASS_C", "DELAY_RESPON", "DELAY_CAUSAL", "SOLUTION", "COORDINATION", "DEADLINES"];
                break;
            case "6": // D3
                queryData.DELAY_TYPE = ddl.DELAY_CLASS.filter(x => x.SET_TYPE === "D3");
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C", "DELAY_KIND", "DELAY_CLASS_C", "DELAY_SUBCLASS_C", "DELAY_RESPON", "DELAY_CAUSAL", "SOLUTION", "COORDINATION", "DEADLINES"];
                break;
            case "7": // 撤銷列管
                queryData.TUBE_STATUS = "S3";
                selectdList = ["PROJECT_NAME", "PROJECT_NO", "MASTER_ORGAN_C", "EXEC_ORGAN_C", "CLOSED_OR_REVOKE"];
                break;
            default:
                break;
        }

        let selectedColumns = [];
        column.forEach(x => {
            if (selectdList.includes(x.Key)) {
                selectedColumns.push(x.Key);
            }
        })

        return {
            QueryData: queryData,
            SelectedColumns: selectedColumns,
        }
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    //利用Ref把Formik的reset功能拉出來，以提供外部按鈕呼叫
    const handleReset = () => {
        scrollTop("UnitingQuery");

        setQueryCondition(getQueryCondition(isHAND, handOrgId));
        setSelectedColumnKey(tempSelectedColumnKey);
        setSelectedAllKey([]);
        if (formRef.current) {
            formRef.current.handleReset();
        }
    }

    // 查詢
    const query = (data) => {
        if (selectedColumnKey.length === 0) {
            showGlobalMessageBox("請至少選擇一個自選欄位。")
            return
        }

        setQueryCondition(data);
        setShowQryResult(true);
    }

    // 取得執行方式下拉選單
    const loadCheckpointDropDown = async (cpKind) => {
        return await loadCheckPoint(cpKind);
    }

    // 取得自選欄位
    const getOpetionFieldElement = () => {
        let element = [], title = [], content = [], current = "";
        const combine = () => {
            element.push(
                <>
                    <tr><th>{title}</th> </tr>
                    <tr>
                        <td>
                            <div style={{ display: "flex", flexWrap: "wrap" }}>
                                {content}
                            </div>
                        </td>
                    </tr>
                </>
            )
        }

        optionColumns.filter(x => x.Hidden === false).forEach(x => {
            if (!IsNullOrEmpty(current) && current !== x.Parent) {
                combine();
                title = [];
                content = [];
            }

            current = x.Parent;
            x.Level === 0
                ? title.push(
                    <div style={{ display: "flex" }}>
                        {x.Title}
                        <Checkbox
                            value={x.Key}
                            label={"全選"}
                            checked={selectedAllKey.includes(x.Key)}
                            onChange={(e) => {
                                let checkedData = [...selectedColumnKey];
                                let parent = e.target.element.value;
                                let optionColumnList = optionColumns.filter(y => y.Parent === parent && !y.Hidden);
                                optionColumnList.forEach(y => {
                                    y.IsCheck = e.value
                                    if (e.value) {
                                        if (!checkedData.includes(y.Key))
                                            checkedData.push(y.Key);
                                    } else {
                                        checkedData.splice(checkedData.indexOf(y.Key), 1)
                                    }
                                })
                                setSelectedColumnKey(checkedData);

                                if (e.value) {
                                    setSelectedAllKey([...selectedAllKey, parent]);
                                }
                                else {
                                    setSelectedAllKey([...selectedAllKey.filter(y => y !== parent)]);
                                }
                            }}
                        />
                    </div>
                )
                : content.push(
                    <div style={{ width: "25%", padding: "5px 15px" }}>
                        <Checkbox
                            value={x.Key}
                            label={x.Title}
                            checked={selectedColumnKey.includes(x.Key)}
                            onChange={(e) => {
                                let key = e.target.element.value;
                                let parent = optionColumns.find(y => y.Key === key)?.Parent;

                                let checkedData = [...selectedColumnKey];
                                if (e.value) {
                                    checkedData.push(key);

                                } else {
                                    checkedData.splice(checkedData.indexOf(key), 1);
                                    setSelectedAllKey([...selectedAllKey.filter(y => y !== parent)]);
                                }
                                setSelectedColumnKey(checkedData);
                            }}
                        />
                    </div>
                )
        })
        combine();
        return element;
    }

    // 比對標案系統
    const getCheckBox = (ddlKey, field, setValues, values, width) => {
        const data = ddlData[ddlKey]
        return (
            <div style={{ display: "flex", flexWrap: "wrap" }}>
                {data?.map(x => {
                    return (
                        <div style={{ width: `${width}%`, paddingRight: "15px" }}>
                            <Checkbox
                                checked={values[field].includes(x.SET_TYPE)}
                                value={x.SET_TYPE}
                                label={x.SET_VALUE}
                                onChange={(e) => {
                                    let dataItem = [...values[field]];
                                    if (e.value) {
                                        dataItem.push(e.target.element.value);
                                    } else {
                                        dataItem.splice(dataItem.indexOf(e.target.element.value), 1)
                                    }
                                    setValues({ ...values, [field]: dataItem });
                                }}
                            />
                        </div>
                    )
                })}
            </div>
        )
    }

    // 預警類型1、預警類型2
    const getCheckBoxRadio = (ddlKey, field, setValues, values, errors, width) => {
        const data = ddlData[ddlKey];

        return (
            <>
                <div style={{ display: "flex", flexWrap: "wrap" }}>
                    {data?.map(x => {
                        return (
                            <div style={{ width: `${width}%`, paddingRight: "15px" }}>
                                <Checkbox
                                    checked={values[field] === x.SET_TYPE}
                                    value={x.SET_TYPE}
                                    label={x.SET_VALUE}
                                    onChange={(e) => {
                                        const val = values[field];
                                        const newVal = e.target.element.value;
                                        setValues({ ...values, [field]: val === newVal ? "" : newVal });
                                    }}
                                />
                            </div>
                        )
                    })}
                </div>
                <div>{<Error>{errors[field]}</Error>}</div>
            </>
        )
    }

    // 開竣工區間、比對標案系統
    const getRadioBox = (key, field, setValues, values, errors) => {
        const data = ddlData[key];
        return (
            <>
                <div style={{ display: "flex", flexWrap: "wrap" }}>
                    {data?.map(x => {
                        return (
                            <div style={{ paddingRight: "15px" }}>
                                <RadioButton
                                    checked={x.SET_TYPE === values[field]}
                                    name={field}
                                    value={x.SET_TYPE}
                                    label={x.SET_VALUE}
                                    onChange={(e) => {
                                        setValues({ ...values, [field]: e.target.element.value });
                                    }}
                                />
                            </div>
                        )
                    })}
                </div>
                <div>{<Error>{errors[field]}</Error>}</div>
            </>
        )
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        MASTER_DEPT: Yup.string().nullable()
            .test('MASTER_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHAND && CheckDept(this.parent) ? false : true;
            }),
        EXEC_DEPT: Yup.string().nullable()
            .test('EXEC_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHAND && CheckDept(this.parent) ? false : true;
            }),
        ASS_DEPT: Yup.string().nullable()
            .test('ASS_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHAND && CheckDept(this.parent) ? false : true;
            }),
        AGCY_DEPT: Yup.string().nullable()
            .test('AGCY_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHAND && CheckDept(this.parent) ? false : true;
            }),
        ALERT_TYPE_2_INFO: Yup.string()
            .test('ALERT_TYPE_2_INFO', '此欄位必選', function (item) {
                return IsNullOrEmpty(item) && !IsNullOrEmpty(this.parent.ALERT_TYPE_2_CONDITION) ? false : true;
            }),
        ALERT_TYPE_2_CONDITION: Yup.string()
            .test('ALERT_TYPE_2_CONDITION', '此欄位必選', function (item) {
                return IsNullOrEmpty(item) && !IsNullOrEmpty(this.parent.ALERT_TYPE_2_INFO) ? false : true;
            }),
    })

    const CheckDept = (dataItem) => {
        let data = [dataItem.MASTER_DEPT, dataItem.EXEC_DEPT, dataItem.ASS_DEPT, dataItem.AGCY_DEPT];
        return data.filter(x => !IsNullOrEmpty(x)).length === 0;
    }

    const onFormDatePickerChange = (e, setValues, values, isStart) => {
        let newObj = {};
        newObj[e.target.name] = e.target.value == null
            ? null
            : isStart
                ? FormatDate(e.target.value, "YYYY-MM-DD")
                : FormatDate(e.target.value, "YYYY-MM-DD") + " 23:59:59";
        setValues({ ...values, ...newObj })
    }

    return (
        <>
            {
                !showQryResult &&
                <PageContainer
                    className="UnitingQuery"
                    style={{ overflow: "auto", height: "100%" }}
                >
                    <div className='fn-buttons fixed-search'>
                        <h3 className="k-dialog-titlebar">綜合查詢</h3>
                        <div className="fixed-search-btn">
                            <Button title="查詢" className="k-button-lighten" onClick={() => handleSubmit()}>查詢</Button>
                            <Button title="取消" className="k-button-lighten" onClick={() => handleReset()}>取消</Button>
                        </div>
                    </div>
                    <h3 className="k-dialog-titlebar" style={{ marginTop: "60px" }}>自選條件</h3>
                    <Formik
                        initialValues={queryCondition}
                        validationSchema={validateField}
                        innerRef={formRef}
                        onSubmit={(data) => query(data)}
                        enableReinitialize //允許重複賦予初始值
                    >
                        {prop => {
                            const {
                                values,
                                errors,
                                handleBlur,
                                handleChange,
                                setValues
                            } = prop;
                            return (
                                <form>
                                    <table>
                                        <tr>
                                            <th>年度</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <DropDownListWithValue
                                                        name="PROJECT_YEAR_S"
                                                        data={ddlData.PROJECT_YEAR}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.PROJECT_YEAR_S}
                                                        onChange={(e) => { setValues({ ...values, PROJECT_YEAR_S: e.target.value }) }}
                                                    />

                                                    {
                                                        ddlData.PROJECT_YEAR_STATUS !== undefined &&
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
                                                    }
                                                    <i
                                                        className={`fa ${values.PIS_SELECT === "1" ? "fa-star_Yellow" : "fa-star-o_Yellow"}`}
                                                        style={{ marginLeft: "10px" }}
                                                        onClick={() => { setValues({ ...values, PIS_SELECT: IsNullOrEmpty(values.PIS_SELECT) ? "1" : "" }) }}
                                                    ></i>
                                                    <span style={{ marginLeft: '5px' }}>以釘選計畫為查詢範圍</span>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>統計年月</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <DropDownListWithValue
                                                        name="STATISTICS_YEAR"
                                                        data={ddlData.PROJECT_YEAR}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.STATISTICS_YEAR}
                                                        onChange={(e) => { setValues({ ...values, STATISTICS_YEAR: e.target.value }) }}
                                                    />
                                                    <DropDownListWithValue
                                                        name="STATISTICS_MONTH"
                                                        data={ddlData.MONTH}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.STATISTICS_MONTH}
                                                        onChange={(e) => { setValues({ ...values, STATISTICS_MONTH: e.target.value }) }}
                                                    />
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                計畫編號
                                            </th>
                                            <td colSpan={2}>
                                                <TextInput
                                                    name="PROJECT_NO"
                                                    value={values.PROJECT_NO}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                    style={{ width: "100%" }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                計畫名稱
                                            </th>
                                            <td colSpan={2}>
                                                <TextInput
                                                    name="PROJECT_NAME"
                                                    value={values.PROJECT_NAME}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                    style={{ width: "100%" }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>主管機關</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="MASTER_DEPT"
                                                    data={ddlData.ORGAN}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.MASTER_DEPT}
                                                    error={errors.MASTER_DEPT}
                                                    onChange={(e) => { setValues({ ...values, MASTER_DEPT: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>執行機關</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="EXEC_DEPT"
                                                    data={ddlData.ORGAN}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.EXEC_DEPT}
                                                    error={errors.EXEC_DEPT}
                                                    onChange={(e) => { setValues({ ...values, EXEC_DEPT: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>協辦機關</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="ASS_DEPT"
                                                    data={ddlData.ORGAN}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.ASS_DEPT}
                                                    error={errors.ASS_DEPT}
                                                    onChange={(e) => { setValues({ ...values, ASS_DEPT: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>代辦機關</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="AGCY_DEPT"
                                                    data={ddlData.ORGAN}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.AGCY_DEPT}
                                                    error={errors.AGCY_DEPT}
                                                    onChange={(e) => { setValues({ ...values, AGCY_DEPT: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>計畫總經費</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <NumericTextInput
                                                        name="PROJECT_EXS_S"
                                                        value={values.PROJECT_EXS_S}
                                                        onChange={(e) => {
                                                            setValues({ ...values, PROJECT_EXS_S: e.target.value })
                                                        }}
                                                    />
                                                    至
                                                    <NumericTextInput
                                                        name="PROJECT_EXS_E"
                                                        value={values.PROJECT_EXS_E}
                                                        onChange={(e) => {
                                                            setValues({ ...values, PROJECT_EXS_E: e.target.value })
                                                        }}
                                                    />
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>地區別</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="AREA"
                                                    data={ddlData.AREA}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.AREA}
                                                    onChange={(e) => {
                                                        setValues({ ...values, AREA: e.target.value })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>建設類別</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='BUILD_KIND'
                                                    value={values.BUILD_KIND}
                                                    data={ddlData.COM_PLANKIND}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, BUILD_KIND: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>主要建設</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='MAIN_BUILD'
                                                    value={values.MAIN_BUILD}
                                                    data={ddlData.COM_PLANKIND}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, MAIN_BUILD: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>附屬設施</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='SUB_BUILD'
                                                    value={values.SUB_BUILD}
                                                    data={ddlData.COM_PLANKIND}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, SUB_BUILD: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>相關審查</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='REVIEWITEM'
                                                    value={values.REVIEWITEM}
                                                    data={ddlData.COM_REVIEWITEM}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, REVIEWITEM: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>特殊加註</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='SPEC_NOTE'
                                                    value={values.SPEC_NOTE}
                                                    data={ddlData.SPEC_NOTE}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, SPEC_NOTE: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>會議種類</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='CONFERENCE_GENRE'
                                                    value={values.COM_CONFERENCEGENRE}
                                                    data={ddlData.COM_CONFERENCEGENRE}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, CONFERENCE_GENRE: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>立案時間</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <TwDatePicker
                                                        name="CREATEDTIME_S"
                                                        format="yyy/MM/dd"
                                                        onChange={(e) => {
                                                            onFormDatePickerChange(e, setValues, values, true);
                                                        }}
                                                        value={values.CREATEDTIME_S == null ? null : new Date(values.CREATEDTIME_S)}
                                                    />
                                                    <span>~</span>
                                                    <TwDatePicker
                                                        name="CREATEDTIME_E"
                                                        format="yyy/MM/dd"
                                                        onChange={(e) => {
                                                            onFormDatePickerChange(e, setValues, values, false);
                                                        }}
                                                        value={values.CREATEDTIME_E == null ? null : new Date(values.CREATEDTIME_E)}
                                                    />
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>期程調整區間</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <TwDatePicker
                                                        name="APPRV_DATE_S"
                                                        format="yyy/MM/dd"
                                                        onChange={(e) => {
                                                            onFormDatePickerChange(e, setValues, values, true);
                                                        }}
                                                        value={values.APPRV_DATE_S == null ? null : new Date(values.APPRV_DATE_S)}
                                                    />
                                                    <span>~</span>
                                                    <TwDatePicker
                                                        name="APPRV_DATE_E"
                                                        format="yyy/MM/dd"
                                                        onChange={(e) => {
                                                            onFormDatePickerChange(e, setValues, values, false);
                                                        }}
                                                        value={values.APPRV_DATE_E == null ? null : new Date(values.APPRV_DATE_E)}
                                                    />
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>列管狀態</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="TUBE_STATUS"
                                                    data={ddlData.TUBE_STATUS}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    value={values.TUBE_STATUS}
                                                    onChange={(e) => { setValues({ ...values, TUBE_STATUS: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>分案或併案</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="MERGE_STATUS"
                                                    data={ddlData.PROMERGESTATUS}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    value={values.MERGE_STATUS}
                                                    onChange={(e) => { setValues({ ...values, MERGE_STATUS: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>平時管考意見備註</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='COM_IPCMEMO'
                                                    value={values.COM_IPCMEMO}
                                                    data={ddlData.COM_IPCMEMO}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, COM_IPCMEMO: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>執行方式</th>
                                            <td colSpan={2}>
                                                <CascadeDropDown
                                                    fristDdlData={ddlData.CP_KIND}
                                                    firstDdlValue={values.CP_KIND}
                                                    firstColumn="CP_KIND"
                                                    firstTextField="SET_VALUE"
                                                    firstDataItemKey="SET_TYPE"
                                                    secondDdlInitData={[{ text: "請選擇", value: "" }]}
                                                    secondDdlValue={values.RUNWAY_C}
                                                    secondColumn="RUNWAY_C"
                                                    getSecondDdlData={(data) => loadCheckpointDropDown(data)}
                                                    secondDdlStyle={{ width: '250px' }}
                                                    values={values}
                                                    setValues={setValues}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>檢核點進度落後天數</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <NumericTextInput
                                                        name="DELAY_DAY_START"
                                                        value={values.DELAY_DAY_START}
                                                        onChange={(e) => {
                                                            setValues({ ...values, DELAY_DAY_START: e.target.value })
                                                        }}
                                                    />天 至
                                                    <NumericTextInput
                                                        name="DELAY_DAY_END"
                                                        value={values.DELAY_DAY_END}
                                                        onChange={(e) => {
                                                            setValues({ ...values, DELAY_DAY_END: e.target.value })
                                                        }}
                                                    />天
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th colSpan={3}>以下限定工程類使用條件</th>
                                        </tr>
                                        <tr>
                                            <th>執行階段</th>
                                            <td colSpan={2}>
                                                <DropDownListWithValue
                                                    name="EXEC_STAGE"
                                                    data={ddlData.EXEC_STAGE}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    value={values.EXEC_STAGE}
                                                    onChange={(e) => { setValues({ ...values, EXEC_STAGE: e.target.value }) }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>開竣工區間</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    {getRadioBox("CTRL_POINT", "CTRL_POINT", setValues, values, errors)}
                                                    <TwDatePicker
                                                        name="COMPLETED_START"
                                                        format="yyy/MM/dd"
                                                        onChange={handleChange}
                                                        value={values.COMPLETED_START == null ? null : new Date(values.COMPLETED_START)}
                                                    />
                                                    至
                                                    <TwDatePicker
                                                        name="COMPLETED_END"
                                                        format="yyy/MM/dd"
                                                        onChange={handleChange}
                                                        value={values.COMPLETED_END == null ? null : new Date(values.COMPLETED_END)}
                                                    />
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>實際工程進度</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <NumericTextInput
                                                        name="IPC_ACT_PRG_STAR"
                                                        value={values.IPC_ACT_PRG_STAR}
                                                        onChange={(e) => {
                                                            setValues({ ...values, IPC_ACT_PRG_STAR: e.target.value })
                                                        }}
                                                    />%
                                                    ~
                                                    <NumericTextInput
                                                        name="IPC_ACT_PRG_END"
                                                        value={values.IPC_ACT_PRG_END}
                                                        onChange={(e) => {
                                                            setValues({ ...values, IPC_ACT_PRG_END: e.target.value })
                                                        }}
                                                    />%
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>落後類型</th>
                                            <td colSpan={2}>
                                                <MultiSelect
                                                    popupSettings={
                                                        { className: "dropdown-text-size" }
                                                    }
                                                    placeholder="請選擇   "
                                                    name='DELAY_TYPE'
                                                    value={values.DELAY_TYPE}
                                                    data={ddlData.DELAY_CLASS}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    onChange={(e) => {
                                                        setValues({
                                                            ...values, DELAY_TYPE: e.value.length === 0 ? "" : e.value
                                                        })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>施工進度落後</th>
                                            <td colSpan={2}>
                                                <div style={{ display: "flex", alignItems: "center" }}>
                                                    <NumericTextInput
                                                        name="DELAY_PRG"
                                                        value={values.DELAY_PRG}
                                                        onChange={(e) => {
                                                            setValues({ ...values, DELAY_PRG: e.target.value * -1 })
                                                        }}
                                                    />%
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>比對標案系統</th>
                                            <td style={{ width: "40%" }}>
                                                {getCheckBox("CMP_COND_1", "COMPARE_PCC_INFO", setValues, values, 30)}
                                            </td>
                                            <td>
                                                {getRadioBox("CMP_COND_2", "COMPARE_PCC_CONDITION", setValues, values, errors)}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                預警類型1
                                            </th>
                                            <td colSpan={2}>
                                                {getCheckBoxRadio("CMP_COND_3", "ALERT_TYPE_1", setValues, values, errors)}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                預警類型2
                                            </th>
                                            <td>
                                                {getCheckBoxRadio("CTRL_CHK_POINT_TYPE", "ALERT_TYPE_2_INFO", setValues, values, errors, 30)}
                                            </td>
                                            <td>
                                                {getCheckBoxRadio("CMP_COND_4", "ALERT_TYPE_2_CONDITION", setValues, values, errors)}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>是否使用界接資料</th>
                                            <td colSpan={2}>
                                                <Checkbox
                                                    checked={values.IS_USER_FTY_DATA}
                                                    value={values.IS_USER_FTY_DATA}
                                                    label={"是"}
                                                    onChange={(e) => {
                                                        setValues({ ...values, IS_USER_FTY_DATA: e.value })
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        {isRDEC &&
                                            <tr>
                                                <th>
                                                    管考備註
                                                </th>
                                                <td colSpan={2}>
                                                    <TextInput
                                                        name="AUDIT_OPINION"
                                                        value={values.AUDIT_OPINION}
                                                        onChange={handleChange}
                                                        onBlur={handleBlur}
                                                        style={{ width: "100%" }}
                                                    />
                                                </td>
                                            </tr>
                                        }
                                    </table>
                                </form>
                            );
                        }}
                    </Formik>
                    <h3 className="k-dialog-titlebar">自選欄位</h3>
                    <form>
                        <table>
                            {getOpetionFieldElement()}
                        </table>
                    </form>
                    <GotoTopBtn targetClass="UnitingQuery" />
                </PageContainer>
            }

            {
                showQryResult &&
                <UnitingQueryGrid
                    queryCondition={queryCondition}
                    selectedColumnKey={selectedColumnKey}
                    optionColumns={optionColumns}
                    setShowQryResult={setShowQryResult}
                    setQueryCondition={setQueryCondition}
                    setSelectedColumnKey={setSelectedColumnKey}
                />
            }
        </>
    )
}

export default UnitingQueryMain;