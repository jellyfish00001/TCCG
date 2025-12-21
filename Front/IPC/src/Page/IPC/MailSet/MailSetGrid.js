import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Window } from '@progress/kendo-react-dialogs';
import MailSetWindow from './MailSetWindow';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { WindowResizehook } from '../../../Hook/useWindowResize';

export const MailSetGrid = (props) => {
    const { data, loadData } = props;
    const [gridData, setGridData] = React.useState([]);
    //視窗狀態
    const [windowStatus, setWindowStatus] = React.useState({
        visible: false,
        mailId: ""
    });
    const dimensions = WindowResizehook();

    React.useEffect(() => {
        setGridData(AddNoColumn(data));
    }, [data])

    // 關閉視窗
    const closeWindow = () => {
        setWindowStatus({
            visible: false,
            mailId: ""
        })
    }

    // 修改欄位
    const EditCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                    setWindowStatus({
                        visible: true,
                        mailId: prop.dataItem.MAIL_ID
                    })
                }} />
            </CommandCell>
        )
    }

    // 停用欄位
    const DelCell = (prop) => {
        return (
            <td style={{ textAlign: "center" }}>
                {prop.dataItem.DEL_FLG ? "是" : "否"}
            </td>
        )
    }

    return (
        <PageContainer>
            <Grid
                data={gridData}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn field="NO" title="No" width="50px" cell={(prop) => {
                    return <td style={{ textAlign: "center" }}>{prop.dataItem.NO}</td>
                }} />
                <GridColumn title="編輯" cell={EditCell} width="50px" />
                <GridColumn title="停用" cell={DelCell} width="50px" />
                <GridColumn field="MAIL_NAME" title="範本名稱" width="270px" />
                <GridColumn field="MAIL_SUBJECT" title="範本主旨" />
            </Grid>

            {windowStatus.visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        onClose={() => closeWindow()}
                        title="修改"
                        width={dimensions.width * 0.7}
                        height={dimensions.height * 0.9}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <MailSetWindow
                            windowStatus={windowStatus}
                            loadData={loadData}
                        />
                    </Window>
                </div>
            }
        </PageContainer>
    );
}
export default MailSetGrid;