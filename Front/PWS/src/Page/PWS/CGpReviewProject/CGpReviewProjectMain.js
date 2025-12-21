import React, { useState, useRef, useEffect } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell"
import { Button } from '@progress/kendo-react-buttons';
import OrgSelectPanel from '../../../Components/Selector/OrgSelectPanel';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import PureHtmlTextAreaInput from '../../../Components/Input/PureHtmlTextAreaInput'
import { Formik } from 'formik';
import TextInput from '../../../Components/Input/TextInput';
import { ExportGrid } from '../../../Basic/Download';
import { openProjectChapter } from "../ProjectChapter/ProjectChapterService";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { NumericTextInputCell } from '../../../Components/GridCell/NumericTextInputCell';

const CGpReviewProjectMain = (props) => {
    // 這裡假設grid資料從外部加載
    const { data } = props;
    // 假資料
    const [fakedata, setFakedata] = useState([
        {
            Number: 1,
            Check: "建議核列",
            PublicBudget: 100,
            FundBudget: 100,
        },
        {
            Number: 1,
            Check: "不建議核列",
            PublicBudget: 200,
            FundBudget: 200,
        },
    ]);
    // 表單資料
    const [formData, setFormData] = useState({
        PROJECT_NO: "",
        PROJECT_NAME: "",
        PRO_STATUS: "",
        EXP_RESULT: "",
        DATE: "",
    });
    const formRef = useRef(null);
    // 下拉選單計畫類別
    const Options = [
        { text: "重大施政", value: "A" },
        { text: "委託研究", value: "B" },
    ];
    //利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    };
    // 發送稽催填報
    const saveChanges = () => {

    }
    // 輸入框輸入
    const onCellInputBlur = (item) => {

    }
    //改變輸入框
    const onCellInputChange = (item) => {

    }
    //數字輸入框
    const numerCell = (props) => {
        const value = props.dataItem[props.field];
        const displayValue = (value === null || value === undefined) ? 0 : value;
    return (
        <NumericTextInputCell
                {...props}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                onCellInputChange={onCellInputChange}
                AlwaysEdit={true}
                min={0}
            />
    );
    };
    // 刪除
    const clean = () => {
        setFormData({
            YEAR: "",
            PLAN_NO: "",
            PLAN_STATUS: "",
            PLAN_NAME: "",
            REP_AUTHER: "",
            REP_UNIT: "",
        });
    }

    return (
        <>
                <PageContainer style={{ overflow: "auto", height: "100%" }}>
                    <CollapseBoardCard
                        button={
                            <>
                                    <Button title="存檔" className="k-button-lighten" onClick={() => ({})}>存檔</Button>
                                    <Button title="取消" className="k-button-lighten" onClick={() => ({})}>取消</Button>
                                    <Button title="已審核" className="k-button-lighten" onClick={handleSubmit}>已審核</Button>
                                    <Button title="返回" className="k-button-lighten" onClick={clean}>返回</Button>
                            </>
                        }
                        title="經費需求事項"
                        isFirstArea={true}
                    > 
                    <Formik
                        initialValues={formData}
                        onSubmit={(data) => saveChanges(data)}
                        enableReinitialize={true}
                        innerRef={formRef}
                        //字段改變不驗證
                        validateOnChange={false}
                    >
                        {props => {
                            const {
                                values,
                                errors,
                                handleChange,
                                setValues
                            } = props;
                            return (
                                <form>
                                    <table>
                                        <tr>
                                            <th>
                                                 年度需求數（千元）
                                            </th>
                                            <td colSpan={3}>
                                                {values.YEAR}
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                計畫狀態
                                            </th>
                                            <td style={{width:"30%"}}>
                                                {values.PRO_STATUS}
                                            </td>
                                            <th>
                                                執行類別
                                            </th>
                                            <td>
                                                { values.PRO_TYPE === "A" ? "重大施政" : "委託研究" }
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                建議核列金額
                                            </th>
                                            <td colSpan={3}>
                                            <Grid
                                                style={{ overflow: 'auto', height: '100%' }}
                                                resizable={true}
                                                data={fakedata}
                                            >
                                                <GridNoRecords>無資料</GridNoRecords>
                                                <GridColumn field="Number" title="優先序" width="100" />
                                                <GridColumn field="Check" title=" " width="100" />
                                                <GridColumn field="PublicBudget" title="公務預算" cell={numerCell}/>
                                                <GridColumn field="FundBudget" title="基金預算" cell={numerCell}/>
                                            </Grid>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>
                                                專案小組審查意見
                                            </th>
                                            <td colSpan={3}>
                                                <TextInput
                                                    name="REP_AUTHER"
                                                    value={values.REP_AUTHER}
                                                    onChange={handleChange}
                                                    style={{ width: "50%" }}
                                                />
                                            </td>
                                        </tr>
                                    </table>
                                </form>
                            )
                        }}
                    </Formik>
                    </CollapseBoardCard>
                </PageContainer>
        </>
    );
}
export default CGpReviewProjectMain;