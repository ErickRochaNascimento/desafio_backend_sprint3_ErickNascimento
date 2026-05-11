using BancoDigital.Data;
using BancoDigital.DTOs;
using BancoDigital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BancoDigital.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroDTO dto)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { mensagem = "E-mail já cadastrado." });

        var usuario = new Usuario
        {
            Nome = dto.Nome.ToUpper(),
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return Ok(new { mensagem = "Usuário registrado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return Unauthorized(new { mensagem = "Credenciais inválidas." });

        var token = GerarToken(usuario);
        return Ok(new { token, nome = usuario.Nome, perfil = usuario.Perfil });
    }

    private string GerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpDelete("excluir-conta")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ExcluirConta([FromBody] LoginDTO dto)
    {
        var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var usuario = await _context.Usuarios
            .Include(u => u.Contas)
            .ThenInclude(c => c.Transacoes)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null || usuario.Email != dto.Email)
            return Unauthorized(new { mensagem = "E-mail incorreto ou Senha incorreta." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return Unauthorized(new { mensagem = "E-mail incorreto ou Senha incorreta." });

        // Verifica se alguma conta tem saldo
        var contasComSaldo = usuario.Contas.Where(c => c.Saldo > 0).ToList();
        if (contasComSaldo.Any())
        {
            var lista = string.Join(", ", contasComSaldo.Select(c => $"Conta ID {c.Id} (R$ {c.Saldo:F2})"));
            return BadRequest(new { mensagem = $"Você ainda possui saldo nas contas: {lista}. Saque ou transfira antes de sair." });
        }

        // Remove transações, contas e usuário
        foreach (var conta in usuario.Contas)
        {
            _context.Transacoes.RemoveRange(conta.Transacoes);
        }
        _context.Contas.RemoveRange(usuario.Contas);
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Conta excluída com sucesso." });
    }
}