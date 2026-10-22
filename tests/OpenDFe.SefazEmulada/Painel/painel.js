// O painel da SEFAZ emulada (E.6): fala só com as rotas /_controle da própria porta, sem biblioteca nem build. Todo
// texto que vem do emulador entra pelo textContent, nunca como HTML.
"use strict";

const estado = {
  emulador: null,
  autorizadores: [],
  falhas: [],
  notas: [],
  eventos: [],
  inutilizacoes: [],
  distribuicao: [],
  contribuintes: [],
  irregulares: [],
  csc: [],
  pedidos: [],
  pedidoEscolhido: null,
  notaEscolhida: null,
};

const TIPOS_EVENTO = {
  "110110": "Carta de correção",
  "110111": "Cancelamento",
  "110112": "Cancelamento por substituição",
  "210200": "Confirmação da operação",
  "210210": "Ciência da emissão",
  "210220": "Desconhecimento da operação",
  "210240": "Operação não realizada",
};

// O nome curto de cada serviço, nos cartões dos autorizadores.
const SERVICOS = {
  NFeStatusServico4: "Status",
  NFeAutorizacao4: "Autorização",
  NFeRetAutorizacao4: "Recibo",
  NFeConsultaProtocolo4: "Consulta",
  NFeRecepcaoEvento4: "Eventos",
  NFeInutilizacao4: "Inutilização",
  CadConsultaCadastro4: "Cadastro",
  NFeDistribuicaoDFe: "Distribuição",
};

const FALHAS = [
  { tipo: "paralisacao", nome: "Paralisação", texto: "O cStat 108 ou 109, depois das rejeições do próprio pedido.", campos: ["codigo", "observacao"] },
  { tipo: "atraso", nome: "Atraso", texto: "Espera e depois responde normalmente.", campos: ["milissegundos"] },
  { tipo: "tempoEsgotado", nome: "Tempo esgotado", texto: "Nunca responde: segura o pedido até o cliente desistir.", campos: [] },
  { tipo: "conexaoDerrubada", nome: "Conexão derrubada", texto: "Derruba sem resposta. Depois de processar: a nota fica autorizada e a resposta não chega.", campos: ["depois"] },
  { tipo: "respostaIlegivel", nome: "Resposta ilegível", texto: "HTTP 200 com o envelope cortado ao meio.", campos: [] },
  { tipo: "falhaSoap", nome: "Falha SOAP", texto: "HTTP 500 com a falha soap:Receiver.", campos: [] },
  { tipo: "loteProcessando", nome: "Lote em processamento", texto: "A consulta do recibo responde 105. Só no NFeRetAutorizacao4.", campos: [], servico: "NFeRetAutorizacao4" },
];

// Os elementos

function el(tag, atributos, ...filhos) {
  const elemento = document.createElement(tag);
  for (const [nome, valor] of Object.entries(atributos ?? {})) {
    if (valor === null || valor === undefined || valor === false) continue;
    if (nome === "class") elemento.className = valor;
    else if (nome.startsWith("on")) elemento.addEventListener(nome.slice(2), valor);
    else if (valor === true) elemento.setAttribute(nome, "");
    else elemento.setAttribute(nome, String(valor));
  }
  for (const filho of filhos.flat()) {
    if (filho === null || filho === undefined || filho === false) continue;
    elemento.append(filho instanceof Node ? filho : document.createTextNode(String(filho)));
  }
  return elemento;
}

const $ = (id) => document.getElementById(id);

function substituir(alvo, ...filhos) {
  alvo.replaceChildren(...filhos.flat().filter((f) => f !== null && f !== undefined));
}

function hora(instante) {
  if (!instante) return "";
  const data = new Date(instante);
  return Number.isNaN(data.getTime()) ? String(instante) : data.toLocaleTimeString("pt-BR", { hour12: false });
}

function dataHora(instante) {
  if (!instante) return "";
  const data = new Date(instante);
  return Number.isNaN(data.getTime()) ? String(instante) : data.toLocaleString("pt-BR", { hour12: false });
}

function selo(texto, tipo) {
  return el("span", { class: "selo" + (tipo ? " " + tipo : "") }, texto);
}

