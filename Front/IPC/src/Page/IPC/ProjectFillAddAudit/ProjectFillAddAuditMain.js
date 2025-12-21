import React, { useState, useEffect, useRef } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { loadFormData, SaveProjectFillAddAudit } from './ProjectFillAddAuditService'
import { closeAndbackToParentWindow, SetMaskOnOff, FormatDate, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { Formik } from "formik";
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import { GetSetParam, openProjectPrint } from '../../../Basic/CommonService';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import TextAreaInput from '../../../Components/Input/TextAreaInput';
import { showGlobalConfirmBox, showGlobalMessageBox } from '../../../Route/RootMiddleware';
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';

export const ProjectFillAddAuditMain = (props) => {

    let {
        location: {
            state
        },
        location: {
            state: {
                projectNo,
            } = {
                projectNo: null,
            }
        }
    } = props;

    if (!state) {
        showGlobalMessageBox('請使用正確途徑進入此功能', () => props.history.push('/Home'));
    }

    // Form 資料
    const [formData, setFormData] = useState({});
    const formRef = useRef(null);

    // 特殊加註資料
    const [specNoteDdData, setSpecNoteDdData] = useState([]);
    const [specNoteDatas, setSpecNoteDatas] = useState([]);

    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmitOuter = (type) => {
        if (formRef.current) {
            formRef.current.values.SaveType = type;
            formRef.current.handleSubmit()
        }
    }

    // 取得頁面資料
    const initPageData = async () => {
        // 取得元件資料
        let dropDownData = await GetSetParam("SPEC_NOTE");
        setSpecNoteDdData([...dropDownData]);
        // 取得計劃資料
        let data = await loadFormData(projectNo);
        if (data != null) {
            if (data.SpecNoteDatas != null && data.SpecNoteDatas.length > 0) {
                let specNoteDatas = dropDownData.filter(i => {
                    if (data.SpecNoteDatas.map(x => x.SET_TYPE).includes(i.SET_TYPE))
                        return i;
                });
                setSpecNoteDatas(specNoteDatas);
            }
            setFormData({ ...data })
        }

    }

    // 存檔/送出
    const SaveData = async (data) => {
        if (data.SaveType == 2) {
            if (data.REVIEW_RESULT != 'Y' && data.REVIEW_RESULT != 'R') {
                showGlobalMessageBox('審核結果為必填！')
            } else {
                showGlobalConfirmBox('請確認是否送出審查結果？', async () => {
                    if (!data.REVIEW_RESULT) {
                        showGlobalMessageBox("確認送出前，請先選擇審核結果");
                    } else {
                        await saveEvent(data);
                    }
                })
            }
        } else {
            await saveEvent(data);
        }
    }

    // 存檔事件
    const saveEvent = async (data) => {
        if (data.SpecNoteDatas) {
            data.SpecNoteDatas.map(x => {
                x.PROJECT_NO = projectNo;
                x.SOURCE_ID = projectNo;
            });
        }
        SetMaskOnOff(true);
        let saveResult = await SaveProjectFillAddAudit(data);
        SetMaskOnOff(false);
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                data.SaveType == 1 ? initPageData() :
                    // 關閉章節頁，並導回來源列表頁
                    closeAndbackToParentWindow(window);
            });
        }
    }

    // 取消
    const Cancel = async () => {
        SetMaskOnOff(true);
        await initPageData();
        SetMaskOnOff(false);
    }

    // 預覽列印
    const preview = async () => {
        openProjectPrint(state);
    }

    useEffect(() => {
        initPageData();
    }, [])

    return (
        <CollapseBoardCard
            button={
                <>
                    <Button type="submit" onClick={() => { handleSubmitOuter(1) }}>存檔</Button>
                    <Button type="button" className='k-button-lighten' title="取消" onClick={Cancel} >取消</Button>
                    <Button title="預覽列印" className="k-button-lighten" onClick={preview}>預覽列印</Button>
                </>
            }
            title="立案審核" isFirstArea={true}
        >
            <div className='fn-buttons'>
                <Button type='button' title="確認送出" onClick={() => { handleSubmitOuter(2) }} >確認送出</Button>
            </div>
            <Formik
                initialValues={formData}
                onSubmit={(data) => {
                    SaveData(data)
                }}
                enableReinitialize={true}
                innerRef={formRef}
            >
                {props => {
                    const {
                        values,
                        handleSubmit,
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit} >
                            <table>
                                <tbody>
                                    <tr>
                                        <th>&ensp;特殊加註</th>
                                        <td>
                                            <MultiSelect
                                                popupSettings={
                                                    { className: "dropdown-text-size" }
                                                }
                                                data={specNoteDdData}
                                                textField="SET_VALUE"
                                                dataItemKey="SET_TYPE"
                                                onChange={(e) => {
                                                    setSpecNoteDatas([...e.value])
                                                    setValues({ ...values, SpecNoteDatas: e.target.value })
                                                }}
                                                value={specNoteDatas}
                                                placeholder={"請選擇"}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>立案審核意見</th>
                                        <td>
                                            {
                                                (values.ProjLogs !== undefined && values.ProjLogs !== null) &&
                                                values.ProjLogs.map((item, i) =>
                                                    <>
                                                        <span>{i + 1}.</span>
                                                        <span>{FormatDate(item.LOG_DATE)} </span>
                                                        <span>{item.LOG_STATUS}</span>
                                                        <span>{IsNullOrEmpty(item.MEMO) ? "" : ":"}</span>
                                                        <span>{item.MEMO}</span>
                                                        <span>。</span>
                                                        <br />
                                                    </>
                                                )
                                            }
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>&ensp;管考意見</th>
                                        <td>
                                            <TextAreaInput
                                                rows={7}
                                                maxlength={500}
                                                style={{ width: "100%" }}
                                                onBlur={(e) => {
                                                    setValues({
                                                        ...values,
                                                        MEMO_EVALUATION: e.target.element.current.value
                                                    });
                                                }}
                                                defaultValue={values.MEMO_EVALUATION}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th className="addRedStar">
                                            審核結果
                                        </th>
                                        <td><RadioBoxList
                                            group='REVIEW_RESULT'
                                            valueField='value'
                                            textField='text'
                                            data={[
                                                { text: '審核通過', value: 'Y', checked: values.REVIEW_RESULT == 'Y' },
                                                { text: '退回補正', value: 'R', checked: values.REVIEW_RESULT == 'R' },
                                            ]}
                                            onChange={(e) => setValues({ ...values, REVIEW_RESULT: e.value })}
                                        /></td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    )
                }}
            </Formik>
        </CollapseBoardCard>
    );
}
export default ProjectFillAddAuditMain