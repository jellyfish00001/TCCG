import { Window } from '@progress/kendo-react-dialogs';
import React from 'react';
import { WindowResizehook } from '../../../Hook/useWindowResize';
import ProjectFillBasicMain from '../ProjectFillBasic/ProjectFillBasicMain';

const ProjectAddWindow = props => {
    const dimensions = WindowResizehook();
    let { visible, onClose, saveEvent } = props
    let necessaryObj = {
        location: {
            state: {},
        }
    }

    return (
        <>
            {visible &&
                <div className="fullscreen window-fullscreen">
                    <Window
                        title='新增計畫'
                        onClose={() => { onClose() }}
                        initialWidth={dimensions.width > 700 ? 800 : dimensions.width * .7}
                        initialHeight={dimensions.height > 700 ? 500 : dimensions.height * .8}
                        draggable={false}
                        resizable={false}
                        modal={true}
                    >
                        <ProjectFillBasicMain
                            {...necessaryObj}
                            projectAddOnSave={(projectNo) => { saveEvent(projectNo) }}
                        />
                    </Window>
                </div>
            }
        </>
    )
}

export default ProjectAddWindow;