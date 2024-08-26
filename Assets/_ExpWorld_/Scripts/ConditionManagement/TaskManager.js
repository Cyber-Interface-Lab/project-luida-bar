$.onStart(() => {
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        if (item.id === "5570182165721890090") { // ConditionManager
            item.send("exp_conditionDependentObject", true);
        }
    });

    $.state.xyCoords = [
        shuffleArray([{ x: 10, y: 10 }, { x: -10, y: 10 }, { x: 10 , y: -10 }, { x: -10, y: -10 }]),
        shuffleArray([{ x: 30, y: 0  }, { x: 0  , y: 30 }, { x: -30, y: 0   }, { x: 0  , y: -30 }]),
        shuffleArray([{ x: 30, y: 30 }, { x: -30, y: 30 }, { x: 30 , y: -30 }, { x: -30, y: -30 }]),
        shuffleArray([{ x: 50, y: 0  }, { x: 0  , y: 50 }, { x: -50, y: 0   }, { x: 0  , y: -50 }]),
        shuffleArray([{ x: 50, y: 50 }, { x: -50, y: 50 }, { x: 50 , y: -50 }, { x: -50, y: -50 }])
    ];
    [2, 8, 16, 48].map(size => $.subNode("Target_" + size)).forEach(target => {
        target.setEnabled(false);
    });
    $.state.timer = 0;
    $.state.isTaskStarted = false;
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
    let xyCoords = $.state.xyCoords;
    let xyCoord = xyCoords[parseInt(currentConditions["xyDistIndex"])].pop();
    $.state.xyCoords = xyCoords;
    $.setStateCompat("owner", "x", xyCoord.x);
    $.setStateCompat("owner", "y", xyCoord.y);
    let x = xyCoord.x / 300;
    let y = xyCoord.y / 300;
    let z = parseInt(currentConditions["depth"]) / 300;
    let size = currentConditions["size"];

    $.subNode("Reset").setEnabled(true);
    $.state.currentTarget = $.subNode("Target_" + size);
    $.state.currentTarget.setPosition($.subNode("Reset").getPosition().clone().add(new Vector3(x, y, z)));
}

// Real-time execution depending on current condition
function tick (deltaTime) {
    if ($.state.isTaskStarted) {
        $.state.timer = $.state.timer + deltaTime;
    }

    if ($.getStateCompat("this", "isBlockSelected", "boolean")) {
        if ($.state.isTaskStarted) {
            onTargetSelected();
        } else {
            Reset();
        }
    }
}

function Reset() {
    $.subNode("Reset").setEnabled(false);
    $.log($.state.currentTarget.name);
    $.log($.state.currentTarget.getPosition());
    $.state.currentTarget.setEnabled(true);
    $.state.timer = 0;
    $.state.isTaskStarted = true;
}

function onTargetSelected() {
    $.state.isTaskStarted = false;
    $.state.currentTarget.setEnabled(false);
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