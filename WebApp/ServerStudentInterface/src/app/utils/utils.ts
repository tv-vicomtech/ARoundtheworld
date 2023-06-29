import { PREFIX_UNITY_STUDENT, PREFIX_WEB_STUDENT, PREFIX_WEB_TEACHER } from '../interfaces/constants'
import * as THREE from 'three'

const prefixList = [ PREFIX_UNITY_STUDENT, PREFIX_WEB_STUDENT, PREFIX_WEB_TEACHER ]

export function stripPrefix (usernameWithPrefix: string): string {
  const regex = new RegExp(prefixList.join('|'), 'g')
  return usernameWithPrefix.replace(regex, '')
}

const COLORS = [ '#4dc9f6', '#f67019', '#f53794', '#537bc4', '#acc236', '#166a8f', '#00a950', '#58595b', '#8549ba' ]

export function color (index) {
  return COLORS[index % COLORS.length]
}

export function convertXYZToLatLong (pX: number, pY: number, pZ: number) {
  const R = 6371
  // Make x, y, z between -1 and 1
  let x = 2 * pX
  let y = 2 * pY
  let z = 2 * pZ
  let lat, lon, latLon
  let pi = Math.PI
  if (Math.abs(y) < 1) {
    lat = Math.asin(y) * (180 / pi);
  }
  else {
    lat = Math.asin(y / R) * (180 / pi)
  }
  lon = Math.atan2(z, x) * (180 / pi)
  latLon = [lat.toFixed(2), lon.toFixed(2)]
  return latLon
}

export function calculateScore(lat, lon, correctLan, correctLon, difficulty, time_left)
{
  // Compute score of the answer, up to a maximum of 10 points.
  // The formula is as follows:
  // First, sum half od the difficulty (so we avoid having 0)
  // Then, calculate a score based on the distance
  // Then, assign one bonus point if the answer was close (less than 300km) and the student answered in less than 10 seconds
  const MAX_SCORE = 10;
  const CLOSE_FOR_BONUS = 500; // se puede tunear quizás
  const TIME_FOR_BONUS = 20;
  var points = 0;
  points += difficulty/2;
  const d = calculateDistance(lat, lon, correctLan, correctLon);
  // console.log("Distance is ", d);

  // assign remaining points based on the distance from the correct answer (-1 that can be given based on time)
  const max_points_left = MAX_SCORE - 1 - points; 

  if (d < 50) {
      points += max_points_left;
  }
  else if (d < 100) {
      points += 0.95 * max_points_left;
  }
  else if (d < 200) {
      points += 0.9 * max_points_left;
  }
  else if (d < 300) {
      points += 0.85 * max_points_left;
  }
  else if (d < 400) {
      points += 0.8 * max_points_left;
  }
  else if (d < 500) {
      points += 0.7 * max_points_left;
  }
  else if (d < 700) {
      points += 0.6 * max_points_left;
  }
  else if (d < 900) {
      points += 0.5 * max_points_left;
  }
  else if (d < 1500) {
      points += 0.4 * max_points_left;
  }
  else if (d < 2000) {
      points += 0.3 * max_points_left;
  }
  else if (d < 3000) {
      points += 0.2 * max_points_left;
  }
  else if (d < 4000) {
      points += 0.15 * max_points_left;
  }
  else if (d < 5000) {
      points += 0.1 * max_points_left;
  }
  else if (d < 10000) {
      points += 0.05 * max_points_left;
  }

  if (d <= CLOSE_FOR_BONUS && time_left >= TIME_FOR_BONUS)
  {
      points += 1;
  }

  return points;
}

