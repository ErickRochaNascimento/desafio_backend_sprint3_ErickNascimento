using BancoDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace BancoDigital.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Conta> Contas { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
}