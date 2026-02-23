const stateEnterActions = {
    3: [
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: (deltaTime) => {
            $.state.isInTask = true;
            $.state.hits = 0;
        } }
    ],
    6: [
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    3: [
        { type: "exec", action: (deltaTime) => {
            $.state.isInTask = false;
            SendDataToCollector("sideHits", $.state.hits);
        } },
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ]
};


function Start() {
  $.state.hits = 0;
  $.state.isInTask = false;
}
$.onCollide((collision) => {
  if (collision.handle !== null && collision.handle.type === "player") return;
  $.subNode('Collider').getUnityComponent('AudioSource').play();
  if ($.state.isInTask) $.state.hits += 1;
});