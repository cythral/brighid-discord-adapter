#!/bin/sh

set -eo pipefail

if [ "$Environment" != "local" ]; then
    export Adapter__ClientSecret=$(decrs ${Encrypted__Adapter__ClientSecret});
    export Adapter__Token=$(decrs ${Encrypted__Adapter__Token});
    export Identity__ClientSecret=$(decrs ${Encrypted__Identity__ClientSecret});
    export Database__Password=$(decrs ${Encrypted__Database__Password});

    runuser --user brighid /app/Adapter
    exit $?
fi

watch /app "runuser --user brighid /app/Adapter"
