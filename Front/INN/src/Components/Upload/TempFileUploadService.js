import * as React from 'react';
import { showGlobalMessageBox, getGlobalServerConfig } from '../../Route/RootMiddleware';
import CacheLoader from '../../Basic/CacheLoader';
import { api } from '../../Basic/ApiFetch';

import classnames from 'classnames/bind';
import utils from '../../Css/custom/Utils.module.css';
import { saveAs } from '@progress/kendo-file-saver';
import { UploadFileStatus } from '@progress/kendo-react-upload';
import { SetMaskOnOff } from '../../Basic/SDOExtension';
let backEndUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * filesList 結構轉換
 * @param {*} files // 檔案資訊
 * @param {string} fileIdName // 儲存file於DB的'ID'欄位名稱
 * @param {string} fileSrcName// 儲存file於DB的'原上傳檔名'欄位名稱
 */
export const fileList = (files, fileIdName, fileSrcName = "FILE_NAME") => {
    if (files !== undefined || files.length > 0) {
        let result = files.map(i => {
            let fileName = i[fileSrcName].split('.')[0];
            return {
                name: fileName,
                extension: '.' + i[fileSrcName].split('.')[1],
                size: 1,
                progress: 0,
                status: UploadFileStatus.Initial,
                uid: i["uid"] == undefined ? 'uid' + fileName : i["uid"],
                FileId: i[fileIdName],
                // 是否已上傳到正確位置，PS:grid file上傳是採取先上傳到暫存地方，存檔後才放到正確的檔案路徑
                isUploaded: i["isUploaded"] == undefined ? true : i["isUploaded"],
                // grid file需要紀錄該object
                getRawFile: i["getRawFile"] == undefined ? null : i["getRawFile"],

            };
        });
        return result;
    }
    return [];
}

/**
 * 
 * @typedef {object} tempFileParam 
 * @param {*} files 檔案清單
 * @param {*} editedFiles 檔案上傳異動相關資訊 ※必須傳入 useRef([])
 * @param {*} setFiles 設定檔案清單
 * @param {*} setIsDataChange 紀錄資料是否修改
 * @param {*} multiple 一次選擇文件數量，true多個 false單個
 * @param {*} downFile 呼叫外部下載已上傳檔案API
 * @returns 
 */

/**
 * 
 * @param {...tempFileParam} props 參考結構請詳閱: {@link tempFileParam}
 * @returns 
 */
