import { TestBed } from '@angular/core/testing'

import { XapiService } from './xapi.service'

describe('XapiService', () => {
  let service: XapiService

  beforeEach(() => {
    TestBed.configureTestingModule({})
    service = TestBed.inject(XapiService)
  })

  it('should be created', () => {
    expect(service).toBeTruthy()
  })
})
