import { Injectable } from '@angular/core'
import { Observable, Subject } from 'rxjs'
import { Student } from '../interfaces/student'
import { Question } from '../interfaces/question'
import { Answer_Coords } from '../interfaces/answer'
import { PREFIX_UNITY_STUDENT, PREFIX_WEB_STUDENT, PREFIX_WEB_TEACHER } from '../interfaces/constants'
import { color, stripPrefix } from '../utils/utils'
import { MatTableDataSource } from '@angular/material/table'

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  students: Student[] = []
  studentSubject = new Subject<Student[]>()
  studentsDataSource = new MatTableDataSource<Student>([])
  chartSubject = new Subject()
  currentPlayerIndex: number = 0
  constructor () {}

  getStudents (): Observable<Student[]> {
    return this.studentSubject.asObservable()
  }

  getChartSubject (): Observable<any> {
    return this.chartSubject.asObservable()
  }

  getStudentNameList (type: string): Array<string> {
    const resultList = []
    this.students.map(student => {
      if (student.agentID.includes(PREFIX_WEB_TEACHER)) {
        return
      }
      if (type === 'player' && student.agentID.includes(PREFIX_UNITY_STUDENT)) {
        resultList.push(stripPrefix(student.agentID))
      } else if (type === 'observer' && student.agentID.includes(PREFIX_WEB_STUDENT)) {
        resultList.push(stripPrefix(student.agentID))
      }
    })
    return resultList
  }

  getStudentColorList (): Array<string> {
    const resultList = []
    this.students.map(student => {
      if (student.agentID.includes(PREFIX_WEB_TEACHER)) {
        return
      }
      resultList.push(student.color)
    })
    return resultList
  }

  getStudentScoreList (type: string): Array<number> {
    const resultList = []
    this.students.map(student => {
      if (student.agentID.includes(PREFIX_WEB_TEACHER)) {
        return
      }
      if (type === 'player' && student.agentID.includes(PREFIX_UNITY_STUDENT)) {
        resultList.push(student.score)
      } else if (type === 'observer' && student.agentID.includes(PREFIX_WEB_STUDENT)) {
        resultList.push(student.score)
      }
    })
    return resultList
  }

  getPlayingStudents (): Student[] {
    return this.students.filter(s => s.agentID.includes(PREFIX_UNITY_STUDENT))
  }

  getStudent (agentID: String): Student {
    return this.students.find(q => q.agentID === agentID)!
  }

  getStudentScore (pStudent: string): string {
    // Find the student in collection
    let result = 0
    const student = this.getStudent(pStudent)
    if (student) {
      result = student.score
    }
    return result.toString()
  }

  // Active students are taken from the playingStudents list, not from the students list
  advanceStudentIndex (): number {
    if (this.currentPlayerIndex >= this.getPlayingStudents().length - 1) {
      this.currentPlayerIndex = 0
    } else {
      this.currentPlayerIndex += 1
    }
    return this.currentPlayerIndex
  }

  getCurrentPlayer (): Student {
    return this.getPlayingStudents()[this.currentPlayerIndex]
  }

  assignQuestion (question: Question, pStudent: Student): void {
    // Get the student by agentId
    const student = this.getStudent(pStudent.agentID)
    if (student) {
      question.validated = false
      student.questionsAsked.set(question, {
        lat: '',
        lon: '',
        screenshot: ''
      })
    }
  }

  getStudentDownloadData (): string {
    return this.students
      .map(
        student =>
          `${student.agentID.replace(PREFIX_WEB_STUDENT, '').replace(PREFIX_UNITY_STUDENT, '')},${student.score}`
      )
      .join('\n')
  }

  assignAnswer (pStudent: string, answer: Answer_Coords): void {
    // Find the student in collection
    const student = this.getStudent(pStudent)
    if (student) {
      // Get last Question Asked
      const lastAskedQuestion: [Question, Answer_Coords] = Array.from(student.questionsAsked).pop()

      if (lastAskedQuestion && lastAskedQuestion[0]) {
        const question = lastAskedQuestion[0]
        // Update the answer
        student.questionsAsked.set(question, answer)
      }
    }
  }

  updateStudentScore (pStudent: string, score: number): void {
    // Find the student in collection
    const student = this.getStudent(pStudent)
    if (student) {
      student.score += score
      this.chartSubject.next(true)
    }
  }

  updateObserverStudentScore (pStudent: string, answer: string, question: Question): number {
    const student = this.getStudent(pStudent)
    const correctPoints = 1
    let resultScore = 0
    // A conversion map may be needed to fit the format of the answer to the question data
    if (question.correctAnswer.continent && question.correctAnswer.continent == answer) {
      student.score += correctPoints
      resultScore = student.score
    }
    return resultScore
  }

  addStudent (student: Student): void {
    if (student.agentID.includes(PREFIX_WEB_TEACHER)) {
      console.log(`Teacher connected: ${student.agentID}`)
      return
    }
    student.color = color(this.students.length)
    this.students.push(student)
    this.studentSubject.next(this.students)
    this.chartSubject.next(true)
  }

  removeStudent (agentid: string): void {
    this.students = this.students.filter(s => s.agentID !== agentid)
    this.studentSubject.next(this.students)
  }

  disconnect (): void {
    this.students = []
    this.studentSubject.next(this.students)
  }

  resetQuizData (): void {
    this.students.map(student => {
      ;(student.questionsAsked = new Map()), (student.score = 0)
    })
    this.currentPlayerIndex = 0
  }
}
