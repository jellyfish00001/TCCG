import React, { useState, useEffect, useContext } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import 'bootstrap/dist/css/bootstrap.min.css';
import AddMdf from './EmpAgent-AddMdf/EmpAgentAddMdf';
import EmpAgentService from './empAgent.service';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { FormatDate } from '../../../Basic/SDOExtension';

const EmpAgent = () => {
    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);

    const [empAgents, setEmpAgents] = useState([]);
    const [count, setCount] = useState(0);
    const [pageable, setPageable] = useState({
        buttonCount: 10,
        info: true,
        type: 'numeric',
        pageSizes: false,
        previousNext: true
    });
    const [gridWidth, setGridWidth] = useState(0);
    const [addMdf, setAddMdf] = useState({
        visible: false,
        sid: '',
    });

    const grid = React.createRef();
    const service = EmpAgentService();

    useEffect(() => {
        setGridWidth(grid.current.element.offsetWidth);
        search();
    }, []);

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadData = async () => {
        let data = await service.getEmpAgent();
        setEmpAgents(data);
        setCount(data.length);
    }

    const search = async () => {
        await loadData();
    }

    const openCreate = sid => {
        setAddMdf({
            visible: true,
            sid: sid
        });
    }

    const closeCreate = async () => {
        setAddMdf({
            visible: false,
            sid: ''
        })
        await loadData();
    }

    const showDelete = sid => {
        showConfirmBox('進行刪除作業，確定嗎?', () => { confirmDelete(sid) });
    }

    const confirmDelete = async sid => {
        let result = await service.confirmDelete(sid);
        showMessage(result.message, {
            onOkAction: search
        });
    }

    const EditCell = (props) => {
        return (
            <td>
                <Button icon='close' look='bare' onClick={() => showDelete(props.dataItem.SID)}></Button>
            </td>
        )
    }

    const AgentDateCell = (props) => {
        let agentForm = new Date(props.dataItem.AGENT_FROM);
        let agentTo = new Date(props.dataItem.AGENT_TO);
        return (
            <td>{FormatDate(agentForm)} ~ {FormatDate(agentTo)}</td>
        )
    }

    return (
        <div className="fnForm">
            <div className="fn-buttons">
                <Button onClick={search}>查詢</Button>
                <Button onClick={() => openCreate('')}>新增</Button>
            </div>
            <div>
                <Grid
                    ref={grid}
                    style={{ height: '700px' }}
                    data={empAgents}
                    total={count}
                    pageSize={10}
                    pageable={pageable}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(2)} field="NO" title="No" />
                    <GridColumn width={setWidth(5)} title="" cell={EditCell} />
                    <GridColumn field="AGENT_NAME" title="代理帳號" />
                    <GridColumn field="AGENT_DATE" title="代理期間" cell={AgentDateCell} />
                </Grid>
            </div>
            {addMdf.visible && < AddMdf
                sid={addMdf.sid}
                onClose={closeCreate} />
            }
        </div>
    )
}

export default EmpAgent;