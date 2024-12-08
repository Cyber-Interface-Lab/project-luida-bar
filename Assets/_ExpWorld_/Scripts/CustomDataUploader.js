$.onStart(() => {
    $.state.customData = {};
})

$.onUpdate(() => {
    if ($.getStateCompat("this", "exp_recordCustomData", "boolean")) {
        $.setStateCompat("this", "exp_recordCustomData", false);
        recordData();
    }
    if ($.getStateCompat("this", "exp_uploadCustomData", "boolean")) {
        $.setStateCompat("this", "exp_uploadCustomData", false);
        uploadData();
    }
})

function recordData () {
    $.state.customData = calculateData();
}

function uploadData () {
    $.log("Send final answers: " + JSON.stringify($.state.customData));
    let request = { type: "uploadCustomData", dataByFileName: JSON.stringify($.state.customData) };
    $.callExternal(JSON.stringify(request), "customDataUploaded");
}

$.onExternalCallEnd((res, meta, err) =>
{
    if (res == null) {
        $.log("callExternal ERROR: " + err);
        return;
    }

    if (meta === "customDataUploaded") {
        $.log("Custom recorded data uploaded");
    }
});