ARG CONFIGURATION=Release

FROM public.ecr.aws/cythral/brighid/base:0.1.0
ARG PROJECT_DIRECTORY
ARG OUTPUT_DIRECTORY
ARG CONFIGURATION

ENV \
    CONFIGURATION=${CONFIGURATION} \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

WORKDIR /app
COPY ${PROJECT_DIRECTORY}/entrypoint.sh ${OUTPUT_DIRECTORY} ./

RUN chmod +x /app/entrypoint.sh && \
    setcap 'cap_net_bind_service=+ep' /app/Adapter

EXPOSE 80
ENTRYPOINT /app/entrypoint.sh