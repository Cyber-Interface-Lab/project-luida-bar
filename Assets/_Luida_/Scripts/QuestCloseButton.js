$.onInteract((player) => {
    $.setStateCompat("owner", "DisplayQuestInfo", false);
    const currentQuestID = $.groupState.questIdByPlayerID[player.id];
    if (currentQuestID && currentQuestID > 0) {
        $.setStateCompat("owner", "DropWorldGate" + currentQuestID, false);
    }
})
