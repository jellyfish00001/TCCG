import * as React from 'react';
import { Grid, GridColumn, GridNoRecords } from '@progress/kendo-react-grid';
import { api } from '../../../Basic/ApiFetch';
import { Pageable } from '../../../Basic/BasicData';
import { Button } from '@progress/kendo-react-buttons';
import { AddNoColumn, HtmlDecode } from '../../../Basic/SDOExtension';
import TwDatePicker from '../../../Components/DateInputs/TwDatePicker'
import moment from 'moment-taiwan';
import { PageContainer } from '../../../Basic/PageContainer';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'

const DetailComponent = props => {
    let subject = 'SUBJECT:' + HtmlDecode(props.dataItem.MAIL_SUBJECT);
    let content = 'CONTENT:' + HtmlDecode(props.dataItem.MAIL_CONTENT);
    return (
        <div style={{ whiteSpace: "pre-wrap" }}>{subject + '\r\n' + content}</div>
    )
}


const MailSenderCell = props => {
    let mailSender = JSON.parse(HtmlDecode(HtmlDecode(props.dataItem[props.field])));
    return (
        <td style={{ color: 'Blue' }}>
            {/* <a href={'mailto:' + mailSender.Address}>{mailSender.DisplayName}</a> */}
        </td>
    )
}

const MultipleMailCell = props => {
    let mails = JSON.parse(HtmlDecode(HtmlDecode(props.dataItem[props.field])));

    let mailDisplayName = '';
    mails.forEach(mail => {
        mailDisplayName += mail.DisplayName + ','
    });

    if (mailDisplayName.length > 0)
        mailDisplayName = mailDisplayName.slice(0, mailDisplayName.length - 1);
    return (
        <td>
            {mailDisplayName}
        </td>
    )
}

const SendFlgCell = props => {
    let value = props.dataItem[props.field]
    return (
        <td>
            {value ? '已' : '未'}寄出
        </td>
    )
}

const LogDateCell = props => {
    let value = props.dataItem[props.field];
    let date = new Date(value);
    return (
        //yyyy/MM/dd HH:mm:ss
        <td>{date.getFullYear()}/{date.getMonth() + 1}/{date.getDate()} {date.getHours()}:{date.getMinutes()}:{date.getSeconds()}</td>
    )
}

const Query = () => {
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
        mail: [],
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
        let url = getGlobalServerConfig().backEndUrl.get() + 'MailLog?'
        url += 'START_DATE=' + searchConditions.startDate;
        url += '&END_DATE=' + searchConditions.endDate;
        url += '&PAGE_NO=' + searchConditions.page;
        url += '&PAGE_SIZE=' + searchConditions.pageSize;
        let response = await api.Read(url);
        let searchResult = {
            mail: [],
            count: 0
        }
        if (response.ok)
            searchResult.mail = AddNoColumn(await response.json(), searchConditions.page, searchConditions.pageSize);
        if (searchResult.mail != null && searchResult.mail.length !== 0)
            searchResult.count = searchResult.mail[0].DATA_COUNT;
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
                    data={searchResult.mail}
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
                    <GridColumn field="MAIL_SENDER" title="寄件者" cell={MailSenderCell} />
                    <GridColumn field="MAIL_RECEIVER" title="收件者" cell={MultipleMailCell} />
                    <GridColumn field="CC_RECEIVER" title="副本" cell={MultipleMailCell} />
                    <GridColumn field="BCC_RECEIVER" title="密件" cell={MultipleMailCell} />
                    <GridColumn field="SEND_FLG" title="寄送狀態" cell={SendFlgCell} />
                    <GridColumn field="LOG_DATE" title="公告日期" cell={LogDateCell} />
                </Grid>
            </div>
        </PageContainer>
    )
}

export default Query;