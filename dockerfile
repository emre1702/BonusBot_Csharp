ARG DOTNET_VER_SDK=5.0.102-focal
ARG DOTNET_VER_ASPNET=5.0.2-focal
ARG OPERATING_SYSTEM=ubuntu.20.04-x64
ARG OPERATING_SYSTEM_IMAGE=20.04
ARG CERTIFICATE_PASSWORD="tdsv"

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VER_SDK} AS build-env
ARG OPERATING_SYSTEM
ARG CERTIFICATE_PASSWORD
WORKDIR /bonusbot-source

COPY . .

RUN [ ! -d /bonusbot-data/TDSConnectorServer.pfx ] \
    && dotnet dev-certs https --clean \
    && dotnet dev-certs https -ep /bonusbot-data/TDSConnectorServer.pfx -p ${CERTIFICATE_PASSWORD} \
    ; exit 0

RUN dotnet publish -o build -c Debug -r ${OPERATING_SYSTEM}

#FROM ubuntu:${OPERATING_SYSTEM_IMAGE} AS release
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VER_ASPNET} AS release

# Install dependencies
RUN apt-get update && apt-get install -y \
    libc6-dev \
	libunwind8 \
    libssl1.1 \
	&& rm -rf /var/lib/apt/lists/*

# Add bonusbot user
RUN useradd -m -d /home/bonusbot bonusbot

WORKDIR /home/bonusbot

COPY --from=build-env /bonusbot-source/build .

RUN [ ! -d /bonusbot-data ] && mkdir -p /bonusbot-data; exit 0

ADD ./entrypoint.sh ./entrypoint.sh
EXPOSE 443/tcp 3000 5000

RUN chown -R bonusbot:bonusbot . \
    && chown -R bonusbot:bonusbot /bonusbot-data \
    && chmod +x Core \
    && chmod +x entrypoint.sh

RUN rm -rf /bonusbot-source

USER bonusbot
CMD ./entrypoint.sh
ENTRYPOINT ["/bin/sh", "./entrypoint.sh"]