$.onStart(() => {
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        if (item.id === "5570182165721890090") { // ConditionManager
            item.send("exp_conditionDependentObject", true);
        }
    });

    $.state.timer = 0;
    $.state.justTouched = false;
    $.state.handOffset = new Quaternion().setFromEulerAngles(new Vector3(0, 90, 0));
})

$.onUpdate(() => {
    if ($.getStateCompat("this", "exp_conditionChanged", "boolean")) {
        $.setStateCompat("this", "exp_conditionChanged", false);
        $.state.currentCondition = $.state.conditions[$.getStateCompat("global", "exp_conditionID", "integer")];
        onConditionChanged();
    }

    tick();
})

$.onReceive((messageType, arg, sender) => {
    if (messageType === "exp_updateConditions") {
        $.state.conditions = arg;
        $.setStateCompat("this", "exp_conditionChanged", true);
    }
})

// Execution when condition changed
function onConditionChanged () {
    if (!$.state.player || !$.state.originPos || !$.state.targetPos) {
        $.state.player = $.getPlayersNear($.getPosition(), Infinity)[0];
        $.state.originPos = $.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.3, 0.3));
        $.state.targetPos = $.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.3, 0.6));
    }
    $.log($.state.originPos);
    $.log($.state.targetPos);
    $.state.gain = 1;
    $.subNode("Sphere").setPosition($.state.originPos);
    $.subNode("DebugText").setText($.state.currentCondition["gain"]);
}

// Real-time execution depending on current condition
function tick () {
    if (!$.state.player || !$.state.originPos) return;
    $.subNode("RightHandAnchor").setPosition(
        $.state.originPos.clone()
            .add($.state.player.getHumanoidBonePosition(HumanoidBone.RightHand).clone()
                .sub($.state.originPos)
                .multiplyScalar($.state.gain || 1)));
    $.subNode("RightHandAnchor").setRotation($.state.player.getHumanoidBoneRotation(HumanoidBone.RightHand).clone().multiply($.state.handOffset));
    
    if ($.state.justTouched) {
        $.state.timer = $.state.timer + 1;
        if ($.state.timer > 10) {
            $.state.justTouched = false;
            $.state.timer = 0;
        } else {
            $.setStateCompat("this", "isSphereTouched", false);
        }
    } else if ($.getStateCompat("this", "isSphereTouched", "boolean")) {
        $.state.justTouched = true;
        $.setStateCompat("this", "isSphereTouched", false);
        if ($.state.isReaching) {
            onTargetTouched();
        } else {
            onOriginTouched();
        }
    }
}

function onOriginTouched () {
    $.subNode("Sphere").setPosition($.state.targetPos);
    $.state.gain = $.state.currentCondition["gain"] ? parseFloat($.state.currentCondition["gain"]) : 1;
    $.state.isReaching = true;
}

function onTargetTouched () {
    $.sendSignalCompat("this", "state_triggerTransition");
    $.state.isReaching = false;
}