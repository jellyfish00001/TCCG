import { api } from '../../../Basic/ApiFetch';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import { AddNoColumn } from '../../../Basic/SDOExtension';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

/**
 * 更改歷年執行情形
 * @param {*} data 
 * @returns 
 */
export const SaveFundingExecution = async (data) => {
    let url = APIUrl + 'FundingExecution/SaveFundingExecution';
    let response = await api.Post(url, JSON.stringify(data), null);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
};

/**
 * 取得全部資料
 * @param {*} PLANNO 
 * @param {*} PLANYEAR 
 * @returns 
 */
export const GetGridData = async (planNo, planYear) => {
    let url = APIUrl + 'FundingExecution/GetFundingExecution';
    let form = new FormData();
    form.append('PLANNO', planNo);
    form.append('PLANYEAR', planYear);
    let response = await api.Post(url, form, new Headers(), false);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return AddNoColumn(result) ;
};

// 驗證規則
export const ExecutionValidation = Yup.array().of(
    Yup.object().shape({
        EXEDESC: Yup.string().nullable().when('RATIO', {
            is: (val) => val && val < 80,
            then: Yup.string().required('預算執行率如未達80%，請敘明原因'),
        }),
    })
).nullable(); 

/**
 * 存檔驗證
 * @returns 
 */
export const FundingValidation = Yup.array().of(
    Yup.object().shape({
        FUNDDESC: Yup.string().required('請填寫經費需求細項'),
        AMOUNT: Yup.number().moreThan(0, '數量(千元)必須大於等於0'),
        PRICE: Yup.number().moreThan(0, '單價(千元)必須大於等於0'),
    })
).nullable(); 