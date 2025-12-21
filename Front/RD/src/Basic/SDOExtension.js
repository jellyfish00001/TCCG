//*****共用的method工具*****

import { defaultFormat, GetHistory } from './BasicData';
import moment from 'moment-taiwan';
import * as React from 'react';
import { getGlobalServerConfig } from '../Route/RootMiddleware';
import CacheLoader from './CacheLoader';
import { screen } from '@testing-library/react';
import { menuIcons } from '../Basic/BasicData';
import { SetStorageData } from './CommonService';


export const IsNullOrEmpty = (object) => {
    if (object === null || object === undefined)
        return true;
    if (typeof object === "string" && object === '')
        return true;
    if (typeof object === "object" && JSON.stringify(object) === "[]")
        return true;
    return false;
}

//為陣列資料加上No欄位供Grid顯示
//data:陣列資料
//page?:頁數  ps:從0開始
//pageSize?:每頁筆數
export const AddNoColumn = (data, page = 0, pageSize = 0) => {
    if (!Array.isArray(data) || data.length === 0)
        return data;

    if (data[0].hasOwnProperty('NO'))
        return data

    let No = 1;
    No += page * pageSize;
    data.forEach(dataItem => {
        Object.defineProperty(dataItem, 'NO', {
            value: No++,
            writable: true, //可寫
            enumerable: true //可被列舉出 
        })
    })
    return data;
}


export const HtmlDecode = text => {
    // return text;
    // text = text.replace(/<p>/gi, '＜p＞');
    // text = text.replace(/<\/p>/gi, '＜／p＞');
    // text = text.replace(/<br>/gi, '＜br＞');
    // text = text.replace(/<br\/>/gi, '＜br／＞');
    let temp = document.createElement("div");
    temp.innerHTML = text;
    let output = temp.innerText || temp.textContent;
    temp = null;
    // output = output.replace(/＜p＞/gi, '<p>');
    // output = output.replace(/＜／p＞/gi, '</p>');
    // output = output.replace(/＜br＞/gi, '<br>');
    // output = output.replace(/＜br／＞/gi, '<br>');
    return output;
}

export const SetMaskOnOff = async flag => {
    let overlay = document.getElementById("globalOverlay");
    if (overlay !== null) {
        overlay.className = flag ? "fullscreen" : "hide fullscreen";
    }
}

export const FormatDate = (date, format = defaultFormat) => {
    /* 常用日期格式:
        YYYY-MM-DD      e.g. 2020-09-10
        YYYY/MM/DD HH:mm:ss     e.g. 2020/09/10 10:11:11
        tYY/MM/DD       e.g. 109/09/10
        tYY年MM月DD日     e.g. 109年09月10日
        MM月DD日 tYY年    e.g. 09月10日 109年
    */
    if (IsNullOrEmpty(date))
        return null;
    return moment(date).format(format);
}

//機關選單資料轉換(取得Org object value)
export const GetOrgValue = (data) => {
    let value = [];
    data.map(item => {
        value.push(item.ORG_ID)
        return value;
    })
    return value;
}

export const IsFunction = functionToCheck => {
    return functionToCheck && {}.toString.call(functionToCheck) === '[object Function]';
}

/**
     * 紀錄GridData的暫存行為，以供增刪修時使用
     * @param {*} editedGridData 傳入useRef以進行記錄
     * @param {*} item 需增刪的資料
     * @param {string} newItemFieldName 供新增資料判定的欄位名
     * @param {string} itemFieldName 供舊資料判定的欄位名
     */
