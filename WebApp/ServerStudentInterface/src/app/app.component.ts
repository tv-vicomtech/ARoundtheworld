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
import { XapiService } from './services/xapi.service'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: [ './app.component.css' ]
})
export class AppComponent {
  title = 'ARETE Student 👩‍🎓💻'
  activeQuizName: string
  quizList: Quiz[] = []

  selectedQuizIndex: number = 0
  selectedQuestionIndex: number = 0
  selectedStudentIndex: number = 0

  quizStarted: boolean = false

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
        this.selectedQuizIndex = 0
        this.selectedQuestionIndex = 0
        this.selectedStudentIndex = 0
      }
    })
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
