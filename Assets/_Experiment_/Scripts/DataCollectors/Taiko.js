function calculateData () {
  let returnData = $.state.customData;
  const CONDITION = $.groupState.currentCondition;
  const PARTICIPANTS = $.groupState.participants;
  const COLLECTED_DATA = $.groupState.collectedData;

  function saveData_data() {
function float32ToFloat16Bits(val) {
  const f32 = new Float32Array(1);
  const u32 = new Uint32Array(f32.buffer);
  f32[0] = val;
  const x = u32[0];

  const sign     = (x >>> 16) & 0x8000;          // 1-bit
  let   exponent = ((x >>> 23) & 0xFF) - 127 + 15; // 5-bit (再バイアス)
  let   mantissa =  x & 0x7FFFFF;                // 23-bit

  if (exponent <= 0) {
    if (exponent < -10) return sign;             // underflow → ±0
    mantissa = (mantissa | 0x800000) >> (1 - exponent);
    return sign | (mantissa + 0x1000 >> 13);
  }

  if (exponent >= 31) {
    return sign | 0x7C00 | (mantissa ? 1 : 0);   // preserve NaN payload 最低1bit
  }

  return sign | (exponent << 10) | (mantissa + 0x1000 >> 13);
}
function float32ToFloat16Hex(val) {
  return float32ToFloat16Bits(val).toString(16).padStart(4, "0");
}
let encodedPositionStr = "";
let bone;

["Head", "Neck", "Chest", "Spine", "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand", "RightShoulder", "RightUpperArm", "RightLowerArm", "RightHand"].forEach(boneLabel => {
  bone = PARTICIPANTS[0].getHumanoidBonePosition(HumanoidBone[boneLabel]);
  encodedPositionStr += (float32ToFloat16Hex(bone.x) + float32ToFloat16Hex(bone.y) + float32ToFloat16Hex(bone.z));
});

return {
  s: CONDITION['selfAvatar'],
  p: encodedPositionStr,
  t: Date.now()
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
