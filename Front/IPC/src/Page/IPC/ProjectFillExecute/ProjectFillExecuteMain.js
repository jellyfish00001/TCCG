import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import CollapseBoardCard from "../../../Components/BoardCard/CollapseBoardCard";
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput';
import NumericTextInput from '../../../Components/Input/NumericTextInput';
import { showGlobalConfirmBox, showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { GetIsUserFtyData, openProjectPrint } from '../../../Basic/CommonService';
import CommonTooltip from "../../../Components/Tooltip/CommonTooltip";
import { Button } from "@progress/kendo-react-buttons";
import { getProjecFillExecute, saveProjecFillExecute } from "./ProjectFillExecuteService";
import ProjectFillExecuteGrid from "./ProjectFillExecuteGrid";

const ProjectFillExecuteMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                projectNo,
                showSomeBtn,
                isRdecFun,
                funRole
            } = {
                projectNo: null,
                showSomeBtn: null,
                isRdecFun: false,
                funRole: 0
            }
        }
    } = props;
    //#region 參數宣告
    const loadTimes = useRef(0);

    // 表格資料
    const [formData, setFormData] = useState({});

    // 是否顯示更多
    const [isReadMore, setIsReadMore] = useState({ state: false, text: "顯示更多" });

    const formRef = useRef(null);

    // 可否存檔 (若當期已送出或超過填報週期則不可存檔)
    const [canSave, setCanSave] = useState(false);

    // 是否為"工程類"
    const isEngineering = useRef(false);

    // 當期執行情形是否已送出
    const isSend = useRef(false);

    // 是否使用國發會界接資料
    const [isUserFtyData, setIsUserFtyData] = useState(false);
    //#endregion

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    /**
    * 存檔行為
    * @param {*} data 
    */
    const saveChanges = async (data) => {
        // 若當期已送出，則取消當期執行情形送出
        if (isSend.current && funRole === 0) {
            showGlobalConfirmBox("異動資料會產生落後原因分析，需再重新執行情形送出", async () => {
                data.CancelSend = true;
                await saveEvent(data);
            })
        } else {
            await saveEvent(data);
        }
    }

    /**
     * 存檔事件
     * @param {*} data 
     */
    const saveEvent = async (data) => {
        let errMsg = dataCheck(data);
        if (IsNullOrEmpty(errMsg)) {
            let saveResult = await saveProjecFillExecute(data);
            if (saveResult.success) {
                let isCompleteWork = saveResult.message == "False" ? "，請至檢核點完成日期確認竣工的實際完成日期是否正確。\n實際施工進度已達100，竣工實際日期需要填入。" : "";
                if (saveResult.data >= 80) {
                    await new Promise((resolve) => {
                        let simularityPercentage = saveResult.data == 100 ? 100 : saveResult.data.toFixed(2);
                        showGlobalMessageBox(`本月執行情形與上月雷同，請確認是否正確! 相似度達 : ${simularityPercentage}%`, resolve);
                    });
                }
                await new Promise((resolve) => {
                    showGlobalMessageBox("存檔成功" + isCompleteWork, () => {
                        window.location.reload();
                        resolve();
                    });
                });
            } else {
                showGlobalMessageBox(saveResult.message);
            }
        } else {
            showGlobalMessageBox(errMsg);
        }
    }

    // 資料檢核
    const dataCheck = (data) => {
        // 施工進度必填
        if (isEngineering.current && (IsNullOrEmpty(data.IPC_ACT_PRG) || IsNullOrEmpty(data.IPC_RES_PRG))) {
            return "施工進度必填";
        }
        return '';
    }

    // 載入資料
    const loadTableData = async (seq) => {
        SetMaskOnOff(true);
        let result = await getProjecFillExecute(projectNo, seq);
        setCanSave(result.CanSave);

        isEngineering.current = result.IsEngStartWork;
        isSend.current = result.IS_SEND;
        //施工方式為"工程類"，且辦理開工的實際完成日期已填寫
        if (result.IsEngStartWork) {
            isEngineering.current = result.IsEngStartWork;
            result = setDefaultProgress(result)
        }
        setFormData({ ...result, loadTimes: loadTimes.current });
        loadTimes.current = loadTimes.current + 1;
        SetMaskOnOff(false);
    }

    // 已開工，則施工進度給預設值0
    const setDefaultProgress = (data) => {
        return {
            ...data, IPC_RES_PRG: data.IPC_RES_PRG ?? 0,
            IPC_ACT_PRG: data.IPC_ACT_PRG ?? 0
        }
    }

    // 累計預定施工進度 & 累計實際施工進度
    const PrgCell = (values, handleChange, handleBlur) => {
        if (!isEngineering.current) {
            return <></>
        }
        else if (values.IsCompletedWork) {
            return (
                <>
                    <tr>
                        <th className="addRedStar">累計預定施工進度%</th>
                        <td>{values.IPC_RES_PRG}</td>
                    </tr>
                    <tr>
                        <th className="addRedStar">累計實際施工進度%</th>
                        <td>{values.IPC_ACT_PRG}</td>
                    </tr>
                </>
            )
        }
        else {
            return (
                <>
                    <tr>
                        <th className="addRedStar">
                            累計預定施工進度%
                        </th>
                        <td>
                            <NumericTextInput
                                name="IPC_RES_PRG"
                                min={0}
                                max={100}
                                inputType={'text'}
                                value={values.IPC_RES_PRG}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                WithFormik={true}
                            />
                        </td>
                    </tr>
                    <tr>
                        <th className="addRedStar">
                            累計實際施工進度%
                        </th>
                        <td>
                            <NumericTextInput
                                name="IPC_ACT_PRG"
                                min={0}
                                max={100}
                                inputType={'text'}
                                value={values.IPC_ACT_PRG}
                                onChange={handleChange}
                                onBlur={handleBlur}
                                WithFormik={true}
                            />
                        </td>
                    </tr>
                </>
            )
        }
    }

    const loadData = async () => {
        SetMaskOnOff(true);
        // 載入表格資料
        await loadTableData("");

        setIsUserFtyData(await GetIsUserFtyData(projectNo));

        SetMaskOnOff(false);
    }

    useEffect(() => {
        loadData();
    }, [])

    return (
        <CollapseBoardCard button={
            <>
                {
                    (isRdecFun || (showSomeBtn && canSave && !isUserFtyData)) &&
                    <Button title="存檔" onClick={() => handleSubmit()}>存檔</Button>
                }
                <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
            </>}
            title="每月辦理情形" isFirstArea={true}>
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
                        handleChange,
                        handleBlur
                    } = props;
                    return (
                        <form >
                            <table>
                                <tr>
                                    <th>
                                        填報期間
                                    </th>
                                    <td>
                                        {IsNullOrEmpty(values.YEAR) || IsNullOrEmpty(values.MONTH) ? "" :
                                            `${values.YEAR}_${values.MONTH}`}
                                    </td>
                                </tr>
                                {PrgCell(values, handleChange, handleBlur)}
                                <tr>
                                    <th className="addRedStar">執行情形</th>
                                    <td>
                                        <PureHtmlTextAreaInput
                                            rows={6}
                                            maxlength={500}
                                            name='EXECUTE_CONDITION'
                                            value={values.EXECUTE_CONDITION}
                                            onChange={handleChange}
                                            onBlur={handleBlur}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th>
                                        <CommonTooltip title={"需協辦事項"} content={"無須協辦事項請填「無」。"} />
                                    </th>
                                    <td>
                                        <PureHtmlTextAreaInput
                                            rows={6}
                                            maxlength={500}
                                            name='ASSISTANT_ITEM'
                                            value={values.ASSISTANT_ITEM}
                                            onChange={handleChange}
                                            onBlur={handleBlur}
                                        />
                                    </td>
                                </tr>
                            </table>
                        </form>
                    );
                }}
            </Formik>

            <ProjectFillExecuteGrid
                projectNo={projectNo}
                isEngineering={isEngineering.current}
                state={isReadMore.state}
                loadTableData={loadTableData}
            />

        </CollapseBoardCard>
    )
}

export default ProjectFillExecuteMain;