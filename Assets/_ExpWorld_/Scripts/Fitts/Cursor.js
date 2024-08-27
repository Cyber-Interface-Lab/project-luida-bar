$.onUpdate(() => {
    if ($.getStateCompat("this", "state_enter", "boolean")) {
        $.setStateCompat("this", "state_enter", false);
        if (!$.state.player) {
            $.state.player = $.getPlayersNear($.getPosition(), Infinity)[0];
        }
        $.setPosition($.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.25, 0.5)));
    }
})