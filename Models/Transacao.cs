namespace BancoDigital.Models
{
    public class Transacao
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal Taxa { get; set; } = 0;
        public string Descricao { get; set; } = string.Empty;
        public string NomeRemetente { get; set; } = string.Empty;
        public string NomeDestinatario { get; set; } = string.Empty;
        public DateTime RealizadaEm { get; set; } = DateTime.UtcNow;
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
    }
}