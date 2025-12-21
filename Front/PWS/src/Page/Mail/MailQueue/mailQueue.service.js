import { api } from '../../../Basic/ApiFetch';
import { ServerConfig } from '../../../Basic/BasicData';
import { AddNoColumn, HtmlDecode } from '../../../Basic/SDOExtension';
import {getGlobalServerConfig} from '../../../Route/RootMiddleware'

const MailQueueService = () => {
    const mailQueueUrl = getGlobalServerConfig().backEndUrl.get() + 'MailQueue/';
    const mailSetUrl = getGlobalServerConfig().backEndUrl.get() + 'MailSet/';

    const loadMailQueue = async () => {
        const response = await api.Read(mailQueueUrl);
        let mailQueue = []

        if(response.ok)
            mailQueue = AddNoColumn(await response.json())
        
        return mailQueue;
    }

    const loadMailQueueById = async queueId => {
        let response = await api.Read(mailQueueUrl + queueId);
        if (response.ok)
            return await response.json();
        else
            return null;
    }

    const loadMailSet = async () => {
        let response = await api.Read(mailSetUrl);
        let mailSet = []

        if (response.ok)
            mailSet = await response.json();

        return mailSet;
    }

    const loadMailTemplate = async mailId => {
        let response = await api.Read(mailSetUrl + mailId);
        let mailTemplate = {
            MAIL_SUBJECT: '',
            MAIL_CONTENT: '',
        }

        if (response.ok) {
            let result = await response.json();
            mailTemplate.MAIL_SUBJECT = result.MAIL_SUBJECT;
            mailTemplate.MAIL_CONTENT = HtmlDecode(HtmlDecode(result.MAIL_CONTENT));
        }

        return mailTemplate;
    }

    const insertQueue = async mailQueue => {
        let response = await api.Insert(mailQueueUrl, JSON.stringify(mailQueue));
        return{ 
            ok: response.ok,
            result: await response.json(),
        }
    }

    const updateQueue = async mailQueue => {
        let response = await api.Update(mailQueueUrl, JSON.stringify(mailQueue));
        return{ 
            ok: response.ok,
            result: await response.json(),
        }
    }

    const deleteQueue = async queueId => {
        let response = await api.Delete(mailQueueUrl + queueId);
        return{
            ok: response.ok,
            result: await response.json(),
        }
    }

    return {
        loadMailQueue: loadMailQueue,
        loadMailQueueById: loadMailQueueById,
        loadMailSet: loadMailSet,
        loadMailTemplate: loadMailTemplate,
        insertQueue: insertQueue,
        updateQueue: updateQueue,
        deleteQueue: deleteQueue,
    }
}

export default MailQueueService;