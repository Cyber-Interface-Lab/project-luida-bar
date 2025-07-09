const stateEnterActions = {
    2: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            PARTICIPANTS[0].setMoveSpeedRate(0.1);
            PARTICIPANTS[0].setPosition(new Vector3(0, 0, -2));
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    2: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ]
};