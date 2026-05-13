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
    var mapa = { success: 'alerta-sucesso', danger: 'alerta-erro', warning: 'alerta-aviso' };
    var el = document.getElementById(id);
    if (!el) return;
    el.className = 'alerta ' + (mapa[tipo] || 'alerta-info');
    el.textContent = texto;
    el.classList.remove('oculto');

    // Forçar cores conforme solicitado
    if (tipo === 'success') {
        el.style.color = '#75d8a4'; // Verde
        el.style.borderColor = '#198754';
    } else {
        el.style.color = '#f5a0a7'; // Vermelho
        el.style.borderColor = '#dc3545';
    }

    // Garantir cores específicas conforme solicitado (ajustado para tema escuro)
    if (tipo === 'success') el.style.color = '#75d8a4'; // Verde claro para sucesso no tema escuro
    if (tipo === 'danger' || tipo === 'warning') el.style.color = '#f5a0a7'; // Vermelho claro para erro no tema escuro
}

function limparMsg(id) {
    const el = document.getElementById(id);
    el.classList.add('oculto');
    el.textContent = '';
}

// ── REQUISIÇÕES HTTP ──
function fazerRequisicao(endpoint, metodo = 'GET', dados = null, comToken = true) {
    const opcoes = {
        method: metodo,
        headers: { 'Content-Type': 'application/json' }
    };

    if (comToken && token) {
        opcoes.headers['Authorization'] = 'Bearer ' + token;
    }

    if (dados) {
        opcoes.body = JSON.stringify(dados);
    }

    return fetch(BASE + endpoint, opcoes)
        .then(res => res.json().then(data => ({ ok: res.ok, status: res.status, data })))
        .catch(err => ({ ok: false, status: 0, data: { mensagem: 'Erro ao conectar.' } }));
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