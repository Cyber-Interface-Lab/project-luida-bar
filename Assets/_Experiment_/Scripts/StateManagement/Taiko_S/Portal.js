const stateEnterActions = {
    5: [
        { type: "sleep", value: 10 },
        { type: "exec", action: () => {
            $.setPosition(new Vector3(0, 0, -2))
        } },
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
};


// function Start() { }
// function Update(deltaTime) { }
// $.onCollide((collision) => { });
// $.onGrab((isGrab, isLeftHand, player) => { });
// $.onInteract((player) => { });
// $.onUse((isDown, player) => { });
// $.onPhysicsUpdate((deltaTime) => { });
// $.onReceive((messageType, arg, sender) => { });