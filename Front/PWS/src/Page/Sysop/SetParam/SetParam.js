import React, { useState, useEffect } from 'react';
import { Button, } from '@progress/kendo-react-buttons'
import { Grid, GridColumn, GridNoRecords, GridDetailRow } from '@progress/kendo-react-grid';
import AddMdfItem from './SetParam-AddMdfItem/SetParamAddMdfItem'
import AddMdf from './SetParam-AddMdf/SetParamAddMdf'
import { PageContainer } from '../../../Basic/PageContainer';
import { Tooltip } from '@progress/kendo-react-tooltip';
import setParamService from './setparam.service';
import { Pageable } from '../../../Basic/BasicData';


const Query = () => {
    const [paramItemData, setParamItemData] = useState([]);
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [isItemAddMdf, setIsItemAddMdf] = useState({ visible: false, setItem: "", title: "" })
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, setItem: "", setType: "", title: "" })

    // 取得api資料
    const getParamItemData = async () => {
        setParamItemData(await setParamService.getParamItemData());
    }

    // 取得對應的關卡資料
    const getParamData = async (event) => {
        event.dataItem.expanded = event.value;
        let setItem = event.dataItem.SET_ITEM;
        // 取得資料
        let paramData = await setParamService.getParamData(setItem);

        let data = paramItemData.slice();
        // 找出對應的主檔資料
        let index = data.findIndex(d => d.SET_ITEM === setItem);
        // 加入關卡資料至grid
        data[index].details = paramData;
        setParamItemData(data);
    }


    const DetailComponent = (props) => {
        const data = props.dataItem.details;
        if (data) {
            return (
                <Grid data={data}>
                    <GridColumn width="30px" field="NO" title="No" />
                    <GridColumn width={70} cell={(props) =>
                        <td>
                            <Button icon="edit" look="bare" title={"修改"} onClick={() => setIsAddMdf({ visible: true, setItem: props.dataItem.SET_ITEM, setType: props.dataItem.SET_TYPE, title: "修改" })} />
                        </td>} />
                    <GridColumn field="SET_TYPE" title="參數代碼" width="120px" />
                    <GridColumn field="SET_VALUE" title="參數" />
                    <GridColumn field="MEMO" title="說明" />
                </Grid>
            );
        }
        return (
            <div style={{ height: "50px", width: '100%' }}>
                <div style={{ position: 'absolute', width: '100%' }}>
                    <div className="k-loading-image" />
                </div>
            </div >
        );
    }

    useEffect(() => {
        //載入時取得參數主檔清單
        getParamItemData();
    }, []);

    const toolbar = [
        <Button type="button" onClick={getParamItemData} >查詢</Button>,
        <Button type="button" onClick={() => setIsItemAddMdf({ visible: true, setItem: "", title: "新增" })}>新增參數類別</Button>,
        <Button type="button" onClick={() => setIsAddMdf({ visible: true, setItem: "", setType: "", title: "新增" })}>新增參數</Button>
    ]

    return (
        <PageContainer
            toolbar={toolbar}
        >
            <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    data={paramItemData.slice(paging.skip, paging.take + paging.skip)}
                    detail={DetailComponent}
                    total={paramItemData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                    expandField="expanded"
                    onExpandChange={getParamData}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width="30px" field="NO" title="No" />
                    <GridColumn width={110} cell={(props) =>
                        <td>
                            <Button icon="edit" look="bare" title={"修改"} onClick={() => setIsItemAddMdf({ visible: true, setItem: props.dataItem.SET_ITEM, title: "修改" })} />
                            <Button icon="track-changes-enable" look="bare" title={"新增參數"} onClick={() => setIsAddMdf({ visible: true, setItem: props.dataItem.SET_ITEM, setType: "", title: "新增" })} />
                        </td>} />
                    <GridColumn field="SET_ITEM" title="參數類別代碼" />
                    <GridColumn field="SET_ITEM_NAME" title="參數類別" />
                    <GridColumn field="MEMO" title="說明" />
                </Grid>
            </Tooltip>

            {/* 新增or修改視窗 */}
            {isItemAddMdf.visible && <AddMdfItem
                title={isItemAddMdf.title}
                closeWindow={() => setIsItemAddMdf({ visible: false, setItem: "", title: "" })}
                setItem={isItemAddMdf.setItem}
                refreshGrid={getParamItemData}
            />}

            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({ visible: false, setItem: "", setType: "", title: "" })}
                setItem={isAddMdf.setItem}
                setType={isAddMdf.setType}
                refreshGrid={getParamItemData}
            />}
        </PageContainer>
    );

}

export default Query;