export const TempFileUploadService = (props) => {
    let { files, editedFiles, setFiles, setIsDataChange, multiple, downFile, setDeleteType } = props

    /**
     * 狀態變化：文件狀態改變時觸發
     * @param {*} e 
     */
    const onUploaderStatusChange = async (e) => {
        if (e.hasOwnProperty('response')) {
            if (e.response.hasOwnProperty('headers')) {
                let headers = e.response.headers;

                //更新驗證token
                if (headers.hasOwnProperty('authorization')) {
                    getGlobalServerConfig().BasicData.token.set(headers['authorization']);
                }
                if (headers.hasOwnProperty('cacheToken')) {
                    CacheLoader().SetCache(headers['cacheToken']);
                }
            }

            // 檔案狀態
            if (e.response && e.response.response) {
                let response = e.response.response;
                let thisFile = e.affectedFiles[0];

                if (response.success) {
                    // 上傳成功
                    if (thisFile.status === 4) {
                        let thisUid = response.message; // 新增成功回傳的uid

                        if (!multiple) {
                            // 如果是單檔，先清除之前新增的暫存檔
                            editedFiles.current = editedFiles.current.filter(x => x.EditType > 1);
                        }
                        // 寫到 editedFiles
                        let newFileObj = {
                            EditType: 1, // 新增
                            FileName: thisFile.name,
                            Extension: thisFile.extension,// 檔案副檔名
                            Uid: thisUid
                        };
                        editedFiles.current.push(newFileObj);
                        // 將檔案uid改成回傳uid值
                        thisFile.uid = thisUid;
                        // 設定資料有改過
                        setIsDataChange(true);
                    }
                } else {
                    e.newState.pop();
                    showGlobalMessageBox(thisFile.name + " 上傳失敗");
                }
            } else {
                showGlobalMessageBox("請洽系統管理員");
            }

            setFiles(e.newState);
        }
        SetMaskOnOff(false)
    }

    /**
     * 添加：當選擇新文件上傳時觸發
     * @param {*} e 
     */
    const onAdd = (e) => {
        SetMaskOnOff(true)
        if (!multiple) {
            // 如果是單檔，要刪掉已上傳的檔案
            let deleteFile = files.find(x => x.isUploaded);
            if (deleteFile !== undefined) {
                removeFile(true, deleteFile, true);
            }
        }
        // check uploaded file size is not over 20MB 
        if (e.affectedFiles.length > 0 && e.affectedFiles[0].size < 20971520) {
            setFiles(e.newState);
        } else {
            showGlobalMessageBox("上傳檔案不得超過20MB");
        }

        if (e.affectedFiles.filter(x => x.validationErrors !== undefined).length > 0) {
            SetMaskOnOff(false)
        }
    }

    const linkClasses = classnames(utils.underline, "k-link");

    /**
     * 自訂Upload的清單
     * @param {*} props 
     * @returns 
     */
    const customListItemUI = (props) => {
        return (
            <ul >
                {props.files.map((file) => {
                    let statusStr = '';
                    if (file.status === 0)
                        statusStr = '上傳失敗';

                    if (file.validationErrors && file.validationErrors[0] === 'invalidFileExtension')
                        statusStr = '禁止的檔案格式';

                    return (
                        <>
                            <li key={file.name} style={{ display: 'flex', alignItems: 'center' }}>
                                {
                                    file.isUploaded
                                        ?
                                        <div style={{ display: 'flex', alignItems: 'center' }}>
                                            <a href="/" className="k-button k-button-icon" onClick={(e) => {
                                                e.preventDefault();
                                                removeFile(file.isUploaded, file)
                                            }}>
                                                <span className="k-icon k-i-close"></span>
                                            </a>
                                            <span style={{ marginLeft: '10px' }} onClick={() => downFile(file.FileId)}
                                                className={linkClasses}>
                                                {file.name + file.extension + '  (已上傳)'}
                                            </span>
                                        </div>
                                        :
                                        <div style={{ display: 'flex', alignItems: 'center' }}>
                                            <a href="/" className="k-button k-button-icon" onClick={(e) => {
                                                e.preventDefault();
                                                removeFile(file.isUploaded, file)
                                            }}>
                                                <span className="k-icon k-i-close"></span>
                                            </a>
                                            <span style={{ marginLeft: '10px' }} onClick={() => saveAs(file.getRawFile(), file.name)}
                                                className={linkClasses}>
                                                {(file.name.includes(".") ? file.name : file.name + file.extension)
                                                    + '  (已暫存未上傳)'}
                                            </span>
                                        </div>
                                }
                                {<span style={{ color: 'red' }}>&nbsp;&nbsp; {statusStr}</span>}
                            </li>
                        </>
                    )
                })}
            </ul>
        );
    };

    /**
     * 移除已上傳或未上傳的檔案
     * @param {boolean} isUploaded 是否為已上傳的檔案
     * @param {*} file 檔案 
     */
    const removeFile = async (isUploaded, file, isOnAdd) => {
        setIsDataChange(true);
        if (!isOnAdd && setDeleteType !== undefined) {
            // For grid file delete ， 0 : 刪除已上傳檔案， 1 : 刪除暫存檔案
            setDeleteType(isUploaded ? "0" : "1");
        }
        if (isUploaded) {
            let editFile = {
                EditType: 2, // 刪除
                FileId: file.FileId,
                FileName: file.name,
                Extension: file.extension
            };
            editedFiles.current.push(editFile);
            setFiles(files.filter(f => f !== file));
        } else {
            let result = await removeTempFile(file.uid);
            if (result.success) {
                // 將此檔案從files中移除
                setFiles(files.filter(f => f !== file));
                // 將此檔案從異動檔案清單移除
                editedFiles.current = editedFiles.current.filter(editedFile => editedFile.Uid !== file.uid);
            } else {
                showGlobalMessageBox(result.message);
            }
        }
    }

    /**
     * 移除單筆暫存檔案
     * @param {String} uid 檔案ID
     * @returns 
     */
    const removeTempFile = async (uid) => {
        let url = backEndUrl + 'UploadFile/RemoveTempFile';
        SetMaskOnOff(true);
        let response = await api.Post(url, JSON.stringify(uid));
        SetMaskOnOff(false);
        let result = null;
        if (response != null && response.ok) {
            result = response.json();
        }
        return result;
    }

    return {
        uploaderParam: {
            listItemUI: customListItemUI,
            onStatusChange: onUploaderStatusChange,
            onAdd: onAdd,
        }
    }
}