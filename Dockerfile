ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.4.0.61
ARG CONFIGURATION

ENV CONFIGURATION=${CONFIGURATION}
WORKDIR /app
COPY ./entrypoint.sh /
COPY ./bin/Adapter/${CONFIGURATION}/linux-musl-arm64/publish ./

RUN setcap 'cap_net_bind_service=+ep' /app/Adapter

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=1s --retries=3 \
    CMD curl --fail --http2-prior-knowledge http://localhost/healthcheck || exit 1

ENTRYPOINT [ "/entrypoint.sh" ]
