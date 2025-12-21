import React, { useState, useEffect, useRef } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { RadioGroup } from '@progress/kendo-react-inputs';
import { FormatDate, IsNullOrEmpty, SetMaskOnOff, closeAndbackToParentWindow } from '../../../Basic/SDOExtension';
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import TextAreaWrapInput from '../../../Components/Input/TextAreaWrapInput';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { initData, getPageData, saveAuditReview, getZip } from './ProjectAdjustRevokeReviewService';
import classnames from 'classnames/bind';
import utils from '../../../Css/custom/Utils.module.css';
import { downProjectAttachment } from "../../../Basic/CommonService";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';

const ProjectAdjustRevokeReviewMain = (props) => {
    const {
        location: {
            state
        },
        location: {
            /**傳入的參數 */
            state: {
                awKind,
                projectNo,
                projAdjId,
                isShowBtn
            } = {
                awKind: null,
                projectNo: null,
                projAdjId: null,
                isShowBtn: true,
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    // 初始表單資料
    const initFormData = useRef({ ...initData, AW_KIND: awKind });
    // 表單編輯資料
    const [editedFormData, setEditedFormData] = useState({...initData, AW_KIND: awKind });

    // 申請項目文字
    const [awKindText, setAwKindText] = useState("");
    // 撤銷原因清單
    const [revokeReasons, setRevokeReasons] = useState([]);

    // ---------上傳檔案-------------
    const [files, setFiles] = useState([]);
    // 下載檔案
    const downFile = async (fileId) => {
        await downProjectAttachment(fileId);
    }

    useEffect(() => {
        loadData();
    }, []);

    // 取資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let result = await getPageData(projAdjId);
        if (result.length == 3) {
            initFormData.current = result[0];
            setEditedFormData(result[0]);
            setFiles(result[0].Files);
            setAwKindText(result[1].SET_VALUE);
            setRevokeReasons(result[2]);
        }
        SetMaskOnOff(false);
    }

    /**
     * 存檔
     * @param {*} isSend 0:存檔、1:確認送出 
     */
    const save = (isSend = 0) => {
        if (IsNullOrEmpty(editedFormData.REVIEW_RESULT)) {
            showGlobalMessageBox("管考審核結果必填");
        }
        else {
            let data = {...editedFormData, IS_SEND: isSend};
            if (isSend == 1) {
                showGlobalConfirmBox("請確認是否送出審核結果？",  () => saveData(data));
            }
            else {
                saveData(data);
            }
        }
    }

    const saveData = async (data) => {
        SetMaskOnOff(true);
        let saveResult = await saveAuditReview(data);
        SetMaskOnOff(false);

        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                // 確認送出
                if (data.IS_SEND == 1) {
                    closeAndbackToParentWindow(window);
                }
                // 存檔
                else {
                    window.location.reload();
                }
            });
        }
        else {
            showGlobalMessageBox(saveResult.message);
        }
    }

    /**
     * 取消
     */
    const reset = () => {
        setEditedFormData(initFormData.current);
    }

    /**
     * 下載zip檔
     */
    const downloadZip = async () => {
        await getZip(projectNo, projAdjId);
    }

    /**
     * 顯示撤銷原因
     * @returns 
     */
    const displayReasons = () => {
        if (IsNullOrEmpty(revokeReasons) || IsNullOrEmpty(editedFormData.Reasons)) {
            return null;
        }
        return (
            editedFormData.Reasons.map(reason => {
                let text = revokeReasons.find(item => item.SET_TYPE == reason.SET_TYPE).SET_VALUE;
                return (
                    <li key={reason.SET_TYPE} style={{ display: 'flex', alignItems: 'center' }}>
                        {reason.SET_TYPE == '99' ?
                            text + "：" + editedFormData.OTHER_REASON
                            :
                            text
                        }
                    </li>
                )
            })
        )
    }
    
    /**
     * 顯示佐證資料
     * @returns 
     */
    const displayFiles = () => {
        if (IsNullOrEmpty(files)) {
            return null;
        }
        return (
            <>
                <div className="fn-buttons">
                    <Button title="下載全部" type='button' onClick={() => downloadZip()} >下載全部</Button>
                </div>
                {
                    files.map(file => {
                        return (
                            <li key={file.IDENTITY_FIELD} style={{ display: 'flex', alignItems: 'center' }}>
                                <div style={{ display: 'flex', alignItems: 'center' }}>
                                    <span onClick={() => downFile(file.IDENTITY_FIELD)}
                                        className={classnames(utils.underline, "k-link")}>
                                        {file.FILE_NAME}
                                    </span>
                                </div>
                            </li>
                        )
                    })
                }
            </>
        )
    }

    return (
        <CollapseBoardCard button={
            isShowBtn && <>
                <Button title="存檔" type='button' onClick={() => save()} >存檔</Button>
                <Button title="取消" className="k-button-lighten" onClick={reset}>取消</Button>
            </>
        } title="計畫撤銷審核" isFirstArea={true} >
            <form>
                {isShowBtn && <div className="fn-buttons">
                    <Button title="確認送出" type='button' onClick={() => save(1)} >確認送出</Button>
                </div>}
                <table>
                    <tbody>
                        <tr>
                            <th>申請項目</th>
                            <td>
                                {awKindText}
                            </td>
                        </tr>
                        <tr>
                            <th>執行機關</th>
                            <td>
                                {editedFormData.EXEC_UNIT}
                            </td>
                        </tr>
                        <tr>
                            <th>計畫名稱</th>
                            <td>
                                {editedFormData.PROJECT_NAME}
                            </td>
                        </tr>
                        <tr>
                            <th>撤銷原因</th>
                            <td>
                                {displayReasons()}
                            </td>
                        </tr>
                        <tr>
                            <th>計畫撤銷原因說明</th>
                            <td>
                                <TextAreaWrapInput value={editedFormData.ADJUST_REASON} />
                            </td>
                        </tr>
                        <tr>
                            <th>核准日期</th>
                            <td>
                                {FormatDate(editedFormData.APPRV_DATE, 'tYY/MM/DD')}
                            </td>
                        </tr>
                        <tr>
                            <th>佐證資料</th>
                            <td>
                                {displayFiles()}
                            </td>
                        </tr>
                        <tr>
                            <th>管考意見</th>
                            <td>
                                <TextAreaInput
                                    rows={3}
                                    max={500}
                                    maxLength={500}
                                    name="REVIEW_COMMENTS"
                                    defaultValue={editedFormData.REVIEW_COMMENTS == null ? '' : editedFormData.REVIEW_COMMENTS}
                                    onBlur={(e) => {
                                        setEditedFormData({
                                            ...editedFormData,
                                            REVIEW_COMMENTS: e.target.element.current.value
                                        })
                                    }}
                                />
                            </td>
                        </tr>
                        <tr>
                            <th className="addRedStar">管考審核結果</th>
                            <td>
                                <RadioGroup
                                    name="REVIEW_RESULT"
                                    value={IsNullOrEmpty(editedFormData.REVIEW_RESULT) ? "" : editedFormData.REVIEW_RESULT}
                                    onChange={(e) => {
                                        setEditedFormData({
                                            ...editedFormData,
                                            REVIEW_RESULT: e.value
                                        })
                                    }}
                                    layout={"horizontal"}
                                    data={[
                                        { label: "審核通過", value: 'Y' },
                                        { label: "審核未通過", value: 'N' },
                                        { label: "退回修正", value: 'R' },
                                    ]}
                                />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </form>
        </CollapseBoardCard>
    )
}

export default ProjectAdjustRevokeReviewMain;