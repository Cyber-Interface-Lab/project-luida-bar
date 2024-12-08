$.onStart(() => {
  $.state.answerValue = null;
  $.state.destroyable = false;
  $.getItemsNear($.getPosition(), 0.1).forEach(item => {
      item.send("form_on_answer_option_spawned", true);
  });
})

$.onUpdate(() => {
  // if ($.getStateCompat("this", "form_try_answer", "boolean")) {
  //     $.setStateCompat("this", "form_try_answer", false);
  //     answer();
  // }
    if ($.state.destroyable && $.getStateCompat("owner", "form_destroy_answer_option", "boolean")) {
      $.sendSignalCompat("this", "form_destroy_answer_option");
  }
})

$.onReceive((messageType, arg, sender) => {
  switch (messageType) {
      case "form_init_answer_option":
        $.setStateCompat("owner", "form_destroy_answer_option", false);
        $.state.formController = sender;
        $.state.destroyable = true;
        // if (arg["value"]) $.state.answerValue = arg["value"]
        // if (arg["label"] && $.subNode("Text")) $.subNode("Text").setText(arg["label"]);
        break;
      // case "form_destroy_answer_option":
      //     $.setStateCompat("this", "form_destroy_answer_option", true);
      //     break;
      default:
          break;
  }
})

function answer() {
  $.state.formController.send("form_answer", $.state.answerValue);
}

$.onInteract(player => {
    player.requestTextInput("form_request_text_input", "回答を入力してください");
})

$.onTextInput((text, meta, status) => {
    if (meta !== "form_request_text_input") return;
    switch(status) {
      case TextInputStatus.Success:
        $.state.answerValue = text;
        $.subNode("Text").setText(text);
        answer();
        break;
      case TextInputStatus.Busy:
        // 5秒後にretryする
        $.state.should_retry = true;
        $.state.retry_timer = 5;
        break;
      case TextInputStatus.Refused:
        // 拒否された場合は諦める
        $.state.should_retry = false;
        break;
    }
});