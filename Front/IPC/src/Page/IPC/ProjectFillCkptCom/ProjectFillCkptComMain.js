import React, { useState, useEffect, useRef } from 'react';
import * as Yup from 'yup';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { GetHistory } from '../../../Basic/BasicData';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { getCurrentCycleData, openProjectPrint, CheckIsRDECRole } from '../../../Basic/CommonService';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { Button } from '@progress/kendo-react-buttons';
import { getPageData, saveProjectFillCkptCom } from './ProjectFillCkptComService';
import ProjectFillCkptComGrid from './ProjectFillCkptComGrid';
import ProjectEngSystemForm from './ProjectEngSystemForm';
import ProjectFillContactForm from './ProjectFillContactForm';
import { ProjectScheduleWindow } from '../ProjectScheduleRPT/ProjectScheduleWindow';

export const ProjectFillCkptComMain = (
    {
        location: { state },
        location: {
            state: {
                projectNo,
                showSomeBtn,
                cycleData,
                isRdecFun,
                funRole
            }
        }
    }
) => {

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => GetHistory().push('/Home'));
    }

    // 一天的毫秒數(用於計算所需月)
    const oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds

    // 聯繫資訊資料
    const [contactData, setContactData] = useState({});
    // 聯繫資訊存檔用(避免畫面不必要rerendering)
    const contactDataForSave = useRef({});
    // 聯繫資訊錯誤訊息
    const contactDataErrors = useRef({})
    // 檢核點資料
    const [checkpointData, setCheckpointData] = useState([]);
    // 檢核點資料存檔用(避免畫面不必要rerendering)
    const checkpointDataForSave = useRef([]);
    // 工程資料
    const [engFormData, setEngFormData] = useState({})
    // 工程資料存檔用
    const engFormDataForSave = useRef({});
    // 已上傳工程預定進度表
    const [fileList, setFileList] = useState([]);
    // 辦理開工檢核點實際完成日期是否有填
    const [isStartWork, setIsStartWork] = useState(false);
    //是否為本府執行案件
    const [isTycgProject, setIsTycgProject] = useState(false);
    // 是否辦理開工(From DB ，不會隨者前端異動而異動)
    const isStartWorkFromDB = useRef(false);
    // 是否使用國發會介接資料
    const [isUserFtyData, setIsUserFtyData] = useState(false)
    // 是否關聯工程會標案
    const [isAssociatePCC, setIsAssociatePCC] = useState(false);
    // 執行方式是否為工程類
    const [isEngineering, setIsEngineering] = useState(false);
    // 可否存檔 (若當期已送出或超過填報週期則不可存檔)
    const [canSave, setCanSave] = useState(true);
    // 當期執行情形是否已送出
    const isSend = useRef(false);
    // 控制確認介接工程會標案管理系統視窗 visibility
    const [isInterfacingSysConfirmVisible, setInterfacingSysConfirmVisible] = useState(false);
    // 辦理驗收檢核點實際完成日期是否有填
    const [isEndWork, setIsEndWork] = useState(false);
    // 進度甘特圖window reference
    const scheduleWindowRef = useRef();


    // 取得頁面所有區塊資料
    const initPageData = async (isOpenInterfacingSysConfirm = false) => {
        SetMaskOnOff(true);
        let result = await getPageData(projectNo);
        let cycleData = await getCurrentCycleData();
        if (result) {
            // 可否存檔
            setCanSave(result.CanSave);

            isSend.current = result.IS_SEND;
            //是否為本府執行案件
            setIsTycgProject(result.IS_TYCG_PROJECT)
            // 設定是否為工程類
            setIsEngineering(result.CP_KIND === '0')
            // 設定聯繫資訊資料
            setContactData({ ...result });
            contactDataForSave.current = { ...result };
            // 設定檢核點資料
            if (result.CustomChkItemModels.length > 0) {
                // 判斷是否開工
                let isStartWork = result.CustomChkItemModels.find(x => x.CTRL_POINT === 'A' && x.ACTUAL_ENDDATE != null)
                if (isStartWork) {
                    isStartWorkFromDB.current = true;
                    setIsStartWork(true)
                }
                let isEndWork = result.CustomChkItemModels.find(x => x.PROGRESS === 100 && x.ACTUAL_ENDDATE != null)
                if (isEndWork) {
                    setIsEndWork(true)
                }

                let chkptGridData = changeCheckItemDateModel([...result.CustomChkItemModels]);

                // 非管考權限才有限制
                if (!isRdecFun) {
                    let endWork = chkptGridData.find(x => x.CTRL_POINT === "C");
                    chkptGridData.map(x => {
                        // 如果是以界接資料填報，檢核點全部不能填
                        if (result.IS_USER_FTY_DATA === true) {
                            x.disabled = true;
                        }
                        // 要把比 CTRL_POINT=C 的 CHECKITEM_SEQ 大的資料都不能填實際完成日期
                        else if (endWork !== undefined && x.CHECKITEM_SEQ > endWork.CHECKITEM_SEQ) {
                            x.disabled = true;
                        }
                        else if (!IsNullOrEmpty(x.MDF_DATE)) {
                            // tYY/MM/DD to DateTime
                            let mdfDate = new Date(`${Number(x.MDF_DATE.substr(0, 3)) + 1911}${x.MDF_DATE.substr(3)}`);
                            x.disabled = mdfDate < new Date(cycleData.FILL_START_DATE);
                        }
                    })
                }
                setCheckpointData([...chkptGridData]);
            }
            // 是否使用國發會介接資料
            setIsUserFtyData(result.IS_USER_FTY_DATA ?? false);
            // 是否關聯工程會
            setIsAssociatePCC(!IsNullOrEmpty(result.PCC_PROJECT_UID));

            // 設定工程資料
            setEngFormData({ ...result });
            engFormDataForSave.current = {
                file: null,
                formData: { ...result }
            }
            // 設定已上傳檔案資料
            if (result.FileModels.length > 0) {
                result.FileModels.map(x => x.UploadDate = IsNullOrEmpty(x.CRT_DATE) ? "" : FormatDate(x.CRT_DATE, 'tYY/MM/DD'));
                setFileList([...result.FileModels]);
            }

            setInterfacingSysConfirmVisible(isOpenInterfacingSysConfirm);
        }
        SetMaskOnOff(false);
    }

    /**
     * 物件轉換，轉成可存檔Model對應欄位
     * @param {*} dataList 待轉換資料
     * @returns 
     */
    const changeCheckItemDateModel = (dataList) => {
        return dataList.map((d, index) => {
            let estimateEndDate = IsNullOrEmpty(d.ESTIMATED_ENDDATE) ? new Date() : new Date(d.ESTIMATED_ENDDATE);
            let actualEndDate = IsNullOrEmpty(d.ACTUAL_ENDDATE) ? "" : new Date(d.ACTUAL_ENDDATE);
            // 所需時間(月)
            let diffDays = 0;
            let diffMonth = 0;
            // 實際完成日期時間差(月)
            let diffMonthAct = 0;

            if (index > 0) {
                let previousDate = IsNullOrEmpty(dataList[index - 1].ESTIMATED_ENDDATE) ? new Date() : dataList[index - 1].ESTIMATED_ENDDATE;
                let currDate = estimateEndDate;

                let prevDateAct = IsNullOrEmpty(dataList[index - 1].ACTUAL_ENDDATE) ? '' : dataList[index - 1].ACTUAL_ENDDATE;
                let currDateAct = actualEndDate;

                diffDays = Math.round(Math.abs((new Date(previousDate) - new Date(currDate)) / oneDay));
                diffMonth = Math.round((diffDays / 30) * 10) / 10

                if (!IsNullOrEmpty(prevDateAct) && !IsNullOrEmpty(currDateAct)) {
                    let isNegative = new Date(prevDateAct) > new Date(currDateAct) ? '-' : '';
                    diffMonthAct = isNegative + Math.round(((Math.round(Math.abs((new Date(prevDateAct) - new Date(currDateAct)) / oneDay))) / 30) * 10) / 10;
                }
            }

            return {
                SEQ: d.SEQ,
                PROJECT_NO: projectNo,
                CHECKITEM_SEQ: d.CHECKITEM_SEQ,
                CHECKITEM_NAME: d.CHECKITEM_NAME,
                PROGRESS: d.PROGRESS,
                ESTIMATED_ENDDATE: FormatDate(estimateEndDate, 'tYY/MM/DD'),
                ESTIMATED_ENDDATE_DATE: estimateEndDate,
                PCC_ESTIMATED_ENDDATE: IsNullOrEmpty(d.PCC_ESTIMATED_ENDDATE) ? '' : FormatDate(d.PCC_ESTIMATED_ENDDATE, 'tYY/MM/DD'),
                IS_DELAY: 0,
                diffMonth: diffMonth,
                ACTUAL_ENDDATE: actualEndDate,
                ACTUAL_ENDDATE_OLD: actualEndDate,
                PCC_ACTUAL_ENDDATE: IsNullOrEmpty(d.PCC_ACTUAL_ENDDATE) ? "" : FormatDate(d.PCC_ACTUAL_ENDDATE, 'tYY/MM/DD'),
                MDF_DATE: IsNullOrEmpty(d.ACTUAL_ENDDATE) || IsNullOrEmpty(d.MDF_DATE) ? '' : FormatDate(d.MDF_DATE, 'tYY/MM/DD'),
                diffMonthActual: diffMonthAct,
                CTRL_POINT: d.CTRL_POINT
            }
        })
    }

    // 計畫聯繫資訊異動
    const contactDataChange = async (contactData, errors) => {
        contactDataForSave.current = contactData;
        contactDataErrors.current = errors
        setIsTycgProject(contactData.IS_TYCG_PROJECT);
    }

    // 檢核點資料異動
    const gridChange = async (gridData) => {
        checkpointDataForSave.current = gridData;
    }

    // 工程資料異動
    const engSystemFormChange = async (engData) => {
        engFormDataForSave.current = engData;
    }

    // 存檔
    const save = async () => {
        // 若當期已執行情形送出，需檢核是否會產生落後原因
        if (isSend.current && funRole === 0) {
            showGlobalConfirmBox("異動資料會產生落後原因分析，需再重新執行情形送出", async () => {
                await saveEvent(true);
            })
        } else {
            await saveEvent();
        }
    }

    // 存檔事件
    const saveEvent = async (cancelSend = false) => {
        let emails = !IsNullOrEmpty(contactDataForSave.current.REAL_EMAIL)
            ? contactDataForSave.current.REAL_EMAIL.split(';')
            : [];
        let isSuccess = true;
        for (let index = 0; index < emails.length; index++) {
            const email = emails[index];
            let isValid = await validateField.isValid({ REAL_EMAIL: email });
            if (!isValid) {
                isSuccess = isSuccess ? isValid : false;
            }
        }
        if (!isSuccess) {
            showGlobalMessageBox("E-mail格式不正確")
            return;
        }

        let requestBody = {
            model: {
                PROJECT_NO: projectNo,
                CONTRACT_FINISH_DATE: engFormDataForSave.current.formData.CONTRACT_FINISH_DATE,
                REAL_CONTACT: contactDataForSave.current.REAL_CONTACT,
                REAL_TEL: contactDataForSave.current.REAL_TEL,
                REAL_EMAIL: contactDataForSave.current.REAL_EMAIL,
                // 本府執行案件為否,清空聯繫資訊
                PCC_PROJECT_UID: contactDataForSave.current.IS_TYCG_PROJECT == 0 ? "" : contactDataForSave.current.PCC_PROJECT_UID,
                PCC_PROJECT_NO: contactDataForSave.current.IS_TYCG_PROJECT == 0 ? "" : contactDataForSave.current.PCC_PROJECT_NO,
                PCC_PROJECT_NAME: contactDataForSave.current.IS_TYCG_PROJECT == 0 ? "" : contactDataForSave.current.PCC_PROJECT_NAME,
                FACTORY_CONTACT: contactDataForSave.current.IS_TYCG_PROJECT == 0 ? "" : contactDataForSave.current.FACTORY_CONTACT,
                FACTORY_TEL: contactDataForSave.current.IS_TYCG_PROJECT == 0 ? "" : contactDataForSave.current.FACTORY_TEL,
                CustomChkItemModels: checkpointDataForSave.current.filter(x => x.ACTUAL_ENDDATE !== ''),
                IS_USER_FTY_DATA: contactDataForSave.current.IS_TYCG_PROJECT == 0 ? false : contactDataForSave.current.IS_USER_FTY_DATA,
                IS_TYCG_PROJECT: contactDataForSave.current.IS_TYCG_PROJECT,
                PROCUREMENT_AMT: engFormDataForSave.current.formData.PROCUREMENT_AMT,
                TENDER_AWARDING_AMT: engFormDataForSave.current.formData.TENDER_AWARDING_AMT,
                // 刪除竣工資料；原本辦理開工DB有資料，使用者移除辦理開工實際完成日期，須將竣工資料移除
                DeleteEngData: isStartWorkFromDB.current === true && !isStartWork,
                CancelSend: cancelSend,
                RemovedFileIds: engFormDataForSave.current.removedFileIds
            },
            file: engFormDataForSave.current.file
        }
        SetMaskOnOff(true);
        let saveResult = await saveProjectFillCkptCom(requestBody);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                window.location.reload();
            })
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    // 聯繫資訊欄位驗證
    const validateField = Yup.object().shape({
        REAL_EMAIL: Yup.string().nullable().email("E-mail格式不正確")
    });

    // 取消
    const cancel = async () => {
        initPageData();
    }

    // 預覽列印
    const preview = async () => {
        openProjectPrint(state);
    }

    useEffect(() => {
        initPageData();
    }, [])

    // 清除開工實際完成日期
    const clearStartWorkDate = () => {
        setContactData({
            ...contactData,
            PCC_PROJECT_UID: "",
            PCC_PROJECT_NO: "",
            FACTORY_CONTACT: null,
            FACTORY_TEL: null
        });
        contactDataForSave.current.PCC_PROJECT_UID = "";
        contactDataForSave.current.PCC_PROJECT_NO = "";
        contactDataForSave.current.FACTORY_CONTACT = null;
        contactDataForSave.current.FACTORY_TEL = null;
    }

    return (
        <>
            {state &&
                <div>
                    <div className='fn-buttons fixed-buttons full-fixed-buttons'>
                        {
                            (isRdecFun || (showSomeBtn && canSave)) &&
                            <>
                                <Button type='button' onClick={save}>存檔</Button>
                                <Button type='button' className="k-button-lighten" onClick={cancel}>取消</Button>
                            </>
                        }
                        <Button type='button' className='k-button-lighten' onClick={preview}>預覽列印</Button>
                        <Button type='button' className='k-button-lighten' onClick={() => { scheduleWindowRef.current.open() }}>進度甘特圖</Button>
                    </div>
                    {/* 聯繫資訊 */}
                    <ProjectFillContactForm
                        PROJECT_NO={projectNo}
                        data={contactData}
                        contactDataChange={contactDataChange}
                        isAssociate={isAssociatePCC}
                        isEngineering={isEngineering}
                        isRdecFun={isRdecFun}
                    />
                    {/* 檢核點完成日期 */}
                    <CollapseBoardCard title='檢核點完成日期' titleStyle={{ 'margin-top': '0px' }}>
                        <ProjectFillCkptComGrid
                            PROJECT_NO={projectNo}
                            data={checkpointData}
                            gridChange={gridChange}
                            startWork={(isStartWork) => { setIsStartWork(isStartWork) }}
                            isAssociate={isAssociatePCC}
                            isUserFtyData={isUserFtyData}
                            reloadData={initPageData}
                            isEngineering={isEngineering}
                            isStartWork={isStartWork}
                            isTycgProject={isTycgProject}
                            clearStartWorkDate={clearStartWorkDate}
                            isInterfacingSysConfirmVisible={isInterfacingSysConfirmVisible}
                            setInterfacingSysConfirmVisible={setInterfacingSysConfirmVisible}
                            canSave={canSave}
                            showSomeBtn={showSomeBtn}
                            isEndWork={isEndWork}
                            isRdecFun={isRdecFun}
                        />
                        {/* 總/分月期程調整 & 工程進度表 區塊 */}
                        <ProjectEngSystemForm
                            data={engFormData}
                            engSystemFormChange={engSystemFormChange}
                            fileList={fileList}
                            isEngineering={isEngineering}
                            isStartWork={isStartWork}
                            isRdecFun={isRdecFun}
                        />
                    </CollapseBoardCard>
                    <ProjectScheduleWindow
                        projectNo={projectNo}
                        checkedItems={['3', '5']}
                        projectAwStatus={contactData.PROJECT_AW_STATUS}
                        ref={scheduleWindowRef}
                    />
                </div>

            }
        </>
    );
}
export default ProjectFillCkptComMain