// O cStat da situação ("cStat 100", "cStat 100, com a resposta cortada"): o selo pelo resultado.
function seloSituacao(pedido) {
  const codigo = /cStat (\d{3})/.exec(pedido.situacao)?.[1];
  if (pedido.status === null || pedido.status === undefined) return selo(pedido.situacao, "erro");
  if (pedido.status >= 400) return selo(pedido.situacao, "erro");
  if (!codigo) return selo(pedido.situacao, "alerta");
  const sucesso = ["100", "101", "102", "103", "104", "107", "111", "112", "128", "135", "137", "138", "150"];
  const espera = ["105", "108", "109"];
  return selo(pedido.situacao, sucesso.includes(codigo) ? "ok" : espera.includes(codigo) ? "alerta" : "erro");
}

// Os avisos

let temporizadorAviso = 0;
function avisar(texto, sucesso = false) {
  const aviso = $("aviso");
  aviso.textContent = texto;
  aviso.classList.toggle("sucesso", sucesso);
  aviso.hidden = false;
  clearTimeout(temporizadorAviso);
  temporizadorAviso = setTimeout(() => { aviso.hidden = true; }, sucesso ? 2500 : 8000);
}

// A API do controle

async function api(metodo, caminho, corpo) {
  const opcoes = { method: metodo, headers: {} };
  if (corpo !== undefined) {
    opcoes.headers["Content-Type"] = "application/json";
    opcoes.body = JSON.stringify(corpo);
  }
  const resposta = await fetch("/_controle" + caminho, opcoes);
  const texto = await resposta.text();
  const dados = texto ? JSON.parse(texto) : null;
  if (!resposta.ok) throw new Error(dados?.erro ?? `HTTP ${resposta.status} em ${metodo} /_controle${caminho}`);
  return dados;
}

// A ação do usuário, com o aviso de sucesso ou do erro; verdadeiro se deu certo.
async function executar(acao, sucesso) {
  try {
    await acao();
    if (sucesso) avisar(sucesso, true);
    await atualizar();
    return true;
  } catch (erro) {
    avisar(erro.message);
    return false;
  }
}

// O que o painel mostra, lido de novo a cada aviso de estado

async function atualizar() {
  const [falhas, notas, eventos, inutilizacoes, distribuicao, contribuintes, irregulares, csc] = await Promise.all([
    api("GET", "/falhas"), api("GET", "/notas"), api("GET", "/eventos"), api("GET", "/inutilizacoes"),
    api("GET", "/distribuicao"), api("GET", "/cadastros"), api("GET", "/irregulares"), api("GET", "/csc"),
  ]);
  Object.assign(estado, { falhas, notas, eventos, inutilizacoes, distribuicao, contribuintes, irregulares, csc });
  desenharAutorizadores();
  desenharFalhas();
  desenharNotas();
  desenharDistribuicao();
  desenharCadastros();
  if (estado.notaEscolhida && !estado.notas.some((n) => n.chave === estado.notaEscolhida)) fecharNota();
  else if (estado.notaEscolhida) abrirNota(estado.notaEscolhida, false);
}

let temporizadorAtualizacao = 0;
function atualizarDepois() {
  clearTimeout(temporizadorAtualizacao);
  temporizadorAtualizacao = setTimeout(() => atualizar().catch((erro) => avisar(erro.message)), 250);
}

// As abas

function abrirAba(nome) {
  for (const botao of document.querySelectorAll(".abas button")) {
    if (botao.dataset.aba === nome) botao.setAttribute("aria-current", "page");
    else botao.removeAttribute("aria-current");
  }
  for (const secao of document.querySelectorAll("main > .aba")) secao.hidden = secao.id !== nome;
  history.replaceState(null, "", "#" + nome);
  try { localStorage.setItem("sefaz-emulada.aba", nome); } catch { /* sem armazenamento: começa na primeira */ }
}

// Os autorizadores

function paralisacoes(sigla) {
  return estado.falhas.filter((f) => f.tipo === "paralisacao" && f.destino?.toUpperCase() === sigla.toUpperCase());
}

