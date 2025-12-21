import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { HtmlDecode, AddNoColumn } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const MailSetService = () =>{
    const mailSetUrl = getGlobalServerConfig().backEndUrl.get() + 'MailSet/';

    const loadMailSet = async () => {
        const response = await api.Read(mailSetUrl);
        let mailSet = [];

        if(response.ok)
            mailSet = AddNoColumn(await response.json());
        
        return mailSet;
    }

    const loadMailSetById = async mailId => {
        const response = await api.Read(mailSetUrl + mailId);
        let mailSet = {
            MAIL_ID:'',
            MAIL_NAME:'',
            MAIL_SUBJECT: '',
            MAIL_CONTENT: '',
        }

        if(response.ok){
            let mail = await response.json();
            mailSet = {
                MAIL_ID: mail.MAIL_ID,
                MAIL_NAME: mail.MAIL_NAME,
                MAIL_SUBJECT: mail.MAIL_SUBJECT,
                MAIL_CONTENT: mail.MAIL_CONTENT,
            }
        }
        
        return mailSet;
    }

    const loadMailType = async () => {
        let response = await api.Read(getGlobalServerConfig().backEndUrl.get() + 'SetParam/MailType');
        let mailType = []

        if(response.ok)
            mailType = await response.json();
        return mailType;
    }

    const loadMailRole = async () => {
        let response = await api.Read(getGlobalServerConfig().backEndUrl.get() + 'MailRole/');
        let mailRole = []

        if(response.ok)
            mailRole = await response.json();
        return mailRole;
    }

    const loadRecipientById = async mailId => {
        const response = await api.Read(mailSetUrl + 'ReadRecipientById/' + mailId);
        let recipients = [];

        if(response.ok)
            recipients = await response.json();
        
        return recipients;
    }

    const insertMailSet = async mailSet => {
        const response = await api.Insert(mailSetUrl, JSON.stringify(mailSet));
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    const updateMailSet = async mailSet => {
        const response = await api.Update(mailSetUrl, JSON.stringify(mailSet));
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    const updateRecipients = async (mailId,recipients) => {
        const response = await api.Update(
            mailSetUrl + 'UpdateRecipient', 
            JSON.stringify({
                MAIL_ID: mailId,
                RECIPIENT: recipients
            }));
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    const deleteMailSet = async mailId => {
        let response = await api.Delete(mailSetUrl + mailId);
        return {
            ok: response.ok,
            result: await response.json()
        }
    }

    return {
        loadMailSet: loadMailSet,
        loadMailSetById: loadMailSetById,
        loadMailType: loadMailType,
        loadMailRole: loadMailRole,
        loadRecipientById: loadRecipientById,
        insertMailSet: insertMailSet,
        updateMailSet: updateMailSet,
        updateRecipients: updateRecipients,
        deleteMailSet: deleteMailSet,
    }
}

export default MailSetService;