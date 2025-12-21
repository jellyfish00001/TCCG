import React from 'react';
import { PageContainer } from "../../../Basic/PageContainer";
import { Button } from '@progress/kendo-react-buttons';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { Window } from '@progress/kendo-react-dialogs';
import SetContactService from './SetContactService';
import SetContactWindow from './SetContactWindow';
import { orderBy } from "@progress/kendo-data-query";
import { WindowResizehook } from '../../../Hook/useWindowResize';

export const SetContactMain = (props) => {
    const dimensions = WindowResizehook();

    const [gridData, setGridData] = React.useState([]);
    // 傳入跳窗資料
    const [dataItem, setDataItem] = React.useState({});
    //視窗顯示狀態
    const [windowVisible, setWindowVisible] = React.useState(false);
    //排序
    const [sort, setSort] = React.useState([
        {
            field: "", dir: "",
        },
    ]);

    //取得機關窗口維護資料
    const loadData = async () => {
        SetMaskOnOff(true);
        let data = await SetContactService.getSetContact();
        setGridData(data);
        SetMaskOnOff(false);
    }

    React.useEffect(() => {
        loadData();
    }, [])

    /**
     * 機關窗口欄位
     * @param {*} prop 
     * @returns 
     */
    const ContactCell = (prop) => {
        let data = prop.dataItem.DeptContact;
        return (
            <td>
                {data.map(x => {
                    return (
                        <div>
                            {x.SOURCE == 1 &&
                                <span>{x.CONTACT_NAME}</span>
                            }
                            {x.SOURCE == 2 &&
                                <span>{x.CONTACT}(連絡電話：{x.TEL} 聯絡信箱：{x.EMAIL})</span>
                            }
                        </div>
                    )
                })}
            </td>
        )
    }

    /**
     * 編輯欄位
     * @param {*} prop 
     * @returns 
     */
    const EditCommandCell = (prop) => {
        return (
            <CommandCell>
                <Button title={"編輯"} icon='edit' look='default' onClick={() => {
                    setWindowVisible(true);
                    setDataItem(prop.dataItem);
                }} />
            </CommandCell>
        )
    }

    /**
     * 切換排序
     * @param {*} event 
     */
    const sortChange = (event) => {
        setGridData(orderBy(gridData, event.sort));
        setSort(event.sort);
    };

    return (
        <>
            <PageContainer style={{
                height: '100%',
                overflow: 'auto'
            }}
                toolbar={<h3 className="k-dialog-titlebar">{"機關窗口維護"}</h3>}
            >
                <Grid
                    style={{
                        height: '100%',
                        overflow: 'auto',
                    }}
                    data={gridData}
                    resizable={true}
                    sort={sort}
                    onSortChange={sortChange}
                    sortable={{
                        allowUnsort: true, mode: "single",
                    }}>
                    <GridNoRecords>無資料</GridNoRecords>
                    <GridColumn title="編輯" cell={EditCommandCell} width="50px" />
                    <GridColumn field="ORGAN_NAME" title="機關名稱" width="200px" />
                    <GridColumn title="機關窗口" cell={ContactCell} />
                </Grid>

                {windowVisible &&
                    <div className="fullscreen window-fullscreen">
                        <Window
                            title={"機關聯絡窗口"}
                            onClose={() => setWindowVisible(false)}
                            width={dimensions.width * 0.6}
                            height={dimensions.height * 0.8}
                            draggable={false}
                            resizable={false}
                            modal={true}
                        >
                            <SetContactWindow
                                loadData={loadData}
                                data={dataItem}
                            />
                        </Window>
                    </div>
                }

            </PageContainer>
        </>
    )
}
export default SetContactMain;