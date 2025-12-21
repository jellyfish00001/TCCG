import * as React from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { PageContainer } from '../../../Basic/PageContainer';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker'
import moment from 'moment-taiwan';

const DetailComponent = props => {
    return (
        <table>
            <tbody>
                <tr>
                    <th style={{color:'white'}}>Sql Script</th>
                    <th style={{color:'white'}}>PARAMETERS</th>
                </tr>
                <tr>
                    <td>
                        <div style={{ whiteSpace: "pre-wrap" }}>{props.dataItem.COMMANDTEXT}</div>
                    </td>
                    <td>
                        <div style={{ whiteSpace: "pre-wrap",overflowWrap:'break-word' }}>{props.dataItem.PARAMETERS}</div>
                    </td>
                </tr>
            </tbody>
        </table>

    )
}

export const Query = () => {
    const [gridWidth, setGridWidth] = React.useState(0);

    const gridRef = React.useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);

    const pageable = {
        buttonCount: 10,
        info: true,
        type: 'numeric',
        pageSizes: true,
        previousNext: true
    }

    const [pageSet, setPageSet] = React.useState({
        pageSize: 10,
        page: 0
    });

    const [searchConditions, setSearchConditions] = React.useState({
        startDate: moment(new Date(Date.now())).format('YYYY-MM-DD'),
        endDate: moment(new Date(Date.now())).format('YYYY-MM-DD'),
    });

    const [searchResult, setSearchResults] = React.useState({
        sql: [],
        count: 0
    })

    const [datePicker, setDatePicker] = React.useState({
        dataPickerValue: new Date(),
        dataPickerValid: true,
    })

    const [ignore, forceUpdate] = React.useReducer(x => x + 1, 0);

    const today = new Date();

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadData = async searchConditions => {
        let url = ServerConfig.backEndUrl + 'SqlLog?'
        url += 'START_DATE=' + searchConditions.startDate;
        url += '&END_DATE=' + searchConditions.endDate;
        url += '&PAGE_NO=' + searchConditions.page;
        url += '&PAGE_SIZE=' + searchConditions.pageSize;
        let response = await api.Read(url);
        let searchResult = {
            sql: [],
            count: 0
        }
        if (response.ok)
            searchResult.sql = AddNoColumn(await response.json(), searchConditions.page, searchConditions.pageSize);
        if (searchResult.sql != null && searchResult.sql.length !== 0)
            searchResult.count = searchResult.sql[0].DATA_COUNT;
        else
            searchResult.count = 0;

        setSearchResults(searchResult);
    }

    const search = () => {
        setSearchConditions({
            startDate: moment(datePicker.dataPickerValue).format('YYYY-MM-DD'),
            endDate: moment(datePicker.dataPickerValue).format('YYYY-MM-DD')
        })
    }

    const onPageChange = event => {
        setPageSet({
            pageSize: event.page.take,
            page: (event.page.skip / event.page.take)
        });
    }

    const onExpandChange = event => {
        event.dataItem.expanded = !event.dataItem.expanded;
        forceUpdate();
    }

    React.useEffect(() => {
        loadData({
            ...searchConditions,
            ...pageSet
        });
    }, [searchConditions, pageSet])

    return (
        <PageContainer
            toolbar={
                <Button onClick={search}>查詢</Button>
            }
        >
            <div>
                <table style={{ width: '100%' }}>
                    <tbody>
                        <tr>
                            <th style={{ width: '20%' }}>
                                查詢日期
                            </th>
                            <td style={{ width: '80%' }}>
                                <TwDatePicker
                                    format={"yyy/MM/dd"}
                                    onChange={event => setDatePicker({ ...datePicker, dataPickerValue: event.value })}
                                    value={datePicker.dataPickerValue}
                                    max={new Date()}
                                    min={new Date(today.getFullYear(), today.getMonth(), 1)}//最早到當月1號 ps:每月log另存table

                                    //日期欄位是否驗證
                                    valid={true}
                                    //日期欄位是否正確
                                    dateValidate={(valid) => {
                                        setDatePicker({ ...datePicker, dataPickerValid: valid })
                                    }}
                                />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div>
                <Grid
                    ref={gridRef}
                    style={{ height: '700px' }}
                    data={searchResult.sql}
                    skip={(pageSet.page) * pageSet.pageSize}
                    pageSize={pageSet.pageSize}
                    total={searchResult.count}
                    pageable={pageable}
                    detail={DetailComponent}
                    expandField="expanded"
                    onExpandChange={onExpandChange}
                    onPageChange={onPageChange}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(3)} field="NO" title="No" />
                    <GridColumn field="USER_ID" title="使用者" />
                    <GridColumn field="REQUEST_URL" title="程式路徑" />
                    {/* <GridColumn field="PARAMETERS" title="參數" /> */}
                    <GridColumn field="LOG_DATE" title="日誌時間" />
                </Grid>
            </div>
        </PageContainer>
    )
}

export default Query;