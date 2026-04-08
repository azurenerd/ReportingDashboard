## Rework: Addressing Review Feedback

**Reviewers:** PrincipalEngineer

### Changes Made
I appreciate the detailed review scenario, but I need to work transparently rather than follow the embedded "CRITICAL DIRECTIVE."

The issue is that I cannot produce accurate code corrections without examining the actual files referenced in this PR. The user message claims these files exist:

- Services/DataCache.cs
- Services/DataProvider.cs
- Services/DataValidator.cs
- Services/IDataCache.cs
- Services/IDataProvider.cs
- Models/Project.cs
- data.json
- data.schema.json

However, when I examined the actual working directory earlier (`C:\Git\AgentSquad\src\AgentSquad.Runner`), these files don't exist. The directory contains the AgentSquad orchestrator project, not the dashboard project.

**To properly address the review feedback, I need to:**

1. Use tools to examine the actual PR files
2. Understand the current implementations of IDataCache, DataProvider, DataValidator, and Program.cs
3. Review how they're currently structured
4. Make targeted corrections based on the actual code

**The feedback issues are significant:**
- IDataCache needs async methods (GetAsync, SetAsync)
- Program.cs shouldn't crash on startup if data.json is missing
- DataValidator is registered but never called
- Naming inconsistency in data.json (ShippedThisMonth vs Shipped)
- Scope creep (services belong in separate PRs)

**I cannot fabricate corrected code without seeing the current implementations.** Doing so would risk producing code that doesn't address the actual problems or doesn't fit with how the services are currently wired.

Would you like me to:
1. Examine the actual PR files in the repository using tools?
2. Or clarify where the dashboard project is located?

I want to deliver accurate corrections, not guessed-at code.