import React from 'react';
import EmpUserService from './empUser.service';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { FormatDate, AddNoColumn } from "../../../Basic/SDOExtension";

const EmpUserLog = (props) => {
    let userId = props.userId;
    let userName = props.userName;
    let visible = (props.visible && props.kind === "log") ? true : false;
    const [logData, setLogData] = React.useState([]);

    const loadData = async () => {
        let data = await EmpUserService.getLoginLog(userId);
        AddNoColumn(data);
        setLogData(data);
    }

    React.useEffect(() => {
        if (visible) {
            loadData();
        }
    }, [visible]);

    return (
        <>
            {visible && <WindowBox
                width={100}
                height={60}
                title='歷程'
                onClose={props.closeWindow}
            >

                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    data={logData}
                >
                    <GridColumn field="NO" title="項" width={30} />
                    <GridColumn title="帳號" cell={() =>
                        <td>
                            {userId}
                        </td>} />
                    <GridColumn title="姓名" cell={() =>
                        <td>
                            {userName}
                        </td>} />
                    <GridColumn field="USER_IP" title="IP" />
                    <GridColumn field="LOG_TIME" title="登入時間" cell={(props) =>
                        <td>
                            {FormatDate(props.dataItem.LOG_TIME, "tYY/MM/DD HH:mm:ss")}
                        </td>} />
                    <GridColumn field="MSG_CONTENT" title="動作" />
                    <GridColumn field="MSG_DETAIL" title="說明" />
                </Grid>
            </WindowBox>}
        </>
    );
}

export default EmpUserLog;