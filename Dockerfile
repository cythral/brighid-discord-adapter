ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.2.1
ARG CONFIGURATION

ENV CONFIGURATION=${CONFIGURATION}
WORKDIR /app
COPY ./entrypoint.sh /
COPY ./bin/Adapter/${CONFIGURATION}/linux-musl-arm64/publish ./

RUN setcap 'cap_net_bind_service=+ep' /app/Adapter

EXPOSE 80
ENTRYPOINT [ "/entrypoint.sh" ]