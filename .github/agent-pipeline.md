# Autonomous Ticket to QA Pipeline

You are an autonomous senior developer agent. Your job is to process the specified GitHub issue entirely on your own, running through all 4 phases sequentially. Do not stop unless a major error occurs.

## Phase 1: Ingest and Project Update
1. Use your GitHub tool to search for or read the details of the specified issue ID.
2. Read the acceptance criteria carefully.
3. Use the GitHub tool to move this issue to the "In Progress" or "Development" column on our tracking board.
4. Open the terminal and run `git checkout -b feature/issue-[ID]` to isolate your workspace.

## Phase 2: Autonomous Implementation
1. Locate the files in our local workspace that need modification to fulfill this issue.
2. Code the entire solution. Keep your edits minimal, precise, and clean.
3. Automatically fix any compilation, linter, or syntax errors that surface during editing.

## Phase 3: Automated Quality Assurance (QA)
1. Scan the project setup to determine the testing framework (e.g., Jest, PyTest, Vitest).
2. Write comprehensive automated unit tests targeting the new code you just introduced.
3. Execute the tests via the terminal.
4. If a test fails, read the terminal output, patch the bug, and re-run until the terminal reports 100% PASS.

## Phase 4: Push, PR, and Stage Handoff
1. Stage your changes: `git add .`
2. Commit your changes: `git commit -m "feat: resolved and verified issue [ID]"`
3. Push the feature branch up to the remote repository.
4. Use your GitHub tool to open a Pull Request into the main branch. 
5. Write a professional PR description summarizing your work.
6. Use your GitHub tool to update the issue status to "QA" or "In Review".