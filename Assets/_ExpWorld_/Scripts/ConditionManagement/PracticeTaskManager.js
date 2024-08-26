const params = [
    { x: 30, y: 30, z: -25, s: 8 },
    { x: 50, y: 0 , z: 0  , s: 2 },
    { x: 0, y: -50, z: 25, s: 16 }
];

$.onStart(() => {
    [2, 8, 16].map(size => $.subNode("Target_" + size)).forEach(target => {
        target.setEnabled(false);
    });
    $.state.paramId = 0;
    $.state.isTaskStarted = false;
    $.state.size = "";
})

$.onUpdate(() => {
    if ($.getStateCompat("this", "state_enter", "boolean")) {
        $.setStateCompat("this", "state_enter", false);
        startNextTrial();
        $.state.paramId = $.state.paramId + 1;
    }

    tick();
})

function startNextTrial () {
    let id = $.state.paramId;
    let x = params[id].x / 300;
    let y = params[id].y / 300;
    let z = params[id].z / 300;
    $.state.targetName = "Target_" + params[id].s;

    if (!$.state.player) {
        $.state.player = $.getPlayersNear($.getPosition(), Infinity)[0];
        $.subNode("Reset").setPosition($.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, 0, 0.5)));
    }
    $.subNode("Reset").setEnabled(true);
    $.subNode($.state.targetName).setPosition($.subNode("Reset").getPosition().clone().add(new Vector3(x, y, z)));
}

function tick () {
    if ($.getStateCompat("this", "isTargetSelected", "boolean")) {
        $.setStateCompat("this", "isTargetSelected", false);
        if ($.state.isTaskStarted) {
            onTargetSelected();
        } else {
            reset();
        }
    }
}

function reset() {
    $.subNode("Reset").setEnabled(false);
    $.subNode($.state.targetName).setEnabled(true);
    $.state.timer = 0;
    $.state.isTaskStarted = true;
}

function onTargetSelected() {
    $.state.isTaskStarted = false;
    $.subNode($.state.targetName).setEnabled(false);
    $.sendSignalCompat("this", "state_triggerTransition");
}