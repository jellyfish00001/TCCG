import { api } from '../../Basic/ApiFetch';
import { AddNoColumn} from '../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../Route/RootMiddleware'

let aplUrl = getGlobalServerConfig().APL_backEndUrl.get()

const ResetPW = async(data)=>{
    let url = aplUrl+'Forgot/ResetPW'
    let response = await api.Post(url,JSON.stringify(data));
    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

const RVWResetService = {
    ResetPW:ResetPW,
    
}

export default RVWResetService