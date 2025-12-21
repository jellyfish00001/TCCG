import React, { useState, useRef, useEffect } from "react";
import { Button } from '@progress/kendo-react-buttons';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { openProjectPrint } from '../../../Basic/CommonService';
import { GetPlanDataList } from './GpReviewProjectService';
import { CommandCell } from "../../../Components/GridCell/CommandCell";

export const GpReviewProjectWindow = (props) => {
    const {
        closeWindow,
        setIpcPlanNo,
        setIpcPlanName
    } = props;
    // grid的資料
    const [gridData, setGridData] = useState([]);

    /**
     * 初始化資料
     * @returns
     */
    const loadData = async () => {
        let result = await GetPlanDataList();
        setGridData(result);
    };

    // 初始化取得資料
    useEffect(() => {
            loadData();
    }, []);

    const handleButtonClick = (props, e) => {
        e.preventDefault();
        setIpcPlanNo(props.dataItem.PROJECT_NO);
        setIpcPlanName(props.dataItem.PROJECT_NAME);
        closeWindow();
    }

    // 預覽列印
    const preview = (prop) => {
        let data = {
            projectNo: prop.dataItem.PROJECT_NO,
            projectName: prop.dataItem.PROJECT_NAME
        }
        openProjectPrint(data);
    }

    // 關聯按鈕
    const CustomButtonCell = (props) => {
        return (
            <CommandCell>
                <Button title={"關聯"} icon='add' look='default' onClick={(e) => handleButtonClick (props, e)} />
            </CommandCell>
        );
    };

    const projectNameCell = props => {
        return (
            <td style={{ minWidth: '200px' }}>
                <a 
                onClick={() => preview(props)}
                >
                {props.dataItem.PROJECT_NAME}
                </a>
            </td>
        )
    }
    
        
    return (
        <WindowBox
            width={85}
            height={50}
            onClose={props.closeWindow}
            title={"關聯重大建設"}
        >

            <Grid
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="關聯" title="關聯" width={100} cell={CustomButtonCell} />
                <GridColumn field="PROJECT_NO" title="計畫編號" width={150} />
                <GridColumn field="PROJECT_NAME" title="計畫名稱" width={200} cell={projectNameCell}/>
                <GridColumn field="MASTER_ORGAN_NAME" title="主管機關" width={150} />
                <GridColumn field="EXEC_ORGAN_NAME" title="執行機關" width={150} />
                <GridColumn field="ALL_JOB" title="計畫內容" width={300} />
            </Grid>
        </WindowBox>
    );
};
  
export default GpReviewProjectWindow;