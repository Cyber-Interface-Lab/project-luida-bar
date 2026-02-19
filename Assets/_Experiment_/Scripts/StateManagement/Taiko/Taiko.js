const stateEnterActions = {
    2: [
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: (deltaTime) => {
            PARTICIPANTS[1].setMoveSpeedRate(0.1);
            PARTICIPANTS[1].setPosition(new Vector3(0, 0, -2));
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    2: [
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', false);
        } },
        { type: "exec", action: (deltaTime) => {
            PARTICIPANTS[1].setMoveSpeedRate(1);
        } }
    ]
};