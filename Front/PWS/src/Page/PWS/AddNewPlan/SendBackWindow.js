import React, { useState, useRef, useEffect } from "react";
import { Button } from '@progress/kendo-react-buttons';
import { TextArea } from '@progress/kendo-react-inputs';
import WindowBox from '../../../Components/Dialogs/WindowBox';
import { showGlobalMessageBox } from "../../../Route/RootMiddleware";
import { SendBackProject, SendBackProjectNO } from "../GpReviewProject/GpReviewProjectService";

const SendBackWindow = (props) => {
  const { closeWindow, PlanNoList, BackLogFrom, SendBackType } = props;
  const [backReason, setBackReason] = useState("");  // 退件原因的状态

  /**
   * 退回計畫
   */
  const back = async () => {
    let ReviewListModel = PlanNoList.map((planNo) => ({
      BACK_REASON: backReason,
      PlanNo: planNo,
      SOURCE_TYPE: BackLogFrom,
      ACTION_TYPE: "B"
    }));
    let result = "";
    if (SendBackType == "sendPlan") {
      result = await SendBackProject(ReviewListModel);
    }
    else {
      result = await SendBackProjectNO(ReviewListModel);
    }
    if (result.success) {
      showGlobalMessageBox(result.message, () => { closeWindow(); window.location.reload(); });
    }
    else {
      showGlobalMessageBox(result.message);
    }
  };

  return (
    <WindowBox
      width={70}
      height={40}
      onClose={props.closeWindow}
      title={"退回計畫原因"}
      style={{ marginTop: "30px" }}
    >
      <div style={{ margin: "10px" }}>
        <Button onClick={back}>
          退回
        </Button>
      </div>
      <TextArea
        label="退件原因"
        value={backReason}
        onChange={(e) => { setBackReason(e.value); }}
        name="BACK_REASON"

        rows={5}
      />
    </WindowBox>
  );
};

export default SendBackWindow;
