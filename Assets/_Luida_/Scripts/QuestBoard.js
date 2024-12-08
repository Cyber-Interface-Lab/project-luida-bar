// const itemTemplateId = new ItemTemplateId("941a190d-c7a8-42d7-b6c9-bd0b3d127e35");
// const position = $.getPosition().clone();
// const rotation = new Quaternion().identity();
const data = ["Hello", "Cluster", "from", "Cyber", "Lab"];
const numberPerPage = 5;

$.onStart(() => {
    $.state.allQuestsCount = 0;
    setCurrentPage(1);
    $.state.quests = [];
    requestQuestList();
});

$.onExternalCallEnd((res, meta, err) =>
{
    if (res == null) {
        $.log("callExternal ERROR: " + err);
        return;
    }

    if (meta === "getQuestList") {
        let parsedRes = JSON.parse(res);
        $.state.quests = parsedRes.quests;
        $.state.allQuestsCount = parsedRes.allQuestsCount;
        $.subNode("AllPagesNumber").setText(Math.ceil($.state.allQuestsCount / numberPerPage));

        for(let i = 0; i < Math.min(parsedRes.quests.length, numberPerPage); i++)
        {
            const questTitle = $.subNode("Quest_" + i);
            if (questTitle) {
                var titleStr = parsedRes.quests[i].title + "（" + parsedRes.quests[i].playersCount + "人待ち）";
                if (titleStr.length > 16) {
                    titleStr = titleStr.substring(0, 16) + "\n" + titleStr.substring(16, titleStr.length);
                }
                questTitle.setText(titleStr);
            }
        }
    }
});

$.onReceive((messageType, arg, sender) => {
    switch (messageType) {
        case "quest_board_update":
            requestQuestList();
            break;
        case "quest_board_to_next":
            toNext();
            break;
        case "quest_board_to_prev":
            toPrev();
            break;
        case "get_quest_board":
            sender.send("return_quest_board", true);
            break;
        case "quest_board_get_current_page":
            sender.send("quest_board_current_page", $.state.currentPage);
            break;
        default:
            break;
    }
})

function requestQuestList () {
    let request = {type: "questList", page: $.state.currentPage, number: numberPerPage};
    $.callExternal(JSON.stringify(request), "getQuestList");
}

function toPrev () {
    if ($.state.currentPage <= 1) return;
    setCurrentPage($.state.currentPage - 1);
    requestQuestList();
}

function toNext () {
    if ($.state.currentPage >= Math.ceil($.state.allQuestsCount / numberPerPage)) return;
    setCurrentPage($.state.currentPage + 1);
    requestQuestList();
}

function setCurrentPage (page) {
    $.state.currentPage = page;
    $.subNode("CurrentPageNumber").setText($.state.currentPage);
}