import React from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { SetMaskOnOff, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import ProjectFillFileUpService from './ProjectFillFileUpService';
import ProjectFillFileUpGrid from './ProjectFillFileUpGrid';
import ShowMoreFile from './ShowMoreFile';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { openProjectPrint } from '../../../Basic/CommonService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";

export const ProjectFillFileUpMain = (props) => {
    let {
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
                showSomeBtn
            }
        }
    } = props;

    // 主辦區塊資料
    const [handData, setHandData] = React.useState([]);
    // 管考區塊資料
    const [rdecData, setRdecData] = React.useState([]);
    // 其他檔案
    const [otherFiles, setOtherFiles] = React.useState({
        ProjectOtherAttachmentModels: [],
        AdjustScheHistoryModels: [],
    });
    // 檔案類型下拉資料
    const [fileKindDDL, setFileKindDDl] = React.useState([]);
    //紀錄異動資料
    const editedHandGridData = React.useRef([]);
    const editedRdecGridData = React.useRef([]);

    const loadData = async () => {
        SetMaskOnOff(true);
        await loadAttachment();
        await loadOtherData();
        await loadDropDownData();
        SetMaskOnOff(false);
    }

    //載入相關檔案
    const loadAttachment = async () => {
        let data = await ProjectFillFileUpService.getProjectAttachment(projectNo, "01");
        // 主辦區域顯示資料(主辦+管考開放)
        let handUser = data.filter(x => IsNullOrEmpty(x.IS_DISPLAY) || x.IS_DISPLAY)
            .map(x => {
                if (IsNullOrEmpty(x.IS_DISPLAY)) {
                    return { ...x, editable: true }
                }
                else {
                    return { ...x, editable: false }
                }
            });
        // 管考區域顯示資料
        let rdec = data.filter(x => !IsNullOrEmpty(x.IS_DISPLAY))
            .map(x => {
                return { ...x, editable: true }
            });
        setHandData(handUser);
        setRdecData(rdec);
    }

    // 載入其他檔案
    const loadOtherData = async () => {
        let data = await ProjectFillFileUpService.getProjectOtherAttachment(projectNo);
        setOtherFiles(data);
    }

    // 載入檔案類型下拉資料
    const loadDropDownData = async () => {
        let data = await ProjectFillFileUpService.getFileKindDropDown();
        setFileKindDDl(data);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 存檔
     */
    const save = async () => {
        //驗證
        let notValids = [];
        for (let index = 0; index < handData.length; index++) {
            const item = handData[index];
            let isValid = await ProjectFillFileUpService.validateField.isValid(item);
            if (!isValid || item.FILE_NAME == null) {
                notValids.push({ type: "hand", key: index, value: isValid });
            }
        }
        for (let index = 0; index < rdecData.length; index++) {
            const item = rdecData[index];
            let isValid = await ProjectFillFileUpService.validateField.isValid(item);
            if (!isValid || item.FILE_NAME == null) {
                notValids.push({ type: "rdec", key: index, value: isValid });
            }
        }
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
        }
        else {
            SetMaskOnOff(true);
            let saveData = [...editedHandGridData.current, ...editedRdecGridData.current];
            let saveResult = await ProjectFillFileUpService.saveProjectAttachment(saveData);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message,
                    () => window.location.reload());
            }
            else {
                showGlobalMessageBox(saveResult.message);
            }
        }
    }

    /**
     * 清空暫存資料
     */
    const resetData = async () => {
        SetMaskOnOff(true);
        await loadAttachment();
        await loadOtherData();
        editedHandGridData.current = [];
        editedRdecGridData.current = [];
        SetMaskOnOff(false);
    }

    return (
        <>
            <CollapseBoardCard button={
                <>
                    {
                        showSomeBtn &&
                        <>
                            <Button title="存檔" onClick={() => { save() }} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={() => { resetData() }} >取消</Button>
                        </>
                    }
                    <Button title="預覽列印" className="k-button-lighten" onClick={() => openProjectPrint(state)}>預覽列印</Button>
                </>}
                title="相關檔案上傳(主辦)" isFirstArea={true} >
                <>
                    <ShowMoreFile
                        isRdecFun={isRdecFun}
                        otherFiles={otherFiles}
                    />

                    <ProjectFillFileUpGrid
                        projectNo={projectNo}
                        gridData={handData}
                        setGridData={setHandData}
                        editedGridData={editedHandGridData}
                        fileKindDDL={fileKindDDL}
                        type={"hand"}
                        showSomeBtn={showSomeBtn}
                    />
                </>
            </CollapseBoardCard>

            {isRdecFun &&
                <CollapseBoardCard title="相關檔案上傳(管考)" initValue={false}>
                    <ProjectFillFileUpGrid
                        projectNo={projectNo}
                        gridData={rdecData}
                        setGridData={setRdecData}
                        editedGridData={editedRdecGridData}
                        fileKindDDL={fileKindDDL}
                        type={"rdec"}
                        showSomeBtn={showSomeBtn}
                    />
                </CollapseBoardCard>
            }
        </>
    )
}
export default ProjectFillFileUpMain;