function desenharAutorizadores() {
  const cartoes = estado.autorizadores.map((autorizador) => {
    const ativas = paralisacoes(autorizador.sigla);
    const todas = ativas.filter((f) => !f.servico);
    const paralisado = todas.length > 0;
    const tipo = autorizador.sigla === "AN" ? "Ambiente Nacional"
      : autorizador.tipoEmissao ? `Contingência, tpEmis ${autorizador.tipoEmissao}`
      : autorizador.sigla.startsWith("SV") ? "SEFAZ virtual" : "SEFAZ da UF";
    const situacao = paralisado ? selo(`Paralisado (${todas[0].codigo})`, "erro")
      : ativas.length > 0 ? selo(`${ativas.length} serviço(s) paralisado(s)`, "alerta") : selo("Operando", "ok");
    const contingencia = paralisado && autorizador.contingencia.length > 0
      ? el("p", null, "Na contingência: ", [...new Set(autorizador.contingencia.map((c) => c.svc))].join(", "),
        " atende ", autorizador.contingencia.map((c) => c.uf).join(", "), ".")
      : null;
    const acoes = paralisado || ativas.length > 0
      ? [el("button", { type: "button", class: "primario pequeno", onclick: () => liberar(autorizador.sigla) }, "Liberar")]
      : [
        el("button", { type: "button", class: "pequeno", title: "Serviço paralisado momentaneamente", onclick: () => paralisar(autorizador.sigla, "108") }, "Paralisar 108"),
        el("button", { type: "button", class: "pequeno", title: "Serviço paralisado sem previsão", onclick: () => paralisar(autorizador.sigla, "109") }, "Paralisar 109"),
      ];
    return el("article", { class: "cartao" + (paralisado ? " paralisado" : "") },
      el("div", { class: "cartao-topo" }, el("div", null, el("strong", null, autorizador.sigla), " ", el("span", { class: "tipo" }, tipo)), situacao),
      autorizador.atendidas.length > 0
        ? el("div", { class: "ufs", title: "As UFs atendidas" }, autorizador.atendidas.map((uf) => el("span", null, uf)))
        : el("p", null, "Sem UF: a distribuição e a manifestação."),
      el("div", { class: "servicos", title: "Os serviços que o emulador responde aqui" },
        autorizador.servicos.map((s) => el("span", { title: s }, SERVICOS[s] ?? s))),
      contingencia,
      el("div", { class: "acoes" }, acoes));
  });
  substituir($("lista-autorizadores"), cartoes);
}

function paralisar(sigla, codigo) {
  return executar(() => api("POST", "/falhas", { tipo: "paralisacao", destino: sigla, codigo }), `${sigla} paralisado (${codigo}).`);
}

function liberar(sigla) {
  return executar(async () => {
    for (const falha of paralisacoes(sigla)) await api("DELETE", `/falhas/${falha.id}`);
  }, `${sigla} liberado.`);
}

// As falhas

function desenharFalhas() {
  $("contagem-falhas").textContent = estado.falhas.length || "";
  const linhas = estado.falhas.map((falha) => el("tr", null,
    el("td", { class: "num" }, falha.id),
    el("td", null, FALHAS.find((f) => f.tipo === falha.tipo)?.nome ?? falha.tipo),
    el("td", null, falha.destino ?? "todos"),
    el("td", null, falha.servico ?? "todos"),
    el("td", null, [
      falha.milissegundos ? `${falha.milissegundos} ms` : null,
      falha.codigo ? `cStat ${falha.codigo}` : null,
      falha.observacao ? `"${falha.observacao}"` : null,
      falha.depois ? "depois de processar" : null,
    ].filter(Boolean).join(", ")),
    el("td", null, falha.restantes ?? "até remover"),
    el("td", null, el("button", { type: "button", class: "pequeno", onclick: () => executar(() => api("DELETE", `/falhas/${falha.id}`)) }, "Remover"))));
  substituir($("lista-falhas"), linhas);
  $("vazio-falhas").hidden = linhas.length > 0;
}

function opcoesDestino(selecionado) {
  return [el("option", { value: "" }, "Todos"), ...estado.autorizadores.map((a) => el("option", { value: a.sigla, selected: a.sigla === selecionado }, a.sigla))];
}

function servicosConhecidos() {
  return [...new Set(estado.autorizadores.flatMap((a) => a.servicos))].sort();
}

