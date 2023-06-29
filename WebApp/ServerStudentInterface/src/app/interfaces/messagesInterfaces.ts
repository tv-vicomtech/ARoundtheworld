/**
 * Interfaces for the mesages used to send and receive information through Orkestra Server
 **/
export interface ActiveUser_Message {
  type: string
  sender: string
  username: string
}

export interface Answer_Message {
  type: string
  sender: string
  px: number
  py: number
  pz: number
  screenshot: string
  time_left: number
}

export interface StatusWebRTC_Message {
  type: string
  sender: string
  status: string
}

export interface Question_Message {
  type: string
  sender: string
  content: string
  username: string
  correctAnswer: string
  id: string
  quiz: string
}

export interface Result_Message {
  type: string
  sender: string
  value: string
}

export interface ActiveCamera_Message {
  type: string
  sender: string
  value: boolean
  user: string
}

export interface Notification_Message {
  type: string
  sender: string
  content: string
  timestamp: number
}

export interface MobileDevice_Message {
  type: string
  sender: string
  device: string
}

export interface Suggestion_Message {
  type: string
  sender: string
  value: string
}

export interface TimeoutTurn_Message {
  type: string
  sender: string
  username: string
}

export interface Help_Message {
  type: string
  sender: string
  helpType: string
  content: string
}

export interface PendingAnswer_Message {
  type: string
  sender: string
  px: number
  py: number
  pz: number
  rx: number
  ry: number
  rz: number
  sx: number
  sy: number
  sz: number
  screenshot: string
}

export interface ObjectTransform_Message {
  posX: number
  posY: number
  posZ: number
  scaleX: number
  scaleY: number
  scaleZ: number
  rotX: number
  rotY: number
  rotZ: number
  name: string
  quat: number[]
}

export interface CameraTransform_Message extends ObjectTransform_Message {
  fieldOfView: number
  posX2: number
  posY2: number
  posZ2: number
  camQuat: number[]
}

export interface QuizFinish_Message {
  type: string
  sender: string
  quiz: string
}

export const QUESTION_TYPE = 'Question'
export const RESULT_TYPE = 'Result'
export const ANSWER_TYPE = 'Answer'
export const STATUSWEBRTC = 'StatusWebRTC'
export const ACTIVEUSER_TYPE = 'ActiveUser'
export const NOTIFICATION_TYPE = 'Notification'
export const ACTIVECAMERA_TYPE = 'ActiveCamera'
export const MOBILEDEVICE_TYPE = 'MobileDevice'
export const TIMEOUT_TURN_TYPE = 'TimeoutTurn'
export const HELP_TYPE = 'HelpMessage'
export const PENDING_ANSWER_TYPE = 'PendingAnswer'
export const SCORE_TYPE = 'ScoreMessage'
export const TRANSFORM_TYPE = 'ObjectTransform'
export const CAMERA_TRANSFORM_TYPE = 'CameraTransform'
export const QUIZ_FINISH_TYPE = 'QuizFinishMessage'
