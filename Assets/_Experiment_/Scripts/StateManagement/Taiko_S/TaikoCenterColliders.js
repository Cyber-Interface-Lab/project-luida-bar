const stateEnterActions = {
    2: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.setStateCompat('this', 'hits', 0);
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    2: [
        { type: "exec", action: () => {
            SendDataToCollector(
              'centerHits',
              $.getStateCompat('this', 'hits', 'integer')
            )
        } },
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ]
};


function Start() { $.setStateCompat('this', 'hits', 0); }