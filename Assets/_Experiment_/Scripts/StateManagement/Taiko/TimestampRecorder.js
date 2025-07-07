const stateEnterActions = {
    3: [
        { type: "exec", action: () => {
            $.state.startTime = Date.now();
        } }
    ],
    5: [
        { type: "exec", action: () => {
            $.sendSignalCompat('this', 'exp_uploadCustomData');
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    3: [
        { type: "exec", action: () => {
            $.state.endTime = Date.now();
            SendDataToCollector("timestamp", {
              start: $.state.startTime,
              end: $.state.endTime
            });
        } }
    ],
    4: [
        { type: "exec", action: () => {
            $.sendSignalCompat('this', 'exp_recordCustomData');
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