const stateEnterActions = {
    1: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.subNode('Text').setText(`This experiment...`);
        } }
    ],
    2: [
        { type: "exec", action: () => {
            $.subNode('Text').setText(`Try striking the Taiko freely for 15 seconds`);
        } }
    ],
    3: [
        { type: "exec", action: () => {
            $.subNode('Text').setText(`Play the Taiko freely for 1 minute.`);
        } }
    ],
    4: [
        { type: "exec", action: () => {
            $.subNode('Text').setText(`Take a break for 30 seconds.`);
        } }
    ],
    5: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ],
    6: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.subNode('Text').setText(`Thank you for the participation!`);
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