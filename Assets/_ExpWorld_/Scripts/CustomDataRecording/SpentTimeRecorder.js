function calculateData () {
    let fileName = "spentTimeByTarget";
    
    let x = $.getStateCompat("owner", "x", "integer");
    let y = $.getStateCompat("owner", "y", "integer");
    let z = parseInt(currentConditions["depth"]);
    let s = parseInt(currentConditions["size"]);
    let t = $.getStateCompat("owner", "spentTime", "float");
    
    let recordedSpentTimes = $.state.customData[fileName] || [];
    recordedSpentTimes.push({ x, y, z, s, t });
    return { ...$.state.customData, [fileName]: recordedSpentTimes };
}