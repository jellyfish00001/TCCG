import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { MultiSelect } from "@progress/kendo-react-dropdowns";
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import SetContactService from './SetContactService';
import { getUserListByOuId } from '../../../Basic/CommonService';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { RequiredHeaderCell } from '../../../Components/GridCell/RequiredHeaderCell';
import * as Yup from 'yup';

export const SetContactWindow = (props) => {
    const { loadData, data } = props;
    // 聯絡窗口下拉選單資料
    const [contactDdlData, setContactDdlData] = React.useState([]);
    const contactDefaultDdlData = React.useRef([]);
    // 聯絡窗口已選擇資料
    const [contactSelData, setContactSelData] = React.useState([]);
    // 聯絡人清單資料
    const [gridData, setGridData] = React.useState([]);
    const [isDataChange, setIsDataChange] = React.useState(false);

    const loadContactData = async () => {
        SetMaskOnOff(true);
        await getUserList();
        await getContactData();
        SetMaskOnOff(false);
    }
    //取得聯絡窗口人員下拉資料
    const getUserList = async () => {
        let userListData = await getUserListByOuId(data.ORGAN, 1);
        setContactDdlData(userListData);
        contactDefaultDdlData.current = userListData;
    }

    // 載入聯絡窗口、自訂聯絡人資料
    const getContactData = async () => {
        let contactData = await SetContactService.getSetContactByOrgan(data.ORGAN);
        // SOURCE=1為SC資料(下拉選單)、SOURCE=2為自訂聯絡人資料(grid)
        let selSCData = contactData.filter(x => x.SOURCE == 1).map(x => x.CONTACT);
        let selectedData = contactDefaultDdlData.current.filter(i => {
            if (selSCData.includes(i.value)) {
                return i;
            }
        });
        setContactSelData([...selectedData]);
        setGridData(contactData.filter(x => x.SOURCE == 2));
    }

    React.useEffect(() => {
        loadContactData();
    }, [])

    /**
     * MultiSelect onChange事件
     * @param {*} e 
     */
    const onChange = (e) => {
        setContactSelData([...e.value]);
    };

    /**
     * 移除grid資料
     * @param {*} dataItem 
     */
    const remove = async (dataItem) => {
        //判定刪除的是否為新增的資料
        let index = dataItem.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
            :
            gridData.findIndex(record => record.DC_ID === dataItem.DC_ID);

        gridData.splice(index, 1);
        dataItem.editType = 3;
        setGridData([...gridData]);
    }

    /**
     * 刪除欄位
     * @param {*} prop 
     * @returns 
     */
    const DelCommandCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"刪除"} icon='close' look='default' onClick={() => {
                    remove(prop.dataItem)
                }} />
            </CommandCell>
        )
    }

    // 欄位驗證
    const validateField = Yup.object().shape({
        CONTACT: Yup.string().required("此欄位為必填"),
        TEL: Yup.string().required("此欄位為必填"),
        EMAIL: Yup.string().required("此欄位為必填").email("E-mail格式不正確"),
    });

    // 新增自訂聯絡人
    const add = () => {
        //算出hiddenIndexs最末碼以幫新增的資料加入流水號
        const hiddenIndexs = gridData.map(item => item.hiddenIndex ? item.hiddenIndex : 0);
        const LasthiddenIndex = hiddenIndexs.length > 0 ? hiddenIndexs.sort((a, b) => { return a - b; }).pop() : 0;
        const newRecord = {
            hiddenIndex: LasthiddenIndex + 1,
            ORGAN: data.ORGAN,
            SOURCE: 2,
            CONTACT: "",
            TEL: "",
            EMAIL: "",
            editType: 1
        }
        setGridData([...gridData, newRecord]);
    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = (item) => {
        let index = item.hiddenIndex ?
            gridData.findIndex(record => record.hiddenIndex === item.hiddenIndex)
            :
            gridData.findIndex(record => record.DC_ID === item.DC_ID);
        gridData.splice(index, 1, item);
        setIsDataChange(!isDataChange);
    }

    //文字輸入框
    const textInputCell = (prop) => {
        let max = 30;
        switch (prop.field) {
            case "TEL":
                max = 20; break;
            case "EMAIL":
                max = 128; break;
        }
        return (
            <TextInputCell
                {...prop}
                maxlength={max}
                required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                AlwaysEdit={true}
            />
        )
    }

    // 存檔
    const save = async () => {
        //驗證
        let notValids = [];
        for (let index = 0; index < gridData.length; index++) {
            const item = gridData[index];
            let isValid = await validateField.isValid(item);
            if (!isValid) {
                notValids.push(isValid);
            }
        }
        if (notValids.length > 0) {
            showGlobalMessageBox('儲存失敗，請確認資料填妥後重新嘗試!');
        }
        else {
            // 選擇的聯絡窗口資料
            let contactSaveData = contactSelData.map(x => {
                return { ORGAN: data.ORGAN, SOURCE: 1, CONTACT: x.value }
            })
            // 聯絡窗口資料、自訂聯絡人資料
            let DeptContact = [...contactSaveData, ...gridData];
            let saveData = {
                ORGAN: data.ORGAN,
                DeptContact: DeptContact
            }
            SetMaskOnOff(true);
            let saveResult = await SetContactService.saveSetContact(saveData);
            SetMaskOnOff(false);
            if (saveResult.success) {
                showGlobalMessageBox(saveResult.message, () => {
                    loadData();
                });
            }
            else {
                showGlobalMessageBox(saveResult.message);
            }
        }
    }

    return (
        <PageContainer toolbar={
            <>
                <Button type="button" title="存檔" onClick={() => save()}>存檔</Button>
                <Button type="button" title="取消" className="k-button-lighten" onClick={() => loadContactData()}>取消</Button>
                <Button type="button" title="新增自訂聯絡人" onClick={() => add()}>新增自訂聯絡人</Button>
            </>
        }>
            <form>
                <table>
                    <tbody>
                        <tr>
                            <th>機關名稱</th>
                            <td>{data.ORGAN_NAME}</td>
                            <th>機關代碼</th>
                            <td>{data.ORGAN}</td>
                        </tr>
                        <tr>
                            <th>聯絡窗口</th>
                            <td colSpan={3}>
                                <MultiSelect
                                    popupSettings={
                                        { className: "dropdown-text-size" }
                                    }
                                    data={contactDdlData}
                                    textField="text"
                                    dataItemKey="value"
                                    onChange={onChange}
                                    value={contactSelData} />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </form>
            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                data={gridData}
                resizable={true}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={DelCommandCell} width="50px" />
                <GridColumn field="CONTACT" title="聯絡人姓名" cell={textInputCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="TEL" title="聯絡電話" cell={textInputCell} headerCell={RequiredHeaderCell} />
                <GridColumn field="EMAIL" title="聯絡信箱" cell={textInputCell} headerCell={RequiredHeaderCell} />
            </Grid>
        </PageContainer>
    )
}
export default SetContactWindow;