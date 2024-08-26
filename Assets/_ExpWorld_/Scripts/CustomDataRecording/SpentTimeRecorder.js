function calculateData () {
  let fileName = "spentTime";

  if ($.state.customData[fileName] && $.state.customData[fileName].length >= 10) {
    uploadData();
    $.state.customData = { ...$.state.customData, [fileName]: [] };
  }

  let x = $.getStateCompat("owner", "x", "integer");
  let y = $.getStateCompat("owner", "y", "integer");
  let z = parseInt($.state.currentCondition["d"]);
  let s = parseInt($.state.currentCondition["s"]);
  let t = $.getStateCompat("owner", "spentTime", "float").toFixed(4);
  
  let recordedSpentTimes = $.state.customData[fileName] || [];
  recordedSpentTimes.push({ x, y, z, s, t });
  return { ...$.state.customData, [fileName]: recordedSpentTimes };
}