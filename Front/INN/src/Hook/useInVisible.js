import { useEffect } from 'react'

///當Visible變動為false時，執行action
const useInVisible = (action, visible) => {
    useEffect(() => {
        if(!visible)
            action();
    }, [visible])
}

export default useInVisible;