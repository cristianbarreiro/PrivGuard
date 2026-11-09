# Security Policy

## Supported Versions

PrivGuard aims to address security vulnerabilities affecting actively maintained versions of the project.

Until an explicit release support policy is established, security fixes will be evaluated based on the affected version, the severity of the issue, and the feasibility of applying a fix.

| Version | Supported |
| ------- | --------- |
| Development branch | Best effort |
| Released versions | Evaluated on a case-by-case basis |
| End-of-life versions | No guaranteed security updates |

A supported release does not imply that every security issue has been eliminated. Users should keep PrivGuard updated to the latest available release.

## Reporting a Vulnerability

**Please do not disclose suspected security vulnerabilities in public issues before a fix or coordinated disclosure has been considered.**

If you discover a potential security vulnerability in PrivGuard, please report it privately through GitHub's [private vulnerability reporting](https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-managing-vulnerabilities) feature, if enabled for this repository.

If private vulnerability reporting is unavailable, contact the project maintainer through an established private communication channel. Do not include sensitive information in public discussions.

### What to Include

Please provide as much of the following information as possible:

- A clear description of the vulnerability and its potential impact.
- The affected version, commit, or branch.
- The operating system and relevant environment details.
- Reproduction steps or a proof of concept, where safe to provide.
- Any suggested mitigation or remediation, if known.

Please avoid sharing real credentials, personal data, or other sensitive information in vulnerability reports.

### What to Expect

- Reports will be reviewed as soon as reasonably possible.
- The maintainer may request additional information to reproduce or assess the issue.
- If the vulnerability is confirmed, its severity, impact, and remediation options will be evaluated.
- If the report is not considered a vulnerability, the reporter will be informed when reasonably possible, along with an explanation where appropriate.
- Fixes and security advisories may be published when appropriate, taking into account the impact on users and the risks of disclosure.

No fixed response or remediation timeframe is guaranteed at this stage.

### Coordinated Disclosure

Please allow the maintainer a reasonable opportunity to investigate and address a confirmed vulnerability before publicly disclosing technical details.

Disclosure timing should take into account the severity of the issue, the availability of a fix, and the potential risk to users. We appreciate responsible reporting and cooperation throughout the process.

## Scope

Security reports may concern, among other things:

- Unauthorized access to protected resources.
- Failures in privacy protection or enforcement mechanisms.
- Incorrect restoration or persistence of protection state.
- Privilege escalation or unintended system modifications.
- Exposure of sensitive information, credentials, or tokens.
- Security weaknesses in authentication, integrations, or data storage.
- Vulnerabilities in dependencies that materially affect PrivGuard.

Reports should demonstrate a plausible security impact on PrivGuard or its users.

## Out of Scope

The following are generally not considered security vulnerabilities on their own:

- Issues without a demonstrable security impact.
- Theoretical weaknesses without a realistic attack scenario.
- Vulnerabilities in unsupported third-party software that do not affect PrivGuard.
- General feature requests or non-security bugs.

This list is not exhaustive. Each report will be evaluated on its merits.

---

Thank you for helping improve PrivGuard's security and protect its users.
