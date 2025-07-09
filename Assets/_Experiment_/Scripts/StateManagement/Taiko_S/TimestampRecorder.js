const stateEnterActions = {
    2: [
        { type: "exec", action: () => {
            $.state.startTime = Date.now();
        } }
    ],
    3: [
        { type: "exec", action: () => {
            $.sendSignalCompat('this', 'exp_recordCustomData');
        } }
    ],
    4: [
        { type: "exec", action: () => {
            $.sendSignalCompat('this', 'exp_uploadCustomData');
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    2: [
        { type: "exec", action: () => {
            $.state.endTime = Date.now();
            SendDataToCollector("timestamp", {
              start: $.state.startTime,
              end: $.state.endTime
            });
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