using System.ComponentModel.DataAnnotations;

namespace BancoDigital.DTOs;

public class ContaDTO
{
    [Required]
    public string Tipo { get; set; } = string.Empty;
}

public class LoginDTO
{
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
}

public class RegistroDTO
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required][MinLength(6)] public string Senha { get; set; } = string.Empty;
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos")]
    public string Cpf { get; set; } = string.Empty;
    [Required] public DateTime DataNascimento { get; set; }
}

public class AlterarSenhaDTO
{
    [Required] public string Cpf { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required][MinLength(6)] public string NovaSenha { get; set; } = string.Empty;
}

public class TransacaoDTO
{
    [Required] public string Tipo { get; set; } = string.Empty;
    [Required][Range(0.01, double.MaxValue)] public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int? ContaDestinoId { get; set; }
}