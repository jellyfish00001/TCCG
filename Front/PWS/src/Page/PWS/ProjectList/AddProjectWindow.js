import React, { useState, useRef, useEffect } from "react";
import { Formik, Form, Field } from 'formik';
import { Button } from '@progress/kendo-react-buttons';
import TextInput from '../../../Components/Input/TextInput';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { RadioButton } from '@progress/kendo-react-inputs';
import { showGlobalMessageBox, showGlobalConfirmBox } from "../../../Route/RootMiddleware";
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import { SavePWSSDPLANMAIN, SavePWSSDPLANMAINB} from "../AddNewPlan/AddNewPlanService";
import { openProjectChapter } from "../ProjectChapter/ProjectChapterService";

const AddProjectWindow = (props) => {
    //外部傳進setwin的資料
    const { closeWindow } = props;
    //表單異動資料
    const formRef = useRef();
    //表單資料
    const [formData, setFormData] = useState({});

    // 表單初始化資料
    useEffect(() => {

    }, []);

    /**
     * 存檔
     * @param {*} data 
     * @returns 
     */
    const save = async (data) => {
        if (!data.PLANNAME || !data.PLANKIND) {
            showGlobalMessageBox("請將欄位填寫完整");
            return;
        }
        showGlobalConfirmBox("是否確定新增計畫",async () => {
            let savedata = {...data};
            SetMaskOnOff(true);
            let saveResult = null;
            if(data.PLANKIND == "1"){
                saveResult = await SavePWSSDPLANMAIN(savedata);
            }else{
                saveResult = await SavePWSSDPLANMAINB(savedata);
            }
            SetMaskOnOff(false);
            // 成功訊息
            if (saveResult.success) {
                closeWindow();
                showGlobalMessageBox(saveResult.message, () => {
                    openProjectChapter(saveResult.data, 0);
                    window.location.reload();
                });
            }
        });
    }

    /**
     * 取消
     */
    const cancel = () => {
        closeWindow();
    };

    return (
        <WindowBox
            width={50}
            height={35}
            onClose={closeWindow}
            title={"新增計畫"}
        >
            <Formik
                initialValues={formData}
                onSubmit={(data) => save(data)}
                enableReinitialize={true}
                innerRef={formRef}
            >
                {props => {
                    const {
                        values,
                        errors,
                        handleChange,
                        handleSubmit,
                        setFieldValue,
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
                                        <span>【存檔後產生】</span>
                                    </td>
                                </tr>
                                <tr>
                                    <th className="addRedStar">
                                        計畫名稱
                                    </th>
                                    <td >
                                        <TextInput
                                            name="PLANNAME"
                                            value={values.PLANNAME}
                                            onChange={handleChange}
                                            error={errors.PLANNAME}
                                            style={{ width: "100%" }}
                                            maxLength={100}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th className="addRedStar">
                                        計畫類別
                                    </th>
                                    <td>
                                    <RadioButton
                                        name="PLANKIND"
                                        value={"1"}
                                        label="重大施政"
                                        checked={values.PLANKIND === "1"}
                                        onChange={() => setFieldValue("PLANKIND", "1")}
                                    />
                                    <RadioButton
                                        name="PLANKIND"
                                        value={"2"}
                                        label="委託研究"
                                        checked={values.PLANKIND === "2"}
                                        onChange={() => setFieldValue("PLANKIND", "2")}
                                    />
                                </td>
                                </tr>
                            </table>
                        </form>
                    )
                }}
            </Formik>

        </WindowBox>
    )
}
export default AddProjectWindow;