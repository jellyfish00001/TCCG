import React, { useState } from 'react'
import { Button } from '@progress/kendo-react-buttons';
import { Input } from '@progress/kendo-react-inputs';
import Tour from 'reactour';

const style = {
    btn: {
        height: '32px',
        borderRadius: '4px',
        margin: '10px 4px'
    },
    blue: { backgroundColor: '#4da0d6' },
    primary: { backgroundColor: '#007bff' },
    success: { backgroundColor: '#28a745' },
    info: { backgroundColor: '#17a2b8' },
    warning: { backgroundColor: '#ffc107' },
    danger: { backgroundColor: '#dc3545' },
    link: { backgroundColor: 'lightgray' },
}

const DemoEnjoyHint = () => {
    const [tourVisible, setTourVisible] = useState(false);
    const [currentStep, setCurrentStep] = useState(0);
    const [endTourKey, setEndTourKey] = useState('');

    const steps = [
        {
            selector: '[class=".step1"]',
            content: (
                <div className="custom-reactour-helper">
                    <div className="text-block" style={{ marginLeft: '48px', fontSize: '24px' }}>歡迎使用Reactour引導工具</div>
                </div>
            )
        },
        {
            selector: '[class="k-button .step2"]',
            content: (
                <div className="custom-reactour-helper">
                    <div className="text-block">
                        設定聚焦區塊:
                        <pre style={{ color: 'white' }}>
                            {`<`}<text-g>Button </text-g><text-lb>className</text-lb>=<text-or>".step2"</text-or>{`>Start!</`}<text-g>Button</text-g>{`>`}<br />
                            <text-b>const step</text-b>{` = [\r\n  {\r\n    `}
                            <text-lb>selector: </text-lb><text-or>'[class=".k-button .step2"]'</text-or>{`,\r\n    `}
                            <text-lb>content: </text-lb><text-or>'customize message'</text-or>{`\r\n  }\r\n]\r\n`}
                            {`<`}<text-g>Tour</text-g><br />
                            <text-lb>{`  steps`}</text-lb>=<text-b>{`{steps}\r\n`}</text-b>
                            {`/>`}
                        </pre>
                    </div>
                </div>
            ),
            position: "right"
        },
        {
            selector: '[class=".step3"]',
            content: (
                <div className="custom-reactour-helper">
                    <div className="text-block">
                        設定按鈕文字:
                        <pre style={{ color: 'white' }}>
                            {`<`}<text-g>Tour</text-g><br />
                            <text-lb>{`  prevButton`}</text-lb>=<text-b>{'{'}</text-b>{`<`}<text-b>span</text-b>{`>`}Previous Step{`</`}<text-b>span</text-b>{`>}\r\n`}
                            {`/>`}
                        </pre>
                    </div>
                </div>
            )
        },
        {
            selector: '[class="k-button .step4"]',
            content: (
                <div className="custom-reactour-helper">
                    <div className="text-block">
                        顯示或隱藏按鈕:
                        <pre style={{ color: 'white' }}>
                            {`<`}<text-g>Tour</text-g><br />
                            <text-lb>{`  showButtons`}</text-lb>=<text-b>{'{'}false{`}`}</text-b><br />
                            {`/>`}
                        </pre>
                    </div>
                </div>
            ),
            position: "top"
        },
        {
            selector: '[class=".step5 k-textbox"]',
            content: (
                <div className="custom-reactour-helper">
                    <div className="text-block">
                        {'輸入 GSS\r\n'}
                        {'按下Enter結束引導工具\r\n'}
                    </div>
                </div>
            ),
            action: () => {
                console.log('last step');
            },
            position: "right"
        },
    ]

    //
    const setTourState = () => {
        setTourVisible(!tourVisible);
    }
    return (
        <div>
            <div style={{ width: '100%', padding: '0px 250px 0px 250px' }}>
                <div style={{ marginTop: "80px" }} >
                    <div>
                        <div className=".step1">
                            <h1>Reactour</h1>
                            <p><i>Tourist Guide into your React Components</i></p>
                        </div>
                        <div>
                            <p><Button className=".step2" style={{ ...style.btn, ...style.blue, fontSize: '20px' }} onClick={() => { console.log('Welcome~') }}>Start!</Button></p>
                            <p><a href='/' onClick={(e) => {
                                e.preventDefault();
                                setTourState();
                            }}>Show Hint</a></p>
                        </div>
                    </div>
                </div>
                <div>
                    <div>
                        <h1 className=".step3">Buttons</h1>
                    </div>
                    <p>
                        <Button style={{ ...style.btn, ...style.default }}>Default</Button>
                        <Button style={{ ...style.btn, ...style.primary }}>Primary</Button>
                        <Button className=".step4" style={{ ...style.btn, ...style.success }} onClick={() => { setCurrentStep(currentStep + 1) }}>Success</Button>
                        <Button style={{ ...style.btn, ...style.info }}>Info</Button>
                        <Button style={{ ...style.btn, ...style.warning }}>Warning</Button>
                        <Button style={{ ...style.btn, ...style.danger }}>Danger</Button>
                        <Button style={{ ...style.btn, ...style.link }}>Link</Button>
                    </p>
                    <p>
                        <Button style={{ ...style.btn, ...style.default }} disabled={true}>Default</Button>
                        <Button style={{ ...style.btn, ...style.primary }} disabled={true}>Primary</Button>
                        <Button style={{ ...style.btn, ...style.success }} disabled={true}>Success</Button>
                        <Button style={{ ...style.btn, ...style.info }} disabled={true}>Info</Button>
                        <Button style={{ ...style.btn, ...style.warning }} disabled={true}>Warning</Button>
                        <Button style={{ ...style.btn, ...style.danger }} disabled={true}>Danger</Button>
                        <Button style={{ ...style.btn, ...style.link }} disabled={true}>Link</Button>
                    </p>
                    <p>
                        <Input className=".step5" type='text'
                            onChange={(e) => {
                                setEndTourKey(e.target.value);
                            }}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter' && endTourKey === 'GSS') {
                                    setTourState();
                                }
                            }}
                        />
                    </p>
                </div>
                <Tour
                    steps={steps}
                    isOpen={tourVisible}
                    onRequestClose={() => { setTourState() }}
                    className='custom-reactour-helper'
                    showNumber={false}
                    showNavigation={false}
                    showButtons={currentStep < 3}
                    //closeWithMask={false}
                    disableKeyboardNavigation={true}
                    showCloseButton={false}
                    rounded={5}
                    maskSpace={8}
                    goToStep={currentStep}
                    getCurrentStep={(cur) => setCurrentStep(cur)}
                    prevButton={<span className='custom-reactour-button' style={{ display: currentStep === 0 ? 'none' : '' }}>{currentStep === 2 ? 'Previous Step' : '上一步'}</span>}
                    nextButton={<span className='custom-reactour-button'>下一步</span>}
                    onAfterOpen={() => { setCurrentStep(0); }}
                />
            </div >
            <div>
                <h4>初始設定與組建</h4>
                <pre>{`
    // 引導步驟建立
    const steps = [
        {
            selector: ... , //綁定聚焦區塊
            content: ...    //自訂顯示內容 Type: [string | node]
        },
        
        ...
    ];
    // 控制顯示
    const [visible, setVisible] = useState(true);
    
    return (
        <Tour
            //必須設置這三個參數
            steps={steps}
            isOpen={ visible }
            onRequestClose={() => { setVisible(false) }}
        />

        ...
    );
                `}</pre>
                <h4>綁定聚焦區塊到DOM</h4>
                <pre>{`
    <div className="focus-on-me">
        ...
    </div>

    const steps = [
        {
            selector: '[class="focus-on-me"]',
            content: ...
        },

        ...
    ]
                `}</pre>
                <h4>觸發動作</h4>
                <pre>{`
    const steps = [
        {
            selectore: ... ,
            content: ... ,
            action: () => {
                // do something here !!
            }
        },
    ]
                `}</pre>
                <h4>其他設定</h4>
                <pre>{`
    <Tour
        ...
        getCurrentStep={(currentStep)=>{ 
            //取得目前步驟, 從0開始計算
        }}
        goToStep={number} //前往特定步驟

        prevButton={node} //自訂[上一步]
        nextButton={node} //　　[下一步] 按鈕外觀
        showButtons={boolean} //是否顯示按鈕

        showNumber={boolean} //是否顯示步驟編號

        closeWithMask={boolean} //點擊遮罩關閉引導工具
        disableInteraction={boolean} //聚焦區塊是否可以互動
        
    />
                `}</pre>
            </div>
        </div>
    )
}

export default DemoEnjoyHint;