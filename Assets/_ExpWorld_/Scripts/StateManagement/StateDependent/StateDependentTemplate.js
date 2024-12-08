$.onStart(() => {
    $.sendSignalCompat("this", "state_setID");
})

$.onUpdate(() => {
    if ($.getStateCompat("this", "state_exit", "boolean")) {
        OnStateExit();
        $.setStateCompat("this", "state_exit", false);
    }
    if ($.getStateCompat("this", "state_enter", "boolean")) {
        $.setStateCompat("this", "state_enter", false);
        OnStateEnter();
    }
    if ($.getStateCompat("this", "state_isActive", "boolean")) {
        DuringState();
    }
})

function OnStateEnter() { if ($.getStateCompat("this", "state_id", "integer") === 1) $.log($.getStateCompat("global", "state_currentID", "integer") + " global | this " + $.getStateCompat("this", "state_id", "integer")); }

function OnStateExit () {}

function DuringState () {}