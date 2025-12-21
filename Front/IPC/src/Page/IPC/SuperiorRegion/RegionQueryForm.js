import React from 'react';
import { Formik } from "formik";
import * as Yup from 'yup';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';

const RegionQueryForm = (props) => {
    const { ddlData, formData, formRef, getData } = props;

    // 欄位驗證
    const validateField = Yup.object().shape({
        MASTER_DEPT: Yup.string().nullable()
            .test('MASTER_DEPT', '「主管機關」和「執行機關」請至少選擇一項', function (item) {
                return formData.IS_HAND_ROLE && IsNullOrEmpty(item) && IsNullOrEmpty(this.parent.EXEC_DEPT) ? false : true;
            }),
        EXEC_DEPT: Yup.string().nullable()
            .test('EXEC_DEPT', '「主管機關」和「執行機關」請至少選擇一項', function (item) {
                return formData.IS_HAND_ROLE && IsNullOrEmpty(item) && IsNullOrEmpty(this.parent.MASTER_DEPT) ? false : true;
            }),
    });

    return (
        <Formik
            initialValues={formData}
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
                                <th>工程階段</th>
                                <td colSpan={3}>
                                    <RadioBoxList
                                        group='ENGNEER_STAGE'
                                        valueField='value'
                                        textField='text'
                                        data={[
                                            { text: '總覽', value: '', checked: IsNullOrEmpty(values.ENGNEER_STAGE) },
                                            { text: '規劃階段', value: 'null', checked: values.ENGNEER_STAGE === 'null' },
                                            { text: '施工階段', value: 'A', checked: values.ENGNEER_STAGE === 'A' },
                                            { text: '驗收階段', value: 'B', checked: values.ENGNEER_STAGE === 'B' },
                                        ]}
                                        onChange={(e) => setValues({ ...values, ENGNEER_STAGE: e.value })}
                                        error={errors.ENGNEER_STAGE}
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

export default RegionQueryForm;