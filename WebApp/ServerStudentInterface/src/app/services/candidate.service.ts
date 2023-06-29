import { Injectable } from '@angular/core'
import { Observable, Subject } from 'rxjs'
import { receiver } from 'src/assets/client/public/receiver/js/main'
@Injectable({
  providedIn: 'root'
})
export class CandidateService {
  candidates = new Subject<[]>()

  getCandidates (): Observable<[]> {
    return this.candidates.asObservable()
  }

  updateCandidates () {
    if (receiver) {this.candidates.next(receiver.candidates)}
  }
}
