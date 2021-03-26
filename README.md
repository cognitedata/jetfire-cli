# jetfire-cli

Command-line interface for Jetfire/Transformations.


## Changes between v1 and v2

v2 introduces the possibility to configure email notifications via transformation manifests.
This means that if you run `jetfire deploy` on your old v1 manifests, version 2 will delete all the notification destinations you have configured in the web app, unless you include a `notifications` section in the manifest file like so:

``` yaml
notifications:
  - alice@gmail.com
  - bob@outlook.com
```


## Usage

### Authenticate

To use `jetfire-cli`, the `JETFIRE_API_KEY` environment variable must be set to a valid API key for a service account which has access to Jetfire/Transformations.

By default, `jetfire-cli` runs against the main CDF cluster (europe-west1-1).
To use a different cluster, specify the `--cluster` parameter. Note that this is a global parameter, which must be specified before the subcommand.
For example:

```
jetfire --cluster=greenfield <subcommand> [...args]
```

### Deploy transformations

The primary purpose of `jetfire-cli` is to support continuous delivery, allowing you to manage transformations in a version control system.

Transformations are described by YAML files, whose structure is described further below in this document.

It is recommended to place these manifest files in their own directory, to avoid conflicts with other files.

#### From the command line

To deploy a set of transformations, use the `deploy` subcommand:

```
jetfire deploy <path>
```

The `<path>` argument should point to a directory containing YAML manifests.
This directory is scanned recursively for `*.yml` and `*.yaml` files, so you can organize your transformations into separate subdirectories.

#### From a GitHub Workflow

`jetfire-cli` also provides a GitHub Action, which can be used to deploy transformations.
To deploy a set of transformations in a GitHub workflow, add a step which references the action in your job:

```yaml
- name: Deploy transformations
  uses: cognitedata/jetfire-cli@v2
  with:
    path: transformations
    api-key: ${{ secrets.JETFIRE_API_KEY }}
    # If not using the main europe-west1-1 cluster:
    # cluster: greenfield
  env:
    SOME_API_KEY: ${{ secrets.SOME_API_KEY }}
```

This GitHub action takes the following inputs:

