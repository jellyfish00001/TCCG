//@ts-check
import React, { useState, useEffect, useContext, useRef } from 'react';
import AnnouncementService from './announcement.service';
import { Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { FormatDate } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { Tooltip } from '@progress/kendo-react-tooltip';
import { PageContainer } from '../../../Basic/PageContainer';
import AddMdf from './Announcement-AddMdf/AnnouncementAddMdf';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { showGlobalConfirmBox } from '../../../Route/RootMiddleware';
import { useHookstate } from '@hookstate/core';
import { ExportGrid } from '../../../Basic/Download';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';

const Query = () => {
    const AnnouncementState = useHookstate({ visible: false, sid: "", title: "" });
    const [ annTypeData, setAnnTypeData ] = useState([]);
    const [ announcementData, setAnnouncementData ] = useState([]);
    const [ paging, setPaging ] = useState({ skip: 0, take: 10 })
    const { showMessage } = useContext(MessageBoxContext);
    const GridData = useRef({ props: {}, Columns: [] })

    // 查詢條件
    // const filterField = useRef({
    //     ANN_TYPE: "", // 公告類別
    // }) 
    const [filterField, setFilterField] = useState({ 
        ANN_TYPE : "" // 公告類別
    })

    useEffect(() => {
        loadData();
    }, []);
    
    const loadData = async () => {
        // 取得公告類別下拉清單
        await getAnnTypeData();
        // 取得公告
        await getAnnouncement();
    }

    /**
     * 取得公告
     */
    const getAnnouncement = async () => {
        setAnnouncementData(await AnnouncementService.getAnnouncement(filterField))
    }

    /**
     * 取得公告類別下拉清單
     */
    const getAnnTypeData = async () => {
        setAnnTypeData(await AnnouncementService.getAnnTypeData())
    }

    /**
     * 是否要刪除公告
     * @param {*} sid 
     */
    const isDelte = (sid) => {
        showGlobalConfirmBox("進行刪除作業，確定嗎？",  () => { deleteAnnouncement(sid) });
    }

    /**
     * 刪除公告
     * @param {*} sid 
     */ 
    const deleteAnnouncement = async (sid) => {
        let response = await AnnouncementService.deleteAnnouncement(sid);
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    getAnnouncement()
                }
            }
        })
    }

    const toolbar = [
        <Button onClick={getAnnouncement}>查詢</Button>,
        <Button onClick={() => AnnouncementState.set({ visible: true, sid: '', title: "新增" })}>新增</Button>
    ]

    return (
        <PageContainer
            toolbar={toolbar}
        >
            {/* <table>
                <tbody>
                    <tr>
                        <th>公告類別</th>
                        <td>
                            <DropDownListWithValue
                                data={annTypeData}
                                value={filterField.current.ANN_TYPE}
                                textField="SET_VALUE"
                                dataItemKey="SET_TYPE"
                            />
                        </td>
                    </tr>
                </tbody>
            </table> */}
            <div style={{display:'inline-flex'}}>
                <span style={{ fontWeight: 'bold', padding:'3px'}}>公告類別:</span>
                <DropDownListWithValue
                    data={annTypeData}
                    value={filterField.ANN_TYPE}
                    textField="SET_VALUE"
                    dataItemKey="SET_TYPE"
                    onChange={(e) =>{
                        setFilterField({...filterField, ANN_TYPE:e.value.SET_TYPE})
                    }}
                />
            </div>
            <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    data={announcementData.slice(paging.skip, paging.take + paging.skip)}
                    total={announcementData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                    ref={(e)=>{
                        if(e !=null)
                        {
                            GridData.current.Columns=e.columns;
                            GridData.current.props=e.props ;
                        }                   
                    }}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn field="NO" headerClassName={'breakSpacehHeader'} title="No" width={30} />
                    <GridColumn width={70} cell={(props) =>
                        <td>
                            <Button icon="edit" look="default" title={"修改"} onClick={() => AnnouncementState.set({ visible: true, sid: props.dataItem.SID, title: "修改" })} />
                            <Button icon="close" look="default" title={"刪除"} onClick={() => isDelte(props.dataItem.SID)} />
                        </td>} />
                    <GridColumn headerClassName={'breakSpacehHeader'} title="公告日期" cell={(props) =>
                        <td>
                            {FormatDate(props.dataItem.EFFECTIVE_DATE)}~
                            {FormatDate(props.dataItem.EXPIRE_DATE)}
                        </td>} />
                    <GridColumn field="ANN_TYPE_NAME" headerClassName={'breakSpacehHeader'} title="公告類別"  />
                    <GridColumn field="TITLE" headerClassName={'breakSpacehHeader'} title="公告標題"  />
                    <GridColumn field="MDF_USER" headerClassName={'breakSpacehHeader'} title="編輯者"  />
                    <GridColumn headerClassName={'breakSpacehHeader'} title="最後編輯時間" cell={(props) =>
                        <td>
                            {FormatDate(props.dataItem.MDF_DATE, "tYY/MM/DD HH:mm")}
                        </td>} />
                </Grid>
            </Tooltip>

            {/* 新增or修改視窗 */}
            <AddMdf
                closeWindow={() => AnnouncementState.set({ visible: false, sid: '', title: '' })}
                refreshGrid={getAnnouncement}
                state={AnnouncementState}
            />
        </PageContainer>
    );
}
export default Query;
