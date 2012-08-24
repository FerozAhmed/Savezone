function lightsOn() {
    console.log("lights.js>>> Turning on the light");
    $('#container').fadeIn(500);
};

function lightsOff(callback) {
    console.log("lights.js>>> Turning off the light");
    $('#container').fadeOut(250, callback);
};