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


function Start() {
  // Control flag - only calculate when true
  $.state.isTracking = false;

  // Position/velocity/acceleration tracking
  $.state.prevPos = null;
  $.state.prevVel = null;
  $.state.currentVel = null;
  $.state.prevAccelMag = null;
  $.state.prevPrevAccelMag = null;

  // Swing tracking
  $.state.swingStartTime = null;
  $.state.swingDistance = 0;
  $.state.swingSpeedSum = 0;
  $.state.swingSampleCount = 0;
  $.state.inSwing = false;

  // Completed swings
  $.state.completedSwingSpeeds = [];
  $.state.swingCount = 0;
  $.state.averageSwingSpeed = 0;

  // Collision data
  $.state.collisionVelocity = null;
  $.state.collisionSpeed = null;
  $.state.collisionSpeeds = [];       // All collision speeds
  $.state.collisionCount = 0;
  $.state.averageCollisionSpeed = 0;  // Average across all collisions

  // Time tracking
  $.state.totalTime = 0;
}

function Update(deltaTime) {
  // Skip if not tracking
  if (!$.state.isTracking) {
    // Reset tracking state when disabled to start fresh next time
    $.state.prevPos = null;
    $.state.prevVel = null;
    $.state.currentVel = null;
    $.state.prevAccelMag = null;
    $.state.prevPrevAccelMag = null;
    return;
  }

  $.state.totalTime += deltaTime;

  // Get current position
  const currentPos = $.getPosition();
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

    // Calculate acceleration (requires previous velocity)
    if ($.state.prevVel !== null) {
      const dvx = currentVel.x - $.state.prevVel.x;
      const dvy = currentVel.y - $.state.prevVel.y;
      const dvz = currentVel.z - $.state.prevVel.z;

      const accelMag = Math.sqrt(dvx * dvx + dvy * dvy + dvz * dvz) / deltaTime;

      // Detect local minimum acceleration
      // A local minimum occurs when: prevPrevAccelMag > prevAccelMag < currentAccelMag
      if ($.state.prevAccelMag !== null && $.state.prevPrevAccelMag !== null) {
        const isLocalMin = $.state.prevPrevAccelMag > $.state.prevAccelMag &&
                          $.state.prevAccelMag < accelMag;

        if (isLocalMin) {
          // Found a local minimum - this is a swing endpoint
          if ($.state.inSwing) {
            // End current swing and store its average speed
            if ($.state.swingSampleCount > 0) {
              const swingAvgSpeed = $.state.swingSpeedSum / $.state.swingSampleCount;
              $.state.completedSwingSpeeds.push(swingAvgSpeed);
              $.state.swingCount += 1;

              $.log("Swing #" + $.state.swingCount + " completed. Speed: " +
                    swingAvgSpeed.toFixed(3) + " m/s");
            }
          }

          // Start new swing
          $.state.swingStartTime = $.state.totalTime;
          $.state.swingDistance = 0;
          $.state.swingSpeedSum = 0;
          $.state.swingSampleCount = 0;
          $.state.inSwing = true;
        }
      }

      // Accumulate swing data
      if ($.state.inSwing) {
        const frameDist = Math.sqrt(dx * dx + dy * dy + dz * dz);
        $.state.swingDistance += frameDist;
        $.state.swingSpeedSum += speed;
        $.state.swingSampleCount += 1;
      }

      // Shift acceleration history
      $.state.prevPrevAccelMag = $.state.prevAccelMag;
      $.state.prevAccelMag = accelMag;
    } else {
      // First acceleration calculation
      $.state.prevAccelMag = 0;
    }

    // Store current velocity for next frame and collision detection
    $.state.currentVel = currentVel;
    $.state.prevVel = currentVel.clone();
  }

  // Store current position for next frame
  $.state.prevPos = currentPos.clone();
}

$.onCollide((collision) => {
  // Skip if not tracking
  if (!$.state.isTracking) return;

  // Capture velocity at collision
  if ($.state.currentVel !== null) {
    $.state.collisionVelocity = $.state.currentVel.clone();
    $.state.collisionSpeed = Math.sqrt(
      $.state.currentVel.x * $.state.currentVel.x +
      $.state.currentVel.y * $.state.currentVel.y +
      $.state.currentVel.z * $.state.currentVel.z
    );

    // Track collision speed
    $.state.collisionSpeeds.push($.state.collisionSpeed);
    $.state.collisionCount += 1;

    $.log("Collision #" + $.state.collisionCount + "! Speed: " +
          $.state.collisionSpeed.toFixed(3) + " m/s");
  }
});

function sendSwingData() {
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

  // Calculate average collision speed
  let avgCollisionSpeed = 0;
  if ($.state.collisionSpeeds.length > 0) {
    let totalCollisionSpeed = 0;
    for (let i = 0; i < $.state.collisionSpeeds.length; i++) {
      totalCollisionSpeed += $.state.collisionSpeeds[i];
    }
    avgCollisionSpeed = totalCollisionSpeed / $.state.collisionSpeeds.length;
  }
  $.state.averageCollisionSpeed = avgCollisionSpeed;

  // Send to DataCollector
  SendDataToCollector("leftSwingAvgSpeed", avgSwingSpeed);
  SendDataToCollector("leftCollisionAvgSpeed", avgCollisionSpeed);

  $.log("Data sent - leftSwingAvgSpeed: " + avgSwingSpeed.toFixed(3) +
        " m/s, leftCollisionAvgSpeed: " + avgCollisionSpeed.toFixed(3) + " m/s");

  Reset();
}

function Reset() {
  $.state.prevPos = null;
  $.state.prevVel = null;
  $.state.currentVel = null;
  $.state.prevAccelMag = null;
  $.state.prevPrevAccelMag = null;

  $.state.swingStartTime = null;
  $.state.swingDistance = 0;
  $.state.swingSpeedSum = 0;
  $.state.swingSampleCount = 0;
  $.state.inSwing = false;

  $.state.completedSwingSpeeds = [];
  $.state.swingCount = 0;
  $.state.averageSwingSpeed = 0;

  $.state.collisionVelocity = null;
  $.state.collisionSpeed = null;
  $.state.collisionSpeeds = [];
  $.state.collisionCount = 0;
  $.state.averageCollisionSpeed = 0;

  $.state.totalTime = 0;
}