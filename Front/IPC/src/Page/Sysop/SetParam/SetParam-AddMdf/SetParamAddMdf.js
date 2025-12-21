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
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';

const AddMdf = (props) => {
    const IsAdd = IsNullOrEmpty(props.setType) ? true : false
    // 流程主檔 SET_STAGE：流程關卡明細
    const [paramResult, setParamResult] = useState({
        SET_ITEM: props.setItem,
        SET_TYPE: props.setType,
        SET_VALUE: "",
        MEMO: ""
    });
    const [paramItemDDLList, setParamItemDDLList] = useState([]);

    const { showMessage } = useContext(MessageBoxContext);

    // 取得參數資料
    const getParamBySetType = async () => {
        let functionData = await setParamService.getParamBySetType(props.setItem,props.setType);
        setParamResult(functionData);
    }

    // 重新組下拉選單顯示的資料
    function listData(items) {
        return items.map(item => {
            const result = { text: item.SET_ITEM + '-' + item.SET_ITEM_NAME, value: item.SET_ITEM }
            return result;
        }).filter(f => f !== undefined);
    }

    // 取得參數類別下拉選單
    const getParamItemDDL = async () => {
        let paramItemDDLData = listData(await setParamService.getParamItemData(props.setItem));
        setParamItemDDLList(paramItemDDLData);
    }

    //欄位驗證
    const validateField = Yup.object().shape({
        SET_ITEM: Yup.string()
            .required('參數類別必填'),
        SET_TYPE: Yup.string()
            .required('參數代碼必填'),
        SET_VALUE: Yup.string()
            .required('參數數值必填')
    });

    // 存檔
    const save = async (data) => {
        //新增or修改 功能
        let response;
        if (IsAdd) {
            response = await setParamService.createParam(data);
        } else {
            response = await setParamService.updateParam(data);
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
        getParamItemDDL();
        if (!IsAdd) {
            //取得參數資料
            getParamBySetType();
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
                initialValues={paramResult}
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
                                        <th >參數類別</th>
                                        <td>
                                            <DropDownListWithValue
                                                name="SET_ITEM"
                                                data={paramItemDDLList}
                                                textField="text"
                                                dataItemKey="value"
                                                value={values.SET_ITEM}
                                                defaultItem={{ text: "請選擇", value: "" }}
                                                onChange={(e) => setValues({ ...values, SET_ITEM: e.target.value })}
                                                onBlur={handleBlur}
                                                error={errors.SET_ITEM}
                                                disabled={!IsAdd}
                                                width="300px" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th >參數代碼</th>
                                        <td>
                                            <TextInput
                                                name="SET_TYPE"
                                                value={values.SET_TYPE}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.SET_TYPE}
                                                readOnly={!IsAdd}
                                                disabled={!IsAdd} />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th scope="col">參數數值</th>
                                        <td>
                                            <TextInput
                                                name="SET_VALUE"
                                                value={values.SET_VALUE}
                                                onChange={handleChange}
                                                onBlur={handleBlur}
                                                error={errors.SET_VALUE} />
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