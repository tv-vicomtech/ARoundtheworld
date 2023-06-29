import { Component } from '@angular/core'
import { Quiz } from './interfaces/question'
import { QuestionService } from './services/question.service'
import { StudentService } from './services/student.service'
import { OrkestraService } from './services/orkestra.service'
import { MessageService } from 'primeng/api'
import { Subject } from 'rxjs'
import { MatIconRegistry } from '@angular/material/icon'
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser'
import { Router } from '@angular/router'
import { ScriptService } from './services/script.service'
import { PREFIX_UNITY_STUDENT } from './interfaces/constants'
import { XapiService } from './services/xapi.service'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: [ './app.component.css' ]
})
export class AppComponent {
  title = 'ARETE Teacher 👩‍🏫'
  activeQuizName: string
  quizList: Quiz[] = []

  selectedQuizIndexUI: number = 0
  selectedQuizIndex: number = 0
  selectedQuestionIndex: number = 0
  selectedStudentIndex: number = 0

  quizStarted: boolean = false
  lastQuestion: boolean = false

  destroyed = new Subject<void>()

  private path: string = '../assets/'

  constructor (
    private route: Router,
    private domSanitizer: DomSanitizer,
    private matIconRegistry: MatIconRegistry,
    private questionService: QuestionService,
    public studentService: StudentService,
    public orkestraService: OrkestraService,
    public messageService: MessageService,
    public scriptService: ScriptService,
    public xapiService: XapiService
  ) {
    this.matIconRegistry.addSvgIcon('logout', this.setPath(`${this.path}/logout-svgrepo-com.svg`))
    this.matIconRegistry.addSvgIcon('delete', this.setPath(`${this.path}/mcol_cross.svg`))
  }

  private setPath (url: string): SafeResourceUrl {
    return this.domSanitizer.bypassSecurityTrustResourceUrl(url)
  }

  ngOnInit (): void {
    this.quizList = this.questionService.loadQuizList()
    console.log('this.quizList', this.quizList)

    this.orkestraService.finishQuizSubject.subscribe(value => {
      if (value) {
        this.quizStarted = false
        this.lastQuestion = false
        this.selectedQuizIndexUI = 0
        this.selectedQuizIndex = 0
        this.selectedQuestionIndex = 0
        this.selectedStudentIndex = 0
      }
    })
  }

  canStart (): boolean {
    return (
      this.studentService.students.filter(s => s.agentID.includes(PREFIX_UNITY_STUDENT)).length > 0 &&
      this.selectedQuizIndexUI > 0
    )
  }

  startQuiz (): void {
    // Empty the student question data (if any)
    this.studentService.resetQuizData()
    this.messageService.add({
      severity: 'info',
      summary: 'Started',
      detail: 'Quiz started!'
    })
    if (this.selectedQuizIndexUI && this.selectedQuizIndexUI > 0) {
      // Load the selected quiz and its questions
      this.selectedQuizIndex = this.selectedQuizIndexUI - 1
      this.questionService.selectedQuizIndex = this.selectedQuizIndex
      this.activeQuizName = this.questionService.getQuizName(this.selectedQuizIndex)

      this.orkestraService.activeQuizName = this.activeQuizName
      this.questionService.loadActiveQuizQuestions()

      console.log('----- Quiz questions', this.questionService.questions)

      // Selection from the form
      if (this.selectedStudentIndex > 0) {
        this.studentService.currentPlayerIndex = this.selectedStudentIndex
      }
      if (this.selectedQuestionIndex > 0) {
        this.questionService.currentQuestionIndex = this.selectedStudentIndex
      }

      // Disable buttons
      this.quizStarted = true
      this.orkestraService.quizEnded = false

      // Send statement of the action
      this.xapiService.sendStatement(this.orkestraService.getAgentid(), 'Started', this.activeQuizName)

      // Send the first question of the game
      this.sendQuestion()
    } else {
      console.error('ERROR LOADING THE QUIZ INFO!')
    }
  }

  sendQuestion (): void {
    if (!this.lastQuestion) {
      this.orkestraService.sendQuestion()

      if (this.questionService.getIsLastQuestion()) {
        this.lastQuestion = true // last question
      }
    } else {
      this.orkestraService.finishQuiz()
    }
  }

  downloadResults (): void {
    console.log('DOWNLOADING RESULTS...')
    const downloadData = this.studentService.getStudentDownloadData()
    const csvContent = `data:text/csv;charset=utf-8,${downloadData}`
    const encodedUri = encodeURI(csvContent)
    const link = document.createElement('a')
    link.setAttribute('href', encodedUri)
    link.setAttribute('download', 'my_data.csv')
    document.body.appendChild(link) // Required for FF

    link.click() // The download occurs here
  }

  logout () {
    // Send statement of the action
    this.xapiService.sendStatement(this.orkestraService.getAgentid(), 'Logged Out', this.orkestraService.getSessionId())
    this.orkestraService.disconnect()
    this.studentService.disconnect()
    this.route.navigate([ '/' ])
  }

  ngOnDestroy (): void {
    this.destroyed.next()
    this.destroyed.complete()
  }
}
