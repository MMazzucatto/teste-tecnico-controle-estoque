using ControleEstoque.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ControleEstoque.Api.Infrastructure;

public class EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Produto>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasMaxLength(50);
            e.Property(p => p.Versao).IsConcurrencyToken();
            e.HasMany(p => p.Reservas).WithOne().HasForeignKey(r => r.ProdutoId);
            e.HasMany(p => p.Bloqueios).WithOne().HasForeignKey(b => b.ProdutoId);
        });

        // Importante: o Id é gerado no código, não pelo banco
        mb.Entity<Reserva>().HasKey(r => r.Id);
        mb.Entity<Reserva>().Property(r => r.Id).ValueGeneratedNever();
        mb.Entity<Bloqueio>().HasKey(b => b.Id);
        mb.Entity<Bloqueio>().Property(b => b.Id).ValueGeneratedNever();
    }
}