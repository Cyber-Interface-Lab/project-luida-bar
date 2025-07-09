const stateEnterActions = {
    3: [
        { type: "exec", action: () => {
            $.setPosition(new Vector3(0, 0, -2))
        } },
        { type: "exec", action: () => {
            if (CONDITION['selfAvatar'] === 'happi') {
              PARTICIPANTS[0].setMoveSpeedRate(0.1);
              PARTICIPANTS[0].setPosition(new Vector3(0, 0, -2));
            }
        } },
        { type: "sleep", value: 10 },
        { type: "exec", action: () => {
            if (CONDITION['selfAvatar'] === 'happi') {
              $.setStateCompat('this', 'exp_showItem', true);
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