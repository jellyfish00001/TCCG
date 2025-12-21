import React, { useState } from 'react';
import TextInput from '../../Components/Input/TextInput';

const DemoBarcode = () => {
    const [barcode, setBarcode] = useState("");
    const [barcodeValue, setbarcodeValue] = useState("");

    let onKeyDown = (e) => {
        //keyCode =13 (Enter)
        if (e.keyCode === 13) {
            setbarcodeValue(barcode)
        }
    }

    return (
        <div className="fnForm">
            <tbody>
                <table >
                    <tr>
                        <th >注意事項：</th>
                        <td>
                            <ul>
                                <li>輸入框須為英文輸入法，因為條碼通常是數字組成</li>
                                <li>掃描器會模擬鍵盤輸入，在掃完條碼讀取數字後，輸入完自動執行enter</li>
                            </ul>
                        </td>
                    </tr>
                    <tr>
                        <th >條碼機測試：</th>
                        <td>
                            <ul>
                                <li>如果手邊沒有條碼機，可以下載<a target="_blank" href="https://www.ragic.com/intl/zh-TW/blog/241/%E8%AE%93%E6%89%8B%E6%A9%9F%E8%AE%8A%E8%BA%AB%E6%A2%9D%E7%A2%BC%E6%8E%83%E7%9E%84%E5%99%A8%E7%9A%84-APP%EF%BC%9A%E3%80%8CBarcode-to-PC%E3%80%8D%E4%BB%8B%E7%B4%B9" rel="noopener noreferrer">Barcode-to-PC</a>測試</li>
                                <li>安裝設定完畢後，用手機也可以掃描條碼測試</li>
                            </ul>
                        </td>
                    </tr>
                    <tr>
                        <th >條碼輸入：</th>
                        <td>
                            <TextInput
                                onChange={(e) => setBarcode(e.target.value)}
                                value={barcode}
                                name="Barcode"
                                onKeyDown={(e) => onKeyDown(e)}
                                autoFocus  //游標停駐
                                style={{ imeMode: "disabled" }} //強制轉換為英文輸入法(chrome不支援)
                            />
                        </td>
                    </tr>
                    <tr>
                        <th >顯示條碼：</th>
                        <td>
                            {barcodeValue}
                        </td>
                    </tr>
                </table>
            </tbody>
        </div >
    );
}

export default DemoBarcode;
