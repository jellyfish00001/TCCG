
import React from 'react';
import { Editor, EditorTools, EditorUtils ,ProseMirror} from '@progress/kendo-react-editor';
import { Error } from '@progress/kendo-react-labels';

//editor tools 可依專案自行調整
//props若有傳入自訂的tools，會以自訂的為主
const {
    Bold, Italic, Underline, Strikethrough, Subscript, Superscript,
    ForeColor, BackColor,
    AlignLeft, AlignCenter, AlignRight, AlignJustify,
    Indent, Outdent, OrderedList, UnorderedList,
    Undo, Redo, FontSize, FontName, FormatBlock,
    Link, Unlink, InsertImage, ViewHtml,
    InsertTable,
    AddRowBefore, AddRowAfter, AddColumnBefore, AddColumnAfter,
    DeleteRow, DeleteColumn, DeleteTable,
    MergeCells, SplitCell
} = EditorTools;


const TextEditor = (props) => {

    //從EditorUtils導入要用的function。
    const {
        pasteCleanup,   //清理貼上的功能
        removeAttribute //移除HTML屬性
    } = EditorUtils;

    // 設定貼上時的清除規則
    const pasteSettings = {
        attributes: {
            "*": removeAttribute,
        },
    };

    /**
     * @param {*} e        傳入的事件
     * @param {*} isClear 判斷是否貼上時要加清除規則，預設為false
     * @returns isClear為True，回傳清除完的結果
     * @example
     * <caption>使用方式：</caption>
     * <TextEditor
     *     name="COMMENT"
     *     value={EmailData.mail_content}
     *     onChange={(e) => setEmailData({ ...EmailData, mail_content: e.html })}
     *     error={errors["mail_content"]}
     *     tools={[]}
     *     clearHtml={true} //true，貼上時會清除HTML標籤
     *  />
     */
    const onPasteHtmlChange = (e, isClear=false) => {
        if (isClear) {
            let html = pasteCleanup(e.pastedHtml, pasteSettings);
            return html;
        }
    }


    return (
        <>
            <Editor
                tools={[
                    [Bold, Italic, Underline, Strikethrough],
                    [Subscript, Superscript],
                    ForeColor, BackColor,
                    [AlignLeft, AlignCenter, AlignRight, AlignJustify],
                    [Indent, Outdent],
                    [OrderedList, UnorderedList],
                    FontSize, FontName, FormatBlock,
                    [Undo, Redo],
                    [Link, Unlink, InsertImage, ViewHtml],
                    [InsertTable],
                    [AddRowBefore, AddRowAfter, AddColumnBefore, AddColumnAfter],
                    [DeleteRow, DeleteColumn, DeleteTable],
                    [MergeCells, SplitCell]
                ]}
                {...props}
                onPasteHtml = {(e) => onPasteHtmlChange(e, props.clearHtml)}
            />
            {<Error>{props.error}</Error>}
        </>
    )
}

export default TextEditor