| Name      | Description |
|-----------|-------------|
| `path`    | _(Required)_ The path to a directory containing transformation manifests. This is relative to `$GITHUB_WORKSPACE`, which will be the root of the repository when using [actions/checkout](https://github.com/actions/checkout) with default settings. |
| `api-key` | _(Required)_ The API key used for authenticating with transformations. Equivalent to setting the `JETFIRE_API_KEY` environment variable. |
| `cluster` | _(Optional)_ The name of the cluster where Jetfire/Transformations is hosted. |

Additionally, you must specify environment variables for any API keys referenced in transformation manifests.


See [workflow.example.yml](workflow.example.yml) for a complete example.

#### Manifest file format

```yaml
# Required
externalId: my-transformation


# Required
name: My Transformation


# Optional, default: false
shared: true


# Optional, default: null
# If null, the transformation will not be scheduled.
schedule: "1 * * * *"


# Required
# The path to a file containing the SQL query for this transformation.
query: my-transformation.sql


# Required
# Valid values are: assets, assethierarchy, events, timeseries, datapoints, stringdatapoints
destination: datapoints
# When writing to RAW tables, use the following syntax:
destination:
  type: raw
  rawDatabase: some_database
  rawTable: some_table


# Optional, default: upsert
# Valid values are:
#   upsert: Create new items, or update existing items if their id or externalId already exists.
#   create: Create new items. The transformation will fail if there are id or externalId conflicts.
#   update: Update existing items. The transformation will fail if an id or externalId does not exist.
#   delete: Delete items by internal id.
action: update


# Optional, default: null
# List of email adresses to send emails to on transformation errors
notifications:
  - my.email@provider.com
  ...


# Either this or authentication is required
# The API key that will be used to run the transformation,
# specified as the name of an environment variable.
apiKey: SOME_ENVIRONMENT_VARIABLE
# If you want to use separate API keys for reading/writing data, use the following syntax:
apiKey:
  read: READ_API_KEY
  write: WRITE_API_KEY


# Either this or apiKey is required
# The client credentials to be used in the transformation
authentication:
  tokenUrl: "https://my-idp.com/oauth2/token"
  scopes:
    - https://bluefield.cognitedata.com/.default
  cdfProjectName: my-project
  # The following two is given as the name of an environment variable
  clientId: COGNITE_CLIENT_ID
  clientSecret: COGNITE_CLIENT_SECRET
# Alternatively, if you want to use separate credentials for read and write:
authentication:
  read:
    tokenUrl: "https://my-idp.com/oauth2/token"
    scopes:
      - https://bluefield.cognitedata.com/.default
    cdfProjectName: my-project
    # The following two is given as the name of an environment variable
    clientId: COGNITE_CLIENT_ID
    clientSecret: COGNITE_CLIENT_SECRET
  write:
    tokenUrl: "https://my-idp.com/oauth2/token"
    scopes:
      - https://bluefield.cognitedata.com/.default
    cdfProjectName: another-project
    # The following two is given as the name of an environment variable
    clientId: COGNITE_CLIENT_ID
    clientSecret: COGNITE_CLIENT_SECRET
```

### Start a transformation job

`jetfire-cli` also provides a `run` subcommand, which can start transformation jobs and/or wait for jobs to complete.

At minimum, this command requires either an `--id` or `--external-id` to be specified:

```sh
jetfire run --id=1234
jetfire run --external-id=my-transformation
```

Without any additional arguments, this command will start a transformation job, and exit immediately.
If you want wait for the job to complete, use the `--watch` option:

```sh
jetfire run --id=1234 --watch
```

When using the `--watch` option, `jetfire-cli` will return a non-zero exit code if the transformation job failed, or if it did not finish within a given timeout (which is 12 hours by default). This timeout can be configured using the `--timeout` option.

If you want to watch a job for completion without actually starting a transformation job, specify `--watch-only` instead of `--watch`. This will watch the most recently started job for completion.

See `jetfire run --help` for more details.


### Make a query

`jetfire-cli` also allows you to run queries, and retrieve up to 1000 rows of results.

```sh
jetfire query "select * from _cdf.assets limit 100"
```

This will print a JSON document to standard output containing the results of your query.

#### Query limits

The query command is intended for previewing your SQL queries, and is not designed for large data exports.
For this reason, there are a few limits in place.

First of all, there is a final limit of 1000 rows that can be returned from your query.
This is a limitation in the backend itself, although this may change in the future.
This is equivalent to a final LIMIT clause on your query - aggregations and joins may still process more than 1000 rows.

There is also a _source limit_, which is configurable using the `--source-limit` parameter.
This limits the number of rows to read from _each data source_.
For example, if the source limit is 100, and you take the `UNION` of two tables, you will get 200 rows back.
This parameter is set to 100 by default, but you can remove this limit by setting it to a value that's less than 1, for example:

```sh
jetfire query --source-limit=0 "select * from db.table"
```

Finally, there is the _schema inference limit_, which is configurable using the `--infer-schema-limit` parameter.
Because RAW tables have no predefined schema, we need to read some number of rows to infer the schema of the table.
As with the source limit, this is set to 100 by default, and can be made unlimited by setting it to a value that's less than 1.
If your RAW data is not properly being split into separate columns, you should try to increase or remove this limit, for example:

```sh
jetfire query --infer-schema-limit=0 "select * from db.table"
```

## Development

`jetfire-cli` is built on .NET Core 3.1, and uses GitHub Actions/Workflows for CI/CD.

### Prerequisites

1. Install the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download)
2. Run `dotnet restore` to restore NuGet packages

### Running tests

To run tests, you must set the `JETFIRE_CLI_TEST_API_KEY` environment variable to a valid API key for a service account that has access to Jetfire/Transformations.

If you want to run tests against other clusters, set the `JETFIRE_CLI_TEST_CLUSTER` environment variable.
Note that this should be set to the name of the cluster (e.g. `greenfield`), not the full URL.

To run tests, simply run `dotnet test`.

### Release a new version

To make a new release, push a tag named `release/<version>`, where `<version>` is a valid .NET version string, like `1.2.3` or `1.0.0-alpha-5`.
This is picked up by a GitHub workflow, which will automatically build and publish your release.
