const stateEnterActions = {
    0: [
        { type: "exec", action: (deltaTime) => {
            $.setStateCompat('this', 'exp_showItem', true);
        } }
    ],
    3: [
        { type: "exec", action: (deltaTime) => {
            $.state.isTracking = true;
        } }
    ],
    4: [
        { type: "exec", action: (deltaTime) => {
            sendSwingData();
        } }
    ]
};

const duringStateActions = {
};

const stateExitActions = {
    3: [
        { type: "exec", action: (deltaTime) => {
            $.state.isTracking = false;
        } }
    ]
};


// Velocity threshold for swing detection
const VEL_Y_THRESHOLD = 0.1;       // m/s - Y velocity below this is "near zero"
const SWING_MIN_SAMPLES = 3;       // minimum frames for valid swing

function Start() {
  // Control flag - only calculate when true
  $.state.isTracking = false;

  // Position/velocity tracking
  $.state.prevPos = null;
  $.state.currentVel = null;

  // Swing tracking (based on Y position/velocity direction)
  $.state.swingStartTime = null;
  $.state.swingDistance = 0;
  $.state.swingSpeedSum = 0;
  $.state.swingSampleCount = 0;
  $.state.swingMaxSpeed = 0;  // Peak speed during current swing (used as collision speed)
  $.state.inSwing = false;
$.state.wasMovingDown = false;  // Track if Y velocity was negative (moving down)

// Completed swings
  $.state.completedSwingSpeeds = [];
  $.state.swingCount = 0;
  $.state.averageSwingSpeed = 0;

  // Peak speeds per swing (treated as collision speeds)
  $.state.swingPeakSpeeds = [];       // Peak speed from each swing
  $.state.averagePeakSpeed = 0;       // Average peak speed across all swings

  // Time tracking
  $.state.totalTime = 0;
}

function Update(deltaTime) {
  // Skip if not tracking
  if (!$.state.isTracking) {
    // Reset tracking state when disabled to start fresh next time
    $.state.prevPos = null;
$.state.currentVel = null;
    return;
  }

  $.state.totalTime += deltaTime;

// Get current position
  const currentPos = $.getPosition().clone();
  if (!currentPos) return;

  // Calculate velocity (requires previous position)
if ($.state.prevPos !== null) {
    const dx = currentPos.x - $.state.prevPos.x;
const dy = currentPos.y - $.state.prevPos.y;
    const dz = currentPos.z - $.state.prevPos.z;

const currentVel = new Vector3(
      dx / deltaTime,
      dy / deltaTime,
dz / deltaTime
    );
    const speed = Math.sqrt(dx * dx + dy * dy + dz * dz) / deltaTime;

    // Detect swing based on Y position (Y velocity direction)
const velY = currentVel.y;
    const isMovingDown = velY < -VEL_Y_THRESHOLD; // Negative Y = moving down

    if ($.state.wasMovingDown && !isMovingDown) {
      // Was moving down, now stopped or moving up: downward swing completed
if ($.state.inSwing && $.state.swingSampleCount >= SWING_MIN_SAMPLES) {
const swingAvgSpeed = $.state.swingSpeedSum / $.state.swingSampleCount;
const swingSpeeds = $.state.completedSwingSpeeds;
        swingSpeeds.push(swingAvgSpeed);
$.state.completedSwingSpeeds = swingSpeeds;

        // Record peak speed as collision speed for this swing
        const peakSpeeds = $.state.swingPeakSpeeds;
peakSpeeds.push($.state.swingMaxSpeed);
        $.state.swingPeakSpeeds = peakSpeeds;

$.state.swingCount += 1;
        $.log("Downward swing #" + $.state.swingCount + " completed. Avg: " +
              swingAvgSpeed.toFixed(3) + " m/s, Peak: " + $.state.swingMaxSpeed.toFixed(3) + " m/s");
      }

      // Reset for next swing
      $.state.swingStartTime = null;
      $.state.swingDistance = 0;
      $.state.swingSpeedSum = 0;
      $.state.swingSampleCount = 0;
$.state.swingMaxSpeed = 0;
      $.state.inSwing = false;
    }

    if (!$.state.wasMovingDown && isMovingDown) {
      // Started moving down: new downward swing starting
$.state.inSwing = true;
      $.state.swingStartTime = $.state.totalTime;
}

    $.state.wasMovingDown = isMovingDown;

    // Accumulate swing data (only during downward movement)
    if ($.state.inSwing) {
      const frameDist = Math.sqrt(dx * dx + dy * dy + dz * dz);
      $.state.swingDistance += frameDist;
$.state.swingSpeedSum += speed;
      $.state.swingSampleCount += 1;

// Track peak speed during this swing
      if (speed > $.state.swingMaxSpeed) {
        $.state.swingMaxSpeed = speed;
      }
    }

    // Store current velocity for collision detection
    $.state.currentVel = currentVel;
  }

// Store current position for next frame
  $.state.prevPos = currentPos.clone();
}

