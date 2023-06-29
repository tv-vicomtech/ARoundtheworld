import { Injectable } from '@angular/core'
import { Orkestra } from 'orkestraLib'
import {
  ActiveCamera_Message,
  ACTIVECAMERA_TYPE,
  ACTIVEUSER_TYPE,
  Answer_Message,
  ANSWER_TYPE,
  Notification_Message,
  NOTIFICATION_TYPE,
  Result_Message,
  RESULT_TYPE,
  TIMEOUT_TURN_TYPE,
  TimeoutTurn_Message,
  Help_Message,
  HELP_TYPE,
  PENDING_ANSWER_TYPE,
  PendingAnswer_Message,
  QUESTION_TYPE,
  Question_Message,
  SCORE_TYPE,
  ObjectTransform_Message,
  TRANSFORM_TYPE,
  CAMERA_TRANSFORM_TYPE,
  CameraTransform_Message,
  QUIZ_FINISH_TYPE
} from '../interfaces/messagesInterfaces'
import { QuestionService } from './question.service'
import { StudentService } from './student.service'
import { MessageService } from 'primeng/api'
import { Subject } from 'rxjs'
import { applyCorrection, calculateScore, convertXYZToLatLong, stripPrefix } from '../utils/utils'
import { PREFIX_UNITY_STUDENT } from '../interfaces/constants'
import { XapiService } from './xapi.service'
import { HelpService } from './help.service'
import { Student } from '../interfaces/student'

@Injectable({
  providedIn: 'root'
})
export class OrkestraService {
  orkestra: Orkestra
  sessionId: string
  login: boolean = false
  helpMap: Map<string, number> = new Map<string, number>()
  someoneIsPlaying: Subject<boolean> = new Subject<boolean>()
  pendingAnswer: Subject<boolean> = new Subject<boolean>()
  earthTransformSubject: Subject<ObjectTransform_Message> = new Subject<ObjectTransform_Message>()
  cameraTransformSubject: Subject<CameraTransform_Message> = new Subject<CameraTransform_Message>()
  earthPinSubject: Subject<PendingAnswer_Message> = new Subject<PendingAnswer_Message>()
  clearPinsSubject: Subject<boolean> = new Subject<boolean>()
  finishQuizSubject: Subject<boolean> = new Subject<boolean>()
  lastQuestion: boolean = false
  activeQuizName: string
  quizEnded = false
  pendingAnswerData: PendingAnswer_Message = null

  constructor (
    public studentService: StudentService,
    public questionService: QuestionService,
    public messageService: MessageService,
    public xapiService: XapiService,
    public helpService: HelpService
  ) {}

  getAgentid (): string {
    return this.orkestra.users.get('me').agentid
  }

  getSessionId (): string {
    return this.sessionId
  }

  connect (agentID_default: string, room_default: string): void {
    const protocol = window.location.protocol === 'https:' ? 'https' : 'http'
    // Connect to orkestra
    this.orkestra = new Orkestra({
      url: `${protocol}://${window.location.hostname}:3000/`,
      agentid: agentID_default,
      channel: room_default,
      privateChannel: false,
      profile: 'viewer',
      master: true
    })

    this.orkestra.userObservable.subscribe((userInfo: any) => {
      this.manageEvents(userInfo)
    })

    this.orkestra.appObservable.subscribe((z: any) => {
      this.manageEvents(z)
    })

    // Initialize help maps
    this.resetContinentHelpMap()
    this.resetThumbHelpMap()

    this.sessionId = room_default

    // Send statement of the action
    this.xapiService.sendStatement(this.getAgentid(), 'Logged In', this.getSessionId())

    this.login = true
  }

  registerEvents () {
    this.orkestra.setAppAttribute(QUESTION_TYPE, 'undefined')
    this.orkestra.setAppAttribute(ANSWER_TYPE, 'undefined')
    this.orkestra.setAppAttribute(RESULT_TYPE, 'undefined')
    this.orkestra.setAppAttribute(NOTIFICATION_TYPE, 'undefined')
    this.orkestra.setAppAttribute(HELP_TYPE, 'undefined')
    this.orkestra.setAppAttribute(SCORE_TYPE, 'undefined')
    this.orkestra.setAppAttribute(QUIZ_FINISH_TYPE, 'undefined')
  }

  sendActiveUser (userName: string) {
    this.orkestra.setAppAttribute(ACTIVEUSER_TYPE, {
      sender: this.getAgentid(),
      type: ACTIVEUSER_TYPE,
      username: userName
    })
  }

