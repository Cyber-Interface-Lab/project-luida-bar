const numberPerPage = 30;

$.onStart(() => {
    $.state.questBoard = $.worldItemReference("QuestBoard");
    $.state.currentQuestBoardPage = 1;
    $.state.isLoading = false;
    $.setStateCompat("owner", "AllowJoinExp", true);
})

$.onUpdate(() => {
    if ($.state.isLoading) return;

    if ($.state.questBoard && $.getStateCompat("owner", "triggerQuest", "integer") >= 0) {
        $.log("load new quest info: " + $.getStateCompat("owner", "triggerQuest", "integer"));
        $.state.isLoading = true;
        $.state.questBoard.send("quest_board_get_current_page", true);
        $.subNode("Title").setText("Loading...");
        $.subNode("Description").setText("");
        $.subNode("Prerequisite").setText("");
        $.subNode("Reward").setText("");
    }
});

$.onExternalCallEnd((res, meta, err) =>
{
    if (res == null) {
        $.log("callExternal ERROR: " + err);
        $.setStateCompat("owner", "triggerQuest", -1);
        if (meta === "getQuestInfo") $.setStateCompat("this", "AllowJoinExp", false);
        return;
    }

    if (meta === "getQuestInfo") {
        let parsedRes = JSON.parse(res);
        $.state.quests = parsedRes.quests;
        const quest = parsedRes.quest;

        const title = insertLineBreaks(quest.title, 50);
        $.subNode("Title").setText(title);
        const description = insertLineBreaks(quest.description, 70);
        $.subNode("Description").setText(description);
        const prerequisite = insertLineBreaks(quest.prerequisite, 70);
        $.subNode("Prerequisite").setText("参加条件：" + prerequisite);

        $.subNode("Reward").setText("報酬：" + quest.reward);
        $.setStateCompat("owner", "currentQuestID", ($.state.currentQuestBoardPage - 1) * numberPerPage + $.getStateCompat("owner", "triggerQuest", "integer")) + 1;

        // $.setStateCompat("this", "AllowJoinExp", +quest.playersCount === 0);
        $.setStateCompat("this", "AllowJoinExp", true);
        $.setStateCompat("owner", "triggerQuest", -1);

        $.state.isLoading = false;
    }
});

$.onReceive((messageType, arg, sender) => {
    switch (messageType) {
        case "quest_board_current_page":
            $.state.currentQuestBoardPage = arg;
            sendQuestInfoRequest();
            break;
        default:
            break;
    }
});

function sendQuestInfoRequest () {
    if (!$.groupState.quests || !$.groupState.quests[$.getStateCompat("owner", "triggerQuest", "integer")]) {
        $.setStateCompat("owner", "triggerQuest", -1);
        $.state.isLoading = false;
        $.setStateCompat("owner", "DisplayQuestInfo", false);
        return;
    }
    let request = {
        type: "questInfo",
        id: $.groupState.quests[$.getStateCompat("owner", "triggerQuest", "integer")].eID, // ($.state.currentQuestBoardPage - 1) * numberPerPage + $.getStateCompat("owner", "triggerQuest", "integer"),
        token: TOKEN,
        isTest: IS_TEST
    };
    $.callExternal(JSON.stringify(request), "getQuestInfo");
}

function insertLineBreaks(str, maxLength = 70) {
    let currentLength = 0;
    let result = '';
    
    for (let char of str) {
      // Check if the character is full-width or half-width
      currentLength += char.match(/[^\x00-\x7F]/) ? 2 : 1;
      
      // If the accumulated length exceeds maxLength, insert a line break
      if (currentLength > maxLength) {
        result += '\n';
        currentLength = char.match(/[^\x00-\x7F]/) ? 2 : 1;
      }
      
      result += char;
    }
    
    return result;
}
