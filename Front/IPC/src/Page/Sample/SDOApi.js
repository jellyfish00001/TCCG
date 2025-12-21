import React from 'react';

const SDOApi = () => {
    return (
        <div>
            <h3>AsposeSet - Office(Aspose.net)產製模組</h3>
            Bin目錄需同時置放以下檔案
            <ul>
                <li>Aspose.Cells.dll</li>
                <li>Aspose.Words.dll</li>
                <li>Aspose.Total.655.lic</li>
            </ul>
            <h4>(轉檔word格式) AsposeSet.Word().ConvertType(inputStream, toFormat)</h4>
            <pre>
                <i>
                    {`
    inputStream => 待轉換 word file stream
    toFormat => {docx|doc|odt|pdf|html}
    return 轉換後byte[]
`}
                </i>
            </pre>
            <h4>(從html生成word) AsposeSet.Word().GenByHtml(htmlSource)</h4>
            <pre>
                <i>
                    {`
    htmlSource => 待轉換 html source
    return 轉換後byte[]
`}
                </i>
            </pre>
            <h4>(轉檔Excel格式) AsposeSet.Eexcel().ConvertType(inputStream, fromFormat, toFormat)</h4>
            <pre>
                <i>
                    {`
    inputStream => 待轉換 excel file stream
    fromFormat => {xlsx|xls|csv|ods|pdf|html}
    toFormat => {xlsx|xls|csv|ods|pdf|html}
    return 轉換後byte[]
`}
                </i>
            </pre>
            <h4>(匯入excel成動態資料) AsposeSet.Eexcel().Import(inputStream, format, sheetIndex = 0)</h4>
            <pre>
                <i>
                    {`
    inputStream => 待匯入 excel file stream
    format => {xlsx|xls|csv|ods|pdf|html}
    sheetIndex => 待分析 excel sheet
    return list<dynamic> excel資料
`}
                </i>
            </pre>
            <h4>(匯出資料成Xslx格式) AsposeSet.Eexcel().Import(items, sheetName = "匯出資料", headers = null, toFormat = FileFormat.xlsx)</h4>
            <pre>
                <i>
                    {`
    items => 待匯出資料列表
    sheetName => 匯出 excel sheet name
    headers => 匯出 excel column header titles
    toFormat => {xlsx|xls|csv|ods|pdf|html}
    return 轉換後byte[]
`}
                </i>
            </pre>
            <h3>CryptSet - 字串加解密模組</h3>
            <h4>(AES256加密) CryptSet.Encrypt(strSource).AES256()</h4>
            <pre>
                <i>
                    {`
    strSource => 待加密字串
    return 加密後字串 (AES256)
`}
                </i>
            </pre>
            <h4>(AES256解密) CryptSet.Decrypt(strSource).AES256()</h4>
            <pre>
                <i>
                    {`
    strSource => 待解密字串
    return 解密後字串 (AES256)
`}
                </i>
            </pre>
            <h4>(SHA512加密) CryptSet.Encrypt(strSource).SHA512()</h4>
            <pre>
                <i>
                    {`
    strSource => 待加密字串
    return 加密後字串 (SHA512)
`}
                </i>
            </pre>
            <h3>MailSet - 郵件寄送模組</h3>
            (系統參數) MailConfig<br />
            <ul>
                <li>MailServer ={'>'} 郵件伺服器</li>
                <li>SenderMail ={'>'} 寄件者電子郵件</li>
                <li>SenderTitle ={'>'} 寄件者稱呼</li>
            </ul>
            <h4>(建立郵件範本) MailSet.Mail().Generate(strMailId, objContent)</h4>
            <pre>
                <i>
                    {`
    strMailId => 郵件範本代碼
    objContent => 對應變數資料
    return {MailMessage} 
`}
                </i>
            </pre>
            <h4>(寄送郵件) MailSet.Mail().Send(objMail)</h4>
            <pre>
                <i>
                    {`
    objMail => 待寄送MailMessage
`}
                </i>
            </pre>
            <h3>MaskSet - 字串遮罩模組</h3>
            <h4>MaskSet.Mask(strSource).Set(startIdx, maskLength, mosaic = '*')</h4>
            <pre>
                <i>
                    {`
    strSource => 待遮罩字串
    startIdx => 遮罩起始位置
    maskLength => 遮罩長度
    mosaic => 遮罩替換字元
    return 遮罩後字串
`}
                </i>
            </pre>
            <h4>(身分證字串格式遮罩) MaskSet.Mask(strSource).MaskId()</h4>
            <pre>
                <i>
                    {`
    strSource => 待遮罩字串
    return 遮罩後字串
`}
                </i>
            </pre>
            <h4>(電話字串格式遮罩) MaskSet.Mask(strSource).MaskTel()</h4>
            <pre>
                <i>
                    {`
    strSource => 待遮罩字串
    return 遮罩後字串
`}
                </i>
            </pre>
            <h4>(姓名字串格式遮罩) MaskSet.Mask(strSource).MaskName()</h4>
            <pre>
                <i>
                    {`
    strSource => 待遮罩之字串
    return 遮罩後字串
`}
                </i>
            </pre>
        </div>
    );
}

export default SDOApi;