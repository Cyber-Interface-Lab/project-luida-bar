const xyCoordCandidates = [
    [{ x: 10, y: 10 }, { x: -10, y: 10 }, { x: 10 , y: -10 }, { x: -10, y: -10 }],
    [{ x: 30, y: 0  }, { x: 0  , y: 30 }, { x: -30, y: 0   }, { x: 0  , y: -30 }],
    [{ x: 30, y: 30 }, { x: -30, y: 30 }, { x: 30 , y: -30 }, { x: -30, y: -30 }],
    [{ x: 50, y: 0  }, { x: 0  , y: 50 }, { x: -50, y: 0   }, { x: 0  , y: -50 }],
    [{ x: 50, y: 50 }, { x: -50, y: 50 }, { x: 50 , y: -50 }, { x: -50, y: -50 }]
];

$.onStart(() => {
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        if (item.id === "5570182165721890090") { // ConditionManager
            item.send("exp_conditionDependentObject", true);
        }
    });

    [2, 8, 16, 48].map(size => $.subNode("Target_" + size)).forEach(target => {
        target.setEnabled(false);
    });
    $.state.timer = 0;
    $.state.isTaskStarted = false;
    $.state.size = "";
})

$.onUpdate((deltaTime) => {
    if ($.getStateCompat("this", "exp_conditionChanged", "boolean")) {
        $.setStateCompat("this", "exp_conditionChanged", false);
        $.state.currentCondition = $.state.conditions[$.getStateCompat("global", "exp_conditionID", "integer")];
        onConditionChanged();
    }

    tick(deltaTime);
})

$.onReceive((messageType, arg, sender) => {
    if (messageType === "exp_updateConditions") {
        $.state.conditions = arg;
        $.setStateCompat("this", "exp_conditionChanged", true);
    }
})

// Execution when condition changed
function onConditionChanged () {
    let xyCoord = xyCoordCandidates[parseInt($.state.currentCondition["xy"])][Math.floor(Math.random() * 4)];
    $.setStateCompat("owner", "x", xyCoord.x);
    $.setStateCompat("owner", "y", xyCoord.y);
    let x = xyCoord.x / 300;
    let y = xyCoord.y / 300;
    let z = parseInt($.state.currentCondition["d"]) / 300;
    $.state.targetName = "Target_" + $.state.currentCondition["s"];

    $.subNode("Reset").setEnabled(true);
    $.subNode($.state.targetName).setPosition($.subNode("Reset").getPosition().clone().add(new Vector3(x, y, z)));
}

// Real-time execution depending on current condition
function tick (deltaTime) {
    if ($.state.isTaskStarted) {
        $.state.timer = $.state.timer + deltaTime;
    }

    if ($.getStateCompat("this", "isTargetSelected", "boolean")) {
        $.setStateCompat("this", "isTargetSelected", false);
        if ($.state.isTaskStarted) {
            onTargetSelected();
        } else {
            Reset();
        }
    }
}

function Reset() {
    $.subNode("Reset").setEnabled(false);
    $.subNode($.state.targetName).setEnabled(true);
    $.state.timer = 0;
    $.state.isTaskStarted = true;
}

function onTargetSelected() {
    $.state.isTaskStarted = false;
    $.subNode($.state.targetName).setEnabled(false);
    $.setStateCompat("owner", "spentTime", $.state.timer);
    $.sendSignalCompat("this", "recordSpentTime");
}

function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
}

function extendArray(array, times) {
    let extendedArray = [];
    for (let i = 0; i < times; i++) {
        extendedArray = extendedArray.concat(array);
    }
    return extendedArray;
}