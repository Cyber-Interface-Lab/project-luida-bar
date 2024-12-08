function calculateData () {
    let fileName = "sampleFileName";
    // Implement your calculation for custom data to record & upload
    
    // Return the data in the following format:
    return { ...$.state.customData, [fileName]: {} }; // $.state.customData is the custom data recorded before 
}