// ==========================================
// UTILS.JS - Funções Compartilhadas
// ==========================================

let BASE = window.location.origin + '/api';
let token = '';

// ── VALIDAÇÃO E MÁSCARA ──
function mascaraCPF(input) {
    let v = input.value.replace(/\D/g, "");
    if (v.length > 11) v = v.slice(0, 11);
    if (v.length > 9) v = v.replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, "$1.$2.$3-$4");
    else if (v.length > 6) v = v.replace(/^(\d{3})(\d{3})(\d{0,3})$/, "$1.$2.$3");
    else if (v.length > 3) v = v.replace(/^(\d{3})(\d{0,3})$/, "$1.$2");
    input.value = v;
}

function validarCPF(cpf) {
    cpf = cpf.replace(/\D/g, "");
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;
    let soma = 0, resto;
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
// Usa setAttribute para sobrescrever qualquer style inline existente no elemento
function mostrarMsg(id, texto, tipo) {
    var el = document.getElementById(id);
    if (!el) return;
    el.textContent = texto;
    var sucesso = tipo === 'success';
    el.setAttribute('style',
        'display:block;' +
        'padding:10px 14px;' +
        'border-radius:8px;' +
        'font-size:13px;' +
        'margin-top:12px;' +
        'margin-bottom:8px;' +
        (sucesso
            ? 'background:rgba(0,208,132,0.15);color:#00d084;border:1px solid rgba(0,208,132,0.4);'
            : 'background:rgba(255,69,96,0.15);color:#ff4560;border:1px solid rgba(255,69,96,0.4);')
    );
}

function limparMsg(id) {
    const el = document.getElementById(id);
    if (!el) return;
    el.setAttribute('style', 'display:none;');
    el.textContent = '';
}

// ── REQUISIÇÕES HTTP ──
function fazerRequisicao(endpoint, metodo = 'GET', dados = null, comToken = true) {
    const opcoes = {
        method: metodo,
        headers: {
            'Content-Type': 'application/json',
            'Cache-Control': 'no-cache',
            'Pragma': 'no-cache'
        }
    };
    if (comToken && token) opcoes.headers['Authorization'] = 'Bearer ' + token;
    if (dados) opcoes.body = JSON.stringify(dados);

    const url = metodo === 'GET'
        ? BASE + endpoint + (endpoint.includes('?') ? '&' : '?') + '_=' + Date.now()
        : BASE + endpoint;

    return fetch(url, opcoes)
        .then(res => res.json().then(data => ({ ok: res.ok, status: res.status, data })))
        .catch(() => ({ ok: false, status: 0, data: { mensagem: 'Erro ao conectar.' } }));
}

// ── VALIDAÇÃO DE FORMULÁRIO ──
function validarCampos(campos) {
    for (let campo of campos) {
        if (!campo.value || campo.value.trim() === '') return false;
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
    modal.querySelectorAll('input, textarea, select').forEach(input => input.value = '');
    modal.querySelectorAll('[id*="msg"]').forEach(msg => msg.setAttribute('style', 'display:none;'));
}

// ── FORMATAÇÃO ──
function formatarData(dataString) {
    return new Date(dataString).toLocaleString('pt-BR');
}

function formatarMoeda(valor) {
    return 'R$ ' + parseFloat(valor).toFixed(2);
}

function formatarValorExtrato(valor, negativo) {
    return (negativo ? '- ' : '') + formatarMoeda(Math.abs(valor));
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