  manageEvents (json: any) {
    try {
      const obj = JSON.parse(JSON.stringify(json))
      if (obj.data) {
        if (obj.data.value) {
          if (obj.data.value !== 'undefined' && obj.key !== undefined) {
            if (obj.key == ANSWER_TYPE) {
              // An answer has been submitted
              const answer: Answer_Message = JSON.parse(json.data.value)
              console.log('----- Answer', answer)
              this.pendingAnswerData = null
              // Convert answer data to lat and lon (apply the phi correction before getting the lat & lon values)
              const answerCoordsCorrected = applyCorrection([ answer.px, answer.py, answer.pz ])
              const result = convertXYZToLatLong(
                answerCoordsCorrected.x,
                answerCoordsCorrected.y,
                answerCoordsCorrected.z
              )
              const answerLat = result[0]
              const answerLon = result[1]

              const activeQuestion = this.questionService.getCurrentQuestion()
              // Validate answer
              const correctAnswer = activeQuestion.correctAnswer
              let answerScore: number = calculateScore(
                answerLat,
                answerLon,
                correctAnswer.lat,
                correctAnswer.lon,
                activeQuestion.difficulty,
                answer.time_left
              )
              answerScore = Number(answerScore.toFixed(2))

              // Add answer to the dashboard
              this.studentService.assignAnswer(answer.sender, {
                x: answer.px.toString(),
                y: answer.py.toString(),
                z: answer.pz.toString(),
                lat: answerLat.toString(),
                lon: answerLon.toString(),
                screenshot: answer.screenshot,
                score: answerScore.toString(),
                time: answer.time_left.toFixed(0).toString()
              })

              // Add points to the dashboard
              this.studentService.updateStudentScore(answer.sender, answerScore)

              // Send the score to the student to update his/her UI
              this.sendScoreUpdate(answer.sender, this.studentService.getStudentScore(answer.sender))

              // Send statement of the action
              this.xapiService.sendStatement(
                answer.sender,
                'Sent',
                `(${answer.px.toString()}, ${answer.py.toString()}, ${answer.pz.toString()}), (${answerLat.toString()}, ${answerLon.toString()}) ${answer.time_left
                  .toFixed(0)
                  .toString()}`
              )

              // Send statement of the action
              this.xapiService.sendStatement(this.getAgentid(), 'Checked', JSON.stringify(answer))

              // Send statement of the action
              this.xapiService.sendStatement(
                this.getAgentid(),
                'Assigned',
                `${answerScore.toString()};${stripPrefix(this.studentService.getCurrentPlayer().agentID)}`
              )

              // Remove the pins from the earth
              this.clearPins()

              // Send next question to the next student after a small delay
              this.studentService.advanceStudentIndex()
              this.questionService.advanceQuestionIndex()
              setTimeout(this.sendQuestion.bind(this), 3000)

              // Disable the continent help panel
              this.pendingAnswer.next(false)
              this.someoneIsPlaying.next(false)
              this.resetContinentHelpMap()
              this.resetThumbHelpMap()
            } else if (obj.key == NOTIFICATION_TYPE) {
              let notification: Notification_Message = JSON.parse(JSON.stringify(json.data.value))
              if (!notification.sender) {
                notification = JSON.parse(json.data.value)
              }
              console.log('----- Notification', notification)
              const player = this.studentService.getCurrentPlayer()

              if (
                notification.content &&
                notification.content.includes('accepted the new question') &&
                notification.sender &&
                notification.sender === player.agentID
              ) {
                // Send statement of the action
                this.xapiService.sendStatement(this.getAgentid(), 'Set Turn', stripPrefix(player.agentID))

                // Enable the continent help panel
                this.someoneIsPlaying.next(true)
              }

              if (notification.content && notification.content.includes('rejected the answer')) {
                if (this.pendingAnswerData) {
                  const pendingAnswerCoordsCorrected = applyCorrection([
                    this.pendingAnswerData.px,
                    this.pendingAnswerData.py,
                    this.pendingAnswerData.pz
                  ])

                  const result = convertXYZToLatLong(
                    pendingAnswerCoordsCorrected.x,
                    pendingAnswerCoordsCorrected.y,
                    pendingAnswerCoordsCorrected.z
                  )
                  const pendingAnswerLat = result[0]
                  const pendingAnswerLon = result[1]

                  // Send statement of the action
                  this.xapiService.sendStatement(
                    this.pendingAnswerData.sender,
                    'Canceled',
                    `(${pendingAnswerCoordsCorrected.x.toString()}, ${pendingAnswerCoordsCorrected.y.toString()}, ${pendingAnswerCoordsCorrected.z.toString()}), (${pendingAnswerLat.toString()}, ${pendingAnswerLon.toString()})`
                  )
                }

                this.clearPins()
                this.pendingAnswer.next(false)
                this.resetThumbHelpMap()
              }

              if (
                notification.content &&
                notification.content.includes('rejected the new question') &&
                notification.sender &&
                notification.sender === player.agentID
              ) {
                // send the same question to the next player
                this.studentService.advanceStudentIndex()
                this.sendQuestion()
              }

              // Display a modal with the message
              this.messageService.add({
                severity: 'info',
                summary: `${stripPrefix(notification.sender)}: `,
                detail: stripPrefix(notification.content)
              })
            } else if (obj.key == TIMEOUT_TURN_TYPE) {
              const timeoutTurnMessage: TimeoutTurn_Message = JSON.parse(JSON.stringify(json.data.value))
              console.log('----- TimeoutTurn', timeoutTurnMessage)
              // Prepare the next question to be sent automatically
              // If there is only one player, send the next question, if not, send the same question to the next player
              const playingStudents = this.studentService.students.filter(s => s.agentID.includes(PREFIX_UNITY_STUDENT))
              if (playingStudents.length === 1) {
                // send the next question to the same player
                this.questionService.advanceQuestionIndex()
              } else if (playingStudents.length > 1) {
                // send the same question to the next player
                this.studentService.advanceStudentIndex()
              } else {
                // no players ???
                console.warn('Are there no players?')
                return
              }

              this.clearPins()
              this.sendQuestion()

              // Reset the help panels
              this.someoneIsPlaying.next(false)
              this.pendingAnswer.next(false)
              this.resetContinentHelpMap()
              this.resetThumbHelpMap()
            } else if (obj.key == HELP_TYPE) {
              let helpMessage: Help_Message = JSON.parse(JSON.stringify(json.data.value))
              console.log('----- HelpMessage', helpMessage)
              if (!helpMessage.sender) {
                helpMessage = JSON.parse(json.data.value)
              }
              this.registerHelpMessage(helpMessage.content)

              // Validate the help sent in case it adds points to the observer
              const score = this.studentService.updateObserverStudentScore(
                helpMessage.sender,
                helpMessage.content,
                this.questionService.getCurrentQuestion()
              )

              // Send the score back to the helper if it got ay points
              if (score > 0 && helpMessage.sender.includes(PREFIX_UNITY_STUDENT)) {
                // Send the score to the student to update his/her UI
                this.sendScoreUpdate(helpMessage.sender, this.studentService.getStudentScore(helpMessage.sender))
              }
            } else if (obj.key == PENDING_ANSWER_TYPE) {
              let pendingAnswerMessage: PendingAnswer_Message = JSON.parse(JSON.stringify(json.data.value))
              if (!pendingAnswerMessage.sender) {
                pendingAnswerMessage = JSON.parse(json.data.value)
              }
              console.log('----- Pending answer', pendingAnswerMessage)
              this.pendingAnswerData = pendingAnswerMessage
              const pendingAnswerCoordsCorrected = applyCorrection([
                pendingAnswerMessage.px,
                pendingAnswerMessage.py,
                pendingAnswerMessage.pz
              ])

              const result = convertXYZToLatLong(
                pendingAnswerCoordsCorrected.x,
                pendingAnswerCoordsCorrected.y,
                pendingAnswerCoordsCorrected.z
              )
              const pendingAnswerLat = result[0]
              const pendingAnswerLon = result[1]

              // Send statement of the action
              this.xapiService.sendStatement(
                pendingAnswerMessage.sender,
                'Placed',
                `(${pendingAnswerCoordsCorrected.x.toString()}, ${pendingAnswerCoordsCorrected.y.toString()}, ${pendingAnswerCoordsCorrected.z.toString()}), (${pendingAnswerLat.toString()}, ${pendingAnswerLon.toString()})`
              )

              this.pendingAnswer.next(true)
              // Create a pin in the earth's model
              this.earthPinSubject.next(pendingAnswerMessage)
              this.resetThumbHelpMap()
            } else if (obj.key == TRANSFORM_TYPE) {
              let objectTransformMessage: ObjectTransform_Message = JSON.parse(JSON.stringify(json.data.value))
              if (!objectTransformMessage.name) {
                objectTransformMessage = JSON.parse(json.data.value)
              }
              // console.log('----- Object transform', objectTransformMessage)
              this.earthTransformSubject.next(objectTransformMessage)
            } else if (obj.key == CAMERA_TRANSFORM_TYPE) {
              let cameraTransformMessage: CameraTransform_Message = JSON.parse(JSON.stringify(json.data.value))
              if (!cameraTransformMessage.name) {
                cameraTransformMessage = JSON.parse(json.data.value)
              }
              // console.log('----- Camera transform', cameraTransformMessage)
              this.cameraTransformSubject.next(cameraTransformMessage)
            }
          }
        }
      }
      if (obj.evt == 'agent_join' && obj.data.agentid != this.getAgentid()) {
        // Add to students list
        this.studentService.addStudent({
          agentID: obj.data.agentid,
          questionsAsked: new Map(),
          score: 0,
          mobileDevice: false
        })
        // console.log(`Agent joined: ${obj.data.agentid}`)
      }
      if (obj.evt == 'agent_left' && obj.data.agentid != this.getAgentid()) {
        // Remove from students list
        this.studentService.removeStudent(obj.data.agentid)
        console.log(`Agent left: ${obj.data.agentid}`)
      }
    } catch (e) {
      console.error(e)
    }
  }

