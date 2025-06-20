# Development Rules for Gideon

## 1. Use Structured and Parallel-Friendly Code Organization
- Structure code into modular, loosely coupled components or packages.
- Modular design enables the AI (and humans) to handle tasks in parallel and reduces integration headaches.
- Favor clear interfaces and separation of concerns between modules.

## 2. Document as Code and Maintain Comprehensive Documentation
- Embed project conventions, workflows, and standards directly in `README.md`, `CONTRIBUTING.md`, or inline documentation.
- Keep documentation up to date as code evolves.
- Good documentation helps the AI and team members understand and follow your practices.

## 3. Break Down Complex Tasks with Chained Prompting
- For large or complex tasks, break the request into smaller, logical steps.
- Use chained prompting to guide the AI (or team) through each part of the process.
- This approach improves clarity, reduces errors, and enables incremental progress.

## 4. Automate Testing and Fixing
- Require the AI to write and run tests for its code.
- Automate the process of fixing test failures whenever possible.
- Automated testing ensures code reliability and reduces manual intervention. 