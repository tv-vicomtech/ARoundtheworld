function distance(lat1, lon1, lat2, lon2)
{
    const EARTH_RADIUS = 6371; // consideramos la tierra perfectamente esferica

    const toRadian = angle => (Math.PI / 180) * angle;
    const distance = (a, b) => (Math.PI / 180) * (a - b);

    // Degrees --> radians
    const dLat = distance(lat2, lat1);
    const dLon = distance(lon2, lon1);

    lat1 = toRadian(lat1);
    lat2 = toRadian(lat2);
    
    // Haversine formula: https://en.wikipedia.org/wiki/Haversine_formula
    const haversine_angle = Math.pow(Math.sin(dLat / 2), 2) + Math.pow(Math.sin(dLon / 2), 2) * Math.cos(lat1) * Math.cos(lat2);
    const c = 2 * Math.asin(Math.sqrt(haversine_angle));

    // calculate the result
    const haversine_distance = c * EARTH_RADIUS;
    return haversine_distance;
}