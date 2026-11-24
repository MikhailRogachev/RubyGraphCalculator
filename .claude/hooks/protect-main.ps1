$data = [Console]::In.ReadToEnd() | ConvertFrom-Json
$cmd = $data.tool_input.command

if ($cmd -match '\bgit\b.*\b(commit|push)\b') {
    $branch = git branch --show-current
    if ($branch -eq 'main') {
        [Console]::Error.WriteLine(
            "Commits and pushes to main are not allowed by the project rules. " +
            "Create a branch (git checkout -b <name>), commit there, " +
            "and open a pull request with gh pr create.")
        exit 2
    }
}
exit 0