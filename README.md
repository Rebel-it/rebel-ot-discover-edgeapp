# OPC UA Edge Sync
A web-based tool that discovers OPC UA data nodes and provisions them as Variables and Tags in the IXON Cloud platform. It runs as two Docker containers on an IXON SecureEdge Pro device and is operated through a wizard UI in your browser.

## What it does

The application consists of a **React frontend** (wizard UI) and an **ASP.NET Core backend** (API). Together they guide an OT engineer through:

1. Authenticating with IXON Cloud using a service account (Application ID + Access Token).
2. Auto-detecting the IXON company and matching the SecureEdge Pro to its IXON agent.
3. Connecting to a local OPC UA server and verifying the connection.
4. Selecting or creating an OPC UA data source on the IXON agent.
5. Running a **variable discovery** — the backend recursively browses the OPC UA address space and registers every data node as a Variable in IXON Cloud. Nodes that already exist are skipped.
6. Reviewing discovered variables and creating **Tags** (logging configuration) for the ones you want to monitor.

The result is that all OPC UA variables exposed by connected industrial equipment become immediately available for logging, alarming, and visualisation in the IXON portal — without any manual configuration.

## Documentation

Architecture and design documentation is located in the [`docs/`](docs/) folder and follows the [arc42](https://arc42.org/) template.

## Contributing

- **Feature requests** — open an issue using the [User Story](.github/ISSUE_TEMPLATE/user_story.md) template.
- **Bug reports** — open an issue using the [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) template.

## Getting started

For a full step-by-step deployment guide, see [getting_started.adoc](getting_started.adoc).



