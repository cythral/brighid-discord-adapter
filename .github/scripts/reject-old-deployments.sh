#!/bin/bash
set -eo pipefail

repo=$1
escaped_repo=$(echo $repo | xargs | sed 's/\//\\\//')

environment_id=$(gh api repos/$repo/environments --jq '[.environments[] | select(.name == "Production")][0].id')
deployment_id=$(gh api repos/$repo/deployments --jq '[.[] | select(.environment == "Production")][1].id')
target_url=$(gh api repos/$repo/deployments/$deployment_id/statuses --jq '.[0].target_url')
job_id=$(echo $target_url | sed -E "s/https:\/\/github.com\/${escaped_repo}\/runs\/([0-9]+).*/\1/")
run_id=$(gh api repos/$repo/actions/jobs/$job_id --jq '.run_id')

function has_pending_deployment() {
    set +eo pipefail
    count=$(gh api repos/$repo/actions/runs/$run_id/pending_deployments --jq ". | length")
    local result=$?
    set -eo pipefail
    
    if [ "$result" = "0" ] && [ "$count" != "0" ]; then
        echo "true"
        return
    fi

    echo "false"
}

if [ "$(has_pending_deployment)" = "true" ]; then
    gh api \
        --method POST \
        repos/$repo/actions/runs/$run_id/pending_deployments \
        --input - <<<$(echo "{\"state\":\"rejected\",\"environment_ids\":[$environment_id],\"comment\":\"superseded\"}")
fi
