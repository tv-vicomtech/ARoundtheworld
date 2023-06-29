import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BarcharComponent } from './barchar.component';

describe('BarcharComponent', () => {
  let component: BarcharComponent;
  let fixture: ComponentFixture<BarcharComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BarcharComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BarcharComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
