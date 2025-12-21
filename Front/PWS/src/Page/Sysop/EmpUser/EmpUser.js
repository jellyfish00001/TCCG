import React, { useState, useEffect, useRef } from 'react';
import EmpUserService from './empUser.service'
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn } from '@progress/kendo-react-grid';
import TextInput from '../../../Components/Input/TextInput';
import { PageContainer } from '../../../Basic/PageContainer';
import { Pageable } from '../../../Basic/BasicData';
import { DropDownListWithValue } from '../../../Components/Dropdowns/DropDownListWithValue';
import Table from '../../../Css/custom/Table.module.css';
import { FormatDate } from "../../../Basic/SDOExtension";
import { Checkbox } from '@progress/kendo-react-inputs';
import { GetHistory } from '../../../Basic/BasicData';
import { ExportGrid } from '../../../Basic/Download';
import EmpUserLog from './EmpUserLog';

const EmpUser = () => {
    const [slideVisible, setSlideVisible] = useState(true)
    const [paging, setPaging] = useState({ skip: 0, take: 10 })
    const [userData, setUserData] = useState([])
    const [delflgData, setDelflgData] = useState([])
    //視窗狀態
    const [windowStatus, setWindowStatus] = useState({visible: false, kind: "", userId: "", userName: ""});
    // 機關ID
    const [orgId, setOrgId] = useState();
    // 資料匯入excel
    const GridDataForExport = useRef({ props: {}, Columns: [] });
    // 查詢條件
    const [filterField, setFilterField] = useState({
        USER_ID: "", // 帳號
        USER_NAME: "", // 姓名
        ORG_ID: "", // 組織
        USER_EMAIL: "", // 	E-mail
        IS_LAST_SUCCLOGIN: "0", // 是否
        DAY_COUNT: 90, // 天數
        DEL_FLG: 2
    })

    useEffect(() => {
        //取得組織清單
        getOrgData()
        //取得帳號是否停用下拉選單
        getDelFlg()
    }, []);

    //取得使用者清單
    const getEmpUser = async () => {
        setUserData(await EmpUserService.getEmpUser(filterField))
    }

    // 帳號有效性下拉
    const getDelFlg = async () => {
        setDelflgData([{ DELFLG_DISPLAY: "全部", DELFLG: 2 }, { DELFLG_DISPLAY: "有效", DELFLG: 0 }, { DELFLG_DISPLAY: "無效", DELFLG: 1 }]);
    }

    const clearFliter = async () => {
        // 取得組織資料
        let orgData = await EmpUserService.getOrgDDLData();
        setFilterField({
            //查詢條件
            USER_ID: "", // 帳號
            USER_NAME: "", // 姓名
            ORG_ID: orgData[0].value, // 組織
            USER_EMAIL: "", // 	E-mail
            IS_LAST_SUCCLOGIN: "0", // 是否
            DAY_COUNT: 90, // 天數
            DEL_FLG: 2
        });
    }

    // 組織下拉選單
    const getOrgData = async () => {
        let orgData = await EmpUserService.getOrgDDLData();
        setOrgId(orgData);
        setFilterField({ ...filterField, ORG_ID: orgData[0].value });
        // 初始化查詢
        setUserData(await EmpUserService.getEmpUser({ ...filterField, ORG_ID: orgData[0].value }))
    }

    // 查詢畫面
    const queryConditions = (
        <form>
            <table className={Table.fullWidth}>
                <tbody>
                    <tr>
                        <th>組織</th>
                        <td colSpan={3}>
                            {/* 樹狀下拉  */}
                            {/* <DropDownTreeWithValue
                                data={orgData}
                                dataItemKey="ORG_ID"
                                textField="ORG_DISPLAY"
                                subItemsField="items"
                                setValue={(data) => {
                                    filterField.ORG_ID=data.value == null ? "" : data.value.ORG_ID;
                                }}
                            ></DropDownTreeWithValue> */}
                            <DropDownListWithValue
                                name="ORG_ID"
                                data={orgId}
                                value={filterField.ORG_ID}
                                textField={"text"}
                                dataItemKey={"value"}
                                onChange={(e) => {
                                    // 紀錄機關名稱
                                    setFilterField({ ...filterField, ORG_ID: e.target.value });
                                }}
                            />
                        </td>
                    </tr>
                    <tr>
                        <th>使用者帳號</th>
                        <td>
                            <TextInput
                                onChange={(e) => setFilterField({ ...filterField, USER_ID: e.target.value })}
                                value={filterField.USER_ID}
                                name="USER_ID"
                                style={{ width: "100%" }}
                            />
                        </td>
                        <th>姓名</th>
                        <td width="40%">
                            <TextInput
                                onChange={(e) => setFilterField({ ...filterField, USER_NAME: e.target.value })}
                                value={filterField.USER_NAME}
                                name="USER_NAME"
                                style={{ width: "100%" }}
                            />
                        </td>
                    </tr>
                    <tr>
                        <th>帳號是否有效</th>
                        <td>
                            <DropDownListWithValue
                                data={delflgData}
                                dataItemKey="DELFLG"
                                textField="DELFLG_DISPLAY"
                                value={filterField.DEL_FLG}
                                onChange={(data) => {
                                    setFilterField({ ...filterField, DEL_FLG: data.target.value });
                                }}
                            ></DropDownListWithValue>
                        </td>
                        <th>未登入天數</th>
                        <td width="40%">
                            <Checkbox
                                label={'超過'}
                                value={"1"}
                                checked={filterField.IS_LAST_SUCCLOGIN === "1"}
                                onChange={(e) => setFilterField({ ...filterField, IS_LAST_SUCCLOGIN: e.value ? "1" : "0" })}
                            />
                            <span>90天未登入</span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </form>
    );

    return (
        <>
            <h3 className="k-dialog-titlebar">系統管理&gt;使用者管理</h3>
            <PageContainer
                toolbar={
                    <>
                        <Button onClick={() => setSlideVisible(!slideVisible)}>{slideVisible ? "隱藏查詢條件" : "顯示查詢條件"}</Button>
                        <Button onClick={getEmpUser}>查詢</Button>
                        <Button onClick={clearFliter}>清除</Button>
                        <Button onClick={(e) => { GetHistory().push('/Home/Sysop/EmpUser/Add', { userId: "" }); }}>新增帳號</Button>
                        <Button title='匯出清單' disabled={userData.length === 0} onClick={() => ExportGrid(GridDataForExport.current, "xlsx")}>匯出Excel</Button>
                    </>
                }
            >
                {slideVisible && queryConditions}

                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto'
                    }}
                    data={userData.slice(paging.skip, paging.take + paging.skip)}
                    total={userData.length}
                    skip={paging.skip}
                    take={paging.take}
                    pageable={Pageable}
                    onPageChange={(e) => setPaging({ skip: e.page.skip, take: e.page.take })}
                    ref={(e) => {
                        if (e != null) {
                            GridDataForExport.current.Columns = e.columns;
                            GridDataForExport.current.props = e.props;
                        }
                    }}
                >
                    <GridColumn field="NO" headerClassName={'breakSpacehHeader'} title="項" width={30} />
                    <GridColumn width={40} headerClassName={'breakSpacehHeader'} title="編輯" cell={(props) =>
                        <td>
                            {
                                !props.dataItem.DEL_FLG &&
                                <Button icon="edit" look="default" title={"修改"} onClick={(e) => {
                                    GetHistory().push('/Home/Sysop/EmpUser/Add', { userId: props.dataItem.USER_ID });
                                }} />
                            }
                        </td>
                    } />
                    <GridColumn field="USER_ID" headerClassName={'breakSpacehHeader'} title="帳號" cell={(props) =>
                        <td>
                            <u style={{ textDecoration: 'underline', color: 'blue', cursor: "pointer" }} onClick={() => {
                                setWindowStatus({ visible: true, kind: "log", userId: props.dataItem.USER_ID, userName: props.dataItem.USER_NAME })
                            }}>{props.dataItem.USER_ID}</u>
                        </td>
                    } />
                    <GridColumn field="USER_NAME" headerClassName={'breakSpacehHeader'} title="姓名" />
                    <GridColumn headerClassName={'breakSpacehHeader'} title="是否有效" cell={(props) =>
                        <td>
                            {props.dataItem.DEL_FLG ? "無效" : "有效"}
                        </td>} />
                    <GridColumn field="ORG_NAME" headerClassName={'breakSpacehHeader'} title="機關" />
                    <GridColumn field="USER_TEL" headerClassName={'breakSpacehHeader'} title="電話" />
                    <GridColumn headerClassName={'breakSpacehHeader'} title="上次登入時間" cell={(props) =>
                        <td>
                            {parseInt(FormatDate(props.dataItem.LAST_SUCCLOGIN, "tYY")) < 0 ? "" : FormatDate(props.dataItem.LAST_SUCCLOGIN, "tYY/MM/DD HH:mm")}
                        </td>} />
                </Grid>

                {windowStatus.visible && <EmpUserLog
                    closeWindow={() => setWindowStatus({ visible: false, kind: "", userId: "", userName: "" })}
                    visible={windowStatus.visible}
                    kind={windowStatus.kind}
                    userId={windowStatus.userId}
                    userName={windowStatus.userName}
                />}
            </PageContainer>
        </>
    );
}

export default EmpUser;
