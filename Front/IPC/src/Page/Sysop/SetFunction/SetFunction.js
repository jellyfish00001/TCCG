import React, { useState, useEffect, useContext } from 'react';
import { Button, } from '@progress/kendo-react-buttons'
import { Pageable } from '../../../Basic/BasicData';
import { Input } from '@progress/kendo-react-inputs';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import AddMdf from './SetFunction-AddMdf/SetFunctionAddMdf';
import { PageContainer } from '../../../Basic/PageContainer';
import { Tooltip } from '@progress/kendo-react-tooltip';
import SetFunctionService from './setFunction.service';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import { useTranslation } from 'react-i18next';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';

const Query = () => {
    const [functionDDLList, setFunctionDDLList] = useState([]);
    const [readByGroupData, setReadByGroupData] = useState([]);
    const [searchData, setSearchData] = useState({
        parentId: "",
        functionId: "",
        functionName: ""
    });
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [active, setActive] = useState({})
    /**@type {object} activeItem */
    const [activeItem, setActiveItem] = useState({})
    const [isAddMdf, setIsAddMdf] = useState({ visible: false, functionId: "", functionLevel: "", title: "" })
    const { showConfirmBox } = useContext(ConfirmBoxContext);
    const { showMessage } = useContext(MessageBoxContext);
    const { t } = useTranslation();
    // 取得api資料
    const getReadByGroup = async () => {
        setReadByGroupData(await SetFunctionService.getReadByGroup(searchData));
    }

    const getFunctionDDLList = async () => {
        setFunctionDDLList(await SetFunctionService.getFunctionDDLList());
    }

    const refreshData = async () => {
        getReadByGroup();
        getFunctionDDLList();
    }

    // 每次異動input後回寫至state
    const handleChange = (e) => {
        setSearchData(prevState => ({
            ...prevState,
            [e.target.props.name]: e.target.value
        }));
    }

    // grid拖拉欄位
    function dragCell(props) {
        return (<td onDragOver={(e) => {
            e.preventDefault();
            reorder(props.dataItem);
            e.dataTransfer.dropEffect = "copy";
        }}>
            <span
                className="k-icon k-i-move"
                draggable="true"
                style={{ cursor: 'move' }}
                onMouseDown={()=>{setActiveItem(props.dataItem);}}
                onDragStart={(e) => {
                    dragStart(props.dataItem);
                    e.dataTransfer.setData("dragging", "");
                }}
            />
        </td>);
    }

    // reorder、dragStart 為畫面拖拉欄位所需
    const reorder = (dataItem) => {
        console.log('reorder');
        console.log('activeItem：' + activeItem.FUNCTION_ID);
        console.log('dataItem' + dataItem.FUNCTION_ID);
        if (activeItem === dataItem) {
            return;
        }
        let reorderedData = readByGroupData.slice();
        let prevIndex = reorderedData.findIndex(p => (p === activeItem));
        let nextIndex = reorderedData.findIndex(p => (p === dataItem));
        reorderedData.splice(prevIndex, 1);
        reorderedData.splice(nextIndex, 0, activeItem);

        setReadByGroupData(reorderedData);
        setActive(activeItem);
    }

    const dragStart = (dataItem) => {
        setReadByGroupData(readByGroupData);
        setActiveItem(dataItem);
        console.log('dragStart');
    }

    // 更新排序
    const setSortorder = async () => {
        let response = await SetFunctionService.setSortorder(readByGroupData)
        showMessage(response.result, {
            onOkAction: () => {
                if (response.ok) {
                    refreshData()
                }
            }
        })
    }

    //刪除功能 
    const isDelte = (functionId) => {
        showConfirmBox(t('common.message.delete01'), () => { deleteFunction(functionId) })
    }

    const deleteFunction = async (functionId) => {
        let response = await SetFunctionService.deleteFunction(functionId)
        showMessage(response.result.message, {
            onOkAction: () => {
                if (response.ok) {
                    refreshData()
                }
            }
        })
    }

    // 開啟修改視窗
    const openUpadteWindow = (dataItem) => {
        let functionLevel = undefined;
        // 判斷是否為類別，不是傳遞所屬的功能類別資料
        if (dataItem.FUNCTION_URL === "") {
            functionLevel = "root";
        }

        //設定傳出值
        setIsAddMdf({ visible: true, functionId: dataItem.FUNCTION_ID, functionLevel: functionLevel, title: t('common.button.update') })
    }

    useEffect(() => {
        //載入時取得功能和下拉選單資料 
        refreshData();
    }, []);

    const toolbar = [
        <Button onClick={refreshData} >{t('common.button.query')}</Button>,
        <Button onClick={() => setIsAddMdf({ visible: true, functionId: "", functionLevel: "root", title: t('common.button.create') })}>{t('sysop.setFunction.setFunction01')}</Button>,
        <Button onClick={() => setIsAddMdf({ visible: true, functionId: "", functionLevel: "", title: t('common.button.create') })}>{t('common.button.create')}</Button>,
        <Button onClick={setSortorder} >{t('sysop.setFunction.setFunction02')}</Button>
    ]

    return (
        <PageContainer
            toolbar={toolbar}
        >
            <table style={{ width: '600px' }}   >
                <tbody>
                    <tr>
                        <th scope="col">{t('sysop.setFunction.setFunction03')}</th>
                        <td>
                            <DropDownListWithValue
                                name="parentId"
                                data={functionDDLList}
                                textField="text"
                                dataItemKey="value"
                                defaultItem={{ text: t('sysop.setFunction.setFunction03'), value: "" }}
                                value={searchData.parentId}
                                onChange={handleChange} />
                        </td>
                    </tr>
                    <tr>
                        <th scope="col">{t('sysop.setFunction.setFunction04')}</th>
                        <td>
                            <Input type="text" name="functionId" onChange={handleChange} />
                        </td>
                        <th scope="col">{t('sysop.setFunction.setFunction05')}</th>
                        <td>
                            <Input type="text" name="functionName" onChange={handleChange} />
                        </td>
                    </tr>
                </tbody>
            </table>

            <Tooltip openDelay={10} position="bottom" anchorElement="target">
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    data={readByGroupData.slice(paging.skip, paging.take + paging.skip)}
                    total={readByGroupData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width="30px" field="NO" title="No" />
                    <GridColumn title={t('sysop.setFunction.setFunction06')} width="50px" cell={dragCell} />
                    <GridColumn width={70} cell={(props) =>
                        <td>
                            <Button icon="edit" look="default" title={t('common.button.update')} onClick={() => { openUpadteWindow(props.dataItem); }} />
                            <Button icon="close" look="default" title={t('common.button.delete')} onClick={() => isDelte(props.dataItem.FUNCTION_ID)} />
                        </td>} />
                    <GridColumn field="FUNCTION_ID" title={t('sysop.setFunction.setFunction04')} />
                    <GridColumn field="FUNCTION_NAME" title={t('sysop.setFunction.setFunction05')} />
                    <GridColumn field="FUNCTION_URL" title={t('sysop.setFunction.setFunction07')}/>
                </Grid>
            </Tooltip>

            {/* 新增or修改視窗 */}
            {isAddMdf.visible && <AddMdf
                title={isAddMdf.title}
                closeWindow={() => setIsAddMdf({ visible: false, functionId: "", functionLevel: "", title: "" })}
                functionId={isAddMdf.functionId}
                functionLevel={isAddMdf.functionLevel}
                functionDefaultDDL={searchData.parentId === "" ? functionDDLList[0].value : searchData.parentId}
                refreshGrid={refreshData}
            />}
        </PageContainer>
    );

}
export default Query;

