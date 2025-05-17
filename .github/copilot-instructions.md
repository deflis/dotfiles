# このリポジトリについて

このリポジトリは chezmoi で管理されている dotfiles です。
chezmoi は、dotfiles を管理するためのツールで、シンボリックリンクを使用して、ホームディレクトリにある設定ファイルを管理します。

chezmoi は、 Go の `text/template` パッケージを使用しており、Go のドキュメントを参照することができます。
以下は chezmoi のテンプレートで利用可能な関数のリストです。

# Directives

File-specific template options can be set using template directives in the
template of the form:

    chezmoi:template:$KEY=$VALUE

which sets the template option `$KEY` to `$VALUE`. `$VALUE` must be quoted if it
contains spaces or double quotes. Multiple key/value pairs may be specified on a
single line.

Lines containing template directives are removed to avoid parse errors from any
delimiters. If multiple directives are present in a file, later directives
override earlier ones.

## Delimiters

By default, chezmoi uses the standard `text/template` delimiters `{{` and `}}`.
If a template contains the string:

    chezmoi:template:left-delimiter=$LEFT right-delimiter=$RIGHT

Then the delimiters `$LEFT` and `$RIGHT` are used instead. Either or both of
`left-delimiter=$LEFT` and `right-delimiter=$RIGHT` may be omitted. If either
`$LEFT` or `$RIGHT` is empty then the default delimiter (`{{` and `}}`
respectively) is set instead.

The delimiters are specific to the file in which they appear and are not
inherited by templates called from the file.

!!! example

    ```sh
    #!/bin/sh
    # chezmoi:template:left-delimiter="# [[" right-delimiter=]]

    # [[ "true" ]]
    ```

## Encoding

Templates are always written in UTF-8 with no byte order mark.

By default, the result of executing a template is also UTF-8 with no
byte order mark but this can be transformed into another encoding with the
template directive:

    chezmoi:template:encoding=$ENCODING

where `$ENCODING` is one of:

| Encoding        | Description                                  |
| --------------- | -------------------------------------------- |
| `utf-8`         | UTF-8 with no byte order mark                |
| `utf-8-bom`     | UTF-8 with a byte order mark                 |
| `utf-16-be`     | Big-endian UTF-16 with no byte order mark    |
| `utf-16-be-bom` | Big-endian UTF-16 with a byte order mark     |
| `utf-16-le`     | Little-endian UTF-16 with no byte order mark |
| `utf-16-le-bom` | Little-endian UTF-16 with a byte order mark  |

!!! example

    ```
    {{/* chezmoi:template:encoding=utf-16-le */}}
    ```

## Format indent

By default, chezmoi's `toJson`, `toToml`, and `toYaml` template functions use
the default indent of two spaces. The indent can be overridden with:

    chezmoi:template:format-indent=$STRING

to set the indent to be the literal `$STRING`, or

    chezmoi:template:format-indent-width=$WIDTH

to set the indent to be `$WIDTH` spaces.

!!! example

    ```
    {{/* chezmoi:template:format-indent="\t" */}}
    {{ dict "key" "value" | toJson }}
    ```

!!! example

    ```
    {{/* chezmoi:template:format-indent-width=4 */}}
    {{ dict "key" "value" | toYaml }}
    ```

## Line endings

Many of the template functions available in chezmoi primarily use UNIX-style
line endings (`lf`/`\n`), which may result in unexpected output when running
`chezmoi diff` on a `modify_` template. These line endings can be overridden
with a template directive:

    chezmoi:template:line-endings=$VALUE

`$VALUE` can be an arbitrary string or one of:

| Value    | Effect                                                               |
| -------- | -------------------------------------------------------------------- |
| `crlf`   | Use Windows line endings (`\r\n`)                                    |
| `lf`     | Use UNIX-style line endings (`\n`)                                   |
| `native` | Use platform-native line endings (`crlf` on Windows, `lf` elsewhere) |

## Missing keys

By default, chezmoi will return an error if a template indexes a map with a key
that is not present in the map. This behavior can be changed globally with the
`template.options` configuration variable or with a template directive:

    chezmoi:template:missing-key=$VALUE

`$VALUE` can be one of:

| Value     | Effect                                                                                        |
| --------- | --------------------------------------------------------------------------------------------- |
| `error`   | Return an error on any missing key (default)                                                  |
| `invalid` | Ignore missing keys. If printed, the result of the index operation is the string `<no value>` |
| `zero`    | Ignore missing keys. If printed, the result of the index operation is the zero value          |

# Templates

chezmoi executes templates using [`text/template`][go-template]. The result is
treated differently depending on whether the target is a file or a symlink.

If target is a file, then:

- If the result is an empty string, then the file is removed.

- Otherwise, the target file contents are result.

If the target is a symlink, then:

- Leading and trailing whitespace are stripped from the result.

- If the result is an empty string, then the symlink is removed.

- Otherwise, the target symlink target is the result.

chezmoi executes templates using `text/template`'s `missingkey=error` option,
which means that misspelled or missing keys will raise an error. This can be
overridden by setting a list of options in the configuration file.

!!! hint

    For a full list of template options, see [`Template.Option`][option].

!!! example

    ```toml title="~/.config/chezmoi/chezmoi.toml"
    [template]
        options = ["missingkey=zero"]
    ```

[go-template]: https://pkg.go.dev/text/template
[option]: https://pkg.go.dev/text/template?tab=doc#Template.Option

# Variables

chezmoi provides the following automatically-populated variables:

