---
name: Issue Template
about: Template for Developers
title: Feature Card
labels: ''
assignees: ''

---

## Description
<!-- Provide a detailed description of the issue or feature request. Include relevant information, context, and screenshots if applicable. -->

## Checklist
- [ ] Task 1: Describe the first task to complete for resolving this issue.
- [ ] Task 2: Describe the second task.
- [ ] Task 3: Add more tasks as needed.

## Workflow (for new developers)
### 1. Create a New Branch
To work on this issue, first create a new branch from the main branch.

1. Make sure you are on the main branch:
   ```bash
   git checkout main
   ```

2. Pull the latest changes:
   ```bash
   git pull origin main
   ```

3. Create a new branch named after the issue:
   ```bash
   git checkout -b issue-<issue-number>-<short-description>
   ```
   Replace `<issue-number>` with the GitHub issue number and `<short-description>` with a brief description of the issue.

### 2. Work on the Issue
1. Make changes to your local repository.
2. Stage your changes:
   ```bash
   git add .
   ```

3. Commit your changes with a meaningful message:
   ```bash
   git commit -m "Fixes <issue-number>: <brief-description>"
   ```

4. Push the changes to the remote repository:
   ```bash
   git push origin issue-<issue-number>-<short-description>
   ```

### 3. Create a Pull Request
1. Go to your repository on GitHub.
2. You should see a prompt to create a pull request for your new branch. Click on it.
3. Make sure the base branch is `main` and the compare branch is your feature branch.
4. Provide a descriptive title and include `Fixes #<issue-number>` in the description to link the issue.
5. Click **Create pull request**.

### 4. Merge the Pull Request
1. Wait for review and approval.
2. Once approved, merge the pull request into the main branch.
3. After merging, you can delete the branch on Github, local and then verify the deletion:
   ```bash
   git push origin --delete issue-<issue-number>-<short-description>
   git branch -d issue-<issue-number>-<short-description>
   git branch
   ```
4. Pull the latest changes to update your local repository:
   ```bash
   git checkout main
   git pull origin main
   ```

### Notes
- Ensure all tasks in the checklist are completed before creating a pull request.
- Follow the project's coding standards and guidelines.
