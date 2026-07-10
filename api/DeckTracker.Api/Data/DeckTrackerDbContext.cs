using DeckTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DeckTracker.Api.Data;

public class DeckTrackerDbContext(DbContextOptions<DeckTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<CardType> CardTypes => Set<CardType>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<DeckCard> DeckCards => Set<DeckCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>()
            .HasMany(g => g.CardTypes)
            .WithOne(ct => ct.Game)
            .HasForeignKey(ct => ct.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Cards)
            .WithOne(c => c.Game)
            .HasForeignKey(c => c.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict, not cascade: a game with logged decks can't be deleted (checked explicitly),
        // so this should never fire in practice.
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Decks)
            .WithOne(d => d.Game)
            .HasForeignKey(d => d.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CardType>()
            .HasMany(ct => ct.Cards)
            .WithOne(c => c.Type)
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeckCard>(entity =>
        {
            entity.HasKey(dc => new { dc.DeckId, dc.CardId });
            entity.HasOne(dc => dc.Deck)
                .WithMany(d => d.Cards)
                .HasForeignKey(dc => dc.DeckId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(dc => dc.Card)
                .WithMany(c => c.DeckCards)
                .HasForeignKey(dc => dc.CardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Deck>()
            .Property(d => d.Result)
            .HasConversion<string>();
    }
}
