import React from 'react';
import GoogleMapReact from 'google-map-react';
import '../../../Css/Map.css';
import { IsNullOrEmpty } from '../../../Basic/SDOExtension';

// 基本資料 地圖
const ProjectBasicMap = (props) => {
    const { myPosition } = props;

    //#region 單點地圖定位=>google map控制
    const mapDefaultProps = React.useRef({
        key: process.env.REACT_APP_GOOGLE_API_KEY,
        zoom: 12, // 縮放視角
        //預設地址為 桃園區縣府路1號 - 桃園市政府
        center: {
            lat: 24.993098524588383,
            lng: 121.30101509392262
        }
    });
    const [mapApiLoaded, setMapApiLoaded] = React.useState(false);
    const [mapInstance, setMapInstance] = React.useState(null);
    const [mapApi, setMapApi] = React.useState(null);
    const prevMarkerRef = React.useRef([]); // 記錄先前的marker

    const mapOption = (maps) => {
        return {
            streetViewControl: true,
            scrollwheel: false,  //是否允許使用者對地圖物件使用滑鼠滾輪
            mapTypeId: 'roadmap',
            zoomControl: true, //縮放地圖
            zoomControlOptions: {
                position: maps.ControlPosition.TOP_RIGHT
            },
            mapTypeControl: true, //地圖類型
            mapTypeControlOptions: {
                style: maps.MapTypeControlStyle.DROPDOWN_MENU
            }
        }
    }

    // 當地圖載入完成，將地圖實體與地圖 API 傳入 state 供之後使用
    const apiHasLoaded = (map, maps) => {
        let marker = new maps.Marker({
            position: { lat: map.center.lat(), lng: map.center.lng() },
            map
        });
        prevMarkerRef.current = marker;
        setMapInstance(map);
        setMapApi(maps);
        setMapApiLoaded(true);
        return marker;
    };

    // 根據xy找地點
    const findLocationByXY = async (X_COORD, Y_COORD) => {
        if (mapApiLoaded) {
            if (!IsNullOrEmpty(X_COORD) && !IsNullOrEmpty(Y_COORD)) {
                //移除當下位置的marker
                prevMarkerRef.current.setMap(null);

                let marker = new mapApi.Marker({
                    position: { lat: parseFloat(X_COORD), lng: parseFloat(Y_COORD) },
                    map: mapInstance
                });

                prevMarkerRef.current = marker;

                let markerPosition = marker.getPosition();
                let geocoder = new mapApi.Geocoder();
                geocoder.geocode({ 'latLng': markerPosition }, (results, status) => {
                    if (status === mapApi.GeocoderStatus.OK) {
                        if (results) {
                            mapInstance.setCenter(markerPosition);
                        }
                    }
                })
            }
        }
    }

    React.useEffect(() => {
        findLocationByXY(myPosition.lat, myPosition.lng);
    }, [mapApiLoaded, myPosition])
    //#endregion 單點地圖定位=>google map控制

    return (
        <div style={{ width: '700px', height: '350px' }}>
            <GoogleMapReact
                bootstrapURLKeys={{ key: mapDefaultProps.current.key }}
                defaultCenter={mapDefaultProps.current.center}
                defaultZoom={mapDefaultProps.current.zoom}
                yesIWantToUseGoogleMapApiInternals // 設定為 true
                onGoogleApiLoaded={({ map, maps }) => apiHasLoaded(map, maps)}
                options={mapOption}
            >
            </GoogleMapReact>
        </div>
    )
}

export default ProjectBasicMap;