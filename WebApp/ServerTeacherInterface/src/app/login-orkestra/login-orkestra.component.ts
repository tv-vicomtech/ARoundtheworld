import { Component, Input, OnInit } from '@angular/core'
import { OrkestraSession } from '../interfaces/orkestraSession'
import { Router } from '@angular/router'
import { OrkestraService } from '../services/orkestra.service'
import { PREFIX_WEB_TEACHER } from '../interfaces/constants'

@Component({
  selector: 'app-login-orkestra',
  templateUrl: './login-orkestra.component.html',
  styleUrls: [ './login-orkestra.component.css' ]
})
export class LoginOrkestraComponent implements OnInit {
  public orkestraSession!: OrkestraSession
  @Input() room_default?: string
  @Input() agentID_default?: string
  errorMsg = ''

  constructor (private route: Router, public orkestraService: OrkestraService) {}

  ngOnInit (): void {
    this.agentID_default = 'Teacher'
    this.room_default = 'Xabier Zubiri Manteo'
  }

  public onLogin (): void {
    if (this.agentID_default.length > 2 && this.room_default.length > 2) {
      this.errorMsg = ''
      this.orkestraService.connect(PREFIX_WEB_TEACHER + this.agentID_default, this.room_default)
      this.route.navigate([ '/dashboard' ])
    } else {
      this.errorMsg = 'Please insert a valid name and session name, with a minimum of 3 characters'
    }
  }
}
