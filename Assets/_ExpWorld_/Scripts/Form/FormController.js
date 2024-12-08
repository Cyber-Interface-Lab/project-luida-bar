const dummyQuestions = [
    { title: "どのくらいの頻度でVRを体験していますか？", description: "どのくらいの頻度でVRを体験していますか？", questionTypeID: 0, answerOptions: ["なし", "年に一回未満", "年に一回以上", "月に1~2回くらい", "週1回くらい", "週2~3回くらい", "週4回以上"], isRequired: true },
    { title: "VRについて興味ありますか？", description: "1:全く興味ない、7:とても興味ある", questionTypeID: 1, answerOptions: [1, 2, 3, 4, 5, 6, 7], isRequired: true },
    { title: "VRデバイスのメーカーとして認識している会社を選んでください", description: "会社として聞いたことはあるが、VRメーカーであることは知っていなければ、選択しないでください", questionTypeID: 2, answerOptions: ["Meta (Oculus)", "Vive", "Valve", "Sony", "Pico", "DPVR"], isRequired: true },
    { title: "10歳以上ですか？", description: "10歳未満の方はVRデバイスの使用をお控えください", questionTypeID: 3, answerOptions: [], isRequired: true },
    { title: "コメント", description: "何かコメントがあればどうぞ！", questionTypeID: 4, answerOptions: [], isRequired: false }
]

const answerOptionUISpawnCenter = new Vector3(0, -0.3, 0);

const attrsByQuestionType = [
    { questionType: "Options (single answer)",      answerType: "radio",    aTemplateName: "radio_button" },
    { questionType: "Linear scale",                 answerType: "radio",    aTemplateName: "scale_button" },
    { questionType: "Options (multiple answers)",   answerType: "check",    aTemplateName: "checkbox" },
    { questionType: "Toggle",                       answerType: "toggle",   aTemplateName: "toggle" },
    { questionType: "Text",                         answerType: "text",     aTemplateName: "text_input" },
]

$.onStart(() => {
    reset();
})

$.onUpdate(() => {
    if (!$.state.isInitiated && $.getStateCompat("this", "form_set_content_active", "boolean")) {
        $.state.isInitiated = true;
        $.state.qID = 0;
        
        let request = {type: "questions", eID: expID, qID: $.getStateCompat("this", "qID", "integer")};
        $.callExternal(JSON.stringify(request), "getQuestions");
    }
    if ($.state.waitBeforeSpawningAnswerOption) {
        if ($.state.timer < 10) {
            $.state.timer = $.state.timer + 1;
        } else {
            $.state.waitBeforeSpawningAnswerOption = false;
            $.state.timer = 0;
            $.sendSignalCompat("this", "form_spawn_" + attrsByQuestionType[$.state.questionTypeID].aTemplateName);
        }
    }
    if ($.state.waitBeforeInitQuestion) {
        if ($.state.timer < 10) {
            $.state.timer = $.state.timer + 1;
        } else {
            $.state.waitBeforeInitQuestion = false;
            $.state.timer = 0;
            initQuestion();
        }
    }
})

$.onInteract(() => {
    $.setStateCompat("this", "form_set_content_active", true);
});