function montarFormulariosFalhas() {
  const cartoes = FALHAS.map((modelo) => {
    const campos = [];
    if (modelo.campos.includes("codigo")) {
      campos.push(el("div", { class: "duas" },
        el("label", null, "Código", el("select", { name: "codigo" }, el("option", { value: "108" }, "108, momentaneamente"), el("option", { value: "109" }, "109, sem previsão"))),
        el("label", null, "Observação (xObs)", el("input", { name: "observacao", maxlength: 255 }))));
    }
    if (modelo.campos.includes("milissegundos"))
      campos.push(el("label", null, "Milissegundos", el("input", { name: "milissegundos", type: "number", min: 1, value: 3000, required: true })));
    if (modelo.campos.includes("depois"))
      campos.push(el("label", { class: "opcao" }, el("input", { name: "depois", type: "checkbox" }), "Depois de processar (a queda no meio)"));
    const servicos = modelo.servico ? [modelo.servico] : servicosConhecidos();
    const formulario = el("form", { class: "formulario" },
      el("div", { class: "duas" },
        el("label", null, "Destino", el("select", { name: "destino" }, opcoesDestino())),
        el("label", null, "Serviço", el("select", { name: "servico" },
          modelo.servico ? null : el("option", { value: "" }, "Todos"),
          servicos.map((s) => el("option", { value: s }, s))))),
      campos,
      el("label", null, "Vezes (vazio: até remover)", el("input", { name: "vezes", type: "number", min: 1, placeholder: "até remover" })),
      el("div", null, el("button", { type: "submit", class: "primario" }, "Comandar")));
    formulario.addEventListener("submit", (evento) => {
      evento.preventDefault();
      const dados = new FormData(formulario);
      const pedido = { tipo: modelo.tipo };
      if (dados.get("destino")) pedido.destino = dados.get("destino");
      if (dados.get("servico")) pedido.servico = dados.get("servico");
      if (dados.get("vezes")) pedido.vezes = Number(dados.get("vezes"));
      if (dados.get("milissegundos")) pedido.milissegundos = Number(dados.get("milissegundos"));
      if (dados.get("codigo")) pedido.codigo = dados.get("codigo");
      if (dados.get("observacao")) pedido.observacao = dados.get("observacao");
      if (dados.get("depois")) pedido.depois = true;
      executar(() => api("POST", "/falhas", pedido), `${modelo.nome} comandada.`);
    });
    return el("article", { class: "cartao" }, el("strong", null, modelo.nome), el("p", null, modelo.texto), formulario);
  });
  substituir($("formularios-falhas"), cartoes);
}

// Os pedidos

function desenharPedidos() {
  const filtro = $("filtro-pedidos").value.trim().toLowerCase();
  const pedidos = estado.pedidos
    .filter((p) => !filtro || `${p.destino} ${p.servico} ${p.situacao} ${p.transmissor}`.toLowerCase().includes(filtro))
    .slice().reverse();
  const linhas = pedidos.map((pedido) => el("tr", {
    class: pedido.id === estado.pedidoEscolhido ? "escolhida" : null,
    onclick: () => abrirPedido(pedido.id),
  },
  el("td", { class: "num" }, pedido.id),
  el("td", null, hora(pedido.recebimento)),
  el("td", null, pedido.destino),
  el("td", null, pedido.servico),
  el("td", { class: "quebra" }, seloSituacao(pedido)),
  el("td", null, pedido.status ?? "—"),
  el("td", { class: "num" }, pedido.milissegundos.toLocaleString("pt-BR"))));
  substituir($("lista-pedidos"), linhas);
  $("vazio-pedidos").hidden = linhas.length > 0;
  $("contagem-pedidos").textContent = estado.pedidos.length || "";
}

function acrescentarPedido(resumo) {
  if (estado.pedidos.some((p) => p.id === resumo.id)) return;
  estado.pedidos.push(resumo);
  const limite = estado.emulador?.pedidos ?? 100;
  while (estado.pedidos.length > limite) estado.pedidos.shift();
  if (!$("pausar-pedidos").checked) desenharPedidos();
}

async function abrirPedido(id) {
  estado.pedidoEscolhido = id;
  desenharPedidos();
  const detalhe = $("detalhe-pedido");
  try {
    const pedido = await api("GET", `/pedidos/${id}`);
    const resposta = pedido.resposta ?? null;
    substituir(detalhe,
      el("div", { class: "detalhe-topo" },
        el("div", null,
          el("h4", null, `#${pedido.id} ${pedido.destino}/${pedido.servico}`),
          el("p", { class: "meta" }, `${dataHora(pedido.recebimento)} · ${pedido.milissegundos} ms · HTTP ${pedido.status ?? "sem resposta"} · transmissor ${pedido.transmissor || "—"}`),
          el("p", { class: "meta" }, pedido.situacao)),
        el("button", { type: "button", class: "pequeno", onclick: fecharPedido }, "Fechar")),
      blocoXml("Pedido", pedido.pedido, true),
      resposta === null ? el("p", { class: "meta" }, "Sem resposta: a conexão caiu ou nunca respondeu.") : blocoXml("Resposta", resposta, true),
      resposta === null ? null : documentosCompactados(resposta));
    detalhe.hidden = false;
  } catch (erro) {
    avisar(erro.message);
  }
}

function fecharPedido() {
  estado.pedidoEscolhido = null;
  $("detalhe-pedido").hidden = true;
  desenharPedidos();
}

