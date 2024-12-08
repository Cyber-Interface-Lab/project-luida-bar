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
        $.state.isLoading = true;
        $.state.questBoard.send("quest_board_get_current_page", true);
    }
});

$.onExternalCallEnd((res, meta, err) =>
{
    if (res == null) {
        $.log("callExternal ERROR: " + err);
        $.setStateCompat("owner", "triggerQuest", -1);
        return;
    }

    if (meta === "getQuestInfo") {
        let parsedRes = JSON.parse(res);
        $.state.quests = parsedRes.quests;
        const quest = parsedRes.quest;

        $.subNode("Title").setText(quest.title);
        $.subNode("Description").setText(quest.description);
        $.subNode("Prerequisite").setText("参加条件：" + quest.prerequisite);
        $.subNode("Reward").setText("報酬：" + quest.reward);
        $.setStateCompat("owner", "currentQuestID", ($.state.currentQuestBoardPage - 1) * numberPerPage + $.getStateCompat("owner", "triggerQuest", "integer")) + 1;

        $.setStateCompat("this", "AllowJoinExp", +quest.playersCount === 0);
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
    let request = {
        type: "questInfo",
        id: ($.state.currentQuestBoardPage - 1) * numberPerPage + $.getStateCompat("owner", "triggerQuest", "integer"),
        isTest: true
    };
    $.callExternal(JSON.stringify(request), "getQuestInfo");
}