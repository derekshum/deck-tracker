import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GameService } from '../game.service';

@Component({
  selector: 'app-game-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './game-form.component.html',
  styleUrl: './game-form.component.scss'
})
export class GameFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  protected readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly gameService = inject(GameService);

  isEditMode = false;
  gameId: number | null = null;
  isLoading = false;
  isSaving = false;
  error: string | null = null;

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: [null, [Validators.maxLength(500)]]
  });

  ngOnInit(): void {
    this.gameId = this.route.snapshot.paramMap.get('id')
      ? Number(this.route.snapshot.paramMap.get('id'))
      : null;
    this.isEditMode = this.gameId !== null;

    if (this.isEditMode && this.gameId) {
      this.loadGame(this.gameId);
    }
  }

  private loadGame(id: number): void {
    this.isLoading = true;
    this.error = null;

    this.gameService.getById(id).subscribe({
      next: (game) => {
        this.form.patchValue({
          name: game.name,
          description: game.description
        });
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load game. Please try again.';
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

    const request$ = this.isEditMode && this.gameId
      ? this.gameService.update(this.gameId, payload)
      : this.gameService.create(payload);

    request$.subscribe({
      next: () => {
        this.router.navigate(['/games']); // TODO: create this page
      },
      error: () => {
        this.error = 'Failed to save game. Please try again.';
        this.isSaving = false;
      }
    });
  }
}