import React, { useState, useEffect, useCallback } from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Button } from '@progress/kendo-react-buttons';
import { AddMdf } from './CertificateBinding-AddMdf/CertificateBindingAddMdf';
import { PageContainer } from '../../../Basic/PageContainer'
import certificateBindingService from './certificateBinding.service';

const DelFlgCell = props => {
    let value = props.dataItem[props.field];
    return (
        <td>{value ? '停用' : '啟用'}</td>
    )
}

const LogDateCell = props => {
    let value = props.dataItem[props.field];
    let date = new Date(value);
    return (
        //yyyy/MM/dd HH:mm:ss
        <td>{date.getFullYear()}/{date.getMonth() + 1}/{date.getDate()} {date.getHours()}:{date.getMinutes()}:{date.getSeconds()}</td>
    )
}

const EditCell = showCreateAction => {
    return props => {
        return (
            <td>
                <Button icon='edit' look='bare' onClick={() => showCreateAction(true, props.dataItem.TOKEN_ID)}></Button>
            </td>
        )
    }
}

const Query = props => {
    const [gridWidth, setGridWidth] = useState(0);
    const [certificates, setCertificates] = useState([]);
    const [addMdfState, setAddMdfState] = useState({
        tokenId: '',
        visible: false,
    });

    //讀取Grid寬度
    const gridRef = useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);

    const loadData = async () => {
        setCertificates(await certificateBindingService.loadCertificate());
    }

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const showCreate = (visible, tokenId) => {
        setAddMdfState({
            tokenId: tokenId,
            visible: visible,
        })
    }

    //Mount時讀取資料
    useEffect(() => {
        loadData();
    }, [])

    //關閉彈出視窗時，重新讀取資料
    useEffect(() => {
        if (!addMdfState.visible)
            loadData();
    }, [addMdfState])


    const toolbar = [
        <Button key='searchButton' onClick={() => loadData()}>查詢</Button>,
        <Button key='createButton' onClick={() => showCreate(true, '')}>新增</Button>
    ]

    return (
        <PageContainer
            toolbar={toolbar}
        >
            <div>
                <Grid
                    style={{ height: '700px' }}
                    data={certificates}
                    ref={gridRef}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(3)} field="NO" title="No" />
                    <GridColumn width={setWidth(4)} cell={EditCell(showCreate)} />
                    <GridColumn field="USER_ID" title="使用者ID" />
                    <GridColumn field="TOKEN" title="憑證" />
                    <GridColumn width={setWidth(10)} field="DEL_FLG" title="啟用狀況" cell={DelFlgCell} />
                    <GridColumn field="MDF_DATE" title="最後異動時間" cell={LogDateCell} />
                </Grid>
            </div>
            {addMdfState.visible && <AddMdf
                tokenId={addMdfState.tokenId}
                onClose={() => showCreate(false, '')}
            />}
        </PageContainer>
    )
}

export default Query