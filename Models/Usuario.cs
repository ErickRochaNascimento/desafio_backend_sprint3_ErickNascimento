using static System.Net.Mime.MediaTypeNames;

namespace BancoDigital.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Perfil { get; set; } = "cliente";
    public ICollection<Conta> Contas { get; set; } = new List<Conta>();
}