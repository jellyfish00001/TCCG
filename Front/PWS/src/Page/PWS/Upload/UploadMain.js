import React, { useState, useRef, useEffect } from "react";
import { Button } from '@progress/kendo-react-buttons';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { getProjectAttachment, saveProjectAttachment, getFileKindDropDown, validateField} from './UploadService';
import UploadGrid from './UploadGrid';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import * as Yup from 'yup';

export const UploadMain = (props) => {
    let { 
        location: { 
            state: { 
                projectNo,
                projectIsSend,
                } = { 
                    projectNo: "",
                    projectIsSend: true,
                    } } } = props;
    
    const showSomeBtn = !projectIsSend;
    // 主辦區塊資料
    const [handData, setHandData] = useState([]);
    const editedHandGridData = useRef([]);
    // 檔案類型下拉資料
    const [fileKindDDL, setFileKindDDl] = useState([]);

    /**
     * 載入資料
     * @returns
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        await loadAttachment();
        await loadDropDownData();
        SetMaskOnOff(false);
    }
    
    /**
     * 載入相關檔案
     * @returns
     */
    const loadAttachment = async () => {
        let data = await getProjectAttachment(projectNo, "01");
        let handUser = data.map(x => ({ ...x, editable: true }));
        setHandData(handUser);
    }
    
    /**
     * 載入檔案類型下拉資料
     * @returns
     */
    const loadDropDownData = async () => {
        let data = await getFileKindDropDown();
        setFileKindDDl(data);
    }

    // 載入資料
    useEffect(() => {
        loadData();
    }, [])

    /**
     * 存檔
     * @returns
     */
    const save = async () => {
        // 驗證
        let notValids = [];
        for (let index = 0; index < handData.length; index++) {
            const item = handData[index];
            let isValid = await validateField.isValid(item);
            if (!isValid || item.FILE_NAME == null) {
                notValids.push({ type: "hand", key: index, value: isValid });
            }
        }
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
        } else {
            SetMaskOnOff(true);
            let saveData = [...editedHandGridData.current];
            saveData = saveData.map(item => {
                return {
                    ...item,
                    DB: 5
                };
            });
            let saveResult = await saveProjectAttachment(saveData);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => window.location.reload());
            } else {
                showGlobalMessageBox(saveResult.message);
            }
        }
    }

    /**
     * 重置資料
     */
    const resetData = async () => {
        SetMaskOnOff(true);
        await loadAttachment();
        await loadDropDownData();
        editedHandGridData.current = [];
        SetMaskOnOff(false);
    }

    return (
        <>
            <CollapseBoardCard button={
                <>
                    { showSomeBtn &&
                        <>
                            <Button title="存檔" onClick={() => { save() }} >存檔</Button>
                            <Button title="取消" className="k-button-lighten" onClick={() => { resetData() }} >取消</Button>
                        </>
                    }
                </>}
                title="相關檔案上傳(主辦)" isFirstArea={true} >
                <>
                    <UploadGrid
                        projectNo={projectNo}
                        gridData={handData}
                        setGridData={setHandData}
                        editedGridData={editedHandGridData}
                        fileKindDDL={fileKindDDL}
                        showSomeBtn={showSomeBtn}
                    />
                </>
            </CollapseBoardCard>
        </>
    )
}
export default UploadMain;
