import React from 'react';
import { updateProjectPisSelect } from "../ProjectList/ProjectListService.js";
import { PageContainer } from "../../../Basic/PageContainer";
import { IsNullOrEmpty, SetMaskOnOff } from '../../../Basic/SDOExtension';
import { CheckIsHANDRole } from '../../../Basic/CommonService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { ExportGrid } from '../../../Basic/Download';
import { Button } from "@progress/kendo-react-buttons";

import Service from './SuperiorDelayPlanService';
import DelayPlanQueryForm from './DelayPlanQueryForm';
import DelayPlanGrid from './DelayPlanGrid';

const SuperiorDelayPlanMain = () => {

    const formData = React.useRef({
        MASTER_DEPT: "", // 主管機關
        EXEC_DEPT: "", // 執行機關
        PROJECT_YEAR: "", // 計畫年度
        PROJECT_NAME: "", // 計畫名稱
        RPT_TYPE: "", // 落後類別
        RPT_NAME: "", // 報表名稱
        IS_HAND_ROLE: false, // 是否只有主辦權限
    });

    // Grid資料
    const [formObj, setFormObj] = React.useState(formData.current);
    const [gridData, setGridData] = React.useState([]);
    // 下拉選單資料
    const [ddlData, setDdlData] = React.useState({
        MASTER_ORGAN: [], // 主管機關
        EXEC_ORGAN: [], // 執行機關
        PLAN_YEAR: [], // 計畫年度
    });
    // 匯出Grid資料
    const GridDataForExport = React.useRef({ props: {}, Columns: [] });
    // Formik innerRef
    const formRef = React.useRef(null);

    React.useEffect(() => {
        loadData();
    }, [])

    const loadData = async () => {
        await getAllDropDowns();
    }

    // 取得下拉清單
    const getAllDropDowns = async () => {
        SetMaskOnOff(true);
        const checkIsHANDRole = await CheckIsHANDRole();
        const data = await Service.getAllDropDowns(checkIsHANDRole);
        setDdlData({ ...ddlData, MASTER_ORGAN: data[0], EXEC_ORGAN: data[0], PLAN_YEAR: data[1] });
        formData.current.IS_HAND_ROLE = checkIsHANDRole;
        SetMaskOnOff(false);
    }

    // 查詢
    const onsubmit = async () => {
        if (formRef.current) {
            formRef.current.handleSubmit();
        }
    }

    // 取資料
    const getData = async (data) => {
        SetMaskOnOff(true);
        const rData = await Service.GetDelayPlan(data);
        setGridData([...rData]);
        setFormObj(data);
        SetMaskOnOff(false);
    }

    // 清除
    const onClear = () => {
        if (formRef.current) {
            setGridData([]);
            formRef.current.handleReset();
        }
    }

    // 匯出
    const exportRPT = async (extension) => {
        GridDataForExport.current.Columns = GridDataForExport.current.Columns.filter(x => x.title !== "項" && x.title !== "釘選");
        ExportGrid(GridDataForExport.current, extension, IsNullOrEmpty(formData.current.RPT_NAME) ? '落後案件查詢' : formData.current.RPT_NAME);
    }

    // 更新關心個案狀態
    const pisSelectProject = async (projectNo, pisSelectVal) => {
        SetMaskOnOff(true);
        const result = await updateProjectPisSelect(projectNo, pisSelectVal);
        SetMaskOnOff(false);
        if (result.success) {
            showGlobalMessageBox(result.message, () => getData(formObj));
        }
    }

    return (
        <PageContainer
            toolbar={
                <>
                    <h3 className="k-dialog-titlebar">決策分析 – 落後案件查詢</h3>
                    <Button title="查詢" className='k-button-lighten' onClick={() => onsubmit()}>查詢</Button>
                    <Button title="清除" className='k-button-lighten' onClick={() => onClear()}>清除</Button>
                    <Button title="匯出Excel" className='k-button-lighten' onClick={() => exportRPT('xlsx')} disabled={gridData.length <= 0}>匯出Excel</Button>
                    <Button title="匯出Ods" className='k-button-lighten' onClick={() => exportRPT('ods')} disabled={gridData.length <= 0}>匯出Ods</Button>
                </>
            }
        >
            <DelayPlanQueryForm
                ddlData={ddlData}
                formData={formData}
                formRef={formRef}
                getData={getData}
            />

            <DelayPlanGrid
                gridData={gridData}
                setGridData={setGridData}
                GridDataForExport={GridDataForExport}
                pisSelectProject={pisSelectProject}
            />

        </PageContainer>
    );
}

export default SuperiorDelayPlanMain;