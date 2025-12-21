import React, { useState } from 'react';
import CheckBoxList from '../../Components/Input/CheckBoxList';
import RadioBoxList from '../../Components/Input/RadioBoxList';
import OrgSelect from '../../Components/Selector/Orgselector'
import OrgUserSelector from '../../Components/Selector/OrgUserSelector'
import DualListBox from 'react-dual-listbox';
import { FileUploader } from '../../Components/Upload/FileUploader'
import TwDatePicker from '../../Components/DateInputs/TwDatePicker'

export default function Query() {
    const [selected, setSelected] = useState(['one']);
    const [fileSeqNo, setFileSeqNo] = useState([]);
    const [date, setDate] = useState(null);
    const [files, setFiles] = React.useState([]);

    return (
        <div className="fnForm">
            <h4>CheckBoxList</h4>
            <CheckBoxList
                group='CheckBoxList'
                groupName='CheckBoxList'
                valueField='id'
                textField='name'
                data={[
                    { id: 1, name: 'First' },
                    { id: 2, name: 'Second', checked: true },
                    { id: 3, name: 'Third', disabled: true }
                ]}
                onChange={event => { console.log(event); }}
            />
            <pre>
                {'<CheckBoxList'}{'\n'}
                {'    group=\'CheckBoxList\''}{'      // -group(string):群組ID'}{'\n'}
                {'    groupName=\'CheckBoxList\''}{'  // -groupName(string)?:群組名稱 ps:不為空值則顯示Title'}{'\n'}
                {'    valueField=\'id\''}{'           // -valueField(string):value欄位名稱'}{'\n'}
                {'    textField=\'name\''}{'          // -textField(string):text欄位名稱'}{'\n'}
                {'    data={['}{'                   // -data(array<object>):設定資料。 ps:checked欄位可預設是否勾選、disabled欄位可設定停用及啟用'}{'\n'}
                {'        {id:1, name:\'First\'},'}{'\n'}
                {'        {id:2, name:\'Second\', checked:true},'}{'\n'}
                {'        {id:3, name:\'Third\', disabled:true}'}{'\n'}
                {'    ]}'}{'\n'}
                {'    onChange={event => {console.log(event);}}'}{'\n'}
                {'/>'}{'\n'}
            </pre>
            <h4>RadioBoxList</h4>
            <RadioBoxList
                group='RadioBoxList'
                groupName='RadioBoxList'
                valueField='id'
                textField='name'
                data={[
                    { id: 1, name: 'First' },
                    { id: 2, name: 'Second', checked: true },
                    { id: 3, name: 'Third', disabled: true }
                ]}
                onChange={event => { console.log(event); }}
            />
            <pre>
                {'<RadioBoxList'}{'\n'}
                {'    group=\'RadioBoxList\''}{'      // -group(string):群組ID'}{'\n'}
                {'    groupName=\'RadioBoxList\''}{'  // -groupName(string)?:群組名稱 ps:不為空值則顯示Title'}{'\n'}
                {'    valueField=\'id\''}{'           // -valueField(string):value欄位名稱'}{'\n'}
                {'    textField=\'name\''}{'          // -textField(string):text欄位名稱'}{'\n'}
                {'    data={['}{'                   // -data(array<object>):設定資料。 ps:checked欄位可預設是否勾選、disabled欄位可設定停用及啟用'}{'\n'}
                {'        {id:1, name:\'First\'},'}{'\n'}
                {'        {id:2, name:\'Second\', checked:true},'}{'\n'}
                {'        {id:3, name:\'Third\', disabled:true}'}{'\n'}
                {'    ]}'}{'\n'}
                {'    onChange={event => {console.log(event);}}'}{'\n'}
                {'/>'}{'\n'}
            </pre>
            <h4>OrgSelect</h4>
            <OrgSelect
                multiple={false}
                setValue={e => { console.log(e) }}
            />
            <OrgSelect
                multiple={true}
                value={[
                    { ORG_ID: "LSBU3", ORG_DISPLAY: "LSBU3-精實服務三處" },
                    { ORG_ID: "AOSD3", ORG_DISPLAY: "AOSD3-客戶維運服務三部" },
                    { ORG_ID: "SDSD3", ORG_DISPLAY: "SDSD3-系統開發服務三部" }
                ]}
                setValue={e => { console.log(e) }}
            />
            <pre>
                {'<OrgSelect'}{'\n'}
                {'    multiple={true}'}{'\n'}
                {'    value={['}{'\n'}
                {'        {ORG_ID: "LSBU3", ORG_DISPLAY: "LSBU3-精實服務三處"},'}{'\n'}
                {'        {ORG_ID: "AOSD3", ORG_DISPLAY: "AOSD3-客戶維運服務三部"},'}{'\n'}
                {'        {ORG_ID: "SDSD3", ORG_DISPLAY: "SDSD3-系統開發服務三部"}'}{'\n'}
                {'    ]}'}{'\n'}
                {'    setValue={e => {console.log(e)}}'}{'\n'}
                {'/>'}{'\n'}
            </pre>
            <h4>DualListBox</h4>
            <DualListBox
                options={[
                    { value: 'one', label: 'Option One' },
                    { value: 'two', label: 'Option Two' },
                ]}
                selected={selected}
                onChange={e => {
                    setSelected(e);
                    console.log(e);
                }}
            />
            <pre>
                {'<DualListBox'}{'\n'}
                {'    options={['}{'\n'}
                {'        { value: \'one\', label: \'Option One\' },'}{'\n'}
                {'        { value: \'two\', label: \'Option Two\' },'}{'\n'}
                {'    selected={selected}'}{'\n'}
                {'    onChange={e => {'}{'\n'}
                {'        setSelected(e);'}{'\n'}
                {'        console.log(e);'}{'\n'}
                {'   }}'}{'\n'}
                {'/>'}{'\n'}
            </pre>
            <h4>OrgUserSelector</h4>
            <OrgUserSelector
                value={selected}
                setValue={e => {
                    setSelected(e);
                    console.log(e);
                }}
            />
            <pre>
                {'<OrgUserSelector'}{'\n'}
                {'    value={selected}'}{'\n'}
                {'    setValue={e => {'}{'\n'}
                {'        setSelected(e);'}{'\n'}
                {'        console.log(e);'}{'\n'}
                {'   }}'}{'\n'}
                {'/>'}{'\n'}
            </pre>
            <h4>TwDatePicker</h4>
            <table><tbody>
                <tr>
                    <td>
                        <TwDatePicker
                            value={date}
                            onChange={e => { console.log(e); setDate(e.target.value); }}
                        />
                    </td>
                    <td>
                        <TwDatePicker
                            id='test'
                            value={date}
                            onChange={e => { console.log(e); setDate(e.target.value); }}
                            timepick={true}
                        />
                    </td>
                </tr>
                <tr>
                    <td>
                        <pre>
                            {'<TwDatePicker'}{'\n'}
                            {'    value={date}'}{'\n'}
                            {'    onChange={e => {'}{'\n'}
                            {'       setDate(e.target.value);'}{'\n'}
                            {'       console.log(e);'}{'\n'}
                            {'    }}'}{'\n'}
                            {'/>'}{'\n'}
                        </pre>
                    </td>
                    <td>
                        <pre>
                            {'<TwDatePicker'}{'\n'}
                            {'    value={date}'}{'\n'}
                            {'    timepick={true}'}{'\n'}
                            {'    onChange={e => {'}{'\n'}
                            {'       setDate(e.target.value);'}{'\n'}
                            {'       console.log(e);'}{'\n'}
                            {'    }}'}{'\n'}
                            {'/>'}{'\n'}
                        </pre>
                    </td>
                </tr>
            </tbody></table>
            <h4>FileUploader</h4>
            <FileUploader
                fileSeqNo={fileSeqNo}
                setValue={setFileSeqNo}
                files={files}
                setFiles={setFiles}
            />
            <pre>
                {'<FileUploader'}{'\n'}
                {'    fileSeqNo={fileSeqNo}'}{'\n'}
                {'    setValue={setFileSeqNo}'}{'\n'}
                {'/>'}{'\n'}
            </pre>
        </div>
    )
}