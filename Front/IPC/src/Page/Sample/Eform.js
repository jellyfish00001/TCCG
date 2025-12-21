import React, { useState } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Slide } from '@progress/kendo-react-animation';
import { TreeView } from '@progress/kendo-react-treeview';
import { Grid, GridColumn } from '@progress/kendo-react-grid';

const Eform = () => {
    //Treeview 資料
    const item = [
        {
            text: "EFORM_SET - 表單主檔",
            hasSub: true,
            items: [
                { text: "form_id - 表單代碼" },
                { text: "form_type - 表單類型(ref:SET_PARAM|eFormType)" },
                { text: "form_name - 表單名稱" },
                { text: "effective_date - 啟用日期" },
                { text: "expire_date - 停用日期" },
                { text: "memo - 說明註記" },
            ]
        },
        {
            text: "MAP_EFORM_ORG - 表單組織對應",
            hasSub: true,
            items: [
                { text: "form_id - 表單代碼" },
                { text: "org_id - 組織單位代碼" }
            ]
        },
        {
            text: "MAP_EFORM_FLOW - 表單流程對應",
            hasSub: true,
            items: [
                { text: "form_id - 表單代碼" },
                { text: "flow_id - 流程代碼" },
                { text: "alert_day - 流程逾時天數限制" }
            ]
        },
        {
            text: "EFIELD_SET - 表單欄位主檔",
            hasSub: true,
            items: [
                { text: "fn_GetFlowMapRole" },
                { text: "field_id - 欄位代碼" },
                { text: "field_name - 欄位顯示名稱" },
                { text: "field_size - 欄位長度" },
                { text: "field_type - 欄位類型(ref:SET_PARAM|eFieldType)" },
                { text: "field_exttype - 欄位子類型(ref:SET_PARAM|eFieldExtType)" },
                { text: "field_option - 欄位顯示參數，以JSON格式儲存" },
                { text: "field_isfill - 必填" },
                { text: "memo - 說明註記" },
                { text: "del_flg - 刪除標記" },
            ]
        },
        {
            text: "MAP_EFORM_EFIELD - 表單欄位對應",
            hasSub: true,
            items: [
                { text: "form_id - 表單代碼" },
                { text: "field_id - 欄位代碼" },
                { text: "sort_id - 欄位順序" },
                { text: "issplitter - 是否為併排欄位(目前支援一行兩欄表現)" },
            ]
        },
        {
            text: "EFORM_DATAFILL - 表單填寫主檔",
            hasSub: true,
            items: [
                { text: "fill_id - 表單填寫代碼" },
                { text: "form_id - 表單代碼" },
                { text: "fill_data - 表單填寫內容，以JSON格式儲存" },
                { text: "flow_code - 流程代碼" },
                { text: "del_flg - 刪除標記" },
            ]
        },
        {
            text: "EFORM_FLOWLOG - 表單流程啟動日誌",
            hasSub: true,
            items: [
                { text: "flow_code - 流程代碼" },
                { text: "fill_id - 表單填寫代碼" },
            ]
        },
    ]

    //TreeView Css自訂
    const customItemRender = props => {
        if (props.itemHierarchicalIndex.length <= 1)
            return <h5>{props.item.text}</h5>
        return props.item.text
    }

    //開關組織樹
    const expandChangeHandler = (event) => {
        event.item.expanded = !event.item.expanded;
    }
    //hook控制treeView開關
    const [treeVisible, setTreeVisible] = useState(true);

    //hook控制grid分頁
    const [gridPage, setGridPage] = useState({
        take: 10,
        page: 0,
        pageable: {
            info: true,
            type: 'numeric',
        }
    });

    //gridData
    const gridData = [
        { type: "0 - 純文字顯示", extype: "", option: "", size: "N" },
        { type: "1 - 下拉式選單", extype: "", option: '[{ "Text": "選項", "Value": "數值" }, ...]', size: "N" },
        { type: "1 - 下拉式選單", extype: "11 - 多選格式", option: '[{ "Text": "選項", "Value": "數值" }, ...]', size: "N" },
        { type: "1 - 下拉式選單", extype: "12 - 組織選單", option: '{ "PARENT_NODEID": 指定根組織, "MULTISELECT": true || false }', size: "N" },
        { type: "1 - 下拉式選單", extype: "13 - 人員選單", option: "", size: "N" },
        { type: "1 - 下拉式選單", extype: "14 - Checkbox格式選單", option: '[{ "Text": "選項", "Value": "數值 || _GROUP", "Disabled": "true || false"  },...]', size: "N" },
        { type: "1 - 下拉式選單", extype: "15 - Radio格式選單", option: '[{ "Text": "選項", "Value": "數值 || _GROUP", "Disabled": "true || false"  },...]', size: "N" },
        { type: "2 - 單行文字框", extype: "", option: "", size: "Y" },
        { type: "2 - 單行文字框", extype: "21 - 數值", option: '{ "format": n || c || p, "decimals": 小數點進位位置, "min": 最小值, "max": 最大值 }', size: "N" },
        { type: "2 - 單行文字框", extype: "22 - 日期", option: '{ "DISPLAYTIME": true || false, "CASCADEFROM": 連動控制項ID }', size: "N" },
        { type: "2 - 單行文字框", extype: "23 - 格式化文字", option: '{ "mask": 格式化範本 }', size: "Y" },
        { type: "2 - 單行文字框", extype: "24 - 拉霸式數值格式", option: '{ "min": 最小值, "max": 最大值 }', size: "N" },
        { type: "3 - 多行文字框", extype: "", option: "", size: "Y" },
        { type: "3 - 多行文字框", extype: "31 - 簡易編輯文字框", option: "", size: "Y" },
        { type: "3 - 多行文字框", extype: "32 - 完整編輯文字框", option: "", size: "Y" },
        { type: "4 - 檔案上傳", extype: "", option: '{"MULTIPLE": true || false }', size: "N" },
    ];

    //grid換頁控制
    const gridPageChange = (event) => {
        let state = {
            take: event.page.take,
            page: (event.page.skip / event.page.take),
            pageable: gridPage.pageable
        }
        setGridPage(state);
    }

    let skip = gridPage.take * gridPage.page;
    return (
        <div>
            <div className="fn-buttons">
                <Button icon="menu" onClick={() => { setTreeVisible(!treeVisible) }}></Button>
            </div>
            {/* 滑出式查詢條 */}
            <Slide
                style={{ width: '100%' }}
                //動畫長短設定
                transitionExitDuration={500}
                transitionEnterDuration={500}
            >
                {
                    treeVisible &&
                    <div style={{
                        overflow: 'auto',
                    }}>
                        <h4>SQL Table</h4>
                        <TreeView
                            data={item}
                            //顯示文字
                            textField="text"
                            //顯示subTree
                            hasChildrenField="hasSub"
                            //展開狀態
                            expandField="expanded"
                            //展開icon顯示
                            expandIcons={true}
                            //顯示狀態
                            onExpandChange={expandChangeHandler} //展開
                            itemRender={customItemRender}
                        >
                        </TreeView>
                    </div>
                }
            </Slide>
            <fieldset>
                <legend>表單欄位定義</legend>
                <Grid
                    data={gridData.slice(skip, skip+10)}
                    skip={skip}
                    total={gridData.length}
                    pageSize={gridPage.take}
                    pageable={gridPage.pageable}
                    onPageChange={gridPageChange}
                >
                    <GridColumn title="欄位類型" field="type" width="200px"></GridColumn>
                    <GridColumn title="欄位子類型" field="extype" width="200px"></GridColumn>
                    <GridColumn title="欄位顯示參數" field="option"></GridColumn>
                    <GridColumn title="長度限制" field="size" width="100px"></GridColumn>
                </Grid>
            </fieldset>
        </div>
    );
}

export default Eform;