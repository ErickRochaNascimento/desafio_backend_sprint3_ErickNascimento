using BancoDigital.Data;
using BancoDigital.Models;
using BancoDigital.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoDigital.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Listar todos os usuários (comuns e admins)
        [HttpGet("usuarios")]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new {
                    u.Id,
                    u.Nome,
                    u.Email,
                    u.Cpf,
                    u.Perfil,
                    u.DataNascimento,
                    QtdContas = u.Contas.Count
                })
                .ToListAsync();
            return Ok(usuarios);
        }

        // 2. Ver contas de um usuário específico e seus extratos
        [HttpGet("usuario/{usuarioId}/detalhes")]
        public async Task<IActionResult> VerDetalhesUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => new
                {
                    u.Nome,
                    u.Email,
                    Contas = u.Contas.Select(c => new
                    {
                        c.Id,
                        c.Tipo,
                        c.Saldo,
                        c.NumeroConta,
                        Extrato = c.Transacoes.OrderByDescending(t => t.RealizadaEm).Take(10).Select(t => new
                        {
                            t.Id,
                            t.Tipo,
                            t.Valor,
                            Data = t.RealizadaEm,
                            t.Descricao,
                            t.NomeDestinatario,
                            t.NomeRemetente
                        })
                    })
                })
                .FirstOrDefaultAsync();

            if (usuario == null) return NotFound(new { mensagem = "Usuário não encontrado." });

            return Ok(usuario);
        }

        // 3. Criar um novo usuário Administrador
        [HttpPost("criar-admin")]
        public async Task<IActionResult> CriarAdmin([FromBody] RegistroDTO dto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { mensagem = "E-mail já cadastrado." });

            if (await _context.Usuarios.AnyAsync(u => u.Cpf == dto.Cpf))
                return BadRequest(new { mensagem = "CPF já cadastrado." });

            var admin = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Cpf = dto.Cpf,
                DataNascimento = dto.DataNascimento,
                Perfil = "admin",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            };

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Novo administrador criado com sucesso!" });
        }

        // 4. Ver extrato total do banco
        [HttpGet("extrato-geral")]
        public async Task<IActionResult> VerExtratoGeral()
        {
            var transacoes = await _context.Transacoes
                .OrderByDescending(t => t.RealizadaEm)
                .Take(50)
                .Select(t => new {
                    t.Id,
                    t.Tipo,
                    t.Valor,
                    Data = t.RealizadaEm,
                    t.Descricao,
                    t.NomeDestinatario,
                    t.NomeRemetente,
                    Usuario = t.Conta.Usuario.Nome
                })
                .ToListAsync();
            return Ok(transacoes);
        }
    }
}