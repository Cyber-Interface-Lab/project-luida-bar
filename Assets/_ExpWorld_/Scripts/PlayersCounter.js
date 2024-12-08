const worldID = "67a71729-1985-4b57-bac5-72ff96d1b9b4";
const countPlayersInterval = 60;

$.onStart(() => {
    $.state.timer = 0;
    $.state.lastPlayersCount = 0;
    $.state.currentPlayersCount = 0;
    $.state.isSendingUpdateRequest = false;
})

$.onUpdate(() => {
    $.state.timer = $.state.timer + 1;

    if ($.getStateCompat("global", "PlayerJoined", "boolean")) {
        if ($.state.isSendingUpdateRequest) {
            $.state.timer = 0;
        } else {
            $.sendSignalCompat("this", "ResetPlayerJoinedStatus");
            $.state.timer = countPlayersInterval + 1;
            $.state.isSendingUpdateRequest = true;
        }
    } else if ($.state.isSendingUpdateRequest) {
        $.state.isSendingUpdateRequest = false;
    }

    if ($.state.timer > countPlayersInterval) {
        $.state.timer = 0;
        updatePlayersCount();
    }
})

function updatePlayersCount () {
    $.state.lastPlayersCount = $.state.currentPlayersCount;
    $.state.currentPlayersCount = $.getPlayersNear($.getPosition().clone(), 100).length;
    if ($.state.lastPlayersCount !== $.state.currentPlayersCount) {
        $.callExternal(JSON.stringify({ type: "updatePlayersCount", expIdentifier: worldID, playersCount: $.state.currentPlayersCount }), "playersCountUpdated");
    }
}