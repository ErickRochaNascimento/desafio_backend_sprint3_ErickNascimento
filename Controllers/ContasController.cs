using BancoDigital.Data;
using BancoDigital.DTOs;
using BancoDigital.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BancoDigital.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContasController(AppDbContext context) => _context = context;

    private int GetUsuarioId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMinhasContas()
    {
        var contas = await _context.Contas
            .Where(c => c.UsuarioId == GetUsuarioId())
            .ToListAsync();
        return Ok(contas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var conta = await _context.Contas
            .Include(c => c.Transacoes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conta == null || conta.UsuarioId != GetUsuarioId())
            return NotFound(new { mensagem = "Conta não encontrada." });

        return Ok(conta);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ContaDTO dto)
    {
        var conta = new Conta
        {
            Tipo = dto.Tipo,
            UsuarioId = GetUsuarioId()
        };
        _context.Contas.Add(conta);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = conta.Id }, conta);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ContaDTO dto)
    {
        var conta = await _context.Contas.FindAsync(id);
        if (conta == null || conta.UsuarioId != GetUsuarioId())
            return NotFound(new { mensagem = "Conta não encontrada." });

        conta.Tipo = dto.Tipo;
        await _context.SaveChangesAsync();
        return Ok(conta);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Encerrar(int id)
    {
        var conta = await _context.Contas.FindAsync(id);
        if (conta == null || conta.UsuarioId != GetUsuarioId())
            return NotFound();

        // Saque automático do saldo restante
        if (conta.Saldo > 0)
        {
            decimal taxa = conta.Tipo switch
            {
                "Corrente" => 5.00m,
                "Empresarial" => conta.Saldo * 0.01m,
                _ => 0
            };

            decimal valorSacado = conta.Saldo - taxa;
            if (valorSacado < 0) valorSacado = 0;

            var transacao = new BancoDigital.Models.Transacao
            {
                ContaId = conta.Id,
                Tipo = "Saque",
                Valor = valorSacado,
                Taxa = taxa,
                Descricao = "Saque automático ao encerrar conta",
                NomeRemetente = string.Empty,
                NomeDestinatario = string.Empty
            };
            _context.Transacoes.Add(transacao);
            conta.Saldo = 0;
            await _context.SaveChangesAsync();
        }

        _context.Contas.Remove(conta);
        await _context.SaveChangesAsync();
        return Ok(new { mensagem = "Conta encerrada com saque automático do saldo." });
    }
}