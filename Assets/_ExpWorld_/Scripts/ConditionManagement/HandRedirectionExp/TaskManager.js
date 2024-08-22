$.onStart(() => {
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        if (item.id === "5570182165721890090") { // ConditionManager
            item.send("exp_conditionDependentObject", true);
        }
    });

    $.state.timer = 0; // タイマーの初期化
    $.state.isTouchable = true; // 緑の玉を触れられるようにするフラグ
    $.state.handOffset = new Quaternion().setFromEulerAngles(new Vector3(0, 90, 0)); // 実身体の手（コントローラ）とバーチャル手の回転の差を補正するオフセットを設定する
})

$.onUpdate(() => {
    if ($.getStateCompat("this", "exp_conditionChanged", "boolean")) {
        $.setStateCompat("this", "exp_conditionChanged", false);
        $.state.currentCondition = $.state.conditions[$.getStateCompat("global", "exp_conditionID", "integer")];
        onConditionChanged();
    }

    tick();
})

$.onReceive((messageType, arg, sender) => {
    if (messageType === "exp_updateConditions") {
        $.state.conditions = arg;
        $.setStateCompat("this", "exp_conditionChanged", true);
    }
})

// 試行（実験条件：ゲインの値）が変わるたびに実行される
function onConditionChanged () {
    // 変数の値の初期化：プレイヤー、原点、目標地点
    if (!$.state.player || !$.state.originPos || !$.state.targetPos) {
        $.state.player = $.getPlayersNear($.getPosition(), Infinity)[0];
        $.state.originPos = $.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.3, 0.3));
        $.state.targetPos = $.state.player.getHumanoidBonePosition(HumanoidBone.Head).clone().add(new Vector3(0, -0.3, 0.6));
    }
    
    $.state.gain = 1; // 原点に触れる前はゲインの値が1のまま
    $.subNode("Sphere").setPosition($.state.originPos); // 緑の玉を原点に動かす
}

// フレームごとに実行される
function tick () {
    if (!$.state.player || !$.state.originPos) return;

    // バーチャル手の位置を計算する：原点からの実身体の手（コントローラ）の相対位置×ゲイン
    $.subNode("RightHandAnchor").setPosition(
        $.state.originPos.clone()
            .add($.state.player.getHumanoidBonePosition(HumanoidBone.RightHand).clone()
                .sub($.state.originPos)
                .multiplyScalar($.state.gain || 1)));
    
    // バーチャル手の回転を実身体の手と同期させる
    $.subNode("RightHandAnchor").setRotation($.state.player.getHumanoidBoneRotation(HumanoidBone.RightHand).clone().multiply($.state.handOffset));
    

    if (!$.state.isTouchable) { // 緑の玉が触れられたばかりで、しばらく触れても反応させてはいけない場合
        $.state.timer = $.state.timer + 1; // タイマー + 1
        
        if ($.state.timer > 10) { //　緑の玉が触れられた時点から10フレーム経ったら
            $.state.isTouchable = true; // 次のフレームから緑の玉を再び触れられるようにする
            $.state.timer = 0; // タイマーを0に戻す
        } else {
            $.setStateCompat("this", "isSphereTouched", false); // 緑の玉が触れられたと検知するフラグをfalseに固定させる
        }
    } else if ($.getStateCompat("this", "isSphereTouched", "boolean")) { // 緑の玉が触れられたと検知したら（trueになったら）
        /*
            緑の玉が触れられたら　$.getStateCompat("this", "isSphereTouched", "boolean")　の値が変わるように、
            このスクリプトが付いたアイテムにCCKのコンポーネント`On Collide Item Trigger`を追加し、
            このアイテムに向けてキー`isSphereTouched`で、メッセージ内容=trueを発信するようにしてください
        */

        $.state.isTouchable = false; // 二重クリックを防ぐために、緑の玉を触れられるようにするフラグをfalseにする
        $.setStateCompat("this", "isSphereTouched", false); // 緑の玉が触れられたと検知するフラグをfalseに戻す

        if ($.state.isReaching) {
            // 目標地点にある緑の玉が触れられる場合
            onTargetTouched();
        } else {
            // 原点にある緑の玉が触れられる場合
            onOriginTouched();
        }
    }
}

// 原点にある緑の玉が触れられる時に実行される
function onOriginTouched () {
    $.subNode("Sphere").setPosition($.state.targetPos); // 緑の玉を目標地点に動かし
    $.state.gain = $.state.currentCondition["gain"] ? parseFloat($.state.currentCondition["gain"]) : 1; // この試行におけるゲインの値を設定する
    $.state.isReaching = true; // リーチング（目標地点まで手を伸ばす）フラグをtrueにする
}

// 目標地点にある緑の玉が触れられる時に実行される
function onTargetTouched () {
    $.sendSignalCompat("this", "state_triggerTransition"); // 次のフェーズ（質問に回答するフェーズ）に遷移させる
    /*
        この関数が実行されると次のフェーズに遷移されるように、
        このスクリプトが付いたアイテムにCCKのコンポーネント`Global Logic`を追加してください。
        その`Global Logic`の中身を、このアイテムに向けたキー`state_triggerTransition`を検知し、
        globalに向けたキー`state_triggerTransition`でsignalを発信するようにしてください
    */

    $.state.isReaching = false; // リーチング（目標地点まで手を伸ばす）フラグをfalseにする
}