| Variable                     | Type     | Value                                                                                                                                                    |
| ---------------------------- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `.chezmoi.arch`              | string   | Architecture, e.g. `amd64`, `arm`, etc. as returned by [runtime.GOARCH][constants]                                                                       |
| `.chezmoi.args`              | []string | The arguments passed to the `chezmoi` command, starting with the program command                                                                         |
| `.chezmoi.cacheDir`          | string   | The cache directory                                                                                                                                      |
| `.chezmoi.config`            | object   | The configuration, as read from the config file                                                                                                          |
| `.chezmoi.configFile`        | string   | The path to the configuration file used by chezmoi                                                                                                       |
| `.chezmoi.destDir`           | string   | The destination directory                                                                                                                                |
| `.chezmoi.executable`        | string   | The path to the `chezmoi` executable, if available                                                                                                       |
| `.chezmoi.fqdnHostname`      | string   | The fully-qualified domain name hostname of the machine chezmoi is running on                                                                            |
| `.chezmoi.gid`               | string   | The primary group ID                                                                                                                                     |
| `.chezmoi.group`             | string   | The group of the user running chezmoi                                                                                                                    |
| `.chezmoi.homeDir`           | string   | The home directory of the user running chezmoi                                                                                                           |
| `.chezmoi.hostname`          | string   | The hostname of the machine chezmoi is running on, up to the first `.`                                                                                   |
| `.chezmoi.kernel`            | object   | Contains information from `/proc/sys/kernel`. Linux only, useful for detecting specific kernels (e.g. Microsoft's WSL kernel)                            |
| `.chezmoi.os`                | string   | Operating system, e.g. `darwin`, `linux`, etc. as returned by [runtime.GOOS][constants]                                                                  |
| `.chezmoi.osRelease`         | object   | The information from `/etc/os-release`, Linux only, run `chezmoi data` to see its output                                                                 |
| `.chezmoi.pathListSeparator` | string   | The path list separator, typically `;` on Windows and `:` on other systems. Used to separate paths in environment variables. i.e., `/bin:/sbin:/usr/bin` |
| `.chezmoi.pathSeparator`     | string   | The path separator, typically `\` on windows and `/` on unix. Used to separate files and directories in a path. i.e., `c:\see\dos\run`                   |
| `.chezmoi.sourceDir`         | string   | The source directory                                                                                                                                     |
| `.chezmoi.sourceFile`        | string   | The path of the template relative to the source directory                                                                                                |
| `.chezmoi.targetFile`        | string   | The absolute path of the target file for the template                                                                                                    |
| `.chezmoi.uid`               | string   | The user ID                                                                                                                                              |
| `.chezmoi.username`          | string   | The username of the user running chezmoi                                                                                                                 |
| `.chezmoi.version.builtBy`   | string   | The program that built the `chezmoi` executable, if set                                                                                                  |
| `.chezmoi.version.commit`    | string   | The git commit at which the `chezmoi` executable was built, if set                                                                                       |
| `.chezmoi.version.date`      | string   | The timestamp at which the `chezmoi` executable was built, if set                                                                                        |
| `.chezmoi.version.version`   | string   | The version of chezmoi                                                                                                                                   |
| `.chezmoi.windowsVersion`    | object   | Windows version information, if running on Windows                                                                                                       |
| `.chezmoi.workingTree`       | string   | The working tree of the source directory                                                                                                                 |

`.chezmoi.windowsVersion` contains the following keys populated from the
registry key
`Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows
NT\CurrentVersion`.

| Key                         | Type    |
| --------------------------- | ------- |
| `currentBuild`              | string  |
| `currentMajorVersionNumber` | integer |
| `currentMinorVersionNumber` | integer |
| `currentVersion`            | string  |
| `displayVersion`            | string  |
| `editionID`                 | string  |
| `productName`               | string  |

Additional variables can be defined in the config file in the `data` section.
Variable names must consist of a letter and be followed by zero or more letters
and/or digits.

[constants]: https://pkg.go.dev/runtime?tab=doc#pkg-constants

# 1Password functions

The `onepassword*` template functions return structured data from
[1Password][1p] using the [1Password CLI][op] (`op`).

!!! info

    When using the 1Password CLI with biometric authentication, chezmoi derives
    values from `op account list` that can resolves into the appropriate
    1Password *account-uuid*.

    As an example, if `op account list --format=json` returns the following
    structure:

    ```json
    [
      {
        "url": "account1.1password.ca",
        "email": "my@email.com",
        "user_uuid": "some-user-uuid",
        "account_uuid": "some-account-uuid"
      }
    ]
    ```

    The following values can be used in the `account` parameter and the value
    `some-account-uuid` will be passed as the `--account` parameter to `op`.

    - `some-account-uuid`
    - `some-user-uuid`
    - `account1.1password.ca`
    - `account1`
    - `my@email.com`
    - `my`
    - `my@account1.1password.ca`
    - `my@account1`

    If there are multiple accounts and any value exists more than once, that
    value will be removed from the account mapping. That is, if you are signed
    into `my@email.com` and `your@email.com` for `account1.1password.ca`, then
    `account1.1password.ca` will not be a valid lookup value, but `my@account1`,
    `my@account1.1password.ca`, `your@account1`, and
    `your@account1.1password.ca` would all be valid lookups.

!!! warning

    chezmoi has experimental support for [1Password secrets
    automation][automation] modes. These modes change how the 1Password CLI
    works and affect all functions. Most notably, `account` parameters are not
    allowed on all 1Password template functions.

[1p]: https://1password.com/
[op]: https://developer.1password.com/docs/cli
[automation]: /user-guide/password-managers/1password.md#secrets-automation

# `onepassword` _uuid_ [_vault_ [*account*]]

`onepassword` returns structured data from [1Password][1p] using the
[1Password CLI][op] (`op`). _uuid_ is passed to
`op item get $UUID --format json` and the output from `op` is parsed as JSON.
The output from `op` is cached so calling `onepassword` multiple times with the
same _uuid_ will only invoke `op` once. If the optional _vault_ is supplied, it
will be passed along to the `op item get` call, which can significantly improve
performance. If the optional _account_ is supplied, it will be passed along to
the `op item get` call, which will help it look in the right account, in case
you have multiple accounts (e.g., personal and work accounts).

If there is no valid session in the environment, by default you will be
interactively prompted to sign in.

The 1password CLI command can be set with the `onePassword.command` config
variable, and extra arguments can be specified with the `onePassword.args`
config variable.

!!! example

    ```
    {{ (onepassword "$UUID").fields[1].value }}
    {{ (onepassword "$UUID" "$VAULT_UUID").fields[1].value }}
    {{ (onepassword "$UUID" "$VAULT_UUID" "$ACCOUNT_NAME").fields[1].value }}
    {{ (onepassword "$UUID" "" "$ACCOUNT_NAME").fields[1].value }}
    ```

    A more robust way to get a password field would be something like:

    ```
    {{ range (onepassword "$UUID").fields -}}
    {{   if and (eq .label "password") (eq .purpose "PASSWORD") -}}
    {{     .value -}}
    {{   end -}}
    {{ end }}
    ```

!!! warning

    When using [1Password secrets automation][automation], the *account*
    parameter is not allowed.

[1p]: https://1password.com/
[op]: https://support.1password.com/command-line-getting-started/
[automation]: /user-guide/password-managers/1password.md#secrets-automation

# `onepasswordDetailsFields` _uuid_ [_vault_ [*account*]]

`onepasswordDetailsFields` returns structured data from [1Password][1p] using
the [1Password CLI][op] (`op`). _uuid_ is passed to `op get item $UUID`, the
output from `op` is parsed as JSON, and elements of `details.fields` are
returned as a map indexed by each field's `id` (if set) or `label` (if set and
`id` is not present).

If there is no valid session in the environment, by default you will be
interactively prompted to sign in.

The output from `op` is cached so calling `onepasswordDetailsFields` multiple
times with the same _uuid_ will only invoke `op` once. If the optional
_vault_ is supplied, it will be passed along to the `op get` call, which
can significantly improve performance. If the optional _account_ is
supplied, it will be passed along to the `op get` call, which will help it look
in the right account, in case you have multiple accounts (e.g. personal and work
accounts).

!!! example

    ```
    {{ (onepasswordDetailsFields "$UUID").password.value }}
    {{ (onepasswordDetailsFields "$UUID" "$VAULT_UUID").password.value }}
    {{ (onepasswordDetailsFields "$UUID" "$VAULT_UUID" "$ACCOUNT_NAME").password.value }}
    {{ (onepasswordDetailsFields "$UUID" "" "$ACCOUNT_NAME").password.value }}
    ```

!!! example

    Given the output from `op`:

    ```json
    {
        "uuid": "$UUID",
        "details": {
            "fields": [
                {
                    "designation": "username",
                    "name": "username",
                    "type": "T",
                    "value": "exampleuser"
                },
                {
                    "designation": "password",
                    "name": "password",
                    "type": "P",
                    "value": "examplepassword"
                }
            ]
        }
    }
    ```

    the return value of `onepasswordDetailsFields` will be the map:

    ```json
    {
        "username": {
            "designation": "username",
            "name": "username",
            "type": "T",
            "value": "exampleuser"
        },
        "password": {
            "designation": "password",
            "name": "password",
            "type": "P",
            "value": "examplepassword"
        }
    }
    ```

!!! warning

    When using [1Password secrets automation][automation], the *account*
    parameter is not allowed.

[1p]: https://1password.com/
[op]: https://support.1password.com/command-line-getting-started/
[automation]: /user-guide/password-managers/1password.md#secrets-automation

# `onepasswordDocument` _uuid_ [_vault_ [*account*]]

`onepasswordDocument` returns a document from [1Password][1p] using the
[1Password CLI][op] (`op`). _uuid_ is passed to `op get document $UUID` and the
output from `op` is returned. The output from `op` is cached so calling
`onepasswordDocument` multiple times with the same _uuid_ will only invoke `op`
once. If the optional _vault_ is supplied, it will be passed along to the `op
get` call, which can significantly improve performance. If the optional
_account_ is supplied, it will be passed along to the `op get` call, which will
help it look in the right account, in case you have multiple accounts (e.g.,
personal and work accounts).

If there is no valid session in the environment, by default you will be
interactively prompted to sign in.

!!! example

    ```
    {{- onepasswordDocument "$UUID" -}}
    {{- onepasswordDocument "$UUID" "$VAULT_UUID" -}}
    {{- onepasswordDocument "$UUID" "$VAULT_UUID" "$ACCOUNT_NAME" -}}
    {{- onepasswordDocument "$UUID" "" "$ACCOUNT_NAME" -}}
    ```

!!! warning

    When using [1Password Connect][connect], `onepasswordDocument` is not
    available.

!!! warning

    When using [1Password Service Accounts][accounts], the *account* parameter
    is not allowed.

[1p]: https://1password.com/
[op]: https://developer.1password.com/docs/cli
[connect]: /user-guide/password-managers/1password.md#1password-connect
[accounts]: /user-guide/password-managers/1password.md#1password-service-accounts

# `onepasswordItemFields` _uuid_ [_vault_ [*account*]]

`onepasswordItemFields` returns structured data from [1Password][1p] using the
[1Password CLI][op] (`op`). _uuid_ is passed to `op item get $UUID --format
json`, the output from `op` is parsed as JSON, and each element of
`details.sections` are iterated over and any `fields` are returned as a map
indexed by each field's `n`.

If there is no valid session in the environment, by default you will be
interactively prompted to sign in.

!!! example

    The result of

    ```
    {{ (onepasswordItemFields "abcdefghijklmnopqrstuvwxyz").exampleLabel.value }}
    ```

    is equivalent to calling

    ```console
    $ op item get abcdefghijklmnopqrstuvwxyz --fields label=exampleLabel
    # or
    $ op item get abcdefghijklmnopqrstuvwxyz --fields exampleLabel
    ```

!!! example

    Given the output from `op`:

    ```json
    {
        "id": "$UUID",
        "title": "$TITLE",
        "version": 1,
        "vault": {
            "id": "$vaultUUID"
        },
        "category": "LOGIN",
        "last_edited_by": "userUUID",
        "created_at": "2022-01-12T16:29:26Z",
        "updated_at": "2022-01-12T16:29:26Z",
        "sections": [
            {
                "id": "$sectionID",
                "label": "Related Items"
            }
        ],
        "fields": [
            {
                "id": "nerlnqbfzdm5q5g6ydsgdqgdw4",
                "type": "STRING",
                "label": "exampleLabel",
                "value": "exampleValue"
            }
        ],
    }
    ```

    the return value of `onepasswordItemFields` will be the map:

    ```json
    {
        "exampleLabel": {
            "id": "string",
            "type": "D4328E0846D2461E8E455D7A07B93397",
            "label": "exampleLabel",
            "value": "exampleValue"
        }
    }
    ```

!!! warning

    When using [1Password secrets automation][automation], the *account*
    parameter is not allowed.

[1p]: https://1password.com/
[op]: https://support.1password.com/command-line-getting-started/
[automation]: /user-guide/password-managers/1password.md#secrets-automation

# `onepasswordRead` _url_ [*account*]

`onepasswordRead` returns data from [1Password][1p] using the [1Password
CLI][op] (`op`). _url_ is passed to the `op read --no-newline` command. If
_account_ is specified, the extra arguments `--account $ACCOUNT` are passed to
`op`.

If there is no valid session in the environment, by default you will be
interactively prompted to sign in.

!!! example

    The result of

    ```
    {{ onepasswordRead "op://vault/item/field" }}
    ```

    is equivalent to calling

    ```console
    $ op read --no-newline op://vault/item/field
    ```

!!! warning

    When using [1Password secrets automation][automation], the *account*
    parameter is not allowed.

[1p]: https://1password.com/
[op]: https://developer.1password.com/docs/cli
[automation]: /user-guide/password-managers/1password.md#secrets-automation

# `awsSecretsManager` _arn_

`awsSecretsManager` returns structured data retrieved from [AWS Secrets
Manager][awssm]. _arn_ specifies the `SecretId` passed to
[`GetSecretValue`][gsv]. This can either be the full ARN or the [simpler
name][name] if applicable.

[awssm]: https://aws.amazon.com/secrets-manager/
[gsv]: https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
[name]: https://docs.aws.amazon.com/secretsmanager/latest/userguide/troubleshoot.html#ARN_secretnamehyphen

# `awsSecretsManagerRaw` _arn_

`awsSecretsManager` returns the raw string value retrieved from [AWS Secrets
Manager][awssm]. _arn_ specifies the `SecretId` passed to
[`GetSecretValue`][gsv]. This can either be the full ARN or the [simpler
name][name] if applicable.

[awssm]: https://aws.amazon.com/secrets-manager/
[gsv]: https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
[name]: https://docs.aws.amazon.com/secretsmanager/latest/userguide/troubleshoot.html#ARN_secretnamehyphen

# AWS Secrets Manager functions

The `awsSecretsManager*` functions return data from [AWS Secrets Manager][awssm]
using the [`GetSecretValue`][gsv] API.

The profile and region are pulled from the standard environment variables and
shared config files but can be overridden by setting `awsSecretsManager.profile`
and `awsSecretsManager.region` configuration variables respectively.

[awssm]: https://aws.amazon.com/secrets-manager/
[gsv]: https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html

# `azureKeyVault` _secret name_ [*vault-name*]

`azureKeyVault` returns a secret value retrieved from an [Azure Key
Vault][azkv].

The mandatory `secret name` argument specifies the _name of the secret_ to
retrieve.

The optional `vault name` argument specifies the _name of the vault_, if not set,
the default vault name will be used.

!!! warning

    The current implementation will always return the latest version of the secret.
    Retrieving a specific version of a secret is not supported.

[azkv]: https://learn.microsoft.com/en-us/azure/key-vault/general/

# `bitwarden` [*arg*...]

`bitwarden` returns structured data retrieved from [Bitwarden][bitwarden] using
the [Bitwarden CLI][cli] (`bw`). *arg*s are passed to `bw get` unchanged and the
output from `bw get` is parsed as JSON.

The output from `bw get` is cached so calling `bitwarden` multiple times with
the same arguments will only invoke `bw` once.

!!! example

    ```
    username = {{ (bitwarden "item" "$ITEMID").login.username }}
    password = {{ (bitwarden "item" "$ITEMID").login.password }}
    ```

[bitwarden]: https://bitwarden.com
[cli]: https://bitwarden.com/help/cli

# `bitwardenAttachment` _filename_ _itemid_

`bitwardenAttachment` returns a document from [Bitwarden][bitwarden] using the
[Bitwarden CLI][cli] (`bw`). _filename_ and _itemid_ are passed to `bw get
attachment $FILENAME --itemid $ITEMID` and the output is returned.

The output from `bw` is cached so calling `bitwardenAttachment` multiple times
with the same _filename_ and _itemid_ will only invoke `bw` once.

!!! example

    ```
    {{- bitwardenAttachment "$FILENAME" "$ITEMID" -}}
    ```

[bitwarden]: https://bitwarden.com/
[cli]: https://bitwarden.com/help/article/cli/

# `bitwardenAttachmentByRef` _filename_ _args_

`bitwardenAttachmentByRef` returns a document from [Bitwarden][bitwarden] using
the [Bitwarden CLI][cli] (`bw`). This method requires two calls to `bw` to
complete:

1. First, _args_ are passed to `bw get` in order to retrieve the item's
   _itemid_.
2. Then, _filename_ and _itemid_ are passed to `bw get attachment $FILENAME
--itemid $ITEMID` and the output from `bw` is returned.

The output from `bw` is cached so calling `bitwardenAttachmentByRef` multiple
times with the same _filename_ and _itemid_ will only invoke `bw` once.

!!! example

    ```
    {{- bitwardenAttachmentByRef "$FILENAME" "$ARGS" -}}
    ```

!!! example

    ```
    {{- bitwardenAttachmentByRef "id_rsa" "item" "example.com" -}}
    ```

[bitwarden]: https://bitwarden.com/
[cli]: https://bitwarden.com/help/article/cli/

# `bitwardenFields` [*arg*...]

`bitwardenFields` returns structured data retrieved from [Bitwarden][bitwarden]
using the [Bitwarden CLI][cli] (`bw`). *arg*s are passed to `bw get` unchanged,
the output from `bw get` is parsed as JSON, and the elements of `fields` are
returned as a dict indexed by each field's `name`.

The output from `bw get` is cached so calling `bitwardenFields` multiple times
with the same arguments will only invoke `bw get` once.

!!! example

    ```
    {{ (bitwardenFields "item" "$ITEMID").token.value }}
    ```

!!! example

    Given the output from `bw get`:

    ```json
    {
        "object": "item",
        "id": "bf22e4b4-ae4a-4d1c-8c98-ac620004b628",
        "organizationId": null,
        "folderId": null,
        "type": 1,
        "name": "example.com",
        "notes": null,
        "favorite": false,
        "fields": [
            {
                "name": "hidden",
                "value": "hidden-value",
                "type": 1
            },
            {
                "name": "token",
                "value": "token-value",
                "type": 0
            }
        ],
        "login": {
            "username": "username-value",
            "password": "password-value",
            "totp": null,
            "passwordRevisionDate": null
        },
        "collectionIds": [],
        "revisionDate": "2020-10-28T00:21:02.690Z"
    }
    ```

    the return value if `bitwardenFields` will be the map:

    ```json
    {
        "hidden": {
            "name": "hidden",
            "type": 1,
            "value": "hidden-value"
        },
        "token": {
            "name": "token",
            "type": 0,
            "value": "token-value"
        }
    }
    ```

[bitwarden]: https://bitwarden.com
[cli]: https://bitwarden.com/help/cli

# `bitwardenSecrets` _secret-id_ [*access-token*]

`bitwardenSecrets` returns structured data from [Bitwarden][bitwarden] using the
[Bitwarden Secrets CLI][secrets] (`bws`). _secret-id_ is passed to `bws secret
get` and the output from `bws secret get` is parsed as JSON and returned.

If the additional _access-token_ argument is given, it is passed to `bws secret
get` with the `--access-token` flag.

The output from `bws secret get` is cached so calling `bitwardenSecrets`
multiple times with the same _secret-id_ and _access-token_ will only invoke
`bws secret get` once.

!!!

    ```
    {{ (bitwardenSecrets "be8e0ad8-d545-4017-a55a-b02f014d4158").value }}
    ```

[bitwarden]: https://bitwarden.com
[secrets]: https://bitwarden.com/help/secrets-manager-cli/

# Bitwarden functions

The `bitwarden*` and `rbw*` functions return data from [Bitwarden][bitwarden]
using the [Bitwarden CLI][bw] (`bw`), [Bitwarden Secrets CLI][bws] (`bws`), and
[`rbw`][rbw] commands.

[bitwarden]: https://bitwarden.com
[bw]: https://bitwarden.com/help/article/cli/
[bws]: https://bitwarden.com/help/secrets-manager-cli/
[rbw]: https://github.com/doy/rbw

# `rbw` _name_ [*arg*...]

`rbw` returns structured data retrieved from [Bitwarden][bitwarden] using
[`rbw`][rbw]. _name_ is passed to `rbw get --raw`, along with any extra *arg*s,
and the output is parsed as JSON.

The output from `rbw get --raw` is cached so calling `rbw` multiple times with
the same arguments will only invoke `rbw` once.

!!! example

    ```
    username = {{ (rbw "test-entry").data.username }}
    password = {{ (rbw "test-entry" "--folder" "my-folder").data.password }}
    ```

[bitwarden]: https://bitwarden.com
[rbw]: https://github.com/doy/rbw

# `rbwFields` _name_ [*arg*...]

`rbw` returns structured data retrieved from [Bitwarden][bitwarden] using
[`rbw`][rbw]. _name_ is passed to `rbw get --raw`, along with any extra *arg*s,
the output is parsed as JSON, and the elements of `fields` are returned as
a dict indexed by each field's `name`.

The output from `rbw get --raw` is cached so calling `rbwFields` multiple times with
the same arguments will only invoke `rbwFields` once.

!!! example

    ```
    {{ (rbwFields "item").name.value }}
    {{ (rbwFields "item" "--folder" "my-folder").name.value }}
    ```

[bitwarden]: https://bitwarden.com
[rbw]: https://github.com/doy/rbw

# `dashlaneNote` _filter_

`dashlaneNote` returns the content of a secure note from [Dashlane][dashlane]
using the [Dashlane CLI][cli] (`dcli`). _filter_ is passed to `dcli note`, and
the output from `dcli note` is just read as a multi-line string.

The output from `dcli note` is cached so calling `dashlaneNote` multiple times
with the same _filter_ will only invoke `dcli note` once.

!!! example

    ```
    {{ dashlaneNote "filter" }}
    ```

[dashlane]: https://dashlane.com
[cli]: https://github.com/Dashlane/dashlane-cli

# `dashlanePassword` _filter_

`dashlanePassword` returns structured data from [Dashlane][dashlane] using the
[Dashlane CLI][cli] (`dcli`). _filter_ is passed to `dcli password --output
json`, and the output from `dcli password` is parsed as JSON.

The output from `dcli password` cached so calling `dashlanePassword` multiple
times with the same _filter_ will only invoke `dcli password` once.

!!! example

    ```
    {{ (index (dashlanePassword "filter") 0).password }}
    ```

[dashlane]: https://dashlane.com
[cli]: https://github.com/Dashlane/dashlane-cli

# Dashlane functions

The `dashlane*` functions return data from [Dashlane][dashlane]
using the [Dashlane CLI][cli].

[dashlane]: https://dashlane.com
[cli]: https://github.com/Dashlane/dashlane-cli

# `doppler` _key_ [_project_ [*config*]]

`doppler` returns the secret for the specified project and configuration from
[Doppler][doppler] using `doppler secrets download --json --no-file`.

If either of _project_ or _config_ are empty or
omitted, then chezmoi will use the value from the
`doppler.project` and
`doppler.config` config variables if they are set and not empty.

!!! example

    ```
    {{ doppler "SECRET_NAME" "project_name" "configuration_name" }}
    ```

[doppler]: https://www.doppler.com

# `dopplerProjectJson` [_project_ [*config*]]

`dopplerProjectJson` returns the secret for the specified project and
configuration from [Doppler][doppler] using `doppler secrets download --json
--no-file` as `json` structured data.

If either of _project_ or _config_ are empty or
omitted, then chezmoi will use the value from the
`doppler.project` and
`doppler.config` config variables if they are set and not empty.

!!! example

    ```
    {{ (dopplerProjectJson "project_name" "configuration_name").SECRET_NAME }}
    ```

[doppler]: https://www.doppler.com

# Doppler

chezmoi includes support for [Doppler][doppler] using the `doppler` CLI to
expose data through the `doppler` and `dopplerProjectJson` template functions.

[doppler]: https://www.doppler.com

# `ejsonDecrypt` _filePath_

`ejsonDecrypt` returns the decrypted content of an [ejson][ejson]-encrypted
file.

_filePath_ indicates where the encrypted file is located.

The decrypted file is cached so calling `ejsonDecrypt` multiple times with the
same _filePath_ will only run through the decryption process once. The cache is
shared with `ejsonDecryptWithKey`.

!!! example

    ```
    {{ (ejsonDecrypt "my-secrets.ejson").password }}
    ```

[ejson]: https://github.com/Shopify/ejson

# `ejsonDecryptWithKey` _filePath_ _key_

`ejsonDecryptWithKey` returns the decrypted content of an
[ejson][ejson]-encrypted file.

_filePath_ indicates where the encrypted file is located,
and _key_ is used to decrypt the file.

The decrypted file is cached so calling `ejsonDecryptWithKey` multiple
times with the same _filePath_ will only run through the decryption
process once. The cache is shared with `ejsonDecrypt`.

!!! example

    ```
    {{ (ejsonDecryptWithKey "my-secrets.ejson" "top-secret-key").password }}
    ```

[ejson]: https://github.com/Shopify/ejson

# ejson functions

The `ejson*` functions return data from [ejson][ejson]-encrypted files.

[ejson]: https://github.com/Shopify/ejson

# `comment` _prefix_ _text_

`comment` returns _text_ with each line prefixed with _prefix_.

!!! example

    ```
    {{ "Header" | comment "# " }}
    ```

# `completion` _shell_

`completion` returns chezmoi's shell completion for _shell_. _shell_ can be one
of `bash`, `fish`, `powershell`, or `zsh`.

!!! example

    ```
    {{ completion "zsh" }}
    ```

# `decrypt` _ciphertext_

`decrypt` decrypts _ciphertext_ using chezmoi's configured encryption method.

!!! example

    ```
    {{ joinPath .chezmoi.sourceDir ".ignored-encrypted-file.age" | include | decrypt }}
    ```

# `deleteValueAtPath` _path_ _dict_

`deleteValueAtPath` modifies _dict_ to delete the value at _path_ and returns
_dict_. _path_ can be either a string containing a `.`-separated list of keys or
a list of keys.

If _path_ does not exist in _dict_ then `deleteValueAtPath` returns _dict_
unchanged.

!!! example

    ```
    {{ dict "outer" (dict "inner" "value") | deleteValueAtPath "outer.inner" | toJson }}
    {{ dict | setValueAtPath "key1" "value1" | setValueAtPath "key2.nestedKey" "value2" | toJson }}
    {{ dict | setValueAtPath (list "key2" "nestedKey") "value2" | toJson }}
    ```

# `encrypt` _plaintext_

`encrypt` encrypts _plaintext_ using chezmoi's configured encryption method.

# `eqFold` _string1_ _string2_ [*extraString*...]

`eqFold` returns the boolean truth of comparing _string1_ with _string2_ and
any number of *extraString*s under Unicode case-folding.

!!! example

    ```
    {{ $commandOutput := output "path/to/output-FOO.sh" }}
    {{ if eqFold "foo" $commandOutput }}
    # $commandOutput is "foo"/"Foo"/"FOO"...
    {{ else if eqFold "bar" $commandOutput }}
    # $commandOutput is "bar"/"Bar"/"BAR"...
    {{ end }}
    ```

# `findExecutable` _file_ _path-list_

`findExecutable` searches for an executable named _file_ in directories
identified by _path-list_. The result will be the executable file concatenated
with the matching path. If an executable _file_ cannot be found in _path-list_,
`findExecutable` returns an empty string.

`findExecutable` is provided as an alternative to [`lookPath`][site-lookpath] so
that you can interrogate the system PATH as it would be configured after
`chezmoi apply`. Like `lookPath`, `findExecutable` is not hermetic: its return
value depends on the state of the file system at the moment the template is
executed. Exercise caution when using it in your templates.

The return value of the first successful call to `findExecutable` is cached, and
future calls to `findExecutable` with the same parameters will return this path.

!!! info

    On Windows, the resulting path will contain the first found executable
    extension as identified by the environment variable `%PathExt%`.

!!! example

    ```
    {{ if findExecutable "mise" (list "bin" "go/bin" ".cargo/bin" ".local/bin") }}
    # $HOME/.cargo/bin/mise exists and will probably be in $PATH after apply
    {{ end }}
    ```

[site-lookpath]: /reference/templates/functions/lookPath.md

# `findOneExecutable` _file-list_ _path-list_

`findOneExecutable` searches for an executable from _file-list_ in directories
identified by _path-list_, finding the first matching executable in the first
matching directory (each directory is searched for matching executables in
turn). The result will be the executable file concatenated with the matching
path. If an executable from _file-list_ cannot be found in _path-list_,
`findOneExecutable` returns an empty string.

`findOneExecutable` is provided as an alternative to [`lookPath`][lookpath]
so that you can interrogate the system PATH as it would be configured after
`chezmoi apply`. Like `lookPath`, `findOneExecutable` is not hermetic: its
return value depends on the state of the file system at the moment the template
is executed. Exercise caution when using it in your templates.

The return value of the first successful call to `findOneExecutable` is cached,
and future calls to `findOneExecutable` with the same parameters will return
this path.

!!! info

    On Windows, the resulting path will contain the first found executable
    extension as identified by the environment variable `%PathExt%`.

!!! example

    ```
    {{ if findOneExecutable (list "eza" "exa") (list "bin" "go/bin" ".cargo/bin" ".local/bin") }}
    # $HOME/.cargo/bin/exa exists and will probably be in $PATH after apply
    {{ end }}
    ```

[lookpath]: /reference/templates/functions/lookPath.md

# `fromIni` _initext_

`fromIni` returns the parsed value of _initext_.

!!! example

    ```
    {{ (fromIni "[section]\nkey = value").section.key }}
    ```

# `fromJson` _jsontext_

`fromJson` parses _jsontext_ as JSON and returns the parsed value.

JSON numbers that can be represented exactly as 64-bit signed integers are
returned as such. Otherwise, if the number is in the range of 64-bit IEEE
floating point values, it is returned as such. Otherwise, the number is returned
as a string. See [RFC7159 Section 6][rfc7159s6].

[rfc7159S6]: https://www.rfc-editor.org/rfc/rfc7159#section-6

# `fromJsonc` _jsonctext_

`fromJsonc` parses _jsonctext_ as JSONC using
[`github.com/tailscale/hujson`][hujson] and returns the parsed value.

[hujson]: https://github.com/tailscale/hujson

# `fromToml` _tomltext_

`fromToml` returns the parsed value of _tomltext_.

!!! example

    ```
    {{ (fromToml "[section]\nkey = \"value\"").section.key }}
    ```

# `fromYaml` _yamltext_

`fromYaml` returns the parsed value of _yamltext_.

!!! example

    ```
    {{ (fromYaml "key1: value\nkey2: value").key2 }}
    ```

# `glob` _pattern_

`glob` returns the list of files matching _pattern_ according to
[`doublestar.Glob`][glob]. Relative paths are interpreted relative to the
destination directory.

[glob]: https://pkg.go.dev/github.com/bmatcuk/doublestar/v4#Glob

# `hexDecode` _hextext_

`hexDecode` returns _hextext_ decoded from a hex-encoding string.

!!! example

    ```
    {{ hexDecode "68656c6c6f" }}
    ```

# `hexEncode` _string_

`hexEncode` returns _string_ encoded as a hex string.

!!! example

    ```
    {{ hexEncode "example" }}
    ```

# `include` _filename_

`include` returns the literal contents of the file named `*filename*`. Relative
paths are interpreted relative to the source directory.

# `includeTemplate` _filename_ [*data*]

`includeTemplate` returns the result of executing the contents of _filename_
with the optional _data_. Relative paths are first searched for in
`.chezmoitemplates` and, if not found, are interpreted relative to the source
directory.

# Functions

All standard [`text/template`][go-template] and [text template functions from
`sprig`][sprig] are included. chezmoi provides some additional functions.

[go-template]: https://pkg.go.dev/text/template
[sprig]: http://masterminds.github.io/sprig/

# `ioreg`

On macOS, `ioreg` returns the structured output of the `ioreg -a -l` command,
which includes detailed information about the I/O Kit registry.

On non-macOS operating systems, `ioreg` returns `nil`.

The output from `ioreg` is cached so multiple calls to the `ioreg` function
will only execute the `ioreg -a -l` command once.

!!! example

    ```
    {{ if eq .chezmoi.os "darwin" }}
    {{   $serialNumber := index ioreg "IORegistryEntryChildren" 0 "IOPlatformSerialNumber" }}
    {{ end }}
    ```

!!! warning

    The `ioreg` function can be very slow and should not be used. It will be
    removed in a later version of chezmoi.

# `isExecutable` _file_

`isExecutable` returns true if a file is executable.

!!! example

    ```
    {{ if isExecutable "/bin/echo" }}
    # echo is executable
    {{ end }}
    ```

# `joinPath` _element_...

`joinPath` joins any number of path elements into a single path, separating
them with the OS-specific path separator. Empty elements are ignored. The
result is cleaned. If the argument list is empty or all its elements are empty,
`joinPath` returns an empty string. On Windows, the result will only be a UNC
path if the first non-empty element is a UNC path.

!!! example

    ```text
    {{ joinPath .chezmoi.homeDir ".zshrc" }}
    ```

# `jq` _query_ _input_

`jq` runs the [jq][jq] query _query_ against _input_ and returns a list of
results.

!!! example

    ```
    {{ dict "key" "value" | jq ".key" | first }}
    ```

!!! warning

    `jq` uses [`github.com/itchyny/gojq`][gojq], which behaves slightly
    differently to the `jq` command in some [edge cases][cases].

[jq]: https://jqlang.org
[gojq]: https://github.com/itchyny/gojq
[cases]: https://github.com/itchyny/gojq#difference-to-jq

# `lookPath` _file_

`lookPath` searches for an executable named _file_ in the directories named by
the `PATH` environment variable. If file contains a slash, it is tried directly
and the `PATH` is not consulted. The result may be an absolute path or a path
relative to the current directory. If _file_ is not found, `lookPath` returns
an empty string.

`lookPath` is not hermetic: its return value depends on the state of the
environment and the file system at the moment the template is executed. Exercise
caution when using it in your templates.

The return value of the first successful call to `lookPath` is cached, and
future calls to `lookPath` for the same _file_ will return this path.

!!! example

    ```
    {{ if lookPath "diff-so-fancy" }}
    # diff-so-fancy is in $PATH
    {{ end }}
    ```

# `lstat` _name_

`lstat` runs [`os.Lstat`][lstat] on _name_. If _name_ exists it returns
structured data. If _name_ does not exist then it returns a false value. If
`os.Lstat` returns any other error then it raises an error. The structured value
returned if _name_ exists contains the fields `name`, `size`, `mode`, `perm`,
`modTime`, `isDir`, and `type`.

`lstat` is not hermetic: its return value depends on the state of the file
system at the moment the template is executed. Exercise caution when using it in
your templates.

!!! example

    ```
    {{ if eq (joinPath .chezmoi.homeDir ".xinitrc" | lstat).type "symlink" }}
    # ~/.xinitrc exists and is a symlink
    {{ end }}
    ```

[lstat]: https://pkg.go.dev/os#File.Lstat

# `mozillaInstallHash` _path_

`mozillaInstallHash` returns the Mozilla install hash for _path_. This is a
convenience function to assist the management of Firefox profiles.

# `output` _name_ [*arg*...]

`output` returns the output of executing the command _name_ with *arg*s. If
executing the command returns an error then template execution exits with an
error. The execution occurs every time that the template is executed. It is the
user's responsibility to ensure that executing the command is both idempotent
and fast.

!!! example

    ```
    current-context: {{ output "kubectl" "config" "current-context" | trim }}
    ```

# `outputList` _name_ [*argList*]

`outputList` returns the output of executing the command _name_ with the
_argList_. If executing the command returns an error then template execution
exits with an error. The execution occurs every time that the template is
executed. It is the user's responsibility to ensure that executing the command
is both idempotent and fast.

This differs from [`output`][output] in that it allows for the _argsList_ to be
created programmatically.

!!! example

    ```
    {{- $args := (list "config" "current-context")  }}
    current-context: {{ outputList "kubectl" $args | trim }}
    ```

[output]: /reference/templates/functions/output.md

# `pruneEmptyDicts` _dict_

`pruneEmptyDicts` modifies _dict_ to remove nested empty dicts. Properties are
pruned from the bottom up, so any nested dicts that themselves only contain
empty dicts are pruned.

!!! example

    ```
    {{ dict "key" "value" "inner" (dict) | pruneEmptyDicts | toJson }}
    ```

# `quoteList` _list_

`quoteList` returns a list where each element is the corresponding element in
_list_ quoted.

!!! example

    ```
    {{ $args := list "alpha" "beta" "gamma" }}
    command {{ $args | quoteList }}
    ```

    ```
    [section]
        array = [{{- $list | quoteList | join ", " -}}]
    ```

# `replaceAllRegex` _expr_ _repl_ _text_

`replaceAllRegex` returns _text_ with all substrings matching the regular
expression _expr_ replaced with _repl_. It is an alternative to [sprig's
`regexpReplaceAll` function][sprig] with a different argument order that
supports pipelining.

!!! example

    ```
    {{ "foo subject string" | replaceAllRegex "foo" "bar" }}
    ```

[sprig]: http://masterminds.github.io/sprig/strings.html

# `setValueAtPath` _path_ _value_ _dict_

`setValueAtPath` modifies _dict_ to set the value at _path_ to _value_ and
returns _dict_. _path_ can be either a string containing a `.`-separated list of
keys or a list of keys. The function will create new key/value pairs in _dict_
if needed.

This is an alternative to [sprig's `set` function][dict] with a different
argument order that supports pipelining.

!!! example

    ```
    {{ dict | setValueAtPath "key1" "value1" | setValueAtPath "key2.nestedKey" "value2" | toJson }}
    {{ dict | setValueAtPath (list "key2" "nestedKey") "value2" | toJson }}
    ```

[dict]: http://masterminds.github.io/sprig/dicts.html

# `stat` _name_

`stat` runs [`os.Stat`][stat] on _name_. If _name_ exists it returns structured
data. If _name_ does not exist then it returns a false value. If `os.Stat`
returns any other error then it raises an error. The structured value returned
if _name_ exists contains the fields `name`, `size`, `mode`, `perm`, `modTime`,
`isDir`, and `type`.

`stat` is not hermetic: its return value depends on the state of the file system
at the moment the template is executed. Exercise caution when using it in your
templates.

!!! example

    ```
    {{ if stat (joinPath .chezmoi.homeDir ".pyenv") }}
    # ~/.pyenv exists
    {{ end }}
    ```

[stat]: https://pkg.go.dev/os#File.Stat

# `toIni` _value_

`toIni` returns the ini representation of _value_, which must be a dict.

!!! example

    ```
    {{ dict "key" "value" "section" (dict "subkey" "subvalue") | toIni }}
    ```

!!! warning

    The ini format is not well defined, and the particular variant generated
    by `toIni` might not be suitable for you.

# `toPrettyJson` [*indent*] _value_

`toPrettyJson` returns the JSON representation of _value_. The optional _indent_
specifies how much nested elements are indented relative to their parent.
_indent_ defaults to two spaces.

!!! examples

    ```
    {{ dict "a" (dict "b" "c") | toPrettyJson "\t" }}
    ```

# `toString` _value_

`toString` returns the string representation of _value_. Notably, if _value_ is
a pointer, then it is safely dereferenced. If _value_ is a nil pointer, then
`toString` returns the string representation of the zero value of the pointee's
type.

# `toStrings` [*value*...]

`toStrings` converts each argument to a string and returns the list of strings.

# `toToml` _value_

`toToml` returns the TOML representation of _value_.

!!! example

    ```
    {{ dict "key" "value" | toToml }}
    ```

# `toYaml` _value_

`toYaml` returns the YAML representation of _value_.

!!! example

    ```
    {{ dict "key" "value" | toYaml }}
    ```

# `warnf` _format_ [*arg*...]

`warnf` prints a message to stderr prefixed by `chezmoi: warning: ` and returns
the empty string. _format_ is interpreted as a [printf-style format string][fmt]
with the given *arg*s.

[fmt]: https://pkg.go.dev/fmt#hdr-Printing

# `gitHubKeys` _user_

`gitHubKeys` returns _user_'s public SSH keys from GitHub using the GitHub API.
The returned value is a slice of structs with `.ID` and `.Key` fields.

!!! warning

    If you use this function to populate your `~/.ssh/authorized_keys` file
    then you potentially open SSH access to anyone who is able to modify or add
    to your GitHub public SSH keys, possibly including certain GitHub
    employees. You should not use this function on publicly-accessible machines
    and should always verify that no unwanted keys have been added, for example
    by using the `-v` / `--verbose` option when running `chezmoi apply` or
    `chezmoi update`.

    Additionally, GitHub automatically [removes keys which haven't been used in
    the last year][timeout]. This may cause your keys to be removed from
    `~/.ssh/authorized_keys` suddenly, and without any warning or indication of
    the removal. You should provide one or more keys in plain text alongside
    this function to avoid unknowingly losing remote access to your machine.

!!! example

    ```
    {{ range gitHubKeys "user" }}
    {{- .Key }}
    {{ end }}
    ```

[timeout]: https://docs.github.com/en/authentication/troubleshooting-ssh/deleted-or-missing-ssh-keys

# `gitHubLatestRelease` _owner-repo_

`gitHubLatestRelease` calls the GitHub API to retrieve the latest release about
the given _owner-repo_, returning structured data as defined by the
[GitHub Go API bindings][bindings].

Calls to `gitHubLatestRelease` are cached so calling `gitHubLatestRelease` with
the same _owner-repo_ will only result in one call to the GitHub API.

!!! example

    ```
    {{ (gitHubLatestRelease "docker/compose").TagName }}
    ```

!!! hint

    Some fields in the returned object have type `*string`. Use the
    [`toString` template function][toString] to convert these to strings.

[bindings]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryRelease
[toString]: ../functions/toString.md

# `gitHubLatestReleaseAssetURL` _owner-repo_ _pattern_

`gitHubLatestReleaseAssetURL` calls the GitHub API to retrieve the latest
release about the given _owner-repo_, returning structured data as defined by
the [GitHub Go API bindings][bindings]. It then iterates through all the
release's assets, returning the first one that matches _pattern_. _pattern_ is
a shell pattern as [described in `path.Match`][match].

Calls to `gitHubLatestReleaseAssetURL` are cached so calling
`gitHubLatestReleaseAssetURL` with the same _owner-repo_ will only result in one
call to the GitHub API.

!!! example

    ```
    {{ gitHubLatestReleaseAssetURL "FiloSottile/age" (printf "age-*-%s-%s.tar.gz" .chezmoi.os .chezmoi.arch) }}
    {{ gitHubLatestReleaseAssetURL "twpayne/chezmoi" (printf "chezmoi-%s-%s" .chezmoi.os .chezmoi.arch) }}

    ```

!!! hint

    Some fields in the returned object have type `*string`. Use the
    [`toString` template function][toString] to convert these to strings.

[bindings]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryRelease
[match]: https://pkg.go.dev/path#Match
[toString]: ../functions/toString.md

# `gitHubLatestTag` _owner-repo_

`gitHubLatestTag` calls the GitHub API to retrieve the latest tag for the given
_owner-repo_, returning structured data as defined by the [GitHub Go API
bindings][bindings].

Calls to `gitHubLatestTag` are cached the same as [`githubTags`][tags],
so calling `gitHubLatestTag` with the same _owner-repo_ will only result in one
call to the GitHub API.

!!! example

    ```
    {{ (gitHubLatestTag "docker/compose").Name }}
    ```

!!! warning

    The `gitHubLatestTag` returns the first tag returned by the [list repository
    tags GitHub API endpoint][endpoint]. Although this seems to be the most
    recent tag, the GitHub API documentation does not specify the order of the
    returned tags.

!!! hint

    Some fields in the returned object have type `*string`. Use the
    [`toString` template function][toString] to convert these to strings.

[bindings]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryTag
[endpoint]: https://docs.github.com/en/rest/repos/repos#list-repository-tags
[tags]: /reference/templates/github-functions/gitHubTags.md
[toString]: ../functions/toString.md

# `gitHubRelease` _owner-repo_ _version_

`gitHubRelease` calls the GitHub API to retrieve the latest releases about
the given _owner-repo_, It iterates through all the versions of the release,
fetching the first entry equal to _version_.

It then returns structured data as defined by the [GitHub Go API bindings][bindings].

Calls to `gitHubRelease` are cached so calling `gitHubRelease` with
the same _owner-repo_ _version_ will only result in one call to the GitHub API.

!!! example

    ```
    {{ (gitHubRelease "docker/compose" "v2.29.1").TagName }}
    ```

!!! hint

    Some fields in the returned object have type `*string`. Use the
    [`toString` template function][toString] to convert these to strings.

[bindings]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryRelease
[toString]: ../functions/toString.md

# `gitHubReleaseAssetURL` _owner-repo_ _version_ _pattern_

`gitHubReleaseAssetURL` calls the GitHub API to retrieve the latest
releases about the given _owner-repo_, returning structured data as defined by
the [GitHub Go API bindings][bindings]. It iterates through all the versions of
the release, returning the first entry equal to _version_. It then iterates
through all the release's assets, returning the first one that matches
_pattern_. _pattern_ is a shell pattern as [described in `path.Match`][match].

Calls to `gitHubReleaseAssetURL` are cached so calling
`gitHubReleaseAssetURL` with the same _owner-repo_ will only result in one
call to the GitHub API.

!!! example

    ```
    {{ gitHubReleaseAssetURL "FiloSottile/age" "age v1.2.0" (printf "age-*-%s-%s.tar.gz" .chezmoi.os .chezmoi.arch) }}
    {{ gitHubReleaseAssetURL "twpayne/chezmoi" "v2.50.0" (printf "chezmoi-%s-%s" .chezmoi.os .chezmoi.arch) }}
    ```

[bindings]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryRelease
[match]: https://pkg.go.dev/path#Match

# `gitHubReleases` _owner-repo_

`gitHubReleases` calls the GitHub API to retrieve the first page of releases for
the given _owner-repo_, returning structured data as defined by the
[GitHub Go API bindings][github-go].

Calls to `gitHubReleases` are cached so calling `gitHubReleases` with the same
_owner-repo_ will only result in one call to the GitHub API.

!!! example

    ```
    {{ (index (gitHubReleases "docker/compose") 0).TagName }}
    ```

!!! note

    The maximum number of items returned by `gitHubReleases` is determined by
    default page size for the GitHub API.

!!! warning

    The values returned by `gitHubReleases` are not directly queryable via the
    [`jq`][jq] function and must instead be converted through JSON:

    ```
    {{ gitHubReleases "docker/compose" | toJson | fromJson | jq ".[0].tag_name" }}
    ```

!!! hint

    Some fields in the returned object have type `*string`. Use the
    [`toString` template function][toString] to convert these to strings.

[github-go]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryRelease
[jq]: /reference/templates/functions/jq.md
[toString]: ../functions/toString.md

# `gitHubTags` _owner-repo_

`gitHubTags` calls the GitHub API to retrieve the first page of tags for the
given _owner-repo_, returning structured data as defined by the
[GitHub Go API bindings][github-go].

Calls to `gitHubTags` are cached so calling `gitHubTags` with the same
_owner-repo_ will only result in one call to the GitHub API.

!!! example

    ```
    {{ (index (gitHubTags "docker/compose") 0).Name }}
    ```

!!! note

    The maximum number of items returned by `gitHubReleases` is determined by
    default page size for the GitHub API.

!!! warning

    The values returned by `gitHubTags` are not directly queryable via the
    [`jq`][jq] function and must instead be converted through JSON:

    ```
    {{ gitHubTags "docker/compose" | toJson | fromJson | jq ".[0].name" }}
    ```

!!! hint

    Some fields in the returned object have type `*string`. Use the
    [`toString` template function][toString] to convert these to strings.

[github-go]: https://pkg.go.dev/github.com/google/go-github/v61/github#RepositoryTag
[jq]: /reference/templates/functions/jq.md
[toString]: ../functions/toString.md

# GitHub functions

The `gitHub*` template functions return data from the GitHub API.

By default, chezmoi makes anonymous GitHub API requests, which are subject to
[GitHub's rate limits][limits] (currently 60 requests per hour per source IP
address). chezmoi caches results from identical GitHub API requests for the
period defined in `gitHub.refreshPeriod` (default one minute).

If any of the environment variables `$CHEZMOI_GITHUB_ACCESS_TOKEN`,
`$GITHUB_ACCESS_TOKEN`, or `$GITHUB_TOKEN` are found, then the first one found
will be used to authenticate the GitHub API requests which have a higher rate
limit (currently 5,000 requests per hour per user).

In practice, GitHub API rate limits are high enough chezmoi's caching of results
mean that you should rarely need to set a token, unless you are sharing a source
IP address with many other GitHub users. If needed, the GitHub documentation
describes how to [create a personal access token][pat].

[limits]: https://docs.github.com/en/rest/overview/resources-in-the-rest-api#rate-limiting
[pat]: https://docs.github.com/en/github/authenticating-to-github/creating-a-personal-access-token

# `gopass` _gopass-name_

`gopass` returns passwords stored in [gopass][gopass] using the gopass CLI
(`gopass`). _gopass-name_ is passed to `gopass show --password $GOPASS_NAME` and
the first line of the output of `gopass` is returned with the trailing newline
stripped. The output from `gopass` is cached so calling `gopass` multiple times
with the same _gopass-name_ will only invoke `gopass` once.

!!! example

    ```
    {{ gopass "$PASS_NAME" }}
    ```

[gopass]: https://www.gopass.pw/

# `gopassRaw` _gopass-name_

`gopass` returns raw passwords stored in [gopass][gopass] using the gopass CLI
(`gopass`). _gopass-name_ is passed to `gopass show --noparsing $GOPASS_NAME`
and the output is returned. The output from `gopassRaw` is cached so calling
`gopassRaw` multiple times with the same _gopass-name_ will only invoke `gopass`
once.

[gopass]: https://www.gopass.pw/

# gopass functions

The `gopass*` template functions return data stored in [gopass][gopass] using
the gopass CLI (`gopass`) or builtin code.

By default, chezmoi will use the gopass CLI (`gopass`). Depending on your gopass
configuration, you may have to enter your passphrase once for each secret.

When setting `gopass.mode` to `builtin`, chezmoi use builtin code to access the
goapass database and caches your passphrase in plaintext in memory until chezmoi
terminates.

!!! warning

    Using the builtin code is experimental and may be removed.

[gopass]: https://www.gopass.pw/

# `hcpVaultSecret` _key_ [_application-name_ [_project-id_ [*organization-id*]]]

`hcpVaultSecret` returns the plain text secret from [HCP Vault Secrets][secrets]
using `vlt secrets get --plaintext`.

If any of _application-name_, _project-id_, or _organization-id_ are empty or
omitted, then chezmoi will use the value from the
`hcpVaultSecret.applicationName`, `hcpVaultSecret.projectId`, and
`hcpVaultSecret.organizationId` config variables if they are set and not empty.

!!! example

    ```
    {{ hcpVaultSecret "username" }}
    ```

!!! info

    If you access HCP Vault Secrets through the `hcp`, this function **will not
    work**. See [`vlt` vs `hcp`: Upgrades that Break][break].

[secrets]: https://developer.hashicorp.com/hcp/docs/vault-secrets
[break]: /user-guide/password-managers/hcp-vault-secrets.md#hcp-broken

# `hcpVaultSecretJson` _key_ [_application-name_ [_project-id_ [*organization-id*]]]

`hcpVaultSecretJson` returns structured data from [HCP Vault Secrets][secrets]
using `vlt secrets get --format=json`.

If any of _application-name_, _project-id_, or _organization-id_ are empty or
omitted, then chezmoi will use the value from the
`hcpVaultSecret.applicationName`, `hcpVaultSecret.projectId`, and
`hcpVaultSecret.organizationId` config variables if they are set and not empty.

!!! example

    ```
    {{ (hcpVaultSecretJson "secret_name" "application_name").created_by.email }}
    ```

!!! info

    If you access HCP Vault Secrets through the `hcp`, this function **will not
    work**. See [`vlt` vs `hcp`: Upgrades that Break][break].

[secrets]: https://developer.hashicorp.com/hcp/docs/vault-secrets
[break]: /user-guide/password-managers/hcp-vault-secrets.md#hcp-broken

# HCP Vault Secrets

chezmoi includes support for [HCP Vault Secrets][secrets] using the `vlt` CLI to
expose data through the `hcpVaultSecret` and `hcpVaultSecretJson` template
functions.

!!! info

    If you access HCP Vault Secrets through the `hcp`, these functions **will
    not work**. See [`vlt` vs `hcp`: Upgrades that Break][break].

[secrets]: https://developer.hashicorp.com/hcp/docs/vault-secrets
[break]: /user-guide/password-managers/hcp-vault-secrets.md#hcp-broken

# `exit` _code_

`exit` stops template execution and causes chezmoi to exit with _code_.

# Init functions

These template functions are only available when generating a config file with
`chezmoi init`. For testing with `chezmoi execute-template`, pass the `--init`
flag to enable them.

# `promptBool` _prompt_ [*default*]

`promptBool` prompts the user with _prompt_ and returns the user's response
interpreted as a boolean. If _default_ is passed and the user's response is
empty then it returns _default_. The user's response is interpreted as follows
(case insensitive):

| Response                | Result  |
| ----------------------- | ------- |
| 1, on, t, true, y, yes  | `true`  |
| 0, off, f, false, n, no | `false` |

# `promptBoolOnce` _map_ _path_ _prompt_ [*default*]

`promptBoolOnce` returns the value of _map_ at _path_ if it exists and is a
boolean value, otherwise it prompts the user for a boolean value with _prompt_
and an optional _default_ using `promptBool`.

!!! example

    ```
    {{ $hasGUI := promptBoolOnce . "hasGUI" "Does this machine have a GUI" }}
    ```

# `promptChoice` _prompt_ _choices_ [*default*]

`promptChoice` prompts the user with _prompt_ and _choices_ and returns the user's response. _choices_ must be a list of strings. If _default_ is passed and the user's response is empty then it returns _default_.

!!! example

    ```
    {{- $choices := list "desktop" "server" -}}
    {{- $hosttype := promptChoice "What type of host are you on" $choices -}}
    [data]
        hosttype = {{- $hosttype | quote -}}
    ```

# `promptChoiceOnce` _map_ _path_ _prompt_ _choices_ [*default*]

`promptChoiceOnce` returns the value of _map_ at _path_ if it exists and is a
string, otherwise it prompts the user for one of _choices_ with _prompt_ and an
optional _default_ using [`promptChoice`][pc].

!!! example

    ```
    {{- $choices := list "desktop" "laptop" "server" "termux" -}}
    {{- $hosttype := promptChoiceOnce . "hosttype" "What type of host are you on" $choices -}}
    [data]
        hosttype = {{- $hosttype | quote -}}
    ```

[pc]: /reference/templates/init-functions/promptChoice.md

# `promptInt` _prompt_ [*default*]

`promptInt` prompts the user with _prompt_ and returns the user's response
interpreted as an integer. If _default_ is passed and the user's response is
empty then it returns _default_.

# `promptIntOnce` _map_ _path_ _prompt_ [*default*]

`promptIntOnce` returns the value of _map_ at _path_ if it exists and is an
integer value, otherwise it prompts the user for a integer value with _prompt_
and an optional _default_ using `promptInt`.

!!! example

    ```
    {{ $monitors := promptIntOnce . "monitors" "How many monitors does this machine have" }}
    ```

# `promptMultichoice` _prompt_ _choices_ [*default*]

`promptMultichoice` prompts the user with _prompt_ and _choices_ and returns the
user's response. _choices_ must be a list of strings. If a _default_ list is
passed and the user's response is empty then it returns _default_.

!!! example

    ```
    {{- $choices := list "chocolate" "strawberry" "vanilla" "pistachio" -}}
    {{- $icecream := promptMultichoice "What type of ice cream do you like"
        $choices (list "pistachio" "chocolate")
    -}}
    [data]
        icecream = {{- $icecream | toToml -}}
    ```

# `promptMultichoiceOnce` _map_ _path_ _prompt_ _choices_ [*default*]

`promptMultichoiceOnce` returns the value of _map_ at _path_ if it exists and is
a string, otherwise it prompts the user for one of _choices_ with _prompt_ and
an optional list _default_ using [`promptMultichoice`][pm].

!!! example

    ```
    {{- $choices := list "chocolate" "strawberry" "vanilla" "pistachio" -}}
    {{- $icecream := promptMultichoiceOnce
        . "icecream"
        "What type of ice cream do you like"
        $choices
        (list "pistachio" "chocolate")
    -}}
    [data]
        icecream = {{- $icecream | toToml -}}
    ```

[pm]: /reference/templates/init-functions/promptMultichoice.md

# `promptString` _prompt_ [*default*]

`promptString` prompts the user with _prompt_ and returns the user's response
with all leading and trailing spaces stripped. If _default_ is passed and the
user's response is empty then it returns _default_.

!!! example

    ```
    {{ $email := promptString "email" -}}
    [data]
        email = {{ $email | quote }}
    ```

# `promptStringOnce` _map_ _path_ _prompt_ [*default*]

`promptStringOnce` returns the value of _map_ at _path_ if it exists and is a
string value, otherwise it prompts the user for a string value with _prompt_ and
an optional _default_ using `promptString`.

!!! example

    ```
    {{ $email := promptStringOnce . "email" "What is your email address" }}
    ```

# `stdinIsATTY`

`stdinIsATTY` returns `true` if chezmoi's standard input is a TTY. It is
primarily useful for determining whether `prompt*` functions should be called
or default values be used.

!!! example

    ```
    {{ $email := "" }}
    {{ if stdinIsATTY }}
    {{   $email = promptString "email" }}
    {{ else }}
    {{   $email = "user@example.com" }}
    {{ end }}
    ```

# `writeToStdout` _string_...

`writeToStdout` writes each _string_ to stdout.

!!! example

    ```text
    {{- writeToStdout "Hello, world\n" -}}
    ```

# KeePassXC functions

The `keepassxc*` template functions return structured data retrieved from
a [KeePassXC][keepassxc] database using the KeePassXC CLI (`keepassxc-cli`)

The database is configured by setting `keepassxc.database` in the configuration
file. You will be prompted for the database password the first time
`keepassxc-cli` is run, and the password is cached, in plain text, in memory
until chezmoi terminates.

The command used can be changed by setting the `keepassxc.command` configuration
variable, and extra arguments can be added by setting `keepassxc.args`. The
password prompt can be disabled by setting `keepassxc.prompt` to `false`.

By default, chezmoi will prompt for the KeePassXC password when required and
cache it for the duration of chezmoi's execution. Setting `keepassxc.mode` to
`open` will tell chezmoi to instead open KeePassXC's console with `keepassxc-cli
open` followed by `keepassxc.args`. chezmoi will use this console to request
values from KeePassXC.

When setting `keepassxc.mode` to `builtin`, chezmoi uses a builtin library to
access a keepassxc database, which can be handy if `keepassxc-cli` is not
available. Some KeePassXC features (such as Yubikey-enhanced encryption) may not
be available with builtin support.

[keepassxc]: https://keepassxc.org/

# `keepassxc` _entry_

`keepassxc` returns structured data for _entry_ using `keepassxc-cli`.

The output from `keepassxc-cli` is parsed into key-value pairs and cached so
calling `keepassxc` multiple times with the same _entry_ will only invoke
`keepassxc-cli` once.

!!! example

    ```
    username = {{ (keepassxc "example.com").UserName }}
    password = {{ (keepassxc "example.com").Password }}
    ```

# `keepassxcAttachment` _entry_ _name_

`keepassxcAttachment` returns the attachment with _name_ of _entry_ using
`keepassxc-cli`.

!!! info

    `keepassxcAttachment` requires `keepassxc-cli` version 2.7.0 or later.

!!! example

    ```
    {{- keepassxcAttachment "SSH Config" "config" -}}
    ```

# `keepassxcAttribute` _entry_ _attribute_

`keepassxcAttribute` returns the attribute _attribute_ of _entry_ using
`keepassxc-cli`, with any leading or trailing whitespace removed.

!!! example

    ```
    {{ keepassxcAttribute "SSH Key" "private-key" }}
    ```

# Keeper functions

The `keeper*` functions return data from [Keeper][keeper] [Commander CLI][cli]
(`keeper`).

The command used can by changed by setting the `keeper.command` configuration
variable, and extra arguments can be added by setting `keeper.args`.

[keeper]: https://www.keepersecurity.com/
[cli]: https://docs.keeper.io/secrets-manager/commander-cli

# `keeper` _uid_

`keeper` returns structured data retrieved from [Keeper][keeper] using the
[Commander CLI][cli]. _uid_ is passed to `keeper get --format=json` and the
output is parsed as JSON.

[keeper]: https://www.keepersecurity.com/
[cli]: https://docs.keeper.io/secrets-manager/commander-cli

# `keeperDataFields` _uid_

`keeperDataFields` returns the `.data.fields` elements of `keeper get
--format=json *uid*` indexed by `type`.

## Examples

```text
url = {{ (keeperDataFields "$UID").url }}
login = {{ index (keeperDataFields "$UID").login 0 }}
password = {{ index (keeperDataFields "$UID").password 0 }}
```

# `keeperFindPassword` _query_

`keeperFindPassword` returns the output of `keeper find-password query`. _query_
can be a UID or a path.

# `keyring` _service_ _user_

`keyring` retrieves the value associated with _service_ and _user_ from the
user's keyring.

| OS      | Keyring                     |
| ------- | --------------------------- |
| macOS   | Keychain                    |
| Linux   | GNOME Keyring               |
| Windows | Windows Credentials Manager |
| FreeBSD | GNOME Keyring               |

!!! example

    ```
    [github]
        user = {{ .github.user | quote }}
        token = {{ keyring "github" .github.user | quote }}
    ```

!!! warning

    On FreeBSD, the `keyring` template function is only available if chezmoi
    was compiled with cgo enabled. The official release binaries of chezmoi are
    **not** compiled with cgo enabled, and `keyring` will always return an
    empty string.

# LastPass functions

The `lastpass*` template functions return structured data from
[LastPass][latpass] using the [LastPass CLI][cli] (`lpass`).

[lastpass]: https://lastpass.com/
[cli]: https://lastpass.github.io/lastpass-cli/lpass.1.html

# `lastpass` _id_

`lastpass` returns structured data from [LastPass][lastpass] using the [LastPass
CLI][cli] (`lpass`). _id_ is passed to `lpass show --json $ID` and the output
from `lpass` is parsed as JSON. In addition, the `note` field, if present, is
further parsed as colon-separated key-value pairs. The structured data is an
array so typically the `index` function is used to extract the first item. The
output from `lastpass` is cached so calling `lastpass` multiple times with the
same _id_ will only invoke `lpass` once.

!!! example

    ```
    githubPassword = {{ (index (lastpass "GitHub") 0).password | quote }}
    {{ (index (lastpass "SSH") 0).note.privateKey }}
    ```

[lastpass]: https://lastpass.com/
[cli]: https://lastpass.github.io/lastpass-cli/lpass.1.html

# `lastpassRaw` _id_

`lastpassRaw` returns structured data from [LastPass][lastpass] using the
[LastPass CLI][cli] (`lpass`). It behaves identically to the `lastpass`
function, except that no further parsing is done on the `note` field.

!!! example

    ```
    {{ (index (lastpassRaw "SSH Private Key") 0).note }}
    ```

[lastpass]: https://lastpass.com/
[cli]: https://lastpass.github.io/lastpass-cli/lpass.1.html

# pass functions

The `pass` template functions return passwords stored in [pass][pass] using the
pass CLI (`pass`).

!!! hint

    To use a pass-compatible password manager like [passage][passage], set
    `pass.command` to the name of the binary and use chezmoi's `pass*` template
    functions as if you were using pass.

    ```toml title="~/.config/chezmoi/chezmoi.toml"
    [pass]
        command = "passage"
    ```

[pass]: https://www.passwordstore.org/
[passage]: https://github.com/FiloSottile/passage,

# `pass` _pass-name_

`pass` returns passwords stored in [pass][pass] using the pass CLI (`pass`).
_pass-name_ is passed to `pass show $PASS_NAME` and the first line of the output
of `pass` is returned with the trailing newline stripped. The output from `pass`
is cached so calling `pass` multiple times with the same _pass-name_ will only
invoke `pass` once.

!!! example

    ```
    {{ pass "$PASS_NAME" }}
    ```

[pass]: https://www.passwordstore.org/

# `passFields` _pass-name_

`passFields` returns structured data stored in [pass][pass] using the pass CLI
(`pass`). _pass-name_ is passed to `pass show $PASS_NAME` and the output is
parsed as colon-separated key-value pairs, one per line. The return value is
a map of keys to values.

!!! example

    Given the output from `pass`:

    ```
    GitHub
    login: username
    password: secret
    ```

    the return value will be the map:

    ```json
    {
        "login": "username",
        "password": "secret"
    }
    ```

!!! example

    ```
    {{ (passFields "GitHub").password }}
    ```

[pass]: https://www.passwordstore.org

# `passRaw` _pass-name_

`passRaw` returns passwords stored in [pass][pass] using the pass CLI (`pass`).
_pass-name_ is passed to `pass show $PASS_NAME` and the output is returned. The
output from `pass` is cached so calling `passRaw` multiple times with the same
_pass-name_ will only invoke `pass` once.

[pass]: https://www.passwordstore.org/

# Passhole

chezmoi includes support for [KeePass][keepass] using the [Passhole CLI][cli]
(`ph`).

[keepass]: https://keepass.info/
[cli]: https://github.com/Evidlo/passhole

# passhole _path_ _field_

`passhole` returns the _field_ of _path_ from a [KeePass][keepass] database
using [passhole][passhole]'s `ph` command.

!!! example

    ```
    {{ passhole "example.com" "password" }}
    ```

[keepass]: https://keepass.info/
[passhole]: https://github.com/Evidlo/passhole

# Generic secret functions

The `secret*` template functions return the output of the generic secret command
defined by the `secret.command` configuration variable.

# `secret` [*arg*...]

`secret` returns the output of the generic secret command defined by the
`secret.command` configuration variable with `secret.args` and *arg*s with
leading and trailing whitespace removed. The output is cached so multiple calls
to `secret` with the same *arg*s will only invoke the generic secret command
once.

# `secretJSON` [*arg*...]

`secretJSON` returns structured data from the generic secret command defined by
the `secret.command` configuration variable with `secret.args` and *arg*s. The
output is parsed as JSON. The output is cached so multiple calls to `secret`
with the same _args_ will only invoke the generic secret command once.

# `vault` _key_

`vault` returns structured data from [Vault][vault] using the [Vault CLI][cli]
(`vault`). _key_ is passed to `vault kv get -format=json $KEY` and the output
from `vault` is parsed as JSON. The output from `vault` is cached so calling
`vault` multiple times with the same _key_ will only invoke `vault` once.

!!! example

    ```
    {{ (vault "$KEY").data.data.password }}
    ```

[vault]: https://www.vaultproject.io/
[cli]: https://www.vaultproject.io/docs/commands/
