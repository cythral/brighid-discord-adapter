ARG CONFIGURATION=Release
ARG AWSCLI_VERSION=2.2.1
ARG ASPNET_VERSION
ARG ASSEMBLY_NAME

FROM public.ecr.aws/cythral/unzip AS awscli
ARG AWSCLI_VERSION
WORKDIR /aws
ADD https://awscli.amazonaws.com/awscli-exe-linux-x86_64-${AWSCLI_VERSION}.zip ./awscli.zip
RUN unzip awscli.zip && \
    rm awscli.zip

FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION}
ARG PROJECT_DIRECTORY
ARG OUTPUT_DIRECTORY
ARG CONFIGURATION
ARG ASSEMBLY_NAME

ENV \
    CONFIGURATION=${CONFIGURATION} \
    DLL_PATH=/app/${ASSEMBLY_NAME}.dll

WORKDIR /app
COPY ${PROJECT_DIRECTORY}/entrypoint.sh /
COPY ${OUTPUT_DIRECTORY} .
COPY --from=awscli /aws /usr/local/share/aws

RUN \
    /usr/local/share/aws/aws/install && \
    apt-get update && apt-get install -y libcap2-bin && \
    rm -rf /var/lib/apt/lists/* && \
    setcap 'cap_net_bind_service=+ep' /usr/share/dotnet/dotnet && \
    useradd --no-create-home brighid && \
    usermod --shell /bin/false brighid && \
    usermod --lock brighid && \
    chmod +x /entrypoint.sh

EXPOSE 80
ENTRYPOINT /entrypoint.sh