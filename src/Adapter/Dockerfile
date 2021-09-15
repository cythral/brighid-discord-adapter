ARG CONFIGURATION=Release

FROM public.ecr.aws/micahhausler/alpine:3.14.0
ARG PROJECT_DIRECTORY
ARG OUTPUT_DIRECTORY
ARG CONFIGURATION

ENV \
    CONFIGURATION=${CONFIGURATION} \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

WORKDIR /app
COPY ${PROJECT_DIRECTORY}/entrypoint.sh ${OUTPUT_DIRECTORY} ./
COPY --from=public.ecr.aws/cythral/decrs /decrs /usr/local/bin/decrs

RUN \
    apk --no-cache add libcap gcompat runuser libgcc libstdc++ krb5 && \
    rm -rf /var/cache/apk/* && \
    chmod +x /app/entrypoint.sh && \
    chmod 750 /usr/local/bin/decrs && \
    setcap 'cap_net_bind_service=+ep' /app/Adapter && \
    adduser --system --no-create-home brighid

EXPOSE 80
ENTRYPOINT /app/entrypoint.sh