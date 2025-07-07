function calculateData () {
  let returnData = $.state.customData;
  const CONDITION = $.groupState.currentCondition;
  const PARTICIPANTS = $.groupState.participants;
  const COLLECTED_DATA = $.groupState.collectedData;

  function saveData_data() {
return {
  s: COLLECTED_DATA['timestamp'].start,
  e: COLLECTED_DATA['timestamp'].end,
  centerHits: COLLECTED_DATA['centerHits'],
  sideHits: COLLECTED_DATA['sideHits'],
  self: CONDITION['selfAvatar'],
  other: CONDITION['otherAvatar'],
  number: CONDITION['number']
};
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
