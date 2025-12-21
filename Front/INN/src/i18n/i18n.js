import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './en';
import tw from './zh-TW';

const resources = {
  'en': en,
  'zh-TW': tw
};

i18n.use(initReactI18next).init({
  resources,
  lng: 'zh-TW',             //預設語言
  fallbackLng: 'zh-TW',     //如果當前切換的語言沒有對應的翻譯則使用這個語言，
  interpolation: {
    escapeValue: false,
  },
});

export default i18n;