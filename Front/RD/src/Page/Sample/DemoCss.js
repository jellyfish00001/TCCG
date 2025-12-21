import React, { useState } from 'react';
import { Button } from '@progress/kendo-react-buttons';
import { Slide } from '@progress/kendo-react-animation';
const DemoCss = () => {
    const [slideVisible, setSlideVisible] = useState(true);
    const slideChangeHandler = () => {
        setSlideVisible(!slideVisible);
    }
    let strong = {
        lineHeight: '40px',
        fontSize: '24px',
        fontWeight: 'bold',
        color: '#0b77c3',
    }
    return (
        <div className="fnForm">
            <div className="expand">
                <div>
                    <Button icon="menu" onClick={slideChangeHandler} style={{ margin: "0 10px", color: "black", background: "none", border: "none" }}></Button>
                </div>
                <div>
                    <Slide>
                        {
                            slideVisible &&
                            <div>
                                <ul>
                                    <li>\Content\Site.css</li>
                                    <li>\Content\SDObasic\SDObasic-blue.css</li>
                                    <li>\Content\SDObasic\SDObasic-green.css</li>
                                    <li>\Content\SDObasic\SDObasic-purple.css</li>
                                </ul>
                            </div>
                        }
                    </Slide>
                </div>
                <div>
                    <div style={strong}>標題</div>
                    <div>
                        <h1>h1:36px</h1>
                        <h2>h2:30px</h2>
                        <h3>h3:24px</h3>
                        <h4>h4:18px</h4>
                        <h5>h5:14px</h5>
                        <h6>h6:12px</h6>
                        <pre>
                            {"<h1>h1:36px</h1>"}
                        </pre>
                    </div>
                </div>
                <div>
                    <div style={strong}>清單</div>
                    <div>
                        <ul>
                            <li>無序清單：項目1</li>
                            <li>無序清單：項目2</li>
                            <li>無序清單：項目3</li>
                        </ul>
                        <pre>{'<ul>'} <br />{'<li>...</li>'} <br />{'</ul>'} </pre>
                        <ol>
                            <li>有序清單：項目1</li>
                            <li>有序清單：項目2</li>
                            <li>有序清單：項目3</li>
                        </ol>
                        <pre>{'<ol>'} <br />{'<li>...</li>'} <br />{'</ol>'} </pre>
                        <ul className="list-inline">
                            <li>橫式清單項目1</li>
                            <li>橫式清單項目2</li>
                            <li>橫式清單項目3</li>
                        </ul>
                        <pre>{'<ul className="list-inline">'} <br />{'<li>...</li>'} <br />{'</ul>'} </pre>
                    </div>
                </div>
                <div>
                    <div style={strong}>Checkbox和Radio</div>
                    <h4>橫式排列</h4>
                    <div>
                        <form>
                            <label className="checkbox-inline">
                                <input type="checkbox" id="inlineCheckbox1" value="option1" /> 1
                                </label>
                            <label className="checkbox-inline">
                                <input type="checkbox" id="inlineCheckbox2" value="option2" /> 2
                                </label>
                            <label className="checkbox-inline">
                                <input type="checkbox" id="inlineCheckbox3" value="option3" /> 3
                                </label>
                        </form>
                        <br />
                        <form>
                            <label className="radio-inline">
                                <input type="radio" name="inlineRadioOptions" id="inlineRadio1" value="option1" /> 1
                                </label>
                            <label className="radio-inline">
                                <input type="radio" name="inlineRadioOptions" id="inlineRadio2" value="option2" /> 2
                                </label>
                            <label className="radio-inline">
                                <input type="radio" name="inlineRadioOptions" id="inlineRadio3" value="option3" /> 3
                                </label>
                        </form>
                    </div>
                    <pre>
                        {`<label className="checkbox-inline"><input type="checkbox" id="inlineCheckbox1" value="option1">1</label>
<label className="radio-inline"><input type="radio" id="inlineRadio1" value="option1">1</label>`}
                    </pre>
                </div>
                <div>
                    <div style={strong}>按鈕</div>
                    <div>
                        <button>系統預設</button>&nbsp;
                            <button className="btn-default">Default</button>&nbsp;
                            <button className="btn-primary">Primary</button>&nbsp;
                            <button className="btn-success">Success</button>&nbsp;
                            <button className="btn-info">Info</button>&nbsp;
                            <button className="btn-warning">Warning</button>&nbsp;
                            <button className="btn-danger">Danger</button>&nbsp;
                            <button className="btn-link">Link</button>
                    </div>
                    <pre>
                        {`<button>系統預設</button>
<button className="btn-default">Default</button>
<button className="btn-primary">Primary</button>
<button className="btn-success">Success</button>
<button className="btn-info">Info</button>
<button className="btn-warning">Warning</button>
<button className="btn-danger">Danger</button>
<button className="btn-link">Link</button>`}
                    </pre>
                </div>
                <div>
                    <div style={strong}>Web Font Icon</div>
                    <div>
                        有400種圖示，將class內的 k-i-copy 改成想要的圖示名稱即可<br />
                        <span className="k-icon k-i-copy"></span>
                    </div>
                    <pre>
                        {`<span className="k-icon k-i-copy"></span>`}
                    </pre>
                    <div>
                        要改背景顏色，只需將class內的 btn-success 換成要的按鈕顏色即可<br />
                        <a className="btn-success" href="#/Home/Sample/Demo/SampleCSS"><span className="k-icon k-i-copy"></span></a>
                    </div>
                    <pre>
                        {`<a className="btn-success" href="#/Home/Sample/Demo/SampleCSS"><span className="k-icon k-i-copy"></span></a>`}
                    </pre>
                </div>
                <div style={{ margin: '15px' }}>
                    <div className="Action-icon">
                        <h4>Actions</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-undo" title="k-icon k-i-undo"></span>
                            <span className="k-icon k-i-redo" title="k-icon k-i-redo"></span>
                            <span className="k-icon k-i-reset" title="k-icon k-i-reset"></span>
                            <span className="k-icon k-i-reload" title="k-icon k-i-reload"></span>
                            <span className="k-icon k-i-non-recurrence" title="k-icon k-i-non-recurrence"></span>
                            <span className="k-icon k-i-reset-sm" title="k-icon k-i-reset-sm"></span>
                            <span className="k-icon k-i-reload-sm" title="k-icon k-i-reload-sm"></span>
                            <span className="k-icon k-i-clock" title="k-icon k-i-clock"></span>
                            <span className="k-icon k-i-calendar" title="k-icon k-i-calendar"></span>
                            <span className="k-icon k-i-save" title="k-icon k-i-save"></span>
                            <span className="k-icon k-i-print" title="k-icon k-i-print"></span>
                            <span className="k-icon k-i-edit" title="k-icon k-i-edit"></span>
                            <span className="k-icon k-i-delete" title="k-icon k-i-delete"></span>
                            <span className="k-icon k-i-attachment" title="k-icon k-i-attachment"></span>
                            <span className="k-icon k-i-attachment-45" title="k-icon k-i-attachment-45"></span>
                            <span className="k-icon k-i-link-horizontal" title="k-icon k-i-link-horizontal"></span>
                            <span className="k-icon k-i-unlink-horizontal" title="k-icon k-i-unlink-horizontal"></span>
                            <span className="k-icon k-i-link-vertical" title="k-icon k-i-link-vertical"></span>
                            <span className="k-icon k-i-unlink-vertical" title="k-icon k-i-unlink-vertical"></span>
                            <span className="k-icon k-i-lock" title="k-icon k-i-lock"></span>
                            <span className="k-icon k-i-unlock" title="k-icon k-i-unlock"></span>
                            <span className="k-icon k-i-cancel" title="k-icon k-i-cancel"></span>
                            <span className="k-icon k-i-cancel-outline" title="k-icon k-i-cancel-outline"></span>
                            <span className="k-icon k-i-cancel-circle" title="k-icon k-i-cancel-circle"></span>
                            <span className="k-icon k-i-check" title="k-icon k-i-check"></span>
                            <span className="k-icon k-i-check-outline" title="k-icon k-i-check-outline"></span>
                            <span className="k-icon k-i-check-circle" title="k-icon k-i-check-circle"></span>
                            <span className="k-icon k-i-close" title="k-icon k-i-close"></span>
                            <span className="k-icon k-i-close-outline" title="k-icon k-i-close-outline"></span>
                            <span className="k-icon k-i-close-circle" title="k-icon k-i-close-circle"></span>
                            <span className="k-icon k-i-plus" title="k-icon k-i-plus"></span>
                            <span className="k-icon k-i-plus-outline" title="k-icon k-i-plus-outline"></span>
                            <span className="k-icon k-i-plus-circle" title="k-icon k-i-plus-circle"></span>
                            <span className="k-icon k-i-minus" title="k-icon k-i-minus"></span>
                            <span className="k-icon k-i-minus-outline" title="k-icon k-i-minus-outline"></span>
                            <span className="k-icon k-i-minus-circle" title="k-icon k-i-minus-circle"></span>
                            <span className="k-icon k-i-sort-asc" title="k-icon k-i-sort-asc"></span>
                            <span className="k-icon k-i-sort-desc" title="k-icon k-i-sort-desc"></span>
                            <span className="k-icon k-i-unsort" title="k-icon k-i-unsort"></span>
                            <span className="k-icon k-i-sort-clear" title="k-icon k-i-sort-clear"></span>
                            <span className="k-icon k-i-sort-asc-sm" title="k-icon k-i-sort-asc-sm"></span>
                            <span className="k-icon k-i-sort-desc-sm" title="k-icon k-i-sort-desc-sm"></span>
                            <span className="k-icon k-i-filter" title="k-icon k-i-filter"></span>
                            <span className="k-icon k-i-filter-clear" title="k-icon k-i-filter-clear"></span>
                            <span className="k-icon k-i-filter-sm" title="k-icon k-i-filter-sm"></span>
                            <span className="k-icon k-i-filter-sort-asc-sm" title="k-icon k-i-filter-sort-asc-sm"></span>
                            <span className="k-icon k-i-filter-sort-desc-sm" title="k-icon k-i-filter-sort-desc-sm"></span>
                            <span className="k-icon k-i-filter-add-expression" title="k-icon k-i-filter-add-expression"></span>
                            <span className="k-icon k-i-filter-add-group" title="k-icon k-i-filter-add-group"></span>
                            <span className="k-icon k-i-login" title="k-icon k-i-login"></span>
                            <span className="k-icon k-i-logout" title="k-icon k-i-logout"></span>
                            <span className="k-icon k-i-download" title="k-icon k-i-download"></span>
                            <span className="k-icon k-i-upload" title="k-icon k-i-upload"></span>
                            <span className="k-icon k-i-hyperlink-open" title="k-icon k-i-hyperlink-open"></span>
                            <span className="k-icon k-i-hyperlink-open-sm" title="k-icon k-i-hyperlink-open-sm"></span>
                            <span className="k-icon k-i-launch" title="k-icon k-i-launch"></span>
                            <span className="k-icon k-i-window" title="k-icon k-i-window"></span>
                            <span className="k-icon k-i-windows" title="k-icon k-i-windows"></span>
                            <span className="k-icon k-i-window-minimize" title="k-icon k-i-window-minimize"></span>
                            <span className="k-icon k-i-gear" title="k-icon k-i-gear"></span>
                            <span className="k-icon k-i-gears" title="k-icon k-i-gears"></span>
                            <span className="k-icon k-i-wrench" title="k-icon k-i-wrench"></span>
                            <span className="k-icon k-i-preview" title="k-icon k-i-preview"></span>
                            <span className="k-icon k-i-zoom" title="k-icon k-i-zoom"></span>
                            <span className="k-icon k-i-zoom-in" title="k-icon k-i-zoom-in"></span>
                            <span className="k-icon k-i-zoom-out" title="k-icon k-i-zoom-out"></span>
                            <span className="k-icon k-i-pan" title="k-icon k-i-pan"></span>
                            <span className="k-icon k-i-calculator" title="k-icon k-i-calculator"></span>
                            <span className="k-icon k-i-cart" title="k-icon k-i-cart"></span>
                            <span className="k-icon k-i-connector" title="k-icon k-i-connector"></span>
                            <span className="k-icon k-i-plus-sm" title="k-icon k-i-plus-sm"></span>
                            <span className="k-icon k-i-minus-sm" title="k-icon k-i-minus-sm"></span>
                        </div>
                    </div>
                    <div className="Alerts-and-Notifications-icon">
                        <h4>Alerts and Notifications</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-notification" title="k-icon k-i-notification"></span>
                            <span className="k-icon k-i-information" title="k-icon k-i-information"></span>
                            <span className="k-icon k-i-question" title="k-icon k-i-question"></span>
                            <span className="k-icon k-i-warning" title="k-icon k-i-warning"></span>
                        </div>
                    </div>
                    <div className="Editing-icon">
                        <h4>Editing</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-page-properties" title="k-icon k-i-page-properties"></span>
                            <span className="k-icon k-i-bold" title="k-icon k-i-bold"></span>
                            <span className="k-icon k-i-italic" title="k-icon k-i-italic"></span>
                            <span className="k-icon k-i-underline" title="k-icon k-i-underline"></span>
                            <span className="k-icon k-i-font-family" title="k-icon k-i-font-family"></span>
                            <span className="k-icon k-i-foreground-color" title="k-icon k-i-foreground-color"></span>
                            <span className="k-icon k-i-convert-lowercase" title="k-icon k-i-convert-lowercase"></span>
                            <span className="k-icon k-i-convert-uppercase" title="k-icon k-i-convert-uppercase"></span>
                            <span className="k-icon k-i-strikethrough" title="k-icon k-i-strikethrough"></span>
                            <span className="k-icon k-i-sub-script" title="k-icon k-i-sub-script"></span>
                            <span className="k-icon k-i-sup-script" title="k-icon k-i-sup-script"></span>
                            <span className="k-icon k-i-div" title="k-icon k-i-div"></span>
                            <span className="k-icon k-i-all" title="k-icon k-i-all"></span>
                            <span className="k-icon k-i-h1" title="k-icon k-i-h1"></span>
                            <span className="k-icon k-i-h2" title="k-icon k-i-h2"></span>
                            <span className="k-icon k-i-h3" title="k-icon k-i-h3"></span>
                            <span className="k-icon k-i-h4" title="k-icon k-i-h4"></span>
                            <span className="k-icon k-i-h5" title="k-icon k-i-h5"></span>
                            <span className="k-icon k-i-h6" title="k-icon k-i-h6"></span>
                            <span className="k-icon k-i-list-ordered" title="k-icon k-i-list-ordered"></span>
                            <span className="k-icon k-i-list-unordered" title="k-icon k-i-list-unordered"></span>
                            <span className="k-icon k-i-indent-increase" title="k-icon k-i-indent-increase"></span>
                            <span className="k-icon k-i-indent-decrease" title="k-icon k-i-indent-decrease"></span>
                            <span className="k-icon k-i-insert-up" title="k-icon k-i-insert-up"></span>
                            <span className="k-icon k-i-insert-middle" title="k-icon k-i-insert-middle"></span>
                            <span className="k-icon k-i-insert-down" title="k-icon k-i-insert-down"></span>
                            <span className="k-icon k-i-align-top" title="k-icon k-i-align-top"></span>
                            <span className="k-icon k-i-align-middle" title="k-icon k-i-align-middle"></span>
                            <span className="k-icon k-i-align-bottom" title="k-icon k-i-align-bottom"></span>
                            <span className="k-icon k-i-align-left" title="k-icon k-i-align-left"></span>
                            <span className="k-icon k-i-align-center" title="k-icon k-i-align-center"></span>
                            <span className="k-icon k-i-align-right" title="k-icon k-i-align-right"></span>
                            <span className="k-icon k-i-align-justify" title="k-icon k-i-align-justify"></span>
                            <span className="k-icon k-i-align-remove" title="k-icon k-i-align-remove"></span>
                            <span className="k-icon k-i-text-wrap" title="k-icon k-i-text-wrap"></span>
                            <span className="k-icon k-i-rule-horizontal" title="k-icon k-i-rule-horizontal"></span>
                            <span className="k-icon k-i-table-align-top-left" title="k-icon k-i-table-align-top-left"></span>
                            <span className="k-icon k-i-table-align-top-center" title="k-icon k-i-table-align-top-center"></span>
                            <span className="k-icon k-i-table-align-top-right" title="k-icon k-i-table-align-top-right"></span>
                            <span className="k-icon k-i-table-align-middle-left" title="k-icon k-i-table-align-middle-left"></span>
                            <span className="k-icon k-i-table-align-middle-center" title="k-icon k-i-table-align-middle-center"></span>
                            <span className="k-icon k-i-table-align-middle-right" title="k-icon k-i-table-align-middle-right"></span>
                            <span className="k-icon k-i-table-align-bottom-left" title="k-icon k-i-table-align-bottom-left"></span>
                            <span className="k-icon k-i-table-align-bottom-center" title="k-icon k-i-table-align-bottom-center"></span>
                            <span className="k-icon k-i-table-align-bottom-right" title="k-icon k-i-table-align-bottom-right"></span>
                            <span className="k-icon k-i-table-align-remove" title="k-icon k-i-table-align-remove"></span>
                            <span className="k-icon k-i-borders-all" title="k-icon k-i-borders-all"></span>
                            <span className="k-icon k-i-borders-outside" title="k-icon k-i-borders-outside"></span>
                            <span className="k-icon k-i-borders-inside" title="k-icon k-i-borders-inside"></span>
                            <span className="k-icon k-i-borders-inside-horizontal" title="k-icon k-i-borders-inside-horizontal"></span>
                            <span className="k-icon k-i-borders-inside-vertical" title="k-icon k-i-borders-inside-vertical"></span>
                            <span className="k-icon k-i-border-top" title="k-icon k-i-border-top"></span>
                            <span className="k-icon k-i-border-bottom" title="k-icon k-i-border-bottom"></span>
                            <span className="k-icon k-i-border-left" title="k-icon k-i-border-left"></span>
                            <span className="k-icon k-i-border-right" title="k-icon k-i-border-right"></span>
                            <span className="k-icon k-i-border-no" title="k-icon k-i-border-no"></span>
                            <span className="k-icon k-i-borders-show-hide" title="k-icon k-i-borders-show-hide"></span>
                            <span className="k-icon k-i-form" title="k-icon k-i-form"></span>
                            <span className="k-icon k-i-form-element" title="k-icon k-i-form-element"></span>
                            <span className="k-icon k-i-code-snippet" title="k-icon k-i-code-snippet"></span>
                            <span className="k-icon k-i-select-all" title="k-icon k-i-select-all"></span>
                            <span className="k-icon k-i-button" title="k-icon k-i-button"></span>
                            <span className="k-icon k-i-select-box" title="k-icon k-i-select-box"></span>
                            <span className="k-icon k-i-calendar-date" title="k-icon k-i-calendar-date"></span>
                            <span className="k-icon k-i-group-box" title="k-icon k-i-group-box"></span>
                            <span className="k-icon k-i-textarea" title="k-icon k-i-textarea"></span>
                            <span className="k-icon k-i-textbox" title="k-icon k-i-textbox"></span>
                            <span className="k-icon k-i-textbox-hidden" title="k-icon k-i-textbox-hidden"></span>
                            <span className="k-icon k-i-password" title="k-icon k-i-password"></span>
                            <span className="k-icon k-i-paragraph-add" title="k-icon k-i-paragraph-add"></span>
                            <span className="k-icon k-i-edit-tools" title="k-icon k-i-edit-tools"></span>
                            <span className="k-icon k-i-template-manager" title="k-icon k-i-template-manager"></span>
                            <span className="k-icon k-i-change-manually" title="k-icon k-i-change-manually"></span>
                            <span className="k-icon k-i-track-changes" title="k-icon k-i-track-changes"></span>
                            <span className="k-icon k-i-track-changes-enable" title="k-icon k-i-track-changes-enable"></span>
                            <span className="k-icon k-i-track-changes-accept" title="k-icon k-i-track-changes-accept"></span>
                            <span className="k-icon k-i-track-changes-accept-all" title="k-icon k-i-track-changes-accept-all"></span>
                            <span className="k-icon k-i-track-changes-reject" title="k-icon k-i-track-changes-reject"></span>
                            <span className="k-icon k-i-track-changes-reject-all" title="k-icon k-i-track-changes-reject-all"></span>
                            <span className="k-icon k-i-document-manager" title="k-icon k-i-document-manager"></span>
                            <span className="k-icon k-i-custom-icon" title="k-icon k-i-custom-icon"></span>
                            <span className="k-icon k-i-dictionary-add" title="k-icon k-i-dictionary-add"></span>
                            <span className="k-icon k-i-image-light-dialog" title="k-icon k-i-image-light-dialog"></span>
                            <span className="k-icon k-i-image-edit" title="k-icon k-i-image-edit"></span>
                            <span className="k-icon k-i-image-map-editor" title="k-icon k-i-image-map-editor"></span>
                            <span className="k-icon k-i-comment" title="k-icon k-i-comment"></span>
                            <span className="k-icon k-i-comment-remove" title="k-icon k-i-comment-remove"></span>
                            <span className="k-icon k-i-comments-remove-all" title="k-icon k-i-comments-remove-all"></span>
                            <span className="k-icon k-i-silverlight" title="k-icon k-i-silverlight"></span>
                            <span className="k-icon k-i-media-manager" title="k-icon k-i-media-manager"></span>
                            <span className="k-icon k-i-video-external" title="k-icon k-i-video-external"></span>
                            <span className="k-icon k-i-flash-manager" title="k-icon k-i-flash-manager"></span>
                            <span className="k-icon k-i-find-and-replace" title="k-icon k-i-find-and-replace"></span>
                            <span className="k-icon k-i-copy" title="k-icon k-i-copy"></span>
                            <span className="k-icon k-i-cut" title="k-icon k-i-cut"></span>
                            <span className="k-icon k-i-paste" title="k-icon k-i-paste"></span>
                            <span className="k-icon k-i-paste-as-html" title="k-icon k-i-paste-as-html"></span>
                            <span className="k-icon k-i-paste-from-word" title="k-icon k-i-paste-from-word"></span>
                            <span className="k-icon k-i-paste-from-word-strip-file" title="k-icon k-i-paste-from-word-strip-file"></span>
                            <span className="k-icon k-i-paste-html" title="k-icon k-i-paste-html"></span>
                            <span className="k-icon k-i-paste-markdown" title="k-icon k-i-paste-markdown"></span>
                            <span className="k-icon k-i-paste-plain-text" title="k-icon k-i-paste-plain-text"></span>
                            <span className="k-icon k-i-apply-format" title="k-icon k-i-apply-format"></span>
                            <span className="k-icon k-i-clear-css" title="k-icon k-i-clear-css"></span>
                            <span className="k-icon k-i-copy-format" title="k-icon k-i-copy-format"></span>
                            <span className="k-icon k-i-strip-all-formating" title="k-icon k-i-strip-all-formating"></span>
                            <span className="k-icon k-i-strip-css-format" title="k-icon k-i-strip-css-format"></span>
                            <span className="k-icon k-i-strip-font-elements" title="k-icon k-i-strip-font-elements"></span>
                            <span className="k-icon k-i-strip-span-elements" title="k-icon k-i-strip-span-elements"></span>
                            <span className="k-icon k-i-strip-word-formatting" title="k-icon k-i-strip-word-formatting"></span>
                            <span className="k-icon k-i-format-code-block" title="k-icon k-i-format-code-block"></span>
                            <span className="k-icon k-i-style-builder" title="k-icon k-i-style-builder"></span>
                            <span className="k-icon k-i-module-manager" title="k-icon k-i-module-manager"></span>
                            <span className="k-icon k-i-hyperlink-light-dialog" title="k-icon k-i-hyperlink-light-dialog"></span>
                            <span className="k-icon k-i-hyperlink-globe" title="k-icon k-i-hyperlink-globe"></span>
                            <span className="k-icon k-i-hyperlink-globe-remove" title="k-icon k-i-hyperlink-globe-remove"></span>
                            <span className="k-icon k-i-hyperlink-email" title="k-icon k-i-hyperlink-email"></span>
                            <span className="k-icon k-i-anchor" title="k-icon k-i-anchor"></span>
                            <span className="k-icon k-i-table-light-dialog" title="k-icon k-i-table-light-dialog"></span>
                            <span className="k-icon k-i-table" title="k-icon k-i-table"></span>
                            <span className="k-icon k-i-table-properties" title="k-icon k-i-table-properties"></span>
                            <span className="k-icon k-i-table-cell" title="k-icon k-i-table-cell"></span>
                            <span className="k-icon k-i-table-cell-properties" title="k-icon k-i-table-cell-properties"></span>
                            <span className="k-icon k-i-table-column-insert-left" title="k-icon k-i-table-column-insert-left"></span>
                            <span className="k-icon k-i-table-column-insert-right" title="k-icon k-i-table-column-insert-right"></span>
                            <span className="k-icon k-i-table-row-insert-above" title="k-icon k-i-table-row-insert-above"></span>
                            <span className="k-icon k-i-table-row-insert-below" title="k-icon k-i-table-row-insert-below"></span>
                            <span className="k-icon k-i-table-column-delete" title="k-icon k-i-table-column-delete"></span>
                            <span className="k-icon k-i-table-row-delete" title="k-icon k-i-table-row-delete"></span>
                            <span className="k-icon k-i-table-cell-delete" title="k-icon k-i-table-cell-delete"></span>
                            <span className="k-icon k-i-table-delete" title="k-icon k-i-table-delete"></span>
                            <span className="k-icon k-i-cells-merge" title="k-icon k-i-cells-merge"></span>
                            <span className="k-icon k-i-cells-merge-horizontally" title="k-icon k-i-cells-merge-horizontally"></span>
                            <span className="k-icon k-i-cells-merge-vertically" title="k-icon k-i-cells-merge-vertically"></span>
                            <span className="k-icon k-i-cell-split-horizontally" title="k-icon k-i-cell-split-horizontally"></span>
                            <span className="k-icon k-i-cell-split-vertically" title="k-icon k-i-cell-split-vertically"></span>
                            <span className="k-icon k-i-pane-freeze" title="k-icon k-i-pane-freeze"></span>
                            <span className="k-icon k-i-row-freeze" title="k-icon k-i-row-freeze"></span>
                            <span className="k-icon k-i-column-freeze" title="k-icon k-i-column-freeze"></span>
                            <span className="k-icon k-i-toolbar-float" title="k-icon k-i-toolbar-float"></span>
                            <span className="k-icon k-i-spell-checker" title="k-icon k-i-spell-checker"></span>
                            <span className="k-icon k-i-validation-xhtml" title="k-icon k-i-validation-xhtml"></span>
                            <span className="k-icon k-i-validation-data" title="k-icon k-i-validation-data"></span>
                            <span className="k-icon k-i-toggle-full-screen-mode" title="k-icon k-i-toggle-full-screen-mode"></span>
                            <span className="k-icon k-i-formula-fx" title="k-icon k-i-formula-fx"></span>
                            <span className="k-icon k-i-sum" title="k-icon k-i-sum"></span>
                            <span className="k-icon k-i-symbol" title="k-icon k-i-symbol"></span>
                            <span className="k-icon k-i-dollar" title="k-icon k-i-dollar"></span>
                            <span className="k-icon k-i-percent" title="k-icon k-i-percent"></span>
                            <span className="k-icon k-i-custom-format" title="k-icon k-i-custom-format"></span>
                            <span className="k-icon k-i-decimal-increase" title="k-icon k-i-decimal-increase"></span>
                            <span className="k-icon k-i-decimal-decrease" title="k-icon k-i-decimal-decrease"></span>
                            <span className="k-icon k-i-font-size" title="k-icon k-i-font-size"></span>
                            <span className="k-icon k-i-image-absolute-position" title="k-icon k-i-image-absolute-position"></span>
                        </div>
                    </div>
                    <div className="Files-and-Folders-icon">
                        <h4>Files and Folders</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-folder" title="k-icon k-i-folder"></span>
                            <span className="k-icon k-i-folder-open" title="k-icon k-i-folder-open"></span>
                            <span className="k-icon k-i-folder-add" title="k-icon k-i-folder-add"></span>
                            <span className="k-icon k-i-folder-up" title="k-icon k-i-folder-up"></span>
                            <span className="k-icon k-i-folder-more" title="k-icon k-i-folder-more"></span>
                            <span className="k-icon k-i-aggregate-fields" title="k-icon k-i-aggregate-fields"></span>
                            <span className="k-icon k-i-file" title="k-icon k-i-file"></span>
                            <span className="k-icon k-i-file-add" title="k-icon k-i-file-add"></span>
                            <span className="k-icon k-i-file-txt" title="k-icon k-i-file-txt"></span>
                            <span className="k-icon k-i-file-csv" title="k-icon k-i-file-csv"></span>
                            <span className="k-icon k-i-file-excel" title="k-icon k-i-file-excel"></span>
                            <span className="k-icon k-i-file-word" title="k-icon k-i-file-word"></span>
                            <span className="k-icon k-i-file-mdb" title="k-icon k-i-file-mdb"></span>
                            <span className="k-icon k-i-file-ppt" title="k-icon k-i-file-ppt"></span>
                            <span className="k-icon k-i-file-pdf" title="k-icon k-i-file-pdf"></span>
                            <span className="k-icon k-i-file-psd" title="k-icon k-i-file-psd"></span>
                            <span className="k-icon k-i-file-flash" title="k-icon k-i-file-flash"></span>
                            <span className="k-icon k-i-file-config" title="k-icon k-i-file-config"></span>
                            <span className="k-icon k-i-file-ascx" title="k-icon k-i-file-ascx"></span>
                            <span className="k-icon k-i-file-bac" title="k-icon k-i-file-bac"></span>
                            <span className="k-icon k-i-file-zip" title="k-icon k-i-file-zip"></span>
                            <span className="k-icon k-i-film" title="k-icon k-i-film"></span>
                            <span className="k-icon k-i-css3" title="k-icon k-i-css3"></span>
                            <span className="k-icon k-i-html5" title="k-icon k-i-html5"></span>
                            <span className="k-icon k-i-html" title="k-icon k-i-html"></span>
                            <span className="k-icon k-i-css" title="k-icon k-i-css"></span>
                            <span className="k-icon k-i-js" title="k-icon k-i-js"></span>
                            <span className="k-icon k-i-exe" title="k-icon k-i-exe"></span>
                            <span className="k-icon k-i-csproj" title="k-icon k-i-csproj"></span>
                            <span className="k-icon k-i-vbproj" title="k-icon k-i-vbproj"></span>
                            <span className="k-icon k-i-cs" title="k-icon k-i-cs"></span>
                            <span className="k-icon k-i-vb" title="k-icon k-i-vb"></span>
                            <span className="k-icon k-i-sln" title="k-icon k-i-sln"></span>
                            <span className="k-icon k-i-cloud" title="k-icon k-i-cloud"></span>
                            <span className="k-icon k-i-file-horizontal" title="k-icon k-i-file-horizontal"></span>
                        </div>
                    </div>
                    <div className="Images-icon">
                        <h4>Images</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-photo-camera" title="k-icon k-i-photo-camera"></span>
                            <span className="k-icon k-i-image" title="k-icon k-i-image"></span>
                            <span className="k-icon k-i-image-export" title="k-icon k-i-image-export"></span>
                            <span className="k-icon k-i-zoom-actual-size" title="k-icon k-i-zoom-actual-size"></span>
                            <span className="k-icon k-i-zoom-best-fit" title="k-icon k-i-zoom-best-fit"></span>
                            <span className="k-icon k-i-image-resize" title="k-icon k-i-image-resize"></span>
                            <span className="k-icon k-i-crop" title="k-icon k-i-crop"></span>
                            <span className="k-icon k-i-mirror" title="k-icon k-i-mirror"></span>
                            <span className="k-icon k-i-flip-horizontal" title="k-icon k-i-flip-horizontal"></span>
                            <span className="k-icon k-i-flip-vertical" title="k-icon k-i-flip-vertical"></span>
                            <span className="k-icon k-i-rotate" title="k-icon k-i-rotate"></span>
                            <span className="k-icon k-i-rotate-right" title="k-icon k-i-rotate-right"></span>
                            <span className="k-icon k-i-rotate-left" title="k-icon k-i-rotate-left"></span>
                            <span className="k-icon k-i-brush" title="k-icon k-i-brush"></span>
                            <span className="k-icon k-i-palette" title="k-icon k-i-palette"></span>
                            <span className="k-icon k-i-paint" title="k-icon k-i-paint"></span>
                            <span className="k-icon k-i-line" title="k-icon k-i-line"></span>
                            <span className="k-icon k-i-brightness-contrast" title="k-icon k-i-brightness-contrast"></span>
                            <span className="k-icon k-i-saturation" title="k-icon k-i-saturation"></span>
                            <span className="k-icon k-i-invert-colors" title="k-icon k-i-invert-colors"></span>
                            <span className="k-icon k-i-transperancy" title="k-icon k-i-transperancy"></span>
                            <span className="k-icon k-i-greyscale" title="k-icon k-i-greyscale"></span>
                            <span className="k-icon k-i-blur" title="k-icon k-i-blur"></span>
                            <span className="k-icon k-i-sharpen" title="k-icon k-i-sharpen"></span>
                            <span className="k-icon k-i-shape" title="k-icon k-i-shape"></span>
                            <span className="k-icon k-i-round-corners" title="k-icon k-i-round-corners"></span>
                            <span className="k-icon k-i-front-element" title="k-icon k-i-front-element"></span>
                            <span className="k-icon k-i-back-element" title="k-icon k-i-back-element"></span>
                            <span className="k-icon k-i-forward-element" title="k-icon k-i-forward-element"></span>
                            <span className="k-icon k-i-backward-element" title="k-icon k-i-backward-element"></span>
                            <span className="k-icon k-i-align-left-element" title="k-icon k-i-align-left-element"></span>
                            <span className="k-icon k-i-align-center-element" title="k-icon k-i-align-center-element"></span>
                            <span className="k-icon k-i-align-right-element" title="k-icon k-i-align-right-element"></span>
                            <span className="k-icon k-i-align-top-element" title="k-icon k-i-align-top-element"></span>
                            <span className="k-icon k-i-align-middle-element" title="k-icon k-i-align-middle-element"></span>
                            <span className="k-icon k-i-align-bottom-element" title="k-icon k-i-align-bottom-element"></span>
                            <span className="k-icon k-i-thumbnails-up" title="k-icon k-i-thumbnails-up"></span>
                            <span className="k-icon k-i-thumbnails-right" title="k-icon k-i-thumbnails-right"></span>
                            <span className="k-icon k-i-thumbnails-down" title="k-icon k-i-thumbnails-down"></span>
                            <span className="k-icon k-i-thumbnails-left" title="k-icon k-i-thumbnails-left"></span>
                            <span className="k-icon k-i-full-screen" title="k-icon k-i-full-screen"></span>
                            <span className="k-icon k-i-full-screen-exit" title="k-icon k-i-full-screen-exit"></span>
                            <span className="k-icon k-i-reset-color" title="k-icon k-i-reset-color"></span>
                        </div>
                    </div>
                    <div className="Layout-and-Navigation">
                        <h4>Layout and Navigation</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-arrow-45-up-right" title="k-icon k-i-arrow-45-up-right"></span>
                            <span className="k-icon k-i-arrow-45-down-right" title="k-icon k-i-arrow-45-down-right"></span>
                            <span className="k-icon k-i-arrow-45-down-left" title="k-icon k-i-arrow-45-down-left"></span>
                            <span className="k-icon k-i-arrow-45-up-left" title="k-icon k-i-arrow-45-up-left"></span>
                            <span className="k-icon k-i-arrow-60-up" title="k-icon k-i-arrow-60-up"></span>
                            <span className="k-icon k-i-arrow-60-right" title="k-icon k-i-arrow-60-right"></span>
                            <span className="k-icon k-i-arrow-60-down" title="k-icon k-i-arrow-60-down"></span>
                            <span className="k-icon k-i-arrow-60-left" title="k-icon k-i-arrow-60-left"></span>
                            <span className="k-icon k-i-arrow-end-up" title="k-icon k-i-arrow-end-up"></span>
                            <span className="k-icon k-i-arrow-end-right" title="k-icon k-i-arrow-end-right"></span>
                            <span className="k-icon k-i-arrow-end-down" title="k-icon k-i-arrow-end-down"></span>
                            <span className="k-icon k-i-arrow-end-left" title="k-icon k-i-arrow-end-left"></span>
                            <span className="k-icon k-i-arrow-double-60-up" title="k-icon k-i-arrow-double-60-up"></span>
                            <span className="k-icon k-i-arrow-seek-up" title="k-icon k-i-arrow-seek-up"></span>
                            <span className="k-icon k-i-arrow-double-60-right" title="k-icon k-i-arrow-double-60-right"></span>
                            <span className="k-icon k-i-arrow-seek-right" title="k-icon k-i-arrow-seek-right"></span>
                            <span className="k-icon k-i-arrow-double-60-down" title="k-icon k-i-arrow-double-60-down"></span>
                            <span className="k-icon k-i-arrow-seek-down" title="k-icon k-i-arrow-seek-down"></span>
                            <span className="k-icon k-i-arrow-double-60-left" title="k-icon k-i-arrow-double-60-left"></span>
                            <span className="k-icon k-i-arrows-kpi" title="k-icon k-i-arrows-kpi"></span>
                            <span className="k-icon k-i-arrows-no-change" title="k-icon k-i-arrows-no-change"></span>
                            <span className="k-icon k-i-arrow-overflow-down" title="k-icon k-i-arrow-overflow-down"></span>
                            <span className="k-icon k-i-arrow-chevron-up" title="k-icon k-i-arrow-chevron-up"></span>
                            <span className="k-icon k-i-arrow-chevron-right" title="k-icon k-i-arrow-chevron-right"></span>
                            <span className="k-icon k-i-arrow-chevron-down" title="k-icon k-i-arrow-chevron-down"></span>
                            <span className="k-icon k-i-arrow-chevron-left" title="k-icon k-i-arrow-chevron-left"></span>
                            <span className="k-icon k-i-arrow-up" title="k-icon k-i-arrow-up"></span>
                            <span className="k-icon k-i-arrow-right" title="k-icon k-i-arrow-right"></span>
                            <span className="k-icon k-i-arrow-down" title="k-icon k-i-arrow-down"></span>
                            <span className="k-icon k-i-arrow-left" title="k-icon k-i-arrow-left"></span>
                            <span className="k-icon k-i-arrow-drill" title="k-icon k-i-arrow-drill"></span>
                            <span className="k-icon k-i-arrow-parent" title="k-icon k-i-arrow-parent"></span>
                            <span className="k-icon k-i-arrow-root" title="k-icon k-i-arrow-root"></span>
                            <span className="k-icon k-i-arrows-resizing" title="k-icon k-i-arrows-resizing"></span>
                            <span className="k-icon k-i-arrows-dimensions" title="k-icon k-i-arrows-dimensions"></span>
                            <span className="k-icon k-i-arrows-swap" title="k-icon k-i-arrows-swap"></span>
                            <span className="k-icon k-i-drag-and-drop" title="k-icon k-i-drag-and-drop"></span>
                            <span className="k-icon k-i-categorize" title="k-icon k-i-categorize"></span>
                            <span className="k-icon k-i-grid" title="k-icon k-i-grid"></span>
                            <span className="k-icon k-i-grid-layout" title="k-icon k-i-grid-layout"></span>
                            <span className="k-icon k-i-group" title="k-icon k-i-group"></span>
                            <span className="k-icon k-i-ungroup" title="k-icon k-i-ungroup"></span>
                            <span className="k-icon k-i-handler-drag" title="k-icon k-i-handler-drag"></span>
                            <span className="k-icon k-i-layout" title="k-icon k-i-layout"></span>
                            <span className="k-icon k-i-layout-1-by-4" title="k-icon k-i-layout-1-by-4"></span>
                            <span className="k-icon k-i-layout-2-by-2" title="k-icon k-i-layout-2-by-2"></span>
                            <span className="k-icon k-i-layout-side-by-side" title="k-icon k-i-layout-side-by-side"></span>
                            <span className="k-icon k-i-layout-stacked" title="k-icon k-i-layout-stacked"></span>
                            <span className="k-icon k-i-columns" title="k-icon k-i-columns"></span>
                            <span className="k-icon k-i-rows" title="k-icon k-i-rows"></span>
                            <span className="k-icon k-i-reorder" title="k-icon k-i-reorder"></span>
                            <span className="k-icon k-i-menu" title="k-icon k-i-menu"></span>
                            <span className="k-icon k-i-more-vertical" title="k-icon k-i-more-vertical"></span>
                            <span className="k-icon k-i-more-horizontal" title="k-icon k-i-more-horizontal"></span>
                        </div>
                    </div>
                    <div className="Mapping-icon">
                        <h4>Mapping</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-globe-outline" title="k-icon k-i-globe-outline"></span>
                            <span className="k-icon k-i-globe" title="k-icon k-i-globe"></span>
                            <span className="k-icon k-i-marker-pin" title="k-icon k-i-marker-pin"></span>
                            <span className="k-icon k-i-marker-pin-target" title="k-icon k-i-marker-pin-target"></span>
                            <span className="k-icon k-i-pin" title="k-icon k-i-pin"></span>
                            <span className="k-icon k-i-unpin" title="k-icon k-i-unpin"></span>
                        </div>
                    </div>
                    <div className="Media-icon">
                        <h4>Media</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-play" title="k-icon k-i-play"></span>
                            <span className="k-icon k-i-pause" title="k-icon k-i-pause"></span>
                            <span className="k-icon k-i-stop" title="k-icon k-i-stop"></span>
                            <span className="k-icon k-i-rewind" title="k-icon k-i-rewind"></span>
                            <span className="k-icon k-i-forward" title="k-icon k-i-forward"></span>
                            <span className="k-icon k-i-volume-down" title="k-icon k-i-volume-down"></span>
                            <span className="k-icon k-i-volume-up" title="k-icon k-i-volume-up"></span>
                            <span className="k-icon k-i-volume-off" title="k-icon k-i-volume-off"></span>
                            <span className="k-icon k-i-hd" title="k-icon k-i-hd"></span>
                            <span className="k-icon k-i-subtitles" title="k-icon k-i-subtitles"></span>
                            <span className="k-icon k-i-playlist" title="k-icon k-i-playlist"></span>
                            <span className="k-icon k-i-audio" title="k-icon k-i-audio"></span>
                        </div>
                    </div>
                    <div className="Social-Sharing-icon">
                        <h4>Social Sharing</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-share" title="k-icon k-i-share"></span>
                            <span className="k-icon k-i-user" title="k-icon k-i-user"></span>
                            <span className="k-icon k-i-inbox" title="k-icon k-i-inbox"></span>
                            <span className="k-icon k-i-blogger" title="k-icon k-i-blogger"></span>
                            <span className="k-icon k-i-blogger-box" title="k-icon k-i-blogger-box"></span>
                            <span className="k-icon k-i-delicious" title="k-icon k-i-delicious"></span>
                            <span className="k-icon k-i-delicious-box" title="k-icon k-i-delicious-box"></span>
                            <span className="k-icon k-i-digg" title="k-icon k-i-digg"></span>
                            <span className="k-icon k-i-digg-box" title="k-icon k-i-digg-box"></span>
                            <span className="k-icon k-i-email" title="k-icon k-i-email"></span>
                            <span className="k-icon k-i-email-box" title="k-icon k-i-email-box"></span>
                            <span className="k-icon k-i-facebook" title="k-icon k-i-facebook"></span>
                            <span className="k-icon k-i-facebook-box" title="k-icon k-i-facebook-box"></span>
                            <span className="k-icon k-i-google" title="k-icon k-i-google"></span>
                            <span className="k-icon k-i-google-box" title="k-icon k-i-google-box"></span>
                            <span className="k-icon k-i-google-plus" title="k-icon k-i-google-plus"></span>
                            <span className="k-icon k-i-google-plus-box" title="k-icon k-i-google-plus-box"></span>
                            <span className="k-icon k-i-linkedin" title="k-icon k-i-linkedin"></span>
                            <span className="k-icon k-i-linkedin-box" title="k-icon k-i-linkedin-box"></span>
                            <span className="k-icon k-i-myspace" title="k-icon k-i-myspace"></span>
                            <span className="k-icon k-i-myspace-box" title="k-icon k-i-myspace-box"></span>
                            <span className="k-icon k-i-pinterest" title="k-icon k-i-pinterest"></span>
                            <span className="k-icon k-i-pinterest-box" title="k-icon k-i-pinterest-box"></span>
                            <span className="k-icon k-i-reddit" title="k-icon k-i-reddit"></span>
                            <span className="k-icon k-i-reddit-box" title="k-icon k-i-reddit-box"></span>
                            <span className="k-icon k-i-stumble-upon" title="k-icon k-i-stumble-upon"></span>
                            <span className="k-icon k-i-stumble-upon-box" title="k-icon k-i-stumble-upon-box"></span>
                            <span className="k-icon k-i-tell-a-friend" title="k-icon k-i-tell-a-friend"></span>
                            <span className="k-icon k-i-tell-a-friend-box" title="k-icon k-i-tell-a-friend-box"></span>
                            <span className="k-icon k-i-tumblr" title="k-icon k-i-tumblr"></span>
                            <span className="k-icon k-i-tumblr-box" title="k-icon k-i-tumblr-box"></span>
                            <span className="k-icon k-i-twitter" title="k-icon k-i-twitter"></span>
                            <span className="k-icon k-i-twitter-box" title="k-icon k-i-twitter-box"></span>
                            <span className="k-icon k-i-yammer" title="k-icon k-i-yammer"></span>
                            <span className="k-icon k-i-yammer-box" title="k-icon k-i-yammer-box"></span>
                            <span className="k-icon k-i-behance" title="k-icon k-i-behance"></span>
                            <span className="k-icon k-i-behance-box" title="k-icon k-i-behance-box"></span>
                            <span className="k-icon k-i-dribbble" title="k-icon k-i-dribbble"></span>
                            <span className="k-icon k-i-dribbble-box" title="k-icon k-i-dribbble-box"></span>
                            <span className="k-icon k-i-rss" title="k-icon k-i-rss"></span>
                            <span className="k-icon k-i-rss-box" title="k-icon k-i-rss-box"></span>
                            <span className="k-icon k-i-vimeo" title="k-icon k-i-vimeo"></span>
                            <span className="k-icon k-i-vimeo-box" title="k-icon k-i-vimeo-box"></span>
                            <span className="k-icon k-i-youtube" title="k-icon k-i-youtube"></span>
                            <span className="k-icon k-i-youtube-box" title="k-icon k-i-youtube-box"></span>
                        </div>
                    </div>
                    <div className="Toggle-icon">
                        <h4>Toggle</h4>
                        <div className="icon-display">
                            <span className="k-icon k-i-heart-outline" title="k-icon k-i-heart-outline"></span>
                            <span className="k-icon k-i-heart" title="k-icon k-i-heart"></span>
                            <span className="k-icon k-i-star-outline" title="k-icon k-i-star-outline"></span>
                            <span className="k-icon k-i-star" title="k-icon k-i-star"></span>
                            <span className="k-icon k-i-checkbox" title="k-icon k-i-checkbox"></span>
                            <span className="k-icon k-i-checkbox-checked" title="k-icon k-i-checkbox-checked"></span>
                            <span className="k-icon k-i-tri-state-indeterminate" title="k-icon k-i-tri-state-indeterminate"></span>
                            <span className="k-icon k-i-tri-state-null" title="k-icon k-i-tri-state-null"></span>
                            <span className="k-icon k-i-circle" title="k-icon k-i-circle"></span>
                            <span className="k-icon k-i-radiobutton" title="k-icon k-i-radiobutton"></span>
                            <span className="k-icon k-i-radiobutton-checked" title="k-icon k-i-radiobutton-checked"></span>
                        </div>
                    </div>
                </div>
            </div>
        </div >
    );
}

export default DemoCss;