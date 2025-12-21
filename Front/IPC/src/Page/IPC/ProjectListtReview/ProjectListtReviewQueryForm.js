import React from 'react';
import { Formik } from 'formik';
import Table from '../../../Css/custom/Table.module.css';
import { PageContainer } from '../../../Basic/PageContainer';
import { SetMaskOnOff } from "../../../Basic/SDOExtension";
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from '@progress/kendo-react-buttons';
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import { Checkbox } from "@progress/kendo-react-inputs";
import { getAllDropDowns } from "../ProjectList/ProjectListService";

export const ProjectListtReviewQueryForm = props => {

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
    const loadData = async (isClearConditions = false) => {
        SetMaskOnOff(true);
        //取得所有下拉清單資料
        let dropDownDatas = await getAllDropDowns();
        if (dropDownDatas) {
            await initForm(dropDownDatas, isClearConditions);
        }
        SetMaskOnOff(false);
    }

    // 初始Form
    const initForm = async (dropDownDatas, isClearConditions) => {
        // 載入下拉選單資料
        setDdlData({
            PROJECT_STATUS: dropDownDatas[0],
            PROJECT_YEAR: dropDownDatas[4],
            CP_KIND: dropDownDatas[1],
            SPEC_NOTE: dropDownDatas[2],
            EXEC_ORGAN_C: dropDownDatas[3],
            PROJECT_YEAR_STATUS: [
                { value: "A", text: "含之前所有案件" },
                { value: "B", text: "含之前未結案件" }
            ]
        })

        // // 計畫審查 計畫狀態預設值 => 2(立案審核)、5(結案審核)資料
        // const projStatusDefaultList = ['2', '5'];
        // let defaultProjectStatusVal = [];
        // if (dropDownDatas[0].length > 0) {
        //     defaultProjectStatusVal = dropDownDatas[0].filter(x => {
        //         return projStatusDefaultList.indexOf(x.value) !== -1
        //     })
        // }

        // 設定form預設值
        let defaultValue = {
            PROJECT_YEAR: (new Date().getFullYear() - 1911).toString(),
            PROJECT_NAME: "",
            // PROJECT_STATUS: defaultProjectStatusVal,
            PROJECT_STATUS: [],
            PROJECT_NO: "",
            CP_KIND: dropDownDatas[1][0] ? dropDownDatas[1][0].value : '',
            SPEC_NOTE: dropDownDatas[2][0] ? dropDownDatas[2][0].value : '',
            EXEC_ORGAN_C: dropDownDatas[3][0] ? dropDownDatas[3][0].value : '',
            PROJECT_YEAR_STATUS: "B"
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
                                        <Button className='k-button-lighten' type="button" onClick={clearQueryConditions}>清除</Button>
                                    </div>
                                    <table className={Table.fullWidth}>
                                        <tbody>
                                            <tr>
                                                <th>年度</th>
                                                <td colSpan={3}>
                                                    <div style={{ display: "flex" }}>
                                                        <DropDownListWithValue
                                                            data={ddlData.PROJECT_YEAR}
                                                            textField={"text"}
                                                            dataItemKey={"value"}
                                                            value={values.PROJECT_YEAR}
                                                            onChange={(e) => { setQueryData({ ...queryData, PROJECT_YEAR: e.value.value }) }}
                                                        />

                                                        {
                                                            ddlData.PROJECT_YEAR_STATUS !== undefined &&
                                                            ddlData.PROJECT_YEAR_STATUS.map(x => {
                                                                return (
                                                                    <>
                                                                        &nbsp;
                                                                        <Checkbox
                                                                            checked={values.PROJECT_YEAR_STATUS === x.value}
                                                                            value={x.value}
                                                                            label={x.text}
                                                                            onChange={(e) => {
                                                                                setQueryData({ ...queryData, PROJECT_YEAR_STATUS: e.value ? e.target.element.value : "" })
                                                                            }}
                                                                        />
                                                                    </>
                                                                )
                                                            })
                                                        }
                                                    </div>
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>作業階段</th>
                                                <td>
                                                    <MultiSelect
                                                        popupSettings={
                                                            { className: "dropdown-text-size" }
                                                        }
                                                        data={ddlData.PROJECT_STATUS}
                                                        textField="text"
                                                        dataItemKey="value"
                                                        onChange={(e) => { setQueryData({ ...queryData, PROJECT_STATUS: e.target.value }) }}
                                                        value={values.PROJECT_STATUS}
                                                        placeholder={"請選擇"}
                                                    />
                                                </td>
                                                <th>計畫名稱</th>
                                                <td width="40%">
                                                    <TextInput
                                                        name="PROJECT_NAME"
                                                        value={values.PROJECT_NAME}
                                                        onChange={handleChange}
                                                        error={errors.PROJECT_NAME}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PROJECT_NAME: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>特殊加註</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        style={{ width: "250px" }}
                                                        data={ddlData.SPEC_NOTE}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.SPEC_NOTE}
                                                        onChange={(e) => { setQueryData({ ...queryData, SPEC_NOTE: e.value.value }) }}
                                                    />
                                                </td>
                                                <th>計畫編號</th>
                                                <td>
                                                    <TextInput
                                                        name="PROJECT_NO"
                                                        value={values.PROJECT_NO}
                                                        error={errors.PROJECT_NO}
                                                        style={{ width: '100%' }}
                                                        onChange={handleChange}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PROJECT_NO: e.target.value }) }}
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

export default ProjectListtReviewQueryForm;