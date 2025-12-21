/*
    組織樹 元件
*/
import React, { useState, useEffect } from 'react';
import { Error } from '@progress/kendo-react-labels';
import { Button } from '@progress/kendo-react-buttons';
import { Dialog, DialogActionsBar } from '@progress/kendo-react-dialogs';
import { TreeView } from '@progress/kendo-react-treeview';
import { MultiSelect } from '@progress/kendo-react-dropdowns';
import { ServerConfig } from '../../Basic/BasicData';
import { api } from '../../Basic/ApiFetch';
import { IsNullOrEmpty } from '../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../Route/RootMiddleware';


const Orgselector = (props) => {
    const [orgDropdowns, setOrgDropdowns] = useState([]) //多選下拉選單的value
    const [orgTree, setOrgTree] = useState([])//treeview的機關資料
    const [checkOrgItems, setCheckOrgItems] = useState([])//多選(選取的機關object)
    const [selectedOrgItem, setSelectedOrgItem] = useState(null)//單選
    const [visible, setVisible] = useState(false)

    const [ignore, forceUpdate] = React.useReducer(x => x + 1, 0);

    useEffect(() => {
        let data = { ...props }
        setOrgDropdowns(IsNullOrEmpty(data.value) ? [] : data.value)
    }, [props.value]);

    //開啟or關閉OrgTreeView
    const showOrgTreeView = async (show, isSubmit) => {
        setVisible(!visible)
        if (show) {
            getOrgByParentOrgId("")
            if (!IsNullOrEmpty(orgDropdowns)) {
                //選取的機關
                /**
                 * @type {object}
                 */
                let data = IsNullOrEmpty(checkOrgItems) ? [] : [{ ...checkOrgItems }];
                orgDropdowns.forEach(defaultOrg => (
                    data.push({ ORG_ID: defaultOrg.ORG_ID, ORG_DISPLAY: defaultOrg.ORG_DISPLAY })
                ))
                //將選取的機關回塞到checkOrgItems
                setCheckOrgItems(data)
            }
        }
        else {
            //關閉時清空
            setOrgTree([])
            setCheckOrgItems([])
            setSelectedOrgItem(null)
            let data = props.multiple ? checkOrgItems : (IsNullOrEmpty(selectedOrgItem) ? [] : [{ ORG_ID: selectedOrgItem.ORG_ID, ORG_DISPLAY: selectedOrgItem.ORG_DISPLAY }])
            //關閉視窗並且是確定按鈕，須將選取的資料回傳
            if (!show && isSubmit) {
                //將選取的機關object回塞到下拉選單
                setOrgDropdowns(data)
                //設定value  ps:傳入的data 為object  ex:[{ORG_ID: "xxx", ORG_DISPLAY: "xxx"}]
                props.setValue(data)
            }
        }
    }

    //取得機關清單byOrgId
    const getOrgByParentOrgId = async (orgId) => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'EmpOrg/' + orgId
        let response = await api.Get(url)

        let data = await response.json();
        //多選下拉選單有資料的話，treeview該筆對應的資料會被選取
        if (!IsNullOrEmpty(orgDropdowns)) {
            orgDropdowns.forEach((val) => {
                let defaultOrg = data.find(org => org.ORG_ID === val.ORG_ID);
                if (defaultOrg) {
                    if (props.multiple)
                        defaultOrg.checked = true;
                    else {
                        if (!selectedOrgItem) {
                            defaultOrg.selected = true;
                            setSelectedOrgItem(defaultOrg)
                        }
                    }
                }
            })
        }

        //剛打開treeview
        if (IsNullOrEmpty(orgId)) {
            setOrgTree(data)
        } else {
            //若有orgId，則是treeView展開
            return data
        }
    }

    //treeView機關展開
    const onExpandChange = async (e) => {
        e.item.expanded = !e.item.expanded;
        forceUpdate();
        if (!e.item.items) {
            getOrgByParentOrgId(e.item.ORG_ID).then(items => {
                if (items.length > 0) {
                    e.item.items = items;
                    forceUpdate();
                }
            });
        }
    }

    //treeView機關多選
    const onCheckChange = (e) => {
        e.item.checked = !e.item.checked;
        forceUpdate();
        if (e.item.checked) {
            //將選取的機關回塞到checkOrgItems
            setCheckOrgItems([...checkOrgItems, { ORG_ID: e.item.ORG_ID, ORG_DISPLAY: e.item.ORG_DISPLAY }])
        } else {
            //將取消勾選的data移除
            setCheckOrgItems(checkOrgItems.filter(item => item.ORG_ID !== e.item.ORG_ID))
        }
    };

    //treeView機關單選
    const onItemClick = (e) => {
        if (props.multiple)
            return;

        e.item.selected = !e.item.selected;

        if (selectedOrgItem) {
            selectedOrgItem.selected = false
        }

        setSelectedOrgItem(e.item.selected ? e.item : null)
    }

    const onChange = (e) => {
        setOrgDropdowns(e.target.value)
        props.setValue(e.target.value)
    }

    return (
        <>
            <table>
                <tbody>
                    <tr>
                        <td>
                            <MultiSelect
                                name="MAP_ORG"
                                data={orgDropdowns}
                                textField="ORG_DISPLAY"
                                dataItemKey="ORG_ID"
                                value={orgDropdowns}
                                onChange={(e) => onChange(e)}
                                popupSettings={{ className: 'invisibleElement' }}
                            />
                        </td>
                        <td>
                            <Button type="button" onClick={() => showOrgTreeView(true)}>...</Button>
                        </td>
                    </tr>
                    <tr>
                        <td>{<Error>{props.error}</Error>}</td>
                    </tr>
                </tbody>
            </table>
            {visible && <Dialog
                width={400}
                height={600}
            >
                <div style={{ height: "92%", overflow: "auto" }}>
                    <TreeView data={orgTree}
                        textField="ORG_DISPLAY"
                        hasChildrenField="HASCHILDREN"
                        expandIcons={true}
                        checkboxes={props.multiple}
                        onExpandChange={onExpandChange}
                        onItemClick={onItemClick}
                        onCheckChange={onCheckChange}
                    />
                </div>
                <DialogActionsBar>
                    <button className="k-button" onClick={() => showOrgTreeView(false, true)}>確定</button>
                    <button className="k-button" onClick={() => showOrgTreeView(false)}>取消</button>
                </DialogActionsBar>
            </Dialog>}
        </>
    )
}

export default Orgselector

