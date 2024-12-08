const numberPerPage = 30;

$.onStart(() => {
  $.state.allQuestsCount = 0;
  setCurrentPage(1);
  $.state.quests = [];
  requestQuestList();
});

$.onExternalCallEnd((res, meta, err) => {
  if (res == null) {
    $.log("callExternal ERROR: " + err);
    return;
  }

  if (meta === "getQuestList") {
    let parsedRes = JSON.parse(res);
    // isTest = true のクエストのみフィルタリング
    const filteredQuests = parsedRes.quests.filter(
      (quest) => quest.isTest === false
    );
    $.state.quests = filteredQuests;

    $.state.allQuestsCount = filteredQuests.length;

    // 全ページ数を計算してUIに反映
    $.subNode("AllPagesNumber").setText(
      Math.ceil($.state.allQuestsCount / numberPerPage)
    );

    for (let i = 0; i < Math.min($.state.allQuestsCount, numberPerPage); i++) {
      const questTitle = $.subNode("Quest_" + i);
      if (questTitle) {
        var titleStr = filteredQuests[i].title;
        if (titleStr.length > 16) {
          titleStr =
            titleStr.substring(0, 16) +
            "\n" +
            titleStr.substring(16, titleStr.length);
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
    case "quest_board_get_current_page":
      sender.send("quest_board_current_page", $.state.currentPage);
      break;
    default:
      break;
  }
});

function requestQuestList() {
  let request = {
    type: "questList",
    page: $.state.currentPage,
    number: numberPerPage,
    isTest: false
  };
  $.callExternal(JSON.stringify(request), "getQuestList");
}

function toPrev() {
  if ($.state.currentPage <= 1) return;
  setCurrentPage($.state.currentPage - 1);
  requestQuestList();
}

function toNext() {
  if ($.state.currentPage >= Math.ceil($.state.allQuestsCount / numberPerPage))
    return;
  setCurrentPage($.state.currentPage + 1);
  requestQuestList();
}

function setCurrentPage(page) {
  $.state.currentPage = page;
  $.subNode("CurrentPageNumber").setText($.state.currentPage);
}
