import React, { useState, useEffect } from 'react';
import { IsNullOrEmpty } from '../../../../Basic/SDOExtension';
import { TabStrip, TabStripTab } from '@progress/kendo-react-layout';
import ContentEditor from './ContentEditor';
import WindowBox from '../../../../Components/Dialogs/WindowBox'

const AddMdf = props => {
    const [selected, changeTab] = useState(0);
    const [createState, setCreateState] = useState({
        hasCreated: false,
        mailId: ''
    })

    const handleSelect = event => {
        changeTab(event.selected);
    }

    useEffect(() => {
        changeTab(0);
        if (props.visible)
            setCreateState({
                hasCreated: !IsNullOrEmpty(props.mailId),
                mailId: props.mailId
            })
        else {
            setCreateState({
                hasCreated: false,
                mailId: ''
            })
        }
    }, [props.visible, props.mailId])

    return (
        <div>
            {props.visible &&
                <WindowBox title={props.isCreate ? '新增' : '修改'} onClose={props.onClose} width='85' height='90'>
                    <TabStrip selected={selected} onSelect={handleSelect}>
                        <TabStripTab title="郵件範本">
                            <ContentEditor
                                mailId={createState.mailId}
                                visible={selected === 0}
                                setHasCreated={(hasCreated, newMail) => setCreateState({
                                    hasCreated: hasCreated,
                                    mailId: hasCreated ? newMail : createState.mailId//Create完畢後儲存MAIL_ID供寄、受件者設定使用
                                })}
                            />
                        </TabStripTab>
                        {/* <TabStripTab title="寄、收件者" disabled={!createState.hasCreated}>
                            <RecipientQuery
                                mailId={createState.mailId}
                                visible={selected === 1}
                            />
                        </TabStripTab> */}
                    </TabStrip>
                </WindowBox>
            }
        </div>
    )
}

export default AddMdf;