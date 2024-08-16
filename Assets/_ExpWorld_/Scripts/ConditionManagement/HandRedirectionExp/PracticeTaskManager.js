const gains = [1, 0.75, 1.25];

$.onStart(() => {
    $.state.timer = 0;
    $.state.gainId = 0;
    $.state.justTouched = false;
    $.state.handOffset = new Quaternion().setFromEulerAngles(new Vector3(0, 90, 0));
})

$.onUpdate(() => {
    if ($.getStateCompat("this", "state_enter", "boolean")) {
        $.setStateCompat("this", "state_enter", false);
        onTaskStarted();
    }

    tick();
})

function onTaskStarted () {
    if (!$.state.player || !$.state.originPos || !$.state.targetPos) {
        $.state.player = $.getPlayersNear($.getPosition(), Infinity)[0];
        $.state.originPos = $.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.3, 0.3));
        $.state.targetPos = $.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.3, 0.6));
    }
    $.state.gain = 1;
    $.subNode("Sphere").setPosition($.state.originPos);
    $.subNode("DebugText").setText(gains[$.state.gainId]);
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
    $.state.gain = gains[$.state.gainId];
    $.state.isReaching = true;
}

function onTargetTouched () {
    $.state.gainId = $.state.gainId + 1;
    $.sendSignalCompat("this", "state_triggerTransition");
    $.state.isReaching = false;
}