$.onInteract((player) => {
    if (!$.groupState.quests) return;
    const triggerQuestID = $.getStateCompat("this", "triggerQuest", "integer");
    const quest = $.groupState.quests[triggerQuestID];
    if (!quest) return;

    $.setStateCompat("owner", "DisplayQuestInfo", true);
    const worldItemTemplateId = new WorldItemTemplateId("questInfoText");
    const questInfoText = $.createItem(worldItemTemplateId, new Vector3(0, 1.65, 1.7), new Quaternion());
    const eID = quest.eID;
    questInfoText.send("onCreate", { player, eID, token: TOKEN, endpointId: ENDPOINT_ID, isTest: IS_TEST });

    let questIdByPlayerID = { ...$.groupState.questIdByPlayerID };
    questIdByPlayerID[player.id] = ($.groupState.currentPage - 1) * 30 + triggerQuestID + 1;
    $.groupState.questIdByPlayerID = questIdByPlayerID;
})
