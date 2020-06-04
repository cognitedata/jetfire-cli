# jetfire-cli

Command-line interface for Jetfire/Transformations.


## Development

`jetfire-cli` is built on .NET Core 3.1, and uses GitHub Actions/Workflows for CI/CD.

### Prerequisites

1. Install the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)
2. Run `dotnet restore` to restore NuGet packages

### Running tests

To run tests, you must set the `JETFIRE_CLI_TEST_API_KEY` environment variable to a valid API key for a service account that has access to Jetfire/Transformations.

If you want to run tests against other clusters, set the `JETFIRE_CLI_TEST_CLUSTER` environment variable.
Note that this should be set to the name of the cluster (e.g. `greenfield`), not the full URL.

### Release a new version

To make a new release, push a tag named `release/<version>`, where `<version>` is a valid .NET version string, like `1.2.3` or `1.0.0-alpha-5`.
This is picked up by a GitHub workflow, which will automatically build and publish your release.
