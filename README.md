# rebel-ot-discover-edgeapp

[![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=rebel-it_rebel-ot-discover-edgeapp)

A .NET edge application that automatically discovers OPC UA data nodes and provisions them as Variables and Tags in the IXON Cloud platform. It is designed to run as a Docker container on an IXON SecureEdge Pro device.

## What it does
On a 15-minute cycle the application:

1. Connects to a configured OPC UA server and recursively browses its full address space.
2. For every data node found, creates a corresponding **Variable** (data point definition) and **Tag** (logging configuration) in the IXON Cloud via the IXON API v2.
3. Skips nodes that already exist in IXON, so repeated runs are safe and idempotent.

The result is that all OPC UA variables exposed by connected industrial equipment become immediately available for logging, alarming, and visualisation in the IXON portal — without any manual configuration.

## Documentation

Architecture and design documentation is located in the [`docs/`](docs/) folder and follows the [arc42](https://arc42.org/) template.

## Contributing

- **Feature requests** — open an issue using the [User Story](.github/ISSUE_TEMPLATE/user_story.md) template.
- **Bug reports** — open an issue using the [Bug Report](.github/ISSUE_TEMPLATE/bug_report.md) template.

## Getting started

For a full step-by-step deployment guide, see [getting_started.adoc](getting_started.adoc).



