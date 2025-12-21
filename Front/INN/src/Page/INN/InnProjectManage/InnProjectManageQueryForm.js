import React, { useState } from 'react';
import { Formik } from 'formik';
import Table from '../../../Css/custom/Table.module.css';
import { PageContainer } from '../../../Basic/PageContainer';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import RadioBoxList from '../../../Components/Input/RadioBoxList';
import { getPlanYearList, getInnPropsalType } from "../../../Basic/CommonService";

export const InnProjectManageQueryForm = props => {

    let visible = props.visible ? true : false;

    //紀錄查詢資料
    const [queryData, setQueryData] = useState({})
    // 查詢條件下拉式選單
    const [ddlData, setDdlData] = useState({})

    /**
     * 查詢送出
     * @param {*} data 
     */
    const onSubmit = async (data) => {
        props.queryData({ ...data });
    }

    /**
     * 清除
     */
    const clearQueryConditions = () => {
        loadData(true);
    }

    /**
     * 載入資料
     */
    const loadData = async (isClearConditions = false) => {
        SetMaskOnOff(true);
        let Year = await getPlanYearList(false, 10, "B");
        let ProposalType = await getInnPropsalType((new Date().getFullYear() - 1911).toString());
        let dropDownDatas = [Year, ProposalType];
        if (dropDownDatas) {
            await initForm(dropDownDatas, isClearConditions);
        }
        SetMaskOnOff(false);
    }

    // 初始Form
    const initForm = async (dropDownDatas, isClearConditions) => {
        // 載入下拉選單資料
        setDdlData({
            INN_YEAR: dropDownDatas[0],
            PROPOSAL_TYPE: dropDownDatas[1]
        })


        // 設定form預設值
        let defaultValue = {
            INN_YEAR: (new Date().getFullYear() - 1911).toString(),
            INN_PLAN_NAME: "",
            INN_PLAN_NO: "",
            SPONSOR_SEX: "",
            PROPOSAL_TYPE:"",
            GROUP: ""

        }
        setQueryData(defaultValue);
        //將查詢資料傳回外層
        if (!isClearConditions) {
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

    /**
    * 下拉選單的 onChange 事件處理函數
    * @param {*} e 
    */
    const handleYearChange = async (e) => {
        // 更新年度
        SetMaskOnOff(true);

        setQueryData({ ...queryData, INN_YEAR: e.value.value })
        let result = await getInnPropsalType(e.value.value);
        setDdlData({
            ...ddlData,
            PROPOSAL_TYPE: result
        })
        // 更新Grids
        SetMaskOnOff(false);
    };

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
                                                            data={ddlData.INN_YEAR}
                                                            textField={"text"}
                                                            dataItemKey={"value"}
                                                            value={values.INN_YEAR}
                                                            onChange={handleYearChange}
                                                        />
                                                    </div>
                                                </td>
                                                <th>編號</th>
                                                <td width="40%">
                                                    <TextInput
                                                        name="INN_PLAN_NO"
                                                        value={values.INN_PLAN_NO}
                                                        onChange={handleChange}
                                                        error={errors.INN_PLAN_NO}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, INN_PLAN_NO: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>主要提案類別</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        style={{ width: "250px" }}
                                                        data={ddlData.PROPOSAL_TYPE}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.PROPOSAL_TYPE}
                                                        onChange={(e) => { setQueryData({ ...queryData, PROPOSAL_TYPE: e.value.value }) }}
                                                    />
                                                </td>
                                                <th>提案名稱</th>
                                                <td width="40%">
                                                    <TextInput
                                                        name="INN_PLAN_NAME"
                                                        value={values.INN_PLAN_NAME}
                                                        onChange={handleChange}
                                                        error={errors.INN_PLAN_NAME}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, INN_PLAN_NAME: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th >
                                                    組別
                                                </th>
                                                <td >
                                                    <RadioBoxList
                                                        group='GROUP'
                                                        valueField='value'
                                                        textField='text'
                                                        data={[

                                                            {
                                                                text: 'A組', value: 'A', checked: values.GROUP == 'A'
                                                            },
                                                            {
                                                                text: 'B組', value: 'B', checked: values.GROUP == 'B'
                                                            }
                                                        ]}
                                                        onChange={(e) => setQueryData({ ...values, GROUP: e.value})}
                                                    />
                                                </td>
                                                <th >
                                                    性別
                                                </th>
                                                <td >
                                                    <RadioBoxList
                                                        group='SPONSOR_SEX'
                                                        valueField='value'
                                                        textField='text'
                                                        data={[

                                                            {
                                                                text: '男', value: 'M', checked: values.SPONSOR_SEX == 'M'
                                                            },
                                                            {
                                                                text: '女', value: 'F', checked: values.SPONSOR_SEX == 'F'
                                                            },
                                                            {
                                                                text: '其他', value: 'O', checked: values.SPONSOR_SEX == 'O'
                                                            }

                                                        ]}
                                                        onChange={(e) => setQueryData({ ...values, SPONSOR_SEX: e.value})}
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