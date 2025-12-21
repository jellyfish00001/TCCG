import React from 'react';

export const WindowResizehook=()=> {
  const [dimensions, setDimensions] = React.useState({
    height: window.innerHeight,
    width: window.innerWidth
})
React.useEffect(() => {
    //在useeffect內定義function以提供給window監聽
    function handleResize() {
        setDimensions({
            height: window.innerHeight,
            width: window.innerWidth
        })
    }
    window.addEventListener('resize', handleResize)

    //在useEffect內的return function一定會被執行，藉此移除監聽
    return _ => {
        window.removeEventListener('resize', handleResize)
    }
}, [])

  return dimensions;
}