// Os docZip da distribuição, descompactados no navegador (gzip e base64), um bloco cada.
function documentosCompactados(texto) {
  const documento = new DOMParser().parseFromString(texto, "application/xml");
  const compactados = [...documento.getElementsByTagNameNS("http://www.portalfiscal.inf.br/nfe", "docZip")];
  if (compactados.length === 0 || typeof DecompressionStream === "undefined") return null;
  const lista = el("div", { class: "formulario" });
  compactados.forEach(async (compactado, posicao) => {
    const titulo = `docZip ${compactado.getAttribute("NSU") ?? "sem NSU"} · ${compactado.getAttribute("schema")}`;
    const lugar = el("div");
    lista.append(lugar);
    try {
      const bytes = Uint8Array.from(atob(compactado.textContent.trim()), (c) => c.charCodeAt(0));
      const fluxo = new Blob([bytes]).stream().pipeThrough(new DecompressionStream("gzip"));
      lugar.replaceWith(blocoXml(titulo, await new Response(fluxo).text(), posicao === 0));
    } catch (erro) {
      lugar.replaceWith(el("p", { class: "meta" }, `${titulo}: não descompactou (${erro.message}).`));
    }
  });
  return lista;
}

// O XML formatado

function blocoXml(titulo, texto, aberto) {
  const copiar = el("button", {
    type: "button",
    class: "pequeno",
    onclick: (evento) => {
      evento.preventDefault();
      navigator.clipboard?.writeText(texto).then(() => avisar(`${titulo}: copiado.`, true), () => avisar("Não deu para copiar."));
    },
  }, "Copiar");
  const pre = el("pre", { class: "xml" });
  formatarXml(texto, pre);
  return el("details", { open: aberto }, el("summary", null, el("span", { class: "titulo" }, titulo), copiar), pre);
}

// Indenta o XML com os nós do DOM (as cores por classe); o que não é XML sai como veio.
function formatarXml(texto, destino) {
  const documento = new DOMParser().parseFromString(texto, "application/xml");
  if (documento.getElementsByTagName("parsererror").length > 0) {
    destino.textContent = texto;
    return;
  }
  const partes = [];
  const parte = (classe, valor) => partes.push(classe ? el("span", { class: classe }, valor) : document.createTextNode(valor));
  const escapar = (valor) => valor.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
  function abrir(no) {
    parte("t", "<" + no.nodeName);
    for (const atributo of no.attributes) {
      parte(null, " ");
      parte("a", atributo.name);
      parte(null, "=");
      parte("v", `"${escapar(atributo.value).replace(/"/g, "&quot;")}"`);
    }
  }
  function visitar(no, nivel) {
    const recuo = "  ".repeat(nivel);
    if (no.nodeType === Node.COMMENT_NODE) {
      parte("c", `${recuo}<!--${no.nodeValue}-->\n`);
      return;
    }
    if (no.nodeType !== Node.ELEMENT_NODE) return;
    const filhos = [...no.childNodes].filter((f) => f.nodeType === Node.ELEMENT_NODE || f.nodeType === Node.COMMENT_NODE
      || ((f.nodeType === Node.TEXT_NODE || f.nodeType === Node.CDATA_SECTION_NODE) && f.nodeValue.trim() !== ""));
    parte(null, recuo);
    abrir(no);
    if (filhos.length === 0) {
      parte("t", "/>");
      parte(null, "\n");
      return;
    }
    if (filhos.every((f) => f.nodeType === Node.TEXT_NODE || f.nodeType === Node.CDATA_SECTION_NODE)) {
      parte("t", ">");
      parte(null, escapar(filhos.map((f) => f.nodeValue).join("")));
      parte("t", `</${no.nodeName}>`);
      parte(null, "\n");
      return;
    }
    parte("t", ">");
    parte(null, "\n");
    for (const filho of filhos) {
      if (filho.nodeType === Node.TEXT_NODE || filho.nodeType === Node.CDATA_SECTION_NODE) parte(null, `${recuo}  ${escapar(filho.nodeValue.trim())}\n`);
      else visitar(filho, nivel + 1);
    }
    parte(null, recuo);
    parte("t", `</${no.nodeName}>`);
    parte(null, "\n");
  }
  visitar(documento.documentElement, 0);
  destino.replaceChildren(...partes);
}

// As notas

