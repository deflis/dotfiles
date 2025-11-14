# Post-Task Steps
- Run `chezmoi diff` to confirm only intentional template/config changes.
- Optionally `chezmoi apply --dry-run --verbose` to ensure templates render without errors before a full apply.
- Remember autoAdd/autoCommit is enabled; applying may automatically commit—coordinate with repo workflow before running `chezmoi apply`.
- If changes touch run-onchange scripts, delete `scriptState` bucket to force re-run when appropriate.