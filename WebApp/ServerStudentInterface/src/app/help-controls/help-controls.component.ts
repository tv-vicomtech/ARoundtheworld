import { Component, OnInit } from '@angular/core'
import { HelpService } from '../services/help.service'
import { OrkestraService } from '../services/orkestra.service'
import { QuestionService } from '../services/question.service'
import { StudentService } from '../services/student.service'
import { stripPrefix } from '../utils/utils'

@Component({
  selector: 'app-help-controls',
  templateUrl: './help-controls.component.html',
  styleUrls: [ './help-controls.component.css' ]
})
export class HelpControlsComponent implements OnInit {
  constructor (
    public orkestraService: OrkestraService,
    public studentService: StudentService,
    public questionService: QuestionService,
    public helpService: HelpService
  ) {}

  ngOnInit () {
    this.orkestraService.someoneIsPlaying.subscribe(value => {
      if (value) {
        // Game is on -> reset and enable the help panel
        this.helpService.activePlayerName = stripPrefix(this.studentService.getCurrentPlayer().agentID)
        this.helpService.activeQuestion = this.questionService.getCurrentQuestion().title
        this.helpService.isPlaying = true
      } else {
        // No one is playing yet -> hide the help panel
        this.helpService.hideHelpPanel()
      }
    })

    this.orkestraService.pendingAnswer.subscribe(value => {
      if (value) {
        // Pending answer received
        this.helpService.isConfirmingAnswer = true
        this.helpService.help2Sent = false // Can retry multiple times and thumb help can be send in each one of them
      } else {
        // Final answer received
        this.helpService.isConfirmingAnswer = false
      }
    })
  }

  sendContinentHelp (continent: string) {
    this.orkestraService.sendHelp('continent', continent)
    this.helpService.helpSent = true
  }

  sendThumbHelp (thumb: string) {
    this.orkestraService.sendHelp('thumb', thumb)
    this.helpService.help2Sent = true
  }
}
