import React from 'react';
import { Window } from '@progress/kendo-react-dialogs';
import PropTypes from 'prop-types'

//Window視窗(含有遮罩+可使用百分比控制視窗大小)
//Controlled Mode
//Props:
// -width(number):自動設定的寬度 (可有可無)
// -height(number):自動設定的高度 (可有可無)
// -title(string):視窗標題字串
// -onClose(function)):關閉視窗之動作

/**
 * @typedef {object} Props
 * @prop {string} className
 * @prop {number} numberProp
 *
 */
const WindowBox = (props) => {
    //初始值
    const size = { width: 50, height: 60 }
    WindowBox.prototype={
        width:PropTypes.string
    }
    //若有傳入自訂的寬度、高度，以自訂的為主
    const width = window.screen.availWidth * (props.width ? props.width : size.width) / 100;
    const height = window.screen.availHeight * (props.height ? props.height : size.height) / 100;

    return (
        <div className="fullscreen" >
            <div className="k-overlay"></div>
            <Window
                initialWidth={width}
                initialHeight={height}
                title={props.title}
                onClose={props.onClose}
            >
                {props.children}
            </Window>
        </div>
    )
}

export default WindowBox;