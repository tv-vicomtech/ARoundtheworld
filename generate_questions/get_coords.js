let wiki_version = "es";

function set_wiki_version(val) {
    if (val === "en")
        wiki_version = "en";
    else
        wiki_version = "es"; // de momento solo español o inglés
}

function fill_with_lat_lon() {
    const COL_LAT = 3;
    const COL_LON = 4;

    console.log("Fill with lat lon - Table");
    var table = document.getElementById('table-body');
    if (table) {
        console.log("Processing table elements")
        console.log("There are " + table.rows.length + " rows")
        for (let row of table.rows) 
        {
            let location = row.cells[0].innerText
            console.log("Current location: " + location);

            var url = "https://" + wiki_version + ".wikipedia.org/w/api.php?action=query&prop=coordinates&titles=" + location + "&format=json&origin=*"; 

            const getJsonData = async() => {
                let data = await fetch(url).then((response) => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok')
                    }
                    return response.json();
                })
                .then((json_) => {
                    console.log("json: ", json_)
                    return json_.query.pages;
                })
                .catch((error) => {
                    console.error('There has been a problem with your fecth operation:', error);
                })
                return data;
            }
            
            const getData = async () => {
                let pages = await getJsonData()
                console.log("data: ", pages);
                var page = pages[Object.keys(pages)[0]];
                var lat = page.coordinates[0].lat
                var lon = page.coordinates[0].lon
                console.log("Latitute: " + lat);
                console.log("Longitude: " + lon);
                row.cells[COL_LAT].innerText = (Math.round(lat * 100) / 100).toFixed(2);
                row.cells[COL_LON].innerText = (Math.round(lon * 100) / 100).toFixed(2);
            }

            getData();
        }
    }
  }

  const download_to_file = (content, filename, contentType) => {
    const a = document.createElement('a');
    const file = new Blob([content], {type: contentType});
    
    a.href= URL.createObjectURL(file);
    a.download = filename;
    a.click();
    
    URL.revokeObjectURL(a.href);
  };

  function save_json() {
    const contentType = "text/plain"
    const filename = document.getElementById("basic-name").value + ".json";

    const content = new Object();
    content.quizName = filename;
    console.log("Content object: ", content);

    const COL_LOCATION = 0;
    const COL_DIFF = 1;
    const COL_CONT = 2;
    const COL_LAT = 3;
    const COL_LON = 4;

    const questions = [];

    var table = document.getElementById('table-body');
    if (table) {
        for (let row of table.rows) 
        {
            let loc = row.cells[COL_LOCATION].innerText;
            let dif = row.cells[COL_DIFF].innerText;
            let con = row.cells[COL_CONT].querySelector('.cont').value.toUpperCase();
            let lat = row.cells[COL_LAT].innerText;
            let lon = row.cells[COL_LON].innerText;


            const q = {
                location: loc,
                continent: con,
                difficulty: dif,
                latitude: lat,
                longitude: lon
                };

            questions.push(q);
        }
    }
    content.questions = questions;
    
    content_str = JSON.stringify(content);

    console.log("content: ", content_str);

    download_to_file(content_str, filename, contentType);
  }