# Gera o modelo da NFS-e (src/OpenDFe.NFSe.Modelo: as classes, as enumerações e as subclasses de evento) e o mapa do
# leiaute (src/OpenDFe.Leiaute.NFSe.v101/MapasNFSe.cs) a partir da especificação em espec.py (docs/NFSE.md 7, etapa 9.1).
# Os dois saem da mesma especificação, e o LeiauteNFSe101Testes confere a coerência. Os tipos escritos à mão (a base dos
# eventos, o evento registrado, a ChaveNFSe e o IdentificadorDps) não são tocados.
# Uso, da raiz do repositório: python eng/GerarModeloNFSe/gerar.py .
# Depois de gerar: dotnet build, e os membros públicos novos no PublicAPI.Unshipped.txt.
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
from espec import CLASSES, ENUMS, EVENTOS, S, E, DH, AMB, I, L

RAIZ = sys.argv[1]
MODELO = os.path.join(RAIZ, 'src', 'OpenDFe.NFSe.Modelo')
LEIAUTE = os.path.join(RAIZ, 'src', 'OpenDFe.Leiaute.NFSe.v101')
os.makedirs(MODELO, exist_ok=True)
os.makedirs(LEIAUTE, exist_ok=True)

TIPOS = {'s': 'string?', 'd': 'decimal?', 'i': 'int?', 'l': 'long?', 'dt': 'DateOnly?', 'dh': 'DateTimeOffset?', 'uf': 'UF?', 'amb': 'Ambiente?'}

def escrever(caminho, texto):
    with open(caminho, 'w', encoding='utf-8', newline='\n') as f:
        f.write(texto)

def propriedade(c):
    tipo, tag, prop, extra, doc, o = c
    linhas = [f'    /// <summary>{doc}</summary>', f'    [Tag("{tag}")]']
    if tipo in TIPOS:
        linhas.append(f'    public {TIPOS[tipo]} {prop} {{ get; set; }}')
    elif tipo == 'e':
        linhas.append(f'    public {extra[0]}? {prop} {{ get; set; }}')
    elif tipo == 'g':
        linhas.append(f'    public {extra}? {prop} {{ get; set; }}')
    elif tipo == 'G':
        linhas.append(f'    public {extra} {prop} {{ get; set; }} = new();')
    elif tipo == 'L':
        linhas.append(f'    public IList<{extra}> {prop} {{ get; }} = new List<{extra}>();')
    elif tipo == 'Ls':
        linhas.append(f'    public IList<string> {prop} {{ get; }} = new List<string>();')
    else:
        raise ValueError(tipo)
    return '\n'.join(linhas)

CABECALHO = '// Gerado por eng/GerarModeloNFSe (docs/NFSE.md 7): o modelo e o mapa do leiaute saem da mesma especificação.\n'

for c in CLASSES:
    corpo = '\n\n'.join(propriedade(x) for x in c['campos'])
    escrever(os.path.join(MODELO, c['nome'] + '.cs'),
             f"namespace OpenDFe.NFSe;\n\n/// <summary>{c['doc']}</summary>\npublic sealed class {c['nome']}\n{{\n{corpo}\n}}\n")

for e in ENUMS:
    membros = '\n\n'.join(f'    /// <summary>{d} ({v}).</summary>\n    {m} = {v},' for v, m, d in e['membros'])
    escrever(os.path.join(MODELO, e['nome'] + '.cs'),
             f"namespace OpenDFe.NFSe;\n\n/// <summary>{e['doc']} O valor de cada membro é o do XML.</summary>\npublic enum {e['nome']}\n{{\n{membros}\n}}\n")

for nome, tipo, doc, campos, texto101, texto100, emite in EVENTOS:
    corpo = [f'    /// <summary>O tipo do evento: <c>"{tipo}"</c>.</summary>', f'    public override string Tipo => "{tipo}";']
    corpo += ['', *('\n\n'.join(propriedade(x) for x in campos).split('\n'))] if campos else []
    escrever(os.path.join(MODELO, nome + '.cs'),
             f"namespace OpenDFe.NFSe;\n\n/// <summary>{doc}</summary>\n[Tag(\"e{tipo}\")]\npublic sealed class {nome} : EventoNFSe\n{{\n" + '\n'.join(corpo) + "\n}\n")

# ------------------------------------------------------------ mapa do leiaute

def opcoes(o):
    s = ''
    if o.get('v100'):
        s += ', versao100: true'
    if o.get('alias'):
        s += ', outras: [' + ', '.join(f'"{a}"' for a in o['alias']) + ']'
    if o.get('textoLivre100'):
        s += ', textoLivre100: true'
    return s

