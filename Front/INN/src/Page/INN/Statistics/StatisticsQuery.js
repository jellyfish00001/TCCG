import React from 'react';
import { Formik } from "formik";
import { PageContainer } from "../../../Basic/PageContainer";
import { GetHistory } from '../../../Basic/BasicData';
import { SetMaskOnOff, IsNullOrEmpty } from '../../../Basic/SDOExtension';
import { CheckIsHANDRole } from "../../../Basic/CommonService";
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import { Button } from "@progress/kendo-react-buttons";
import { Error } from '@progress/kendo-react-labels';

import StatisticsService from './StatisticsService';

// 查詢-統計報表
const StatisticsQuery = (props) => {
    const {
        location: {
            state: {
                id,
                name,
                rpt_id
            } = {
                id: "",
                name: "",
            }
        }
    } = props;

    const [queryData, setQueryData] = React.useState(StatisticsService.initData);
    const formRef = React.useRef(null);

    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState(StatisticsService.initDdlData);



    const loadData = async () => {
        SetMaskOnOff(true);

        let checkIsHANDRole = await CheckIsHANDRole();
        let ddl = await StatisticsService.getPageData(checkIsHANDRole);
        setDdlData(ddl);
        let queryData = { ...StatisticsService.initData };

        setQueryData({
            ...queryData,
            STATISTICS_ID: id,
            STATISTICS_NAME: name,
            RPT_ID: rpt_id,

        })

        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    const buildButton = () => {
        if (id === 1) {
            return (
                <>
                    <Button type="button" title='年度提案清冊' className="k-button-lighten" onClick={() => { handleSubmit() }} >年度提案清冊</Button>
                </>
            )
        }
        else if (id === 2) {
            return (
                <>
                    <Button type="button" title='各主題統計' className="k-button-lighten" onClick={() => { handleSubmit() }} >各主題統計</Button>
                </>
            )
        }
        else if (id === 3) {
            return (
                <>
                    <Button type="button" title='各年度提案統計' className="k-button-lighten" onClick={() => { handleSubmit() }} >各年度提案統計</Button>
                </>
            )
        }
    }

    // 利用Ref把Formik的submit功能拉出來，以提供外部按鈕呼叫
    const handleSubmit = () => {
        if (formRef.current) {
            formRef.current.handleSubmit()
        }
    }


    const submit = (data) => {
        StatisticsService.exportRPT({ ...data, STATISTICS_NAME: getStatisticsName(data) });
    }

    /**
     * 組成報表名稱
     * @param {object} data 查詢條件
     * @returns 
     */
    const getStatisticsName = (data) => {
        let titleWithDate = `桃園市政府${data.INN_YEAR}年創新提案`;

        // 報表編號
        switch (data.STATISTICS_ID) {
            case 1:
                return `${titleWithDate}年度提案清冊`;
            case 2:
                return `${titleWithDate}各主題統計表`
            case 3:
                return `${titleWithDate}各年度提案數統計表`
            default:
                return '';
        }
    }

    return (
        <PageContainer style={{
            height: '100%',
            overflow: 'auto'
        }}
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">報表列印</h3>
                    <Button title='回報表清單' className="k-button" onClick={() => { GetHistory().push('/Home/Report') }} >回報表清單</Button>
                    {buildButton()}
                </>
            }>
            <Formik
                initialValues={queryData}
                innerRef={formRef}
                onSubmit={(data) => submit(data)}
                enableReinitialize //允許重複賦予初始值
            >
                {prop => {
                    const {
                        values,
                        errors,
                        setValues
                    } = prop;
                    return (
                        <form>
                            <table>
                                <tr>
                                    <th>報表名稱</th>
                                    <td>{name}</td>
                                </tr>
                                <tr >
                                    <th>年度</th>
                                    <td>
                                        <div style={{ display: "flex", alignItems: "center" }}>
                                            <DropDownListWithValue
                                                name="PROJECT_YEAR"
                                                data={ddlData.PLAN_YEAR}
                                                textField={"text"}
                                                dataItemKey={"value"}
                                                value={values.INN_YEAR}
                                                onChange={(e) => { setValues({ ...values, INN_YEAR: e.target.value }) }}
                                            />

                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </form>
                    );
                }}
            </Formik>
        </PageContainer>
    )
}
export default StatisticsQuery;
