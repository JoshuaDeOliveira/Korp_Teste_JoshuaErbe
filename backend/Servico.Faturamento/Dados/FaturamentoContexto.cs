using Microsoft.EntityFrameworkCore;
using Servico.Faturamento.Modelos;

namespace Servico.Faturamento.Dados;

public class FaturamentoContexto : DbContext
{
    public FaturamentoContexto(DbContextOptions<FaturamentoContexto> opcoes) : base(opcoes)
    {
    }

    public DbSet<NotaFiscalCabecalho> Notas => Set<NotaFiscalCabecalho>();
    public DbSet<ItemDaNotaFiscal> Itens => Set<ItemDaNotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscalCabecalho>()
            .HasIndex(n => n.NumeroSequencial)
            .IsUnique();

        modelBuilder.Entity<NotaFiscalCabecalho>()
            .HasMany(n => n.Itens)
            .WithOne(i => i.NotaPai)
            .HasForeignKey(i => i.NotaFiscalCabecalhoId);
    }
}
