import { Injectable } from '@angular/core'
import { Subject } from 'rxjs'
import { Question, Quiz } from '../interfaces/question'
import { HttpClient } from '@angular/common/http'
import data1 from '../../assets/client/public/quizzes/small-countries'
import data2 from '../../assets/client/public/quizzes/landmarks'
import data3 from '../../assets/client/public/quizzes/capitals'
import data4 from '../../assets/client/public/quizzes/cities'
import data5 from '../../assets/client/public/quizzes/mountains'
import data6 from '../../assets/client/public/quizzes/test'
import { Answer_Coords } from '../interfaces/answer'

@Injectable({
  providedIn: 'root'
})
export class QuestionService {
  constructor (private http: HttpClient) {}
  quizList: Quiz[] = []
  selectedQuizIndex: number
  questions: Question[] = []
  questionSubject = new Subject<Question[]>()
  currentQuestionIndex: number = 0

  loadQuizList (): Quiz[] {
    const quizList = [ data1, data2, data3, data4, data5, data6 ]
    this.quizList = quizList
    return this.quizList
  }

  getActiveQuiz (): Quiz {
    return this.quizList[this.selectedQuizIndex]
  }

  loadActiveQuizQuestions (): Question[] {
    const quiz = this.getActiveQuiz()
    this.questions = [] // Reset the question list before populating it
    quiz.questions.map((question, index) => {
      this.questions.push({
        index: index.toString(),
        title: `Locate ${question.location}`,
        correctAnswer: {
          lat: question.latitude,
          lon: question.longitude,
          continent: question.continent
        } as Answer_Coords,
        validated: false,
        difficulty: question.difficulty.toString()
      } as Question)
    })
    return this.questions
  }

  getQuizName (quizIndex: number): string {
    return this.quizList[quizIndex] ? this.quizList[quizIndex].quizName : 'Unnamed quiz'
  }

  advanceQuestionIndex (): number {
    if (this.currentQuestionIndex < this.questions.length) {
      this.currentQuestionIndex += 1
    }
    return this.currentQuestionIndex
  }

  setCurrentQuestion (index: number): void {
    this.currentQuestionIndex = index
  }

  getCurrentQuestion (): Question {
    return this.questions[this.currentQuestionIndex]
  }

  getIsLastQuestion (): boolean {
    return this.currentQuestionIndex === this.questions.length - 1
  }

  resetQuizData (): void {
    this.selectedQuizIndex = 0
    this.currentQuestionIndex = 0
  }
}
