function calculateData () {
  let returnData = $.state.customData;
  const CONDITION = $.groupState.currentCondition;
  const PARTICIPANTS = $.groupState.participants;
  const COLLECTED_DATA = $.groupState.collectedData;

  function saveData_data() {
return {
  start: COLLECTED_DATA['timestamp'].start,
  end: COLLECTED_DATA['timestamp'].end,
  centerHits: COLLECTED_DATA['centerHits'],
  sideHits: COLLECTED_DATA['sideHits'],
  selfAvatar: CONDITION['selfAvatar'],
  otherAvatar: CONDITION['otherAvatar'],
  number: CONDITION['number'],
}
    return {};
  }
  const newRecord_data = saveData_data();
  if ("data" in returnData && Array.isArray(returnData["data"])) {
    returnData["data"].push(newRecord_data);
  } else {
    returnData["data"] = [newRecord_data];
  }

  return returnData;
}