  sendQuestion () {
    if (!this.lastQuestion) {
      const activeQuestion = this.questionService.getCurrentQuestion()
      const player = this.studentService.getCurrentPlayer()

      const q: Question_Message = {
        type: QUESTION_TYPE,
        sender: this.getAgentid(),
        username: player.agentID,
        content: activeQuestion.title,
        correctAnswer: activeQuestion.correctAnswer.continent,
        id: this.questionService.currentQuestionIndex.toString(),
        quiz: this.questionService.selectedQuizIndex.toString()
      }

      // Send statement of the action
      this.xapiService.sendStatement(
        this.getAgentid(),
        'Asked',
        `${this.questionService.selectedQuizIndex.toString()}/${this.questionService.currentQuestionIndex.toString()}`
      )

      // Add the question to the list of asked questions of the student
      this.studentService.assignQuestion(activeQuestion, player)

      // Send to orkestra
      this.orkestra.setAppAttribute(QUESTION_TYPE, q)
      this.sendNotification(`A question has been sent to ${stripPrefix(player.agentID)}`)

      if (this.questionService.getIsLastQuestion()) {
        this.lastQuestion = true // LAST QUESTION!!!!
      }
    } else {
      this.finishQuiz()
    }
  }

  finishQuiz () {
    // End quiz with a message
    this.messageService.add({
      severity: 'success',
      summary: 'Finished',
      detail: 'Quiz completed!'
    })

    // Send statement of the action
    this.xapiService.sendStatement(
      this.getAgentid(),
      'Ended',
      `${this.questionService.selectedQuizIndex.toString()}/${this.activeQuizName}`
    )

    // Reset the state of the game to start another quiz
    this.quizEnded = true
    this.lastQuestion = false
    // Reset the selected quiz and questions
    this.questionService.resetQuizData()
    // Reset the help UI and maps
    this.resetContinentHelpMap()
    this.resetThumbHelpMap()
    this.helpService.hideHelpPanel()
    // Reset the UI
    this.finishQuizSubject.next(true)

    // Send the orkestra message
    this.orkestra.setAppAttribute(QUIZ_FINISH_TYPE, {
      sender: this.getAgentid(),
      type: QUIZ_FINISH_TYPE,
      quiz: this.questionService.getActiveQuiz().quizName
    })
  }