function desenharNotas() {
  const filtro = $("filtro-notas").value.trim().toLowerCase();
  const eventosNota = (chave) => estado.eventos.filter((e) => e.chave === chave);
  const notas = estado.notas
    .filter((n) => !filtro || `${n.chave} ${n.protocolo} ${n.autorizador}`.toLowerCase().includes(filtro))
    .slice().reverse();
  const linhas = notas.map((nota) => el("tr", {
    class: nota.chave === estado.notaEscolhida ? "escolhida" : null,
    onclick: () => abrirNota(nota.chave, true),
  },
  el("td", { class: "mono" }, nota.chave),
  el("td", null, nota.autorizador),
  el("td", null, nota.cancelada ? selo("Cancelada", "erro") : selo(nota.situacao === "150" ? "150, fora de prazo" : "100, autorizada", "ok")),
  el("td", { class: "mono" }, nota.protocolo),
  el("td", null, dataHora(nota.recebimento)),
  el("td", { class: "num" }, eventosNota(nota.chave).length || "")));
  substituir($("lista-notas"), linhas);
  $("vazio-notas").hidden = linhas.length > 0;
  $("contagem-notas").textContent = estado.notas.length || "";

  const faixas = estado.inutilizacoes.slice().reverse().map((f) => el("tr", null,
    el("td", null, f.uf), el("td", { class: "mono" }, f.cnpj), el("td", null, f.modelo), el("td", null, f.serie),
    el("td", { class: "mono" }, f.inicio === f.fim ? f.inicio : `${f.inicio} a ${f.fim}`),
    el("td", { class: "mono" }, f.protocolo), el("td", null, f.autorizador), el("td", null, dataHora(f.registro))));
  substituir($("lista-inutilizacoes"), faixas);
  $("vazio-inutilizacoes").hidden = faixas.length > 0;
}

async function abrirNota(chave, rolar) {
  estado.notaEscolhida = chave;
  desenharNotas();
  try {
    const detalhe = await api("GET", `/notas/${encodeURIComponent(chave)}`);
    const nota = detalhe.nota;
    substituir($("detalhe-nota"),
      el("div", { class: "detalhe-topo" },
        el("div", null,
          el("h4", { class: "mono" }, nota.chave),
          el("p", { class: "meta" }, `${nota.autorizador} · protocolo ${nota.protocolo} · ${dataHora(nota.recebimento)}${nota.recibo ? ` · recibo ${nota.recibo}` : ""}${nota.cancelada ? " · cancelada" : ""}`)),
        el("button", { type: "button", class: "pequeno", onclick: fecharNota }, "Fechar")),
      detalhe.eventos.length === 0 ? el("p", { class: "meta" }, "Sem eventos.") : el("div", { class: "tabela-rolavel" },
        el("table", { class: "tabela" },
          el("thead", null, el("tr", null, el("th", null, "Evento"), el("th", null, "Seq."), el("th", null, "Protocolo"), el("th", null, "Onde"), el("th", null, "Registro"))),
          el("tbody", null, detalhe.eventos.map(({ evento }) => el("tr", null,
            el("td", null, `${evento.tipo} ${TIPOS_EVENTO[evento.tipo] ?? ""}`),
            el("td", { class: "num" }, evento.sequencia),
            el("td", { class: "mono" }, evento.protocolo),
            el("td", null, evento.autorizador),
            el("td", null, dataHora(evento.registro))))))),
      blocoXml("nfeProc", detalhe.xml, false),
      detalhe.eventos.map(({ evento, xml }) => blocoXml(`procEventoNFe ${evento.tipo} (${evento.sequencia})`, xml, false)));
    $("detalhe-nota").hidden = false;
    if (rolar && window.matchMedia("(max-width: 1099px)").matches) $("detalhe-nota").scrollIntoView({ behavior: "smooth", block: "start" });
  } catch (erro) {
    fecharNota();
    avisar(erro.message);
  }
}

function fecharNota() {
  estado.notaEscolhida = null;
  $("detalhe-nota").hidden = true;
  desenharNotas();
}

// A distribuição

function desenharDistribuicao() {
  const blocos = estado.distribuicao.map((interessado) => el("div", { class: "interessado" },
    el("h4", null, interessado.interessado, " ", el("span", { class: "contagem" }, `${interessado.documentos.length} NSU`)),
    el("div", { class: "tabela-rolavel" },
      el("table", { class: "tabela" },
        el("thead", null, el("tr", null, el("th", { class: "num" }, "NSU"), el("th", null, "Schema"), el("th", null, "Chave"), el("th", null, "Recebido"))),
        el("tbody", null, interessado.documentos.slice().reverse().map((d) => el("tr", null,
          el("td", { class: "num" }, d.nsu), el("td", null, d.schema), el("td", { class: "mono" }, d.chave), el("td", null, dataHora(d.recebimento)))))))));
  substituir($("lista-distribuicao"), blocos);
  $("vazio-distribuicao").hidden = blocos.length > 0;
}

