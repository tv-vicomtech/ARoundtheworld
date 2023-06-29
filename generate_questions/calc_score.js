function score(lat, lon, lat_correcta, lon_correcta, dificultad, time_left)
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
    points += dificultad/2;
    const d = distance(lat, lon, lat_correcta, lon_correcta);
    console.log("Distance is ", d);

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



//test chungos, descomentar para hacer más pruebas
const runUnitTests = () => {
    // const a1 = score(40, -70, 40, -70, 5, 1);
    // render(a1);
    // const a2 = score(40, -70, 40, -70, 5, 25);
    // render(a2);
    // const a3 = score(40, -70, 40, 70, 5, 25);
    // render(a3);
    // const a4 = score(40, -70, 40, 70, 2, 25);
    // render(a4);
    // const a5 = score(40, -70, 40, 70, 4, 5);
    // render(a5);
    // const a6 = score(40, -70, 40, -69.5, 5, 21);
    // render(a6);
    // const a7 = score(40, -70, 40, -69, 5, 1);
    // render(a7);
    // const a8 = score(40, -70, 40, -68, 5, 1);
    // render(a8);
    // const a9 = score(40, -70, 40, -67, 5, 1);
    // render(a9);
    // const a10 = score(40, -70, 40, -63, 5, 1);
    // render(a10);
    const a11 = score(47.87, 13.18, 41.90, 12.45, 0, 34);
    render(a11);
    const a12 = score(48.99, 0.77, 42.55, 1.58, 0, 12);
    render(a12);
    const a13 = score(54.38, 9.54, 47.15, 9.55, 0, 40);
    render(a13);
    const a14 = score(1.94, 104.70, 1.28, 103.83, 0, 26);
    render(a14);
    const a15 = score(5.17, 73.72, 4.18, 73.51, 0, 40);
    render(a15);
    const a16 = score(-29.09, 30.21, -26.50, 31.50, 0, 26);
    render(a16);
    const a17 = score(4.19, 178.90, 9.82, 169.29, 0, 28);
    render(a17);
    const a18 = score(-5.07, 176.25, -8.52, 179.20 , 0, 34);
    render(a18);
}

const renderItem = (head, text) => {
  const li = document.createElement('li');
  li.append(text)
  head.append(li)
}

const render = (a) => {
  const el = document.querySelector('.output');
 
  renderItem(el, `Score: ${a}`);

}
  
runUnitTests();