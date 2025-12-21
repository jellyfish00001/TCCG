import React, { useState, useEffect, useRef } from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import Table from '../../../Css/custom/Table.module.css';
import { getPWSSDPlanList, loadAllDropDowns, importPWSSDPlans } from './PlanImportService'
import { showGlobalMessageBox } from '../../../Route/RootMiddleware';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import PWSSDPlanImportGrid from './PWSSDPlanImportGrid';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { NumericTextBox } from '@progress/kendo-react-inputs';

export const PWSSDPlanImport = () => {

    const [organDropDown, setOrganDropDown] = useState([]);
    const [sendStatusDropDown, setSendStatusDropDown] = useState([]);

    // 匯入狀態選單資料
    const isImportDdlData = useRef([
        { text: "請選擇", value: "" },
        { text: "未匯入", value: "0" },
        { text: "已匯入", value: "1" }
    ])

    // 查詢條件
    const [queryConditions, setQueryConditions] = useState({
        planYear: new Date().getFullYear() - 1911,
        organ: '',
        sendStatus: '',
        isImport: ''
    })

    // 已選擇匯入的計畫編號
    const checkedPlan = useRef([]);

    const [gridData, setGridData] = useState([]);

    useEffect(() => {
        loadDropDowns();
    }, [])

    // 取得下拉清單
    const loadDropDowns = async () => {
        SetMaskOnOff(true);
        let dropDowns = await loadAllDropDowns();
        if (dropDowns.length > 0) {
            // 主管機關
            setOrganDropDown([...dropDowns[0]]);
            // 審核狀態
            setSendStatusDropDown([...dropDowns[1]]);

            setQueryConditions({ ...queryConditions, organ: dropDowns[0][0].value, sendStatus: dropDowns[1][0].value })
        }
        await Query();
    }

    // 查詢
    const Query = async () => {
        let requestModel = {
            ...queryConditions, planYear: queryConditions.planYear.toString()
        }

        SetMaskOnOff(true);
        let result = await getPWSSDPlanList(requestModel);

        setGridData([...result]);
        SetMaskOnOff(false);
    }

    // 勾選欲匯入計畫
    const planCheck = (checkedPlans) => {
        checkedPlan.current = checkedPlans;
    }

    // 匯入
    const Import = async () => {
        if (checkedPlan.current.length > 0) {
            let result = await importPWSSDPlans(checkedPlan.current)
            if (result.success) {
                showGlobalMessageBox(result.message, () => { Query() })
            }
        } else {
            showGlobalMessageBox("請至少勾選一筆計畫");
        }
    }

    return (
        <PageContainer style={{
            height: '100%',
            overflow: 'auto',
        }}
            toolbar={
                <div style={{ display: "inline-flex" }}>
                    <Button title="先期資料匯入" className="k-button-lighten" onClick={Import}  >先期資料匯入</Button>
                    <Button title="查詢" className="k-button-lighten" onClick={() => { Query() }} >查詢</Button>
                </div >
            }>
            <form >
                <table className={Table.fullWidth}>
                    <tbody>
                        <tr>
                            <th>計畫年度</th>
                            <td>
                                <NumericTextBox
                                    min={1}
                                    max={999}
                                    value={queryConditions.planYear}
                                    onChange={(e) => {
                                        setQueryConditions({ ...queryConditions, planYear: e.target.value })
                                    }}
                                    onBlur={(e) => {
                                        if (e.target.value >= 1000 || e.target.value <= 0) {
                                            showGlobalMessageBox("請輸入民國年");
                                            setQueryConditions({ ...queryConditions, planYear: new Date().getFullYear() - 1911 })
                                        }
                                    }} />
                            </td>
                            <th>審核狀態</th>
                            <td>
                                <DropDownListWithValue
                                    data={sendStatusDropDown}
                                    textField={"text"}
                                    dataItemKey={"value"}
                                    value={queryConditions.sendStatus}
                                    onChange={(e) => {
                                        setQueryConditions({ ...queryConditions, sendStatus: e.target.value })
                                    }}
                                />
                            </td>
                        </tr>
                        <tr>

                        </tr>
                        <tr>
                            <th>主管機關</th>
                            <td>
                                <DropDownListWithValue
                                    data={organDropDown}
                                    textField={"text"}
                                    dataItemKey={"value"}
                                    value={queryConditions.organ}
                                    onChange={(e) => {
                                        setQueryConditions({ ...queryConditions, organ: e.target.value })
                                    }}
                                />
                            </td>
                            <th>匯入狀態</th>
                            <td>
                                <DropDownListWithValue
                                    data={isImportDdlData.current}
                                    textField={"text"}
                                    dataItemKey={"value"}
                                    value={queryConditions.isImport}
                                    onChange={(e) => {
                                        setQueryConditions({ ...queryConditions, isImport: e.target.value })
                                    }}
                                />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </form>
            <PWSSDPlanImportGrid
                data={gridData}
                planCheck={planCheck}
            />
        </PageContainer >
    );
}
export default PWSSDPlanImport