function sendSwingData() {
  // Finalize any in-progress downward swing before calculating averages
  if ($.state.inSwing && $.state.swingSampleCount >= SWING_MIN_SAMPLES) {
    const swingAvgSpeed = $.state.swingSpeedSum / $.state.swingSampleCount;
const swingSpeeds = $.state.completedSwingSpeeds;
    swingSpeeds.push(swingAvgSpeed);
$.state.completedSwingSpeeds = swingSpeeds;

    // Record peak speed for this swing
    const peakSpeeds = $.state.swingPeakSpeeds;
    peakSpeeds.push($.state.swingMaxSpeed);
$.state.swingPeakSpeeds = peakSpeeds;

    $.state.swingCount += 1;
  }
$.state.inSwing = false;

  // Calculate average swing speed
  let avgSwingSpeed = 0;
  if ($.state.completedSwingSpeeds.length > 0) {
    let totalSwingSpeed = 0;
    for (let i = 0; i < $.state.completedSwingSpeeds.length; i++) {
totalSwingSpeed += $.state.completedSwingSpeeds[i];
    }
    avgSwingSpeed = totalSwingSpeed / $.state.completedSwingSpeeds.length;
  }
  $.state.averageSwingSpeed = avgSwingSpeed;

  // Calculate average peak speed (used as collision speed)
let avgPeakSpeed = 0;
  if ($.state.swingPeakSpeeds.length > 0) {
    let totalPeakSpeed = 0;
    for (let i = 0; i < $.state.swingPeakSpeeds.length; i++) {
      totalPeakSpeed += $.state.swingPeakSpeeds[i];
    }
    avgPeakSpeed = totalPeakSpeed / $.state.swingPeakSpeeds.length;
}
  $.state.averagePeakSpeed = avgPeakSpeed;

  // Send to DataCollector (peak speed is treated as collision speed)
  SendDataToCollector("rightSwingAvgSpeed", avgSwingSpeed);
  SendDataToCollector("rightCollisionAvgSpeed", avgPeakSpeed);

$.log("Data sent - rightSwingAvgSpeed: " + avgSwingSpeed.toFixed(3) +
" m/s, rightCollisionAvgSpeed (peak): " + avgPeakSpeed.toFixed(3) + " m/s");

Reset();
}

function Reset() {
  $.state.prevPos = null;
  $.state.currentVel = null;

  $.state.swingStartTime = null;
  $.state.swingDistance = 0;
  $.state.swingSpeedSum = 0;
  $.state.swingSampleCount = 0;
  $.state.swingMaxSpeed = 0;
  $.state.inSwing = false;
  $.state.wasMovingDown = false;

  $.state.completedSwingSpeeds = [];
  $.state.swingCount = 0;
  $.state.averageSwingSpeed = 0;

  $.state.swingPeakSpeeds = [];
  $.state.averagePeakSpeed = 0;

  $.state.totalTime = 0;
}