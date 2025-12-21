import React, { useState } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { saveAs } from "@progress/kendo-drawing/pdf";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";

const ExportFile = () => {
    // 假資料
    let initialData = [
        { FileName: "報告書範例.PDF", FilePath: "path/to/報告書範例.PDF" },
        { FileName: "相關參考.PDF", FilePath: "path/to/相關參考.PDF" }
    ];

    initialData = AddNoColumn(initialData);
    const [gridData, setGridData] = useState(initialData);

    // 跳轉錯誤內容
    const getNote = () => {

    };

    /**
     * 編輯儲存格
     * @param {*} props 
     * @returns 
     */
    const NumberCell = (props) => (
        <td style={{ fontWeight: 'bold', textAlign: 'center' }}>
            {props.dataItem.NO}
        </td>
    );

    //報告名稱內容
    const FileLinkCell  = (props) => (
        <CommandCell >
            <Button
                title={"下載"}
                onClick={() => getNote(props.dataItem.FileName)}
            >
                {props.dataItem.FileName}
            </Button>
        </CommandCell>
    );

    return (
        <PageContainer style={{ overflow: "auto", height: "100%" }}>
            <CollapseBoardCard
                title="報表列印"
                isFirstArea={true}
            > 
            <Grid
                data={gridData}
                style={{ height: '100%', overflow: 'auto' }}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="NO" title="序號" cell={NumberCell} width="100px" />
                <GridColumn field="FileName" title="報表名稱" cell={FileLinkCell}/>
            </Grid>
            </CollapseBoardCard>
        </PageContainer>
    );
};

export default ExportFile;