export const handleEditedGridData = (editedGridData, item, newItemFieldName, itemFieldName) => {
    let hasChange = false;
    //如果是新資料
    if (item[newItemFieldName] && item.editType === 1) {
        //如果hiddenIndex找的到將item覆蓋以更新新增的資料
        if (editedGridData.current.some(ei => ei[newItemFieldName] === item[newItemFieldName])) {
            let newItems = editedGridData.current.map(ei => ei[newItemFieldName] === item[newItemFieldName] ? item : ei);
            editedGridData.current = newItems;
            hasChange = true;
        } else {//否則紀錄資料
            editedGridData.current.push(item);
            hasChange = true;

        }
    } else if (item.editType === 2) {//更新舊資料
        //如果找的到將item覆蓋以更新資料
        if (editedGridData.current.some(ei => ei[itemFieldName] === item[itemFieldName])) {
            let newItems = editedGridData.current.map(ei => ei[itemFieldName] === item[itemFieldName] ? item : ei);
            editedGridData.current = newItems;
            hasChange = true;

        }
        else {//否則紀錄資料
            editedGridData.current.push(item);
            hasChange = true;
        }
    } else if (item.editType === 3) {//刪除
        //如果找的到
        if (editedGridData.current.some(ei => ei[itemFieldName] === item[itemFieldName] && item[itemFieldName] !== 0)) {

            let newItems = editedGridData.current.map(ei => ei[itemFieldName] === item[itemFieldName] ? item : ei);
            editedGridData.current = newItems;
            hasChange = true;
        }//如果hiddenIndex找的到將新增的紀錄移除
        else if (editedGridData.current.some(ei => ei[newItemFieldName] === item[newItemFieldName] && item[itemFieldName] === 0)) {

            let index = editedGridData.current.findIndex(record => record[newItemFieldName] === item[newItemFieldName]);
            editedGridData.current.splice(index, 1);
            hasChange = true;
        } else {
            if (!item[newItemFieldName])
                editedGridData.current.push(item);
            hasChange = true;
        }
    }
    return hasChange;
}

/**
 * 民國年日期驗證規則
 */
export const twDateRegex = new RegExp(/^([0-9]{2}|[0-9]{3})\/(0[1-9]|1[012])\/(0[1-9]|[12][0-9]|3[01])$/); //ex:110/06/13

/**
 * 連絡電話驗證規則:
 * 1.(2~4碼)1~4碼-4碼
 * 2.(2~4碼)1~4碼-4碼#分機
 */
// export const telRegex = /\(\d{2,4}\)(?:(?=\d{1,4}-\d{4}#)\d{1,4}-\d{4}#\d+$|\d{1,4}-\d{4}$)/g;

/**
 * 連絡電話驗證規則:
 * 只允許數字、括號及#
 */
