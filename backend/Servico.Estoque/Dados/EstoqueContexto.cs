using Microsoft.EntityFrameworkCore;
using Servico.Estoque.Modelos;

namespace Servico.Estoque.Dados;

// contexto do EF, nada demais aqui
public class EstoqueContexto : DbContext
{
    public EstoqueContexto(DbContextOptions<EstoqueContexto> opcoes) : base(opcoes)
    {
    }

    public DbSet<ProdutoEstoque> Produtos => Set<ProdutoEstoque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProdutoEstoque>()
            .HasIndex(p => p.Codigo)
            .IsUnique();
    }
}
