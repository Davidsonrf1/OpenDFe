// O painel da SEFAZ emulada (E.6; a aba NFS-e na E.9): fala só com as rotas /_controle da própria porta, sem biblioteca nem build. Todo
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
  municipios: [],
  nfse: [],
  pedidoEscolhido: null,
  notaEscolhida: null,
  nfseEscolhida: null,
  nfseSituacao: null,
};

// Os eventos da NFS-e (E.8), pelo tipo.
const TIPOS_EVENTO_NFSE = {
  "101101": "Cancelamento",
  "101103": "Análise fiscal para cancelamento",
  "105102": "Cancelamento por substituição",
  "105104": "Cancelamento deferido pela análise",
  "105105": "Cancelamento indeferido pela análise",
  "305101": "Cancelamento por ofício",
};

// A situação da NFS-e pelos eventos, como o /_controle/nfse a dá.
const SITUACOES_NFSE = {
  normal: ["Normal", "ok"],
  cancelada: ["Cancelada", "erro"],
  substituida: ["Substituída", "erro"],
  emAnalise: ["Em análise fiscal", "alerta"],
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
  { tipo: "paralisacao", nome: "Paralisação", texto: "O cStat 108 ou 109, depois das rejeições do próprio pedido. Na Sefin Nacional e no ADN, o HTTP 503, sem código.", campos: ["codigo", "observacao"] },
  { tipo: "atraso", nome: "Atraso", texto: "Espera e depois responde normalmente.", campos: ["milissegundos"] },
  { tipo: "tempoEsgotado", nome: "Tempo esgotado", texto: "Nunca responde: segura o pedido até o cliente desistir.", campos: [] },
  { tipo: "conexaoDerrubada", nome: "Conexão derrubada", texto: "Derruba sem resposta. Depois de processar: a nota fica autorizada e a resposta não chega.", campos: ["depois"] },
  { tipo: "respostaIlegivel", nome: "Resposta ilegível", texto: "A resposta cortada ao meio: o envelope, ou o JSON na Sefin Nacional e no ADN.", campos: [] },
  { tipo: "falhaSoap", nome: "Falha SOAP", texto: "HTTP 500 com a falha soap:Receiver. Na Sefin Nacional e no ADN, o HTTP 500 com o erro interno.", campos: [] },
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
  // A Sefin Nacional e o ADN: pelo HTTP (2xx certo, 404 não achou, o resto erro).
  if (ehRest(pedido.destino)) {
    if (pedido.status === null || pedido.status === undefined || pedido.status >= 500) return selo(pedido.situacao, "erro");
    if (pedido.status >= 200 && pedido.status < 300) return selo(pedido.situacao, "ok");
    return selo(pedido.situacao, pedido.status === 404 ? "alerta" : "erro");
  }
  const codigo = /cStat (\d{3})/.exec(pedido.situacao)?.[1];
  if (pedido.status === null || pedido.status === undefined) return selo(pedido.situacao, "erro");
  if (pedido.status >= 400) return selo(pedido.situacao, "erro");
  if (!codigo) return selo(pedido.situacao, "alerta");
  const sucesso = ["100", "101", "102", "103", "104", "107", "111", "112", "128", "135", "137", "138", "150"];
  const espera = ["105", "108", "109"];
  return selo(pedido.situacao, sucesso.includes(codigo) ? "ok" : espera.includes(codigo) ? "alerta" : "erro");
}

// Os destinos REST da NFS-e (a Sefin Nacional e o ADN), com os serviços de cada um.
function destinosRest() {
  return estado.emulador?.nfse?.destinos ?? [];
}