export const telRegex = /^[0-9\#\(\)\/]+$/g;

/**
 * 手機驗證規則:
 * 10碼
 */
export const mobileRegex = /\d{10}$/;

/**
* 統一編號檢查
* @param {String} regNo 統一編號
* @returns true:符合 false:不符合
*/
export const regNoVerify = (regNo) => {
    // 統一編號數量
    let count = 8;
    // 邏輯乘數
    let multiplier = [1, 2, 1, 2, 1, 2, 4, 1];
    // 最後整除值
    let lastDivisibleVal = 5;

    // 非數字 or 不是8碼 都是錯誤
    if (isNaN(parseInt(regNo)) || regNo.length !== count) {
        return false;
    }

    let result;
    if (parseInt(regNo.charAt(6)) === 7) {
        // 統一編號第7位數為"7"
        result = seventhIsSeven(regNo, count, multiplier, lastDivisibleVal);
    }
    else {
        // 統一編號第7位數非"7"
        result = seventhIsNotSeven(regNo, count, multiplier, lastDivisibleVal);
    }

    return result;
}

/**
 * 第7位數為"7"
 * @param {*} regNo 統一編號
 * @param {*} count 統一編號數量
 * @param {*} multiplier 邏輯乘數
 * @param {*} lastDivisibleVal 最後整除值
 * @returns 
 */
const seventhIsSeven = (regNo, count, multiplier, lastDivisibleVal) => {
    let total1 = 0;
    let total2 = 0;

    for (let i = 0; i < count; i++) {
        if (i === 6) {
            total1 += 1;
            total2 += 0;
        }
        else {
            let val = parseInt(regNo.charAt(i)) * multiplier[i];
            let valStr = val.toString();
            if (valStr.length > 1) {
                for (let j = 0; j < valStr.length; j++) {
                    total1 += parseInt(valStr.charAt(j));
                    total2 += parseInt(valStr.charAt(j));
                }
            }
            else {
                total1 += val;
                total2 += val;
            }
        }
    }
    return total1 % lastDivisibleVal === 0 || total2 % lastDivisibleVal === 0;
}

/**
 * 第7位數非"7"
 * @param {*} regNo 統一編號
 * @param {*} count 統一編號數量
 * @param {*} multiplier 邏輯乘數
 * @param {*} lastDivisibleVal 最後整除值
 * @returns 
 */
const seventhIsNotSeven = (regNo, count, multiplier, lastDivisibleVal) => {
    let total = 0;
    for (let i = 0; i < count; i++) {
        let val = parseInt(regNo.charAt(i)) * multiplier[i];
        let valStr = val.toString();
        if (valStr.length > 1) {
            for (let j = 0; j < valStr.length; j++) {
                total += parseInt(valStr.charAt(j));
            }
        }
        else {
            total += val;
        }
    }

    return total % lastDivisibleVal === 0;
}

export const logOut = async () => {
    //清除BasicData
    getGlobalServerConfig().BasicData.token.set("");
    getGlobalServerConfig().BasicData.userId.set("");
    getGlobalServerConfig().BasicData.email.set("");
    getGlobalServerConfig().BasicData.orgId.set("");
    getGlobalServerConfig().BasicData.roles.set([]);
    //清除快取
    await CacheLoader().DeleteCache();
    //導頁導到登入頁面
    GetHistory().push('/');

    getGlobalServerConfig().BasicData.isCompDone.set("");
}

/**
 * 設定Menu清單
 * @param {*} functionList 
 * @returns 
 */
export const setMenu = (functionList) => {
    let count = 0;
    return functionList.map(o => {
        //設定menu第一層icon跟className
        o.className = menuIcons[count].key;
        o.imageUrl = menuIcons[count].value;
        count++;

        if (o.children?.length == 0 && o.function_url !== '')
            o.children = null;
        return { ...o };
    })
}

/**
 * 開啟新視窗
 * @param {*} url 
 * @param {*} name 
 * @param {*} title 新視窗頁籤名稱
 * @param {*} ispopup 是否為開新視窗，否則為開 tab
 * @param {*} w 開窗寬度
 * @param {*} h 開窗高度
 */
export const openPage = (url, name = "_blank", title = "", ispopup = false, w = 0, h = 0) => {
    let popupSize = "";
    if (ispopup) {
        // Fixes dual-screen position                             Most browsers      Firefox
        const dualScreenLeft = window.screenLeft !== undefined ? window.screenLeft : window.screenX;
        const dualScreenTop = window.screenTop !== undefined ? window.screenTop : window.screenY;

        const width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
        const height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;

        const systemZoom = width / window.screen.availWidth;
        const left = (width - w) / 2 / systemZoom + dualScreenLeft
        const top = (height - h) / 2 / systemZoom + dualScreenTop
        popupSize = `width=${w / systemZoom}, 
                    height=${h / systemZoom}, 
                    top=${top}, 
                    left=${left}`
    }

    const newWindow = window.open(url, name, popupSize);

    if (!IsNullOrEmpty(title)) {
        // 更換新分頁title
        newWindow.onunload = () => {
            setTimeout(() => {
                newWindow.document.title = title;
            }, 200)
        }
    }

    if (window.focus) {
        newWindow.focus();
    }
}

/**
 * 開啟章節視窗
 * @param {*} url 
 * @param {*} name
 * @param {*} title 新視窗頁籤名稱
 */
export const openChapterPage = (url, name, title, storageData) => {
    const newWindow = window.open(url, name, "");

    if (storageData != null) {
        SetStorageData(storageData, "chapterInfo", newWindow);
    }

    if (!IsNullOrEmpty(title)) {
        // 更換新分頁title
        newWindow.onunload = () => {
            setTimeout(() => {
                newWindow.document.title = title;
            }, 200)
        }
    }

    if (window.focus) {
        newWindow.focus();
    }
}

/**
 * 關閉此頁並回到來源頁
 * @param {*} window 傳入視窗
 */
export const closeAndbackToParentWindow = (window) => {
    window.opener.name = 'parentWin';
    window.open(window.opener.location.href, window.opener.name);
    window.opener.focus();
    window.close()
}