export const SERVERURL = location.hostname;

// Hard-coded endpoint to point the remote machine
// export const SERVERURL = 'arete-webrtc.vicomtech.org';
export const SERVERPROTOCOL = 'http:';

export async function getServerConfig() {
  const createResponse = await fetch(SERVERPROTOCOL + '//' + SERVERURL + "/config");
  return await createResponse.json();
}

export function getRTCConfiguration() {
  let config = {};
  config.sdpSemantics = "unified-plan";
  config.iceServers = [{ urls: ["stun:stun.l.google.com:19302"] }];
  return config;
}
