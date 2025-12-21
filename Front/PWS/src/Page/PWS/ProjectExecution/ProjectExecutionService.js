import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { AddNoColumn } from "../../../Basic/SDOExtension";

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/*
*更改歷年執行情形
*/
export const saveExecution = async (data) => {
    let url = APIUrl + 'BudgetExec/SavePWSSDHISTORYEXE';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = null;
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/*
*取歷年執行情形
*/
export const GetExecution = async (PLANID, PLANYEAR) => {
    let url = APIUrl + 'BudgetExec/GetPWSSDHISTORYEXE';
    let form = new FormData();
    form.append('PLANID', PLANID);
    form.append('PLANYEAR', PLANYEAR);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return AddNoColumn(result) ;
};