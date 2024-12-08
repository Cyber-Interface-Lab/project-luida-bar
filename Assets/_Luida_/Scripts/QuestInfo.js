const numberPerPage = 5;

$.onStart(() => {
    $.state.currentQuestBoardPage = 1;
    $.state.isLoading = false;
    $.setStateCompat("owner", "AllowJoinExp", true);
    $.getItemsNear($.getPosition().clone(), 0.1).forEach(item => {
        item.send("get_quest_board", true);
    });
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
        $.subNode("Prerequisite").setText(quest.prerequisite);

        $.setStateCompat("this", "AllowJoinExp", +quest.playersCount === 0);
        $.setStateCompat("owner", "triggerQuest", -1);

        $.state.isLoading = false;
    }
});

$.onReceive((messageType, arg, sender) => {
    switch (messageType) {
        case "return_quest_board":
            $.state.questBoard = sender;
            break;
        case "quest_board_current_page":
            $.state.currentQuestBoardPage = arg;
            sendQuestInfoRequest();
            break;
        default:
            break;
    }
});

function sendQuestInfoRequest () {
    let request = {type: "questInfo", id: ($.state.currentQuestBoardPage - 1) * numberPerPage + $.getStateCompat("owner", "triggerQuest", "integer")};
    $.callExternal(JSON.stringify(request), "getQuestInfo");
}