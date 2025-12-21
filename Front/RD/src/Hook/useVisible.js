import { useEffect } from 'react'

///當Visible變動為true時，執行action
const useVisible = (action, visible) => {
    useEffect(() => {
        if(visible)
            action();
    }, [visible])
}

export default useVisible