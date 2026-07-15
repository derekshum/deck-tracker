import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CardTypeService } from '../card-type.service';

@Component({
  selector: 'app-card-type-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './card-type-form.component.html',
  styleUrl: './card-type-form.component.scss'
})
export class CardTypeFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  protected readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly cardTypeService = inject(CardTypeService);

  isEditMode = false;
  gameId!: number;
  cardTypeId: number | null = null;
  isLoading = false;
  isSaving = false;
  error: string | null = null;

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [null, [Validators.maxLength(500)]]
  });

  ngOnInit(): void {
    this.gameId = Number(this.route.snapshot.paramMap.get('gameId'));
    this.cardTypeId = this.route.snapshot.paramMap.get('cardTypeId')
      ? Number(this.route.snapshot.paramMap.get('cardTypeId'))
      : null;
    this.isEditMode = this.cardTypeId !== null;

    if (this.isEditMode && this.cardTypeId) {
      this.loadCardType(this.cardTypeId);
    }
  }

  private loadCardType(id: number): void {
    this.isLoading = true;
    this.error = null;

    this.cardTypeService.getById(id).subscribe({
      next: (cardType) => {
        this.form.patchValue({
          name: cardType.name,
          description: cardType.description
        });
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load card type. Please try again.';
        this.isLoading = false;
      }
    });
  }

  private normalizeFormValue(value: typeof this.form.value) {
    return {
      ...value,
      description: value.description?.trim() || null
    };
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSaving) return;

    this.isSaving = true;
    this.error = null;

    const payload = this.normalizeFormValue(this.form.value);

    const request$ = this.isEditMode && this.cardTypeId
      ? this.cardTypeService.update(this.cardTypeId, payload)
      : this.cardTypeService.create(this.gameId, payload);

    request$.subscribe({
      next: () => {
        this.router.navigate(['/games', this.gameId, 'card-types']);
      },
      error: () => {
        this.error = 'Failed to save card type. Please try again.';
        this.isSaving = false;
      }
    });
  }
}
