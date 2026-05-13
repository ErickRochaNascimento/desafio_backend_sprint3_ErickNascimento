using BancoDigital.Data; // ADICIONE ESTA LINHA
using BancoDigital.Models; // ADICIONE ESTA LINHA
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

        [HttpGet("usuarios")]
        public async Task<IActionResult> ListarUsuarios() => Ok(await _context.Usuarios.ToListAsync());

        [HttpGet("contas-usuario/{usuarioId}")]
        public async Task<IActionResult> VerContasUsuario(int usuarioId)
        {
            var contas = await _context.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .Include(c => c.Transacoes)
                .ToListAsync();
            return Ok(contas);
        }
    }
}
