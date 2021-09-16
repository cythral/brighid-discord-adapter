ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.1.9
ARG PROJECT_DIRECTORY
ARG OUTPUT_DIRECTORY
ARG CONFIGURATION

ENV CONFIGURATION=${CONFIGURATION}
WORKDIR /app
COPY ${PROJECT_DIRECTORY}/entrypoint.sh /
COPY ${OUTPUT_DIRECTORY} ./

RUN chmod +x /entrypoint.sh && \
    setcap 'cap_net_bind_service=+ep' /app/Adapter

EXPOSE 80
ENTRYPOINT [ "/entrypoint.sh" ]