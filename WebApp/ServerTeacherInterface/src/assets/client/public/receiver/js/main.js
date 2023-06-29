import { Receiver } from "./receiver.js";
import { getServerConfig } from "../../js/config.js";

setup();

let playButton;
export let receiver;
let useWebSocket;
let elementVideo;
let connected = false;

window.document.oncontextmenu = function () {
  return false;     // cancel default menu
};

window.addEventListener('resize', function () {
  if (receiver)
    receiver.resizeVideo();
}, true);

window.addEventListener('beforeunload', async () => {
  if (receiver)
    await receiver.stop();
    connected = false;
}, true);

async function setup() {
  const res = await getServerConfig();
  useWebSocket = res.useWebSocket;
  showWarningIfNeeded(res.startupMode);
  showPlayButton();
}

function showWarningIfNeeded(startupMode) {
  const warningDiv = document.getElementById("warning");
  if (startupMode == "private") {
    warningDiv.innerHTML = "<h4>Warning</h4> This sample is not working on Private Mode.";
    warningDiv.hidden = false;
  }
}

// export function showPlayButton() {
//   var playButtons = document.getElementsByClassName('play');
//   for (var i = 0; i < playButtons.length; i++) {
//     playButton = playButtons[i];
//     if (i == 0) {
//       playButton.id = "playButton_L";
//     }
//     else {
//       playButton.id = "playButton_R";
//     }
//     playButton.alt = 'Start Streaming';
//     playButton.addEventListener('click', onClickPlayButton);
//   }
// }

export function showPlayButton() {
  if (!playButton) {
    playButton = document.getElementById('playButton');
    if (playButton) {
      playButton.addEventListener('click', onClickPlayButton);
    }
  }
}

export function getCandidates() {
  return receiver.candidates;
}

export function subscribeToWebRTC() {
  onClickPlayButton();
}

export async function stopWebRTC() {
  receiver.stop();
}

// function onClickPlayButton() {
//   playButton.style.visibility = 'hidden';
//   const playerDiv = document.getElementById('player');

//   // add video player
//   elementVideo = getElementVideo()

//   prevElementVideo = elementVideo
//   previousReceiverValue = receiver;
//   setupVideoPlayer(elementVideo).then(value => receiver = value);
// }

function onClickPlayButton() {
  if (!connected) {
    // console.log('playButton', playButton);
    // if (!playButton) {
    //   playButton = document.getElementById('playButton');
    // }
    // console.log('playButton', playButton);
    // if (playButton)
    // {
    //   playButton.style.display = 'none';
    // }
    // set up video player
    elementVideo = document.getElementById('Video');
    if (elementVideo == null) {
      elementVideo = document.getElementById('VideoAndroid')
    }
    if (elementVideo != null) {
      elementVideo.style.touchAction = 'none';
      setupVideoPlayer(elementVideo).then(value => {
        receiver = value;
        connected = true;
      });
    }
  }
}

async function setupVideoPlayer(elements) {
  const videoPlayer = new Receiver(elements);
  await videoPlayer.setupConnection(useWebSocket);
  videoPlayer.ondisconnect = onDisconnect;
  return videoPlayer;
}

function getElementVideo() {
  elementVideo = document.getElementById('Video');
  if (elementVideo == null)
    elementVideo = document.getElementById('VideoAndroid')

  return elementVideo
}

function getPlayButton() {
  var playButtons = document.getElementsByClassName('play');
  for (var i = 0; i < playButtons.length; i++) {

    playButton = playButtons[i];
    if (i == 0) {
      playButton.id = "playButton_L";
    }
    else {
      playButton.id = "playButton_R";

    }
    playButton.alt = 'Start Streaming';
    return playButton
  }
}

export function onDisconnect() {
  console.log("----- onDisconnect event incoming");

  // let playButton = getPlayButton()

  // playButton.style.visibility = 'Visible';

  elementVideo = getElementVideo()
  if (receiver) {
    console.log("----- Stopping the WebRTC stream");
    receiver.stop();
    connected = false;
  }

  // showPlayButton();
}

function clearChildren(element) {
  while (element.firstChild) {
    element.removeChild(element.firstChild);
  }
}
