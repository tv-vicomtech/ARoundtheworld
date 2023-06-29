import { Component, OnInit } from '@angular/core'
import { Question } from '../interfaces/question'
import { Student } from '../interfaces/student'
import { StudentService } from '../services/student.service'
import { KeyValue } from '@angular/common'
import { OrkestraService } from '../services/orkestra.service'
import { Subject, takeUntil } from 'rxjs'
import { ScriptService } from '../services/script.service'

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: [ './dashboard.component.css' ]
})
export class DashboardComponent implements OnInit {
  students: Student[] = []
  maxTime = 40

  displayedColumns: string[] = [ 'AgentID', 'Questions', 'Difficulty', 'Answers', 'Time' ]
  destroyed = new Subject<void>()

  constructor (
    public studentService: StudentService,
    public orkestraService: OrkestraService,
    public scriptService: ScriptService
  ) {}

  ngOnInit (): void {
    this.getStudents()
  }

  getStudents (): void {
    this.studentService.studentsDataSource.data = this.studentService.students

    this.studentService
      .getStudents()
      .pipe(takeUntil(this.destroyed))
      .subscribe((stu: Student[]) => {
        this.studentService.studentsDataSource.data = stu
      })
  }

  ngOnDestroy (): void {
    this.destroyed.next()
    this.destroyed.complete()
  }

  originalOrder = (a: KeyValue<Question, string>, b: KeyValue<Question, string>): number => {
    return 0
  }
}
