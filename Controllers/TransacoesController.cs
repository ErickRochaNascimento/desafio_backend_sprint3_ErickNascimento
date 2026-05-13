using BancoDigital.Data;
using BancoDigital.DTOs;
using BancoDigital.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BancoDigital.Controllers;

[ApiController]
[Route("api/contas/{contaId}/transacoes")]
[Authorize]
public class TransacoesController : ControllerBase
{
    private readonly TransacaoService _service;
    private readonly AppDbContext _context;

    public TransacoesController(TransacaoService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }

    private int GetUsuarioId() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Realizar(int contaId, [FromBody] TransacaoDTO dto)
    {
        var (sucesso, mensagem, transacao) =
            await _service.RealizarOperacao(contaId, dto, GetUsuarioId());

        if (!sucesso) return BadRequest(mensagem });
        return Ok(new {sucesso, mensagem, transacao });
    }

    [HttpGet]
    public async Task<IActionResult> Historico(int contaId)
    {
        var conta = await _context.Contas.FindAsync(contaId);
        if (conta == null || conta.UsuarioId != GetUsuarioId())
            return NotFound();

        var historico = await _context.Transacoes
            .Where(t => t.ContaId == contaId)
            .OrderByDescending(t => t.RealizadaEm)
            .ToListAsync();

        return Ok(historico);
    }
}