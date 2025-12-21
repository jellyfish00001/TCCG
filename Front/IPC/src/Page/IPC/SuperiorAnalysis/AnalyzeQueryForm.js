import React from 'react';
import { Formik } from "formik";
import * as Yup from 'yup';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';

const IPCAnalyzeQueryForm = (props) => {
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
                                <th>執行方式</th>
                                <td width={'40%'}>
                                    <RadioBoxList
                                        group='CP_KIND'
                                        valueField='value'
                                        textField='text'
                                        data={[
                                            { text: '總覽', value: '', checked: IsNullOrEmpty(values.CP_KIND) },
                                            { text: '工程類', value: '0', checked: values.CP_KIND === '0' },
                                            { text: '非工程類', value: '1', checked: values.CP_KIND === '1' },
                                        ]}
                                        onChange={(e) => setValues({ ...values, CP_KIND: e.value, CP_KIND_DESC: e.target.element.nextSibling.wholeText })}
                                    />
                                </td>

                                <th>結案狀態</th>
                                <td>
                                    <RadioBoxList
                                        group='IS_PROJECT_FINISH'
                                        valueField='value'
                                        textField='text'
                                        data={[
                                            { text: '全部', value: '', checked: IsNullOrEmpty(values.IS_PROJECT_FINISH) },
                                            { text: '已結案', value: 'Y', checked: values.IS_PROJECT_FINISH === 'Y' },
                                            { text: '未結案', value: 'N', checked: values.IS_PROJECT_FINISH === 'N' },
                                        ]}
                                        onChange={(e) => setValues({ ...values, IS_PROJECT_FINISH: e.value, IS_PROJECT_FINISH_DESC: e.target.element.nextSibling.wholeText })}
                                    />
                                </td>
                            </tr>

                            <tr>
                                <th>計畫經費</th>
                                <td colSpan={3}>
                                    <RadioBoxList
                                        group='PROJ_BUDGET'
                                        valueField='value'
                                        textField='text'
                                        data={[
                                            { text: '全部', value: '', checked: IsNullOrEmpty(values.PROJ_BUDGET) },
                                            { text: '1000萬以上', value: '0', checked: values.PROJ_BUDGET === '0' },
                                            { text: '3000萬以上', value: '1', checked: values.PROJ_BUDGET === '1' },
                                            { text: '5000萬以上', value: '2', checked: values.PROJ_BUDGET === '2' },
                                            { text: '1億以上', value: '3', checked: values.PROJ_BUDGET === '3' },
                                        ]}
                                        onChange={(e) => setValues({ ...values, PROJ_BUDGET: e.value, PROJ_BUDGET_DESC: e.target.element.nextSibling.wholeText })}
                                    />
                                </td>
                            </tr>

                            <tr>
                                <th>分析類別</th>
                                <td colSpan={3}>
                                    <RadioBoxList
                                        group='CHART_TYPE'
                                        valueField='value'
                                        textField='text'
                                        data={[
                                            { text: '建設類別', value: 0, checked: values.CHART_TYPE === 0 },
                                            { text: '主管機關', value: 1, checked: values.CHART_TYPE === 1 },
                                            { text: '辦理地點', value: 2, checked: values.CHART_TYPE === 2 },
                                            { text: '進度', value: 3, checked: values.CHART_TYPE === 3 },
                                        ]}
                                        onChange={(e) => setValues({ ...values, CHART_TYPE: e.value, CHART_TYPE_DESC: e.target.element.nextSibling.wholeText })}
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
                                        onChange={(e) => setValues({ ...values, MASTER_DEPT: e.value.value, MASTER_DEPT_DESC: e.value.value == "" ? "全部" : e.value.text })}
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
                                        onChange={(e) => setValues({ ...values, EXEC_DEPT: e.value.value, EXEC_DEPT_DESC: e.value.value == "" ? "全部" : e.value.text })}
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

export default IPCAnalyzeQueryForm;