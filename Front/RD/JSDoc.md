# JSDOC 使用範例:

**使用環境:Visual Studio Code** <br/>
**推薦安裝組件:Complete JSDoc Tags**
**參考文件 https://devdocs.io/jsdoc/**

## function範例


```javascript
/**
 * @function showGlobalMessageBox
 * @param {string} message 訊息內容
 * @param {requestCallback} [callback] 按下確認後回呼的事件
 * @description 利用globalState顯示全域訊息框
 * @example
 * <caption>使用方式1:</caption>
 * let message="訊息";
 * showGlobalMessageBox(message)
 * @example
 * <caption>使用方式2:</caption>
 * let message="訊息";
 * let callback=()=>{};
 * showGlobalMessageBox(message)
 * @see globalState 參閱globalState
 * @author Peter.Pang <peter_pang@gss.com.tw>
 */
export const showGlobalMessageBox=(message,callback)=>{
    globalState.globalMessageBoxSettings.visible.set(true);
    globalState.globalMessageBoxSettings.message.set(message);
    if(typeof(callback)=="function")
        globalState.globalMessageBoxSettings.onOkAction.set(() => callback);
}
```


## Object範例


```javascript
/**
 * @type {{
 *          token:string,
 *          userId:string,
 *          orgId:string,
 *          email:string
 *       }}
 */
const BasicData = {
    token: '',
    userId: '',
    orgId: '',
    email: ''
}
```