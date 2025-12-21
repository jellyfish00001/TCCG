import React from 'react';
import '../../Css/custom/uploadFile.css';
import { Upload } from '@progress/kendo-react-upload';
import { IsNullOrEmpty } from '../../Basic/SDOExtension';
import CacheLoader from '../../Basic/CacheLoader';
import { getGlobalServerConfig, showGlobalMessageBox } from '../../Route/RootMiddleware';


export const FileUploader = (props) => {

    const isPropsEmpty = (value, defaultValue) => {
        return IsNullOrEmpty(value) ? defaultValue : value;
    }

    const FieldData = React.useRef({
        // 一次選擇文件數量，true多個 false單個
        multiple: isPropsEmpty(props.multiple, true),
        // 選定的文件會立即上傳
        autoUpload: isPropsEmpty(props.autoUpload, true),
        // 自動上傳的存檔url 
        saveUrl: isPropsEmpty(props.saveUrl, ""),
        saveHeaders: props.saveHeaders,
        // 自動上傳的刪除url
        removeUrl: isPropsEmpty(props.removeUrl, ""),
        removeHeaders: props.removeHeaders,
        // 檔案最大限制，預設為20MB (20Mb*1024*1024)
        maxFileSize: isPropsEmpty(props.maxFileSize, 20971520),
        // 檔案副檔名白名單
        allowedExtensions: isPropsEmpty(props.allowedExtensions, [
            // ".rar", ".zip", Nash說因資安考量拿掉
            ".jpg", ".jpeg", ".bmp", ".png",
            ".mpg", ".doc", ".docx", ".ppt", ".pptx", ".pdf",
            ".xls", ".xlsx", ".odt", ".ods", ".odp", ".odg"
        ]),
        // 顯示操作按鈕 autoUpload=false 會出現
        showActionButtons: isPropsEmpty(props.showActionButtons, false),
        // 提醒文字
        reminderText: isPropsEmpty(props.reminderText, ""),
        // 上傳檔案的參數名稱，預設為 files
        saveField: isPropsEmpty(props.saveField, "files"),
        // 上傳前-其他欄位：除了檔案以外，同時傳其他欄位，使用 key value array。
        // ex: [ {key, value}, {key, value} ]
        beforeUpload: props.beforeUpload,
        // 刪除前-其他欄位：除了檔案以外，同時傳其他欄位，使用 key value array。
        // ex: [ {key, value}, {key, value} ]
        beforeRemove: props.beforeRemove,
        // 是否要縮小
        isShrink: props.isShrink !== undefined ? props.isShrink : false,
        // 顯示file清單
        showFileList: isPropsEmpty(props.showFileList, true)
    })

    /**
     * 自動上傳功能
     * @param {*} e 
     */
    const refreshBasicData = (e) => {
        if (e.response && e.response.hasOwnProperty('headers')) {
            let headers = e.response.headers;

            //更新驗證token
            if (headers.hasOwnProperty('authorization')) {
                getGlobalServerConfig().BasicData.token.set(headers['authorization']);
            }
            if (headers.hasOwnProperty('loginCache')) {
                CacheLoader().SetCache(headers['loginCache']);
            }
        }

        //檔案狀態
        let uploadFileStatus = e.affectedFiles[0].status
        if (e.response && e.response.response) {
            let response = e.response.response;
            if (response && response['success'] && e.response.status === 200) {
                //上傳成功
                if (uploadFileStatus === 4) {
                    //將fileSeqNo回寫到upload的files object
                    e.affectedFiles.map(x => x.fileSeqNo = response['message'])
                    let data = [...props.fileSeqNo, response['message']]
                    props.setFileSeqNo(data)
                    props.setValue(data)
                }
                //刪除成功
                else if (uploadFileStatus === 6) {
                    let data = props.fileSeqNo.filter(item => item !== e.affectedFiles[0].fileSeqNo)
                    props.setValue(data)
                }
            } else if (e.response.status === 200) {
                e.newState.pop()
                showGlobalMessageBox(e.affectedFiles[0].name + "上傳失敗")
            } else {
                showGlobalMessageBox("請洽系統管理員")
            }
        }
    }

    /**
     * 添加：當選擇新文件上傳時觸發
     * @param {*} e 
     */
    const onAdd = (e) => {
        props.setFiles(e.newState);
    }

    /**
     * 進展：文件上傳進度改變時觸發
     * @param {*} e 
     */
    const onProgress = (e) => {
        props.setFiles(e.newState);
    }

    /**
     * 刪除：刪除文件時觸發
     * @param {*} e 
     */
    const onRemove = (e) => {
        props.setFiles(e.newState);
        if (props.onStatusChange !== undefined) {
            props.onStatusChange(e);
        }
        else {
            refreshBasicData(e);
        }
    }

    /**
     * 狀態變化：文件狀態改變時觸發
     * @param {*} e 
     */
    const onStatusChange = (e) => {
        props.setFiles(e.newState);
        // 異動檔案的uid
        let changedFileUid = e.affectedFiles[0].uid;
        if (props.onStatusChange !== undefined) {
            props.onStatusChange(e);
        }
        // 若異動檔案是刪除，須等到後端回來的onRemove事件再進行呼叫refreshBasicData，否則response為undefined
        else if (e.newState && e.newState.filter(x => x.uid === changedFileUid && x.status === 6).length === 0) {
            refreshBasicData(e);
        }
    }

    /**
     * 在發出文件刪除請求之前觸發
     * @param {*} e 
     */
    const onBeforeRemove = (e) => {
        if (e.files[0].fileSeqNo) {
            e.additionalData.fileSeqNo = e.files[0].fileSeqNo;
        }
        if (e.files[0].isUploaded) {
            e.additionalData.isUploaded = e.files[0].isUploaded;
        }
        if (FieldData.current.beforeRemove !== undefined) {
            FieldData.current.beforeRemove.forEach(x => {
                e.additionalData[x.key] = x.value;
            });
        }
    }

    /**
     * 在發出文件上傳請求之前觸發
     * @param {*} e 
     */
    const onBeforeUpload = (e) => {
        if (FieldData.current.beforeUpload !== undefined) {
            FieldData.current.beforeUpload.forEach(x => {
                e.additionalData[x.key] = x.value;
            });
        }
    }

    return (
        <div className={FieldData.current.isShrink ? "upload_File" : ""}>
            <Upload
                batch={false} //為true，則所有選定文件將在單個請求中上傳
                multiple={FieldData.current.multiple}
                autoUpload={FieldData.current.autoUpload}
                showActionButtons={FieldData.current.showActionButtons}
                withCredentials={false} //將憑證附加到請求
                restrictions={{
                    maxFileSize: FieldData.current.maxFileSize,
                    //允許的附檔名，可依各專案自行調整 EXE TXT CSV會有資安疑慮請避免開放
                    allowedExtensions: FieldData.current.allowedExtensions
                }}

                files={props.files}
                saveField={FieldData.current.saveField}

                showFileList={FieldData.current.showFileList}
                disabled={isPropsEmpty(props.disabled, false)}
                onAdd={onAdd}
                onProgress={onProgress}
                onStatusChange={onStatusChange}
                onRemove={onRemove}

                saveUrl={FieldData.current.saveUrl}
                saveHeaders={FieldData.current.saveHeaders}
                onBeforeUpload={onBeforeUpload}

                removeUrl={FieldData.current.removeUrl}
                removeHeaders={FieldData.current.removeHeaders}
                onBeforeRemove={onBeforeRemove}
            />
        </div>
    )
}