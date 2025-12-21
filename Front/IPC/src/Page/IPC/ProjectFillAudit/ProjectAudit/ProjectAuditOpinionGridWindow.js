import * as React from 'react';
import { Button } from "@progress/kendo-react-buttons";
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { DropDownListWithValue } from '../../../../Components/Dropdowns/DropDownListWithValue';
import { Checkbox } from '@progress/kendo-react-inputs';
import TextAreaInput from '../../../../Components/Input/TextAreaInput';
import { handleEditedGridData } from '../../../../Basic/SDOExtension';
import { formatNumber } from '@telerik/kendo-intl';
import { Formik } from "formik";
import * as Yup from 'yup';

export const ProjectAuditOpinionGridWindow = (props) => {
    const { data, gridData, setGridData, ipcMemoDdlData, planYearDdlData, editedGridData, close } = props;
    const formRef = React.useRef(null);
    const [monthDdlData, setMonthDdlData] = React.useState([]);

    React.useEffect(() => {
        let monthData = [];
        for (let i = 1; i <= 12; i++) {
            monthData.push({ text: formatNumber(i, "00"), value: formatNumber(i, "00") });
        }
        setMonthDdlData(monthData);
    }, [data])

    // 組備註選項
    const rowbuild = (prop) => {
        let { values, setValues } = prop;
        let element = [];
        for (let i = 0; i < ipcMemoDdlData.length; i += 2) {
            let dataItem = [];
            for (let j = 0; j < 2; j++) {
                if (ipcMemoDdlData[i + j] != undefined) {
                    dataItem.push(ipcMemoDdlData[i + j]);
                } else {
                    dataItem.push("");
                }
            }
            element.push(
                <div style={{ display: "flex" }}>
                    {dataItem.map(x => {
                        return (
                            IsNullOrEmpty(x) ? <td></td> :
                                <div style={{ width: "50%" }}>
                                    <Checkbox
                                        checked={values.ComIPCMemoMappingData.map(y => y.SET_TYPE).includes(x.SET_TYPE)}
                                        value={x.SET_TYPE}
                                        label={x.SET_VALUE}
                                        onChange={(e) => {
                                            let memoArr = [...values.ComIPCMemoMappingData];
                                            if (e.value) {
                                                memoArr.push({ SET_TYPE: e.target.element.value });
                                            } else {
                                                memoArr = values.ComIPCMemoMappingData.filter(y => y.SET_TYPE != e.target.element.value);
                                            }
                                            setValues({ ...values, ComIPCMemoMappingData: memoArr });
                                        }}
                                    />
                                </div>
                        )
                    }
                    )}
                </div>
            )
        }
        return element;

    }
    // 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    //利用Ref把Formik的reset功能拉出來，以提供外部按鈕呼叫
    const cancelChanges = () => {
        if (formRef.current) {
            formRef.current.handleReset();
        }
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        YEAR: Yup.number().required("此欄位為必填").nullable(),
        MONTH: Yup.number().required("此欄位為必填").nullable(),
        AUDIT_OPINION: Yup.string().nullable(),
    });

    // 確認
    const save = async (data) => {
        // 將資料加進grid
        if (data.editType === 1 && data.hiddenIndex === undefined) {
            const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
            const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;
            let newRecord = {
                ...data,
                hiddenIndex: LasthiddenIndex + 1
            };
            editedGridData.current.push(newRecord);
            setGridData([...gridData, newRecord]);
        }
        else {
            if (data.editType !== 1) {
                data.editType = 2;
            }
            let index = data.hiddenIndex ?
                gridData.findIndex(record => record.hiddenIndex === data.hiddenIndex)
                :
                gridData.findIndex(record => record.SEQ === data.SEQ);

            gridData.splice(index, 1, data);
            handleEditedGridData(editedGridData, data, 'hiddenIndex', 'SEQ');
            setGridData([...gridData]);
        }
        // 關閉視窗
        close();
    }

    return (
        <>
            <div className='fn-buttons'>
                <Button type="button" title="確認" onClick={() => handleSubmit()}>確認</Button>
                <Button type="button" title="取消" className="fn-buttons k-button-lighten" onClick={() => cancelChanges()}>取消</Button>
            </div>
            <Formik
                initialValues={data}
                validationSchema={validateField}
                innerRef={formRef}
                onSubmit={save}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                        errors,
                        handleBlur,
                        handleSubmit,
                        handleChange,
                        setValues
                    } = prop;
                    return (

                        <form onSubmit={handleSubmit}>
                            <table>
                                <tbody>
                                    <tr>
                                        <th className='addRedStar'>期間</th>
                                        <td colSpan={2}>
                                            <div style={{ display: "inline-flex" }}>
                                                <DropDownListWithValue
                                                    data={planYearDdlData}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.YEAR}
                                                    onChange={(e) => { setValues({ ...values, YEAR: e.value.value }) }}
                                                />
                                                <DropDownListWithValue
                                                    data={monthDdlData}
                                                    textField={"text"}
                                                    dataItemKey={"value"}
                                                    value={values.MONTH}
                                                    onChange={(e) => { setValues({ ...values, MONTH: e.value.value }) }}
                                                />
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>管考意見</th>
                                        <td colSpan={2}>
                                            <TextAreaInput
                                                name="AUDIT_OPINION"
                                                rows={3}
                                                maxlength={200}
                                                defaultValue={values.AUDIT_OPINION == null ? "" : values.AUDIT_OPINION}
                                                style={{ width: "100%" }}
                                                onBlur={(e) => {
                                                    setValues({
                                                        ...values,
                                                        AUDIT_OPINION: e.target.element.current.value
                                                    })
                                                }}
                                                error={errors.AUDIT_OPINION}
                                            />
                                        </td>
                                    </tr>
                                    <tr>
                                        <th>備註</th>
                                        <td>{rowbuild(prop)}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </>
    )
}
export default ProjectAuditOpinionGridWindow;