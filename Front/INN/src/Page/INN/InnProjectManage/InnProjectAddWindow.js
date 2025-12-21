import React, { useState, useRef, useEffect } from "react";
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import TextInput from '../../../Components/Input/TextInput';
import { Button } from '@progress/kendo-react-buttons';
import { Formik } from 'formik';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { date } from "yup";
import { saveInnBasic, validateBasicField } from "../InnProjectBasic/InnProjectBasicService";

const InnProjectAddWindow = props => {
    const dimensions = WindowResizehook();
    let { visible, onClose, saveEvent } = props
    const INN_YEAR = (new Date().getFullYear() - 1911).toString()
    //表單預設資料
    const [formData, setFormData] = useState({ "INN_PLAN_NAME": "" });

    //表單異動資料
    const formRef = useRef();

    /**
     * 存檔
     * @param {*} data 
     */
    const saveChanges = async (data) => {
        let savedata = {
            InnProjectBasic: { ...data, INN_YEAR }
        }
        if (formRef.current) {
            // 若為新增計畫，跳出確認視窗
            showGlobalConfirmBox("是否確定新增計畫？", () => AddInnData(savedata))
        }

    }

    /**
     * 新增提案
     * @param {*} data 
     */
    const AddInnData = async (data) => {
        SetMaskOnOff(true);
        let saveResult = await saveInnBasic(data)
        if (saveResult.success) {
            showGlobalMessageBox(saveResult.message, () => {
                saveEvent(saveResult.data, data.InnProjectBasic.INN_PLAN_NAME);
            })
        }
        SetMaskOnOff(false);
    }



    /**
     * 取消
     */
    const cancel = (event) => {
        event.preventDefault();
        if (formRef.current) {
            // 重置表單為初始值
            formRef.current.resetForm({values:formData});
        }
    };

    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title='新增提案'
                        onClose={() => { onClose() }}
                        initialWidth={dimensions.width > 700 ? 800 : dimensions.width * .7}
                        initialHeight={dimensions.height > 700 ? 500 : dimensions.height * .8}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <Formik
                            initialValues={formData}
                            onSubmit={(data) => saveChanges(data)}
                            enableReinitialize={true}
                            innerRef={formRef}
                            validationSchema={validateBasicField}
                        >
                            {props => {
                                const {
                                    values,
                                    errors,
                                    handleChange,
                                    handleSubmit,
                                } = props;
                                return (
                                    <form>
                                        <Button title="存檔" onClick={handleSubmit} style={{ marginLeft: '5px' }}>存檔</Button>
                                        <Button title="取消" onClick={cancel} style={{ marginLeft: '10px' }}>取消</Button>
                                        <table>
                                            <tr>
                                                <th>
                                                    計畫編號
                                                </th>
                                                <td>
                                                    {!IsNullOrEmpty(values.INN_PLAN_NO) ? values.INN_PLAN_NO : "(存檔後產生)"}
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    計畫名稱
                                                </th>
                                                <td >
                                                    <TextInput
                                                        name="INN_PLAN_NAME"
                                                        value={values.INN_PLAN_NAME}
                                                        onChange={handleChange}
                                                        error={errors.INN_PLAN_NAME}
                                                        style={{ width: "100%" }}
                                                        maxLength={100}
                                                    />
                                                </td>
                                            </tr>

                                        </table>
                                    </form>
                                )
                            }}
                        </Formik>
                    </Window>
                </div >
            }
        </>
    )
}

export default InnProjectAddWindow;