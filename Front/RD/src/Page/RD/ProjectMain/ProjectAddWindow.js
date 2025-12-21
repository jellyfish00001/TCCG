import React, { useState, useRef } from "react";
import { Window } from '@progress/kendo-react-dialogs';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import TextInput from '../../../Components/Input/TextInput';
import { Button } from '@progress/kendo-react-buttons';
import { Formik } from 'formik';
import { showGlobalMessageBox, showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { SaveRDResearchBasic, validationSchema } from "./ProjectMainService";

const ProjectAddWindow = (props) => {
    const dimensions = WindowResizehook();
    let { visible, onClose, saveEvent } = props
    // 表單預設資料
    const [formData, setFormData] = useState({"PLAN_NAME":"" });
    // 表單異動資料
    const formRef = useRef();
    /**
     * 新增計畫（存檔）
     */
    const savePlan = async (data) => {
        showGlobalConfirmBox("是否確定新增計畫？", () => savePlanEvent(data.PLAN_NAME))
    }
    /**
     * 新增計畫事件（存檔）
     */
    const savePlanEvent = async (planName) => {
        SetMaskOnOff(true);
        let result = await SaveRDResearchBasic(planName)
        if (result.success) {
            showGlobalMessageBox(result.message, () => {
                saveEvent(result.data, planName);
            })
        }
        SetMaskOnOff(false);
    }
    /**
     * 表單取消
     */
    const cancel = (event) => {
        event.preventDefault();
        if (formRef.current) {
            // 重置表單
            formRef.current.resetForm({ values: formData });
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
                            onSubmit={(data) => savePlan(data)}
                            enableReinitialize={true} // 允許重複賦予初始值，要外部傳入 initialValues 更新資料
                            validationSchema={validationSchema}
                            innerRef={formRef}
                    >
                            {props => {
                                const {
                                    values,
                                    errors,
                                    handleChange,
                                    handleSubmit
                                } = props;
                                return (
                                    <form>
                                        <div className="fn-buttons">
                                            <Button title="存檔" onClick={handleSubmit}>存檔</Button>
                                            <Button title="取消" onClick={cancel}>取消</Button>
                                        </div>
                                        <table>
                                            <tr>
                                                <th>
                                                    計畫編號
                                                </th>
                                                <td>
                                                    (存檔後產生)
                                                </td>
                                            </tr>
                                            <tr>
                                                <th className="addRedStar">
                                                    計畫名稱
                                                </th>
                                                <td >
                                                    <TextInput
                                                        name="PLAN_NAME"
                                                        value={values.PLAN_NAME}
                                                        onChange={handleChange}
                                                        error={errors.PLAN_NAME}
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

export default ProjectAddWindow;