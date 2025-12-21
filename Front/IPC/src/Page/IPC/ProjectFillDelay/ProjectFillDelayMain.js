
import React from "react";
import { Formik } from 'formik';
import CollapseBoardCard from "../../../Components/BoardCard/CollapseBoardCard";
import { showGlobalConfirmBox, showGlobalMessageBox } from "../../../Route/RootMiddleware";
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { DropDownListWithValue } from "../../../Components/Dropdowns/DropDownListWithValue";
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { GetIsUserFtyData, openProjectPrint } from '../../../Basic/CommonService';
import { Button } from "@progress/kendo-react-buttons";
import { getAllDropDowns, getDelayClass, getProjectFillDelay, saveProjectFillDelay } from "./ProjectFillDelayService";
import ProjectFillDelayGrid from "./ProjectFillDelayGrid";

const ProjectFillDelayMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                projectNo,
                showSomeBtn,
                showBtnByProjectStatus,
                isRdecFun,
                funRole
            } = {
                projectNo: null,
                showSomeBtn: null,
                showBtnByProjectStatus: null,
                isRdecFun: null,
                funRole: 0
            }
        }
    } = props;

    //#region 參數宣告
    const loadTimes = React.useRef(0);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(
        {
            // 落後類別 
            DELAY_TYPE: [],
            // 責任歸屬
            DELAY_RESPON: []
        });

    // 落後項目 
    const [delayClassDdl, setDelayClassDdl] = React.useState(
        [{
            DELAY_CLASS_SUB_ITEM: "請選擇", DELAY_CLASS_SUB_ID: ""
        }])

    // 表格資料
    const [formData, setFormData] = React.useState({});

    // 是否顯示更多
    const [isReadMore, setIsReadMore] = React.useState({ state: false, text: "顯示更多" });

    const formRef = React.useRef(null);

    // 可否存檔 (若當期已送出或超過填報週期則不可存檔)
    const [canSave, setCanSave] = React.useState(false);

    // 是否使用國發會界接資料
    const [isUserFtyData, setIsUserFtyData] = React.useState(false);
    //#endregion

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    /**
    * 存檔
    * @param {*} data 
    */
    const saveChanges = async (data) => {
        let saveData = {
            ...data,
            DEADLINES: FormatDate(data.DEADLINES, 'YYYY-MM-DD')
        };

        if (data.IS_SEND && funRole === 0) {
            showGlobalConfirmBox("異動資料會產生落後原因分析，需再重新執行情形送出", async () => {
                await saveEvent(saveData, true);
            })
        } else {
            await saveEvent(saveData);
        }
    }

    const saveEvent = async (saveData, cancelSend = false) => {
        let saveResult = await saveProjectFillDelay({ ...saveData, CancelSend: cancelSend });

        if (saveResult.success) {
            showGlobalMessageBox("存檔成功", () => {
                window.location.reload();
            });
        } else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    /**
     * 載入資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        // 載入表格資料
        await loadTableData("");
        // 取得下拉選單
        await getDdlData();

        setIsUserFtyData(await GetIsUserFtyData(projectNo));

        SetMaskOnOff(false);
    }

    /**
     * 取得下拉選單
     */
    const getDdlData = async () => {
        let dropDowns = await getAllDropDowns();
        if (dropDowns.length > 0) {
            setDdlData({
                DELAY_TYPE: [...dropDowns[0]],
                DELAY_RESPON: [...dropDowns[1]]
            })
        }
    }

    /**
     * 載入表格資料
     */
    const loadTableData = async (seq) => {
        SetMaskOnOff(true);
        let result = await getProjectFillDelay(projectNo, seq, isRdecFun);
        setCanSave(result.CanSave);
        if (!IsNullOrEmpty(result.DELAY_CLASS_C)) {
            // 取得落後項目ddl data
            getDelayClassData(result.DELAY_CLASS_C)
        }

        setFormData({ ...result, loadTimes: loadTimes.current });
        loadTimes.current = loadTimes.current + 1;
        SetMaskOnOff(false);
    }

    /**
    * 取得落後項目ddl data
    * @param {*} value 
    */
    const getDelayClassData = async (value) => {
        let data = await getDelayClass(value);
        setDelayClassDdl(data);
    }

    /**
     * 落後類別 change event
     * @param {*} props 
     * @param {*} value 
     */
    const delayTypeChangeEvent = async (props, value) => {
        let { values, setValues } = props;
        getDelayClassData(value);
        setValues({
            ...values,
            DELAY_CLASS_C: value,
            DELAY_SUBCLASS_C: ""
        });
    }

    React.useEffect(() => {
        loadData()
    }, [])

    return (
        <CollapseBoardCard
            button={
                <>
                    {
                        (formData.SEQ > 0 && (isRdecFun || (showSomeBtn && canSave && !isUserFtyData))) &&
                        <Button title="存檔" onClick={() => handleSubmit()}>存檔</Button>
                    }
                    <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
                </>}
            title="落後原因分析" isFirstArea={true}>
            <div className='fn-buttons'>
                <Button title={isReadMore.text}
                    onClick={() => {
                        setIsReadMore({
                            state: !isReadMore.state,
                            text: !isReadMore.state ? "隱藏歷史資料" : "顯示更多"
                        })
                    }}>
                    {isReadMore.text}
                </Button>
            </div>
            {
                formData.SEQ > 0 ?
                    <Formik
                        initialValues={formData}
                        onSubmit={(data) => saveChanges(data)}
                        innerRef={formRef}
                        //允許重複賦予初始值
                        enableReinitialize
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleChange,
                                handleBlur,
                                setValues
                            } = props;
                            return (
                                <form >
                                    <table>
                                        <tr>
                                            <th>
                                                填報期間
                                            </th>
                                            <td>
                                                {`${values.DATA_YEAR}_${values.DATA_MONTH}`}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                落後類別
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    data={ddlData.DELAY_TYPE}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    value={values.DELAY_CLASS_C ?? ""}
                                                    error={errors.DELAY_CLASS_C}
                                                    onChange={(e) => delayTypeChangeEvent(props, e.target.value)}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                落後項目
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    data={delayClassDdl}
                                                    textField="DELAY_CLASS_SUB_ITEM"
                                                    dataItemKey="DELAY_CLASS_SUB_ID"
                                                    value={values.DELAY_SUBCLASS_C ?? ""}
                                                    error={errors.DELAY_SUBCLASS_C}
                                                    onChange={(e) => {
                                                        setValues({ ...values, DELAY_SUBCLASS_C: e.target.value });
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                責任歸屬
                                            </th>
                                            <td>
                                                <DropDownListWithValue
                                                    data={ddlData.DELAY_RESPON}
                                                    textField="SET_VALUE"
                                                    dataItemKey="SET_TYPE"
                                                    value={values.DELAY_RESPON ?? ""}
                                                    error={errors.DELAY_RESPON}
                                                    onChange={(e) => {
                                                        setValues({ ...values, DELAY_RESPON: e.target.value });
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                落後原因
                                            </th>
                                            <td>
                                                <PureHtmlTextAreaInput
                                                    rows={6}
                                                    name='DELAY_CAUSAL'
                                                    value={values.DELAY_CAUSAL}
                                                    error={errors.DELAY_CAUSAL}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                解決對策
                                            </th>
                                            <td>
                                                <PureHtmlTextAreaInput
                                                    rows={6}
                                                    name='SOLUTION'
                                                    value={values.SOLUTION}
                                                    error={errors.SOLUTION}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                需協辦事項
                                            </th>
                                            <td>
                                                <PureHtmlTextAreaInput
                                                    rows={6}
                                                    name='COORDINATION'
                                                    value={values.COORDINATION}
                                                    error={errors.COORDINATION}
                                                    onChange={handleChange}
                                                    onBlur={handleBlur}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th className="addRedStar">
                                                改進完成期限
                                            </th>
                                            <td>
                                                <TwDatePicker
                                                    name="DEADLINES"
                                                    format={"yyy/MM/dd"}
                                                    onChange={handleChange}
                                                    value={values.DEADLINES}
                                                    error={errors.DEADLINES}
                                                />
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                            );
                        }}
                    </Formik> :
                    <div>目前填報週期無落後原因資料</div>
            }

            <ProjectFillDelayGrid
                projectNo={projectNo}
                loadTableData={loadTableData}
                isRdecFun={isRdecFun}
                showBtnByProjectStatus={showBtnByProjectStatus}
                state={isReadMore.state}
            />
        </CollapseBoardCard>
    );
}

export default ProjectFillDelayMain;