$.onReceive((messageType, arg, sender) => {
    if (!$.state.isInitiated) return;
    switch (messageType) {
        case "form_on_answer_option_spawned":
            if (arg && $.state.answerOptions) {
                sender.send("form_init_answer_option", { value: $.state.spawningAnswerID + 1, label: $.state.answerOptions[$.state.spawningAnswerID] });
                if ($.state.spawningAnswerID < $.state.answerOptions.length - 1) {
                    $.state.spawningAnswerID = $.state.spawningAnswerID + 1;
                    spawnAnswerOptionUI();
                }
            }
            break;
        case "form_answer":
            switch ($.state.questionTypeID) {
                case 0: // radio buttons
                case 1: // linear scale
                    $.subNode("RadioButtonIndicator").setEnabled(true);
                    $.subNode("RadioButtonIndicator").setPosition($.state.answerOptionLocalPositions[arg - 1].clone());
                case 3: // toggle
                case 4: // text
                    $.state.tmpAnswer = arg;
                    break;
                case 2: // checkboxes
                    if (!Array.isArray($.state.tmpAnswer)) $.state.tmpAnswer = [];
                    if (arg.isOn) {
                        if (!$.state.tmpAnswer.includes(arg.value)) $.state.tmpAnswer = [ ...$.state.tmpAnswer, arg.value ];
                    } else {
                        if ($.state.tmpAnswer.includes(arg.value)) {
                            $.state.tmpAnswer = $.state.tmpAnswer.filter(item => {
                                return item !== arg.value;
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
            break;
        case "form_to_next":
            saveAnswer();
            break;
        case "form_to_prev":
            toPrev();
            break;
        default:
            break;
    }
})

function tryInitQuestion () {
    $.state.timer = 0;
    $.state.waitBeforeInitQuestion = true;
}

function initQuestion () { // options: array of string
    $.state.answerOptionLocalPositions = [];
    q = $.state.questions[$.state.qID]
    $.subNode("Title").setText(q.title);
    $.subNode("Description").setText(q.description);
    $.state.questionTypeID = q.questionTypeID;
    $.state.answerOptions = q.answerOptions;
    $.state.spawningAnswerID = 0;
    spawnAnswerOptionUI();
}

function setAnswerOptionUISpawnPoint () {
    let pos = answerOptionUISpawnCenter.clone();
    let rot = new Quaternion();;
    switch ($.state.questionTypeID) {
        case 0:
        case 2:
            // let maxStringLength = Math.min(30, $.state.answerOptions.reduce((maxLength, str) => Math.max(maxLength, str.length), 0));
            let x = 0;
            // if ($.state.answerOptions.length <= 5) {
            //     x = -0.1 - 0.3 * ((maxStringLength - 1) / 7)
            // }
            // $.log(x);
            let y = (($.state.answerOptions.length - 1 - $.state.spawningAnswerID) - ($.state.answerOptions.length - 1) / 2) * 0.3;
            pos = pos.add(new Vector3(x, y, 0));
            // let textSize = 0.05;
            // max 5 options (rows) per column
            // 1 columns: x = -0.1 - 0.3 * ((chars - 1) / 7), textsize = 0.05, max 30 chars per text line, 1~2 lines
            // 2 columns: x = -0.5 - 0.065 * (chars - 1) & 0.2, textsize = 0.04, 15 chars per text line, 1~2 lines; textsize = 0.03 for 3 lines
            // > 3 columns: x = -1.5 + 0.1 + (3 / columnCnt) * columnID, textsize = 0.05 - chars * (0.025/(max chars per line)), max chars per line = 3->15, 4->10, 5->6, 6->4 = (60/columnCnt - 5)
            break;
        case 1:
            pos = pos.add(new Vector3((($.state.answerOptions.length > 11 ? 3 : 2) / ($.state.answerOptions.length - 1)) * ($.state.spawningAnswerID - ($.state.answerOptions.length - 1) / 2), 0, 0));
            break;
        case 3:
        case 4:
            break;
        default:
            break;
    }
    $.state.answerOptionLocalPositions = [ ...$.state.answerOptionLocalPositions, pos.clone() ];
    $.subNode("AnswerOptionSpawnPoint").setPosition(pos.clone());
    $.subNode("AnswerOptionSpawnPoint").setRotation(rot.clone());
}

function spawnAnswerOptionUI () {
    setAnswerOptionUISpawnPoint();
    $.state.waitBeforeSpawningAnswerOption = true;
}

function destroyAnswerOptionUIs () {
    $.setStateCompat("owner", "form_destroy_answer_option", true);
}

function saveAnswer () {
    if ($.state.questions[$.state.qID].isRequired && (!$.state.tmpAnswer && $.state.tmpAnswer !== false)) return;
    $.state.answers = { ...$.state.answers, [$.state.qID]: $.state.tmpAnswer };
    $.log("Update answers: " + JSON.stringify($.state.answers));
    toNext();
}

function submitAnswers () {
    $.log("Send final answers: " + JSON.stringify($.state.answers));
    let request = {type: "questionAnswers", eID: expID, qID: $.getStateCompat("this", "qID", "integer"), answers: JSON.stringify($.state.answers)};
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        if (item.id === "5570182165721890090") { // ConditionManager
            item.send("exp_questionnaire_answer", $.state.answers);
        }
    });
    $.callExternal(JSON.stringify(request), "postQuestionAnswers");
    reset();
    $.sendSignalCompat("this", "state_triggerTransition");
}

function toNext () {
    $.state.qID = $.state.qID + 1;
    $.subNode("RadioButtonIndicator").setEnabled(false);
    if ($.state.qID >= $.state.questions.length) {
        submitAnswers();
    } else {
        destroyAnswerOptionUIs();
        $.state.tmpAnswer = null;
        // TODO: hide selection indicator
        tryInitQuestion();
    }
}

function toPrev () {
    if ($.state.qID <= 0) return;
    destroyAnswerOptionUIs();
    $.state.qID = $.state.qID - 1;
    $.state.tmpAnswer = null;
    // TODO: hide selection indicator
    tryInitQuestion();
    $.subNode("RadioButtonIndicator").setEnabled(false);
}

function reset () {
    destroyAnswerOptionUIs();
    $.subNode("RadioButtonIndicator").setEnabled(false);
    $.state.answers = {};
    $.state.qID = 0;
    $.state.answerType = -1;
    $.state.tmpAnswer = null;
    $.state.isInitiated = false;
    $.state.waitBeforeSpawningAnswerOption = false;
    $.state.waitBeforeInitQuestion = false;
    $.state.timer = 0;
    $.state.answerOptionLocalPositions = [];
    $.setStateCompat("this", "form_set_content_active", false);
    // TODO: hide selection indicator
}

$.onExternalCallEnd((res, meta, err) =>
{
    if (res == null) {
        $.log("callExternal ERROR: " + err);
        return;
    }

    if (meta === "getQuestions") {
        // $.log(res);
        let parsedRes = JSON.parse(res);
        $.state.questions = parsedRes.questions;
        tryInitQuestion();

        // for(let i = 0; i < Math.min(parsedRes.quests.length, numberPerPage); i++)
        // {
        //     const questTitle = $.subNode("Quest_" + i);
        //     if (questTitle) questTitle.setText(parsedRes.quests[i].title);
        // }
    }

    if (meta === "postQuestionAnswers") {
        $.log("Answers recorded!");
    }
});