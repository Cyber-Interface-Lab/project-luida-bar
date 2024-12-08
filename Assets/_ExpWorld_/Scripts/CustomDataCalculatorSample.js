function calculateData () {
    let fileName = "sampleFileName";
    // Implement your calculation for custom data to record & upload
    const score = Math.random();
    
    const originalScore = $.state.customData[fileName] ? ($.state.customData[fileName].scores || []) : [];
    $.log({ scores: [...originalScore, score] });
    
    // Return the data in the following format:
    return { ...$.state.customData, [fileName]: { scores: [...originalScore, score] } }; // $.state.customData is the custom data recorded before 
}