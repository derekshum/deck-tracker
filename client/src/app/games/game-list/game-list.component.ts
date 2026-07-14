import { Component, inject, OnInit } from '@angular/core';
import { GameService, GameSummary } from '../game.service';
import { Router } from '@angular/router';
import { CommonModule, PercentPipe } from '@angular/common';

@Component({
  selector: 'app-game-list',
  standalone: true,
  imports: [CommonModule, PercentPipe],
  templateUrl: './game-list.component.html',
  styleUrl: './game-list.component.scss'
})
export class GameListComponent implements OnInit {
  protected readonly router = inject(Router);
  private readonly gameService = inject(GameService);

  games: GameSummary[] = [];
  isLoading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loadGames();
  }

  private loadGames(): void {
    this.isLoading = true;
    this.error = null;

    this.gameService.list().subscribe({ // using subscription rather than observable to allow for optimistic updates on delete
      next: games => {
        this.games = games;
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Failed to load games. Please try again.';
        this.isLoading = false;
      }
    });
  }

  onDelete(id: number): void {
    if (!confirm('Are you sure you want to delete this game? This will delete all associated data.')) {
      return;
    }

     this.gameService.delete(id).subscribe({
      next: () => {
        this.games = this.games.filter(g => g.id !== id);
      },
      error: () => {
        this.error = 'Failed to delete game. Please try again.';
      }
    });
  }
}
