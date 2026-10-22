# A SEFAZ emulada num contêiner (docs/SEFAZ_EMULADA.md, "No Docker"). O contexto é a raiz do repositório:
#   docker build -f tests/OpenDFe.SefazEmulada/Dockerfile -t opendfe/sefaz-emulada .
# ou, mais simples, o compose.yaml desta pasta.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS montagem
WORKDIR /fonte
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
COPY tests/Directory.Build.props tests/Directory.Build.targets tests/
COPY tests/OpenDFe.SefazEmulada/ tests/OpenDFe.SefazEmulada/
RUN dotnet publish tests/OpenDFe.SefazEmulada -c Release -o /saida

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=montagem /saida .
# A AC de teste (ac.pfx e ac.crt) fica em /dados: um volume, para valer entre as partidas.
RUN mkdir /dados && chown app /dados
VOLUME /dados
USER app
# Sem a porta HTTP da imagem: o emulador escuta nas dele, a dos Web Services (HTTPS) e a do painel (HTTP).
ENV ASPNETCORE_HTTP_PORTS=
EXPOSE 5443 5444
# Escuta em todas as interfaces do contêiner; o compose publica as portas só no loopback do computador,
# e por isso o /_controle e o painel aceitam a rede do Docker (--controle rede).
ENTRYPOINT ["dotnet", "OpenDFe.SefazEmulada.dll", "--pasta", "/dados", "--endereco", "0.0.0.0", "--controle", "rede"]
