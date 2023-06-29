import { Injectable } from '@angular/core'
@Injectable({
  providedIn: 'root'
})
export class HelpService {
  activePlayerName: string = ''
  activeQuestion: string = ''
  isPlaying: boolean = false
  isConfirmingAnswer: boolean = false
  helpSent: boolean = false
  help2Sent: boolean = false

  hideHelpPanel () {
    this.activePlayerName = ''
    this.activeQuestion = ''
    this.isPlaying = false
    this.helpSent = false
    this.help2Sent = false
  }
}
