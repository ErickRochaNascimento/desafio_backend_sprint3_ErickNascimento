using BancoDigital.Data;
using BancoDigital.DTOs;
using BancoDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace BancoDigital.Services
{
    public class TransacaoService
    {
        private readonly AppDbContext _context;

        public TransacaoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool sucesso, string mensagem, Transacao? transacao)>
            RealizarOperacao(int contaId, TransacaoDTO dto, int usuarioLogadoId)
        {
            var conta = await _context.Contas.FirstOrDefaultAsync(c => c.Id == contaId);

            if (conta == null) return (false, "Conta não encontrada.", null);
            if (conta.UsuarioId != usuarioLogadoId) return (false, "Acesso negado.", null);

            var usuarioLogado = await _context.Usuarios.FindAsync(usuarioLogadoId);
            string nomeUsuario = usuarioLogado?.Nome ?? "Desconhecido";

            decimal taxa = CalcularTaxa(conta.Tipo, dto.Tipo, dto.Valor);

            switch (dto.Tipo)
            {
                case "Deposito":
                    conta.Saldo += dto.Valor;
                    break;

                case "Saque":
                    if (conta.Saldo < dto.Valor + taxa)
                        return (false, $"Saldo insuficiente. Taxa: R$ {taxa:F2}. Saque maximo: R$ {conta.Saldo - taxa:F2}", null);
                    conta.Saldo -= dto.Valor + taxa;
                    break;

                case "Transferencia":
                    if (dto.ContaDestinoId == null)
                        return (false, "Informe a conta destino.", null);
                    if (dto.ContaDestinoId == contaId)
                        return (false, "Não é possível transferir para a própria conta.", null);

                    var destino = await _context.Contas
                        .Include(c => c.Usuario)
                        .FirstOrDefaultAsync(c => c.Id == dto.ContaDestinoId);

                    if (destino == null)
                        return (false, "Conta destino não encontrada.", null);
                    if (conta.Saldo < dto.Valor + taxa)
                        return (false, "Saldo insuficiente.", null);

                    string nomeDestino = destino.Usuario?.Nome ?? "Desconhecido";

                    conta.Saldo -= dto.Valor + taxa;
                    destino.Saldo += dto.Valor;

                    // Extrato do REMETENTE — mostra destinatário
                    var transacaoRemetente = new Transacao
                    {
                        ContaId = contaId,
                        Tipo = "Transferencia",
                        Valor = dto.Valor,
                        Taxa = taxa,
                        Descricao = dto.Descricao,
                        NomeRemetente = string.Empty,
                        NomeDestinatario = nomeDestino + " (ID " + destino.Id + ")"
                    };
                    _context.Transacoes.Add(transacaoRemetente);

                    // Extrato do DESTINATÁRIO — mostra remetente
                    var transacaoDestino = new Transacao
                    {
                        ContaId = destino.Id,
                        Tipo = "Transferencia",
                        Valor = dto.Valor,
                        Taxa = 0,
                        Descricao = dto.Descricao,
                        NomeRemetente = nomeUsuario + " (ID " + contaId + ")",
                        NomeDestinatario = string.Empty
                    };
                    _context.Transacoes.Add(transacaoDestino);

                    await _context.SaveChangesAsync();
                    return (true, "Transferência realizada!", transacaoRemetente);

                default:
                    return (false, "Operação inválida.", null);
                              }

            // Deposito e Saque — sem remetente/destinatário
            var transacao = new Transacao
            {
                ContaId = contaId,
                Tipo = dto.Tipo,
                Valor = dto.Valor,
                Taxa = taxa,
                Descricao = dto.Descricao,
                NomeRemetente = string.Empty,
                NomeDestinatario = string.Empty
            };

            _context.Transacoes.Add(transacao);
            await _context.SaveChangesAsync();
            return (true, "Operação realizada!", transacao);
        }

        private decimal CalcularTaxa(string tipoConta, string tipoOp, decimal valor)
        {
            if (tipoOp == "Deposito") return 0;
            return tipoConta switch
            {
                "Corrente" => 5.00m,
                "Poupanca" => 0,
                "Empresarial" => valor * 0.01m,
                _ => 0
            };
        }
    }
}