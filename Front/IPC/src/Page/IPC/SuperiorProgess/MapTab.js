import React from 'react';
import GoogleMapReact from 'google-map-react';
import Pin from './Pin';

const MapTab = (props) => {
    const { listData } = props;
    const [pinData, setPinData] = React.useState(listData)

    //#region 單點地圖定位=>google map控制
    const mapDefaultProps = React.useRef({
        key: process.env.REACT_APP_GOOGLE_API_KEY,
        zoom: 14, // 縮放視角
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
            map: null
        });
        prevMarkerRef.current = marker;
        setMapInstance(map);
        setMapApi(maps);
        setMapApiLoaded(true);
        return marker;
    };

    // 根據xy找地點
    const findLocationByXY = async (X_COORD, Y_COORD, PRG_OFFSET, ENGNEER_STAGE) => {
        if (!mapApiLoaded) {
            return;
        }

        //移除當下位置的marker
        prevMarkerRef.current.setMap(null);

        let marker = new mapApi.Marker({
            position: { lat: parseFloat(X_COORD), lng: parseFloat(Y_COORD) },
            map: null
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

        setPinData([{ X_COORD: X_COORD, Y_COORD: Y_COORD, PRG_OFFSET: PRG_OFFSET, ENGNEER_STAGE: ENGNEER_STAGE }])
    }
    //#endregion 單點地圖定位=>google map控制

    React.useEffect(() => {
        setPinData(listData)
    }, [listData])


    return (
        <div style={{ backgroundColor: '#E0E0E0', padding: '5px 10px' }}>
            <h5>執行進度GIS</h5>
            <div style={{ height: '500px', width: '100%', display: 'flex', backgroundColor: 'white' }}>
                <div style={{ width: '20%', marginRight: '10px', padding: '5px', minWidth: '120px', overflowY: 'auto' }}>
                    <table>
                        {
                            listData.length > 0 &&
                            listData.map((item) => (
                                <tr>
                                    <td style={{ borderBottom: '1px solid #BEBEBE' }}>
                                        <a href='/' onClick={(e) => { e.preventDefault(); findLocationByXY(item.X_COORD, item.Y_COORD, item.PRG_OFFSET, item.ENGNEER_STAGE) }}>{item.PROJECT_NAME}</a>
                                    </td>
                                </tr>
                            ))
                        }
                    </table>
                </div>
                <GoogleMapReact
                    bootstrapURLKeys={{ key: mapDefaultProps.current.key }}
                    defaultCenter={mapDefaultProps.current.center}
                    defaultZoom={mapDefaultProps.current.zoom}
                    yesIWantToUseGoogleMapApiInternals // 設定為 true
                    onGoogleApiLoaded={({ map, maps }) => apiHasLoaded(map, maps)}
                    options={mapOption}
                >
                    {pinData.map(item => { return <Pin lat={item.X_COORD} lng={item.Y_COORD} item={item} /> })}
                </GoogleMapReact>
            </div>
        </div>
    )
}
export default MapTab;