// Os cadastros

function desenharCadastros() {
  const contribuintes = estado.contribuintes.map((c) => el("tr", null,
    el("td", null, c.uf), el("td", { class: "mono" }, c.ie), el("td", { class: "mono" }, c.cnpj ?? c.cpf),
    el("td", { class: "quebra" }, c.nome),
    el("td", null, c.habilitado && !estado.irregulares.includes(c.cnpj ?? c.cpf) ? selo("Habilitado", "ok") : selo("Não habilitado", "erro")),
    el("td", null, c.credenciadoNFe),
    el("td", null, el("button", {
      type: "button", class: "pequeno",
      onclick: () => executar(() => api("DELETE", `/cadastros/${encodeURIComponent(c.uf)}/${encodeURIComponent(c.ie)}`)),
    }, "Tirar"))));
  substituir($("lista-contribuintes"), contribuintes);
  $("vazio-contribuintes").hidden = contribuintes.length > 0;

  const irregulares = estado.irregulares.map((documento) => el("li", null, documento,
    el("button", { type: "button", class: "pequeno", "aria-label": `Tirar ${documento}`, onclick: () => executar(() => api("DELETE", `/irregulares/${encodeURIComponent(documento)}`)) }, "×")));
  substituir($("lista-irregulares"), irregulares);
  $("vazio-irregulares").hidden = irregulares.length > 0;

  const codigos = estado.csc.map((c) => el("tr", null,
    el("td", null, c.uf), el("td", { class: "mono" }, c.cnpj), el("td", null, c.id), el("td", { class: "mono" }, c.csc),
    el("td", null, c.revogacao ? dataHora(c.revogacao) : "—"),
    el("td", null, c.revogacao ? null : el("button", {
      type: "button", class: "pequeno",
      onclick: () => executar(() => api("DELETE", `/csc/${encodeURIComponent(c.uf)}/${encodeURIComponent(c.cnpj)}/${encodeURIComponent(c.id)}`), "CSC revogado."),
    }, "Revogar"))));
  substituir($("lista-csc"), codigos);
  $("vazio-csc").hidden = codigos.length > 0;
}

function montarFormulariosCadastros() {
  const siglas = [...new Set(estado.autorizadores.flatMap((a) => a.atendidas))].sort();
  for (const seletor of document.querySelectorAll("select[name='uf']"))
    substituir(seletor, siglas.map((uf) => el("option", { value: uf, selected: uf === "MG" }, uf)));

  const contribuinte = $("form-contribuinte");
  contribuinte.addEventListener("submit", (evento) => {
    evento.preventDefault();
    const dados = new FormData(contribuinte);
    const documento = String(dados.get("documento")).replace(/\D/g, (c) => (/[A-Za-z]/.test(c) ? c.toUpperCase() : ""));
    const corpo = { uf: dados.get("uf"), ie: dados.get("ie"), nome: dados.get("nome"), habilitado: dados.get("habilitado") === "on" };
    if (documento.length === 11) corpo.cpf = documento; else corpo.cnpj = documento;
    if (dados.get("fantasia")) corpo.fantasia = dados.get("fantasia");
    executar(() => api("POST", "/cadastros", corpo), "Contribuinte cadastrado.").then((certo) => certo && contribuinte.reset());
  });

  const irregular = $("form-irregular");
  irregular.addEventListener("submit", (evento) => {
    evento.preventDefault();
    const documento = String(new FormData(irregular).get("documento")).trim().toUpperCase().replace(/[.\-/\s]/g, "");
    executar(() => api("POST", "/irregulares", { documento }), "Irregular cadastrado.").then((certo) => certo && irregular.reset());
  });

  const csc = $("form-csc");
  csc.addEventListener("submit", (evento) => {
    evento.preventDefault();
    const dados = new FormData(csc);
    executar(() => api("POST", "/csc", { uf: dados.get("uf"), cnpj: dados.get("cnpj"), id: dados.get("id"), csc: dados.get("csc") }), "CSC cadastrado.")
      .then((certo) => certo && csc.reset());
  });
}

// A conexão

