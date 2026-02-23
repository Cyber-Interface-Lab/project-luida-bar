const stateEnterActions = {
    2: [
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', true);
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    2: [
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