function ehRest(destino) {
  return destinosRest().some((d) => d.destino.toLowerCase() === String(destino ?? "").toLowerCase());
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
  const [falhas, notas, eventos, inutilizacoes, distribuicao, contribuintes, irregulares, csc, municipios, nfse] = await Promise.all([
    api("GET", "/falhas"), api("GET", "/notas"), api("GET", "/eventos"), api("GET", "/inutilizacoes"),
    api("GET", "/distribuicao"), api("GET", "/cadastros"), api("GET", "/irregulares"), api("GET", "/csc"),
    api("GET", "/municipios"), api("GET", "/nfse"),
  ]);
  Object.assign(estado, { falhas, notas, eventos, inutilizacoes, distribuicao, contribuintes, irregulares, csc, municipios, nfse });
  desenharAutorizadores();
  desenharFalhas();
  desenharNotas();
  desenharNFSe();
  desenharDistribuicao();
  desenharCadastros();
  if (estado.notaEscolhida && !estado.notas.some((n) => n.chave === estado.notaEscolhida)) fecharNota();
  else if (estado.notaEscolhida) abrirNota(estado.notaEscolhida, false);
  // A NFS-e aberta só é lida de novo quando a situação muda (um evento), para não apagar o formulário do evento.
  const escolhida = estado.nfse.find((n) => n.chave === estado.nfseEscolhida);
  if (estado.nfseEscolhida && !escolhida) fecharNFSe();
  else if (escolhida && escolhida.situacao !== estado.nfseSituacao) abrirNFSe(escolhida.chave, false);
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
      falha.tipo === "paralisacao" && ehRest(falha.destino) ? "HTTP 503" : null,
      falha.observacao ? `"${falha.observacao}"` : null,
      falha.depois ? "depois de processar" : null,
    ].filter(Boolean).join(", ")),
    el("td", null, falha.restantes ?? "até remover"),
    el("td", null, el("button", { type: "button", class: "pequeno", onclick: () => executar(() => api("DELETE", `/falhas/${falha.id}`)) }, "Remover"))));
  substituir($("lista-falhas"), linhas);
  $("vazio-falhas").hidden = linhas.length > 0;
}

// Os destinos das falhas: os autorizadores da NF-e e, se a falha vale para eles, os destinos REST da NFS-e.
function opcoesDestino(comRest) {
  return [
    el("option", { value: "" }, "Todos"),
    el("optgroup", { label: "NF-e" }, estado.autorizadores.map((a) => el("option", { value: a.sigla }, a.sigla))),
    comRest ? el("optgroup", { label: "NFS-e" }, destinosRest().map((d) => el("option", { value: d.destino }, d.destino))) : null,
  ];
}

