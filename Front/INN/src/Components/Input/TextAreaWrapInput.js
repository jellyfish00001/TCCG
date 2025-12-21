import React from 'react';

// 多行輸入框：純顯示不可編輯，需有換行效果
const TextAreaWrapInput = (props) => {
    return (
        <span style={{ whiteSpace: "pre-wrap" }}>
            {props.value}
        </span>
    )
}
export default React.memo(TextAreaWrapInput)