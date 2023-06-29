import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginOrkestraComponent } from './login-orkestra.component';

describe('LoginOrkestraComponent', () => {
  let component: LoginOrkestraComponent;
  let fixture: ComponentFixture<LoginOrkestraComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LoginOrkestraComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginOrkestraComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
