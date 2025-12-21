import * as React from 'react';
import TwDatePicker from '../Components/DateInputs/TwDatePicker'

export const Demo =()=>{
    const [announcementData, setAnnouncementData] = React.useState({
       
        EFFECTIVE_DATE: new Date(),
        EXPIRE_DATE: new Date(),
        ATTACH_NAME: []
    });
        return (
        
              <TwDatePicker
                name="EFFECTIVE_DATE"
                format={"yyy/MM/dd"}
                
            />
        );
    
}