// Os serviços do destino escolhido; sem destino, os da NF-e e os da NFS-e.
function servicosDestino(destino) {
  const rest = destinosRest().find((d) => d.destino === destino);
  if (rest) return rest.servicos;
  const autorizador = estado.autorizadores.find((a) => a.sigla === destino);
  if (autorizador) return autorizador.servicos;
  return [...new Set(estado.autorizadores.flatMap((a) => a.servicos)), ...destinosRest().flatMap((d) => d.servicos)].sort();
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
    const seletorServico = el("select", { name: "servico" });
    const preencherServicos = (destino) => substituir(seletorServico,
      modelo.servico ? null : el("option", { value: "" }, "Todos"),
      (modelo.servico ? [modelo.servico] : servicosDestino(destino)).map((s) => el("option", { value: s }, s)));
    preencherServicos("");
    const seletorDestino = el("select", { name: "destino", onchange: () => preencherServicos(seletorDestino.value) }, opcoesDestino(!modelo.servico));
    const formulario = el("form", { class: "formulario" },
      el("div", { class: "duas" },
        el("label", null, "Destino", seletorDestino),
        el("label", null, "Serviço", seletorServico)),
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
      // A paralisação da Sefin Nacional e do ADN é o 503, sem código nem observação.
      if (dados.get("codigo") && !ehRest(pedido.destino)) pedido.codigo = dados.get("codigo");
      if (dados.get("observacao") && !ehRest(pedido.destino)) pedido.observacao = dados.get("observacao");
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
      pedido.pedido ? blocoXml("Pedido", pedido.pedido, true) : el("p", { class: "meta" }, "Sem corpo no pedido."),
      xmlsJson("Pedido", pedido.pedido),
      resposta === null ? el("p", { class: "meta" }, "Sem resposta: a conexão caiu ou nunca respondeu.") : blocoXml("Resposta", resposta, true),
      resposta === null ? null : documentosCompactados(resposta),
      resposta === null ? null : xmlsJson("Resposta", resposta));
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
  // Só no XML: o DOMParser, com o JSON da Sefin, monta o documento de erro com estilo inline, que a CSP barra.
  if (!/^\s*</.test(texto)) return null;
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

// Os XMLs em GZip e Base64 dos JSON da Sefin Nacional (dpsXmlGZipB64, nfseXmlGZipB64, eventoXmlGZipB64...), também
// dentro de listas, descompactados no navegador, um bloco cada.
function xmlsJson(titulo, texto) {
  let json;
  try { json = JSON.parse(texto); } catch { return null; }
  const achados = [];
  (function procurar(valor, caminho) {
    if (Array.isArray(valor)) valor.forEach((item, posicao) => procurar(item, `${caminho}[${posicao}]`));
    else if (valor && typeof valor === "object") {
      for (const [nome, item] of Object.entries(valor)) {
        const onde = caminho ? `${caminho}.${nome}` : nome;
        if (typeof item === "string" && /XmlGZipB64$/.test(nome)) achados.push({ nome: onde, valor: item });
        else procurar(item, onde);
      }
    }
  })(json, "");
  if (achados.length === 0 || typeof DecompressionStream === "undefined") return null;
  const lista = el("div", { class: "formulario" });
  achados.forEach(async ({ nome, valor }, posicao) => {
    const lugar = el("div");
    lista.append(lugar);
    try {
      const bytes = Uint8Array.from(atob(valor), (c) => c.charCodeAt(0));
      const fluxo = new Blob([bytes]).stream().pipeThrough(new DecompressionStream("gzip"));
      lugar.replaceWith(blocoXml(`${titulo}: ${nome}`, await new Response(fluxo).text(), posicao === 0));
    } catch (erro) {
      lugar.replaceWith(el("p", { class: "meta" }, `${titulo}: ${nome} não descompactou (${erro.message}).`));
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

// Indenta o XML com os nós do DOM (as cores por classe) e o JSON (a Sefin Nacional e o ADN); o resto sai como veio.
function formatarXml(texto, destino) {
  if (/^\s*[{[]/.test(texto)) {
    try {
      destino.textContent = JSON.stringify(JSON.parse(texto), null, 2);
      return;
    } catch { /* não é JSON: segue como XML */ }
  }
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

// A NFS-e (E.9): a Sefin Nacional e o ADN, os municípios e as NFS-e emitidas

function seloNFSe(situacao) {
  const [texto, tipo] = SITUACOES_NFSE[situacao] ?? [situacao, "alerta"];
  return selo(texto, tipo);
}

function desenharNFSe() {
  desenharDestinosNFSe();
  desenharMunicipios();
  const filtro = $("filtro-nfse").value.trim().toLowerCase();
  const notas = estado.nfse
    .filter((n) => !filtro || `${n.chave} ${n.municipio} ${n.prestador} ${SITUACOES_NFSE[n.situacao]?.[0] ?? n.situacao}`.toLowerCase().includes(filtro))
    .slice().reverse();
  const linhas = notas.map((nota) => el("tr", {
    class: nota.chave === estado.nfseEscolhida ? "escolhida" : null,
    onclick: () => abrirNFSe(nota.chave, true),
  },
  el("td", { class: "mono" }, nota.chave),
  el("td", { class: "num" }, nota.numero),
  el("td", null, nomeMunicipio(nota.municipio)),
  el("td", { class: "mono" }, nota.prestador),
  el("td", null, seloNFSe(nota.situacao)),
  el("td", null, dataHora(nota.processamento))));
  substituir($("lista-nfse"), linhas);
  $("vazio-nfse").hidden = linhas.length > 0;
  $("contagem-nfse").textContent = estado.nfse.length || "";
}

function nomeMunicipio(codigo) {
  const municipio = estado.municipios.find((m) => m.codigo === codigo);
  return municipio ? `${codigo} ${municipio.nome}` : String(codigo);
}

// Os cartões da Sefin Nacional e do ADN: paralisar (503) e liberar, como os autorizadores.
function desenharDestinosNFSe() {
  const textos = {
    SefinNacional: "A emissão, as consultas e os eventos da NFS-e.",
    ADN: "A parametrização municipal: o convênio, as alíquotas, os regimes, as retenções e os benefícios.",
  };
  const cartoes = destinosRest().map((destino) => {
    const ativas = paralisacoes(destino.destino);
    const todas = ativas.filter((f) => !f.servico);
    const paralisado = todas.length > 0;
    const situacao = paralisado ? selo("Paralisado (503)", "erro")
      : ativas.length > 0 ? selo(`${ativas.length} serviço(s) paralisado(s)`, "alerta") : selo("Operando", "ok");
    const acoes = paralisado || ativas.length > 0
      ? el("button", { type: "button", class: "primario pequeno", onclick: () => liberar(destino.destino) }, "Liberar")
      : el("button", {
        type: "button", class: "pequeno",
        onclick: () => executar(() => api("POST", "/falhas", { tipo: "paralisacao", destino: destino.destino }), `${destino.destino} paralisado (503).`),
      }, "Paralisar");
    return el("article", { class: "cartao" + (paralisado ? " paralisado" : "") },
      el("div", { class: "cartao-topo" }, el("strong", null, destino.destino), situacao),
      el("p", null, textos[destino.destino] ?? ""),
      el("div", { class: "servicos", title: "Os serviços que o emulador responde aqui" }, destino.servicos.map((s) => el("span", null, s))),
      el("div", { class: "acoes" }, acoes));
  });
  substituir($("lista-destinos-nfse"), cartoes);
}

// O resumo da configuração do município, numa linha.
function configuracaoMunicipio(m) {
  const partes = [];
  const aliquotas = Object.entries(m.aliquotas ?? {});
  if (aliquotas.length > 0) partes.push("alíquotas " + aliquotas.map(([codigo, valor]) => `${codigo}=${numero(valor)}%`).join(", "));
  if (m.regimesEspeciais?.length) partes.push("regimes " + m.regimesEspeciais.join(", "));
  if (m.retencoes?.length) partes.push(`${m.retencoes.length} CPF(s) na retenção`);
  if (m.beneficios?.length) partes.push(`${m.beneficios.length} benefício(s)`);
  if (m.prazoCancelamento !== null && m.prazoCancelamento !== undefined) partes.push(`cancelamento em ${m.prazoCancelamento} dia(s)`);
  if (m.prazoSubstituicao !== null && m.prazoSubstituicao !== undefined) partes.push(`substituição em ${m.prazoSubstituicao} dia(s)`);
  return partes.join(" · ") || "—";
}

function numero(valor) {
  return Number(valor).toLocaleString("pt-BR", { maximumFractionDigits: 2 });
}

function desenharMunicipios() {
  const linhas = estado.municipios.map((m) => el("tr", null,
    el("td", { class: "mono" }, m.codigo),
    el("td", { class: "quebra" }, m.nome),
    el("td", null, m.paralisado ? selo("Paralisado (503)", "erro")
      : m.convenio === "ativo" ? selo("Conveniado", "ok") : selo(m.convenio === "inativo" ? "Convênio inativo" : "Sem convênio", "alerta")),
    el("td", null, m.emissor === "proprio" ? "Próprio" : "Nacional"),
    el("td", { class: "num" }, `${numero(m.aliquotaPadrao)}%`),
    el("td", { class: "config quebra" }, configuracaoMunicipio(m)),
    el("td", null, el("div", { class: "acoes-linha" },
      el("button", {
        type: "button", class: m.paralisado ? "primario pequeno" : "pequeno",
        onclick: () => executar(() => api("POST", "/municipios", { ...m, paralisado: !m.paralisado }),
          m.paralisado ? `${m.nome} liberado.` : `${m.nome} paralisado (503).`),
      }, m.paralisado ? "Liberar" : "Paralisar"),
      el("button", { type: "button", class: "pequeno", onclick: () => editarMunicipio(m) }, "Editar"),
      el("button", {
        type: "button", class: "pequeno",
        onclick: () => executar(() => api("DELETE", `/municipios/${m.codigo}`), `${m.nome} volta ao padrão.`),
      }, "Tirar")))));
  substituir($("lista-municipios"), linhas);
  $("vazio-municipios").hidden = linhas.length > 0;
}

// Os campos de lista do formulário, separados por ponto e vírgula (ou quebra de linha).
function itens(texto) {
  return String(texto ?? "").split(/[;\n]/).map((i) => i.trim()).filter(Boolean);
}

function decimal(texto, nome) {
  const valor = Number(String(texto).trim().replace(",", "."));
  if (String(texto).trim() === "" || Number.isNaN(valor)) throw new Error(`${nome}: "${texto}" não é número.`);
  return valor;
}

function inteiro(texto, nome) {
  if (!/^\d+$/.test(String(texto).trim())) throw new Error(`${nome}: "${texto}" não é número inteiro.`);
  return Number(texto);
}

// O município do formulário, no JSON do POST /_controle/municipios; o vazio fica de fora (o padrão do emulador).
function lerFormularioMunicipio(formulario) {
  const dados = new FormData(formulario);
  const municipio = { codigo: inteiro(dados.get("codigo"), "O código"), convenio: dados.get("convenio"), emissor: dados.get("emissor"), paralisado: dados.get("paralisado") === "on" };
  if (String(dados.get("nome")).trim()) municipio.nome = String(dados.get("nome")).trim();
  if (String(dados.get("aliquotaPadrao")).trim()) municipio.aliquotaPadrao = decimal(dados.get("aliquotaPadrao"), "A alíquota padrão");
  if (String(dados.get("prazoCancelamento")).trim()) municipio.prazoCancelamento = inteiro(dados.get("prazoCancelamento"), "O prazo do cancelamento");
  if (String(dados.get("prazoSubstituicao")).trim()) municipio.prazoSubstituicao = inteiro(dados.get("prazoSubstituicao"), "O prazo da substituição");
  const aliquotas = itens(dados.get("aliquotas"));
  if (aliquotas.length > 0) {
    municipio.aliquotas = {};
    for (const item of aliquotas) {
      const [codigo, valor] = item.split("=");
      if (valor === undefined) throw new Error(`Alíquota por código: "${item}" é código=alíquota.`);
      municipio.aliquotas[codigo.trim()] = decimal(valor, `A alíquota do código ${codigo.trim()}`);
    }
  }
  const regimes = itens(dados.get("regimesEspeciais"));
  if (regimes.length > 0) municipio.regimesEspeciais = regimes.map((r) => inteiro(r, "O regime especial"));
  const retencoes = itens(dados.get("retencoes")).map((c) => c.replace(/\D/g, ""));
  if (retencoes.length > 0) municipio.retencoes = retencoes;
  const beneficios = itens(dados.get("beneficios"));
  if (beneficios.length > 0) {
    municipio.beneficios = beneficios.map((item) => {
      const [numeroBeneficio, resto] = item.split("=");
      if (resto === undefined) throw new Error(`Benefício: "${item}" é número=tipo, ou número=4:alíquota.`);
      const [tipo, aliquota] = resto.split(":");
      const beneficio = { numero: numeroBeneficio.trim(), tipo: inteiro(tipo, "O tipo do benefício") };
      if (aliquota !== undefined) beneficio.aliquota = decimal(aliquota, "A alíquota do benefício");
      return beneficio;
    });
  }
  return municipio;
}

function editarMunicipio(m) {
  const formulario = $("form-municipio");
  const campo = (nome) => formulario.elements.namedItem(nome);
  campo("codigo").value = m.codigo;
  campo("nome").value = m.nome ?? "";
  campo("convenio").value = m.convenio ?? "ativo";
  campo("emissor").value = m.emissor ?? "nacional";
  campo("aliquotaPadrao").value = m.aliquotaPadrao ?? "";
  campo("prazoCancelamento").value = m.prazoCancelamento ?? "";
  campo("prazoSubstituicao").value = m.prazoSubstituicao ?? "";
  campo("paralisado").checked = Boolean(m.paralisado);
  campo("aliquotas").value = Object.entries(m.aliquotas ?? {}).map(([codigo, valor]) => `${codigo}=${valor}`).join("; ");
  campo("regimesEspeciais").value = (m.regimesEspeciais ?? []).join("; ");
  campo("retencoes").value = (m.retencoes ?? []).join("; ");
  campo("beneficios").value = (m.beneficios ?? []).map((b) => `${b.numero}=${b.tipo}${b.aliquota !== null && b.aliquota !== undefined ? `:${b.aliquota}` : ""}`).join("; ");
  formulario.querySelector("button[type='submit']").textContent = "Salvar";
  formulario.scrollIntoView({ behavior: "smooth", block: "start" });
  campo("nome").focus({ preventScroll: true });
}

function montarFormularioMunicipio() {
  const formulario = $("form-municipio");
  formulario.addEventListener("reset", () => { formulario.querySelector("button[type='submit']").textContent = "Cadastrar"; });
  formulario.addEventListener("submit", (evento) => {
    evento.preventDefault();
    let municipio;
    try {
      municipio = lerFormularioMunicipio(formulario);
    } catch (erro) {
      avisar(erro.message);
      return;
    }
    const existia = estado.municipios.some((m) => m.codigo === municipio.codigo);
    executar(() => api("POST", "/municipios", municipio), existia ? "Município salvo." : "Município cadastrado.")
      .then((certo) => certo && formulario.reset());
  });
}

async function abrirNFSe(chave, rolar) {
  estado.nfseEscolhida = chave;
  desenharNFSe();
  try {
    const nota = await api("GET", `/nfse/${encodeURIComponent(chave)}`);
    estado.nfseSituacao = nota.situacao;
    substituir($("detalhe-nfse"),
      el("div", { class: "detalhe-topo" },
        el("div", null,
          el("h4", { class: "mono" }, nota.chave),
          el("p", { class: "meta" }, `NFS-e ${nota.numero} · nDFSe ${nota.numeroDfse} · ${nomeMunicipio(nota.municipio)} · prestador ${nota.prestador} · ${dataHora(nota.processamento)}`),
          el("p", { class: "meta mono" }, nota.idDps),
          el("p", null, seloNFSe(nota.situacao))),
        el("button", { type: "button", class: "pequeno", onclick: fecharNFSe }, "Fechar")),
      nota.eventos.length === 0 ? el("p", { class: "meta" }, "Sem eventos.") : el("div", { class: "tabela-rolavel" },
        el("table", { class: "tabela" },
          el("thead", null, el("tr", null, el("th", null, "Evento"), el("th", null, "Seq."), el("th", null, "nDFSe"), el("th", null, "Processado"))),
          el("tbody", null, nota.eventos.map((evento) => el("tr", null,
            el("td", null, `${evento.tipo} ${TIPOS_EVENTO_NFSE[evento.tipo] ?? ""}`),
            el("td", { class: "num" }, evento.sequencia),
            el("td", { class: "num" }, evento.numeroDfse),
            el("td", null, dataHora(evento.processamento))))))),
      formularioEventoMunicipio(nota),
      blocoXml("NFSe", nota.xml, false),
      nota.eventos.map((evento) => blocoXml(`evento ${evento.tipo} (${evento.sequencia})`, evento.xml, false)));
    $("detalhe-nfse").hidden = false;
    if (rolar && window.matchMedia("(max-width: 1099px)").matches) $("detalhe-nfse").scrollIntoView({ behavior: "smooth", block: "start" });
  } catch (erro) {
    fecharNFSe();
    avisar(erro.message);
  }
}

// Os eventos do município (o deferimento, o indeferimento e o ofício), que a Sefin não recebe de quem emite.
function formularioEventoMunicipio(nota) {
  if (nota.situacao === "cancelada" || nota.situacao === "substituida") return null;
  const pendente = nota.situacao === "emAnalise";
  const tipos = pendente ? ["105104", "105105"] : ["305101"];
  const processo = el("input", { name: "processo", inputmode: "numeric", pattern: "[0-9]{1,30}", placeholder: "nProcAdm", required: !pendente });
  const formulario = el("form", { class: "formulario" },
    el("strong", null, "Evento do município"),
    el("div", { class: "duas" },
      el("label", null, "Evento", el("select", { name: "tipo" }, tipos.map((t) => el("option", { value: t }, `${t} ${TIPOS_EVENTO_NFSE[t]}`)))),
      el("label", null, "CPF do agente", el("input", { name: "cpfAgente", required: true, inputmode: "numeric", placeholder: "12345678909" }))),
    el("div", { class: "duas" },
      el("label", null, pendente ? "Processo (opcional)" : "Processo", processo),
      el("label", null, pendente ? "Motivo (xMotivo)" : "Descrição do processo (xProcAdm)",
        el("input", { name: "descricao", required: true, minlength: 15, maxlength: 255 }))),
    el("div", null, el("button", { type: "submit", class: "primario" }, "Registrar")));
  formulario.addEventListener("submit", (evento) => {
    evento.preventDefault();
    const dados = new FormData(formulario);
    const pedido = { tipo: dados.get("tipo"), cpfAgente: String(dados.get("cpfAgente")).replace(/\D/g, ""), descricao: dados.get("descricao") };
    if (dados.get("processo")) pedido.processo = dados.get("processo");
    executar(() => api("POST", `/nfse/${encodeURIComponent(nota.chave)}/eventos`, pedido), `Evento ${pedido.tipo} registrado.`);
  });
  return formulario;
}

function fecharNFSe() {
  estado.nfseEscolhida = null;
  estado.nfseSituacao = null;
  $("detalhe-nfse").hidden = true;
  desenharNFSe();
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
  $("endereco-sefin").textContent = emulador.nfse?.sefinNacional ?? "";
  $("endereco-adn").textContent = emulador.nfse?.parametrizacao ?? "";
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
  $("filtro-nfse").addEventListener("input", desenharNFSe);
  $("esquecer-nfse").addEventListener("click", () => {
    if (confirm("Esquecer as NFS-e e os eventos delas? A numeração continua."))
      executar(() => api("DELETE", "/nfse"), "NFS-e esquecidas.");
  });
  montarFormularioMunicipio();
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
    if (!confirm("Zerar tudo? Some com as falhas, as notas, a distribuição, os CSCs, os irregulares, os contribuintes, os municípios, as NFS-e e os pedidos recentes.")) return;
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
