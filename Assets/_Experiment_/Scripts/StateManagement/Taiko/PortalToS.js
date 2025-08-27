const stateEnterActions = {
    3: [
        { type: "exec", action: () => {
            $.setPosition(new Vector3(0, 0, -2))
        } },
        { type: "exec", action: () => {
            if (CONDITION['selfAvatar'] === 'suit') {
              PARTICIPANTS[1].setMoveSpeedRate(0.1);
              PARTICIPANTS[1].setPosition(new Vector3(0, 0, -2));
            }
        } },
        { type: "sleep", value: 10 },
        { type: "exec", action: () => {
            if (CONDITION['selfAvatar'] === 'suit') {
              $.setStateCompat('this', 'exp_showItem', true);
            }
        } },
        { type: "exec", action: () => {
            if (CONDITION['selfAvatar'] === 'suit') {
              $.log(CONDITION);
              if (CONDITION['qAnswers'] && CONDITION['qAnswers']['1'] && CONDITION['qAnswers']['1'][0] === 2) {
                $.subNode('FemaleAvatarPortal').setEnabled(true);
              } else {
                $.subNode('MaleAvatarPortal').setEnabled(true);
              }
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