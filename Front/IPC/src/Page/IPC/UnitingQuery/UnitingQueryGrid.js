import React from "react";
import Table from '../../../Css/custom/Table.module.css';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import { Pageable } from "../../../Basic/BasicData";
import { PageContainer } from "../../../Basic/PageContainer";
import { FormatDate, SetMaskOnOff } from "../../../Basic/SDOExtension";
import { openProjectPrint } from "../../../Basic/CommonService";
import { Button } from "@progress/kendo-react-buttons";
import { Grid, GridNoRecords, GridColumn } from "@progress/kendo-react-grid";
import { getUnitingQuery, exportRPT } from "./UnitingQueryServer";
import { formatNumber } from '@telerik/kendo-intl';

const UnitingQueryGrid = (props) => {
    const {
        queryCondition,
        selectedColumnKey, // 選取的自選欄位key
        optionColumns, // 所有的自選欄位
        setShowQryResult,
        setQueryCondition,
        setSelectedColumnKey
    } = props;

    const selectedColumn = React.useRef(optionColumns.filter(x => selectedColumnKey.includes(x.Key)));

    // grid
    const [gridData, setGridData] = React.useState([]);
    const [paging, setPaging] = React.useState({ skip: 0, take: 20 });
    const [isLoad, setIsLoad] = React.useState(false);
    // 計畫名稱寬度是否自動
    const [isprojNameWidthAuto, setIsprojNameWidthAuto] = React.useState(true);

    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] });

    const dimensions = WindowResizehook();

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        SetMaskOnOff(true);
        let param = { ...queryCondition };

        // 以下欄位需要其他欄位組合，故複製出來進行加工
        let copySelectedColumnKey = [...selectedColumnKey];

        // 管考進度(預定/實際/差異)
        if (copySelectedColumnKey.includes("RDEC_RAD_PRG")) {
            copySelectedColumnKey.push("RDEC_RES_PRG")
            copySelectedColumnKey.push("RDEC_ACT_PRG")
        }
        // 檢核點(預定/實際)
        if (copySelectedColumnKey.includes("CHECKITEM")) {
            copySelectedColumnKey.push("RES_CHECKITEM")
            copySelectedColumnKey.push("ACT_CHECKITEM")
        }
        // 工程進度(預定/實際/差異)
        if (copySelectedColumnKey.includes("IPC_RAD_PRG")) {
            copySelectedColumnKey.push("IPC_RES_PRG")
            copySelectedColumnKey.push("IPC_ACT_PRG")
            copySelectedColumnKey.push("IPC_DIFF_PRG")
        }
        // 標案系統工程進度(預定/實際/差異)
        if (copySelectedColumnKey.includes("TEN_RAD_PRG")) {
            copySelectedColumnKey.push("TEN_RES_PRG")
            copySelectedColumnKey.push("TEN_ACT_PRG")
            copySelectedColumnKey.push("TEN_DIFF_PRG")
        }

        // 後端預設會撈這兩個欄位資料，故排除，避免SQL重複組
        copySelectedColumnKey = copySelectedColumnKey.filter(x => x !== "PROJECT_NO" && x !== "PROJECT_NAME");

        param.SELECTED_COLUMN = optionColumns.filter(x => copySelectedColumnKey.includes(x.Key));

        param.STATISTICS_MONTH = formatNumber(param.STATISTICS_MONTH, "00");

        // 建設類別、主要建設、附屬設施、相關審查、特殊加註、會議種類、落後類型、平時管考意見備註
        let multiSelectKey = ["BUILD_KIND", "MAIN_BUILD", "SUB_BUILD",
            "REVIEWITEM", "SPEC_NOTE", "CONFERENCE_GENRE", "DELAY_TYPE", "COM_IPCMEMO"];

        multiSelectKey.forEach(x => {
            if (param[x].length > 0) {
                param[x] = param[x].map(x => x.SET_TYPE);
            }
        });

        // 立案時間
        if (param.CREATEDTIME != null) {
            param.CREATEDTIME = FormatDate(param.CREATEDTIME, 'YYYY/MM/DD')
        }
        // 開竣工區間
        if (param.COMPLETED_START != null) {
            param.COMPLETED_START = FormatDate(param.COMPLETED_START, 'YYYY/MM/DD')
        }
        if (param.COMPLETED_END != null) {
            param.COMPLETED_END = FormatDate(param.COMPLETED_END, 'YYYY/MM/DD')
        }

        let data = await getUnitingQuery(param);
        setGridData(data);
        setIsLoad(true);

        let sumWidth = 0;
        selectedColumn.current.forEach(x => {
            sumWidth += x.Width ?? 100;
        });
        setIsprojNameWidthAuto(dimensions.width > sumWidth);


        SetMaskOnOff(false);
    }

    const getCell = (dataItem, column) => {
        let val = dataItem[column.Key];

        let info = {
            PROJECT_NO: dataItem["PROJECT_NO"],
            PROJECT_NAME: dataItem["PROJECT_NAME"],
        }
        // 計畫名稱
        if (column.Key === "PROJECT_NAME") {
            val = <a onClick={() => openProjectPrint(info, "list", 0)}>{dataItem["PROJECT_NAME"]}</a>;
        }

        // 數字需靠右顯示
        return (
            <td className={column.Type === 4 ? Table.textAlign_right : ""}>
                <div style={{ whiteSpace: "pre-wrap" }}>{val}</div>
            </td>
        )
    }

    // 匯出
    const exportPlan = async (extension) => {
        let columns = [];
        GridDataForExport.current.Columns.forEach(item => {
            if (item.field != null)
                columns.push({ Title: item.title, Field: item.field })
            // for 動態欄位查詢，ex:綜合查詢
            else if (item.exportTitle != null && item.exportField)
                columns.push({ Title: item.exportTitle, Field: item.exportField })
        });
        let data = GridDataForExport.current.props.exportData;
        let obj = { Header: columns, Data: data, Format: extension, OutputName: "綜合查詢" };
        exportRPT(obj);
    }

    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">查詢結果</h3>
                    <Button title='回到上一頁' className="k-button-lighten"
                        onClick={() => {
                            setQueryCondition(queryCondition)
                            setSelectedColumnKey(selectedColumnKey)
                            setShowQryResult(false)
                        }} >回到上一頁</Button>
                    <Button title="匯出Excel" className='k-button-lighten' disabled={gridData.length === 0} onClick={() => exportPlan('xlsx')}>匯出Excel</Button>
                    <Button title="匯出Ods" className='k-button-lighten' disabled={gridData.length === 0} onClick={() => exportPlan('ods')}>匯出Ods</Button>
                </>
            }
        >
            {
                isLoad &&
                <Grid
                    ref={(e) => {
                        if (e != null) {
                            GridDataForExport.current.Columns = e.columns;
                            GridDataForExport.current.props = e.props;
                        }
                    }}
                    exportData={gridData}
                    data={gridData.slice(paging.skip, paging.take + paging.skip)}
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    total={gridData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                >
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn title="序號" width={40} cell={(prop) => <td style={{ textAlign: "center" }}>{prop.dataIndex + 1}</td>} />
                    {
                        selectedColumn.current
                            ? selectedColumn.current.map(item =>
                                <GridColumn
                                    exportTitle={item.Title}
                                    exportField={item.Key}
                                    title={<div style={{ whiteSpace: "pre-wrap" }}>{item.Title}</div>}
                                    cell={(x) => getCell(x.dataItem, item)}
                                    width={`${item.Key === "PROJECT_NAME"
                                        ? isprojNameWidthAuto ? '' : '500'
                                        : `${item.Width ?? 100}px`}`}
                                />
                            )
                            : null
                    }
                </Grid>
            }
        </PageContainer>
    );
}
export default UnitingQueryGrid;