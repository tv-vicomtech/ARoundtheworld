import { ComponentFixture, TestBed } from '@angular/core/testing'

import { HelpControlsComponent } from './help-controls.component'

describe('HelpControlsComponent', () => {
  let component: HelpControlsComponent,
   fixture: ComponentFixture<HelpControlsComponent>

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HelpControlsComponent ]
    }).compileComponents()
  })

  beforeEach(() => {
    fixture = TestBed.createComponent(HelpControlsComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
