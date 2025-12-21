import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import ProjectFillBudgetExecService from './ProjectFillBudgetExecService';
import ProjectFillBudgetExecGrid from './ProjectFillBudgetExecGrid';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { showGlobalMessageBox, showGlobalConfirmBox } from "../../../Route/RootMiddleware";
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import { MultiSelect } from "@progress/kendo-react-dropdowns";
import { formatNumber } from '@telerik/kendo-intl';
import { Error } from '@progress/kendo-react-labels';
import { Formik } from 'formik';
import * as Yup from 'yup';
import { openProjectPrint } from '../../../Basic/CommonService';

export const ProjectFillBudgetExecMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                isRdecFun,
                showSomeBtn
            } = {
                projectNo: "",
                isRdecFun: "",
                showSomeBtn: null
            }
        }
    } = props;

    // 當期預算資料
    const [dataItem, setDataItem] = React.useState(ProjectFillBudgetExecService.bgtExecInitData);
    const [gridData, setGridData] = React.useState([]);
    // 選單資料
    const [ddlData, setDdlData] = React.useState({
        IPCBGTEXECFAILED: [],
        IPCBGTEXECFAILEDDUTY: [],
    });
    const failedDefaultDdl = React.useRef([]);
    const dutyDefaultDdl = React.useRef([]);
    const [isShowGrid, setIsShowGrid] = React.useState(false);
    const formRef = React.useRef(null);

    const loadData = async () => {
        await getDdlData();
        await loadProjectFillBudgetExec();
    }

    // 取得選單資料
    const getDdlData = async () => {
        let dropDowns = await ProjectFillBudgetExecService.getAllDdlData();
        if (dropDowns.length > 0) {
            setDdlData({
                // 原因 
                IPCBGTEXECFAILED: [...dropDowns[0]],
                // 責任歸屬
                IPCBGTEXECFAILEDDUTY: [...dropDowns[1]],
            });
            failedDefaultDdl.current = [...dropDowns[0]];
            dutyDefaultDdl.current = [...dropDowns[1]];
        }
    }

    // 取得計畫預算執行情形
    const loadProjectFillBudgetExec = async () => {
        SetMaskOnOff(true);
        let result = JSON.parse(JSON.stringify(ProjectFillBudgetExecService.initData));
        result = await ProjectFillBudgetExecService.getProjectFillBudgetExec(projectNo);
        // 計畫填報周期
        let cycleData = result.ProjectFillCycle;
        mapSelData(result);
        // 抓取當期填報週期資料
        let currentData = result.ProjectBudgetExecute.filter(x => x.EXEC_YEAR === cycleData.PROJECT_YEAR && x.EXEC_MONTH === cycleData.PROJECT_MONTH);
        setGridData(result.ProjectBudgetExecute);
        if (currentData.length === 0) {
            let newItem = { ...ProjectFillBudgetExecService.bgtExecInitData, EXEC_YEAR: cycleData.PROJECT_YEAR, EXEC_MONTH: cycleData.PROJECT_MONTH };
            setDataItem(newItem);
        }
        else {
            setDataItem(currentData[0]);
        }
        SetMaskOnOff(false);
    }

    /**
     * 塞入已選原因、責任歸屬資料
     * @param {*} result 
     * @returns 
     */
    const mapSelData = (result) => {
        result.ProjectBudgetExecute = result.ProjectBudgetExecute.map(x => {
            return {
                ...x,
                failedSelectedData: failedDefaultDdl.current.filter(i => {
                    if (x.FailedMappingData.map(y => y.SET_TYPE).includes(i.SET_TYPE)) {
                        return i;
                    }
                }),
                dutySelectedData: dutyDefaultDdl.current.filter(i => {
                    if (x.FailedDutyMappingData.map(y => y.SET_TYPE).includes(i.SET_TYPE)) {
                        return i;
                    }
                })
            }
        })
        return result;
    }

    // 更新grid
    const refreshGrid = async (seq) => {
        SetMaskOnOff(true);
        let result = await ProjectFillBudgetExecService.getProjectFillBudgetExec(projectNo);
        mapSelData(result);
        // 重塞存檔資料進DataItem
        let currentData = result.ProjectBudgetExecute.filter(x => x.SEQ === seq);
        setDataItem(currentData[0]);
        setGridData(result.ProjectBudgetExecute);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 多選下拉
     * @param {*} prop Formik參數
     * @param {*} ddl 下拉選單資料
     * @param {*} type 區隔 原因、責任歸屬
     * @returns 
     */
    const multiSelect = (prop, ddl, type) => {
        const { values, setValues, errors } = prop;
        const Value = type === "failed" ? values.failedSelectedData : values.dutySelectedData;
        return (
            <>
                <MultiSelect
                    popupSettings={
                        { className: "dropdown-text-size" }
                    }
                    placeholder="請選擇   "
                    data={ddl}
                    textField="SET_VALUE"
                    dataItemKey="SET_TYPE"
                    onChange={(e) => {
                        if (type === "failed") {
                            setValues({
                                ...values,
                                FailedMappingData: e.value.map(item => { return { SET_TYPE: item.SET_TYPE } }),
                                failedSelectedData: [...e.value]
                            })
                        }
                        else {
                            setValues({
                                ...values,
                                FailedDutyMappingData: e.value.map(item => { return { SET_TYPE: item.SET_TYPE } }),
                                dutySelectedData: [...e.value]
                            })
                        }
                    }}
                    value={Value}
                />
                <Error>{type === "failed" ? errors.FailedMappingData : errors.FailedDutyMappingData}</Error>
            </>
        )
    }

    /**
     * 累計執行情形 異動
     * @param {*} field 
     * @param {*} value 
     * @param {*} prop 
     */
    const gtChange = (field, value, prop) => {
        const { values, setValues } = prop;
        const GT_EXPANDED_BUDGET = field === "GT_EXPANDED_BUDGET" ? value : values.GT_EXPANDED_BUDGET;
        const GT_TOTAL = (field === "GT_ACT_BUDGET" ? value : values.GT_ACT_BUDGET) +
            (field === "GT_AP" ? value : values.GT_AP) +
            (field === "GT_BALANCE" ? value : values.GT_BALANCE);
        setValues(
            {
                ...values,
                [field]: value,
                GT_TOTAL: GT_TOTAL,
                GT_EXEC_RATE: GT_EXPANDED_BUDGET === 0 || GT_TOTAL === 0 ? 0 : (GT_TOTAL / GT_EXPANDED_BUDGET) * 100
            }
        );
    }

    /**
     * 本年度執行情形 異動
     * @param {*} field 
     * @param {*} value 
     * @param {*} prop 
     */
    const yearChange = (field, value, prop) => {
        const { values, setValues } = prop;
        const YEAR_EXEC_BUDGET = field === "YEAR_EXEC_BUDGET" ? value : values.YEAR_EXEC_BUDGET;
        const YEAR_BUDGET_ALLOCATED = field === "YEAR_BUDGET_ALLOCATED" ? value : values.YEAR_BUDGET_ALLOCATED;
        setValues(
            {
                ...values,
                [field]: value,
                YEAR_EXEC_RATE: YEAR_BUDGET_ALLOCATED === 0 || YEAR_EXEC_BUDGET === 0 ? 0 : YEAR_EXEC_BUDGET / YEAR_BUDGET_ALLOCATED * 100
            }
        );
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    //利用Ref把Formik的reset功能拉出來，以提供外部按鈕呼叫
    const cancelChanges = () => {
        if (formRef.current) {
            formRef.current.handleReset();
        }
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        // 累計預定支用(元)(a)：當累計實際支用(元)(b)、應付未付數(元)(c)、結餘數(元)(d)其中有一個不為0時就不可為0
        GT_EXPANDED_BUDGET: Yup.number().required("此欄位為必填").nullable()
            .test('GT_EXPANDED_BUDGET', '不可為零', function (item) {
                return item === 0 &&
                    (this.parent.GT_ACT_BUDGET !== 0 || this.parent.GT_AP !== 0 || this.parent.GT_BALANCE !== 0)
                    ? false : true;
            }),
        GT_ACT_BUDGET: Yup.number().required("此欄位為必填").nullable(),
        GT_AP: Yup.number().required("此欄位為必填").nullable(),
        GT_BALANCE: Yup.number().required("此欄位為必填").nullable(),
        YEAR_BUDGET_EXPANDED: Yup.number().required("此欄位為必填").nullable(),
        // 預算分配數(k)：當預算執行數(l)不為0時就不可為0
        YEAR_BUDGET_ALLOCATED: Yup.number().required("此欄位為必填").nullable()
            .test('YEAR_BUDGET_ALLOCATED', '不可為零', function (item) {
                return item === 0 && this.parent.YEAR_EXEC_BUDGET !== 0 ? false : true;
            }),
        YEAR_EXEC_BUDGET: Yup.number().required("此欄位為必填").nullable(),
        FailedMappingData: Yup.array()
            .test("FailedMappingData", '此欄位為必填', function (item) {
                return this.parent.YEAR_EXEC_RATE < 80 && this.parent.YEAR_BUDGET_ALLOCATED !== 0 && item.length === 0 ? false : true;
            }),
        FailedDutyMappingData: Yup.array()
            .test("FailedDutyMappingData", '此欄位為必填', function (item) {
                return this.parent.YEAR_EXEC_RATE < 80 && this.parent.YEAR_BUDGET_ALLOCATED !== 0 && item.length === 0 ? false : true;
            }),
        EXEC_RATE_FAILED_NOTE: Yup.string().nullable()
            .test('EXEC_RATE_FAILED_NOTE', '此欄位為必填', function (item) {
                return this.parent.YEAR_EXEC_RATE < 80 && this.parent.YEAR_BUDGET_ALLOCATED !== 0 && IsNullOrEmpty(item) ? false : true;
            }),
    });

    // 存檔檢查
    const checkData = async (data) => {
        const grExecRateStr = formatNumber(data.GT_EXEC_RATE, "n2");
        const yearExecRateStr = formatNumber(data.YEAR_EXEC_RATE, "n2");

        if (data.GT_EXEC_RATE > 999.99) {
            showGlobalMessageBox(`目前累計經費執行率超過${grExecRateStr}%，請輸入數值是否正確`);
        }
        else if (data.YEAR_EXEC_RATE > 999.99) {
            showGlobalMessageBox(`本年度執行情形率${yearExecRateStr}%，請輸入數值是否正確`);
        }
        else if (data.GT_EXEC_RATE > 100) {
            showGlobalConfirmBox(`目前累計經費執行率超過${grExecRateStr}%`, () => save(data));
        }
        else if (data.YEAR_EXEC_RATE > 100) {
            showGlobalConfirmBox(`本年度執行情形率${yearExecRateStr}%`, () => save(data));
        }
        else {
            save(data);
        }
    }

    // 存檔
    const save = async (data) => {
        data = { ...data, PROJECT_NO: projectNo };
        SetMaskOnOff(true);
        let saveResult = await ProjectFillBudgetExecService.saveProjectFillBudgetExec(data);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox("存檔成功", () => window.location.reload());
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    return (
        <>
            <CollapseBoardCard button={
                <>
                    {
                        showSomeBtn &&
                        <>
                            <Button type="button" title="存檔" onClick={() => handleSubmit()}>存檔</Button>
                            <Button type="button" title="取消" className="k-button-lighten" onClick={cancelChanges}>取消</Button>
                        </>
                    }
                    <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
                </>
            } title="預算執行情形" isFirstArea={true}>
                <div className='fn-buttons'>
                    <Button title={isShowGrid ? "隱藏預算執行情形彙總" : "顯示預算執行情形彙總"} onClick={() => {
                        setIsShowGrid(!isShowGrid);
                    }}>{isShowGrid ? "隱藏預算執行情形彙總" : "顯示預算執行情形彙總"}</Button>
                </div>
                <Formik
                    initialValues={dataItem}
                    onSubmit={checkData}
                    innerRef={formRef}
                    validationSchema={validateField}
                    //允許重複賦予初始值
                    enableReinitialize
                >
                    {prop => {
                        const {
                            values,
                            errors,
                            handleChange,
                            handleBlur,
                            setValues
                        } = prop;
                        return (
                            <form>
                                <table>
                                    <tr>
                                        <th colSpan={2}>累計執行情形</th>
                                    </tr>
                                    <tr>
                                        <th>填報期間</th>
                                        <td>{values.EXEC_YEAR}_{values.EXEC_MONTH}</td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">累計預定支用(元)(a)</th>
                                        <td>
                                            <NumericTextInput
                                                name="GT_EXPANDED_BUDGET"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.GT_EXPANDED_BUDGET}
                                                error={errors.GT_EXPANDED_BUDGET}
                                                onChange={(e) => {
                                                    gtChange("GT_EXPANDED_BUDGET", e.target.value, prop);
                                                }}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip title={"累計實際完成金額(元)(b+c)"} content={"累計實際完成金額：為實際已完成施做，不論是否已估驗計價之金額。"} withoutRedStar={true} />
                                        </th>
                                        <td>{formatNumber(values.GT_ACT_BUDGET + values.GT_AP, "n0")}</td>
                                    </tr>
                                    <tr>
                                        <th><CommonTooltip title={"累計各項經費支用(元)(b)"} content={"累計實際支用：為係指已完成估驗計價，且廠商已領取之累計金額，包含工程預付款及估驗計價保留款等 (代辦案件不含機關間之撥付金額)。"} /></th>
                                        <td>
                                            <NumericTextInput
                                                name="GT_ACT_BUDGET"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.GT_ACT_BUDGET}
                                                error={errors.GT_ACT_BUDGET}
                                                onChange={(e) => {
                                                    gtChange("GT_ACT_BUDGET", e.target.value, prop);
                                                }}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th><CommonTooltip title={"應付未付數(元)(c)"} content={"應付未付數：係指已施作及完成估驗之應付未付廠商款項(代辦案件不含機關間之撥付金額)。"} /></th>
                                        <td>
                                            <NumericTextInput
                                                name="GT_AP"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.GT_AP}
                                                error={errors.GT_AP}
                                                onChange={(e) => {
                                                    gtChange("GT_AP", e.target.value, prop);
                                                }}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th><CommonTooltip title={"結餘數(元)(d)"} content={"結餘數：係指於本年度辦理發包或工程節餘將於年度結束辦理繳庫，不再發包運用金額(代辦案件不含機關間之撥付金額)。"} /></th>
                                        <td>
                                            <NumericTextInput
                                                name="GT_BALANCE"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.GT_BALANCE}
                                                error={errors.GT_BALANCE}
                                                onChange={(e) => {
                                                    gtChange("GT_BALANCE", e.target.value, prop);
                                                }}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colSpan={2}>
                                            目前累計各項經費支用+應付未付數+結餘數(元) b+c+d：{formatNumber(values.GT_TOTAL, "n0")}
                                            <br />
                                            目前累計經費執行率 (b+c+d)/a：{formatNumber(values.GT_EXEC_RATE, "n2")}%
                                        </td>
                                    </tr>
                                </table>

                                <table>
                                    <tr>
                                        <th colSpan={2}>本年度執行情形</th>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">可支用預算數(j)</th>
                                        <td>
                                            <NumericTextInput
                                                name="YEAR_BUDGET_EXPANDED"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.YEAR_BUDGET_EXPANDED}
                                                error={errors.YEAR_BUDGET_EXPANDED}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">預算分配數(k)</th>
                                        <td>
                                            <NumericTextInput
                                                name="YEAR_BUDGET_ALLOCATED"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.YEAR_BUDGET_ALLOCATED}
                                                error={errors.YEAR_BUDGET_ALLOCATED}
                                                onChange={(e) => {
                                                    yearChange("YEAR_BUDGET_ALLOCATED", e.target.value, prop)
                                                }}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">預算執行數(l)</th>
                                        <td>
                                            <NumericTextInput
                                                name="YEAR_EXEC_BUDGET"
                                                format={'n0'}
                                                min={0}
                                                inputType={'text'}
                                                value={values.YEAR_EXEC_BUDGET}
                                                error={errors.YEAR_EXEC_BUDGET}
                                                onChange={(e) => {
                                                    yearChange("YEAR_EXEC_BUDGET", e.target.value, prop)
                                                }}
                                                onBlur={handleBlur}
                                                WithFormik={true}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>本年度執行情形率(m=l/k)</th>
                                        <td>{formatNumber(values.YEAR_EXEC_RATE, "n2")}%</td>
                                    </tr>
                                    {/* 本年度預算執行率未達80% 且 預算分配數(k) != 0 才出現 */}
                                    <tr style={values.YEAR_EXEC_RATE < 80 && values.YEAR_BUDGET_ALLOCATED !== 0 ? null : { display: 'none' }}>
                                        <td colSpan={2}>本年度預算執行率未達80%</td>
                                    </tr>
                                    <tr style={values.YEAR_EXEC_RATE < 80 && values.YEAR_BUDGET_ALLOCATED !== 0 ? null : { display: 'none' }}>
                                        <th>
                                            <CommonTooltip title={"原因"} content={"請勾選至少一項 預算執行率未達80%原因"} />
                                        </th>
                                        <td>{multiSelect(prop, ddlData.IPCBGTEXECFAILED, "failed")}</td>
                                    </tr>
                                    <tr style={values.YEAR_EXEC_RATE < 80 && values.YEAR_BUDGET_ALLOCATED !== 0 ? null : { display: 'none' }}>
                                        <th>
                                            <CommonTooltip title={"責任歸屬"} content={"請勾選至少一項"} />
                                        </th>
                                        <td>{multiSelect(prop, ddlData.IPCBGTEXECFAILEDDUTY, "failedDuty")}</td>
                                    </tr>
                                    <tr style={values.YEAR_EXEC_RATE < 80 && values.YEAR_BUDGET_ALLOCATED !== 0 ? null : { display: 'none' }}>
                                        <th>
                                            <CommonTooltip title={"說明"} content={"150字以內，請輸入 預算執行率未達80%-說明"} />
                                        </th>
                                        <td>
                                            <TextAreaInput
                                                name="EXEC_RATE_FAILED_NOTE"
                                                rows={5}
                                                maxlength={150}
                                                style={{ width: "100%" }}
                                                defaultValue={values.EXEC_RATE_FAILED_NOTE == null ? "" : values.EXEC_RATE_FAILED_NOTE}
                                                error={errors.EXEC_RATE_FAILED_NOTE}
                                                onBlur={(e) => {
                                                    setValues({
                                                        ...values,
                                                        EXEC_RATE_FAILED_NOTE: e.target.element.current.value
                                                    })
                                                }}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>
                                            <CommonTooltip title={"累計執行情形備註"} content={"內文請在150字以內"} withoutRedStar={true} />
                                        </th>
                                        <td>
                                            <TextAreaInput
                                                name="EXEC_NOTE"
                                                rows={5}
                                                maxlength={150}
                                                style={{ width: "100%" }}
                                                defaultValue={values.EXEC_NOTE == null ? "" : values.EXEC_NOTE}
                                                error={errors.EXEC_NOTE}
                                                onBlur={(e) => {
                                                    setValues({
                                                        ...values,
                                                        EXEC_NOTE: e.target.element.current.value
                                                    })
                                                }}
                                            />
                                        </td>
                                    </tr>
                                </table>
                            </form>
                        );
                    }}
                </Formik>

                {isShowGrid &&
                    <ProjectFillBudgetExecGrid
                        gridData={gridData}
                        setGridData={setGridData}
                        isRdecFun={isRdecFun}
                        ddlData={ddlData}
                        setDataItem={setDataItem}
                        loadData={loadProjectFillBudgetExec}
                    />
                }
            </CollapseBoardCard>
        </>
    )
}
export default ProjectFillBudgetExecMain;