def campo_mapa(dono, c):
    tipo, tag, prop, extra, doc, o = c
    get = f'g => g.{prop}'
    put = f'(g, v) => g.{prop} = v'
    op = opcoes(o)
    if tipo == 's':
        return f'Campo<{dono}>.Texto("{tag}", "{prop}", {get}, {put}{op})'
    if tipo == 'd':
        return f'Campo<{dono}>.Decimal("{tag}", "{prop}", {get}, {put}, 2{op})'
    if tipo == 'i':
        return f'Campo<{dono}>.Inteiro("{tag}", "{prop}", {get}, {put}{op})'
    if tipo == 'l':
        return f'Campo<{dono}>.Longo("{tag}", "{prop}", {get}, {put}{op})'
    if tipo == 'dt':
        return f'Campo<{dono}>.Data("{tag}", "{prop}", {get}, {put}{op})'
    if tipo == 'dh':
        return f'Campo<{dono}>.DataHora("{tag}", "{prop}", {get}, {put}{op})'
    if tipo == 'uf':
        return f'Campo<{dono}>.Sigla("{tag}", "{prop}", {get}, {put}{op})'
    if tipo == 'amb':
        return f'Campo<{dono}>.Enumeracao<Ambiente>("{tag}", "{prop}", {get}, {put}, 0{op})'
    if tipo == 'e':
        return f'Campo<{dono}>.Enumeracao<{extra[0]}>("{tag}", "{prop}", {get}, {put}, {extra[1]}{op})'
    if tipo == 'G' and extra == 'Dps':
        return f'Campo<{dono}>.Dps("{tag}", "{prop}", {put})'
    if tipo in ('g', 'G'):
        return f'Campo<{dono}>.Grupo<{extra}>("{tag}", "{prop}", {get}, {put}, () => Mapa{extra}!{op})'
    if tipo == 'L':
        return f'Campo<{dono}>.Lista<{extra}>("{tag}", "{prop}", {get}, () => Mapa{extra}!{op})'
    if tipo == 'Ls':
        return f'Campo<{dono}>.Textos("{tag}", "{prop}", {get}{op})'
    raise ValueError(tipo)

# o tpBM da NFS-e 1.00 é texto livre (M10)
for c in CLASSES:
    if c['nome'] == 'ValoresNFSe':
        c['campos'] = [(t, tag, p, x, d, dict(o, textoLivre100=True)) if tag == 'tpBM' else (t, tag, p, x, d, o) for t, tag, p, x, d, o in c['campos']]

def mapa(nome, campos, escolhas, extras=()):
    linhas = [campo_mapa(nome, c) for c in campos] + list(extras)
    corpo = ',\n            '.join(linhas)
    esc = ', '.join('[' + ', '.join(f'"{p}"' for p in e) + ']' for e in escolhas)
    lista = f'[\n            {corpo},\n        ]' if linhas else '[]'
    return (f'    internal static readonly MapaGrupo Mapa{nome} = MapaGrupo.Criar<{nome}>(\n'
            f'        {lista},\n        [{esc}]);\n')

blocos = []
for c in CLASSES:
    extras = []
    if c['nome'] == 'ValoresNFSe':
        extras.append('Campo<ValoresNFSe>.Desvio("xOutInf", (l, texto) => l.InformacoesFisco100 = texto)')
    blocos.append(mapa(c['nome'], c['campos'], c['escolhas'], extras))

BASE_EVENTO = [
    AMB('tpAmb', 'Ambiente', ''), S('verAplic', 'VersaoAplicativo', ''), DH('dhEvento', 'Data', ''),
    S('CNPJAutor', 'CnpjAutor', ''), S('CPFAutor', 'CpfAutor', ''), S('chNFSe', 'Chave', ''),
]
blocos.append(mapa('EventoNFSe', BASE_EVENTO, [('CnpjAutor', 'CpfAutor')]))
REGISTRADO = [
    S('verAplic', 'VersaoAplicativo', ''), E('ambGer', 'AmbienteGerador', 'AmbienteGeradorEvento', 0, ''),
    I('nSeqEvento', 'Sequencia', ''), DH('dhProc', 'Processamento', ''), L('nDFSe', 'NumeroDfse', ''),
]
blocos.append(mapa('EventoNFSeRegistrado', REGISTRADO, [],
                   ['Campo<EventoNFSeRegistrado>.Pedido("pedRegEvento", "Pedido", (g, v) => g.Pedido = v)']))
for nome, tipo, doc, campos, texto101, texto100, emite in EVENTOS:
    blocos.append(mapa(nome, campos, []))

tabela = ',\n        '.join(
    f'new("{tipo}", "e{tipo}", typeof({nome}), () => new {nome}(), () => Mapa{nome}!, "{t1}", "{t0}", {"true" if emite else "false"})'
    for nome, tipo, doc, campos, t1, t0, emite in EVENTOS)

escrever(os.path.join(LEIAUTE, 'MapasNFSe.cs'),
         CABECALHO + '\nnamespace OpenDFe.NFSe;\n\n'
         '/// <summary>\n'
         '/// O mapa do modelo da NFS-e no leiaute 1.01 (docs/NFSE.md 7): para cada classe, os campos na ordem do XSD, com a tag, a\n'
         '/// propriedade, o tipo e as escolhas. A escrita, a leitura e o caminho do problema andam pelo mesmo mapa. O grupo aponta\n'
         '/// o mapa dele por uma função, porque os campos estáticos se inicializam na ordem do arquivo; o <c>!</c> diz ao\n'
         '/// compilador que, quando a função roda, todos já estão prontos.\n'
         '/// </summary>\n'
         'internal static class MapasNFSe\n{\n' + '\n'.join(blocos) +
         '\n    /// <summary>Os tipos de evento do XSD 1.01, com o texto fixo do <c>xDesc</c> nas duas versões.</summary>\n'
         '    internal static readonly TipoEventoNFSe[] Eventos =\n    [\n        ' + tabela + ',\n    ];\n}\n')

print('classes', len(CLASSES), 'enums', len(ENUMS), 'eventos', len(EVENTOS))
