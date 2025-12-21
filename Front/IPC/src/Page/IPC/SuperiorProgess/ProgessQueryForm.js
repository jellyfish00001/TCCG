import React from 'react';
import { Formik } from "formik";
import * as Yup from 'yup';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { MultiSelect } from "@progress/kendo-react-dropdowns";

const ProgessQueryForm = (props) => {
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
                        <table>
                            <tr>
                                <th>建設類別</th>
                                <td colSpan={3}>
                                    <MultiSelect
                                        popupSettings={
                                            { className: "dropdown-text-size" }
                                        }
                                        placeholder="請選擇"
                                        name='BUILD_KIND'
                                        data={ddlData.COM_PLANKIND}
                                        textField={"SET_VALUE"}
                                        dataItemKey={"SET_TYPE"}
                                        value={values.BUILD_KIND}
                                        onChange={(e) => {
                                            setValues({ ...values, BUILD_KIND: e.value.length === 0 ? "" : e.value })
                                        }}
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
                        </table>
                    </form>
                );
            }}
        </Formik>
    )
}

export default ProgessQueryForm;