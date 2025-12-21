import * as React from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { api } from '../../../Basic/ApiFetch';
import { ServerConfig, Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import { PageContainer } from '../../../Basic/PageContainer';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker'
import moment from 'moment-taiwan';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'


const DetailComponent = props => {
    let text = '';
    for (let prop in props.dataItem) {
        if (prop !== 'DATA_COUNT' && prop !== 'expanded' && prop !== 'NO')
            text += prop + ':' + props.dataItem[prop] + '\r\n';
    }
    return (
        <div style={{ whiteSpace: "pre-wrap" }}>{text}</div>
    )
}

const ResponseCodeCell = props => {
    const value = props.dataItem[props.field];
    return (
        <td key={props.dataItem.SID} style={{ color: value < 200 || value >= 300 ? 'red' : 'green' }}>
            {value}
        </td>
    );
}


export const Query = () => {
    const [gridWidth, setGridWidth] = React.useState(0);

    const gridRef = React.useCallback(grid => {
        if (grid !== null)
            setGridWidth(grid.element.offsetWidth);
    }, []);

    const [pageSet, setPageSet] = React.useState({
        pageSize: 10,
        page: 0
    });

    const [searchConditions, setSearchConditions] = React.useState({
        startDate: moment(new Date(Date.now())).format('YYYY-MM-DD'),
        endDate: moment(new Date(Date.now())).format('YYYY-MM-DD'),
    });

    const [searchResult, setSearchResults] = React.useState({
        api: [],
        count: 0
    })

    const [datePicker, setDatePicker] = React.useState({
        dataPickerValue: new Date(),
        dataPickerValid: true,
    })

    const [ignore, forceUpdate] = React.useReducer(x => x + 1, 0);

    const setWidth = percentage => {
        return Math.round(gridWidth * (percentage / 100));
    }

    const loadData = async searchConditions => {
        let url = getGlobalServerConfig().backEndUrl.get() + 'ApiLog?'
        url += 'START_DATE=' + searchConditions.startDate;
        url += '&END_DATE=' + searchConditions.endDate;
        url += '&PAGE_NO=' + searchConditions.page;
        url += '&PAGE_SIZE=' + searchConditions.pageSize;
        let response = await api.Read(url);
        let searchResult = {
            api: [],
            count: 0
        }
        if (response.ok)
            searchResult.api = AddNoColumn(await response.json(), searchConditions.page, searchConditions.pageSize);
        if (searchResult.api != null && searchResult.api.length !== 0)
            searchResult.count = searchResult.api[0].DATA_COUNT;
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
                    data={searchResult.api}
                    skip={(pageSet.page) * pageSet.pageSize}
                    pageSize={pageSet.pageSize}
                    total={searchResult.count}
                    pageable={Pageable}
                    detail={DetailComponent}
                    expandField="expanded"
                    onExpandChange={onExpandChange}
                    onPageChange={onPageChange}
                >
                    <GridNoRecords> </GridNoRecords>
                    <GridColumn width={setWidth(3)} field="NO" title="No" />
                    <GridColumn width={setWidth(10)} field="RESPONSE_CODE" title="狀態碼" cell={ResponseCodeCell} />
                    <GridColumn width={setWidth(15)} field="IP" title="發送位址" />
                    <GridColumn field="REQUEST_URL" title="請求路徑" />
                    <GridColumn field="LOG_DATE" title="日誌時間" />
                </Grid>
            </div>
        </PageContainer>
    )
}

export default Query;