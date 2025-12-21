
import { Button } from '@progress/kendo-react-buttons';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import React, { useRef } from 'react';
import { CommandCell } from "../../../Components/GridCell/CommandCell";
import { TextInputCell } from '../../../Components/GridCell/TextInputCell';


const InnProjectPartnerGrid = (props) => {

    const { InnPartnerData, setEditedInnPartner } = props

    const [gridData, setGridData] = React.useState([]);

    /**
     * 文字輸入
     * @param {*} props 
     * @returns 
     */
    const textInputCell = props => {
        return (
            <TextInputCell
                {...props}
                //required={true}
                editable={true}
                onCellInputBlur={onCellInputBlur}
                AlwaysEdit={true}
            />
        );

    }

    /**
     * 供可切換欄位的input onBlur callback的事件
     * @param {object} item
     */
    const onCellInputBlur = async (item) => {
        setGridData([...gridData])
        setEditedInnPartner([...gridData]);
    }


    /**
     * 新增
     */
    const addNew = () => {

        //新增時寫入預設值
        if (gridData.length < 5) {
            const newRecord = {
                PARTNER_ORG: "",
                PARTNER_UNIT: "",
                PARTNER_TITLE: "",
                PARTNER_NAME: ""

            }
            setGridData([...gridData, newRecord]);
            setEditedInnPartner([...gridData, newRecord]);

        }

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

    /**
     * 移除
     * @param {*} dataItem 
     */
    const remove = (dataItem) => {
        let index = gridData.findIndex(record => record.hiddenIndex === dataItem.hiddenIndex)
        gridData.splice(index, 1);
        setGridData([...gridData]);
        setEditedInnPartner([...gridData]);

    }


    React.useEffect(() => {
        setGridData(InnPartnerData);
    }, [InnPartnerData])


    return (
        <>
            <div className="fn-buttons">
                <Button title="新增" type="button" onClick={addNew}>新增</Button>
            </div>

            <Grid
                style={{
                    height: '100%',
                    overflow: 'auto',
                }}
                resizable={true}
                data={gridData}
            >
                <GridNoRecords>無資料</GridNoRecords>
                <GridColumn title="刪除" cell={DelCommandCell} width="85%" />
                <GridColumn field="PARTNER_ORG" title="機關" cell={textInputCell} />
                <GridColumn field="PARTNER_UNIT" title="所屬單位" cell={textInputCell} width="250%" />
                <GridColumn field="PARTNER_TITLE" title="職稱" cell={textInputCell} width="200%" />
                <GridColumn field="PARTNER_NAME" title="姓名" cell={textInputCell} width="200%" />
            </Grid>

        </>
    );
}

export default InnProjectPartnerGrid;