function desenharConexao() {
  const emulador = estado.emulador;
  $("endereco").textContent = emulador.endereco ?? "";
  $("arquivo-ac").textContent = emulador.autoridade;
  $("endereco-ws").textContent = emulador.endereco ?? "";
  $("endereco-painel").textContent = emulador.painel ?? location.origin + "/";
  $("limite-pedidos").textContent = emulador.pedidos;
  const filtro = $("filtro-urls").value.trim().toLowerCase();
  const linhas = emulador.urls
    .filter((u) => !filtro || `${u.destino} ${u.servico}`.toLowerCase().includes(filtro))
    .map((u) => el("tr", null, el("td", null, u.destino), el("td", null, u.servico), el("td", { class: "mono quebra" }, u.url),
      el("td", null, el("button", {
        type: "button", class: "pequeno",
        onclick: () => navigator.clipboard?.writeText(u.url).then(() => avisar("URL copiada.", true), () => avisar("Não deu para copiar.")),
      }, "Copiar"))));
  substituir($("lista-urls"), linhas);
}

// O fluxo ao vivo

function ouvir() {
  const indicador = $("fluxo");
  const fonte = new EventSource("/_controle/pedidos/fluxo");
  fonte.addEventListener("open", () => {
    indicador.textContent = "ao vivo";
    indicador.className = "fluxo ligado";
    // Ao reconectar, o que chegou enquanto estava fora.
    const ultimo = estado.pedidos.at(-1)?.id;
    api("GET", ultimo ? `/pedidos?depois=${ultimo}` : "/pedidos").then((novos) => novos.forEach(acrescentarPedido)).catch(() => {});
    atualizarDepois();
  });
  fonte.addEventListener("error", () => {
    indicador.textContent = "sem conexão";
    indicador.className = "fluxo desligado";
  });
  fonte.addEventListener("pedido", (evento) => {
    acrescentarPedido(JSON.parse(evento.data));
    atualizarDepois();
  });
  fonte.addEventListener("estado", () => {
    atualizarDepois();
    api("GET", "/pedidos").then((todos) => {
      estado.pedidos = todos;
      if (estado.pedidoEscolhido && !todos.some((p) => p.id === estado.pedidoEscolhido)) fecharPedido();
      else desenharPedidos();
    }).catch(() => {});
  });
}

// A partida

async function iniciar() {
  for (const botao of document.querySelectorAll(".abas button")) botao.addEventListener("click", () => abrirAba(botao.dataset.aba));
  // A aba da URL (#pedidos), a última aberta ou a primeira.
  let aba = location.hash.slice(1);
  if (!aba) {
    try { aba = localStorage.getItem("sefaz-emulada.aba") ?? ""; } catch { /* sem armazenamento */ }
  }
  abrirAba(document.getElementById(aba)?.classList.contains("aba") ? aba : "autorizadores");
  window.addEventListener("hashchange", () => {
    const nome = location.hash.slice(1);
    if (document.getElementById(nome)?.classList.contains("aba")) abrirAba(nome);
  });

  $("filtro-pedidos").addEventListener("input", desenharPedidos);
  $("pausar-pedidos").addEventListener("change", desenharPedidos);
  $("filtro-notas").addEventListener("input", desenharNotas);
  $("filtro-urls").addEventListener("input", desenharConexao);
  $("limpar-pedidos").addEventListener("click", () => executar(async () => {
    await api("DELETE", "/pedidos");
    estado.pedidos = [];
    fecharPedido();
  }));
  $("remover-falhas").addEventListener("click", () => executar(() => api("DELETE", "/falhas"), "Falhas removidas."));
  $("esquecer-notas").addEventListener("click", () => {
    if (confirm("Esquecer as notas, os lotes, os eventos, as faixas inutilizadas e a distribuição?"))
      executar(() => api("DELETE", "/notas"), "Notas esquecidas.");
  });
  $("zerar").addEventListener("click", () => {
    if (!confirm("Zerar tudo? Some com as falhas, as notas, a distribuição, os CSCs, os irregulares, os contribuintes e os pedidos recentes.")) return;
    executar(async () => {
      await api("DELETE", "");
      estado.pedidos = [];
      fecharPedido();
    }, "Tudo zerado.");
  });

  try {
    const [emulador, autorizadores, pedidos] = await Promise.all([api("GET", "/emulador"), api("GET", "/autorizadores"), api("GET", "/pedidos")]);
    Object.assign(estado, { emulador, autorizadores, pedidos });
    desenharConexao();
    montarFormulariosFalhas();
    montarFormulariosCadastros();
    desenharPedidos();
    await atualizar();
  } catch (erro) {
    avisar(`O emulador não respondeu: ${erro.message}`);
  }
  ouvir();
}

iniciar();
