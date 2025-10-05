const numberPerPage = 30;

$.onStart(() => {
  $.state.allQuestsCount = 0;
  setCurrentPage(1);
  $.groupState.quests = [];
  $.groupState.questIdByPlayerID = {};
  $.state.sentRequestsCount = 0;
  requestQuestList();
});

$.onExternalCallEnd((res, meta, err) => {
  if (res == null) {
    $.log(meta + " callExternal ERROR: " + err);
    return;
  }

  if (meta === "getQuestList") {
    let parsedRes = JSON.parse(res);

    const lastQuestsCount = $.groupState.quests.length || 0;
    const filteredQuests = parsedRes.quests.filter(
      (quest) => quest.isTest === IS_TEST
    ).reverse();
    const quests = [ ...$.groupState.quests ];
    $.log("getQuestList retrieved " + quests.length + " experiments");
    $.groupState.quests = [ ...filteredQuests, ...quests ];

    if (filteredQuests.length >= 30 && $.groupState.quests.length < numberPerPage) {
      $.state.sentRequestsCount += 1;
      requestQuestList($.state.sentRequestsCount);
    } else {
      $.state.sentRequestsCount = 0;
    }

    $.state.allQuestsCount = $.groupState.quests.length;

    // 全ページ数を計算してUIに反映
    $.subNode("AllPagesNumber").setText(
      Math.ceil($.state.allQuestsCount / numberPerPage)
    );

    for (let i = 0; i < Math.min(filteredQuests.length, numberPerPage); i++) {
      $.log("Exp " + i + ": " + JSON.stringify(filteredQuests[i]));
      const questTitle = $.subNode("Quest_" + (i + lastQuestsCount));
      if (questTitle) {
        var titleStr = (!!filteredQuests[i].isAccessible ? "" : "[準備中]") + filteredQuests[i].title;
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
      sender.send("quest_board_current_page", $.groupState.currentPage);
      break;
    default:
      break;
  }
});

function requestQuestList(i = 0) {
  if (i === 0) $.groupState.quests = [];
  let request = {
    type: "questList",
    page: ($.groupState.currentPage - 1) * 3 + i + 1,
    number: 30,
    token: TOKEN,
    isTest: IS_TEST
  };

  $.callExternal(new ExternalEndpointId(ENDPOINT_ID), JSON.stringify(request), "getQuestList");
}

function toPrev() {
  if ($.groupState.currentPage <= 1) return;
  setCurrentPage($.groupState.currentPage - 1);
  requestQuestList();
}

function toNext() {
  if ($.groupState.currentPage >= Math.ceil($.state.allQuestsCount / numberPerPage))
    return;
  setCurrentPage($.groupState.currentPage + 1);
  requestQuestList();
}

function setCurrentPage(page) {
  $.groupState.currentPage = page;
  $.subNode("CurrentPageNumber").setText($.groupState.currentPage);
}
