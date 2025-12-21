import React, { useState, useEffect } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Input } from '@progress/kendo-react-inputs';
import { Slide } from '@progress/kendo-react-animation';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { IsNullOrEmpty, FormatDate } from '../../../Basic/SDOExtension';
import ActiveFlowService from './activeFlow.service';
import FlowDetail from '../FlowDetail/FlowDetail';

const ActiveFlow = () => {
    /* use State */
    const [inputVisible, setInputVisible] = useState(true);
    const [data, setData] = useState([]);
    const [count, setCount] = useState(0);
    const [page, setPage] = useState({
        take: 20, skip: 0
    });
    const [detailFlowCode, setDetailFlowCode] = useState('');
    const [flowCode, setFlowCode] = useState('');
    const [flowDetailVisible, setFlowDetailVisible] = useState(false);
    const service = ActiveFlowService();

    /* use Effect */
    useEffect(() => {
        loadData();
    }, [])

    /* 滑動查詢區控制 */
    const slideChangeHandler = () => {
        setInputVisible(!inputVisible);
    }

    /* 查詢 */
    const onRequestClick = async () => {
        loadData();
    }

    /* load data */
    const loadData = async () => {
        try {
            let result = await service.readActiveFlow(flowCode);
            setData(result);
            setCount(result.length);
            setPage({ take: 20, skip: 0 });
        }
        catch (e) {
            console.log("ErrorMessage: " + e)
        }
    }

    /* 換頁控制 */
    const pageChange = event => {
        setPage({
            skip: event.page.skip,
            take: event.page.take
        })
    }

    /* 流程明細開關控制 */
    const toolCellStyle = (props) => {
        return (
            <td>
                <Button
                    icon="grid-layout"
                    onClick={() => {
                        setDetailFlowCode(props.dataItem.flow_code);
                        setFlowDetailVisible(true);
                    }}
                ></Button>
            </td>
        );
    }

    /* 流程關卡欄位合併 */
    const customCellStyle = (props) => {
        let di = props.dataItem;
        let org = "", role = "";
        if (!IsNullOrEmpty(di.org_name)) org = di.org_name + ' ';
        if (!IsNullOrEmpty(di.role_name)) role = '(' + di.role_name + ') ';
        return <td>{org + role + (di.user_name ?? "")}</td>
    }

    /* date format */
    const formatDate = props => {
        return <td>{FormatDate(props.dataItem.mdf_date, "tYY/MM/DD HH:mm:ss")}</td>
    }

    /* 查詢流程代碼輸入 */
    const onFlowCodeChange = event => {
        setFlowCode(event.value);
    }

    return (
        /* 設定背景css 白色 */
        < div className="fnForm" >
            <div>
                {/* 按鈕工具區塊 */}
                <div className="fn-buttons">
                    <Button icon="menu" onClick={slideChangeHandler} style={{ margin: "0 10px", color: "black", background: "none", border: "none", boxShadow: "none" }}></Button>
                    <Button onClick={onRequestClick}>查詢</Button>
                </div>
                {/* 滑出式查詢條 */}
                <Slide
                    //動畫長短設定
                    transitionExitDuration={500}
                    transitionEnterDuration={500}
                >
                    {inputVisible && (
                        <div>
                            {/*  top right bot  left */}
                            <label style={{ margin: '20px 10px 20px 50px' }}>流程代碼</label>
                            <Input type="text" name="flowCode" onChange={onFlowCodeChange} />
                        </div>
                    )}
                </Slide>
            </div>
            <Grid
                className="expand"
                data={data.slice(page.skip, page.skip + page.take)}
                total={count}
                skip={page.skip}
                scrollable='scrollable'
                pageSize={page.take}
                pageable={{
                    info: true,
                    type: 'numeric',
                    previousNext: true
                }}
                onPageChange={pageChange}
                resizable
            >
                <GridNoRecords> </GridNoRecords>
                <GridColumn title=" " field="NO" width={40} />
                <GridColumn cell={toolCellStyle} width={80} />
                <GridColumn title="流程代碼" field="flow_code" />
                <GridColumn title="目前流程關卡" cell={customCellStyle} />
                <GridColumn title="最新簽核時間" field="mdf_date" cell={formatDate} width={150} />
            </Grid>

            {flowDetailVisible && <FlowDetail
                flowCode={detailFlowCode}
                isAdmin={true}
                onClose={() => {
                    setDetailFlowCode('')
                    setFlowDetailVisible(false);
                    // loadData();
                }}
            >
            </FlowDetail>
            }
        </div>
    );
}

export default ActiveFlow;