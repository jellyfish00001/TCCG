import React from 'react';
import { Formik } from 'formik';
import Table from '../../../Css/custom/Table.module.css';
import { PageContainer } from '../../../Basic/PageContainer';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
//import { getAllDropDowns } from "../ProjectList/ProjectListService";

export const InnProjectManageQueryForm = props => {

    let visible = props.visible ? true : false;

    //紀錄查詢資料
    const [queryData, setQueryData] = React.useState({})
    // 查詢條件下拉式選單
    const [ddlData, setDdlData] = React.useState({})

    //查詢送出
    const onSubmit = async (data) => {
        // 查詢前將計畫狀態調整為API格式
        let projectStatus = data.PROJECT_STATUS;
        props.queryData({ ...data, PROJECT_STATUS: projectStatus.map(x => x.value) });
    }

    //清除
    const clearQueryConditions = () => {
        loadData(true);
    }

    /**
     * 載入資料
     */
    const loadData = async () => {
        SetMaskOnOff(true);
        //取得所有下拉清單資料
        // let dropDownDatas = await getAllDropDowns();
        // if (dropDownDatas) {
        //     await initForm(dropDownDatas, isClearConditions);
        // }
        SetMaskOnOff(false);
    }

    // 初始Form
    const initForm = async (dropDownDatas, isClearConditions) => {
        // 載入下拉選單資料
        setDdlData({
            // PROJECT_STATUS: dropDownDatas[0],
            PLAN_YEAR: dropDownDatas[4],
            // CP_KIND: dropDownDatas[1],
            // SPEC_NOTE: dropDownDatas[2],
            // EXEC_ORGAN_C: dropDownDatas[3],
            PLAN_CLASS: [
                { value: "A", text: "TEST" },
                { value: "B", text: "TEST2" }
            ]
        })


        // 設定form預設值
        let defaultValue = {
            PLAN_CLASS: "B"
        }

        setQueryData(defaultValue);
        //將查詢資料傳回外層
        if (!isClearConditions) {
            // await props.queryData({ ...defaultValue, PROJECT_STATUS: projStatusDefaultList });
            await props.queryData({ ...defaultValue });
        }
    }

    React.useEffect(() => {
        loadData();
    }, []);

    React.useEffect(() => {
        // 恢復查詢條件至初始值
        if (props.defaultQueryConditionCnt !== 0) {
            clearQueryConditions();
        }
    }, [props.defaultQueryConditionCnt])

    //點選後傳回外層點選的類型
    return (
        <>
            {
                visible &&
                <PageContainer>
                    <Formik
                        initialValues={queryData}
                        onSubmit={(data) => onSubmit(data)}
                        enableReinitialize
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleChange,
                                handleSubmit,
                            } = props;
                            return (
                                <form onSubmit={handleSubmit}>
                                    <div className="fn-buttons">
                                        <Button className='k-button-lighten' type="submit">查詢</Button>
                                        <Button className='k-button-lighten' type="button" onClick={clearQueryConditions}>取消</Button>
                                    </div>
                                    <table className={Table.fullWidth}>
                                        <tbody>
                                            <tr>
                                                <th>年度</th>
                                                <td>
                                                    <div style={{ display: "flex" }}>
                                                        <DropDownListWithValue
                                                            data={ddlData.PLAN_YEAR}
                                                            textField={"text"}
                                                            dataItemKey={"value"}
                                                            value={values.PLAN_YEAR}
                                                            onChange={(e) => { setQueryData({ ...queryData, PLAN_YEAR: e.value.value }) }}
                                                        />
                                                    </div>
                                                </td>
                                                <th>編號</th>
                                                <td width="40%">
                                                    <TextInput
                                                        name="PROJECT_NAME"
                                                        value={values.PLAN_NO}
                                                        onChange={handleChange}
                                                        error={errors.PLAN_NO}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PLAN_NO: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>主要提案類別</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        style={{ width: "250px" }}
                                                        data={ddlData.PLAN_CLASS}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.PLAN_CLASS}
                                                        onChange={(e) => { setQueryData({ ...queryData, PLAN_CLASS: e.value.value }) }}
                                                    />
                                                </td>
                                                <th>提案名稱</th>
                                                <td width="40%">
                                                    <TextInput
                                                        name="PLAN_NAME"
                                                        value={values.PLAN_NAME}
                                                        onChange={handleChange}
                                                        error={errors.PLAN_NAME}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PLAN_NAME: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th >
                                                    組別
                                                </th>
                                                <td >
                                                    <RadioBoxList
                                                        group='NUMBEROfPEOPLE'
                                                        valueField='value'
                                                        textField='text'
                                                        data={[

                                                            {
                                                                text: 'A組', value: 'A',
                                                            },
                                                            {
                                                                text: 'B組', value: 'B',
                                                            }
                                                        ]}
                                                    />
                                                </td>
                                                <th >
                                                    性別
                                                </th>
                                                <td >
                                                    <RadioBoxList
                                                        group='GENDER'
                                                        valueField='value'
                                                        textField='text'
                                                        data={[

                                                            {
                                                                text: '男', value: 'A',
                                                            },
                                                            {
                                                                text: '女', value: 'B',
                                                            },
                                                            {
                                                                text: '其他', value: 'O',
                                                            }

                                                        ]}
                                                    />
                                                </td>

                                            </tr>
                                        </tbody>
                                    </table>
                                </form>
                            );
                        }}
                    </Formik>
                </PageContainer>
            }
        </>
    )
};

export default InnProjectManageQueryForm;