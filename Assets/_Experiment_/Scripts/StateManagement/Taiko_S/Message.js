const stateEnterActions = {
    0: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.subNode('Text').setText(`目の前の鏡を見ながら、
            準備体操を30秒間で
            行ってください。`);
        } },
        { type: "exec", action: () => {
            if (PARTICIPANTS) {
              PARTICIPANTS[1].setMoveSpeedRate(0.1);
              PARTICIPANTS[1].setPosition(new Vector3(0, 0, -2));
            }
        } }
    ],
    1: [
        { type: "exec", action: () => {
            $.subNode('Text').setText(`この後出てくる他の人と一緒に
            太鼓演奏を6回（一回1分40秒+休憩30秒）
            で行ってください。
            音楽に合わせて自由に叩いてください。
            他の人に合わせる必要はありません。
            10秒後に始まります。`);
        } },
        { type: "exec", action: () => {
            if (PARTICIPANTS) {
              PARTICIPANTS[1].setMoveSpeedRate(0.1);
              PARTICIPANTS[1].setPosition(new Vector3(0, 0, -2));
            }
        } }
    ],
    2: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.subNode('Text').setText(`太鼓演奏をはじめてください！`);
        } },
        { type: "sleep", value: 3 },
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', false);
        } }
    ],
    3: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.subNode('Text').setText(`30秒休憩`);
        } }
    ],
    5: [
        { type: "exec", action: () => {
            $.setStateCompat('this', 'exp_showItem', true);
        } },
        { type: "exec", action: () => {
            $.subNode('Text').setText(`ご参加ありがとうございました。
            10秒後に自動的に
            ご退室いただきます。`);
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