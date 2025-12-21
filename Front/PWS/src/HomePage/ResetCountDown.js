import React from "react";
import { Button } from "@progress/kendo-react-buttons";
import ReactCountdown from 'react-countdown';
import { formatNumber } from '@telerik/kendo-intl';

const ResetCountDown = (props) => {

    const timer = React.useRef(null)
    const key = React.useRef(Math.random().toString().substr(2, 8))

    const renderer = ({ minutes, seconds }) => {
        return <div>
            <span style={{ fontWeight: "bold" }}>自動登出倒數：</span>
            <span style={{ color: "red" }}>{formatNumber(minutes, '00')}:
                {formatNumber(seconds, '00')}</span></div>
    };

    React.useEffect(() => {
        document.getElementById('root').classList.add("new-bg");
    }, []);

    return (
        <div className={'reset-count-down'}>
            <ReactCountdown
                key={key.current}
                ref={timer}
                date={localStorage.getItem('expires')}
                renderer={renderer}
                onComplete={async () => window.close()}
            />
            <div style={{ marginLeft: "5px" }}>
                <Button
                    onClick={() => {
                        localStorage.setItem("RestCountDown", "Y");
                        window.close()
                    }}
                    style={{ fontSize: "24px" }}>
                    重新計時
                </Button>
            </div>

        </div>
    );
}
export default ResetCountDown;