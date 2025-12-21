import React from 'react'
import { FormatDate, IsNullOrEmpty } from "../../../Basic/SDOExtension";

/**
 * 總期程/分月期程調整歷程
 * @param {array} historyList 歷程資料
 * @param {string} scheType 調整類別 Y: 總期程調整、M: 分月期程調整
 * @returns 
 */
export const adjustHistory = (historyList, scheType) => {
    if (IsNullOrEmpty(historyList)) {
        return null;
    }
    return (
        historyList.map(history => {
            if (scheType == "Y" && history.SCHE_TYPE == scheType) {
                return (
                    <li key={history.SEQ} style={{ display: 'flex', alignItems: 'center' }}>
                        {"第" + history.SEQ + "次：" +
                            "原定" + (IsNullOrEmpty(history.ORI_ESTIMATED_ENDDATE) ? '' : FormatDate(history.ORI_ESTIMATED_ENDDATE)) + "完成，" +
                            "調整至" + (IsNullOrEmpty(history.ADJ_LAST_DATE) ? '' : FormatDate(history.ADJ_LAST_DATE)) +
                            "(核准日期：" + (IsNullOrEmpty(history.APPRV_DATE) ? '' : FormatDate(history.APPRV_DATE)) + ")。" +
                            "調整原因：" + (IsNullOrEmpty(history.REASON) ? '' : history.REASON)
                        }
                    </li>
                )
            }
            else if (scheType == "M" && history.SCHE_TYPE == scheType) {
                return (
                    <li key={history.SEQ} style={{ display: 'flex', alignItems: 'center' }}>
                        {"第" + history.SEQ + "次：" +
                            (IsNullOrEmpty(history.ADJ_LAST_DATE) ? '' : FormatDate(history.ADJ_LAST_DATE)) +
                            "(核准日期：" + (IsNullOrEmpty(history.APPRV_DATE) ? '' : FormatDate(history.APPRV_DATE)) + ")。" +
                            "調整原因：" + (IsNullOrEmpty(history.REASON) ? '' : history.REASON)
                        }
                    </li>
                )
            }
            return null;
        })
    )
}