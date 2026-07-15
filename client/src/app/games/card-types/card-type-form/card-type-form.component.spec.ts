import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CardTypeFormComponent } from './card-type-form.component';

describe('CardTypeFormComponent', () => {
  let component: CardTypeFormComponent;
  let fixture: ComponentFixture<CardTypeFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardTypeFormComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CardTypeFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
