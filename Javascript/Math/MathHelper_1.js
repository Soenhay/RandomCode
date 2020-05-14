// Get the linear interpolation between two values.
Math.lerp1d = function (startValue, targetValue, amount = 0.1) {
    return (1 - amount) * startValue + amount * targetValue;
};

// Get the linear interpolation between two 2d points.
Math.lerp2d = function (startPoint, targetPoint, amount = 0.1) {
    return { x: Math.lerp1d(startPoint.x, targetPoint.x, amount), y: Math.lerp1d(startPoint.y, targetPoint.y, amount) };
};

// Get the linear interpoloation between two 3d points.
Math.lerp3d = function (startPoint, targetPoint, amount = 0.1) {
    return { x: Math.lerp1d(startPoint.x, targetPoint.x, amount), y: Math.lerp1d(startPoint.y, targetPoint.y, amount), z: Math.lerp1d(startPoint.z, targetPoint.z, amount) };
}
