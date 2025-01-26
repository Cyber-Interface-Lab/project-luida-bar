$.onStart(() => {
    $.setStateCompat("owner", "DisplayQuestInfo", true);
})

$.onUpdate(() => {
    if (!$.getStateCompat("owner", "DisplayQuestInfo", "boolean")) $.destroy();
})

$.onReceive((messageType, arg, sender) => {
    switch (messageType) {
        case "onCreate":
            $.setVisiblePlayers([arg.player]);
            $.requestOwner(arg.player);
            $.setStateCompat("this", "show", true);
            sendQuestInfoRequest(arg.eID, arg.token, arg.isTest);
            break;
        default:
            break;
    }
});


$.onExternalCallEnd((res, meta, err) => {
    if (res == null) {
        $.log("callExternal ERROR: " + err);
        // $.setStateCompat("owner", "triggerQuest", -1);
        if (meta === "getQuestInfo") $.destroy();
        return;
    }

    if (meta === "getQuestInfo") {
        let parsedRes = JSON.parse(res);
        receiveText(parsedRes.quest);
    }
});

function sendQuestInfoRequest(eID, token, isTest) {
    $.subNode("Title").setText("Loading...");
    $.subNode("Description").setText("");
    $.subNode("Prerequisite").setText("");
    $.subNode("Reward").setText("");

    let request = {
        type: "questInfo",
        id: eID,
        token: token,
        isTest: isTest
    };
    $.callExternal(JSON.stringify(request), "getQuestInfo");
}

function receiveText(quest, currentQuestID) {
    const title = insertLineBreaks(quest.title, 50);
    $.subNode("Title").setText(title);
    const description = insertLineBreaks(quest.description, 70);
    $.subNode("Description").setText(description);
    const prerequisite = insertLineBreaks(quest.prerequisite, 70);
    $.subNode("Prerequisite").setText("参加条件：" + prerequisite);

    $.subNode("Reward").setText("報酬：" + quest.reward);
    // $.setStateCompat("owner", "currentQuestID", ($.state.currentQuestBoardPage - 1) * numberPerPage + $.getStateCompat("owner", "triggerQuest", "integer")) + 1;
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
