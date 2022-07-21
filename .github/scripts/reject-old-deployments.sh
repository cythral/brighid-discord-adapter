#!/bin/bash
set -eo pipefail

repo=$1
escaped_repo=$(echo $repo | xargs | sed 's/\//\\\//')

environment_id=$(gh api repos/$repo/environments --jq '[.environments[] | select(.name == "Production")][0].id')
deployment_id=$(gh api repos/$repo/deployments --jq '[.[] | select(.environment == "Production")][1].id')
target_url=$(gh api repos/$repo/deployments/$deployment_id/statuses --jq '.[0].target_url')
job_id=$(echo $target_url | sed -E "s/https:\/\/github.com\/${escaped_repo}\/runs\/([0-9]+).*/\1/")
run_id=$(gh api repos/$repo/actions/jobs/$job_id --jq '.run_id')

if gh api repos/$repo/actions/runs/$run_id/pending_deployments >/dev/null 2>&1; then
    gh api \
        --method POST \
        repos/$repo/actions/runs/$run_id/pending_deployments \
        --input - <<<$(echo "{\"state\":\"rejected\",\"environment_ids\":[$environment_id],\"comment\":\"superseded\"}")
fi
