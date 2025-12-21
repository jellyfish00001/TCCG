//@ts-check
import { api } from '../../../Basic/ApiFetch';
import { AddNoColumn, IsNullOrEmpty, HtmlDecode } from '../../../Basic/SDOExtension';
import { getGlobalServerConfig } from '../../../Route/RootMiddleware'


let announcementUrl = getGlobalServerConfig().backEndUrl.get() + 'Announcement'

//取得公告
const getAnnouncement = async (data) => {
    let url = announcementUrl + '/GetAnnouncement'
    let response = await api.Post(url, JSON.stringify(data))
    let announcement = []
    if (response.ok) {
        announcement = AddNoColumn(await response.json())
    }

    return announcement
}

//儲存上傳檔案到user資料夾
const saveUploads = async (fileNames) => {
    let url = getGlobalServerConfig().backEndUrl.get() + 'UploadFile/SaveUploads'
    let response = await api.Post(url, JSON.stringify(fileNames))

    return response.ok
}

//新增公告
const insertAnnouncement = async (fileNames, announcementData) => {
    if (fileNames.fileNames.length > 0) {
        if (!await saveUploads(fileNames)) {
            return {
                ok: false,
                result: "檔案上傳有誤，請洽系統管理員!"
            }
        }
    }
    let response = await api.Post((announcementUrl), JSON.stringify(announcementData))

    return {
        ok: response.ok,
        result: (await response.json()).message
    };

}

//修改公告
const updateAnnouncement = async (fileNames, announcementData) => {
    if (fileNames.fileNames.length > 0) {
        if (!await saveUploads(fileNames)) {
            return {
                ok: false,
                result: "檔案上傳有誤，請洽系統管理員!"
            }
        }
    }
    let response = await api.Put((announcementUrl), JSON.stringify(announcementData))
    return {
        ok: response.ok,
        result: (await response.json()).message
    };
}

//刪除公告 
const deleteAnnouncement = async (sid) => {
    let url = announcementUrl + '/' + sid
    let response = await api.Delete(url)

    return {
        ok: response.ok,
        result: await response.json()
    }
}

//取得單筆公告bySId
const getAnnouncementBySId = async (sid) => {
    let announcement = {
        SID: sid,
        TITLE: "",
        COMMENT: "",
        EFFECTIVE_DATE: new Date(),
        EXPIRE_DATE: new Date(),
        ATTACH_NAME: [],
        ANN_TYPE: "", // 公告類別
        IS_URL_LINK: "N", // 是否為外部連結
        URL_LINK: "", // 連結網址
    }

    let url = announcementUrl + '/' + sid
    let response = await api.Get(url, null, null, false)
    if (response.ok) {
        let data = await response.json()
        announcement = {
            ...announcement,
            TITLE: data.TITLE,
            COMMENT: data.COMMENT,
            EFFECTIVE_DATE: new Date(data.EFFECTIVE_DATE),
            EXPIRE_DATE: new Date(data.EXPIRE_DATE),
            ATTACH_NAME: IsNullOrEmpty(data.ATTACH_NAME) ? [] : data.ATTACH_NAME.split(","), //讀取or上傳檔案傳入的參數為array，故此地方調整成array
            ANN_TYPE: IsNullOrEmpty(data.ANN_TYPE) ? "" : data.ANN_TYPE,
            IS_URL_LINK: data.IS_URL_LINK, // 是否為外部連結
            URL_LINK: IsNullOrEmpty(data.URL_LINK) ? "" : data.URL_LINK, // 連結網址
        }
    }
    return announcement
}

const getAnnTypeData = async () => {
    let url = announcementUrl + '/GetAnnType'
    let response = await api.Get(url)
    let annTypes = [{ SET_TYPE: "", SET_VALUE: "請選擇" }]

    let data = await response.json()
    data.forEach(item => {
        annTypes.push({ SET_TYPE: item.SET_TYPE, SET_VALUE: item.SET_VALUE })
    });
    return annTypes
}

/**
 * 取得上傳檔案資訊
 * @param {*} fileSeqNo 
 */
const getGetUploads = async (fileSeqNo) => {
    let data = {
        fileNames: fileSeqNo //存檔的時傳入的參數型態為string array
    }
    let response = await api.Post(getGlobalServerConfig().backEndUrl.get() + 'UploadFile/GetUploads', JSON.stringify(data))
    if (response.ok) {
        return await response.json();
    }
}

const AnnouncementService = {
    getAnnouncement: getAnnouncement,
    insertAnnouncement: insertAnnouncement,
    updateAnnouncement: updateAnnouncement,
    deleteAnnouncement: deleteAnnouncement,
    getAnnouncementBySId: getAnnouncementBySId,
    getAnnTypeData: getAnnTypeData,
    getGetUploads: getGetUploads
}

export default AnnouncementService;