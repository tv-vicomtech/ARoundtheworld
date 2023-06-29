import { TestBed } from '@angular/core/testing';

import { OrkestraService } from './orkestra.service';

describe('OrkestraService', () => {
  let service: OrkestraService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OrkestraService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
