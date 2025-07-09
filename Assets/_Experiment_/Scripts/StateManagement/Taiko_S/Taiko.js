const stateEnterActions = {
    2: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } }
    ],
    4: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', false);
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