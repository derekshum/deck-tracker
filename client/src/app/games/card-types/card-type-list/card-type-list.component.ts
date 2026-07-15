import { Component, inject, OnInit, input } from '@angular/core';
import { CardType, CardTypeService } from '../card-type.service';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { toObservable } from '@angular/core/rxjs-interop';
import { Observable, switchMap } from 'rxjs';
import { GameService, GameSummary } from '../../game.service';

@Component({
  selector: 'app-card-type-list',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './card-type-list.component.html',
  styleUrl: './card-type-list.component.scss'
})
export class CardTypeListComponent implements OnInit {
  protected readonly router = inject(Router);
  private readonly gameService = inject(GameService);
  private readonly cardTypeService = inject(CardTypeService);
  protected readonly gameId = input.required<number>();

  game$: Observable<GameSummary> = toObservable(this.gameId).pipe(
    switchMap(id => this.gameService.getById(id))
  );  // async pipe — read only, never mutated locally
  cardTypes: CardType[] = [];
  isLoading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loadCardTypes();
  }

  private loadCardTypes(): void {
    this.isLoading = true;
    this.error = null;

    // using subscription rather than observable to allow for optimistic updates on delete
    this.cardTypeService.list(this.gameId()).subscribe({ 
      next: cardTypes => {
        this.cardTypes = cardTypes;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load card types for this game. Please try again.'
        this.isLoading = false;
      }
    })
  }

  onDelete(id: number): void {
    if (!confirm('Are you sure you want to delete this card type? This will delete all associated data.')) {
      return;
    }

     this.cardTypeService.delete(id).subscribe({
      next: () => {
        this.cardTypes = this.cardTypes.filter(g => g.id !== id);
      },
      error: () => {
        this.error = 'Failed to delete card type. Please try again.';
      }
    });
  }
}
