import React from 'react';
import { Formik } from "formik";
import * as Yup from 'yup';
import Table from '../../../Css/custom/Table.module.css';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TextInput from '../../../Components/Input/TextInput';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';

const DelayPlanQueryForm = (props) => {
    const { ddlData, formData, formRef, getData } = props;

    // 欄位驗證
    const validateField = Yup.object().shape({
        MASTER_DEPT: Yup.string().nullable()
            .test('MASTER_DEPT', '「主管機關」和「執行機關」請至少選擇一項', function (item) {
                return formData.current.IS_HAND_ROLE && IsNullOrEmpty(item) && IsNullOrEmpty(this.parent.EXEC_DEPT) ? false : true;
            }),
        EXEC_DEPT: Yup.string().nullable()
            .test('EXEC_DEPT', '「主管機關」和「執行機關」請至少選擇一項', function (item) {
                return formData.current.IS_HAND_ROLE && IsNullOrEmpty(item) && IsNullOrEmpty(this.parent.MASTER_DEPT) ? false : true;
            }),
    });

    return (
        <Formik
            initialValues={formData.current}
            validationSchema={validateField}
            innerRef={formRef}
            onSubmit={(data) => getData(data)}
            enableReinitialize //允許重複賦予初始值
        >
            {prop => {
                const { values, errors, setValues } = prop;
                return (
                    <form>
                        <table className={Table.fullWidth}>
                            <tbody>
                                <tr>
                                    <th>落後類別</th>
                                    <td width="40%">
                                        <DropDownListWithValue
                                            style={{ width: '250px' }}
                                            data={[
                                                { value: "", text: "請選擇" },
                                                { value: "1", text: "規劃階段工作進度落後案件" },
                                                { value: "2", text: "施工階段工程進度落後案件" },
                                                { value: "3", text: "驗收階段工作進度落後案件" },
                                                { value: "4", text: "檢核點進度落後60天以上案件" },
                                                { value: "5", text: "工程進度落後15%以上案件" },
                                                { value: "6", text: "連續3個月工程進度落後案件" }
                                            ]}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.RPT_TYPE}
                                            onChange={(e) => {
                                                setValues({ ...values, RPT_TYPE: e.value.value, RPT_NAME: e.value.text });
                                                formData.current.RPT_NAME = e.value.text;
                                            }}
                                            error={errors.RPT_TYPE}
                                        />
                                    </td>

                                    <th>計畫年度</th>
                                    <td>
                                        <DropDownListWithValue
                                            style={{ width: '250px' }}
                                            data={ddlData.PLAN_YEAR}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.PROJECT_YEAR}
                                            onChange={(e) => setValues({ ...values, PROJECT_YEAR: e.value.value })}
                                            error={errors.PROJECT_YEAR}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th>主管機關</th>
                                    <td width="40%">
                                        <DropDownListWithValue
                                            style={{ width: '250px' }}
                                            data={ddlData.MASTER_ORGAN}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.MASTER_DEPT}
                                            onChange={(e) => { setValues({ ...values, MASTER_DEPT: e.value.value }) }}
                                            error={errors.MASTER_DEPT}
                                        />
                                    </td>

                                    <th>執行機關</th>
                                    <td>
                                        <DropDownListWithValue
                                            style={{ width: '250px' }}
                                            data={ddlData.EXEC_ORGAN}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.EXEC_DEPT}
                                            onChange={(e) => { setValues({ ...values, EXEC_DEPT: e.value.value }) }}
                                            error={errors.EXEC_DEPT}
                                        />
                                    </td>
                                </tr>

                                <tr>
                                    <th>計畫名稱</th>
                                    <td colSpan={3}>
                                        <TextInput
                                            name="PROJECT_NAME"
                                            style={{ width: "40%" }}
                                            value={values.PROJECT_NAME}
                                            onChange={(e) => { setValues({ ...values, PROJECT_NAME: e.value }) }}
                                            error={errors.PROJECT_NAME}
                                        />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </form>
                );
            }}
        </Formik>
    )
}

export default DelayPlanQueryForm;