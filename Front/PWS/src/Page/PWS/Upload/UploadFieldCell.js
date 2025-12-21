import React, { useState, useRef, useEffect } from "react";
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { TempFileUploader } from '../../../Components/Upload/TempFileUploader';
import { TempFileUploadService, fileList } from '../../../Components/Upload/TempFileUploadService';
import CacheLoader from '../../../Basic/CacheLoader';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { downProjectAttachment } from '../../../Basic/CommonService';

const UploadFieldCell = ({ dataItem, setFileChange }) => {
    // 檔案上傳異動相關資訊
    const editedFiles = useRef([]);
    // 檔案
    const [files, setFiles] = useState([]);
    // 紀錄資料資否有異動
    const [isDataChange, setIsDataChange] = useState(false);
    const [deleteType, setDeleteType] = useState("");
    // 單檔上傳所需參數
    let param = {
        files,
        editedFiles,
        setFiles,
        setIsDataChange,
        multiple: false,
        downFile: downProjectAttachment,
        setDeleteType
    }
    // 暫存檔上傳Service
    const tempFileUploadService = TempFileUploadService(param);

    /**
     * 檔案上傳事件
     * @returns
     */
    const onFileChange = () => {
        // 把 editedFiles 整包丟進dataItem裡
        dataItem.EditFiles = editedFiles.current;
        // 從 files 取得檔案相關資訊，並塞到 dataItem中
        dataItem.extension = files[0].extension;
        dataItem.FILE_NAME = files[0].name;
        dataItem.getRawFile = files[0].getRawFile;
        dataItem.uid = editedFiles.current[editedFiles.current.length - 1].Uid;
        // 是否已上傳到正確位置，PS:grid file上傳是採取先上傳到暫存地方，存檔後才放到正確的檔案路徑
        dataItem.isUploaded = false;
        // 將加工過後的dataItem 回傳外層
        setFileChange(dataItem);
    }

    // 有檔案資料上傳時，設定GridDataItem
    useEffect(() => {
        // 異動檔案數
        let ecl = editedFiles.current.length;
        if (!IsNullOrEmpty(deleteType) && isDataChange == true) {
            // For grid file delete ， 0 : 刪除已上傳檔案， 1 : 刪除暫存檔案            
            if (deleteType == "1") {
                // 將此檔案從異動檔案清單移除
                editedFiles.current = editedFiles.current.filter(editedFile => editedFile.Uid !== files[0].uid);
            }
            dataItem.FILE_NAME = null;
            dataItem.EditFiles = editedFiles.current;
            setFileChange(dataItem);
            setDeleteType("");
        }
        // 新增與覆蓋
        else if (ecl > 0 && isDataChange == true &&
            !(ecl == 1 && editedFiles.current[0].EditType == 2)) {
            onFileChange();
        }
        setIsDataChange(false);
    }, [isDataChange]);

    // 載入檔案資料
    useEffect(() => {
        let result = IsNullOrEmpty(dataItem.FILE_NAME)
            ? [] : fileList([dataItem], 'IDENTITY_FIELD', 'FILE_NAME');
        setFiles(result);
        editedFiles.current = !IsNullOrEmpty(dataItem) ?
            (!IsNullOrEmpty(dataItem.EditFiles) ? dataItem.EditFiles : []) : [];
    }, []);

    return (
        <div className={"upload_File"} >
            <TempFileUploader
                {...tempFileUploadService.uploaderParam}
                saveUrl={getGlobalServerConfig().backEndUrl.get() + 'UploadFile/UploadTempFile'}
                saveHeaders={{
                    'authorization': getGlobalServerConfig().BasicData.token.get(),
                    'CacheToken': CacheLoader().GetCache(),
                }}
                files={files}
                setFiles={setFiles}
                multiple={false}
            />
        </div>
    )
}

export default UploadFieldCell;