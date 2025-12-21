import React, { useState, useEffect, useRef } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import Table from '../../../Css/custom/Table.module.css';
import { loadCheckPoint, loadFormData, loadCusItem, saveData, checkDataValid, changeCheckItemDateModel } from './ProjectFillCheckPointService'
import { Formik } from "formik";
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { SetMaskOnOff, FormatDate, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { GetSetParam, downProjectAttachment, openProjectPrint } from '../../../Basic/CommonService';
import ProjectFillCheckPointGrid from './ProjectFillCheckPointGrid';
import CommonTooltip from '../../../Components/Tooltip/CommonTooltip';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import CacheLoader from '../../../Basic/CacheLoader';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput';
import { adjustHistory } from '../ProjectAdjuctHistory/AdjustHistoryService';

export const ProjectFillCheckPointMain = (props) => {

    const {
        type,
        location: {
            state
        },
        location: {
            state: {
                projectNo,
                projAdjId,
                showSomeBtn
            } = {
                projectNo: null,
                projAdjId: null,
                showSomeBtn: false
            }
        }
    } = props;


    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    // #region 下拉清單資料
    // 下拉資料
    const [cpKindDropDown, setCpKindDropDown] = useState([]);
    const [checkpointDropDown, setCheckpointDropDown] = useState([]);
    // 下拉選單值
    const [cpKind, setCpKind] = useState("")
    const [checkpoint, setCheckpoint] = useState("")
    // 計劃的執行方式
    const [projRunWayC, setProjRunWayC] = useState("");
    //下拉預設執行方式類別
    const defaultCpKind = useRef("")
    // #endregion

    // Grid 資料
    const [gridData, setGridData] = useState([]);
    const gridSaveData = useRef([]);
    // 表格資料
    const [formData, setFormData] = useState({});
    // 預設資料
    const defaultData = useRef({})
    const formRef = useRef(null);
    // 預定完成期限
    const [estimatedDate, setEstimatedDate] = useState("")
    // 表格資料是否讀取完成
    const [isFormReady, setIsFormReady] = useState(false);

    const loadTimes = React.useRef(0);

    // ------------檔案 (for 期程調整)
    let fileType = 0; // 1: 工程竣工報告表或函報竣工文件、2: 核章版工程預定進度網圖
    const [files, setFiles] = useState([]);
    const defaultFiles = useRef([]); // 預設檔案
    // 檔案上傳異動相關資訊
    const editedFiles = React.useRef([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = React.useState(false);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }
    // 上傳所需參數
    let paramFiles = {
        files: files,
        editedFiles: editedFiles,
        setFiles: setFiles,
        setIsDataChange,
        multiple: true,
        downFile
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(paramFiles);

    /**
     * formik內的datepicker onChange事件使用
     * @param {*} e event
     * @param {*} setValues formik setValues 方法 
     * @param {*} values formik values 物件
     */
    const onFormDatePickerChange = (e, setValues, values) => {
        let newObj = {};
        newObj[e.target.name] = e.target.value;//FormatDate(e.target.value, 'YYYY-MM-DD');
        setValues({ ...values, ...newObj });
    }

    // 取得下拉清單
    const initDropDowns = async () => {
        // 取得執行方式類別下拉選單
        let cpKindData = await GetSetParam('CP_KIND', '');
        if (cpKindData.length > 0) {
            setCpKindDropDown([...cpKindData])
            let defaultCpkind = cpKindData[0].SET_TYPE;
            defaultCpKind.current = defaultCpkind;
        }
    }

    // 取得頁面資料
    const initPageData = async () => {
        SetMaskOnOff(true);
        await initDropDowns();
        initFormData();
        SetMaskOnOff(false);
    }

    // 取得執行方式下拉選單
    const loadCheckpointDropDown = async (cpKind) => {
        let checkpointData = await loadCheckPoint(cpKind, true);
        // 清空執行方式值
        if (isFormReady) {
            setCheckpoint("");
        }
        if (checkpointData.length > 0) {
            setCheckpointDropDown([...checkpointData]);
        }
    }

    // 查詢表格資料資料
    const initFormData = async () => {
        SetMaskOnOff(true);
        // 若為調整，傳 RPOJ_ADJ_ID
        let data = await loadFormData(type === "adjust" ? projAdjId : projectNo, type);
        if (data != null) {
            // 設定執行方式類別
            let cpKind = "";
            if (data.CP_KIND != null && data.CP_KIND !== "") {
                cpKind = data.CP_KIND;
            } else {
                cpKind = defaultCpKind.current;
            }
            data.CP_KIND = cpKind;
            setFormData({ ...data, loadTimes: loadTimes.current });
            loadTimes.current += 1;
            setCpKind(cpKind);

            loadCheckpointDropDown(cpKind);

            // 計畫執行方式
            let projectRunWayC = data.RUNWAY_C !== "" && data.RUNWAY_C !== null ? parseInt(data.RUNWAY_C) : ""
            setProjRunWayC(projectRunWayC);
            setCheckpoint(projectRunWayC);

            // 設定檢核點資料
            const gridData = changeCheckItemDateModel(projectNo, data.CusCheckpointModels, 'CHECKITEM_NAME')
            // 表格預設資料
            defaultData.current = { ...data, CusCheckpointModels: gridData.map(x => { return { ...x } }) }

            // 若為調整，設定檔案
            if (type === "adjust") {
                let fileData = fileList(data.Files, "IDENTITY_FIELD");
                setFiles(fileData);
                defaultFiles.current = fileData;
            }
            setIsFormReady(true)
        }
        SetMaskOnOff(false);
    }

    // 取得計畫檢核點資料
    const loadProjectCheckpoint = async () => {
        let data = await loadFormData(type === "adjust" ? projAdjId : projectNo, type);
        if (data != null && data.CusCheckpointModels) {
            let gridData = changeCheckItemDateModel(projectNo, data.CusCheckpointModels, 'CHECKITEM_NAME', data.CONTROL_DATE1)
            setGridData([...gridData]);
            SetEsimatedEndDate(gridData)
        }
    }

    // 透過切換執行方式取得系統預設檢核點資料
    const loadGridData = async () => {
        SetMaskOnOff(true);
        let data = await loadCusItem(checkpoint);
        // 物件轉換，轉成可存檔Model對應欄位
        let dataResult = changeCheckItemDateModel(projectNo, data, 'NAME');
        // 資料來源為代碼維護設定
        dataResult.map(i => i.fromSettings = true);
        setGridData([...dataResult]);
        // 設定預定完成期限
        SetEsimatedEndDate(dataResult);
        SetMaskOnOff(false);
    }

    // 設定預定完成期限
    const SetEsimatedEndDate = (gridData) => {
        if (gridData.length > 0 && gridData[gridData.length - 1].ESTIMATED_ENDDATE !== '') {
            let estimatedDate = gridData[gridData.length - 1].ESTIMATED_ENDDATE === undefined ? "" :
                FormatDate(gridData[gridData.length - 1].ESTIMATED_ENDDATE, 'tYY/MM/DD');
            setEstimatedDate(estimatedDate);
        } else {
            setEstimatedDate("無");
        }
    }

    // Grid異動
    const gridChange = async (data) => {
        gridSaveData.current = [...data];
    }

    // 存檔
    const SaveData = async (data) => {
        // 依據管制進度排序GridData
        let sortedData = gridSaveData.current.sort((a, b) => (a.PROGRESS > b.PROGRESS) ? 1 : -1);
        // 存檔驗證
        let errMsg = await checkDataValid(data, sortedData, checkpoint);
        // 解構出存檔物件
        let dataForSave = { ...data };
        let sortedDataForSave = sortedData.map(x => { return { ...x } });
        if (!IsNullOrEmpty(errMsg)) {
            showGlobalMessageBox(errMsg);
            return;
        }

        // 存檔前將各自訂檢核點的預計開始日期設定為計劃開始日期
        sortedDataForSave.forEach(d => {
            d.ESTIMATED_STARTDATE = FormatDate(d.ESTIMATED_ENDDATE, 'YYYY-MM-DD');
            d.ESTIMATED_ENDDATE = FormatDate(d.ESTIMATED_ENDDATE, 'YYYY-MM-DD');
            d.PROJECT_NO = dataForSave.PROJECT_NO;
        });

        let requestModel = {
            PROJECT_NO: dataForSave.PROJECT_NO,
            CP_KIND: cpKind,
            RUNWAY_C: checkpoint.toString(),
            OLD_RUNWAY_C: data.OLD_RUNWAY_C,
            MEMO_CHK_POINT: data.MEMO_CHK_POINT,
            PROJECT_LAST_DATE: FormatDate(sortedDataForSave[sortedDataForSave.length - 1].ESTIMATED_ENDDATE, 'YYYY-MM-DD'),
            CONTROL_DATE1: FormatDate(dataForSave.CONTROL_DATE1, 'YYYY-MM-DD'),
            // 最後一項檢核點的預定完成日期
            CONTROL_DATE6: FormatDate(sortedDataForSave[sortedDataForSave.length - 1].ESTIMATED_ENDDATE, 'YYYY-MM-DD'),
            CusCheckpointModels: sortedDataForSave
        }

        // 當調整時，設定 ORI_ESTIMATED_ENDDATE、SCHE_TYPE、PROJ_ADJ_ID、Files
        if (type === "adjust") {
            // 設定原預定完成日期在 PROGRESS = 100 的那筆資料
            let originFullPctItem = defaultData.current.CusCheckpointModels.find(item => Math.floor(item.PROGRESS) === 100);
            let originEstimatedEndDate = FormatDate(originFullPctItem.ORI_ESTIMATED_ENDDATE, 'YYYY-MM-DD');
            let estimatedEndDateIndex = sortedDataForSave.findIndex(x => Math.floor(x.PROGRESS) === 100);
            sortedDataForSave[estimatedEndDateIndex].ORI_ESTIMATED_ENDDATE = originFullPctItem.ORI_ESTIMATED_ENDDATE;
            requestModel.CusCheckpointModels = sortedDataForSave;

            // 設定期程調整類別 (PROGRESS = 100 的那筆資料，原預定完成日期 != 預定完成日期，表總期程調整Y，否則為分月調整M)
            let fullPctItem = sortedDataForSave.find(item => Math.floor(item.PROGRESS) === 100);
            let estimatedEndDate = FormatDate(fullPctItem.ESTIMATED_ENDDATE, 'YYYY-MM-DD');
            requestModel.SCHE_TYPE = originEstimatedEndDate !== estimatedEndDate ? 'Y' : 'M';

            requestModel.PROJ_ADJ_ID = projAdjId;
            requestModel.Files = [{
                PROJECT_NO: projectNo,
                FILE_KIND: fileType === 1 ? "13" : "23",
                EditFiles: editedFiles.current
            }];
        }

        let saveResult = await saveData(requestModel, type);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => { window.location.reload() })
        } else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    // 取消功能
    const cancel = () => {
        // 重設資料為預設資料
        let dfData = defaultData.current;
        // 設定計畫開始日期
        setFormData({ ...formData, CONTROL_DATE1: dfData.CONTROL_DATE1, loadTimes: loadTimes.current });
        loadTimes.current += 1;
        // 設定執行方式
        setCpKind(dfData.CP_KIND ?? defaultCpKind.current);
        setCheckpoint(dfData.RUNWAY_C ? parseInt(dfData.RUNWAY_C) : "");

        // 設定預設檢核點設定 (map出新物件才不會改到defualtData)
        setGridData(dfData.CusCheckpointModels.map(x => { return { ...x } }));
        if (type == 'adjust') {
            editedFiles.current = [];
            setFiles(defaultFiles.current);
        }
    }

    // 預覽列印
    const Preview = async () => {
        openProjectPrint(state);
    }

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }

    useEffect(() => {
        initPageData();
    }, [])

    // 執行方式類別異動，更新執行方式下拉清單資料
    useEffect(() => {
        if (cpKind !== "")
            loadCheckpointDropDown(cpKind)
    }, [cpKind])

    // 執行方式異動，Grid須重撈，且當調整時，清空「工程竣工報告表或函報竣工文件」
    useEffect(() => {
        if (checkpoint !== "") {
            // 若選取的執行方式是該計劃的執行方式，則取得該計劃的檢核點設定資料
            // 反之撈設定檔
            if (checkpoint === projRunWayC) {
                loadProjectCheckpoint()
            } else {
                loadGridData();
                // 清空「工程竣工報告表或函報竣工文件」
                removeFiles(files, setFiles, editedFiles);

            }
        } else {
            setGridData([]);
            setEstimatedDate("無");
            // 清空「工程竣工報告表或函報竣工文件」
            removeFiles(files, setFiles, editedFiles);
        }
    }, [checkpoint])

    /**
     * 是否需要上傳「工程竣工報告表或函報竣工文件」或「核章版工程預定進度網圖」
     * @description
     * 工程竣工報告表或函報竣工文件: 當該筆計畫已填報實際竣工日，需上傳；
     * 核章版工程預定進度網圖: 當該筆計畫已填報實際開工日，但還沒填實際竣工日，需上傳
     * @returns 
     */
    const isNeedFile = () => {
        let doneProgressItem = gridData.find(x => x.CTRL_POINT === "B" && x.ACTUAL_ENDDATE != null);
        let startProgressItem = gridData.find(x => x.CTRL_POINT === "A" && x.ACTUAL_ENDDATE != null);
        if (type === "adjust" && doneProgressItem) {
            fileType = 1;
            return true;
        }
        else if (type === "adjust" && startProgressItem) {
            fileType = 2;
            return true;
        }
        return false;
    }

    /**
     * 刪除檔案
     * @param {*} removeFiles useState 的 files
     * @param {*} setRemoveFiles useState 的 setFiles
     * @param {*} editedRemoveFiles useRef 存資料庫的editedFiles
     */
    const removeFiles = (removeFiles, setRemoveFiles, editedRemoveFiles) => {
        if (type != "adjust") {
            return;
        }
        removeFiles.map(file => {
            if (file.isUploaded) {
                let editFile = {
                    EditType: 2, // 刪除
                    FileId: file.FileId,
                    FileName: file.name,
                    Extension: file.extension
                };
                editedRemoveFiles.current.push(editFile);
                setRemoveFiles(removeFiles.filter(f => f !== file));
            } else {
                // 將此檔案從files中移除
                setRemoveFiles(removeFiles.filter(f => f !== file));
                // 將此檔案從異動檔案清單移除
                editedRemoveFiles.current = editedRemoveFiles.current.filter(editedFile => editedFile.Uid !== file.uid);
            }
        })
    }

    return (
        <>
            <CollapseBoardCard
                button={
                    <>
                        {showSomeBtn &&
                            <>
                                <Button title="存檔" onClick={handleSubmit} >存檔</Button>
                                <Button title="取消" className="k-button-lighten" onClick={cancel}>取消</Button>
                            </>
                        }
                        {type !== "adjust" && <Button title="預覽列印" className='k-button-lighten' onClick={Preview}>預覽列印</Button>}
                    </>
                }
                title={type === "adjust" ? "2.檢核點調整" : "檢核點設定"}
                isFirstArea={true}
            >

                <Formik
                    initialValues={formData}
                    onSubmit={(data) => { SaveData(data) }}
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
                            <form onSubmit={handleSubmit} >
                                <table className={Table.fullWidth}>
                                    <tbody>
                                        <tr>
                                            <th>
                                                <CommonTooltip title={"執行方式"} content={"擇定符合計畫性質之執行方式，並設定預定完成日期。"} />
                                            </th>
                                            <td style={{
                                                'display': 'flex',
                                                // @ts-ignore
                                                'grid-gap': '10px'
                                            }}>
                                                <DropDownListWithValue
                                                    data={cpKindDropDown}
                                                    textField={"SET_VALUE"}
                                                    dataItemKey={"SET_TYPE"}
                                                    value={cpKind}
                                                    onChange={(e) => {
                                                        setCpKind(e.target.value);
                                                        setValues({ ...values, CP_KIND: e.target.value });
                                                    }}
                                                />
                                                <DropDownListWithValue
                                                    data={checkpointDropDown}
                                                    textField={"CHECKPOINT_CLASS"}
                                                    dataItemKey={"CHECKPOINT_CLASS_ID"}
                                                    value={checkpoint}
                                                    style={{ width: '250px' }}
                                                    onChange={(e) => {
                                                        setCheckpoint(e.target.value);
                                                        setValues({ ...values, RUNWAY_C: e.target.value });
                                                    }}
                                                    itemRender={(li, item) => {
                                                        if (item.dataItem.DEL_FLG) {
                                                            return <></>
                                                        }
                                                        else {
                                                            return React.cloneElement(li, li.props, <span>{li.props.children}</span>)
                                                        }
                                                    }}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                <CommonTooltip title={"計畫開始日期"} content={"計畫開始日期需要在第一個檢核點之前。"} />
                                            </th>
                                            <td>
                                                <TwDatePicker
                                                    name="CONTROL_DATE1"
                                                    format={"yyy/MM/dd"}
                                                    onChange={(e) => {
                                                        onFormDatePickerChange(e, setValues, values);
                                                    }}
                                                    // @ts-ignore
                                                    value={values.CONTROL_DATE1 == null ? null : new Date(values.CONTROL_DATE1)}
                                                    // @ts-ignore
                                                    error={errors.CONTROL_DATE1}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                <CommonTooltip title={"檢核點"} content={
                                                    <>
                                                        1.新增自訂檢核點可編輯、刪除，存檔後依據管考進度進行排序。<br />
                                                        2.工程類只可以在辦理開工前新增檢核點。
                                                    </>
                                                } />
                                            </th>
                                            <td>
                                                <ProjectFillCheckPointGrid
                                                    data={gridData}
                                                    gridChange={gridChange}
                                                    type={type}
                                                    checkpoint={checkpoint}
                                                />
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>預定完成期限</th>
                                            <td>{estimatedDate}</td>
                                        </tr>
                                        <tr>
                                            <th>總期程調整歷程</th>
                                            <td>
                                                {adjustHistory(values.AdjustScheHistoryModels, "Y")}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>分月期程調整歷程</th>
                                            <td>
                                                {adjustHistory(values.AdjustScheHistoryModels, "M")}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th >備註</th>
                                            <td>
                                                <PureHtmlTextAreaInput
                                                    rows={6}
                                                    name='MEMO_CHK_POINT'
                                                    value={values.MEMO_CHK_POINT}
                                                    onChange={handleChange}
                                                    onBlur={(e) => {
                                                        setValues({ ...values, MEMO_CHK_POINT: e.target.value });
                                                    }}
                                                    style={{ width: '100%' }}
                                                />
                                            </td>
                                        </tr>
                                        {isNeedFile() &&
                                            <>
                                                <th>
                                                    <CommonTooltip title={<>
                                                        {fileType === 1 &&
                                                            <div style={{ display: 'inline-flex' }}>工程竣工報告表或<br />竣工核定文件</div>
                                                        }
                                                        {fileType === 2 &&
                                                            <div style={{ display: 'inline-flex' }}>核章版工程預定進<br />度網圖或預定竣工日展延核定文件</div>
                                                        }

                                                    </>}
                                                        content={
                                                            <>
                                                                可上傳檔案格式如下<br />
                                                                jpg, jpeg, bmp, png,<br />
                                                                mpg, doc, docx, ppt,<br />
                                                                pptx, pdf, xls, xlsx,<br />
                                                                odt, ods, odp, odg
                                                            </>}
                                                    />
                                                </th>
                                                <td>
                                                    <TempFileUploader
                                                        {...tempFileUploadService.uploaderParam}
                                                        saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                                                        saveHeaders={{
                                                            // @ts-ignore
                                                            'authorization': getGlobalServerConfig().BasicData.token.get(),
                                                            'CacheToken': CacheLoader().GetCache(),
                                                        }}
                                                        files={files}
                                                        setFiles={setFiles}
                                                        multiple={true}
                                                    />
                                                </td>
                                            </>
                                        }
                                    </tbody>
                                </table>
                            </form>

                        )
                    }}
                </Formik>
            </CollapseBoardCard>

        </>
    );
}
export default ProjectFillCheckPointMain