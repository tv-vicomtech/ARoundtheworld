import { TestBed } from '@angular/core/testing';

import { LoginOrkestraService } from './login-orkestra.service';

describe('LoginOrkestraService', () => {
  let service: LoginOrkestraService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoginOrkestraService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
