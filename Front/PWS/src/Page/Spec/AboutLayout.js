import React from 'react';

const AboutLayout = () => {
    const data = [
        { name: "index.js", description: "呼叫App.js，並將App Component的結果顯示出來" },
        { name: "App.js", description: "決定要顯示登入頁面or功能頁面，預設是登入頁面" },
        { name: "Login.js", description: "登入頁面" },
        { name: "Home.js", description: "功能頁面的框架，左邊區塊為導覽列，右邊區塊為功能顯示的畫面" },
        { name: "HomeRoute.js", description: "各個功能頁面的導頁路徑設定" },
    ]
    return (
        <div className="fnForm">
            <h4>React基本架構說明:</h4>
            <ul>
                {data.map(item =>
                    <li>
                        <h5>{item.name}</h5>
                        <i>
                            {item.description}
                        </i>
                    </li>)
                }
            </ul>
        </div >
    );
}
export default AboutLayout; 