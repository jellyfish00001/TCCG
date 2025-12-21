import React, { useState } from "react";
import { PageContainer } from "../../../Basic/PageContainer";
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { saveAs } from "@progress/kendo-drawing/pdf";
import { AddNoColumn } from "../../../Basic/SDOExtension";
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import CollapseBoardCard from '../../../Components/BoardCard/CollapseBoardCard';
import { TextInputCell } from "../../../Components/GridCell/TextInputCell";

const CProjectSendMain = () => {
    // 假資料
    let initialData = [
        { FileName: "報告書範例.PDF", FilePath: "path/to/報告書範例.PDF" },
        { FileName: "相關參考.PDF", FilePath: "path/to/相關參考.PDF" }
    ];

    initialData = AddNoColumn(initialData);
    const [gridData, setGridData] = useState(initialData);
    //預覽列印
    const Preview = () => {
        alert("預覽列印");
    };

    //送出
    const Send = () => {
        alert("送出");
    };

    // 跳轉錯誤章節
    const getChapter = () => {

    };

    // 跳轉錯誤內容
    const getNote = () => {

    };

    const WrongChapter = props => (
        <CommandCell >
            <Button
                title={"錯誤章節"}
                onClick={() => getChapter()}
            >
                錯誤章節
            </Button>
        </CommandCell>
    );

    const WrongNote = props => (
        <CommandCell >
            <Button
                title={"錯誤內容"}
                onClick={() => getNote()}
            >
                錯誤內容
            </Button>
        </CommandCell>
    );

    return (
        <PageContainer style={{ overflow: "auto", height: "100%" }}>
            <CollapseBoardCard
                button={
                    <>
                        <Button title="預覽列印" className="k-button-lighten" onClick={Preview} >預覽列印</Button>
                        <Button title="確認送出" className="k-button-lighten" onClick={Send} >確認送出</Button>
                    </>
                }
                title="計畫送審"
                isFirstArea={true}
            > 
            <Grid
                data={gridData}
                style={{ height: '100%', overflow: 'auto' }}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="WrongChapter" title="錯誤章節" cell={WrongChapter}/>
                <GridColumn field="WrongNote" title="錯誤內容" cell={WrongNote}/>
            </Grid>
            </CollapseBoardCard>
        </PageContainer>
    );
};

export default CProjectSendMain;
