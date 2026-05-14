// ==========================================
// UTILS.JS - Funções Compartilhadas
// ==========================================

let BASE = 'https://localhost:7200/api';
let token = '';

// ── VALIDAÇÃO E MÁSCARA ──
function mascaraCPF(input) {
    let v = input.value.replace(/\D/g, "");
    if (v.length > 11) v = v.slice(0, 11);

    if (v.length > 9) {
        v = v.replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, "$1.$2.$3-$4");
    } else if (v.length > 6) {
        v = v.replace(/^(\d{3})(\d{3})(\d{0,3})$/, "$1.$2.$3");
    } else if (v.length > 3) {
        v = v.replace(/^(\d{3})(\d{0,3})$/, "$1.$2");
    }
    input.value = v;
}

function validarCPF(cpf) {
    cpf = cpf.replace(/\D/g, "");
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;

    let soma = 0;
    let resto;

    for (let i = 1; i <= 9; i++) soma += parseInt(cpf.substring(i - 1, i)) * (11 - i);
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(9, 10))) return false;

    soma = 0;
    for (let i = 1; i <= 10; i++) soma += parseInt(cpf.substring(i - 1, i)) * (12 - i);
    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(10, 11))) return false;

    return true;
}

// ── MENSAGENS ──
function mostrarMsg(id, texto, tipo) {
    var el = document.getElementById(id);
    if (!el) return;

    el.textContent = texto;
    el.style.display = 'block';
    el.style.padding = '10px 14px';
    el.style.borderRadius = '8px';
    el.style.fontSize = '13px';
    el.style.marginTop = '12px';
    el.style.border = '';
    el.className = '';

    if (tipo === 'success') {
        el.style.background = 'rgba(0,208,132,0.12)';
        el.style.color = '#00d084';
        el.style.border = '1px solid rgba(0,208,132,0.3)';
    } else {
        el.style.background = 'rgba(255,69,96,0.12)';
        el.style.color = '#ff4560';
        el.style.border = '1px solid rgba(255,69,96,0.3)';
    }
}

function formatarValorExtrato(valor, negativo) {
    // retorna string já com "R$" e com "-" se negativo (usa formatarMoeda existente)
    const absoluto = formatarMoeda(Math.abs(valor));
    return (negativo ? '- ' : '') + absoluto;
}

function limparMsg(id) {
    const el = document.getElementById(id);
    el.classList.add('oculto');
    el.textContent = '';
}

// ── REQUISIÇÕES HTTP ──
function carregarContas() {
    fetch(BASE + '/contas?_=' + Date.now(), {
        headers: { 'Authorization': 'Bearer ' + token, 'Cache-Control': 'no-cache' }
    }).then(res => res.json()).then(contas => {
        const grid = document.getElementById('listaContasGrid');
        grid.innerHTML = contas.map(c => `
            <div class="stat-card" style="cursor: pointer; transition: all 0.2s;" onmouseover="this.style.borderColor='var(--roxo)'" onmouseout="this.style.borderColor='var(--borda)'">
                <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 12px;">
                    <div>
                        <p class="stat-label">${c.tipo}</p>
                        <p class="stat-valor texto-verde" style="font-size: 24px;">R$ ${c.saldo.toFixed(2)}</p>
                    </div>
                </div>
                <div style="display: flex; gap: 8px;">
                    <button class="btn btn-sm btn-primario" style="flex: 1;" onclick="selecionarContaOperacao(${c.id})">Operar</button>
                    <button class="btn btn-sm btn-primario" onclick="abrirExtratoModal(${c.id}, '${c.tipo}')">Extrato</button>
                    <button class="btn btn-sm btn-perigo" onclick="encerrarConta(${c.id})">Encerrar</button>
                </div>
            </div>
        `).join('');
    });
}

function carregarSaldoConta() {
    const contaId = document.getElementById('contaOperacao').value;
    if (!contaId) {
        document.getElementById('saldoContaOperacao').classList.add('oculto');
        return;
    }

    fetch(BASE + '/contas/' + contaId + '?_=' + Date.now(), {
        headers: { 'Authorization': 'Bearer ' + token, 'Cache-Control': 'no-cache' }
    }).then(res => res.json()).then(conta => {
        document.getElementById('saldoValor').textContent = 'R$ ' + conta.saldo.toFixed(2);
        document.getElementById('saldoContaOperacao').classList.remove('oculto');
        contaAtualSaldo = conta.saldo;
    }).catch(() => { });
}

function carregarExtrato() {
    const contaId = document.getElementById('contaOperacao').value;
    if (!contaId) {
        document.getElementById('extratoTabela').innerHTML = '<tr><td colspan="6" class="texto-discreto" style="text-align: center; padding: 20px;">Selecione uma conta para ver o extrato.</td></tr>';
        return;
    }

    fetch(BASE + '/contas/' + contaId + '/transacoes?_=' + Date.now(), {
        headers: { 'Authorization': 'Bearer ' + token, 'Cache-Control': 'no-cache' }
    }).then(res => res.json()).then(transacoes => {
        const tbody = document.getElementById('extratoTabela');
        if (transacoes.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="texto-discreto" style="text-align: center; padding: 20px;">Nenhuma transação.</td></tr>';
            return;
        }
        tbody.innerHTML = transacoes.map(t => {
            const transferenciaEnviada = t.tipo === 'Transferencia' && !!t.nomeDestinatario;
            const negativo = t.tipo === 'Saque' || transferenciaEnviada;
            const corValor = negativo ? 'val-negativo' : 'val-positivo';
            const sinal = negativo ? '- ' : '+ ';
            const valorFormatado = sinal + formatarMoeda(Math.abs(t.valor));
            return `
                <tr>
                    <td>${t.tipo}</td>
                    <td class="${corValor}">${valorFormatado}</td>
                    <td>R$ ${t.taxa.toFixed(2)}</td>
                    <td>${t.descricao || '-'}</td>
                    <td>${t.nomeDestinatario || t.nomeRemetente || '-'}</td>
                    <td>${formatarData(t.realizadaEm)}</td>
                </tr>
            `;
        }).join('');
    });
}



// ── VALIDAÇÃO DE FORMULÁRIO ──
function validarCampos(campos) {
    for (let campo of campos) {
        if (!campo.value || campo.value.trim() === '') {
            return false;
        }
    }
    return true;
}

// ── MODAL ──
function abrirModal(idModal) {
    document.getElementById(idModal).classList.add('ativo');
}

function fecharModal(idModal) {
    document.getElementById(idModal).classList.remove('ativo');
}

function limparModal(idModal) {
    const modal = document.getElementById(idModal);
    const inputs = modal.querySelectorAll('input, textarea, select');
    inputs.forEach(input => input.value = '');
    const msgs = modal.querySelectorAll('[id*="msg"]');
    msgs.forEach(msg => msg.classList.add('oculto'));
}

// ── FORMATAÇÃO ──
function formatarData(dataString) {
    return new Date(dataString).toLocaleString('pt-BR');
}

function formatarMoeda(valor) {
    return 'R$ ' + parseFloat(valor).toFixed(2);
}

// ── ARMAZENAMENTO ──
function salvarToken(novoToken, nome = '') {
    token = novoToken;
    sessionStorage.setItem('adminToken', novoToken);
    if (nome) sessionStorage.setItem('adminNome', nome);
}

function recuperarToken() {
    token = sessionStorage.getItem('adminToken') || '';
    return token;
}

function limparSessao() {
    token = '';
    sessionStorage.clear();
}

function voltar() {
    window.location.href = 'index.html';
}