namespace BancoDigital.Models
{
    public class Conta
    {
        public int Id { get; set; }
        public string NumeroConta { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        public string Tipo { get; set; } = string.Empty;
        public decimal Saldo { get; set; } = 0;
        public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}