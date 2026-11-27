# Imagen del emulador de la impresora térmica 58HB6 (PAR-PTE-05; ADR-0006).
#
# Uso previsto (aprobado por INFRA): en el equipo del emulador de Android, unida a la red del contenedor del emulador,
# dentro del mismo turno y puesto de emulador, y se baja junto con él (nunca queda viva sola):
#
#   docker run -d --rm --name <nombre> --network container:<contenedor del emulador Android> \
#     --cpus 1 --memory 512m -v <carpeta de salida>:/data \
#     -e Emulador__Bluetooth__Netsim=http://127.0.0.1:<puerto gRPC de netsim> \
#     -e Emulador__Bluetooth__Direccion=<dirección fija, p. ej. 04:00:00:00:00:01> \
#     ghcr.io/hdcm-dev/printthermal_motor_maui-emulator:<versión>
#
# netsim escucha sólo en el loopback del contenedor del emulador; con la red compartida se llega por 127.0.0.1 y no
# hace falta exponerlo. El puerto gRPC está en /tmp/netsim.ini dentro de ese contenedor (grpc.port).
# La API, Scalar y el visor quedan en el puerto 5080 de esa red; la imagen no publica puertos.
# La carpeta de salida tiene que admitir escritura del usuario sin privilegios de la imagen (uid 1654).

ARG VERSION=0.0.0-local

FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble@sha256:212fe4cb74627d5adf9817a4088906ae3edff1bd853bb27565d4b46731efe61e AS compilacion
ARG VERSION
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/PrintThermal.Emulator.Api/PrintThermal.Emulator.Api.csproj -c Release -o /app \
      -p:UseAppHost=false -p:Version=${VERSION} \
    && mkdir -p /data && chown 1654:1654 /data

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-noble-chiseled@sha256:f2557fb81b1cfcfdfcde9d3bf247ed60b2cd38f00a88153f61e34737fab8ade7
ARG VERSION
LABEL org.opencontainers.image.source=https://github.com/hdcm-dev/PrintThermal_Motor_Maui-Emulator
LABEL org.opencontainers.image.description="Emulador de la impresora térmica 58HB6 (Bluetooth Classic sobre netsim, ESC/POS a PNG, API y visor)"
LABEL org.opencontainers.image.version=${VERSION}
WORKDIR /app
COPY --from=compilacion /app ./
COPY --from=compilacion --chown=1654:1654 /data /data
ENV ASPNETCORE_HTTP_PORTS=
ENV ASPNETCORE_URLS=http://0.0.0.0:5080
ENV Emulador__Salida=/data
VOLUME ["/data"]
EXPOSE 5080
USER app
ENTRYPOINT ["dotnet", "PrintThermal.Emulator.Api.dll"]
