import { api } from '../../../Basic/ApiFetch';
import { IsNullOrEmpty, HtmlDecode } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware';
import * as Yup from 'yup';

let APIUrl = getGlobalServerConfig().backEndUrl.get();

//取得郵件範本
const getMailTemplate = async () => {
    let url = APIUrl + 'MailSet/GetMailTemplate';
    let response = await api.Post(url);
    let result = [];
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

//取得郵件範本 by ID
const getMailTemplateById = async (mailId) => {
    let url = APIUrl + 'MailSet/GetMailTemplateById';
    let response = await api.Post(url, JSON.stringify(mailId));
    let result = {}
    if (response.ok) {
        result = await response.json();
    }
    return result;
}

//儲存郵件範本
const saveMailTemplate = async (model) => {
    let url = APIUrl + 'MailSet/SaveMailTemplate';
    let response = await api.Post(url, JSON.stringify(model));
    let result = null;
    if (response.ok) {
        result = response.json();
    }
    return result;
}

// 驗證
const validateField = Yup.object().shape({
    MAIL_NAME: Yup.string().required("範本名稱必填"),
    MAIL_SUBJECT: Yup.string().required("郵件主旨必填"),
    MAIL_CONTENT: Yup.string().test(
        {
            message: "郵件內容必填",
            test: function (value) {
                return IsNullOrEmpty(HtmlDecode(value)) ? false : true;
            }
        }
    ),
})

export const MailSetService = {
    getMailTemplate: getMailTemplate,
    getMailTemplateById: getMailTemplateById,
    saveMailTemplate: saveMailTemplate,
    validateField: validateField,
}

export default MailSetService;