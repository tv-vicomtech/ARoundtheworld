import { Answer_Coords } from './answer'
import { Question } from './question'

export interface Student {
  agentID: string
  score: number
  questionsAsked: Map<Question, Answer_Coords>
  mobileDevice: boolean
  color?: string
}
