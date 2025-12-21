import React, { useState, useRef, useEffect } from "react";
import { Formik } from 'formik';
import * as Yup from 'yup';
import { Pageable } from '../../../Basic/BasicData';
import { PageContainer } from "../../../Basic/PageContainer";
import { FormatDate } from "../../../Basic/SDOExtension";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import TwDatePicker from "../../../Components/DateInputs/TwDatePicker";
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { getPccDSNOs, getPccmXls, DownloadPccmXls } from './GDBXlsService';
import TextInput from "../../../Components/Input/TextInput";

const GDBXls = () => {
    // 查詢條件
    const [formData] = useState({
        DSNO: "",
        SYNC_DATE: null,
        PCC_PROJECT_NO: "",
        PCC_PROJECT_NAME: ""
    });
    // Grid資料
    const [gridData, setGridData] = useState([]);
    // Grid 表頭清單
    const [headers, setHeaders] = useState({});
    const formRef = useRef(null);
    // 下拉清單
    const [ddlData, setDdlData] = useState({});
    // 分頁
    const [paging, setPaging] = useState({ skip: 0, take: 20 });
    // window 值
    const dimensions = WindowResizehook();

    /**
     * 下拉清單
     */
    const setDDL = async () => {
        // 工程標案資料集
        let ddlData = await getPccDSNOs();
        setDdlData({ DSNO: ddlData });
    }

    // 下拉清單
    useEffect(() => {
        setDDL();
    }, [])

    // 取表單資料
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    // 查詢
    const query = async (data) => {
        setHeaders({})
        setGridData([])

        const result = await getPccmXls(data);
        if (!result.success) {
            showGlobalMessageBox(result.message);
        }
        else {
            const model = result.data;
            setGridData(model.Contents);
            setHeaders(model.Headers);
        }
    }
    // 下載
    const Download = async () => {
        let data = formRef.current.values;
        await DownloadPccmXls(data);
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        DSNO: Yup.string().required("此欄位為必填"),
        SYNC_DATE: Yup.string().required("此欄位為必填").nullable()
    });

    // 日期 change 事件 (轉換格式)
    const onFormDatePickerChange = (e, setValues, values) => {
        let newObj = {};
        newObj[e.target.name] = e.target.value == null
            ? null
            : FormatDate(e.target.value, "YYYY-MM-DD");
        setValues({ ...values, ...newObj });
    }

    // 動態產生GridColsJsx
    const genDynamicCols = () => {
        const headerColKeys = Object.keys(headers);
        const headerColValues = Object.values(headers);

        const width = 90;
        const isSetWidth = dimensions.width < headerColKeys.length * width;

        const gridColsJsx = headerColKeys.map((key, i) => {
            let val = headerColValues[i];
            if (isSetWidth) {
                // 每5個字插入換行符號
                let parts = headerColValues[i].match(/.{1,5}/g);
                val = parts.join('<br />');
            }
            return (
                <GridColumn field={key} title={val} width={isSetWidth ? `${width}px` : ""}></GridColumn>
            )
        })
        return gridColsJsx
    }

    return (
        <PageContainer toolbar={
            <>
                <h3 className="k-dialog-titlebar">{"工程標案"}</h3>
                <Button type="button" onClick={handleSubmit}>查詢</Button>
                <Button type="button" onClick={Download}>下載</Button>
            </>
        }>
            <Formik
                initialValues={formData}
                onSubmit={query}
                innerRef={formRef}
                validationSchema={validateField}
                //允許重複賦予初始值
                enableReinitialize
            >
                {prop => {
                    const {
                        values,
                        errors,
                        setValues,
                        handleChange
                    } = prop;
                    return (
                        <form>
                            <table>
                                <tr>
                                    <th>工程標案資料集</th>
                                    <td>
                                        <DropDownListWithValue
                                            data={ddlData.DSNO}
                                            textField={"text"}
                                            dataItemKey={"value"}
                                            value={values.DSNO}
                                            onChange={(e) => {
                                                const value = e.value.value;
                                                setValues({ ...values, DSNO: value });
                                            }}
                                            error={errors.DSNO}
                                        />
                                    </td>
                                    <th>日期</th>
                                    <td>
                                        <TwDatePicker
                                            name="SYNC_DATE"
                                            format="yyy/MM/dd"
                                            onChange={(e) => {
                                                onFormDatePickerChange(e, setValues, values);
                                            }}
                                            value={values.SYNC_DATE == null ? null : new Date(values.SYNC_DATE)}
                                            error={errors.SYNC_DATE}
                                        />
                                    </td>
                                </tr>
                                <tr>
                                    <th>標案編號</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.PCC_PROJECT_NO}
                                            name="PCC_PROJECT_NO"
                                        />
                                    </td>
                                    <th>標案名稱</th>
                                    <td>
                                        <TextInput
                                            onChange={handleChange}
                                            value={values.PCC_PROJECT_NAME}
                                            name="PCC_PROJECT_NAME"
                                            error={errors.PCC_PROJECT_NAME}
                                        />
                                    </td>
                                </tr>
                            </table>
                        </form>
                    );
                }}
            </Formik>

            {
                Object.keys(headers).length > 0 &&
                <>
                    <Grid
                        data={gridData.slice(paging.skip, paging.take + paging.skip)}
                        exportData={gridData}
                        style={{
                            height: '100%',
                            overflow: 'auto',
                        }}
                        resizable={true}
                        skip={paging.skip}
                        take={paging.take}
                        pageable={Pageable}
                        total={gridData.length}
                        onPageChange={(e) => { setPaging({ skip: e.page.skip, take: e.page.take }) }}
                    >
                        <GridNoRecords>無資料</GridNoRecords>
                        {genDynamicCols()}
                    </Grid>
                </>
            }
        </PageContainer>
    )
}
export default GDBXls;