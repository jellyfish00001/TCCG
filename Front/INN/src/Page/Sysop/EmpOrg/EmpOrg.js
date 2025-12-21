import React, { useState, useEffect, useContext } from 'react';
import { TreeView } from '@progress/kendo-react-treeview';
import { Button } from '@progress/kendo-react-buttons';
import { Input } from '@progress/kendo-react-inputs';
import { Slide } from '@progress/kendo-react-animation';
import { MessageBoxContext } from '../../../Components/Dialogs/MessageBox';
import { ConfirmBoxContext } from '../../../Components/Dialogs/ConfirmBox';
import EmpOrgAddMdf from './EmpOrg-AddMdf/EmpOrgAddMdf';
import EmpOrgService from './empOrg.service';


const EmpOrg = () => {
    const { showMessage } = useContext(MessageBoxContext);
    const { showConfirmBox } = useContext(ConfirmBoxContext);

    const [inputVisible, setInputVisible] = useState(false);
    const [addMdfState, setAddMdfState] = useState({
        visible: false,
        type: ''
    });
    const [treeData, setTreeData] = useState([]);
    const [checkedTreeItem, setCheckedTreeItem] = useState([]);
    const [selectedTreeItem, setSelectedTreeItem] = useState({ selected: false });
    const service = EmpOrgService();

    //init data
    useEffect(() => {
        updateTreeData();
    }, []);

    //刪除組織列表異動觸發刪除
    useEffect(() => {
        if (checkedTreeItem.length > 0)
            showConfirmBox("確認要刪除這些組織嗎？", onDeleteHandler);
    }, [checkedTreeItem]);

    //更新組織樹資料
    const updateTreeData = async () => {
        let data = await getEmpOrgData();
        setCheckedTreeItem([]);
        setSelectedTreeItem({ selected: false });
        setTreeData(data);
    }

    //post treeData
    const getEmpOrgData = async (parentOrgId = "") => {
        //初始化節點
        let root = [];
        try {
            root = await service.getEmpOrg(parentOrgId);

            //找尋sub tree
            //foreach不支援非同步, 使用for of
            for (let item of root) {
                let hasChild = item.HASCHILDREN;
                let orgId = item.ORG_ID;
                //有sub tree
                if (hasChild) {
                    item.items = await getEmpOrgData(orgId);
                    if (item.items.length > 0) {
                        //預設為展開, 刷新畫面
                        item.expanded = true;
                    } else {
                        //取得subTree為空, 設回false ((修改sql可解決此問題))
                        item.HASCHILDREN = false;
                    }
                }
            }
            return root;
        }
        catch (e) {
            console.log('ErrorMessage: ' + e);
        }
    }

    //expand tree handler
    const expandChangeHandler = (event) => {
        event.item.expanded = !event.item.expanded;
    }

    //check tree handler
    const checkChangeHandler = (event) => {
        event.item.checked = !event.item.checked;
        //同步子節點勾選狀態
        if (event.item.HASCHILDREN) {
            for (let item of event.item.items) {
                checkAllChild(item, event.item.checked);
            }
        }
        //檢查父節點勾選狀態
        if (!event.item.checked) {
            checkParentState(treeData);
        }
    }

    //同步子節點勾選狀態
    const checkAllChild = (items, checked) => {
        items.checked = checked;
        if (items.HASCHILDREN) {
            for (let item of items.items) {
                checkAllChild(item, items.checked);
            }
        }
    }

    //檢查父節點勾選狀態
    const checkParentState = (items) => {
        let state = true;
        for (let item of items) {
            if (item.HASCHILDREN)
                item.checked = checkParentState(item.items);
            state &= item.checked;
        }
        return state;
    }

    //select tree handler
    const itemClickHandler = (event) => {
        let itemState = { selected: false };
        //取消上次選取
        if (selectedTreeItem) {
            let item = selectedTreeItem;
            item.selected = false;
        }
        //當次選取設為相反狀態
        if (selectedTreeItem !== event.item) {
            event.item.selected = !event.item.selected;
            itemState = event.item;
        }
        setSelectedTreeItem(itemState);
    }

    //滑動查詢區控制
    const slideChangeHandler = () => {
        setInputVisible(!inputVisible);
    }

    //新增修改按鈕觸發
    const openAddMdfHandler = (type = '') => {
        setAddMdfState({
            visible: !addMdfState.visible,
            type: type
        });
    }

    //新增修改完成觸發
    const onAddMdfFinishHandler = () => {
        setAddMdfState({
            visible: false,
            type: ''
        });
        updateTreeData();
    }

    //觸發按鈕刪除
    const deleteDialogHandler = async () => {
        let delTargets = [];
        //統計需要刪除的節點
        await countDelTargets(delTargets, treeData);
        treeData.selected = false;
        setCheckedTreeItem(delTargets);
    }

    //統計需要刪除的節點
    const countDelTargets = async (delTargets, items) => {
        items.forEach(item => {
            // 狀態為打勾,加入刪除目標
            if (item.checked) {
                delTargets.push(item.ORG_ID);
            }
            // 下層組織遞迴判斷
            if (item.HASCHILDREN) {
                countDelTargets(delTargets, item.items);
            }
        });
    }

    //執行刪除
    const onDeleteHandler = async () => {
        //call api執行刪除
        let response = await service.deleteEmpOrgs(checkedTreeItem.join(","));
        let responseData = await response.json();

        //更新提示視窗訊息, 顯示
        showMessage(responseData.message, {
            onOkAction: () => {
                if (response.ok) {
                    updateTreeData();
                }
            }
        });
    }

    return (
        /* 設定背景css 白色 */
        <div className="fnForm">
            <div className="expand">
                {/* 按鈕工具區塊 */}
                <div className="fn-buttons">
                    <Button icon="menu" onClick={slideChangeHandler} style={{ margin: "0 10px", color: "black", background: "none", border: "none", boxShadow: "none" }}></Button>
                    <Button onClick={() => { openAddMdfHandler('POST') }} >新增</Button>
                    <Button onClick={() => { openAddMdfHandler('PUT') }} disabled={!selectedTreeItem.selected} >修改</Button>
                    <Button onClick={deleteDialogHandler}>刪除</Button>
                </div>
                {/* 滑出式查詢條 */}
                <Slide
                    //動畫長短設定
                    transitionExitDuration={500}
                    transitionEnterDuration={500}
                >
                    {inputVisible && (
                        <table style={{ margin: '20px 50px' }}>
                            <tbody>
                                <tr>
                                    <th scope="col"><label>組織名稱</label></th>
                                    <td><Input type="text" name="orgName" /></td>
                                </tr>
                            </tbody>
                        </table>
                    )}
                </Slide>
                {/* 組織樹 */}
                <TreeView
                    data={treeData}
                    //顯示文字
                    textField="ORG_DISPLAY"
                    //顯示subTree
                    hasChildrenField="HASCHILDREN"
                    //展開狀態
                    expandField="expanded"
                    //展開icon顯示
                    expandIcons={true}
                    //顯示勾勾盒
                    checkField="checked"
                    //顯示狀態
                    checkboxes={true}
                    onExpandChange={expandChangeHandler} //展開
                    onCheckChange={checkChangeHandler} //打勾
                    onItemClick={itemClickHandler} //選取
                >
                </TreeView>
            </div>
            {/* 新增組織功能div */}
            <div className="createPanel">
                {addMdfState.visible &&
                    <EmpOrgAddMdf
                        type={addMdfState.type}
                        value={selectedTreeItem}
                        onClose={openAddMdfHandler}
                        onFinish={onAddMdfFinishHandler}
                    />}
            </div>
        </div>
    );
}

export default EmpOrg;