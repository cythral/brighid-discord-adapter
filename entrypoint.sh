#!/bin/sh

set -eo pipefail

curl "$ECS_CONTAINER_METADATA_URI_V4/task"

if [ "$Environment" != "local" ]; then
    export Adapter__ClientSecret=$(decrs ${Encrypted__Adapter__ClientSecret}) || exit 1;
    export Adapter__Token=$(decrs ${Encrypted__Adapter__Token}) || exit 1;
    export Identity__ClientSecret=$(decrs ${Encrypted__Identity__ClientSecret}) || exit 1;
    export Database__Password=$(decrs ${Encrypted__Database__Password}) || exit 1;

    runuser --user brighid /app/Adapter
    exit $?
fi

watch /app "runuser --user brighid /app/Adapter"
