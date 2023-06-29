import { Answer_Coords } from './answer'

export interface Quiz {
  quizName: string
  questions: Quiz_Question[]
}

export interface Quiz_Question {
  location: string
  continent: string
  difficulty: string
  latitude: string
  longitude: string
}

export interface Question {
  index: string
  title: string
  correctAnswer: Answer_Coords
  validated: boolean
  difficulty: string
}
