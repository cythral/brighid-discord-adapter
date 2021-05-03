ARG DOTNET_SDK_VERSION
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS development
ENV \
    CONFIGURATION=Release \
    DLL_PATH=/app/bin/GatewayAdapter/Release/net5.0/publish/GatewayAdapter.dll

WORKDIR /app
COPY . .

RUN if [ ! -f ${DLL_PATH} ]; then dotnet publish; fi

EXPOSE 80
ENTRYPOINT ["dotnet", "watch", "--project", "/app/src/Core", "run"]

FROM mcr.microsoft.com/dotnet/aspnet:5.0.4 AS production

WORKDIR /app
COPY --from=development /app/bin/Core/Release/net5.0/publish /app
COPY entrypoint.sh /

RUN  \
    apt-get update && \
    apt-get install -y python3 python3-pip && \
    pip3 install awscli && \
    chmod +x /entrypoint.sh

EXPOSE 80
ENTRYPOINT /entrypoint.sh

