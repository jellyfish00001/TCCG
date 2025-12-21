import React, { useState, useEffect, useContext } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import '@progress/kendo-theme-default/dist/all.css';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import setParamService from '../setparam.service';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'
import WindowBox from '../../../../Components/Dialogs/WindowBox';
const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.setItem) ? true : false
    // 流程主檔 SET_STAGE：流程關卡明細
    const [paramItemResult, setParamItemResult] = useState({
        SET_ITEM: props.setItem,
        SET_ITEM_NAME: "",
        MEMO: ""
    });
    const { showMessage } = useContext(MessageBoxContext);

    // 取得參數類別
    const getParamItemBySetItem = async () => {
        let functionData = await setParamService.getParamItemBySetItem(props.setItem);
        setParamItemResult(functionData);
    }
    //欄位驗證
    const validateField = Yup.object().shape({
        SET_ITEM: Yup.string()
            .required('功能代碼必填'),
        SET_ITEM_NAME: Yup.string()
            .required('功能名稱必填')
    });

     // 存檔
     const save = async (data) => {
        //新增or修改 功能
        let response;
        if (IsAdd) {
            response = await setParamService.createParamItem(data);
        } else {
            response = await setParamService.updateParamItem(data);
        }

        showMessage(response.result, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                    props.refreshGrid();
                }
            }
        })
    }

    useEffect(() => {
        if (!IsAdd) {
            //取得參數類別
            getParamItemBySetItem();
        }
    }, []);

    return (
        <WindowBox
            width={60}
            height={80}
            title={props.title}
            onClose={props.closeWindow}
        >
            <Formik
                initialValues={paramItemResult}
                validationSchema={validateField}
                onSubmit={(data) => save(data)}
                //允許重複賦予初始值
                enableReinitialize
            >
                {props => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleSubmit,
                        handleReset,
                        handleChange,
                        setValues
                    } = props;
                    return (
                        <form onSubmit={handleSubmit} onReset={handleReset}>
                            <div className="fn-buttons">
                                <Button type="submit">存檔</Button>
                                <Button type="reset">重設</Button>
                            </div>
                            <table >
                                <tbody>
                                    <tr>
                                        <th >功能類別代碼</th>
                                        <td>
                                            <TextInput
                                                name="SET_ITEM"
                                                value={values.SET_ITEM}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.SET_ITEM} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="col">功能名稱</th>
                                        <td>
                                            <TextInput
                                                name="SET_ITEM_NAME"
                                                value={values.SET_ITEM_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.SET_ITEM_NAME} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="col">流程說明</th>
                                        <td colspan="3">
                                            <textarea
                                                className="k-textarea"
                                                name="MEMO"
                                                value={values.MEMO}
                                                onChange={(e) => setValues({ ...values, MEMO: e.target.value })} />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </WindowBox>
    );
}
export default AddMdf;