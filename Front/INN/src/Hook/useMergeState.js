import { useReducer } from 'react'

///以Merge來修改的State
/// call mergeState(newState) then state = {...state, ...newState}
const useMergeState = defaultValue => {
    const [state, mergeState] = useReducer((prev, newState) => ({...prev,...newState}), defaultValue)
    return [state, mergeState];
} 

export default useMergeState;