export function calculateScoreOld(lat, lon, correctLan, correctLon, difficulty, time_left)
{
  // Compute score of the answer, up to a maximum of 10 points.
  // The formula is as follows:
  // First, sum half od the difficulty (so we avoid having 0)
  // Then, calculate a score based on the distance
  // Then, assign one bonus point if the answer was close (less than 300km) and the student answered in less than 10 seconds
  const MAX_SCORE = 10;
  const CLOSE_FOR_BONUS = 300; // se puede tunear quizás
  const TIME_FOR_BONUS = 20;
  var points = 0;
  points += difficulty / 2;
  const d = calculateDistance(lat, lon, correctLan, correctLon);

  // assign remaining points based on the distance from the correct answer (-1 that can be given based on time)
  const max_points_left = MAX_SCORE - 1 - points; 

  if (d < 50) {
      points += max_points_left;
  }
  else if (d < 100) {
      points += 0.9 * max_points_left;
  }
  else if (d < 200) {
      points += 0.8 * max_points_left;
  }
  else if (d < 300) {
      points += 0.7 * max_points_left;
  }
  else if (d < 400) {
      points += 0.6 * max_points_left;
  }
  else if (d < 500) {
      points += 0.5 * max_points_left;
  }
  else if (d < 1000) {
      points += 0.25 * max_points_left;
  }
  else if (d < 2000) {
      points += 0.1 * max_points_left;
  }

  if (d <= CLOSE_FOR_BONUS && time_left >= TIME_FOR_BONUS)
  {
      points += 1;
  }

  return points;
}

function calculateDistance(lat1, lon1, lat2, lon2)
{
  const EARTH_RADIUS = 6371; // We consider a perfectly rounded earth

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

// Convert from degrees to radians.
export function degreesToRadians (degrees) {
	return degrees * Math.PI / 180;
}

// Convert from radians to degrees.
export function radiansToDegrees (radians) {
	return radians * 180 / Math.PI;
}

export function applyCorrection (transform) {
    // Apply a correction on the phi angle
    const polarCoords = new THREE.Spherical()
    polarCoords.setFromVector3(new THREE.Vector3(transform[0], transform[1], transform[2]))
    const fixFactor = calculateFixFactor(polarCoords.phi)
    polarCoords.phi += fixFactor
    const correctedCoords = new THREE.Vector3()
    correctedCoords.setFromSpherical(polarCoords)
    return correctedCoords
}

function calculateFixFactor (phi: number) {
    let factor = 0
    // Southern-hemisphere
    if (phi == 0) {
      factor = 0.4
    } else if (phi < 0.14) {
      factor = 0.18
    } else if (phi < 0.24) {
      factor = 0.16
    } else if (phi < 0.34) {
      factor = 0.15
    } else if (phi < 0.39) {
      factor = 0.14
    } else if (phi < 0.5) {
      factor = 0.13
    } else if (phi < 0.69) {
      factor = 0.09
    } else if (phi < 0.84) {
      factor = 0.07
    } else if (phi < 0.89) {
      factor = 0.04
    } else if (phi < 0.94) {
      factor = 0.02
    } else if (phi < 1.14) {
      factor = 0.008
    } else if (phi < 1.34) {
      factor = 0.005
    } else if (phi < 1.6) {
      // ecuator
      factor = 0
    } else if (phi < 1.8) {
      // Northern-hemisphere
      factor = -0.005
    } else if (phi < 2.1) {
      // canary islands
      factor = -0.008
    } else if (phi < 2.2) {
      // florida north
      factor = -0.02
    } else if (phi < 2.25) {
      // morocco north
      factor = -0.04
    } else if (phi < 2.3) {
      // sicily
      factor = -0.07
    } else if (phi < 2.45) {
      // irun
      factor = -0.09
    } else if (phi < 2.64) {
      // london
      factor = -0.13
    } else if (phi < 2.75) {
      factor = -0.14
    } else if (phi < 2.8) {
      factor = -0.15
    } else if (phi < 2.9) {
      factor = -0.16
    } else if (phi < 3) {
      factor = -0.18
    } else if (phi <= Math.PI) {
      factor = -0.4
    }

    // if (phi >= Math.PI / 2) {
    //   factor = (-1 / (Math.PI - phi)) * 0.08
    // } else {
    //   factor = 1 / phi
    // }
    return factor
  }