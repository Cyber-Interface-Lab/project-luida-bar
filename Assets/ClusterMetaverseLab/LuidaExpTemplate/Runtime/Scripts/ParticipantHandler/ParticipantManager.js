$.onStart(() => {
  $.groupState.isParticipantsEnough = false;
  $.groupState.sessionID = Date.now() + "_" +  (Math.random() + 1).toString(36).substring(7);
  $.groupState.participants = []; // array of PlayerHandle who are currently in the experiment
  $.state.participantsEnvInfo = [];
  $.state.timer = 0;
  
  // TODO: load exp info so that we can check later what environments this experiment requires
})

$.onUpdate((deltaTime) => {
    if (!$.groupState.isParticipantsEnough) {
        $.state.timer += deltaTime;
        if ($.state.timer >= 1){
            $.state.timer = 0;
            let pIdfcs = $.groupState.participants.map(p => p.idfc);
            const newPlayers = $.getPlayersNear($.getPosition(), Infinity)
                .filter(p => !pIdfcs.includes(p.idfc));
            if (newPlayers.length > 0) {
                for (const newPlayer of newPlayers) {
                    // TODO: Check if the player is eligible to join the experiment before adding them to $.groupState.participants.
                    // $.groupState.participants = [ ...$.groupState.participants, newPlayer ];
                    $.setPlayerScript(newPlayer);
                    newPlayer.send("initializeParticipant", true);
                }
            }
        }
    }
/*
  if ($.getStateCompat("this", "exp_checkJoinEligibility", "boolean")) {
    $.setStateCompat("this", "exp_checkJoinEligibility", false);

    let pIdfcs = $.groupState.participants.map(p => p.idfc);
    const newPlayers = $.getPlayersNear($.getPosition(), 1)
      .filter(p => !pIDFCs.includes(p.idfc));
    
    if (newPlayers.length > 0) {
      $.state.newPlayers = newPlayers;
      $.log(newPlayers[0].idfc);

      const request = {
        type: "checkJoinEligibility",
        token: token || "",
        eID: expID || "",
        idfcs: newPlayers.map(p => p.idfc).join("|")
      };
      $.callExternal(new ExternalEndpointId(callExternalEndpointID), JSON.stringify(request), "joinEligibilityChecked");
    }
  }
*/
})

$.onReceive((messageType, arg, sender) => {
    switch (messageType) {
        case "envInfoResponse":
            if (arg.isVr) {
                $.groupState.participants = [ ...$.groupState.participants, sender ];
                $.state.participantsEnvInfo = [
                    ...$.state.participantsEnvInfo,
                    {
                        idfc: sender.idfc,
                        envInfo: arg
                    }
                ];
                
                if ($.state.participantsEnvInfo.length >= pNum) {
                    let request = {
                        type: "uploadCustomData",
                        data: { envInfo: $.state.participantsEnvInfo },
                        token: token || "",
                        eID: expID || "",
                        pID: $.groupState.sessionID, // TODO: change 'pID' to 'sessionID' 
                    };
                    $.callExternal(new ExternalEndpointId(callExternalEndpointID), JSON.stringify(request), "customDataUploaded");
                }
                
                if (!$.groupState.isParticipantsEnough && $.groupState.participants.length >= pNum) {
                    HandleParticipantsEnough();
                }
            } else {
                $.subNode("WorldGateToLuidaBar").setEnabled(true);
            }
            
            break;
        default:
            break;
    }
}, { item: true, player: true });

function HandleParticipantsEnough() {
  $.log("Participants are enough to start the experiment.");
  $.groupState.isParticipantsEnough = true;
    $.sendSignalCompat("this", "exp_playersAreEnough");
    $.sendSignalCompat("this", "exp_StartStateTransition");

  const conditionManager = $.worldItemReference("ConditionManager");
  if (conditionManager) {
    conditionManager.send("luida_participants_info", {
      participants: $.groupState.participants,
      sessionID: $.groupState.sessionID
    });
  }
}

/*
$.onExternalCallEnd((res, meta, err) =>
{
  if (res == null) {
    $.log("callExternal ERROR: " + err);
    return;
  }

  if (meta === "joinEligibilityChecked") {
    const parsedRes = JSON.parse(res);
    $.state.newPlayers.forEach(newPlayer => {
      if (parsedRes.ineligibleIdfcs.includes(newPlayer.idfc)) {
        // TODO: Show message about that this player is not eligible to join this experiment, and teleport the player back to LUIDA bar world.
      } else {
        newPlayer.setMoveSpeedRate(1);
        $.groupState.participants = [ ...$.groupState.participants, newPlayer ];
        // TODO: Instead of enabling move, teleport the player from the checking area to the task area.
      }
    });
    
    $.state.newPlayers = [];
    $.log("Only users who have not joined this experiment before are allowed to proceed to the experiment.");
  }
});
*/
