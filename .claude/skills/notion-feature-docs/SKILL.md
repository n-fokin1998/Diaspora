---
name: notion-feature-docs
description: Write or refresh the Notion documentation page for a Diaspora feature once it has been implemented from a Spec Kit spec (a folder under specs/ with spec.md, plan.md, etc.). Use this whenever the user asks to "document this feature in Notion", "write up the spec for X in Notion", "add docs for a feature or branch", or says a feature is done/merged and wants it captured in Notion. Also use it if they ask to update or refresh an existing feature's Notion page after further changes.
---

# Notion feature docs (Diaspora)

Diaspora's workflow is: specs are written with Spec Kit under `specs/<NNN-feature-name>/`, an
agent implements them, the user may hand-edit the generated plan and re-apply it, and once the
feature is ready the user asks for it to be written up in Notion. This skill turns a finished
spec folder into a short, business-focused Notion page under the workspace's **Diaspora** root
page.

The reader of this page is a teammate trying to understand *what the feature does and why*, not
someone reviewing the implementation. Treat `spec.md` as the primary source — it already contains
the user stories, requirements, edge cases, and assumptions in the right voice. Pull in only as
much from `plan.md` (and `data-model.md` / `research.md` / `quickstart.md` if present) as helps
explain how the feature fits into the system technically — a few sentences, not a copy of the
plan.

## Step 1 — Find the spec folder

If the user names the folder or feature explicitly, use that. Otherwise look at `specs/` in this
repo:

- If there's exactly one folder under `specs/`, use it.
- If there are several, check which one plausibly matches what the user is talking about (most
  recently modified files, or a name matching what they said). If more than one is plausible,
  list the candidates and ask the user which one before doing anything else — don't guess on a
  multi-feature repo.

## Step 2 — Work out the page title

The title format is `<branch name> <feature name>`, e.g. `DSPR-1 Welcome Page`.

- Get the branch name with `git branch --show-current` run in the repo. This is a ticket-style
  name (e.g. `DSPR-1`) and does **not** necessarily match the spec folder's own numeric prefix
  (e.g. `001-welcome-page`) — don't try to derive one from the other, just read the actual
  current branch.
- Get the feature name from the first heading of `spec.md`, which reads
  `# Feature Specification: <Feature Name>` — use `<Feature Name>` as written (title case, no
  extra punctuation).
- Join them with a single space: `{branch} {Feature Name}`.

If the user is documenting a feature that isn't the currently checked-out branch, ask them for
the branch name instead of assuming.

## Step 3 — Read the spec material

Read from the repo:

- `spec.md` — always. This is where the user stories, acceptance scenarios, edge cases,
  functional requirements, success criteria, and assumptions live.
- `plan.md` — read the **Summary** and **Technical Context** sections for a light technical
  picture (what part of the system changed, key technologies). Skip the Constitution Check table,
  Complexity Tracking, and other process bookkeeping — that's not documentation content.
- `data-model.md`, `research.md`, `quickstart.md`, `tasks.md`, `checklists/*.md` — skim only if
  something there materially changes how you'd describe the feature's flow or its place in the
  system (e.g. a new entity, a notable decision). Most of the time `spec.md` plus a couple of
  sentences from `plan.md` is enough; don't feel obliged to mine every file.

## Step 4 — Write the page

Synthesize — don't transcribe. The spec's FR-001/FR-002-style requirement list and Given/When/Then
scenarios are written for engineers verifying behavior; rewrite them as prose a non-engineer
teammate would actually read, the way a good Confluence feature page reads. Use this structure:

1. **Overview** — one short paragraph: what the feature is, who it's for, and why it exists
   (pull this from the spec's `Input` line and its P1 user story).
2. **User Flow** — a few sentences to a short paragraph per user story, in priority order,
   describing what the user does and sees. Skip the "Independent Test" framing entirely — that's
   a testing artifact, not documentation.
3. **Edge Cases** — a short paragraph or a few bullets covering the notable edge cases from the
   spec, in plain language (what happens when X, and why it's handled that way).
4. **Technical Notes** — brief. A short paragraph naming the affected part(s) of the system and
   any technology introduced or notable constraint, drawn from `plan.md`'s Summary/Technical
   Context. This section should stay clearly secondary to the business sections above it — a few
   sentences, not a technical deep-dive.
5. **Out of Scope & Assumptions** — a short paragraph or bullets on what this iteration
   deliberately excludes, from the spec's Assumptions section and any explicitly deferred
   requirements.

Keep the whole page short — the goal is orientation, not exhaustive coverage. If a section would
just restate the previous one, cut it rather than pad it.

## Step 5 — Create or update the Notion page

1. Find the Diaspora root page (search Notion for "Diaspora" if you don't already know its page
   ID from this conversation).
2. Check whether a child page with the exact computed title already exists under it (search by
   title, or look at the root page's content). If it exists, update it in place with
   `notion-update-page` using `command: "replace_content"` — this is a refresh of the same
   feature's docs, not a new feature, so don't create a duplicate.
3. If it doesn't exist, create it with `notion-create-pages`, parented to the Diaspora root page
   (`parent: {type: "page_id", page_id: <diaspora root id>}`), with the computed title as its
   `title` property and the Step 4 content as its `content`.
4. Tell the user the page's title and URL. Don't restate the content back to them in chat — they
   can open the page.

## Notes

- This skill assumes the Notion MCP connection is active. If it isn't available, say so and ask
  the user to attach the relevant spec files instead of trying to work around it.
- Don't invent content that isn't grounded in the spec files — if something the user asked about
  (e.g. a corner case) genuinely isn't covered in the spec material, say so rather than guessing.
