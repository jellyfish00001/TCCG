import React, { useState, useEffect, useContext, useRef } from 'react';
import DimRightService from '../dimRight.service';
import WindowBox from '../../../../Components/Dialogs/WindowBox';
import { Button } from '@progress/kendo-react-buttons';
import DualListBox from 'react-dual-listbox';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { MessageBoxContext } from '../../../../Components/Dialogs/MessageBox';
import { Formik } from 'formik';
import * as Yup from 'yup';
import TextInput from '../../../../Components/Input/TextInput'

const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.rightId) ? true : false
    const [rightData, setRightData] = useState({
        RIGHT_ID: "",
        RIGHT_NAME: "",
        FUNCTIONS: []
    })
    const [functionData, setFunctionData] = useState([]) //功能列表

    const { showMessage } = useContext(MessageBoxContext);

    useEffect(() => {
        //取得功能列表
        getFunction()

        //取得要修改的資料
        if (!IsAdd) {
            //取得權限清單ByRight_ID
            getDimRightByRightId(props.rightId)
        }
    }, []);

    //取得功能列表
    const getFunction = async () => {
        setFunctionData(await DimRightService.getFunction())
    }

    //取得權限清單ByRight_ID
    const getDimRightByRightId = async (rightId) => {
        let data = await DimRightService.getDimRightByRightId(rightId)
        setRightData(data)
    }

    //存檔
    const submit = async (data) => {
        let response;
        //新增or修改 權限
        if (IsAdd) {
            response = await DimRightService.insertDimRight(data)
        } else {
            response = await DimRightService.updateDimRight(data)
        }

        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    props.closeWindow();
                    props.refreshGrid();
                }
            }
        })
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        RIGHT_ID: Yup.string()
            .required('權利代碼必填'),
        RIGHT_NAME: Yup.string()
            .required('權限名稱必填')
    });

    return (
        <WindowBox title={props.title} onClose={props.closeWindow} >
            <Formik
                initialValues={rightData}
                validationSchema={validateField}
                onSubmit={(data) => submit(data)}
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
                            <table>
                                <tbody>
                                    <tr>
                                        <th >權限代碼</th>
                                        <td>
                                            <TextInput
                                                name="RIGHT_ID"
                                                value={values.RIGHT_ID}
                                                readOnly={!IsAdd}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.RIGHT_ID}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>權限名稱</th>
                                        <td>
                                            <TextInput
                                                name="RIGHT_NAME"
                                                value={values.RIGHT_NAME}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.RIGHT_NAME}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>功能列表</th>
                                        <td >
                                            <DualListBox
                                                options={functionData}
                                                selected={values.FUNCTIONS}
                                                onChange={(e) => setValues({ ...values, FUNCTIONS: e })}
                                            />
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