  clearPins () {
    this.clearPinsSubject.next(true)
  }

  sendNotification (content: string) {
    this.orkestra.setAppAttribute(NOTIFICATION_TYPE, {
      sender: this.getAgentid(),
      type: NOTIFICATION_TYPE,
      content
    })
  }

  sendScoreUpdate (name: string, score: string) {
    this.orkestra.setAppAttribute(SCORE_TYPE, {
      sender: this.getAgentid(),
      type: SCORE_TYPE,
      name,
      score
    })
  }

  resetContinentHelpMap () {
    this.helpMap.set('africa', 0)
    this.helpMap.set('antarctica', 0)
    this.helpMap.set('asia', 0)
    this.helpMap.set('oceania', 0)
    this.helpMap.set('europe', 0)
    this.helpMap.set('north-america', 0)
    this.helpMap.set('south-america', 0)
  }

  resetThumbHelpMap () {
    this.helpMap.set('up', 0)
    this.helpMap.set('down', 0)
  }

  sendHelp (type: string, content: string) {
    this.orkestra.setAppAttribute(HELP_TYPE, {
      sender: this.getAgentid(),
      type: HELP_TYPE,
      helpType: type,
      content
    })
    // Send statement of the action
    this.xapiService.sendStatement(this.getAgentid(), 'Suggested', content)
  }

  registerHelpMessage (value: string) {
    this.helpMap.set(value, this.helpMap.get(value) + 1)
  }

  sendResult (student: Student) {
    const r: Result_Message = {
      type: RESULT_TYPE,
      sender: this.getAgentid(),
      value: student.score.toString()
    }
    // Send to orkestra
    this.orkestra.setUserContextData(student.agentID, RESULT_TYPE, r)
  }

  disconnect () {
    this.login = false
  }

  sendActiveCamera (student: Student, value: boolean) {
    const ac: ActiveCamera_Message = {
      sender: this.getAgentid(),
      type: ACTIVECAMERA_TYPE,
      value,
      user: student.agentID
    }
    this.orkestra.setAppAttribute(ACTIVECAMERA_TYPE, ac)
  }
}
