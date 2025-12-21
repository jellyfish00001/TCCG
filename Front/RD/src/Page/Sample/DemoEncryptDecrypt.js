import React, { useState, useContext } from "react";
import { Input } from "@progress/kendo-react-inputs";
import { Button } from "@progress/kendo-react-buttons";
import { ServerConfig } from "../../Basic/BasicData";
import { apiFetch } from "../../Basic/ApiFetch";
import { MessageBoxContext } from "../../Components/Dialogs/MessageBox";
import {getGlobalServerConfig} from '../../Route/RootMiddleware'

const DemoEncryptDecrypt = () => {
    const { showMessage } = useContext(MessageBoxContext);

    const [data, setData] = useState("");
    const [key, setKey] = useState("");
    const [output, setOutput] = useState("");

    //驗證輸入
    const valid = () => {
        if (data.length > 0) {
            return true;
        }
        return false;
    }

    //加密
    const encrypt = (event) => {
        event.preventDefault();
        getOutput("Crypt/EnCryptAES256");
    }

    //解密
    const decrypt = (event) => {
        event.preventDefault();
        getOutput("Crypt/DeCryptAES256");
    }

    //call api to do Encrypt or Decrypt
    const getOutput = async (url) => {
        if (!valid()) return;
        try {
            let request = new Request(getGlobalServerConfig().backEndUrl.get() + url +
                '?data=' + encodeURIComponent(data) +
                '&key=' + encodeURIComponent(key));
            let response = await apiFetch(request);
            if (response.ok) {
                let responseData = await response.text();
                setOutput(responseData);
            }
            else {
                showMessage("加解密失敗,請確認金鑰是否正確");
            }
        }
        catch (e) {
            console.log("ErrorMessage: ", e);
        }
    }

    return (
        <div className="fnForm">
            <div className="expand">
                <form>
                    <table>
                        <tbody>
                            <tr>
                                <th>輸入字串</th>
                                <td>
                                    <Input style={{ width: '300px', margin: '8px 0px', textAlign: 'right' }}
                                        onChange={(event) => { setData(event.value); }}
                                    />
                                </td>
                            </tr>
                            <tr>
                                <th>金鑰</th>
                                <td>
                                    <Input style={{ width: '300px', margin: '8px 0px', textAlign: 'right' }}
                                        placeholder="預設"
                                        onChange={(event) => { setKey(event.value) }}
                                    />
                                </td>
                            </tr>
                            <tr>
                                <th>輸出字串</th>
                                <td>
                                    <Input
                                        className="k-textbox"
                                        style={{ width: '300px', margin: '8px 8px 8px 0px ' }}
                                        value={output}
                                    ></Input>
                                    <Button style={{ marginRight: '5px' }} onClick={encrypt}>加密</Button>
                                    <Button onClick={decrypt}>解密</Button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </form>
                <br />
                <div>
                    <h4>AES256加解密API</h4>
                    <pre>
                        <br />
                        <ul style={{ listStyleType: 'none' }}>
                            <li style={{ color: 'green', fontStyle: 'italic' }}>{'//加密url'}</li>
                            <li>url = getGlobalServerConfig().backEndUrl.get() + "Crypt/EnCryptAES256?data=" + data + "{'&'}key=" + key;</li>
                            <li style={{ color: 'green', fontStyle: 'italic' }}>{'//解密url'}</li>
                            <li>url = getGlobalServerConfig().backEndUrl.get() + "Crypt/DeCryptAES256?data=" + data + "{'&'}key=" + key;</li>
                        </ul>
                    </pre>
                    <h4>RSA加解密API</h4>
                    <pre>
                        <br />
                        <ul style={{ listStyleType: 'none' }}>
                            <li style={{ color: 'green', fontStyle: 'italic' }}>{'//加密url'}</li>
                            <li>url = getGlobalServerConfig().backEndUrl.get() + "Crypt/EnCryptRSA?data=" + data + "{'&'}key=" + key;</li>
                            <li style={{ color: 'green', fontStyle: 'italic' }}>{'//解密url'}</li>
                            <li>url = getGlobalServerConfig().backEndUrl.get() + "Crypt/DeCryptRSA?data=" + data + "{'&'}key=" + key;</li>
                        </ul>
                    </pre>
                    <h4>範例</h4>
                    <pre>
                        <br />
                        <ul style={{ listStyleType: 'none' }}>
                            <li style={{ color: 'green', fontStyle: 'italic' }}>{'//AES256加密'}</li>
                            <li>{`encrypt = async () => {`}</li>
                            <li>    let url = getGlobalServerConfig().backEndUrl.get() + "Crypt/EnCryptAES256?data=" + data + "{'&'}key=" + key;</li>
                            <li>    let request = new Request(url);</li>
                            <li>    let response = await apiFetch(request);</li>
                            <li>{`    if (response.ok) {`}</li>
                            <li style={{ color: 'green', fontStyle: 'italic' }}>{`        //return type: text, use response.text()`}</li>
                            <li>{`        return = await response.text();`}</li>
                            <li>{`    }`}</li>
                            <li>{`}`}</li>
                        </ul>
                    </pre>
                </div>
            </div>
        </div >
    );
}
export default DemoEncryptDecrypt;