import React from 'react';
import { Formik } from 'formik';
import * as Yup from 'yup';
import Table from '../../../Css/custom/Table.module.css';
import { PageContainer } from '../../../Basic/PageContainer';
import { IsNullOrEmpty, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { CheckIsHANDRole } from '../../../Basic/CommonService';
import { GetBasicData } from '../../../Basic/BasicData';
import TextInput from '../../../Components/Input/TextInput';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { CascadeDropDown } from '../../../Components/Dropdowns/CascadeDropDown';
import { Button } from '@progress/kendo-react-buttons';
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import { Checkbox } from "@progress/kendo-react-inputs";
import { getAllDropDowns, loadCheckPoint } from "../ProjectList/ProjectListService";

export const ProjectListtExecQueryForm = props => {

    let visible = props.visible ? true : false;

    //紀錄查詢資料
    const [queryData, setQueryData] = React.useState({});
    // 查詢條件下拉式選單
    const [ddlData, setDdlData] = React.useState({})
    // 是否"只有"主辦權限
    const [isHANDRole, setIsHANDRole] = React.useState(false);

    //查詢送出
    const onSubmit = async (data) => {
        // 查詢前將計畫狀態調整為API欄位格式
        const projStatus = data.PROJECT_STATUS;
        props.queryData({ ...data, PROJECT_STATUS: projStatus.map(x => x.value) });
    }

    //清除
    const clearConditions = () => {
        loadData(true);
    }

    /**
     * 載入資料
     */
    const loadData = async (isClearConditions = false) => {
        SetMaskOnOff(true);
        setIsHANDRole(await CheckIsHANDRole());
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
            DEPT: dropDownDatas[3],
            PROJECT_YEAR_STATUS: [
                { value: "A", text: "含之前所有案件" },
                { value: "B", text: "含之前未結案件" }
            ]

        })

        // 設定form預設值
        let defaultValue = {
            PROJECT_YEAR: (new Date().getFullYear() - 1911).toString(),
            PROJECT_NAME: "",
            PROJECT_STATUS: [],
            PROJECT_NO: "",
            CP_KIND: dropDownDatas[1][0] ? dropDownDatas[1][0].value : "",
            SPEC_NOTE: dropDownDatas[2][0] ? dropDownDatas[2][0].value : "",
            PROJECT_YEAR_STATUS: "B",
            MASTER_DEPT: "",
            EXEC_DEPT: await GetBasicData("orgId"),
            ASS_DEPT: "",
            AGCY_DEPT: "",
            RUNWAY_C: ""
        }

        setQueryData(defaultValue);
        //將查詢資料傳回外層
        if (!isClearConditions) {
            await props.queryData(defaultValue);
        }
    }

    //視窗打開載入公司資料
    React.useEffect(() => {
        loadData();
    }, []);

    // 欄位驗證
    const validateField = Yup.object().shape({
        MASTER_DEPT: Yup.string().nullable()
            .test('MASTER_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && CheckDept(this.parent) ? false : true;
            }),
        EXEC_DEPT: Yup.string().nullable()
            .test('EXEC_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && CheckDept(this.parent) ? false : true;
            }),
        ASS_DEPT: Yup.string().nullable()
            .test('ASS_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && CheckDept(this.parent) ? false : true;
            }),
        AGCY_DEPT: Yup.string().nullable()
            .test('AGCY_DEPT', '「主管機關」、「執行機關」、「協辦機關」、「代辦機關」請至少選擇一項', function (item) {
                return isHANDRole && CheckDept(this.parent) ? false : true;
            })
    });

    const CheckDept = (dataItem) => {
        let data = [dataItem.MASTER_DEPT, dataItem.EXEC_DEPT, dataItem.ASS_DEPT, dataItem.AGCY_DEPT];
        return data.filter(x => !IsNullOrEmpty(x)).length === 0;
    }

    //點選後傳回外層點選的類型
    return (
        <React.Fragment>
            {
                visible &&
                <PageContainer>
                    <Formik
                        initialValues={queryData}
                        validationSchema={validateField}
                        onSubmit={(data) => onSubmit(data)}
                        enableReinitialize
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleSubmit,
                                handleChange,
                            } = props;
                            return (
                                <form onSubmit={handleSubmit}>
                                    <div className="fn-buttons">
                                        <Button className='k-button-lighten' type="submit">查詢</Button>
                                        <Button className='k-button-lighten' type="button" onClick={clearConditions}>清除</Button>
                                    </div>
                                    <table className={Table.fullWidth}>
                                        <tbody>
                                            <tr>
                                                <th>年度</th>
                                                <td style={{ display: "flex" }}>
                                                    <DropDownListWithValue
                                                        style={{ width: "150px" }}
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
                                                <th>作業階段</th>
                                                <td >
                                                    <MultiSelect
                                                        popupSettings={
                                                            { className: "dropdown-text-size" }
                                                        }
                                                        data={ddlData.PROJECT_STATUS}
                                                        textField="text"
                                                        dataItemKey="value"
                                                        onChange={(e) => {
                                                            setQueryData({ ...queryData, PROJECT_STATUS: e.target.value })
                                                        }}
                                                        value={values.PROJECT_STATUS}
                                                        placeholder={"請選擇"}
                                                    />
                                                </td>
                                                <th>計畫編號</th>
                                                <td>
                                                    <TextInput
                                                        name="PROJECT_NO"
                                                        value={values.PROJECT_NO}
                                                        onChange={handleChange}
                                                        error={errors.PROJECT_NO}
                                                        style={{ width: '100%' }}
                                                        onBlur={(e) => { setQueryData({ ...queryData, PROJECT_NO: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>執行方式</th>
                                                <td>
                                                    <CascadeDropDown
                                                        fristDdlData={ddlData.CP_KIND}
                                                        firstDdlValue={values.CP_KIND}
                                                        firstDdlError={errors.CP_KIND}
                                                        firstColumn={"CP_KIND"}
                                                        secondDdlInitData={[{ text: "請選擇", value: "" }]}
                                                        secondDdlValue={values.RUNWAY_C}
                                                        secondDdlError={errors.RUNWAY_C}
                                                        secondColumn={"RUNWAY_C"}
                                                        secondDdlStyle={{ width: '250px' }}
                                                        getSecondDdlData={(data) => loadCheckPoint(data)}
                                                        values={values}
                                                        setValues={(data) => {
                                                            setQueryData({ ...queryData, CP_KIND: data.CP_KIND, RUNWAY_C: data.RUNWAY_C })
                                                        }}
                                                    />
                                                </td>
                                                <th>特殊加註</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        data={ddlData.SPEC_NOTE}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.SPEC_NOTE}
                                                        onChange={(e) => { setQueryData({ ...queryData, SPEC_NOTE: e.value.value }) }}
                                                        style={{ width: '250px' }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>主管機關</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="MASTER_DEPT"
                                                        data={ddlData.DEPT}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.MASTER_DEPT}
                                                        error={errors.MASTER_DEPT}
                                                        onChange={(e) => { setQueryData({ ...queryData, MASTER_DEPT: e.target.value }) }}
                                                    />
                                                </td>
                                                <th>執行機關</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="EXEC_DEPT"
                                                        data={ddlData.DEPT}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.EXEC_DEPT}
                                                        error={errors.EXEC_DEPT}
                                                        onChange={(e) => { setQueryData({ ...queryData, EXEC_DEPT: e.target.value }) }}
                                                    />
                                                </td>
                                            </tr>
                                            <tr>
                                                <th>協辦機關</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="ASS_DEPT"
                                                        data={ddlData.DEPT}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.ASS_DEPT}
                                                        error={errors.ASS_DEPT}
                                                        onChange={(e) => { setQueryData({ ...values, ASS_DEPT: e.target.value }) }}
                                                    />
                                                </td>
                                                <th>代辦機關</th>
                                                <td>
                                                    <DropDownListWithValue
                                                        name="AGCY_DEPT"
                                                        data={ddlData.DEPT}
                                                        textField={"text"}
                                                        dataItemKey={"value"}
                                                        value={values.AGCY_DEPT}
                                                        error={errors.AGCY_DEPT}
                                                        onChange={(e) => { setQueryData({ ...queryData, AGCY_DEPT: e.target.value }) }}
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
        </React.Fragment >
    )
};

export default ProjectListtExecQueryForm;