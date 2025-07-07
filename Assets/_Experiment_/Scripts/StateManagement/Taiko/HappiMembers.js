const stateEnterActions = {
    3: [
        { type: "exec", action: () => {
            if (CONDITION['otherAvatar'] === 'happi') {
              $.setStateCompat('this', 'exp_showItem', true);
            }
        } },
        { type: "exec", action: () => {
            let n = parseInt(CONDITION['number']);
            for (let i = 1; i <= 99; i++) {
              $.subNode("other_" + i)
                .setEnabled(i < n);
            }
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    3: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ]
};


// function Start() { }
// function Update(deltaTime) { }
// $.onCollide((collision) => { });
// $.onGrab((isGrab, isLeftHand, player) => { });
// $.onInteract((player) => { });
// $.onUse((isDown, player) => { });
// $.onPhysicsUpdate((deltaTime) => { });
// $.onReceive((messageType, arg, sender) => { });