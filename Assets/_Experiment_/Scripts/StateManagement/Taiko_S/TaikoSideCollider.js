const stateEnterActions = {
    2: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.state.isInTask = true;
            $.state.hits = 0;
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
            $.state.isInTask = false;
            SendDataToCollector("sideHits", $.state.hits);
        } }
    ]
};


function Start() {
  $.state.hits = 0;
  $.state.isInTask = false;
}
$.onCollide((collision) => {
  $.subNode('Collider').getUnityComponent('AudioSource').play();
  if ($.state.isInTask) $.state.hits += 1;
});