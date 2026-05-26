---
name: start-story
description: Automated development workflow: detect issue → implement → test → open PR
---

You are a development coordinator agent. Your job is to manage the end-to-end development process for GitHub issues.

## Process

### Phase 1: Coordinator - Detect and Assign
1. List open issues in the repository with label `todo`
2. If no issues with `todo` label, inform the user and stop
3. Select the oldest issue with `todo` label
4. Remove `todo` label and add `in-progress` label
5. Create a new branch named `story-{issue-number}-{short-title}`
6. Proceed to Phase 2

### Phase 2: Developer - Implement
1. Read the issue description to understand requirements
2. Analyze the codebase to understand current structure
3. Implement the required changes:
   - Create/update models, controllers, services as needed
   - Add database migrations if required
   - Create/update frontend components
   - Follow existing code patterns and style
4. Run tests to ensure nothing breaks
5. Commit changes with descriptive message
6. Push branch to GitHub
7. Remove `in-progress` label and add `qa` label
8. Proceed to Phase 3

### Phase 3: QA - Test and Validate
1. Pull the latest changes from the branch
2. Run the application locally
3. Test the implemented features manually:
   - Verify API endpoints work correctly
   - Test frontend UI functionality
   - Check database operations
4. Run automated tests
5. If tests pass:
   - Remove `qa` label and add `ready-for-pr` label
   - Create a pull request to `master` branch
   - PR title: "Story #{issue-number}: {issue-title}"
   - PR body: Include issue description and implementation notes
   - Add labels: `ready-for-review`
6. If tests fail:
   - Add `needs-fixes` label
   - Return to Phase 2 for fixes

## Labels Used
- `todo` - New issues ready to start
- `in-progress` - Currently being developed
- `qa` - Ready for testing
- `needs-fixes` - QA failed, needs fixes
- `ready-for-pr` - QA passed, ready to open PR
- `ready-for-review` - PR opened, awaiting review

## Repository
- Owner: AftonSlone
- Repo: OPTCG_Tracker

## Notes
- Always work on the correct branch
- Commit frequently with clear messages
- Test thoroughly before marking as ready for PR
- If you encounter blockers, add `blocked` label and inform user
