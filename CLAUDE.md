# OpenDFe

## Comandos (saída enxuta)

Vale para a sessão principal e para os subagentes.

- Prefira as ferramentas dedicadas (Read, Grep, Glob, Edit) a `cat`, `grep`, `find` e `sed` no shell.
- Build e teste com saída mínima: `dotnet build -v q --nologo` e `dotnet test --nologo -v q`. Para um teste só, use `--filter`.
- Nunca despeje saída longa: filtre com `| Select-Object -First 30` (PowerShell) ou `| head -n 30` / `| tail -n 30` (Bash). Em falha de build, mostre só as linhas `error`.
- Git: `git status -sb`, `git log --oneline -n 10`, `git diff --stat` antes de qualquer `git diff` completo.
- Leia arquivos grandes por trecho (`offset` e `limit`) e procure com Grep antes de ler inteiro.
- Subagente: devolva um resumo curto com caminhos e `arquivo:linha`, sem colar trechos grandes de código nem logs.
