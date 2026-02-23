const stateEnterActions = {
};

const duringStateActions = {
};

const stateExitActions = {
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