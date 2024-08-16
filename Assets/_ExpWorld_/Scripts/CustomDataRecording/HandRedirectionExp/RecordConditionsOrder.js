function calculateData () {
    let fileName = "taskAnswers";
    return {
        ...$.state.customData,
        [fileName]: [
            ...($.state.customData[fileName] || []),
            {
                g: $.state.currentCondition["gain"], 
                a: $.getStateCompat("global", "isFaster", "boolean") ? "F" : "S"
            }
        ]
    };
}